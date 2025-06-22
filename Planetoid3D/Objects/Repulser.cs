using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Planetoid3D
{
    [Serializable]
    public class Repulser : BaseObjectBuilding
    {
        public Repulser() { }
        public Repulser(Planet Planet, Matrix initial, int Owner)
        {
            planet = Planet;
            matrix = initial;
            type = BuildingType.Repulser;
            owner = Owner;
            charge = 0;
        }

        public float charge;
        public Vector3 hitPosition;
        public float hitTimer;
        private bool shieldOn;
        public bool active;
        public float presence;

        public override string SecondHUDLabel
        {
            get
            {
                return (active ? "Disable" : "Enable");
            }
        }

        public override void DoSecondHUDAction()
        {
            active = !active;
        }

        public override void Switch(bool active)
        {
           this.active = active;
        }

        public void ShieldHit(BaseObject body)
        {
            //body.life -= charge;
            hitPosition = Vector3.Normalize(body.matrix.Translation - planet.matrix.Translation);
            body.speed += hitPosition*5;
            hitTimer = (float)Math.Min(body.life*2,1);
            charge -= (body is Asteroid ? body.life * 20 : body.life)*1.2f;
            if (body.life <= 5 && body is Asteroid)
            {
                AudioManager.Play3D(body, "asteroid_death");
                for (int a = 0; a < body.life * 20; a++)
                {
                    GameEngine.fireParticles.AddParticle(body.matrix.Translation +  Util.RandomPointOnSphere(body.life * 4), Vector3.Zero);
                }
                GameEngine.asteroids.Remove((Asteroid)body);
            }
            body.life -= 5;
            AudioManager.Play3D(body, "repulser_impact");
        }

        public override string GetHudText()
        {
            return "Repulser Status: " + (active ? "Active" : "Off") + "\nShield Status: " + (shieldOn ? "Active" : "Off") + "\nCharge: " + ((int)Math.Max(0, charge)) + "%" + base.GetHudText();
        }

        private bool CanActivate
        {
            get
            {
                for (int a = 0; a < planet.buildings.Count; a++)
                {
                    if (planet.buildings[a]!=this && planet.buildings[a] is Repulser && (planet.buildings[a] as Repulser).presence > 0.1)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public override bool Update(float elapsed)
        {
            if (active)
            {
                if (shieldOn == false)
                {
                    if (PlayerManager.GetEnergy(owner) > 0.4)
                    {
                        if (charge < 100)
                        {
                            charge += 0.05f;
                            PlayerManager.ChangeEnergy(owner, -0.4f);
                        }
                        else if (CanActivate)
                        {
                            //Can activate only if no other shield is up
                            AudioManager.Play3D(this, "repulser_on");
                            shieldOn = true;
                            hitTimer = 0;
                        }
                    }
                }
                else
                {
                    //Check for shield hits
                    if (charge > 10)
                    {
                        if (presence < 1)
                        {
                            presence += 0.002f;
                        }
                        for (int a = 0; a < GameEngine.asteroids.Count; a++)
                        {
                            if (Vector3.Distance(GameEngine.asteroids[a].matrix.Translation, planet.matrix.Translation) <= planet.radius * 2)
                            {
                                ShieldHit(GameEngine.asteroids[a]);
                                break;
                            }
                        }
                        for (int a = 0; a < GameEngine.shots.Count; a++)
                        {
                            if (GameEngine.shots[a].owner != owner && Vector3.Distance(GameEngine.shots[a].matrix.Translation, planet.matrix.Translation) <= planet.radius * 2)
                            {
                                ShieldHit(GameEngine.shots[a]);
                                break;
                            }
                        }
                        for (int a = 0; a < GameEngine.planets.Count; a++)
                        {
                            if (a == 0)
                            {
                                for (int b = 0; b < (GameEngine.planets[0] as Sun).sunShots.Count; b++)
                                {
                                    if (Vector3.Distance(planet.matrix.Translation, (GameEngine.planets[0] as Sun).sunShots[b].matrix.Translation) < planet.radius * 2)
                                    {
                                        ShieldHit((GameEngine.planets[0] as Sun).sunShots[b]);
                                        (GameEngine.planets[0] as Sun).sunShots.RemoveAt(b);
                                    }
                                }
                            }
                            if (GameEngine.planets[a] != planet && GameEngine.planets[a].life>0 && Vector3.Distance(GameEngine.planets[a].matrix.Translation, planet.matrix.Translation) <= GameEngine.planets[a].radius + planet.radius * 2)
                            {
                                GameEngine.planets[a].speed *= 0.5f;
                                life = 0;
                                break;
                            }
                            for (int b = 0; b < GameEngine.planets[a].buildings.Count; b++)
                            {
                                if (GameEngine.planets[a].buildings[b].owner!=owner && GameEngine.planets[a].buildings[b] is Catapult && (GameEngine.planets[a].buildings[b] as Catapult).phase==4 && Vector3.Distance(GameEngine.planets[a].buildings[b].matrix.Translation, planet.matrix.Translation) <= planet.radius * 2)
                                {
                                    ShieldHit(GameEngine.planets[a].buildings[b]);
                                    GameEngine.planets[a].buildings[b].life = 0;
                                    break;
                                }
                            }
                        }
                        if (GameEngine.planetoid != null)
                        {
                            if (Vector3.Distance(GameEngine.planetoid.matrix.Translation, planet.matrix.Translation) <= 30 + planet.radius * 2)
                            {
                                GameEngine.planetoid.speed *= 0.5f;
                                life = 0;
                            }
                        }
                    }
                    else if (presence > 0.01f)
                    {
                        presence *= 0.97f;
                    }
                    else
                    {
                        AudioManager.Play3D(this, "repulser_off");
                        shieldOn = false;
                        charge = -100;
                    }
                    charge -= 0.001f;

                    //Decrease the hit timer
                    if (hitTimer > 0)
                    {
                        hitTimer *= 0.99f;
                    }
                }
            }
            else
            {
                shieldOn = false;
                if (presence > 0.01f)
                {
                    presence *= 0.96f;
                }
                else
                {
                    presence = 0;
                }
            }

            //The shield beam
            GameEngine.explosionParticles.SetColor(Color.DodgerBlue);
            GameEngine.explosionParticles.SetSize(charge / 400f);
            GameEngine.explosionParticles.AddParticle(matrix.Translation + matrix.Backward * 5, planet.ParticleSpeedFix + planet.speed);
            if (presence >= 0.4f && PlanetoidGame.details>0)
            {
                GameEngine.explosionParticles.AddParticle(matrix.Translation + matrix.Backward * (planet.radius - 4), planet.ParticleSpeedFix+planet.speed);
            }
            GameEngine.explosionParticles.SetSize(1);
            GameEngine.explosionParticles.SetColor(Color.White);

            return base.Update(elapsed);
        }

        public override void Draw()
        {
            base.Draw();
        }

        public override void SecondDraw()
        {
            if (presence > 0)
            {
                if (PlanetoidGame.details > 0)
                {
                    Util.DrawTextureLine(matrix.Translation + matrix.Backward * 5, matrix.Translation + matrix.Backward * (planet.radius - 4), 1, GameEngine.gameCamera, Color.Lerp(Color.Transparent, Color.Aqua, presence));
                }
                GameEngine.Game.GraphicsDevice.BlendState = BlendState.Additive;
                RenderManager.DrawShield(this);
                GameEngine.Game.GraphicsDevice.BlendState = BlendState.Opaque;
                GameEngine.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }
        }
    }
}
