using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Planetoid3D
{
     [Serializable]
    public class ManualRocket : BaseObjectBuilding
    {
         public ManualRocket() { }
         public ManualRocket(Planet Start, Matrix Initial, int Owner)
        {
            type = BuildingType.Rocket;
            planet = Start;
            whereIStarted = planet;
            flying = false;
            matrix = Initial;

            owner = Owner;
            speed = 0.1f;

            passengers = new Hominid[3];
            life = 150;
        }
       
        public Planet destination;
        public int index_destination;

        private Planet whereIStarted;
        private new float speed;

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
            matrix.Translation += matrix.Backward * speed;
            if (GameEngine.ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl))
            {
                if (speed < 1)
                {
                    speed += 0.1f;
                }
            }
            if (GameEngine.ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                if (speed > 0.1)
                {
                    speed *= 0.95f;
                }
            }
            if (GameEngine.ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
            {
                RotateToFace(matrix.Left, 0.02f);
            }
            if (GameEngine.ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
            {
                RotateToFace(matrix.Right, 0.02f);
            }
            if (GameEngine.ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
            {
                RotateToFace(matrix.Up, 0.02f);
            }
            if (GameEngine.ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
            {
                RotateToFace(matrix.Down, 0.02f);
            }
            if (BlackHoleGravity())
                return true;
            GameEngine.explosionParticles.SetSize(speed/2f);
            GameEngine.explosionParticles.SetColor(RaceManager.GetColor(PlayerManager.GetRace(owner)));
            GameEngine.explosionParticles.AddParticle(matrix.Translation+matrix.Forward*10, Vector3.Zero);
            GameEngine.explosionParticles.SetColor(Color.White);
            GameEngine.explosionParticles.SetSize(1);
            return (life <= 0);
        }

        public override void Draw()
        {
            //Util.DrawLine(matrix.Translation, planet.matrix.Translation, Color.Lime, Color.Lime, GameEngine.gameCamera);
            /*if (destination != null && flying)
            {
                Util.DrawLine(matrix.Translation, destination.matrix.Translation, RaceManager.GetColor(PlayerManager.GetRace(owner)), destination.GetColor(), GameEngine.gameCamera);
            }*/
            base.Draw();
        }
    }
}
