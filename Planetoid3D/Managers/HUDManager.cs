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
    /// <summary>
    /// Manager for the Head Up Display
    /// </summary>
    public static class HUDManager
    {
        public static void Initialize()
        {
            //Panels
            panel_planet = GameEngine.Game.Content.Load<Texture2D>("Panels//panel_planet");
            panel_icon = GameEngine.Game.Content.Load<Texture2D>("Panels//panel_icon");
            panel_icons = new Texture2D[4];
            panel_icons[0] = GameEngine.Game.Content.Load<Texture2D>("Panels//panel_icon_inhabit");
            panel_icons[1] = GameEngine.Game.Content.Load<Texture2D>("Panels//panel_icon_home");
            panel_icons[2] = GameEngine.Game.Content.Load<Texture2D>("Panels//panel_icon_friend");
            panel_icons[3] = GameEngine.Game.Content.Load<Texture2D>("Panels//panel_icon_enemy");
            panel_bottom = GameEngine.Game.Content.Load<Texture2D>("Panels//panel_bottom");
            panel_keldanyum = GameEngine.Game.Content.Load<Texture2D>("Panels//panel_keldanyum");
            panel_energy = GameEngine.Game.Content.Load<Texture2D>("Panels//panel_energy");
            panel_info = GameEngine.Game.Content.Load<Texture2D>("Panels//panel_info");
            panel_object = GameEngine.Game.Content.Load<Texture2D>("Panels//panel_object");
            panel_button = GameEngine.Game.Content.Load<Texture2D>("Panels//panel_button");
            panel_deal = GameEngine.Game.Content.Load<Texture2D>("Panels//panel_deal");

            button_firstAction = new Button(new Vector2(390, GameEngine.Game.GraphicsDevice.Viewport.Height - 180), "Sell", 200, null);
            button_secondAction = new Button(new Vector2(390, GameEngine.Game.GraphicsDevice.Viewport.Height - 230), "", 200, null);

            button_readMessage = new Button(new Vector2(390, GameEngine.Game.GraphicsDevice.Viewport.Height - 116), "", 200, null);
        }

        //Variables
        //the panels are for the planets informations
        private static Texture2D panel_planet;
        private static Texture2D panel_icon;
        public static Texture2D[] panel_icons;
        //the main panel for the gameplay
        //on this panel will appear keldanyum, energy, all kind of ingame informations and building menu
        private static Texture2D panel_bottom;
        private static Texture2D panel_keldanyum;
        private static Texture2D panel_energy;
        //the panel for the building informations, such as resources cost and so on
        public static Texture2D panel_info;
        //this panel will contain all informations about selected object
        private static Texture2D panel_object;
        //this is the panel for the deal management
        public static Texture2D panel_deal;
        //this texture contains three button state images, normal, mouse on, clicked
        public static Texture2D panel_button;

        //Buttons
        public static Button button_build;
        public static Button button_names;
        public static Button button_diplomacy;
        public static Button button_strategy;

        public static Button button_readMessage;
        private static Button button_firstAction;
        private static Button button_secondAction;

        //Player Variables
        public static bool buildingMode;
        public static bool diplomacyMode;
        public static bool strategyMode;
        public static bool displayNames;
        public static bool spaceshipSelected;

        public static float prevKeldanym;
        public static float prevEnergy;

        public static BaseObject lastTargetObject;
        private static BaseObject temp;
        private static Planet temporaryPlanet;

        public static float message_timer;

        public static bool showDebug = true;

        /// <summary>
        /// Update the game HUD, like object information, planets panels etc
        /// </summary>
        public static void Update()
        {
            if (GameEngine.gameMode == GameMode.Tutorial && TutorialManager.CinematicMode)
            {
                return;
            }
            //Reset mouse inhibition flag
            //if (GameEngine.ms.LeftButton == ButtonState.Released)
            //{
            GameEngine.flag_mouseInhibition = false;
            //}
            //Check for the lastTargetObject disappearance
            if (lastTargetObject != null && lastTargetObject.life <= 0)
            {
                spaceshipSelected = false;
                lastTargetObject = null;
            }
            //Start HUD logic
            if (!spaceshipSelected)
            {
                if (!buildingMode)
                {
                    if (!diplomacyMode)
                    {
                        //Administrate Buttons in the object menu
                        if (lastTargetObject != null)
                        {
                            //Mouse inside the object information panel
                            if (GameEngine.ms.X < 360 && GameEngine.ms.Y > GameEngine.Game.GraphicsDevice.Viewport.Height - 270 && GameEngine.ms.Y < GameEngine.Game.GraphicsDevice.Viewport.Height - 130)
                            {
                                GameEngine.flag_mouseInhibition = true;
                            }
                            //Mouse inside the base panel
                            else if (GameEngine.ms.X < 604 && GameEngine.ms.Y > GameEngine.Game.GraphicsDevice.Viewport.Height - 130 && GameEngine.ms.Y < GameEngine.Game.GraphicsDevice.Viewport.Height)
                            {
                                GameEngine.flag_mouseInhibition = true;
                            }
                            //Mouse on the sell button
                            else if (lastTargetObject is BaseObjectBuilding && GameEngine.ms.X > 385 && GameEngine.ms.X < 595 && GameEngine.ms.Y > GameEngine.Game.GraphicsDevice.Viewport.Height - 175 && GameEngine.ms.Y < GameEngine.Game.GraphicsDevice.Viewport.Height - 125)
                            {
                                GameEngine.flag_mouseInhibition = true;
                            }
                            else if (lastTargetObject is BaseObjectBuilding && (lastTargetObject as BaseObjectBuilding).SecondHUDLabel != "")
                            {
                                //Mouse on the action button
                                if (GameEngine.ms.X > 385 && GameEngine.ms.X < 595 && GameEngine.ms.Y > GameEngine.Game.GraphicsDevice.Viewport.Height - 225 && GameEngine.ms.Y < GameEngine.Game.GraphicsDevice.Viewport.Height - 175)
                                {
                                    GameEngine.flag_mouseInhibition = true;
                                }
                            }
                            //A BUILDING IS SELECTED                       
                            if (lastTargetObject is BaseObjectBuilding)
                            {
                                button_firstAction.Update();
                                button_firstAction.text = (lastTargetObject as BaseObjectBuilding).FirstHUDLabel;
                                button_secondAction.text = (lastTargetObject as BaseObjectBuilding).SecondHUDLabel;

                                if (button_firstAction.IsClicked())
                                {
                                    ((BaseObjectBuilding)lastTargetObject).DoFirstHUDAction();
                                    GameEngine.flag_mouseInhibition = true;
                                }
                                else if ((lastTargetObject as BaseObjectBuilding).SecondHUDLabel != "")
                                {
                                    button_secondAction.Update();
                                    if (button_secondAction.IsClicked())
                                    {
                                        (lastTargetObject as BaseObjectBuilding).DoSecondHUDAction();
                                        GameEngine.flag_mouseInhibition = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draw HUD and interface elements
        /// </summary>
        public static void Draw(SpriteFont font)
        {
            //Draw Planets Info Panels
            if (displayNames == true)
            {
                PlanetoidGame.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                if (GameEngine.ms.MiddleButton != ButtonState.Pressed)
                {
                    temp = GameEngine.gameCamera.GetTargetObject(typeof(Planet));
                }

                int index;
                int race;
                Vector2 position;

                for (int a = 0; a < GameEngine.planets.Count; a++)
                {
                    temporaryPlanet = GameEngine.planets[a];
                    if (temporaryPlanet.life > 0 && temporaryPlanet.name != "Sun")
                    {
                        //Get transformed position
                        temporaryPlanet.transformed_position = GameEngine.Game.GraphicsDevice.Viewport.Project(
                               temporaryPlanet.matrix.Translation,
                                GameEngine.gameCamera.projectionMatrix,
                                GameEngine.gameCamera.viewMatrix,
                                Matrix.Identity);

                        if (temporaryPlanet.transformed_position.Z < 1)
                        {
                            temporaryPlanet.transformed_position.Z /= 100;
                            if (temporaryPlanet == temp)
                            {
                                temporaryPlanet.transformed_position.Z = 0.001f;
                            }
                            else
                            {
                                temporaryPlanet.transformed_position.Z += a / 500f;
                            }
                            position = new Vector2(temporaryPlanet.transformed_position.X, temporaryPlanet.transformed_position.Y);

                            PlanetoidGame.spriteBatch.Draw(panel_icon, position, null, temporaryPlanet.GetColor(), 0, new Vector2(50), 1, SpriteEffects.None, temporaryPlanet.transformed_position.Z);
                            PlanetoidGame.spriteBatch.Draw(panel_planet, position, null, temporaryPlanet.GetColor(), 0, Vector2.Zero, 1, SpriteEffects.None, temporaryPlanet.transformed_position.Z - 0.0001f);

                            race = temporaryPlanet.DominatingRace();
                            index = 0;

                            if (race > -1)
                            {
                                if (race == PlayerManager.GetRace(0))
                                {
                                    index = 1;
                                }
                                else if (PlayerManager.GetFriendship(PlayerManager.GetRaceOwner(race), 0) >= 0.5f && PlayerManager.GetTrust(PlayerManager.GetRaceOwner(race), 0) > 0.25f)
                                {
                                    index = 2;
                                }
                                else
                                {
                                    index = 3;
                                }
                            }

                            PlanetoidGame.spriteBatch.Draw(panel_icons[index], position, null, temporaryPlanet.GetColor(), 0, new Vector2(50), 1, SpriteEffects.None, temporaryPlanet.transformed_position.Z - 0.0002f);

                           /* string stat = "";
                            if (race != -1)
                            {
                                if (PlayerManager.players[PlayerManager.GetRaceOwner(race)].cpuController != null)
                                {
                                    PlanetInterface p = PlayerManager.players[PlayerManager.GetRaceOwner(race)].cpuController.GetInterface(temporaryPlanet);
                                    if (p != null)
                                    {
                                        stat = "\nInterface: {danger: " + p.danger + " ,action: " + p.action + "}";
                                    }
                                }
                            }*/


                            PlanetoidGame.spriteBatch.DrawString(font,
                                "Name: " + temporaryPlanet.name +
                                "\nLife: " + Math.Round(temporaryPlanet.life, 1) +
                                "\nPopulation: " + temporaryPlanet.TotalPopulation() + "/" + temporaryPlanet.maxPopulation +
                                "\nAtmosphere: " + temporaryPlanet.atmosphere + (temporaryPlanet.atmosphere != Atmosphere.None ? " " + temporaryPlanet.atmosphere_level + "/100" : "") +
                                "\nAvailable Keldanyum: "+(int)temporaryPlanet.available_keldanyum+
                                "\nDiameter: " + temporaryPlanet.radius * 2 +
                                /*"\nDistance: " + (temporaryPlanet.planet == null ? "ND" : "" + temporaryPlanet.distance) +*/
                                "\nRace: " + (race == -1 ? "Inhabitated" : RaceManager.GetRace(race))/*+stat*/
                                , position, Color.White, 0, new Vector2(-25), 1, SpriteEffects.None, temporaryPlanet.transformed_position.Z - 0.0003f);
                        }
                    }
                }
                PlanetoidGame.spriteBatch.End();
            }
            PlanetoidGame.spriteBatch.Begin();
            if (displayNames == false)
            {
                for (int a = 0; a < GameEngine.balloons.Count; a++)
                {
                    GameEngine.balloons[a].Draw(font);
                }
            }


            BuildingManager.DrawResearchList(font);

            //Draw Building Menu
            if (buildingMode)
            {
                BuildingManager.DrawBuilingMenu();
                if (BuildingManager.menuVoice.cost.Y != -1)
                {
                    string text = "|P|" + BuildingManager.menuVoice.title + ":\n" +
                         BuildingManager.menuVoice.description +
                         (BuildingManager.menuVoice.cost.X > 0 ? "\nCost:\n  " +
                         (BuildingManager.menuVoice.cost.X > PlayerManager.GetKeldanyum(0) ? "|R|" : "|G|") + BuildingManager.menuVoice.cost.X + " Keldanyum\n  " +
                         (BuildingManager.menuVoice.cost.Y > PlayerManager.GetEnergy(0) ? "|R|" : "|G|") + BuildingManager.menuVoice.cost.Y + " Energy" : "") +
                         (BuildingManager.menuVoice.seconds > 0 ? "\n|P|Time Needed: " +
                         BuildingManager.menuVoice.seconds + " sec" : "");

                    if (BuildingManager.menu == 2 && BuildingManager.lastVoice > 0 && BuildingManager.ResearchLevels(0)[BuildingManager.lastVoice - 1] == 5)
                    {
                        text = "|P|" + BuildingManager.menuVoice.title + ":\n" + BuildingManager.menuVoice.description + "\nMAXED";
                    }
                    if (BuildingManager.menuVoice.researchLevels != null)
                    {
                        text += "\nNeeded:";

                        for (int a = 0; a < BuildingManager.menuVoice.researchLevels.Length; a++)
                        {
                            if (BuildingManager.menuVoice.researchLevels[a] >= PlayerManager.players[0].researchLevels[a])
                            {
                                text += "\n|R| - " + BuildingManager.researches[a].title + " level " + BuildingManager.menuVoice.researchLevels[a];
                            }

                        }
                    }
                    if (text.Last() == ':')
                    {
                        text += "\n|G|Nothing :)";
                    }

                    Util.DrawAdaptablePanel(font, text, new Vector2(GameEngine.Game.GraphicsDevice.Viewport.Width - 324, GameEngine.Game.GraphicsDevice.Viewport.Height), BuildingManager.panelFade);
                }
            }
            //Draw Player Panels
            PlanetoidGame.spriteBatch.Draw(panel_bottom, new Vector2(0, GameEngine.Game.GraphicsDevice.Viewport.Height - 130), Color.White);
            if (PlanetoidGame.details > 0)
            {
                int width = (int)(211 * MathHelper.Clamp(PlayerManager.GetKeldanyum(0) / 3000, 0, 1));
                PlanetoidGame.spriteBatch.Draw(panel_keldanyum, new Rectangle(11, GameEngine.Game.GraphicsDevice.Viewport.Height - 103, width, 29), new Rectangle(0, 0, width, 29), Color.White);
                width = (int)(136 * MathHelper.Clamp(PlayerManager.GetEnergy(0) / 3000, 0, 1));
                PlanetoidGame.spriteBatch.Draw(panel_energy, new Rectangle(87, GameEngine.Game.GraphicsDevice.Viewport.Height - 52, width, 25), new Rectangle(0, 0, width, 25), Color.White);
            }
            int ks = (int)Math.Sign(PlayerManager.GetKeldanyum(0) - prevKeldanym);
            int es = (int)Math.Sign(PlayerManager.GetEnergy(0) - prevEnergy);
            //ingame menu text
            Util.DrawColoredString(font,
                (ks == 0 ? "|g|" : (ks == 1 ? "|G|" : "|R|")) + (int)PlayerManager.GetKeldanyum(0) +
                "\n \n" + (es == 0 ? "|g|" : (es == 1 ? "|G|" : "|R|")) + (int)PlayerManager.GetEnergy(0)
                , new Vector2(240, GameEngine.Game.GraphicsDevice.Viewport.Height - 100), 1);

            //Draw buttons
            button_readMessage.Draw(font);

            button_names.Draw(font);
            button_build.Draw(font);
            button_diplomacy.Draw(font);
            button_strategy.Draw(font);


            //Draw object panel
            //SELECTION HUD
            if (lastTargetObject != null)
            {
                if (lastTargetObject is Planet == false && lastTargetObject is Planetoid == false && lastTargetObject is BlackHole == false)
                {
                    PlanetoidGame.spriteBatch.Draw(panel_object, new Vector2(0, GameEngine.Game.GraphicsDevice.Viewport.Height - 270), Color.White);

                    string text = "";
                    if (lastTargetObject is BaseObjectBuilding)
                    {
                        button_firstAction.Draw(font);
                        text = (lastTargetObject as BaseObjectBuilding).type + "\n";
                        if ((lastTargetObject as BaseObjectBuilding).SecondHUDLabel != "")
                        {
                            button_secondAction.Draw(font);
                        }
                    }

                    //Draw object infos
                    PlanetoidGame.spriteBatch.DrawString(font,
                        text + lastTargetObject.GetHudText(),
                        new Vector2(10, GameEngine.Game.GraphicsDevice.Viewport.Height - 250), Util.PanelGray);
                }
            }

            PlanetoidGame.spriteBatch.End();
            //Diplomacy draw
            if (diplomacyMode)
            {
                DiplomacyManager.Draw(font);
            }
        }


        /// <summary>
        /// Draw debugging text, like FPS, version and other useful stuff
        /// </summary>
        public static void DebugText(SpriteFont font)
        {
            //Debug Yext
            Util.DrawColoredString(font, "|W|v " + PlanetoidGame.GAME_VERSION + "\n" + (GameEngine.FPS < 30 ? "|R|" : (GameEngine.FPS < 45 ? "|Y|" : "|G|")) + "FPS: " + GameEngine.FPS + "/60\n|W|Press F2 to toggle debug information", new Vector2(10, 10), 1);
            DebugManager.Draw(font);
        }
    }
}
