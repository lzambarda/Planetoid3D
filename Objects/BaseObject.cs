using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
//using Microsoft.Xna.Framework.Net;
//using Microsoft.Xna.Framework.Storage;

namespace Planetoid3D
{
    [Serializable]
    public class BaseObject
    {
        public Matrix matrix;
        public float life;
        public Vector3 speed;
        public Planet planet;
        public int index_planet;

        /// <summary>
        /// Method to use before the serialization of the object
        /// </summary>
        public virtual void InSerialization()
        {
            if (planet != null)
            {
                index_planet = GameEngine.planets.IndexOf(planet);
                planet = null;
            }
        }

        /// <summary>
        /// Method to use after the serialization of the object, should do the reverse instructions executed with InSerialization()
        /// </summary>
        public virtual void OutSerialization()
        {
            if (index_planet > -1)
            {
                planet = GameEngine.planets[index_planet];
                index_planet = -1;
            }
        }

        /// <summary>
        /// Calculate the gravity and returns if the object is affected by it
        /// </summary>
        /// <returns></returns>
        public bool BlackHoleGravity()
        {
            if (matrix.Translation.Length() > 7000)
            {
                life = 0;
            }
            if (GameEngine.blackHole != null)
            {
                float dist = Vector3.Distance(matrix.Translation, GameEngine.blackHole.matrix.Translation);
                if (dist < 1000)
                {
                    if (dist < 100)
                    {
                        GameEngine.blackHole.life += 2;
                        AudioManager.Play3D(this, "blackhole_suck");
                        return true;
                    }
                    speed += Vector3.Normalize(GameEngine.blackHole.matrix.Translation - matrix.Translation);
                    Vector3 temp = matrix.Translation;
                    matrix *= Matrix.CreateRotationX(MathHelper.ToRadians(speed.X + 0.1f));
                    matrix *= Matrix.CreateRotationY(MathHelper.ToRadians(speed.Y + 0.1f));
                    matrix *= Matrix.CreateRotationZ(MathHelper.ToRadians(speed.Z + 0.1f));
                    matrix.Translation = temp;
                }
                else if (this is BaseObjectBuilding && this is Catapult==false)
                {
                    speed *= 0.98f;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the nearest planet within a given range
        /// </summary>
        public void NearestPlanet(bool sun,ref Planet nearest)
        {
            float minDist = float.MaxValue;
            float temp;
            nearest = null;
            for (int a = (sun?0:1); a < GameEngine.planets.Count; a++)
            {
                if (GameEngine.planets[a].life > 0 && GameEngine.planets[a] != this)
                {
                    temp = Vector3.Distance(matrix.Translation, GameEngine.planets[a].matrix.Translation) - GameEngine.planets[a].radius;
                    if (temp < minDist || nearest == null)
                    {
                        minDist = temp;
                        nearest = GameEngine.planets[a];
                    }
                }
            }
        }

        /// <summary>
        /// Get the first colliding object on a planet withing a given radius
        /// </summary>
        public void GetCollidingObject(float radius,ref BaseObject collider)
        {
            collider = planet.hominids.Find(h => Vector3.Distance(matrix.Translation, h.matrix.Translation) < radius);
            if (collider != null)
            {
                return;
            }
            collider = planet.buildings.Find(b => b != this && Vector3.Distance(matrix.Translation, b.matrix.Translation) < radius * 2);
            if (collider != null)
            {
                return;
            }
            collider = planet.trees.Find(t => t.gaseous==false && Vector3.Distance(matrix.Translation, t.matrix.Translation) < radius);
            if (collider != null)
            {
                return;
            }
        }

        /// <summary>
        /// Rotate the BaseObject matrix to face a given direction
        /// </summary>
        public void RotateToFace(Vector3 wantedDirection,float turnSpeed)
        {
            wantedDirection.Normalize();
            Vector3 shipFront = matrix.Forward;
            Vector3 shipRight;
            Vector3 shipUp = matrix.Up;

            float dot = Vector3.Dot(shipFront, wantedDirection);

            if (dot > -0.99)
            {
                shipFront = Vector3.Lerp(shipFront, wantedDirection, turnSpeed);
            }
            else
            {
                // Special case for if we are turning exactly 180 degrees. 
                RotateToFace(matrix.Right, turnSpeed);
                return;
            }
            shipRight = Vector3.Cross(shipFront, shipUp);
            shipUp = Vector3.Cross(shipRight, shipFront);

            shipFront.Normalize();
            shipRight.Normalize();
            shipUp.Normalize();

            matrix.Forward = shipFront;
            matrix.Right = shipRight;
            matrix.Up = shipUp;
        }

        /// <summary>
        /// Rotate the matrix of this object along the selected axis and with the given amount, translation is conserved
        /// </summary>
        public void RotateByAxis(Vector3 axis, float amount)
        {
            Vector3 pos = matrix.Translation;
            matrix.Translation = Vector3.Zero;
            matrix *= Matrix.CreateFromAxisAngle(axis, amount);
            matrix.Translation = pos;
        }

        /// <summary>
        /// Decrease BaseObject's life
        /// </summary>
        public virtual void Damage(BaseObjectOwned damager, float damages)
        {
            life -= damages;
        }

        /// <summary>
        /// If the object is in the camera view, produce a flash
        /// </summary>
        /// <param name="amount"></param>
        public void Flash(float amount)
        {
            if (this.IsVisible())
            {
                PlanetoidGame.flash += amount;
            }
        }

        public bool IsVisible()
        {
            return Vector3.Dot(Vector3.Normalize(matrix.Translation - GameEngine.gameCamera.position), Vector3.Normalize(GameEngine.gameCamera.tempTar - GameEngine.gameCamera.position)) > 0.2f;
        }

        public virtual string GetHudText()
        {
            return "\nLife: " + (int)life;
        }

        /// <summary>
        /// Create "amount" flames at BaseObject position
        /// </summary>
        public void Burst(int amount, Color color,float radius)
        {
            GameEngine.explosionParticles.SetColor(color);
            amount *= 2;
            while (amount-- > 0)
            {
                GameEngine.explosionParticles.AddParticle(matrix.Translation + Util.RandomPointOnSphere(radius), Vector3.Zero);
            }
            GameEngine.explosionParticles.SetColor(Color.White);
        }

        public virtual void Draw()
        {
            Model model = RenderManager.GetModel(this);
            foreach (ModelMesh mm in model.Meshes)
            {
                foreach (BasicEffect effect in mm.Effects)
                {
                    effect.DiffuseColor = Vector3.One;
                    effect.World = matrix;
                    effect.View = GameEngine.gameCamera.viewMatrix;
                    effect.Projection = GameEngine.gameCamera.projectionMatrix;
                }
                mm.Draw();
            }
        }
    }
}
