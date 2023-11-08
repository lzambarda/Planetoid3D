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
    public class BaseObjectOwned : BaseObject
    {
        public int owner;

        public override void Damage(BaseObjectOwned damager, float damages)
        {
            if (damager != null && owner<8)
            {
                if (PlayerManager.GetState(owner) > PlayerState.Human && planet!=null)
                {
                    //Raise dangerometer
                    try
                    {
                        PlayerManager.players[owner].cpuController.myPlanets.Find(p => p.planet == planet).danger += 0.1f;
                    }
                    catch (NullReferenceException e)
                    {
                    }
                }
                //Modify Relationships
                PlayerManager.PlayerAttack(damager.owner,owner, -damages / 500f, -damages / 750f);
            }
            base.Damage(null, damages);
        }

        /// <summary>
        /// Get the nearest building within a given range
        /// </summary>
        public void NearestRepairable(float minDist,ref BaseObjectOwned repairable)
        {
            float temp;
            repairable = null;
            for (int a = 0; a < planet.buildings.Count(); a++)
            {
                if (planet.buildings[a].life < 100)
                {
                    if (PlayerManager.GetFriendship(planet.buildings[a].owner, owner) >= 0.5f)
                    {
                        temp = Vector3.Distance(matrix.Translation, planet.buildings[a].matrix.Translation);
                        if (temp + planet.buildings[a].life < minDist)
                        {
                            minDist = temp + planet.buildings[a].life;
                            repairable = planet.buildings[a];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if the battle music must start/keep playing
        /// </summary>
        public void CheckForBattleMusic(BaseObjectOwned target)
        {
            if (target.owner==0 && PlayerManager.GetFriendship(owner,0) < 0.5f )
            {
                //This is used as a timer, so the music will not stop if a hominid avoid another for a few seconds :)
                AudioManager.battleIsHappening = (int)MathHelper.Min(AudioManager.battleIsHappening+5, 700);
            }
        }
    }
}
