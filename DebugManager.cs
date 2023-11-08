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
    public static class DebugManager
    {
        private static double[] ums;
        private static int ums_index;
        private static double umsMean;

        private static double[] dms;
        private static int dms_index;
        private static double dmsMean;

        private static Game Game;

        private static double elapsed;

        private static Texture2D dot;

        private static Vector2 startingPoint;

        public static void Initialize(Game _game)
        {
            Game = _game;
            ums = new double[250];
            ums_index = 1;
            dms = new double[250];
            dms_index = 1;

            dot = Game.Content.Load<Texture2D>("dot");

            startingPoint = new Vector2(Game.GraphicsDevice.Viewport.Width - 264, Game.GraphicsDevice.Viewport.Height - 100);
        }

        public static void StartCounting()
        {
            elapsed = DateTime.Now.TimeOfDay.TotalMilliseconds;

        }

        public static void EndCountingUpdate()
        {
            for (int a = ums_index - 1; a > 0; a--)
            {
                ums[a] = ums[a - 1];
            }
            if (ums_index < ums.Length)
            {
                ums_index++;
            }
            ums[0] = (DateTime.Now.TimeOfDay.TotalMilliseconds - elapsed);
        }

        public static void EndCountingDraw()
        {
            for (int a = dms_index - 1; a > 0; a--)
            {
                dms[a] = dms[a - 1];
            }
            if (dms_index < dms.Length)
            {
                dms_index++;
            }
            dms[0] = (DateTime.Now.TimeOfDay.TotalMilliseconds - elapsed);
        }

        /// <summary>
        /// Get the total milliseconds used by the function
        /// </summary>
        public static double UsedTimeByFunction(Func<object> function)
        {
            double usedTime = DateTime.Now.TimeOfDay.TotalMilliseconds;

            function.Invoke();

            return DateTime.Now.TimeOfDay.TotalMilliseconds - usedTime;
        }

        /// <summary>
        /// Draw the textboard (update is included)
        /// </summary>
        public static void Draw(SpriteFont font)
        {
            umsMean = 0;
            for (int a = 0; a < ums_index; a++)
            {
                PlanetoidGame.spriteBatch.Draw(dot, startingPoint + new Vector2(10 + a / 2f, -(float)ums[a]/10), Util.PanelLightGray);
                umsMean += ums[a];
            }
            umsMean /= ums_index;
            umsMean = Math.Round(umsMean, 6);
            dmsMean = 0;
            for (int a = 0; a < dms_index; a++)
            {
                PlanetoidGame.spriteBatch.Draw(dot, startingPoint + new Vector2(10 + a / 2f, 40 - (float)dms[a]/10), Util.PanelLightGray);
                dmsMean += dms[a];
            }
            dmsMean /= dms_index;
            dmsMean = Math.Round(dmsMean, 6);
            PlanetoidGame.spriteBatch.DrawString(font, "Update: "+umsMean+" avg ms\n\nDraw: "+dmsMean+" avg ms\nMemory: " + GC.GetTotalMemory(false) / 1024 + " kb", startingPoint, Util.PanelLightGray);
        }
    }
}
