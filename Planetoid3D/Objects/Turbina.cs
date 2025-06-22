using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Planetoid3D
{
     [Serializable]
    public class Turbina : BaseObjectBuilding
    {
        public Turbina() { }
        public Turbina(Planet Planet, Matrix initial, int Owner)
        {
            planet = Planet;
            matrix = initial;
            type = BuildingType.Turbina;
            angle = 0;
            speed.X = 0;

            owner = Owner;
            replacer = Atmosphere.None;
            //Find a breathable atmosphere for my owner's race
            while (!RaceManager.Tolerate(PlayerManager.GetRace(owner),replacer))
            {
                replacer = (Atmosphere)Util.random.Next(7);
            }
           // AudioManager.Load(ref audio);
        }

        Atmosphere replacer;
        float angle;
        float timerout;
        public bool turned_on;
        //Cue audio;
        //AudioEmitter emitter;

        public override string SecondHUDLabel
        {
            get
            {
                return (turned_on ? "Disable" : "Enable");
            }
        }

        public override void DoSecondHUDAction()
        {
            turned_on = !turned_on;
            /*if (turned_on)
            {
                AudioManager.Play3D(this, "turbina_start");
                timerout = 1.35f;
            }
            else
            {
                audio.Stop(AudioStopOptions.AsAuthored);
                AudioManager.Play3D(this, "turbina_stop");
                AudioManager.Load(ref audio);
            }*/
        }

        public override void Switch(bool active)
        {
            turned_on = active;
           /* if (active)
            {
                AudioManager.Play3D(this, "turbina_start");
                timerout = 1.35f;
            }
            else
            {
                audio.Stop(AudioStopOptions.AsAuthored);
                AudioManager.Play3D(this, "turbina_stop");
                AudioManager.Load(ref audio);
            }*/
        }

        public override string GetHudText()
        {
            if (turned_on)
            {
                return "Status: Active and working.\nCompletion: " + (100-planet.atmosphere_level) + "%" + base.GetHudText();
            }

            return "Status: Currently offline." + base.GetHudText();
        }

        public override bool Update(float elapsed)
        {
            //If turned on and with enough energy
            if (PlayerManager.GetEnergy(owner) > 5 && turned_on)
            {
               /* if (audio.IsPlaying)
                {
                    float distance = MathHelper.Clamp(Vector3.Distance(matrix.Translation, GameEngine.gameCamera.position), 0, 3000);
                    distance = (float)Math.Pow(1 - (distance / 3000f), 2);
                    audio.SetVariable("Distance", distance * (AudioManager.sound_volume / 10f));
                    AudioManager.soundBank.GetCue("turbina_loop").SetVariable("Distance", distance * (AudioManager.sound_volume / 10f));
                }*/
                //If rotation completed
                if (angle+speed.X*2 > MathHelper.TwoPi)
                {
                    //Add particle
                    GameEngine.smokeParticles.SetColor(Planet.GetAtmoshpereColor(replacer));
                    GameEngine.smokeParticles.AddParticle(planet.matrix.Translation + (matrix.Backward * (planet.radius + 13f)), matrix.Backward * speed.X * 10);
                    GameEngine.smokeParticles.SetColor(Color.White);

                    //If the atmosphere is not the one the owner need
                    if (planet.atmosphere != replacer)
                    {
                        //Decrease timer
                        timerout -= speed.X * elapsed * 64f;
                        if (timerout <= 0)
                        {
                            /*if (audio.IsPlaying == false)
                            {
                                audio.Play();
                            }*/
                            //The atmosphere conversion speed is proportional to the planet size
                            timerout = planet.radius/1.2f;
                            //Decrease owner's energy
                            PlayerManager.ChangeEnergy(owner, - 5);
                            //Decrease planet's atmosphere
                            planet.atmosphere_level -= (1 + PlayerManager.players[owner].researchLevels[3]/2);
                            //Replace atmosphere
                            if (planet.atmosphere_level < 1)
                            {
                                planet.atmosphere = replacer;
                                if (owner == 0)
                                {
                                    QuestManager.QuestCall(11);
                                }
                                //Switch off automatically if atmosphere has been modified correctly
                                Switch(false);
                            }
                        }
                    }
                    else
                    {
                        //Switch off because the atmosphere is correct
                        Switch(false);
                    }
                }
                //If turned on, increase rotation speed
                if (speed.X < 1f)
                {
                    speed.X += 0.001f;
                }
            }
            else if (speed.X > 0)
            {
                //If turned off, decrease rotation speed
                speed.X -= 0.001f;
            }

            //Increase angle
            angle += speed.X * elapsed * 16f;
            if (angle > MathHelper.TwoPi)
            {
                angle -= MathHelper.TwoPi;
            }

            return base.Update(elapsed);
        }

        public override void Draw()
        {
            Model model = RenderManager.GetModel(this);
            model.Bones[2].Transform = matrix;

            Matrix temp = matrix;
            temp.Translation = Vector3.Zero;
            temp *= Matrix.CreateFromAxisAngle(matrix.Backward, angle);
            temp.Translation = planet.matrix.Translation + (matrix.Backward * (planet.radius + 10f));
            model.Bones[1].Transform = temp;

            base.DrawUsingBones();
        }
    }
}
