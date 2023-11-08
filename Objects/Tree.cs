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
    public class Tree : BaseObject
    {
        public Tree() { }

        public Tree(Planet myPlanet)
        {
            life = 100;
            planet = myPlanet;
            myAtmosphere = planet.atmosphere;

            //Initialize position on planet and tree scale
            if (myAtmosphere > Atmosphere.Cyanide)
            {
                gaseous=true;
                maxScale = 1;
            }
            else
            {
                gaseous=false;
                maxScale = myPlanet.radius / 10f - (float)Util.random.NextDouble() * 2;
            }
            matrix = Matrix.Invert(Matrix.CreateLookAt(Util.RandomPointOnSphere(planet.radius), Vector3.Zero, Vector3.Up));
            scale = maxScale / 75f;
            matrix *= Matrix.CreateFromAxisAngle(matrix.Backward, (float)Util.random.NextDouble() * MathHelper.TwoPi);
            matrix.Translation = planet.matrix.Translation + matrix.Backward * (planet.radius-0.5f);
        }

        public Tree(Planet myPlanet,Matrix initial)
        {
            life = 100;
            planet = myPlanet;
            myAtmosphere = planet.atmosphere;

            //Initialze position on planet and tree scale
            if (myAtmosphere > Atmosphere.Cyanide)
            {
                gaseous = true;
                maxScale = 0.7f;
            }
            else
            {
                gaseous = false;
                maxScale = myPlanet.radius / 10f - (float)Util.random.NextDouble() * 2;
            }
            scale = maxScale / 75f;
            matrix = initial;

            timer = Util.random.Next(15);
            
        }

        public float maxScale;
        public float scale;
        public new Planet planet;
        public new int index_planet;
        public bool gaseous;
        public Atmosphere myAtmosphere;
        public float timer;

        public new void InSerialization()
        {
            index_planet = GameEngine.planets.IndexOf(planet);
            planet = null;
        }

        public new void OutSerialization()
        {
            if (index_planet > -1)
            {
                planet = GameEngine.planets[index_planet];
                index_planet = -1;
            }
        }

        public void GrowMax()
        {
            scale = maxScale;
        }

        public bool Update(float elapsed)
        {
            if (timer <= 0)
            {
                    timer = 8;
                    if (planet.atmosphere == myAtmosphere)
                    {
                        planet.atmosphere_level += (1 + (int)scale);
                    }
                    else
                    {
                        planet.atmosphere_level -= (1 + (int)scale);
                    }
            }

            timer -= elapsed;

            if (gaseous)
            {
                if ((int)(timer * 1000) % 5 == 0)
                {
                    GameEngine.explosionParticles.AddParticle(planet.matrix.Translation + Util.RandomPointOnSphere(planet.radius + 2), planet.matrix.Translation - planet.oldPosition);
                }
            }

            //Grow
            if (planet.atmosphere == myAtmosphere)
            {
                if (scale < maxScale)
                {
                    scale += 0.0002f;
                }
            }
            else
            {
                scale -= 0.0005f;
                if (scale <= 0)
                {
                    return true;
                }
            }
            //If the tree is not gaseous
            //Update position and follow planet
            matrix.Translation -= planet.oldPosition;
            matrix *= Matrix.CreateFromAxisAngle(planet.axis, planet.spinSpeed * elapsed);
            matrix.Translation += planet.matrix.Translation;

            return (life <= 0);
        }

        public override void Draw()
        {
            Matrix tmp = matrix;
            matrix.Translation = Vector3.Zero;
            matrix *= Matrix.CreateScale(scale);
            matrix.Translation = tmp.Translation;

            Model model = RenderManager.GetModel(this);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = RenderManager.buildingShader;
                    part.Effect.CurrentTechnique = part.Effect.Techniques["Building"];

                    part.Effect.CurrentTechnique.Passes[0].Apply();

                    part.Effect.Parameters["World"].SetValue(matrix);
                    part.Effect.Parameters["View"].SetValue(GameEngine.gameCamera.viewMatrix);
                    part.Effect.Parameters["Projection"].SetValue(GameEngine.gameCamera.projectionMatrix);
                    part.Effect.Parameters["LightDir"].SetValue(Vector3.Normalize(matrix.Translation));
                    part.Effect.Parameters["Texture"].SetValue(RenderManager.treeTextures[(int)myAtmosphere]);
                    part.Effect.Parameters["Color"].SetValue(Vector4.Zero);
                }
                mesh.Draw();
            }

            matrix = tmp;
        }
    }
}