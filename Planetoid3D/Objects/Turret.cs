using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Planetoid3D
{
     [Serializable]
    public class Turret : BaseObjectBuilding
    {
         public Turret() { }
        public Turret(Planet Planet, Matrix initial, int Owner)
        {
            planet = Planet;
            matrix = initial;
            type = BuildingType.Turret;

            reload = 160;
            owner = Owner;
            headAngle = 0;
        }

        private BaseObject myTarget;
        Vector3 direction;
        public int reload;
        public float headAngle;

        private Matrix HeadMatrix()
        {
            Matrix m = matrix;
            m *= Matrix.CreateFromAxisAngle(Vector3.Normalize(m.Right), headAngle);
            m.Translation = matrix.Translation + matrix.Backward * 9;
            return m;
        }

        public override void InSerialization()
        {
            myTarget = null;
            base.InSerialization();
        }

        public override bool Update(float elapsed)
        {
            //Get the nearest asteroid
            NearestAsteroid();
            /*if (Vector3.Distance(matrix.Translation, planet.matrix.Translation) > planet.radius+3)
            {
                matrix = Matrix.Invert(Matrix.CreateLookAt(matrix.Translation, planet.matrix.Translation, Vector3.Up));
                matrix.Translation = planet.matrix.Translation + matrix.Backward * planet.radius;
            }*/
            //If I have a target
            if (myTarget != null)
            {
                //Since the turret shots have speed equal to 80
                //Calculate the distance between the head and the asteroid
                //Then divide it by 80 and obtain the portion of space covered in one step
                float amount = Vector3.Distance(matrix.Translation+matrix.Backward*9, myTarget.matrix.Translation) / 80f;

                //Now get the direction from the head position to the asteroid position
                //Add the asteroid speed and the value found before to the asteroid position
                //THIS DIRECTION STARTS FROM THE HEAD POSITION AND GOES TO THE ASTEROID FUTURE POSITION
                direction =myTarget.matrix.Translation + myTarget.speed * amount - (matrix.Translation+matrix.Backward*9);
                if (direction.Length() > 0)
                {
                    direction.Normalize();
                }

                //Rotate body and head to face direction      
                float dot = Vector3.Dot(matrix.Right, direction);
                matrix *= Matrix.CreateFromAxisAngle(Vector3.Normalize(matrix.Backward), dot/5f);
                matrix.Translation = planet.matrix.Translation + matrix.Backward * planet.radius;

                //Now rotate head to aim at the target
                Matrix head = HeadMatrix();
                dot = Vector3.Dot(head.Down, direction);
                headAngle += dot / 5f;

                //If I can shot
                if (reload <= 0)
                {
                    //If the error is small enough
                    if (Math.Abs(dot) <= 0.1f)
                    {
                        //Shoot
                        reload = 160;
                        GameEngine.shots.Add(new Shot(head, head.Backward * 80, owner, 5));
                        AudioManager.Play3D(this, "laser_huge");              
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
        /// Get the biggest and the nearest asteroid in sight (not hidden by my planet)
        /// </summary>
        private void NearestAsteroid()
        {
            float dist = 600;
            float temp;
            float size = 0.1f;
            myTarget = null;
            Ray collisionRay = new Ray(matrix.Translation + matrix.Backward * 9, Vector3.Zero);
            BoundingSphere planetSphere = new BoundingSphere(planet.matrix.Translation, planet.radius);
            if (GameEngine.planetoid != null)
            {
                collisionRay.Direction = Vector3.Normalize(GameEngine.planetoid.matrix.Translation - collisionRay.Position);
                if (collisionRay.Intersects(planetSphere) == null)
                {
                    if (Vector3.Distance(matrix.Translation, GameEngine.planetoid.matrix.Translation) < dist)
                    {
                        myTarget = GameEngine.planetoid;
                    }
                }
            }

            //if (GameEngine.ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.M))
            {
                DataSectorSubdivider.FindObjectsInSectors<Asteroid>(matrix.Translation, 600);
                for (int a = 0; a < DataSectorSubdivider.sectorsContainedObjects.Count; a++)
                {
                    //should be the biggest (the most dangerous) asteroid in sight
                    if (DataSectorSubdivider.sectorsContainedObjects[a].life > size)
                    {
                        collisionRay.Direction = Vector3.Normalize(DataSectorSubdivider.sectorsContainedObjects[a].matrix.Translation + DataSectorSubdivider.sectorsContainedObjects[a].speed * 5 - collisionRay.Position);
                        //do not aim through the planet!
                        if (collisionRay.Intersects(planetSphere) == null)
                        {
                            temp = Vector3.Distance(matrix.Translation, DataSectorSubdivider.sectorsContainedObjects[a].matrix.Translation);
                            if (temp < dist)
                            {
                                dist = temp;
                                myTarget = DataSectorSubdivider.sectorsContainedObjects[a];
                                size = DataSectorSubdivider.sectorsContainedObjects[a].life;
                            }
                        }
                    }
                }
            }
            /*else
            {
                for (int a = 0; a < GameEngine.asteroids.Count;a++ )
                {
                    //should be the biggest (the most dangerous) asteroid in sight
                    if (GameEngine.asteroids[a].life > size)
                    {
                        collisionRay.Direction = Vector3.Normalize(GameEngine.asteroids[a].matrix.Translation + GameEngine.asteroids[a].speed * 5 - collisionRay.Position);
                        //do not aim through the planet!
                        if (collisionRay.Intersects(planetSphere) == null)
                        {
                            temp = Vector3.Distance(matrix.Translation, GameEngine.asteroids[a].matrix.Translation);
                            if (temp < dist)
                            {
                                dist = temp;
                                myTarget = GameEngine.asteroids[a];
                                size = GameEngine.asteroids[a].life;
                            }
                        }
                    }
                }
            }*/
        }

        public override void Draw()
        {
            Util.DrawLine(matrix.Translation, planet.matrix.Translation, Color.Red, Color.Lime, GameEngine.gameCamera);
            Model model = RenderManager.GetModel(this);
            model.Bones[1].Transform = matrix;
            model.Bones[2].Transform = HeadMatrix();
            base.DrawUsingBones();
        }
    }
}
