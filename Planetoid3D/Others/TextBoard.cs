using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Planetoid3D
{
    public static class TextBoard
    {
        private static string[] messages;
        private static float[] timers;
        private static Vector2 startPoint;

        public static void Initialize(GraphicsDeviceManager graphics)
        {
            startPoint = new Vector2(10, 150);
            messages = new string[16];
            timers = new float[messages.Length];
            ResetTextboard();
        }

        /// <summary>
        /// Erase all textboard messages
        /// </summary>
        public static void ResetTextboard()
        {
            for (int a = 0; a < messages.Length; a++)
            {
                messages[a] = "";
                timers[a] = 0;
            }
        }

        /// <summary>
        /// Erase all textboard messages
        /// </summary>
        public static void AddMessage(string message)
        {
            if (IsMessageContained(message))
            {
                return;
            }
            //shift messages
            for (int a = messages.Length - 1; a > 0; a--)
            {
                messages[a] = messages[a - 1];
                timers[a] = timers[a - 1];
            }
            //finally add message
            messages[0] = message;
            timers[0] = 1;
        }

        /// <summary>
        /// Check if the TextBoard contains a specified text
        /// </summary>
        public static bool IsMessageContained(string message)
        {
            for (int a = messages.Length - 1; a >= 0; a--)
            {
                if (timers[a]>0.2f && messages[a].Contains(message))
                {
                    //Refresh interested message
                    timers[a] = 1;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Draw the textboard (update is included)
        /// </summary>
        public static void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            spriteBatch.Begin();
            for (int a = 0; a < messages.Length; a++)
            {
                if (timers[a] > 0)
                {
                    if (timers[a] > 0.8f)
                    {
                        timers[a] -= 0.0002f;
                    }
                    else
                    {
                        timers[a] -= 0.01f;
                    }
                }
                spriteBatch.DrawString(font, messages[a], startPoint + new Vector2(0, a * 20+(GameEngine.gameMode==GameMode.Tutorial?100:0)), Color.Lerp(Color.Transparent, Color.White, (timers[a]-a/20f)*(1-TutorialManager.CinematicFade)));
            }
            spriteBatch.End();
        }
    }
}
