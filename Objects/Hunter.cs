using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Planetoid3D
{
     [Serializable]
    public class Hunter : BaseObjectBuilding
    {
         public Hunter() { }
        public Hunter(Planet Start, Matrix Initial, int Owner)
        {
            type = BuildingType.Hunter;
            whereIStarted = Start;
            planet = Start;
            flying = false;
            matrix = Initial;
            owner = Owner;
        }
       
        public Vector3 target;
        public Planet destination;
        public int index_destination;

        private Vector3 direction;
        private Planet whereIStarted;
        private int timeout;
        private Ray ray;
        public BaseObjectBuilding nearestEnemy;
        public float reload;
        public Vector3 dodgeDirection;
        public Hominid pilot;
        private BaseObject collider;

        public override string FirstHUDLabel
        {
            get
            {
                return (pilot==null? "Sell" : "Evacuate");
            }
        }

        public override string SecondHUDLabel
        {
            get
            {
                return "Set Destination";
            }
        }

        public override void DoSecondHUDAction()
        {
            HUDManager.spaceshipSelected = true;
        }

        public override string GetHudText()
        {
            if (pilot!=null)
            {
                return "Pilot Specialization: " + pilot.specialization + base.GetHudText();
            }
            return "NO PILOT" + base.GetHudText();
        }

        public override void InSerialization()
        {
            planet = null;
            nearestEnemy = null;
            index_destination = GameEngine.planets.IndexOf(destination);
            destination = null;
            index_planet = GameEngine.planets.IndexOf(whereIStarted);
            whereIStarted = null;
            if (pilot != null)
            {
                pilot.InSerialization();
            }
        }

        public override void OutSerialization()
        {
            if (index_destination > -1)
            {
                destination = GameEngine.planets[index_destination];
            }
            if (index_planet > -1)
            {
                whereIStarted = GameEngine.planets[index_planet];
            }
            NearestPlanet(false, ref planet);
            if (pilot != null)
            {
                pilot.OutSerialization();
            }
        }

        public override void LiftOff(Planet Destination)
        {
            //Need a pilot to Lift Off
            if (Destination is Sun)
            {
                TextBoard.AddMessage("YOU CAN'T COLONIZE THE SUN!");
                return;
            }
            if (pilot!=null)
            {
                if (flying == false)
                {
                    timeout = (int)planet.radius * 3;
                }
                destination = Destination;
                flying = true;
            }
            else
            {
                TextBoard.AddMessage("YOU NEED TO ADD A PILOT FIRST");
            }
        }

        public override bool LeaveHominid()
        {
            if (pilot!=null && planet != null && flying == false)
            {
                pilot.planet = planet;
                pilot.matrix= GetMatrix(10,Vector3.Transform(matrix.Left,Matrix.CreateFromAxisAngle(matrix.Backward,(float)Util.random.NextDouble()*MathHelper.TwoPi)));
                pilot.speed = Vector3.Zero;
                planet.hominids.Add(pilot);
                pilot = null;
                if (planet.hominids.Count >= planet.maxPopulation * 3)
                {
                    QuestManager.QuestCall(8);
                }
                return false;
            }
            return true;
        }

        public override bool Update(float elapsed)
        {
            if (life > 0)
            {
                if (flying)
                {
                    //If I am flying
                    bool proceed = false;
                    //If I've started raising from the ground
                    if (timeout > 0)
                    {
                        //Take distance from planet's ground
                        timeout--;
                        Vector3 dir = Vector3.Normalize(destination.matrix.Translation - matrix.Translation);
                        RotateByAxis(matrix.Backward, Vector3.Dot(matrix.Right, dir) / 10f);
                        matrix.Translation += matrix.Backward * (pilot.specialization == Specialization.Pilot ? 1.5f : 1);
                        proceed = true;
                    }
                    else
                    {
                        //Get the nearest Enemy
                        FindNearestEnemy();
                        //If enemy exists
                        if (nearestEnemy != null)
                        {
                            CheckForBattleMusic(nearestEnemy);
                            //Engage combat
                            target = nearestEnemy.matrix.Translation;

                            //Get direction
                            direction = Vector3.Normalize(target - matrix.Translation);
                            //Get distance
                            float distance = Vector3.Distance(matrix.Translation, target);

                            //Manage distance
                            if (distance < 100)
                            {
                                dodgeDirection = -direction + Util.RandomPointOnSphere(1);
                            }
                            else if (distance > 250 + (100 - life))
                            {
                                dodgeDirection = Vector3.Zero;
                            }

                            //Execute dodge action
                            if (dodgeDirection != Vector3.Zero)
                            {
                                direction = dodgeDirection;
                                if (dodgeDirection.X > dodgeDirection.Z)
                                {
                                    RotateByAxis(matrix.Down, Math.Sign(dodgeDirection.X) / (pilot.specialization == Specialization.Pilot ? 10f : 20f));
                                }
                            }
                            else if (reload <= 0)
                            {
                                //Check if is shot-ready
                                if (Vector3.Distance(direction, matrix.Down) < 0.05f)
                                {
                                    reload = (pilot.specialization == Specialization.Soldier ? 0.5f : 1);
                                    Matrix tmp = Matrix.Invert(Matrix.CreateLookAt(Vector3.Zero, matrix.Down, Vector3.Up));
                                    tmp.Translation = matrix.Translation;
                                    GameEngine.shots.Add(new Shot(tmp, matrix.Down * 80, owner, 10));
                                    AudioManager.Play3D(this, "laser_soft");
                                }
                            }
                            else
                            {
                                reload -= elapsed;
                            }
                        }
                        //If you don't have found a valid enemy
                        else
                        {
                            //If destination has been destroyed return to the planet
                            if ((destination == null || destination.life <= 0))
                            {
                                //If my base planet is dead, search for the nearest planet
                                if (planet == null || planet.life <= 0)
                                {
                                    NearestPlanet(false,ref destination);
                                    //If there is no planet nearby
                                    if (destination == null)
                                    {
                                        //Explode
                                        life = 0;
                                        return true;
                                    }
                                }
                                else
                                {
                                    //Return back, destination is dead!
                                    destination = planet;
                                    return false;
                                }

                            }
                            else  //Get direction
                            {
                                target = destination.matrix.Translation;
                                direction = Vector3.Normalize(target - matrix.Translation);
                            }
                        }
                        //Go to destination planet
                        NearestPlanet(false,ref planet);

                        ray.Position = matrix.Translation;
                        ray.Direction = Vector3.Normalize(direction);

                        //If there is a planet in my warning range
                        if (planet != null /*&& nearestEnemy == null*/)
                        {
                            proceed = true;
                            //If I'm too near to the planet
                            if (Vector3.Distance(matrix.Translation, planet.matrix.Translation) < planet.radius * 3)
                            {
                                //If the planet is my destination and the landing zone is free, start landing procedure
                                if (planet == destination && nearestEnemy == null && BuildingManager.FreeBuildingPosition(planet, matrix.Translation))
                                {
                                    RotateToFace(direction, 0.05f);
                                    matrix.Translation += matrix.Forward / 2f;
                                    proceed = false;
                                }
                                //Avoid the planet, it's not the destination!
                                else if (ray.Intersects(new BoundingSphere(planet.matrix.Translation, planet.radius * 2)) != null)
                                {
                                    direction += Vector3.Normalize(matrix.Translation - planet.matrix.Translation);
                                }
                            }
                        }
                        else
                        {
                            //If I've not a planet, set destination to that variable to avoid multiple variable usage
                            planet = destination;
                            proceed = true;
                        }

                        //I'm actually flying, proceed towards the direction
                        if (proceed)
                        {
                            Rotate(direction, 0.05f);
                            //matrix.Down =//Vector3.Lerp(matrix.Down, direction, 0.1f);
                            matrix.Translation += matrix.Down * (pilot.specialization == Specialization.Pilot || pilot.Ability==6 ? 1.7f : 1.2f);
                        }

                        //I'm on the planet surface and I'm flying
                        if (planet != null && Vector3.Distance(matrix.Translation, planet.matrix.Translation) < planet.radius)//it is in collision
                        {
                            //Land
                            flying = false;
                            whereIStarted.buildings.Remove(this);
                            whereIStarted = planet;
                            whereIStarted.buildings.Add(this);


                            //Free the pilot
                            LeaveHominid();

                            //If my owner it's pc controlled
                            if (owner > 0)
                            {
                                //Tell my owner to interest to this planet
                                PlayerManager.players[owner].cpuController.RegisterPlanet(destination);
                            }

                            //if the camera was following me... make it follow my destination
                            if (HUDManager.lastTargetObject == this)
                            {
                                HUDManager.spaceshipSelected = false;
                                HUDManager.lastTargetObject = destination;
                                GameEngine.gameCamera.target = destination;
                            }

                            direction.Y = destination.radius;
                            return false;
                        }

                        if (BlackHoleGravity())
                        {
                            return true;
                        }
                        matrix.Translation += speed * elapsed;
                    }


                    //Generate particles
                    GameEngine.explosionParticles.SetSize(0.35f);
                    GameEngine.explosionParticles.SetColor(RaceManager.GetColor(PlayerManager.GetRace(owner)));
                    GameEngine.explosionParticles.AddParticle(matrix.Translation, (proceed ? Vector3.Zero : matrix.Forward * 30));
                    GameEngine.explosionParticles.SetSize(1);
                    GameEngine.explosionParticles.SetColor(Color.White);
                }
                else
                {
                    //The hunter is on a planet, it is treated like a normal building
                    //Don't use building standard update because the rocket MUST be in the planet with the head
                    matrix.Translation = Vector3.Zero;
                    matrix *= Matrix.CreateFromAxisAngle(planet.axis, planet.spinSpeed * elapsed);
                    matrix.Translation = planet.matrix.Translation + (matrix.Backward * planet.radius);
                    if (pilot==null)
                    {
                        GetCollidingObject( 10,ref collider);
                        if (collider != null && collider is Hominid && ((Hominid)collider).owner == owner)
                        {
                            pilot = ((Hominid)collider);
                            HominidEnteredBuilding((Hominid)collider);
                        }
                    }
                }
            }
            else
            {
                LeaveHominid();
            }

            return (life<=0);
        }

        public void Rotate(Vector3 wantedDirection, float turnSpeed)
        {
            wantedDirection.Normalize();
            Vector3 shipFront = matrix.Down;
            Vector3 shipRight;
            Vector3 shipUp = matrix.Backward;

            float dot = Vector3.Dot(shipFront, wantedDirection);

            if (dot > -0.99)
            {
                shipFront = Vector3.Lerp(shipFront, wantedDirection, turnSpeed);
            }
            else
            {
                // Special case for if we are turning exactly 180 degrees. 
                Rotate(matrix.Right, turnSpeed);
                return;
            }
            shipRight = Vector3.Cross(shipFront, shipUp);
            shipUp = Vector3.Cross(shipRight, shipFront);

            shipFront.Normalize();
            shipRight.Normalize();
            shipUp.Normalize();

            matrix.Down = shipFront;
            matrix.Left = shipRight;
            matrix.Backward = shipUp;
        }

        private void FindNearestEnemy()
        {
            float min = 400;
            float dist;
            nearestEnemy = null;
            //cycle through buildings
            for (int b = 0; b < GameEngine.planets.Count; b++)
            {
                for (int a = 0; a < GameEngine.planets[b].buildings.Count; a++)
                {
                    //yes it is a spaceship
                    if (GameEngine.planets[b].buildings[a] is Rocket == false)
                    {
                        if (GameEngine.planets[b].buildings[a] is Hunter == false)
                        {
                            if (GameEngine.planets[b].buildings[a].planet != destination)
                            {
                                continue;
                            }
                        }
                        else if (GameEngine.planets[b].buildings[a].flying == false)
                        {
                            continue;
                        }
                    }
                    else if (GameEngine.planets[b].buildings[a].flying == false)
                    {
                        continue;
                    }
                    //yes it is an enemy
                    if (GameEngine.planets[b].buildings[a].owner != owner && PlayerManager.GetFriendship(owner, GameEngine.planets[b].buildings[a].owner) < 0.5f /*&& !GameEngine.buildings[a].flying*/)
                    {
                        dist = Vector3.Distance(matrix.Translation, GameEngine.planets[b].buildings[a].matrix.Translation);
                        if (dist < min)
                        {
                            min = dist;
                            nearestEnemy = GameEngine.planets[b].buildings[a];
                        }
                    }
                }
            }
        }

        public override void Draw()
        {
            //Util.DrawLine(matrix.Translation, planet.matrix.Translation, Color.Lime, Color.Lime, GameEngine.gameCamera);
            if (destination != null && nearestEnemy==null && flying)
            {
                Util.DrawLine(matrix.Translation, destination.matrix.Translation, RaceManager.GetColor(PlayerManager.GetRace(owner)), destination.GetColor(), GameEngine.gameCamera);
            }
            base.Draw();
        }
    }
}
