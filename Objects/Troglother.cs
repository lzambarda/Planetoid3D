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
    public class Troglother : Hominid
    {
        public Troglother() { }

        public Troglother(Planet myPlanet, Nullable<Matrix> initial)
        {
            //Natives
            owner = 8;

            planet = myPlanet;

            if (initial == null)
            {
                GetRandomPosition(ref matrix);
                matrix.Translation += planet.matrix.Translation;
            }
            else
            {
                matrix = (Matrix)initial;
            }

            GetRandomPosition(ref goal);

            life = 500;

            timer = 0;

            speed.X = 6;

            rotation = 0;

            model = RenderManager.GetModel(this);
            headMatrix = matrix;

            SmokeMark();
        }

        public Matrix headMatrix;
        Model model;
        Vector3 headAngles;
        public Vector2[] legAngles = new Vector2[2];
        int legSwitch;
        bool stepSwitch = true;
        float scream;

        public override void OutSerialization()
        {
            base.OutSerialization();
            model = RenderManager.GetModel(this);
        }

        public override string GetHudText()
        {
            return "A savage Troglother\nLife :" +(int)life;
        }

        public override bool Update(float elapsed)
        {
            //Avoid "troglother losing head" event
            /*if (float.IsNaN(headMatrix.M11))
            {
                headMatrix = matrix;
            }*/
            /*if (float.IsNaN(matrix.M11))
            {
                matrix = Matrix.Invert(Matrix.CreateLookAt(matrix.Translation, planet.matrix.Translation, Vector3.Up));
            }*/

            //Decrease breath timer
            timer -= elapsed;
            if (timer <= 0)
            {
                timer = 10;
                AudioManager.Play3D(this, "troglother_hiss");
                if (planet.hominids.Count < planet.maxPopulation + 2)
                {
                    planet.hominids.Add(new Hominid(planet, 8, matrix));
                }
                if (RaceManager.Tolerate(Race, planet.atmosphere))
                {
                    if (planet.atmosphere_level < 1)
                    {
                        life -= 2;
                    }
                    else
                    {
                        planet.atmosphere_level -= 1;
                        if (life < 300)
                        {
                            life++;
                        }
                    }
                }
                else
                {
                    life -= 2;
                }
            }

            BaseObjectOwned collider = null;
            //Get the nearest enemy stuff on the planet
            target = NearestStuff(/*, true*/);

            //If it exists, set the goal to the stuff's position
            if (target != null)
            {
                CheckForBattleMusic(target);
                SetGoal(target.matrix.Translation, planet.matrix.Translation);
                LookAt(target.matrix.Translation);
                scream = 20;
            }
            else if (Vector3.Distance(matrix.Translation, GameEngine.gameCamera.position) < 180)
            {
                //Look at the player and scream
                if (LookAt(GameEngine.gameCamera.position))
                {
                    goal = matrix;
                    goal.Translation -= planet.matrix.Translation;
                    scream -= 0.1f;
                    if (scream > 0.1f && scream < 0.2f)
                    {
                        AudioManager.Play3D(this, "troglother_scream");
                    }
                }
            }
            else
            {
                LookAt(tempGoal.Translation+planet.matrix.Translation);
            }


            //Directly go to goal
            tempGoal = goal;
            //Return to the old target
            collider = target;

            //Take matrix.Translation to relative
            matrix.Translation -= planet.oldPosition;
            //Rotate with planet
            matrix *= Matrix.CreateFromAxisAngle(planet.axis, planet.spinSpeed * elapsed);
            //Save realPosition
            realPosition = matrix.Translation;

            //Take a temp...
            //Matrix temp = matrix;
            //Rotate facing direction
            float dot = Vector3.Dot(matrix.Right, tempGoal.Backward) / 10;
            if (Math.Abs(dot) < 0.2f)
            {
                rotation += dot;
                matrix *= Matrix.CreateFromAxisAngle(matrix.Backward, dot);
                //If there is enough distance from the actual position to the goal position
                if (Vector3.Distance(realPosition, goal.Translation) > 10)
                {
                    //Update goal position following planet's rotation
                    UpdateGoal(elapsed);
                  
                    //Move to goalPosition
                    matrix *= Matrix.CreateFromAxisAngle(crossVector, UpdateAnimation());// 0.0025f);
                }
                else if (tempGoal == goal)
                {
                    Vector3 dir = Vector3.Normalize(matrix.Translation);

                    matrix = Matrix.Invert(Matrix.CreateLookAt(matrix.Translation, Vector3.Zero, Vector3.Up));
                    matrix *= Matrix.CreateFromAxisAngle(matrix.Backward, rotation);
                    matrix.Translation = dir * planet.radius;

                    //headMatrix = matrix;
                    //Get a new goal
                    GetRandomPosition(ref goal);
                }
            }
            //Take matrix.Translation to absolute
            matrix.Translation += planet.matrix.Translation;

            //Collision with everything, check events
            if (collider != null && Vector3.Distance(matrix.Translation, collider.matrix.Translation) < 13)
            {
                if (target != collider && Vector3.Distance(matrix.Translation, collider.matrix.Translation) < 5)
                {
                    GetRandomPosition(ref goal);
                }
                if (collider is BaseObjectBuilding)
                {
                    //DAMAGE BUILDING
                    GameEngine.tsmokeParticles.AddParticle(collider.matrix.Translation + Util.RandomPointOnSphere(5), Vector3.Zero);
                    if ((int)(timer * 10) % 7 == 0)
                    {
                        collider.Damage(this, 0.5f);

                        if (Util.random.Next(100) == 0)
                        {
                            AudioManager.Play3D(this, "impact_huge");
                            collider.Burst(5, Color.Orange, 5);
                            flying = true;
                        }

                        else if ((int)collider.life % 8 == 0)
                        {
                            AudioManager.Play3D(this, "impact_soft");
                        }
                    }
                    //Stay on your place
                    if (collider.life > 0)
                    {
                        goal = collider.matrix;
                        goal.Translation -= planet.matrix.Translation;
                    }
                }
                //Will attack every kind of hominid which is not a trogloid
                else if (collider is Hominid && ((Hominid)collider).owner != owner)
                {
                    //FIGHT
                    GameEngine.tsmokeParticles.AddParticle(collider.matrix.Translation + Util.RandomPointOnSphere(5), Vector3.Zero);
                    //Deal damages to the opponent
                    if ((int)(timer * 10) % 7 == 0)
                    {
                        //Soldier specialization deals more damages
                        collider.Damage(this, 4);
                        //There is the chance to kick the opponent
                        if (Util.random.Next(30) == 0)
                        {
                            AudioManager.Play3D(this, "punch");
                            AudioManager.Play3D(this, "troglother_scream");
                            ((Hominid)collider).flying = true;
                            ((Hominid)collider).speed = (collider.matrix.Backward / 2f + Vector3.Normalize(collider.matrix.Translation - matrix.Translation) / 10f);
                        }
                    }
                    //Stay on your place
                    if (collider.life > 0)
                    {
                        goal = collider.matrix;
                        goal.Translation -= planet.matrix.Translation;
                    }
                }
                //JUST GO AWAY
                else
                {
                    GetRandomPosition(ref goal);
                }
            }

            return (life <= 0);
        }

        protected float UpdateAnimation()
        {
            speed.X = 6;
            //legs
            //walking cycle
            if (stepSwitch)
            {
               if (legAngles[legSwitch].X < MathHelper.PiOver2)
                {
                    //Take forward the leg
                    legAngles[legSwitch].X += 0.01f * speed.X;
                    legAngles[legSwitch].Y += 0.01f * speed.X;

                    //Send behing the other leg
                    legAngles[1 - legSwitch].X -= 0.004f * speed.X;
                }
                else
                {
                    //Fix values
                    legAngles[legSwitch].X = MathHelper.PiOver2;
                    legAngles[legSwitch].Y = MathHelper.PiOver2;
                    legAngles[1 - legSwitch].X = -0.628f;

                    //Go to next step
                    stepSwitch = false;
                }
            }
            else
            {
                if (legAngles[legSwitch].Y > 0.004f)
                {
                    //Recoil the forward leg
                    legAngles[legSwitch].X -= 0.01f * speed.X;
                    legAngles[legSwitch].Y -= 0.01f * speed.X;

                    //Start recoiling the backward leg
                    legAngles[1 - legSwitch].X += 0.004f * speed.X;
                    legAngles[1 - legSwitch].Y += 0.004f * speed.X;
                }
                else
                {
                    stepSwitch = true;
                    legSwitch = (legSwitch == 0 ? 1 : 0);
                }
            }
            return Math.Abs(legAngles[legSwitch].X - MathHelper.PiOver2) / 350f;
        }

        protected bool LookAt(Vector3 position)
        {
            bool see = false;
            headMatrix.Translation = Vector3.Transform(new Vector3(0, -7.5f, 14f), matrix);

            Vector3 direction = Vector3.Normalize(position - headMatrix.Translation);

            if (Vector3.Distance(direction, matrix.Down) < 1.2f)
            {
                if (scream < 0)
                {
                    direction = Util.RandomPointOnSphere(1);
                    GameEngine.gameCamera.shake += 5/Vector3.Distance(matrix.Translation,GameEngine.gameCamera.position);
                    if (scream < -5)
                    {
                        scream = 20;
                    }
                }

                headAngles.Y = Vector3.Dot(headMatrix.Right, direction) / 10f;
                headMatrix *= Matrix.CreateFromAxisAngle(matrix.Backward, headAngles.Y);
                headAngles.Z = Vector3.Dot(headMatrix.Backward, direction) / 10f;
                headMatrix *= Matrix.CreateFromAxisAngle(matrix.Left, headAngles.Z);
                see = true;
            }
            else
            {
                headAngles.Y = Vector3.Dot(headMatrix.Right, matrix.Down) / 10f;
                headMatrix *= Matrix.CreateFromAxisAngle(matrix.Backward, headAngles.Y);
                headAngles.Z = Vector3.Dot(headMatrix.Backward, matrix.Down) / 10f;
                headMatrix *= Matrix.CreateFromAxisAngle(matrix.Left, headAngles.Z);
            }
            headMatrix.Left = Vector3.Normalize(headMatrix.Left);
            headMatrix.Backward = Vector3.Normalize(headMatrix.Backward);
            headMatrix.Up = Vector3.Normalize(headMatrix.Up);
            headMatrix.Translation = Vector3.Transform(new Vector3(0, -7.5f, 16f), matrix);
            return see;
        }

        public override void Draw()
        {
           // Util.DrawMatrixAxis(matrix, GameEngine.gameCamera, 10);
            Vector3 temp = matrix.Translation;
            matrix.Translation += matrix.Backward * 13;

            //body
            model.Bones["body"].Transform = matrix;

            model.Bones["head"].Transform = headMatrix;
            Vector3 transformedPosition = Vector3.Transform(new Vector3(2f, 0, -3), matrix);

            Matrix builder = matrix;
            builder *= Matrix.CreateFromAxisAngle(matrix.Right, -(float)Math.Sin(legAngles[0].X));
            builder.Translation = transformedPosition;
            model.Bones["leg1_FR"].Transform = builder;

            builder *= Matrix.CreateFromAxisAngle(matrix.Right, (float)Math.Sin(legAngles[0].Y));
            builder.Translation = transformedPosition + model.Bones["leg1_FR"].Transform.Forward * 6;
            model.Bones["leg2_FR"].Transform = builder;

            transformedPosition = Vector3.Transform(new Vector3(-2f, 0, -3), matrix);

            builder = matrix;
            builder *= Matrix.CreateFromAxisAngle(matrix.Right, -(float)Math.Sin(legAngles[1].X));
            builder.Translation = transformedPosition;
            model.Bones["leg1_FL"].Transform = builder;

            builder *= Matrix.CreateFromAxisAngle(matrix.Right, (float)Math.Sin(legAngles[1].Y));
            builder.Translation = transformedPosition + model.Bones["leg1_FL"].Transform.Forward * 6;
            model.Bones["leg2_FL"].Transform = builder;

            matrix.Translation = temp;

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = mesh.ParentBone.Transform;
                    effect.View = GameEngine.gameCamera.viewMatrix;
                    effect.Projection = GameEngine.gameCamera.projectionMatrix;
                }
                mesh.Draw();
            }
        }
    }
}