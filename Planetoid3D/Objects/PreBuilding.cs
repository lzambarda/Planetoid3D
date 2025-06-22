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

using System.Xml.Serialization;

namespace Planetoid3D
{
    [Serializable]
    [XmlInclude(typeof(BaseObjectBuilding))]
    public class PreBuilding : BaseObjectBuilding
    {
        public PreBuilding() { }
        public PreBuilding(BaseObjectBuilding building)
        {
            planet = building.planet;
            matrix = building.matrix;
            type = building.type;
            //The average game speed is 60 fps per second
            //To this: second(1) : 60 = total : step
            //So: step = 1/(60*total)
            //"step" is the value you must use to decrease the timer
            total = BuildingManager.GetMenuVoice(type).seconds;
            step = 1 / 60f;
            owner = building.owner;

            if (GameEngine.gameMode != GameMode.Hyperbuild)
            {
                timer = total;
                matrix.Translation = planet.matrix.Translation + (matrix.Backward * planet.radius / 1.5f);
            }
            tempPos = matrix.Translation;
            mybuilding = building;
        }

        public float step;
        public float timer;
        public float total;
        public BaseObjectBuilding mybuilding;
        public Vector3 tempPos;

        public override void InSerialization()
        {
            mybuilding.InSerialization();
            base.InSerialization();
        }

        public override void OutSerialization()
        {
            mybuilding.OutSerialization();
            base.OutSerialization();
        }

        public override string GetHudText()
        {
            return "Under Construction... "+(int)((1-timer/total)*100)+"%"+base.GetHudText();
        }
        
        /// <summary>
        /// Used to return a value between 0.0 and 1.0 indicating the percentual of building completion
        /// </summary>
        public float GetCompletion()
        {
            return 1 - (timer / total);
        }

        public override bool Update(float elapsed)
        {
            matrix.Translation = tempPos;
            //Slowly raise from the ground
            if (timer > 0)
            {
                if /*(planet.DominatingRace() != PlayerManager.GetRace(owner))*/(PlayerManager.GetFriendship(PlayerManager.GetRaceOwner(planet.DominatingRace()),owner)<0.5f && !planet.hominids.Exists(h=>h.owner==owner) )
                {
                    timer += step * 25 * elapsed;
                    if (timer >= total)
                    {
                        return true;
                    }
                }
                float howmany = 0;
                for (int a = 0; a < planet.hominids.Count; a++)
                {
                    if (planet.hominids[a].target == this && PlayerManager.GetFriendship(owner, planet.hominids[a].owner) >= 0.5f && Vector3.Distance(matrix.Translation, planet.hominids[a].matrix.Translation) < (GameEngine.gameMode == GameMode.Giant ? 60 : 30))
                    {
                        if (owner>0 && planet.hominids[a].owner != owner)
                        {
                            //AI will be glad if someone helps it
                            PlayerManager.ChangeFriendship(planet.hominids[a].owner, owner, 0.0002f);
                            PlayerManager.ChangeTrust(planet.hominids[a].owner, owner, 0.0001f);
                        }
                        howmany += (planet.hominids[a].Ability == 3 ? 1.5f : 1)/(howmany==0?1:3);//don't give too much boost during construction
                        planet.hominids[a].Speak(SpeechType.Build);
                    }
                }
                //Decrease timer
                timer -= step * 50 * elapsed * (float)howmany;


                //Update planet's raising
                matrix.Translation = planet.matrix.Translation + (matrix.Backward * (planet.radius / 1.5f + planet.radius / 3 * GetCompletion() + mybuilding.SurfaceOffset));

                //Add particles
                GameEngine.tsmokeParticles.SetColor((howmany == 0 ? Color.Red : Color.Green));
                GameEngine.tsmokeParticles.AddParticle(matrix.Translation + matrix.Backward * 2 + Util.RandomPointOnSphere(10), planet.ParticleSpeedFix + matrix.Backward * 6);
                GameEngine.tsmokeParticles.SetColor(Color.White);
            }
            //Building is finished!
            else
            {
                //Add a new building inGame
                BuildingManager.AddBuilding(type, planet, matrix, owner);
                if (HUDManager.lastTargetObject == this)
                {
                    HUDManager.lastTargetObject = planet.buildings.Last();
                }
                if (owner == 0)
                {
                    AudioManager.Play("building_completed");
                }

                //remove this building
                return true;
            }
            mybuilding.matrix.Translation -= planet.oldPosition;
            mybuilding.matrix *= Matrix.CreateFromAxisAngle(planet.axis, planet.spinSpeed * elapsed);
            mybuilding.matrix.Translation += planet.matrix.Translation;
            //Rotate with planet
            bool returner=base.Update(elapsed);
            tempPos = matrix.Translation;
            matrix.Translation = mybuilding.matrix.Translation;
            return returner;
        }

        public override void Draw()
        {        
            Vector3 temp = mybuilding.matrix.Translation;
            mybuilding.matrix.Translation = tempPos;
            mybuilding.Draw();
            mybuilding.matrix.Translation = temp;

            //GameEngine.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GameEngine.Game.GraphicsDevice.BlendState = BlendState.Additive;
            mybuilding.Draw();
            GameEngine.Game.GraphicsDevice.BlendState = BlendState.Opaque;
        }
    }
}