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
    public class Shot : BaseObjectOwned
    {
        public Shot() { }
        public Shot(Matrix Matrix,Vector3 Speed,int Owner,int damages)
        {
            life = damages;
            matrix = Matrix;
            speed = Speed;
            owner = Owner;
        }

        private BaseObjectOwned building;

        public new void InSerialization()
        {
            building = null;
            planet = null;
        }

        public bool Update(float elapsed)
        {
            bool returner = false;
            if (life > 0)
            {
                //The shot is affected by power consume
                life -= elapsed;
            }
            else
            {
                //Energy finished... die
                return true;
            }
            elapsed *= 6;

            //Move in the direction
            matrix.Translation += speed * elapsed;

            //Shot can damage both building and space threats
            building = null;
            for (int a = 0; a < GameEngine.planets.Count; a++)
            {
                if (Vector3.Distance(matrix.Translation, GameEngine.planets[a].matrix.Translation) < GameEngine.planets[a].radius)
                {
                    returner = true;
                    break;
                }
                for (int b = 0; b < GameEngine.planets[a].buildings.Count; b++)
                {
                    //Same XOR statement used in "NearestStuff"
                    if (PlayerManager.GetFriendship(owner, GameEngine.planets[a].buildings[b].owner) < 0.5f)
                    {
                        if (Vector3.Distance(matrix.Translation, GameEngine.planets[a].buildings[b].matrix.Translation) < 25)
                        {
                            building = GameEngine.planets[a].buildings[b];
                            break;
                        }
                    }
                }
            }
            if (building != null && !returner)
            {
                building.Damage(this, life);
                if ((building as BaseObjectBuilding).flying)
                {
                    if (HUDManager.lastTargetObject != building && building.owner == 0 && GameEngine.gameCamera.target != building.planet)
                    {
                        TextBoard.AddMessage("We are under attack in the space near " + building.planet.name + "!!");
                    }
                }
                else if (building.owner == 0 && GameEngine.gameCamera.target != building.planet)
                {
                    TextBoard.AddMessage("We are under attack on " + building.planet.name + "!!");
                }
                if (building is Hunter)
                {
                    ((Hunter)building).dodgeDirection = Util.RandomPointOnSphere(1);
                    if (owner == 0 && building.life <= 0)
                    {
                        QuestManager.QuestCall(12);
                    }
                }
                returner = true;

            }

            if (!returner)
            {
                //Collision check with asteroids
                foreach (Asteroid asteroid in GameEngine.asteroids)
                {
                    if (Vector3.Distance(matrix.Translation, asteroid.matrix.Translation) < asteroid.life * 15)
                    {
                        if (asteroid.life - life <= 0)
                        {
                            PlayerManager.players[owner].DestroyedAsteroid();
                            asteroid.Explode();
                            asteroid.life = 0;
                            if (owner == 0)
                            {
                                QuestManager.QuestCall(1);
                            }
                        }
                        else
                        {
                            asteroid.Damage(null, life);
                        }
                        return true;
                    }
                }
                if (GameEngine.planetoid != null)
                {
                    if (Vector3.Distance(matrix.Translation, GameEngine.planetoid.matrix.Translation) < 25)
                    {
                        GameEngine.planetoid.Damage(null, life);
                        returner = true;
                    }
                }
            }

            //Burst on explosion
            if (returner)
            {
                AudioManager.Play3D(this, "laser_hit");
                Burst(4, RaceManager.GetColor(PlayerManager.GetRace(owner)), 2);
            }
            return returner;
        }

        /// <summary>
        /// Draw shot with personal color and size
        /// </summary>
        public override void Draw()
        {
            Model model = RenderManager.GetModel(this);
            foreach (ModelMesh mm in model.Meshes)
            {
                foreach (BasicEffect effect in mm.Effects)
                {
                    effect.View = GameEngine.gameCamera.viewMatrix;
                    effect.Projection = GameEngine.gameCamera.projectionMatrix;

                    effect.DiffuseColor = RaceManager.GetColor(PlayerManager.GetRace(owner)).ToVector3();

                    Matrix tmp = matrix * Matrix.CreateScale(life/5f);
                    tmp.Translation = matrix.Translation;

                    effect.World = tmp;
                }
                mm.Draw();
            }
        }
    }
}
