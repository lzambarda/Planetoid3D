using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Planetoid3D
{
     [Serializable]
    public class LKE : BaseObjectBuilding
    {
         public LKE() { }
        public LKE(Planet Planet, Matrix initial, int Race)
        {
            planet = Planet;
            matrix = initial;
            type = BuildingType.LKE;
            owner = Race;
        }

        protected Asteroid myTarget;

        public override void InSerialization()
        {
            myTarget = null;
            base.InSerialization();
        }

        public override bool Update(float elapsed)
        {
            //Get the nearest asteroid
            NearestAsteroid();

            //There is a valid target
            if (myTarget != null && PlayerManager.GetEnergy(owner)>0)
            {
                //Use energy...
                PlayerManager.ChangeEnergy(owner, - 0.9f * elapsed * 16);
                //...to increase keldanyum
                PlayerManager.ChangeKeldanyum(owner,1+ 2 * elapsed * 2*PlayerManager.players[owner].researchLevels[1]/3f);
                //Extract asteroid keldanyum, so consume it
                myTarget.life -= 0.002f * elapsed*16;
                if (PlanetoidGame.details > 0)
                {
                    //Create burst at asteroid position
                    GameEngine.explosionParticles.SetSize(myTarget.life);
                    GameEngine.explosionParticles.SetColor(Color.Lime);
                    GameEngine.explosionParticles.AddParticle(myTarget.matrix.Translation, Vector3.Zero);
                    GameEngine.explosionParticles.SetColor(Color.White);
                    GameEngine.explosionParticles.SetSize(1);
                }
            }

            return base.Update(elapsed);
        }

        private void NearestAsteroid()
        {
            float dist = 600;
            float temp;
            myTarget = null;
            Ray collisionRay = new Ray(matrix.Translation+matrix.Backward*16, Vector3.Zero);
            BoundingSphere planetSphere = new BoundingSphere(planet.matrix.Translation, planet.radius);
            foreach (Asteroid asteroid in GameEngine.asteroids)
            {
                if (asteroid.life > 0.1f)
                {
                    collisionRay.Direction = Vector3.Normalize(asteroid.matrix.Translation - collisionRay.Position);
                    //do not aim through the planet!
                    if (collisionRay.Intersects(planetSphere) == null)
                    {
                        temp = Vector3.Distance(matrix.Translation, asteroid.matrix.Translation);
                        if (temp < dist)
                        {
                            dist = temp;
                            myTarget = asteroid;
                        }
                    }
                }
            }
        }

        public override void Draw()
        {
            base.Draw();
        }

        public override void SecondDraw()
        {
            if (myTarget!=null)
            {
                if (PlanetoidGame.details > 0)
                {
                    Util.DrawTextureLine(matrix.Translation + matrix.Backward * 16, myTarget.matrix.Translation, myTarget.life, GameEngine.gameCamera, Color.Lime);
                    GameEngine.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    GameEngine.Game.GraphicsDevice.BlendState = BlendState.Opaque;
                }
                else
                {
                    Util.DrawLine(matrix.Translation + matrix.Backward * 16, myTarget.matrix.Translation, Color.Lime, Color.Green, GameEngine.gameCamera);
                }
            }
        }
    }
}
