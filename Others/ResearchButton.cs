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
    public class ResearchButton : Button
    {
        public ResearchButton() { }
        public ResearchButton(Research myResearch)
        {
            research = myResearch;
            fadeIn = 1;
            fadeOut = 1;
            this.width = 300;
        }

        public Research research;
        public float fadeIn;
        public float fadeOut;

        public bool UpdateResearch()
        {
            if (fadeOut < 0.9f)
            {
                fadeOut *= 1.1f;
                return false;
            }
            else if (fadeOut < 1)
            {
                fadeOut = 1;
                return true;
            }

            if (fadeIn > 0.001f)
            {
                fadeIn *= 0.9f;
            }
            else
            {
                fadeIn = 0;
                //Real Update
                Update();
                if (IsClicked())
                {
                    //Give back to the player the resources taken
                    PlayerManager.ChangeKeldanyum(0, research.cost.X * BuildingManager.ResearchLevels(0)[research.index]);
                    PlayerManager.ChangeEnergy(0, research.cost.Y * BuildingManager.ResearchLevels(0)[research.index]);
                    fadeOut = 0.1f;
                }
            }

            return false;
        }

        public void DrawResearch(SpriteFont font, int seconds)
        {
            text = research.title + " " + BuildingManager.ResearchLevels(0)[research.index];
            float amount = (float)seconds / (research.seconds * BuildingManager.ResearchLevels(0)[research.index]);
            if (seconds > 0)
            {
                text += " - " + (int)(amount * 100) + " %";
            }
            // Draw(spriteBatch, font, amount);
            Draw(font, Color.Lerp(Color.OrangeRed, Color.LawnGreen, amount));
        }
    }
}
