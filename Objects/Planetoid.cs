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
    public class Planetoid : BaseObject
    {
        public Planetoid()
        {
            life = 2000;
            
            //Generate far away from sun
            matrix = Matrix.Identity * Matrix.CreateTranslation(Util.random.Next(50) + 5000, 2000 - Util.random.Next(4000), 0); 

            //Give a random speed
            speed = -Vector3.Normalize(Util.RandomPointOnSphere(500) + matrix.Translation);

            AudioManager.Play("planetoid_appear");
            AudioManager.StartPlanetoid();
            easterTimer = 0;
        }

        private float distance;

        /// <summary>
        /// This is an EASTER EGG!
        /// To get this the player has to look at the Planetoid for 10 seconds with maximum zoom
        /// </summary>
        private float easterTimer;

        public new void InSerialization()
        {
            planet = null;
        }

        public bool Update(float elapsed)
        {
            //If I got low of life
            if (life <= 0)
            {
                //Die
                //Flash the screen
                AudioManager.Play3D(this, "planetoid_meltdown");
                for (int a = 0; a < 200+(PlanetoidGame.details*50); a++)
                {
                    GameEngine.planetoidParticles.AddParticle(matrix.Translation, Util.RandomPointOnSphere(5+Util.random.Next(10)));
                }
                Flash(5f);
                GameEngine.gameCamera.shake += 10;
                return true;
            }
            //If I'm too far
            if (matrix.Translation.Length() > 7000)
            {
                //Return in the solar system
                speed -= Vector3.Normalize(matrix.Translation)/100f;
                speed += matrix.Up / 1000f;
            }

            elapsed *= 6;
            
            distance = Vector3.Distance(matrix.Translation, GameEngine.gameCamera.position);
            if (distance <= 3000)
            {
                AudioManager.UpdatePlanetoid(distance);
                if (distance < 600)
                {
                    if (GameEngine.gameCamera.shake < (600 - distance) / 240f)
                    {
                        GameEngine.gameCamera.shake += 0.5f;
                    }
                }
            }
            else
            {
                AudioManager.ChangeVolume("Planetoid", 0);
            }
            if (AudioManager.music_volume > 0 && GameEngine.gameCamera.target == this && GameEngine.gameCamera.zoom < 160)
            {
                easterTimer += elapsed;
                if (easterTimer > 180)
                {
                    easterTimer = -float.MaxValue;
                    AudioManager.StartEasterEgg();
                    QuestManager.QuestCall(6);
                }
            }
            else if (easterTimer > 0)
            {
                easterTimer = 0;
            }

            //Get nearest planet
            NearestPlanet(true,ref planet);

            //If nearest planet is not null
            if (planet != null)
            {
                distance = Vector3.Distance(matrix.Translation, planet.matrix.Translation);
                //If  I am in a planet gravity field
                if (distance<planet.radius * 50)
                {
                    if (planet is Sun)
                    {
                        if (distance<planet.radius+350)
                        {
                            //Start shaking camera
                            GameEngine.gameCamera.shake += 75f / Vector3.Distance(matrix.Translation, GameEngine.gameCamera.position);
                            TextBoard.AddMessage("A strong magnetic activity is being developed near the sun!");
                            if (Vector3.Distance(matrix.Translation,planet.matrix.Translation)< planet.radius + 50)
                            {
                                Flash(0.005f);
                                GameEngine.fireParticles.AddParticle(matrix.Translation + speed + Util.RandomPointOnSphere(10), -speed);
                                if (distance< GameEngine.planets[0].radius)
                                {
                                    //Flash the screen
                                    AudioManager.Play3D(this, "planetoid_meltdown");
                                    Flash(1f);
                                    //If the camera was looking at me, change the camera's target to the sun
                                    if (GameEngine.gameCamera.target == this)
                                    {
                                        GameEngine.gameCamera.target = GameEngine.planets[0];
                                    }
                                    //Sun turns green
                                    ((Sun)GameEngine.planets[0]).timer = 500;
                                    //Die
                                    return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (distance < planet.radius * 8)
                        {
                            //All the hominids scream!
                            for (int a = 0; a < planet.hominids.Count; a++)
                            {
                                planet.hominids[a].Speak(SpeechType.Planetoid);
                            }
                        }
                        //If the planet is no the sun, then destroy it!
                        if (distance< planet.radius * 2)
                        {
                            planet.life = 0;
                            planet.deathTimer = 1;
                        }
                    }          
                    
                    //Apply gravity to speed
                    speed += Vector3.Normalize(planet.matrix.Translation - matrix.Translation) / 150 * elapsed;
                }
            }
            speed.Normalize();
            speed *=4;
            //Update planetoid position
            matrix.Translation += speed * elapsed;
            //Add particles
            for (int a = PlanetoidGame.details; a >= 0; a--)
            {
                GameEngine.planetoidParticles.AddParticle(matrix.Translation + speed * 2 + Util.RandomPointOnSphere(20), -speed*4);
            }

            RotateToFace(speed, 1);

            return false;
        }
    }
}
