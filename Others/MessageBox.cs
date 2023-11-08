using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Planetoid3D
{
    public enum MessageBoxResult
    {
        None,
        Ok,
        Cancel
    }
    public static class MessageBox
    {
        public static void Initialize(GameWindow window,GraphicsDeviceManager graphics, ContentManager Content)
        {
            texture = Content.Load<Texture2D>("messagebox_texture");
            position = new Vector2(graphics.PreferredBackBufferWidth / 2 - 251, graphics.PreferredBackBufferHeight / 2 - 88);
            accept = new Button(new Vector2(graphics.PreferredBackBufferWidth / 2 - 210, position.Y + 130), "Ok", 200, null);
            cancel = new Button(new Vector2(graphics.PreferredBackBufferWidth / 2 + 10, position.Y + 130), "Cancel", 200, null);
            atbutton = new Button(new Vector2(graphics.PreferredBackBufferWidth / 2 - 25, position.Y +130), "@", 50, null);
            
            text = "";
            input = "";
            title = "";

            /*InputSystem.Initialize(window);
            //Add an event for a character being added
            InputSystem.CharEntered += delegate(Object o, CharacterEventArgs e)
            {
                if (inputType == 1)
                {
                    if (e.ExtendedKey)
                    {
                    }
                    if (PlanetoidGame.textFont.Characters.Contains(e.Character))
                    {
                        if (PlanetoidGame.textFont.MeasureString(input).X < 300)
                        {
                            input += e.Character;
                        }
                    }
                    else if (e.Character == '\b')
                    {
                        if (input.Length > 0) { input = input.Remove(input.Length - 1); }
                    }
                }
            };*/
        }

        public static void AdaptPosition()
        {
            position = new Vector2(GameEngine.Game.GraphicsDevice.Viewport.Width / 2 - 251, GameEngine.Game.GraphicsDevice.Viewport.Height / 2 - 88);
            accept.position = new Vector2(GameEngine.Game.GraphicsDevice.Viewport.Width / 2 - 210, position.Y + 130);
            cancel.position = new Vector2(GameEngine.Game.GraphicsDevice.Viewport.Width / 2 + 10, position.Y + 130);
        }

        private static string text;
        private static string input;

        private static bool active;
        private static short inputType;
        private static string title;
        private static bool atsymbol;

        public static MessageBoxResult lastResult;

        private static Texture2D texture;

        public static bool IsActive
        {
            get { return active; }
        }

        public static string Title
        {
            get { return title; }
        }

        public static string Input
        {
            get { return input; }
            set { input = value; }
        }

        public static void Reset()
        {
            input = "";
            title = "";
            lastResult = MessageBoxResult.None;
        }

        public static void ByPass(string Title, MessageBoxResult Result)
        {
            title = Title;
            lastResult = Result;
        }

        private static Button accept;
        private static Button cancel;
        private static Button atbutton;
        private static Vector2 position;
        private static int pressTime;
        private static Keys currentKey;
        private static Keys lastKey;
        private static bool caps;

        public static void ShowDialog(string Title, string Text, short InputType)
        {
            AudioManager.Play("messagebox");
            title = Title;
            lastResult = MessageBoxResult.None;
            text = Text;
            inputType = InputType;
            active = true;

            if (Title.Contains("Login"))
            {
                if (atsymbol == false)
                {
                    atsymbol = true;
                    accept.position.X -= 20;
                    cancel.position.X += 20;
                }
            }
            else
            {
                if (atsymbol == true)
                {
                    atsymbol = false;
                    accept.position.X += 20;
                    cancel.position.X -= 20;
                }
            }
        }

        public static MessageBoxResult Update(SpriteFont font)
        {
            Keys[] keys = GameEngine.ks.GetPressedKeys();
            //MANAGE KEYBOARD INPUT
            switch (inputType)
            {
                case 1:
                    bool shifted = keys.Contains(Keys.LeftShift) || keys.Contains(Keys.RightShift);
                    if (keys.Length > 0)
                    {
                        currentKey = keys[0];
                        if (GameEngine.ks.IsKeyDown(currentKey))
                        {
                            if (char.IsLetterOrDigit((char)currentKey) || currentKey == Keys.Space || currentKey == Keys.Back)
                            {
                                pressTime++;
                            }
                            if (lastKey != currentKey)
                            {
                                pressTime = 0;
                            }
                            if (GameEngine.pks.IsKeyUp(currentKey) || pressTime > 30 || lastKey != currentKey)
                            {
                                switch (currentKey)
                                {
                                    case Keys.Back:
                                        if (input.Length > 0)
                                        {
                                            input = input.Remove(input.Length - 1);
                                        }
                                        break;
                                    case Keys.CapsLock:
                                        caps = !caps;
                                        break;
                                    case Keys.Space:
                                        if (font.MeasureString(input).X < 300)
                                        {
                                            input += " ";
                                        }
                                        break;
                                    case Keys.A:
                                    case Keys.B:
                                    case Keys.C:
                                    case Keys.D:
                                    case Keys.E:
                                    case Keys.F:
                                    case Keys.G:
                                    case Keys.H:
                                    case Keys.I:
                                    case Keys.J:
                                    case Keys.K:
                                    case Keys.L:
                                    case Keys.M:
                                    case Keys.N:
                                    case Keys.O:
                                    case Keys.P:
                                    case Keys.Q:
                                    case Keys.R:
                                    case Keys.S:
                                    case Keys.T:
                                    case Keys.U:
                                    case Keys.V:
                                    case Keys.W:
                                    case Keys.X:
                                    case Keys.Y:
                                    case Keys.Z:
                                        if (font.MeasureString(input).X < 300)
                                        {
                                            if (shifted || caps)
                                            {
                                                input += (char)currentKey;
                                            }
                                            else
                                            {
                                                input += char.ToLower((char)currentKey);
                                            }
                                        }
                                        break;
                                    case Keys.D0:
                                        if (font.MeasureString(input).X < 300)
                                        {
                                            input += (shifted ? ")" : "0");
                                        }
                                        break;
                                    case Keys.D1:
                                        if (font.MeasureString(input).X < 300)
                                        {
                                            input += (shifted ? "!" : "1");
                                        }
                                        break;
                                    case Keys.D2:
                                        if (font.MeasureString(input).X < 300)
                                        {
                                            input += (shifted ? "@" : "2");
                                        }
                                        break;
                                    case Keys.D3:
                                        if (font.MeasureString(input).X < 300)
                                        {
                                            input += (shifted ? "#" : "3");
                                        }
                                        break;
                                    case Keys.D4:
                                        if (font.MeasureString(input).X < 300)
                                        {
                                            input += (shifted ? "$" : "4");
                                        }
                                        break;
                                    case Keys.D5:
                                        if (font.MeasureString(input).X < 300)
                                        {
                                            input += (shifted ? "%" : "5");
                                        }
                                        break;
                                    case Keys.D6:
                                        if (font.MeasureString(input).X < 300)
                                        {
                                            input += (shifted ? "&" : "6");
                                        }
                                        break;
                                    case Keys.D7:
                                        if (font.MeasureString(input).X < 300)
                                        {
                                            input += (shifted ? "/" : "7");
                                        }
                                        break;
                                    case Keys.D8:
                                        if (font.MeasureString(input).X < 300)
                                        {
                                            input += (shifted ? "*" : "8");
                                        }
                                        break;
                                    case Keys.D9:
                                        if (font.MeasureString(input).X < 300)
                                        {
                                            input += (shifted ? "(" : "9");
                                        }
                                        break;
                                    case Keys.OemPeriod:
                                        if (font.MeasureString(input).X < 300)
                                        {
                                            input += (shifted ? ":" : ".");
                                        }
                                        break;
                                    case Keys.OemMinus:
                                        if (font.MeasureString(input).X < 300)
                                        {
                                            input += (shifted ? "_" : "-");
                                        }
                                        break;

                                }
                            }
                        }
                        else
                        {
                            pressTime = 0;
                        }
                    }
                    else
                    {
                        currentKey = Keys.None;
                        pressTime = 0;
                    }
                    lastKey = currentKey;
                    break;
                case 2:
                    if (keys.Length == 1 && char.IsLetter((char)keys[0]))
                    {
                        input = keys[0].ToString();
                        lastResult = MessageBoxResult.Ok;
                        active = false;
                        return MessageBoxResult.Ok;
                    }
                    break;
            }

            //BUTTONS UPDATE
            if (inputType < 2)
            {
                accept.Update();
                if (accept.IsClicked())
                {
                    active = false;
                    lastResult = MessageBoxResult.Ok;
                    return MessageBoxResult.Ok;
                }

                cancel.Update();
                if (cancel.IsClicked())
                {
                    active = false;
                    lastResult = MessageBoxResult.Cancel;
                    return MessageBoxResult.Cancel;
                }

                if (atsymbol)
                {
                    atbutton.Update();
                    if (atbutton.IsClicked())
                    {
                        if (font.MeasureString(input).X < 300)
                        {
                            input += "@";
                        }
                    }
                }
            }
            lastResult = MessageBoxResult.None;
            return MessageBoxResult.None;
        }

        public static void Draw(SpriteFont font)
        {
            PlanetoidGame.spriteBatch.Draw(texture, position, Color.White);
            PlanetoidGame.spriteBatch.DrawString(font, title, position + new Vector2(8), Util.PanelGray);
            Util.DrawCenteredText(font, text + " " +(title.Equals("Login Password")?new string('*', input.Length):input) + (inputType == 1 ? "_" : ""), position + new Vector2(texture.Width / 2, texture.Height / 2 - 25), Util.PanelGray);
            if (inputType < 2)
            {
                accept.Draw(font, Color.LawnGreen);
                cancel.Draw(font, Color.OrangeRed);
                if (atsymbol)
                {
                    atbutton.Draw(font, Color.Gold);
                }
            }
        }
    }
}
