using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Planetoid3D
{
     [Serializable]
    public class Catapult : BaseObjectBuilding
    {
         public Catapult() { }
         public Catapult(Planet Start, Matrix Initial, int Owner)
        {
            type = BuildingType.Catapult;
            planet = Start;
            flying = false;
            matrix = Initial;

            owner = Owner;

            life = 100;
        }

        public int timeout;
        public Vector3 direction;
        public bool orbiting;
        public Ray ray;
        public float wingAngle;
        public float edgeAmount;
        public float charge;
        private Planet target;
        public int index_target;
        public bool firing;
        public int phase;
        private Asteroid nearestAsteroid;

        public override float SurfaceOffset
        {
            get
            {
                return 7;
            }
        }

        public override string SecondHUDLabel
        {
            get
            {
                return (flying ? (target!=null ? "Cancel Target" : "Set Target") : "Launch");
            }
        }

        public override void DoSecondHUDAction()
        {
            if (phase < 4)
            {
                if (flying == false)
                {
                    timeout = (int)planet.radius * 2;
                    flying = true;
                    phase = 1;
                }
                else if (target != null)
                {
                    target = null;
                }
                else
                {
                    HUDManager.spaceshipSelected = true;
                }
            }
        }

        public override void InSerialization()
        {
            nearestAsteroid = null;
            index_target = GameEngine.planets.IndexOf(target);
            target = null;
            base.InSerialization();
        }

        public override void OutSerialization()
        {
            if (index_target > -1)
            {
                target = GameEngine.planets[index_target];
            }
            base.OutSerialization();
        }

        public override string GetHudText()
        {
            return "Status: " + (phase == 0 ? "Ready for liftoff" : (phase == 1 ? "Reaching sun orbit" : (phase == 2 ? "Charging..." : (phase == 3 ? "Aiming at target" : "Preparing to crash")))) + "\nCharge: " + Math.Max(0, (int)charge) + "%" + base.GetHudText();
        }

        public override void LiftOff(Planet Destination)
        {
            target = Destination;
        }

        public override bool Update(float elapsed)
        {
            switch (phase)
            {
                case 0:
                    //The catapult is on a planet, it is treated like a normal building
                    base.Update(elapsed);
                    //matrix.Translation = planet.matrix.Translation + (matrix.Backward * (SurfaceOffset + planet.radius));
                    break;
                case 1:
                    if (target == null || target.life <= 0)
                    {
                        target = null;
                    }
                    if (timeout > 0)
                    {
                        //Taking distance from the ground
                        timeout--;
                        matrix.Translation += matrix.Backward * (planet.radius * 2 - timeout) / 100f;
                        RotateToFace(planet.matrix.Translation - matrix.Translation, 0.2f);
                    }
                    else
                    {
                        direction = Vector3.Normalize((Vector3.Cross(Vector3.Normalize(-matrix.Translation), Vector3.Up) * 500 - matrix.Translation));
                        if (matrix.Translation.Length() > 500)
                        {
                            //Get nearest planet
                            NearestPlanet(false, ref planet);
                            if (planet == null)
                            {
                                life = 0;
                                return true;
                            }
                            direction.Normalize();
                            ray.Position = matrix.Translation;
                            ray.Direction = direction;

                            //Try to avoid the collision with the "not destination"
                            if (planet != null && planet is Sun == false)
                            {
                                if (Vector3.Distance(matrix.Translation, planet.matrix.Translation) < planet.radius * 4 && ray.Intersects(new BoundingSphere(planet.matrix.Translation, planet.radius * 2)) != null)
                                {
                                    direction += Vector3.Normalize(matrix.Translation - planet.matrix.Translation);
                                }
                            }
                            //Smoothly rotate to facing direction and move in the direction faced
                            speed = matrix.Backward / (1.5f * elapsed);
                            if (wingAngle > 0)
                            {
                                wingAngle -= 0.002f;
                            }
                            if (edgeAmount > 0)
                            {
                                edgeAmount -= 0.002f;
                            }
                            if (charge < 100)
                            {
                                charge += 0.0001f;
                            }
                        }
                        else
                        {
                            phase = 2;
                        }
                    }
                    break;
                case 2:
                    if (target == null || target.life <= 0)
                    {
                        target = null;
                    }
                    if (NearestAsteroid() && !firing)
                    {
                        life -= 0.1f;
                        nearestAsteroid.speed -= Vector3.Normalize(matrix.Translation - nearestAsteroid.matrix.Translation) * 5;
                    }
                    if (matrix.Translation.Length() < GameEngine.planets[0].radius + 10)
                    {
                        life = 0;
                    }
                    direction = -Vector3.Normalize(matrix.Translation);
                    speed += direction * 3 * elapsed;

                    if (wingAngle < MathHelper.PiOver2)
                    {
                        wingAngle += 0.002f;
                    }
                    if (edgeAmount < 1.5f)
                    {
                        edgeAmount += 0.002f;
                    }
                    else if (charge < 100)
                    {
                        charge += 0.05f;
                    }
                    else if (target!=null)
                    {
                        phase = 3;
                    }

                    break;
                case 3:
                    if ( target == null ||target.life <= 0)
                    {
                        target = null;
                        phase = 2;
                        break;
                    }
                    if (NearestAsteroid() && !firing)
                    {
                        life -= 0.1f;
                        nearestAsteroid.speed -= Vector3.Normalize(matrix.Translation - nearestAsteroid.matrix.Translation) * 5;
                    }
                    if (matrix.Translation.Length() < GameEngine.planets[0].radius + 10)
                    {
                        life = 0;
                    }

                    direction = Vector3.Normalize(target.matrix.Translation - matrix.Translation);
                    speed -= Vector3.Normalize(matrix.Translation) * 3 * elapsed;

                    if (edgeAmount > 0)
                    {
                        edgeAmount -= 0.002f;
                    }
                    if (wingAngle > 0)
                    {
                        wingAngle -= 0.002f;
                    }
                    else if (target != null && Vector3.Distance(matrix.Backward,direction) < 0.1f )
                    {
                        if (speed.Length() > 3f)
                        {
                            speed *= 0.98f;
                        }
                        else if (target.life <= 0)
                        {
                            target = null;
                        }
                        else
                        {
                            ray.Direction = Vector3.Normalize(target.matrix.Translation - matrix.Translation);
                            ray.Position = matrix.Translation;
                            if (ray.Intersects(new BoundingSphere(Vector3.Zero, GameEngine.planets[0].radius + 20)) == null)
                            {
                                phase = 4;
                            }
                        }
                    }
                    break;
                case 4:
                    if (charge > 0 && target!=null && target.life>0)
                    {
                        charge -= 0.025f;
                    }
                    else
                    {
                        life = 0;
                        return true;
                    }
                    direction = Vector3.Normalize(target.matrix.Translation+target.ParticleSpeedFix*Vector3.Distance(target.matrix.Translation,matrix.Translation)/200f - matrix.Translation);
                    if (speed.Length() < 300)
                    {
                        speed += direction / 2f;
                    }
                    NearestPlanet(true, ref planet);
                    if (planet == null)
                    {
                        life = 0;
                        return true;
                    }
                    GameEngine.explosionParticles.SetColor(Color.DodgerBlue);
                    GameEngine.explosionParticles.AddParticle(matrix.Translation + matrix.Backward * 15 + Util.RandomPointOnSphere(5), Vector3.Zero);
                    GameEngine.explosionParticles.SetColor(Color.White);

                    if (Vector3.Distance(planet.matrix.Translation, matrix.Translation)<planet.radius+10)
                    {
                        int own = planet.DominatingRace();
                        if (own > -1)
                        {
                            own = PlayerManager.GetRaceOwner(own);
                            if (own != owner)
                            {
                                PlayerManager.ChangeFriendship(own, owner, -0.4f);
                                PlayerManager.ChangeTrust(own, owner, -0.2f);
                            }
                        }
                        //if the camera was following me... make it follow my destination
                        if (HUDManager.lastTargetObject == this)
                        {
                            HUDManager.spaceshipSelected = false;
                            HUDManager.lastTargetObject = target;
                            GameEngine.gameCamera.target = target;
                        }
                        planet.life -= 4;
                        life = 0;
                    }
                    break;
            }


            /* direction = Vector3.Normalize(target.matrix.Translation - matrix.Translation);
             GameEngine.explosionParticles.SetColor(Color.DodgerBlue);
             GameEngine.explosionParticles.SetSize(charge / 100f);
             GameEngine.explosionParticles.AddParticle(nearestAsteroid.matrix.Translation, 5 * nearestAsteroid.speed);
             GameEngine.explosionParticles.SetSize(1);
             GameEngine.explosionParticles.SetColor(Color.White);
             Vector3 dir = matrix.Translation + matrix.Up * 4 + matrix.Backward * 20 - nearestAsteroid.matrix.Translation;
             nearestAsteroid.speed += Vector3.Normalize(dir);
             nearestAsteroid.speed *= 0.95f;
             charge -= 0.1f;
             if (dir.Length() < 5 && Vector3.Distance(matrix.Backward, Vector3.Normalize(target.matrix.Translation - matrix.Translation)) < 0.1f)
             {
                 fireTimer += 0.01f;
                 if (fireTimer >= 1)
                 {
                     nearestAsteroid.Burst(10, Color.DodgerBlue, 10);
                     nearestAsteroid.speed += matrix.Backward * 50;
                     charge = 0;
                     firing = false;
                     nearestAsteroid = null;
                     fireTimer = 0;
                 }
             }*/
            if (phase > 0)
            {
                RotateToFace(-Vector3.Normalize(direction), (phase==4?0.05f:0.02f));

                if (BlackHoleGravity())
                {
                    return true;
                }
                matrix.Translation += speed * elapsed;
            }
            if (phase == 1 || phase == 4)
            {
                //Add particles
                GameEngine.explosionParticles.SetSize(0.4f);
                GameEngine.explosionParticles.SetColor(RaceManager.GetColor(PlayerManager.GetRace(owner)));
                GameEngine.explosionParticles.AddParticle(matrix.Translation, Vector3.Zero);
                GameEngine.explosionParticles.SetColor(Color.White);
                GameEngine.explosionParticles.SetSize(1);
            }

            return (life <= 0);
        }

        private bool NearestAsteroid()
        {
            float min = 200;
            float temp;
            nearestAsteroid = null;
            foreach (Asteroid asteroid in GameEngine.asteroids)
            {
                temp = Vector3.Distance(matrix.Translation, asteroid.matrix.Translation)-asteroid.life*20;
                if (temp < min)
                {
                    min = temp;
                    nearestAsteroid = asteroid;
                }
            }
            return (min < 5);
        }

        public override void Draw()
        {
            /*if (flying && orbiting == false)
            {
                Util.DrawLine(matrix.Translation, Vector3.Zero, RaceManager.GetColor(PlayerManager.GetRace(owner)), GameEngine.planets[0].GetColor(), GameEngine.gameCamera);
            }
            if (target != null && orbiting)
            {
                Util.DrawLine(matrix.Translation, target.matrix.Translation, RaceManager.GetColor(PlayerManager.GetRace(owner)), target.GetColor(), GameEngine.gameCamera);
                /*if (firing)
                {
                    Util.DrawLine(matrix.Translation, nearestAsteroid.matrix.Translation, Color.CornflowerBlue, Color.CornflowerBlue, GameEngine.gameCamera);
                }*/
            //}*/


            Model model = RenderManager.GetModel(this);

            model.Bones["Body"].Transform = matrix;
            Matrix temp = matrix;
            temp.Translation += matrix.Backward * (2+edgeAmount*7);
            model.Bones["Edge"].Transform = temp;

            temp = matrix * Matrix.CreateFromAxisAngle(matrix.Left, wingAngle);
            temp.Translation = matrix.Translation + matrix.Up * 4 + matrix.Forward * 5;
            model.Bones["LeftWing"].Transform = temp;

            temp = matrix * Matrix.CreateFromAxisAngle(matrix.Right, wingAngle);
            temp.Translation = matrix.Translation + matrix.Down * 4 + matrix.Forward * 5;
            model.Bones["RightWing"].Transform = temp;

            base.DrawUsingBones();
        }

        public override void SecondDraw()
        {
            if (target != null)
            {
                Util.DrawLine(matrix.Translation, target.matrix.Translation, RaceManager.GetColor(PlayerManager.GetRace(owner)), target.GetColor(), GameEngine.gameCamera);
            }
            if (firing)
            {
                if (PlanetoidGame.details > 0)
                {
                    Util.DrawTextureLine(matrix.Translation + matrix.Backward * 15, Vector3.Normalize(matrix.Translation)*GameEngine.planets[0].radius,4, GameEngine.gameCamera, Color.DodgerBlue);
                    GameEngine.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    GameEngine.Game.GraphicsDevice.BlendState = BlendState.Opaque;
                }
                else
                {
                    Util.DrawLine(matrix.Translation + matrix.Backward * 15, Vector3.Normalize(matrix.Translation) * GameEngine.planets[0].radius, Color.DodgerBlue, Color.DodgerBlue, GameEngine.gameCamera);
                }
            }
        }
    }
}
