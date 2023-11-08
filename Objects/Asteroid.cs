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
    public class Asteroid : BaseObject
    {
        public Asteroid() { }

        public Asteroid(bool sunOrbiting)
        {
            life = 0.5f + (float)(Util.random.NextDouble() * 0.75f);

            if (sunOrbiting)
            {
                matrix = Matrix.CreateTranslation(Util.random.Next(50) + 450, 20 - Util.random.Next(40), 0);
                matrix *= Matrix.CreateFromAxisAngle(Vector3.Up, (float)Util.random.NextDouble() * MathHelper.TwoPi);
                speed = Vector3.Normalize(-matrix.Translation);
                speed = Vector3.Cross(Vector3.Up, speed)*10 + new Vector3(0, 2 - (float)Util.random.NextDouble() * 4, 0);
            }
            else
            {
                matrix = Matrix.CreateTranslation(Util.RandomPointOnSphere(5500));
                speed = -Vector3.Normalize(matrix.Translation + Util.RandomPointOnSphere(100));
                speed *= (2 + Util.random.Next(5));
            }

            //Random rotation
            for (int a = Util.random.Next(1000); a > 0; a--)
            {
                Update(0.016f);
            }

            SetUpMatrixScaling();
        }

        public Asteroid(Vector3 position,Vector3 Speed)
        {
            life = 0.25f + (float)(Util.random.NextDouble() * 0.5f);
            matrix = Matrix.CreateTranslation(position);
            speed = Speed;
            SetUpMatrixScaling();
        }

        private void SetUpMatrixScaling()
        {
            matrix.Up *= ((float)(Util.random.NextDouble() * life) / 2f + 0.5f);
            matrix.Right *= ((float)(Util.random.NextDouble() * life) / 2f + 0.5f);
            matrix.Backward *= ((float)(Util.random.NextDouble() * life) / 2f + 0.5f);
        }

        /// <summary>
        /// Is this asteroid carrying a troglother?
        /// </summary>
        public bool trogloted;
        private BaseObject collider;

        private void LeaveTroglother()
        {
            //If this asteroid is carrying a troglother, spawn it
            if (trogloted && planet.hominids.Find(h => h is Troglother) == null)
            {
                if (GameEngine.gameMode == GameMode.Tutorial)
                {
                    return;
                }
                planet.hominids.Add(new Troglother(planet, null));
                planet.hominids.Last().matrix = Matrix.Invert(Matrix.CreateLookAt(matrix.Translation, planet.matrix.Translation, Vector3.Up));
                planet.hominids.Last().matrix.Translation = planet.matrix.Translation + planet.hominids.Last().matrix.Backward * planet.radius;

                if (planet.atmosphere != Atmosphere.Ammonia)
                {
                    if (planet.atmosphere_level < 50)
                    {
                        planet.atmosphere = Atmosphere.Ammonia;
                    }
                    else
                    {
                        planet.atmosphere_level -= 50;
                    }
                }
            }
        }

        public bool Update(float elapsed)
        {
            //If the asteroid is too small slowly destroy it
            if (life <= 0.1f)
            {
                life -= 0.001f;
                if (life <= 0)
                {
                    return true;
                }
            }

            elapsed *= 6;
            //Update matrix translation
            matrix.Translation += speed * elapsed;
            if (matrix.Translation.Length() > 6500)
            {
                return true;
            }

            if (trogloted)
            {
                Burst(1, Color.Purple, life * 10);
            }

            //Get nearest planet
            NearestPlanet(true, ref planet);
            if (planet != null)
            {
                float distance = Vector3.Distance(planet.matrix.Translation, matrix.Translation);
                //Enter in the planet gravity field
                if (distance < planet.radius * 20)
                {
                    //Apply gravity
                    speed += Vector3.Normalize(planet.matrix.Translation - matrix.Translation) / 2 * elapsed;
                    //Apply atmosphere friction
                    if (planet.atmosphere != Atmosphere.None && planet is Sun == false)
                    {
                        if (distance < planet.radius * planet.atmosphere_level / 30f)
                        {
                            life -= speed.Length() * elapsed * elapsed * elapsed * (planet.atmosphere_level / 400f);
                            GameEngine.fireParticles.SetSize(life);
                            GameEngine.fireParticles.AddParticle(matrix.Translation + speed / 2, Vector3.Zero);
                            GameEngine.fireParticles.SetSize(1);
                        }
                    }
                    if (distance < planet.radius * 2)
                    {
                        //If it collides with planet
                        if (distance < planet.radius + life * 5)
                        {
                            if (planet is Sun)
                            {
                                //Generate flames of the color of the sun
                                GameEngine.fireParticles.SetColor(planet.color);
                                for (int b = 0; b < 20; b++)
                                {
                                    GameEngine.fireParticles.AddParticle(
                                        matrix.Translation + Util.RandomPointOnSphere(5),
                                        -speed / 2
                                        );
                                }
                                GameEngine.fireParticles.SetColor(Color.White);
                            }
                            else if (GameEngine.gameMode != GameMode.Tutorial)
                            {
                                //Hit the planet
                                planet.Damage(null, life);
                                planet.available_keldanyum += (life * 250);
                                LeaveTroglother();
                            }

                            //Explode and destroy
                            Explode();
                            return true;
                        }
                        else if (planet is Sun == false)
                        {
                            //Search for a collision with an object
                            GetCollidingObject(life * 15f, ref collider);
                            if (collider != null)
                            {
                                collider.Damage(null, life * 50);
                                Explode();
                                LeaveTroglother();
                                return true;
                            }
                        }
                    }
                }
            }

            //If it collides with the Planetoid, destroy it
            if (GameEngine.planetoid != null)
            {
                if (Vector3.Distance(matrix.Translation, GameEngine.planetoid.matrix.Translation) < 30)
                {
                    Explode();
                    return true;
                }
            }

            //Adjust asteroid's spin
            Vector3 position = matrix.Translation;
            matrix.Translation = Vector3.Zero;
            elapsed /= 20f;
            matrix *= Matrix.CreateFromAxisAngle(Vector3.Right, speed.X * elapsed);
            matrix *= Matrix.CreateFromAxisAngle(Vector3.Up, -speed.Y * elapsed);
            matrix *= Matrix.CreateFromAxisAngle(Vector3.Backward, speed.Z * elapsed);
            matrix.Translation = position;;
            return BlackHoleGravity();
        }

        public void Explode()
        {
            if (life <= 0.1f)
            {
                return;
            }
            AudioManager.Play3D(this, "asteroid_death");
            Vector3 speed;
            for (int a = 0; a < life * 20; a++)
            {
                speed = Util.RandomPointOnSphere(life * 4);
                GameEngine.fireParticles.AddParticle(matrix.Translation + speed, speed);
                GameEngine.asteroids.Add(new Asteroid(matrix.Translation, speed * 2));
                GameEngine.asteroids.Last().life = 0.1f;
            }
        }
    }
}
