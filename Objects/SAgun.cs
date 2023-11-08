using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Planetoid3D
{
    [Serializable]
    public class SAgun : BaseObjectBuilding
    {
         public SAgun() { }
         public SAgun(Planet Planet, Matrix initial, int Owner)
        {
            planet = Planet;
            matrix = initial;
            type = BuildingType.SAgun;

            reload = 80;
            owner = Owner;
            headAngle = 0;
        }

        private BaseObjectBuilding myTarget;
        private Vector3 direction;
        public int reload;
        public float headAngle;

        public override void InSerialization()
        {
            myTarget = null;
            base.InSerialization();
        }

        private Matrix HeadMatrix()
        {
            Matrix m = matrix;
            //m *= Matrix.CreateFromAxisAngle(matrix.Backward, baseAngle);
            m *= Matrix.CreateFromAxisAngle(m.Right, headAngle);
            m.Translation = matrix.Translation + matrix.Backward * 9;
            return m;
        }

        public override bool Update(float elapsed)
        {
            //Get the nearest asteroid
            FirstEnemyShip();

            //If I have a target
            if (myTarget != null)
            {
                CheckForBattleMusic(myTarget);
                //Since the turret shots have speed equal to 80
                //Calculate the distance between the head and the asteroid
                //Then divide it by 80 and obtain the portion of space covered in one step
                float amount = Vector3.Distance(matrix.Translation + matrix.Backward * 9, myTarget.matrix.Translation) / 80f;

                //Now get the direction from the head position to the asteroid position
                //Add the asteroid speed and the value found before to the asteroid position
                //THIS DIRECTION STARTS FROM THE HEAD POSITION AND GOES TO THE ASTEROID FUTURE POSITION
                direction = myTarget.matrix.Translation + 10*(myTarget is Hunter?myTarget.matrix.Down:myTarget.matrix.Backward) * amount - (matrix.Translation + matrix.Backward * 9);
                if (direction.Length() > 0)
                {
                    direction.Normalize();
                }

                //Rotate body and head to face direction
                float dot = Vector3.Dot(matrix.Right, direction);
                matrix *= Matrix.CreateFromAxisAngle(Vector3.Normalize(matrix.Backward), dot / 5f);
                matrix.Translation = planet.matrix.Translation + planet.radius * matrix.Backward;

                //headMatrix *= Matrix.CreateFromAxisAngle(headMatrix.Backward, dot / 5);

                //Now rotate head to aim at the target
                Matrix head = HeadMatrix();
                dot = Vector3.Dot(head.Down, direction);
                headAngle += dot / 5;

                //If I can shot
                if (reload <= 0)
                {
                    //If the error is small enough
                    if (Math.Abs(dot) <= 0.1f)
                    {
                        //Shot
                        reload = 240;
                        GameEngine.shots.Add(new Shot(head, head.Backward * 80 + head.Right / 2, owner, 3 + PlayerManager.players[owner].researchLevels[1] / 2));
                        GameEngine.shots.Add(new Shot(head, head.Backward * 80 + head.Left / 2, owner, 3 + PlayerManager.players[owner].researchLevels[1] / 2));
                        AudioManager.Play3D(this, "laser_medium");
                    }
                }
                else
                {
                    //Wait and reload
                    reload -= (int)(elapsed * 100);
                }
            }
            return base.Update(elapsed);
        }

        /// <summary>
        /// Get the nearest enemy ship within a given range
        /// </summary>
        private void FirstEnemyShip()
        {
            myTarget = null;
            Ray collisionRay = new Ray(matrix.Translation + matrix.Backward * 9, Vector3.Zero);
            BoundingSphere planetSphere = new BoundingSphere(planet.matrix.Translation, planet.radius);
            Vector3 dist;
            //if (GameEngine.ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.M))
            /*{
                DataSectorSubdivider.FindObjectsInSectors<BaseObjectBuilding>(matrix.Translation, 600);
                for (int a = 0; a < DataSectorSubdivider.sectorsContainedObjects.Count; a++)
                {
                    if ((DataSectorSubdivider.sectorsContainedObjects[a] as BaseObjectBuilding).flying)
                    {
                        if (PlayerManager.GetFriendship(owner, (DataSectorSubdivider.sectorsContainedObjects[a] as BaseObjectBuilding).owner) < 0.5f)
                        {
                            if (Vector3.Distance(matrix.Translation, DataSectorSubdivider.sectorsContainedObjects[a].matrix.Translation) < 400)
                            {
                                collisionRay.Direction = Vector3.Normalize(DataSectorSubdivider.sectorsContainedObjects[a].matrix.Translation - collisionRay.Position);
                                //do not aim through the planet!
                                if (collisionRay.Intersects(planetSphere) == null)
                                {
                                    myTarget = (BaseObjectBuilding)DataSectorSubdivider.sectorsContainedObjects[a];
                                }
                            }
                        }
                    }
                }
            }
            else*/
            {
                for (int b = 0; b < GameEngine.planets.Count; b++)
                {
                    for (int a = 0; a < GameEngine.planets[b].buildings.Count(); a++)
                    {
                        if (GameEngine.planets[b].buildings[a].flying)
                        {
                            if (PlayerManager.GetFriendship(owner, GameEngine.planets[b].buildings[a].owner) < 0.5f)
                            {
                                if (Vector3.Distance(matrix.Translation, GameEngine.planets[b].buildings[a].matrix.Translation) < 400)
                                {
                                    collisionRay.Direction = Vector3.Normalize(GameEngine.planets[b].buildings[a].matrix.Translation - collisionRay.Position);
                                    //do not aim through the planet!
                                    if (collisionRay.Intersects(planetSphere) == null)
                                    {
                                        myTarget = GameEngine.planets[b].buildings[a];
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void Draw()
        {
            Model model = RenderManager.GetModel(this);
            model.Bones[1].Transform = matrix;
            model.Bones[2].Transform = HeadMatrix();
            base.DrawUsingBones();
        }
    }
}
