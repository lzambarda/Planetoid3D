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
    public class Button
    {
        public Button() { }
        public Button(Vector2 Position, string Text, int Width, Nullable<char> Shortcut)
        {
            position = Position;
            text = Text;
            width = Width;
            shortcut = Shortcut;
        }

        public int width;
        public Vector2 position;
        byte state;
        public string text;
        public Nullable<char> shortcut;
        public static Button selected;

        public void Update()
        {
            if (GameEngine.Game.IsActive == false)
            {
                return;
            }
            if (shortcut != null)
            {
                if (GameEngine.ks.IsKeyDown((Keys)(byte)shortcut) && GameEngine.pks.IsKeyUp((Keys)(byte)shortcut))
                {
                    state = 3;
                    AudioManager.Play("button_click");
                    return;
                }
            }
            if (GameEngine.ms.X >= position.X && GameEngine.ms.X < position.X + width && GameEngine.ms.Y >= position.Y && GameEngine.ms.Y < position.Y + 50)
            {
                //mouse is on the button
                if (GameEngine.ms.LeftButton == ButtonState.Pressed)
                {
                    if (GameEngine.pms.LeftButton == ButtonState.Released)
                    {
                        state = 2;
                        selected = this;
                        GameEngine.flag_mouseInhibition = true;
                    }
                }
                else if (state == 2 && selected == this)
                {
                    state = 3;
                    AudioManager.Play("button_click");
                    GameEngine.flag_mouseInhibition = true;
                }
                else if (state != 1)
                {
                    AudioManager.Play("button_hover");
                    state = 1;
                }
            }
            else if (selected == this && state == 2 && GameEngine.ms.LeftButton == ButtonState.Pressed)
            {
                state = 2;
            }
            else if (GameEngine.ms.LeftButton == ButtonState.Released)
            {
                state = 0;
            }
        }

        public bool IsClicked()
        {
            if (state == 3)
            {
                state = 0;
                return true;
            }
            return false;
        }

        public bool IsHovered
        {
            get { return state > 0; }
        }

        public void KeepBlocked()
        {
            state = 2;
        }

        public void Draw(SpriteFont font)
        {
            //Start
            PlanetoidGame.spriteBatch.Draw(HUDManager.panel_button, new Rectangle((int)position.X, (int)position.Y, 20, 50), new Rectangle(0, 0 + (50 * state), 20, 50), Color.White);
            //End
            PlanetoidGame.spriteBatch.Draw(HUDManager.panel_button, new Rectangle((int)position.X + width - 20, (int)position.Y, 20, 50), new Rectangle(180, 0 + (50 * state), 20, 50), Color.White);
            //Body
            PlanetoidGame.spriteBatch.Draw(HUDManager.panel_button, new Rectangle((int)position.X + 20, (int)position.Y, width - 40, 50), new Rectangle(20, 0 + (50 * state), 20, 50), Color.White);
            //Text
            if (text != "")
            {
                PlanetoidGame.spriteBatch.DrawString(font, text, position - font.MeasureString(text) / 2 + new Vector2(width / 2, 25), Util.PanelGray);

                //SHORTCUT
                if (shortcut != null)
                {
                    for (int a = 0; a < text.Length; a++)
                    {
                        if (char.ToUpper(text[a]) == shortcut)
                        {
                            PlanetoidGame.spriteBatch.DrawString(font, "_", position - font.MeasureString(text) / 2 + new Vector2(width / 2, 25) + font.MeasureString(text.Substring(0, a)), Util.PanelGray);
                            break;
                        }
                    }
                }
            }
            else
            {
                PlanetoidGame.spriteBatch.DrawString(font, shortcut.ToString(), position - font.MeasureString(shortcut.ToString()) / 2 + new Vector2(width / 2, 25), Util.PanelGray);
            }
        }

        public void Draw(SpriteFont font, Color color)
        {
            //Start
            PlanetoidGame.spriteBatch.Draw(HUDManager.panel_button, new Rectangle((int)position.X, (int)position.Y, 20, 50), new Rectangle(0, 0 + (50 * state), 20, 50), color);
            //End
            PlanetoidGame.spriteBatch.Draw(HUDManager.panel_button, new Rectangle((int)position.X + width - 20, (int)position.Y, 20, 50), new Rectangle(180, 0 + (50 * state), 20, 50), color);
            //Body
            PlanetoidGame.spriteBatch.Draw(HUDManager.panel_button, new Rectangle((int)position.X + 20, (int)position.Y, width - 40, 50), new Rectangle(20, 0 + (50 * state), 20, 50), color);
            //Text
            PlanetoidGame.spriteBatch.DrawString(font, text, position - font.MeasureString(text) / 2 + new Vector2(width / 2, 25), Util.PanelGray);
        }

        public void Draw(SpriteFont font, float completion)
        {
            completion = MathHelper.Clamp(completion, 0, 1);
            //Start
            PlanetoidGame.spriteBatch.Draw(HUDManager.panel_button, new Rectangle((int)position.X, (int)position.Y, 20, 50), new Rectangle(0, 0 + (50 * state), 20, 50), (completion > 0 ? Color.Lime : Color.White));
            //End
            PlanetoidGame.spriteBatch.Draw(HUDManager.panel_button, new Rectangle((int)position.X + width - 20, (int)position.Y, 20, 50), new Rectangle(180, 0 + (50 * state), 20, 50), (completion == 1 ? Color.Lime : Color.White));
            //Body
            PlanetoidGame.spriteBatch.Draw(HUDManager.panel_button, new Rectangle((int)position.X + 20, (int)position.Y, width - 40, 50), new Rectangle(20, 0 + (50 * state), 20, 50), Color.White);
            //Body Completion
            PlanetoidGame.spriteBatch.Draw(HUDManager.panel_button, new Rectangle((int)position.X + 20, (int)position.Y, (int)((width - 40) * completion), 50), new Rectangle(20, 0 + (50 * state), 20, 50), Color.Lime);
            //Text
            PlanetoidGame.spriteBatch.DrawString(font, text, position - font.MeasureString(text) / 2 + new Vector2(width / 2, 25), Util.PanelGray);
        }
    }
}
