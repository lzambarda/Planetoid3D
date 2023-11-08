using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Planetoid3D
{
     [Serializable]
    public class Rocket : BaseObjectBuilding
    {
         public Rocket() { }
        public Rocket(Planet Start, Matrix Initial, int Owner)
        {
            type = BuildingType.Rocket;
            planet = Start;
            whereIStarted = Start;
            flying = false;
            matrix = Initial;

            owner = Owner;

            passengers = new Hominid[3];
            life = 150;
        }
       
        public Planet destination;
        public int index_destination;

        private Vector3 direction;
        private Planet whereIStarted;

        public int timeout;
        private Ray ray;

        private BaseObject collider;

        //these are relative to the passengers
        public int passengersCount;
        public Hominid[] passengers;

        public override float SurfaceOffset
        {
            get
            {
                return 9;
            }
        }

        public override string FirstHUDLabel
        {
            get
            {
                return (passengersCount > 0 ? "Evacuate" : "Sell");
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

        public override void InSerialization()
        {
            planet = null;
            index_destination = GameEngine.planets.IndexOf(destination);
            destination = null;
            index_planet = GameEngine.planets.IndexOf(whereIStarted);
            whereIStarted = null;
            for (int a = 0; a < 3; a++)
            {
                if (passengers[a] != null)
                {
                    passengers[a].InSerialization();
                }
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
            for (int a = 0; a < 3; a++)
            {
                if (passengers[a] != null)
                {
                    passengers[a].OutSerialization();
                }
            }
        }

        public override string GetHudText()
        {
            if (passengersCount > 0)
            {
                string text="Passengers:\n   ";
                for (int a = 0; a < passengersCount; a++)
                {
                    text += passengers[a].specialization + "  ";
                }
                return text + base.GetHudText();
            }
            return "NO PASSENGERS"+base.GetHudText();
        }

        public override void LiftOff(Planet Destination)
        {
            //Need a pilot to Lift Off
            if (Destination is Sun)
            {
                TextBoard.AddMessage("YOU CAN'T COLONIZE THE SUN!");
                return;
            }
            if (passengersCount==3)
            {
                destination = Destination;
                if (flying == false)
                {
                    
                }
                flying = true;
                
            }
            else
            {
                TextBoard.AddMessage("YOU NEED "+(3-passengersCount)+" MORE PASSENGER"+(passengersCount<2?"S":""));
            }
        }

        public override bool  LeaveHominid()
        {
            if (flying == false)
            {
                for (int a = 0; a < passengersCount; a++)
                {
                    //Add the hominids and reload their specializations
                    passengers[a].planet = planet;
                    passengers[a].matrix= GetMatrix(20, Vector3.Transform(matrix.Left, Matrix.CreateFromAxisAngle(matrix.Backward, a)));
                    passengers[a].speed = Vector3.Zero;
                    planet.hominids.Add(passengers[a]);
                    passengers[a] = null;

                    if (planet.hominids.Count >= planet.maxPopulation * 3)
                    {
                        QuestManager.QuestCall(8);
                    }
                }
                if (passengersCount > 0)
                {
                    passengersCount = 0;
                    return false;
                }
            }
            return true;
        }

        public override bool Update(float elapsed)
        {
            if (flying)
            {
                //I am flying
                if (timeout > 0)
                {
                    //Taking distance from the ground
                    timeout--;
                    matrix.Translation += matrix.Backward * (planet.radius * 2 - timeout) / 100f;
                    RotateToFace(planet.matrix.Translation - matrix.Translation, 0.2f);
                }
                else
                {
                    //If destination has been destroyed return to the planet
                    if (destination == null || destination.life <= 0 || destination is Sun)
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
                            if (owner == 0)
                            {
                                QuestManager.QuestCall(15);
                            }
                            return false;
                        }
                    }

                    //Get nearest planet
                    NearestPlanet(false,ref planet);
                    if (planet == null)
                    {
                        life = 0;
                        return true;
                    }
                    direction = Vector3.Normalize(destination.matrix.Translation - matrix.Translation);

                    ray.Position = matrix.Translation;
                    ray.Direction = direction;

                    //Try to avoid the collision with the "not destination"
                    if (planet != null && planet != destination)
                    {
                        if (Vector3.Distance(matrix.Translation, planet.matrix.Translation) < planet.radius * 4 && ray.Intersects(new BoundingSphere(planet.matrix.Translation, planet.radius * 2)) != null)
                        {
                            direction += Vector3.Normalize(matrix.Translation - planet.matrix.Translation);
                        }
                    }
                    else
                    {
                        planet = destination;
                    }


                    //Land if colliding with destination ground
                    if (Vector3.Distance(matrix.Translation, planet.matrix.Translation) < planet.radius + 5)//it is in a planet gravity field
                    {
                        //model = Game1.buildingModels[1, 0];
                        flying = false;
                        whereIStarted.buildings.Remove(this);
                        whereIStarted = planet;
                        whereIStarted.buildings.Add(this);

                        LeaveHominid();

                        //If my owner it's pc controlled
                        if (owner > 0)
                        {
                            //Tell my owner to interest to this planet
                            PlayerManager.players[owner].cpuController.RegisterPlanet(destination);
                            PlayerManager.ChangeKeldanyum(owner, 400);
                            PlayerManager.ChangeEnergy(owner, 200);
                            PlayerManager.players[owner].cpuController.decisionTimer = 0;
                        }

                        direction.Y = destination.radius;

                        //if the camera was following me... make it follow my destination
                        if (HUDManager.lastTargetObject == this)
                        {
                            HUDManager.spaceshipSelected = false;
                            HUDManager.lastTargetObject = destination;
                            GameEngine.gameCamera.target = destination;
                        }
                        return false;
                    }

                    //Smoothly rotate to facing direction and move in the direction faced
                    RotateToFace(-direction, 0.05f);
                    if (passengers[0].Ability==6 ||
                        passengers[0].specialization == Specialization.Pilot ||
                        passengers[1].specialization == Specialization.Pilot ||
                        passengers[2].specialization == Specialization.Pilot)
                    {
                        matrix.Translation += matrix.Backward;
                    }
                    else
                    {
                        matrix.Translation += matrix.Backward / 1.5f;
                    }

                    if (BlackHoleGravity())
                    {
                        return true;
                    }
                    matrix.Translation += speed * elapsed;
                }
                //Add particles
                GameEngine.explosionParticles.SetSize(0.4f);
                GameEngine.explosionParticles.SetColor(RaceManager.GetColor(PlayerManager.GetRace(owner)));
                GameEngine.explosionParticles.AddParticle(matrix.Translation, Vector3.Zero);
                GameEngine.explosionParticles.SetColor(Color.White);
                GameEngine.explosionParticles.SetSize(1);
            }
            else
            {
                //The rocket is on a planet, it is treated like a normal building
                //Don't use building standard update because the rocket MUST be in the planet with the head
                matrix.Translation = Vector3.Zero;
                matrix *= Matrix.CreateFromAxisAngle(planet.axis, planet.spinSpeed * elapsed);
                planet = whereIStarted;

                //It will slowly fall into the ground until it disappear
                if (planet == destination)
                {
                    matrix.Translation = planet.matrix.Translation - (matrix.Backward * direction.Y);
                    direction.Y -= 0.01f;
                    if (direction.Y < planet.radius / 2)
                    {
                        return true;
                    }
                }
                else
                {
                    matrix.Translation = planet.matrix.Translation + (matrix.Backward * (9 + planet.radius));
                    if (passengersCount < 3)
                    {
                        GetCollidingObject( 15,ref collider);
                        if (collider != null && collider is Hominid && ((Hominid)collider).owner == owner)
                        {
                            passengers[passengersCount++] = ((Hominid)collider);
                            HominidEnteredBuilding((Hominid)collider);
                        }
                    }
                }
            }
            return (life <= 0);
        }

        public override void Draw()
        {
            //Util.DrawLine(matrix.Translation, planet.matrix.Translation, Color.Lime, Color.Lime, GameEngine.gameCamera);
            if (destination != null && flying)
            {
                Util.DrawLine(matrix.Translation, destination.matrix.Translation, RaceManager.GetColor(PlayerManager.GetRace(owner)), destination.GetColor(), GameEngine.gameCamera);
            }
            base.Draw();
        }
    }
}
