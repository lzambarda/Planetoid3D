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

using System.Xml;
using System.IO;
using System.Net;
using System.Text;

namespace Planetoid3D {
    public enum Resolution {
        r1024x768,
        r1280x1024,
        r1366x768,
        r1440x960,
        Auto
    }

    public enum Orbit {
        No,
        Soft,
        Normal,
        Strong
    }

    /// <summary>
    /// Menu Manager, last review on version 1.0.2
    /// </summary>
    public static class MenuManager {
        //"Constructor"
        public static void Initialize(GameWindow window, GraphicsDeviceManager Graphics) {
            graphics = Graphics;

            //Initialize external classes
            MessageBox.Initialize(window, graphics, GameEngine.Game.Content);

            #region Create Files
            //READ GAME SETTINGS
            document = new XmlDocument();


            if (!Directory.Exists(GetBasePath())) {
                Directory.CreateDirectory(GetBasePath());
            }

            if (!Directory.Exists(GetSavePath(""))) {
                Directory.CreateDirectory(GetSavePath(""));
            }

            try {
                if (!Directory.Exists(GetBasePath() + "//CustomRaces")) {
                    Directory.CreateDirectory(GetBasePath() + "//CustomRaces");
                    StreamWriter writer = new StreamWriter(GetBasePath() + "//CustomRaces//readme.txt");
                    writer.WriteLine("In this folder you can add all the races you want!");
                    writer.WriteLine("Just follow these easy steps:");
                    writer.WriteLine(" 1 - Create a folder with the name of your race.");
                    writer.WriteLine(" 2 - Enter in the folder..");
                    writer.WriteLine(" 3 - Create a file named 'data.xml' and write whatever you want, remember to follow the instructions!");
                    writer.WriteLine(" 4 - Paint your race and save the image as 'texture.png'.");
                    writer.WriteLine("We've already put an editable race as example!");
                    writer.Close();
                    Directory.CreateDirectory(GetBasePath() + "//CustomRaces//Custom");
                    writer = new StreamWriter(GetBasePath() + "//CustomRaces//Custom//data.xml");
                    writer.WriteLine("<?xml version=\"1.0\" encoding=\"ISO-8859-15\"?>");
                    writer.WriteLine("<Race>\n<!--\nInsert one of the following Atmoshperes:\nOxygen\nMetane\nSulphur\nCyanide\nAmmonia\nChlorine\n-->");
                    writer.WriteLine("<Atmosphere>Oxygen</Atmosphere>\n<!--\nInsert a number from 0 to 13\n-->\n<Ability>0</Ability>");
                    writer.WriteLine("<!--\nWrite whatever you want about your race!\n-->");
                    writer.WriteLine("<Description>This race is a custom race, read 'C:\\Users\\Public\\Documents//Planetoid 3D//CustomRaces//readme.txt' for more information!</Description>\n</Race>");
                    writer.Close();
                    GameEngine.Game.Content.Load<Texture2D>("Races//Custom").SaveAsPng(File.Create(GetBasePath() + "//CustomRaces//Custom//texture.png"), 96, 128);
                }
            } catch (IOException) { }

            int option = 0;
            if (!File.Exists(GetSettingPath())) {
                //CREATE THE FILE
                option = 1;
            } else {
                //IF FILE CORRUPTED, RECREATE IT
                try {
                    DataManager.Decrypt(GetSettingPath());
                    document.Load(GetSettingPath());
                    DataManager.Encrypt(GetSettingPath());
                } catch (XmlException) {
                    bool lost = false;
                    if (!File.Exists(GetSettingPath() + "~1")) {
                        lost = true;
                    }
                    {
                        try {
                            DataManager.Decrypt(GetSettingPath() + "~1");
                            document.Load(GetSettingPath() + "~1");
                            DataManager.Encrypt(GetSettingPath() + "~1");
                            File.Copy(GetSettingPath() + "~1", GetSettingPath(), true);
                        } catch (XmlException) {
                            lost = true;
                        }
                    }
                    if (lost) {
                        option = 2;
                        File.Delete(GetSettingPath());
                        MessageBox.ShowDialog("Oh no!", "The settings file seems to be corrupted!\nI'll try to fix it!", 0);
                    }
                }
            }

            //IF THE SETTINGS FILE DOESN'T EXISTS CREATE IT WITH DEFAULT SETTINGS
            if (option > 0) {
                XmlTextWriter textWriter = new XmlTextWriter(GetSettingPath(), null);
                //File.SetAttributes(GetSettingPath(), FileAttributes.Hidden);
                textWriter.WriteStartDocument();
                textWriter.WriteStartElement("Settings");

                textWriter.WriteStartElement("Music");
                textWriter.WriteValue(10);
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("Sound");
                textWriter.WriteValue(10);
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("Fullscreen");
                textWriter.WriteValue(false);
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("Details");
                textWriter.WriteValue(1);
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("Cameye");
                textWriter.WriteValue(false);
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("Lens");
                textWriter.WriteValue(false);
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("ShortNames");
                textWriter.WriteValue(Keys.X.ToString());
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("ShortBuild");
                textWriter.WriteValue(Keys.B.ToString());
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("ShortDiplomacy");
                textWriter.WriteValue(Keys.D.ToString());
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("ShortStrategy");
                textWriter.WriteValue(Keys.S.ToString());
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("Enabled");
                textWriter.WriteValue((option == 2 ? "keyLost!" : "false"));
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("Resolution");
                textWriter.WriteValue(0);
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("Orbits");
                textWriter.WriteValue(1);
                textWriter.WriteEndElement();


                textWriter.WriteStartElement("Quests");
                for (int a = 0; a < 20; a++) {
                    textWriter.WriteStartElement("Quest");
                    textWriter.WriteValue(0);
                    textWriter.WriteEndElement();
                }
                textWriter.WriteEndElement();
                textWriter.WriteEndDocument();
                textWriter.Close();
                DataManager.Encrypt(GetSettingPath());
            }
            #endregion

            DataManager.Decrypt(GetSettingPath());
            //LOAD AND READ
            document.Load(GetSettingPath());
            //music volume
            AudioManager.music_volume = int.Parse(document.GetElementsByTagName("Music")[0].FirstChild.Value);
            //sound volume
            AudioManager.sound_volume = int.Parse(document.GetElementsByTagName("Sound")[0].FirstChild.Value);
            //fullscreen
            graphics.IsFullScreen = bool.Parse(document.GetElementsByTagName("Fullscreen")[0].FirstChild.Value);
            graphics.ApplyChanges();
            //details
            PlanetoidGame.details = short.Parse(document.GetElementsByTagName("Details")[0].FirstChild.Value);
            Util.InitializeVertices();
            //dynamic camera
            GameEngine.gameCamera.eyeCorner = bool.Parse(document.GetElementsByTagName("Cameye")[0].FirstChild.Value);
            //Lens Flare
            PlanetoidGame.lens.Enabled = bool.Parse(document.GetElementsByTagName("Lens")[0].FirstChild.Value);
            //Get the resolution, only at the beginning of the game
            currentResolution = (Resolution)int.Parse(document.GetElementsByTagName("Resolution")[0].FirstChild.Value);
            //Get the orbits setting
            currentOrbit = (Orbit)int.Parse(document.GetElementsByTagName("Orbits")[0].FirstChild.Value);

            SetViewport();

            //Shortcuts
            HUDManager.button_names = new Button(new Vector2(390, GameEngine.Game.GraphicsDevice.Viewport.Height - 60), "", 50, document.GetElementsByTagName("ShortNames")[0].FirstChild.Value[0]);
            HUDManager.button_build = new Button(new Vector2(440, GameEngine.Game.GraphicsDevice.Viewport.Height - 60), "", 50, document.GetElementsByTagName("ShortBuild")[0].FirstChild.Value[0]);
            HUDManager.button_diplomacy = new Button(new Vector2(490, GameEngine.Game.GraphicsDevice.Viewport.Height - 60), "", 50, document.GetElementsByTagName("ShortDiplomacy")[0].FirstChild.Value[0]);
            HUDManager.button_strategy = new Button(new Vector2(540, GameEngine.Game.GraphicsDevice.Viewport.Height - 60), "", 50, document.GetElementsByTagName("ShortStrategy")[0].FirstChild.Value[0]);

            /* if (document.GetElementsByTagName("Enabled")[0].FirstChild.Value.Equals("false") || !document.GetElementsByTagName("Identifier")[0].FirstChild.Value.Equals(GetProcessorId()))
             {
                 PlanetoidGame.game_enabled = false;
                 //Check online if there's a code with my CPU identifier
                 /*try
                 {
                     WebClient objWebClient = new WebClient();
                     System.Collections.Specialized.NameValueCollection values = new System.Collections.Specialized.NameValueCollection();
                     values.Add("identifier", MenuManager.GetProcessorId());
                     byte[] bytes = objWebClient.UploadValues("http://www.metalsoulstudios.net/reenableGame.php", "POST", values);
                     if (bytes.Length > 0 && (char)bytes[0] != '#')
                     {
                         document.GetElementsByTagName("Enabled")[0].FirstChild.Value = System.Text.UTF8Encoding.UTF8.GetString(bytes);
                         document.GetElementsByTagName("Identifier")[0].FirstChild.Value = GetProcessorId();
                         PlanetoidGame.game_enabled = true;
                     }
                 }
                 catch (WebException e)
                 {

                 }*/
            /* }
             else
             {
                 PlanetoidGame.game_enabled = true;
             }
             */

            AudioManager.ChangeVolume("Music", AudioManager.music_volume);
            AudioManager.ChangeVolume("Sound", AudioManager.sound_volume);

            //Add the achievement in the manager
            int count = 0;
            foreach (XmlNode achievement in document.GetElementsByTagName("Quests")[0].ChildNodes) {
                QuestManager.SetQuest(
                    count,
                    int.Parse(achievement.InnerText)
                    );
                count++;
            }
            DataManager.Encrypt(GetSettingPath());

            currentPanel = 0;//main
            nextPanel = 0;

            //Initialize stuff for the main scene
            actor = new Sun {
                matrix = Matrix.Identity,
                spinSpeed = 0.05f,
                radius = 150,
                atmosphere_level = 100
            };
            camera = new Camera(new Vector3(600, 0, 0));

            //Initialze camera and hominid for the lobby preview
            lobby_actor = new Hominid();
            lobby_camera = new Camera(new Vector3(0, 20, -50));

            back = new RenderTarget2D(graphics.GraphicsDevice, Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
            main_panel = new RenderTarget2D(graphics.GraphicsDevice, Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
            control_panel = new RenderTarget2D(graphics.GraphicsDevice, Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
            option_panel = new RenderTarget2D(graphics.GraphicsDevice, Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
            lobby_panel = new RenderTarget2D(graphics.GraphicsDevice, Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);

            infoFade = 0;
            infoText = "";

            //Initialize main buttons
            main_newgame = new Button(new Vector2(back.Width - 300, back.Height / 2 - 180), "Play", 200, 'P');
            main_continue = new Button(new Vector2(back.Width - 300, back.Height / 2 - 120), "Continue", 200, 'C');
            main_load = new Button(new Vector2(back.Width - 300, back.Height / 2 - 60), "Load", 200, 'L');
            main_options = new Button(new Vector2(back.Width - 300, back.Height / 2), "Options", 200, 'O');
            main_quests = new Button(new Vector2(back.Width - 300, back.Height / 2 + 60), "Quests", 200, null);
            main_extra = new Button(new Vector2(back.Width - 300, back.Height / 2 + 120), "Extra", 200, 'E');
            main_quit = new Button(new Vector2(back.Width - 300, back.Height / 2 + 180), "Quit", 200, 'Q');

            main_questList = new Button[10];
            for (int a = 0; a < main_questList.Length; a++) {
                main_questList[a] = new Button(new Vector2(20, 0), "", 450, null);
            }

            //Initialize control buttons
            load_slots = new List<Button>();
            load_slotsDetails = new List<FileInfo>();
            load_load = new Button(new Vector2(back.Width - 220, back.Height - 200), "Load", 200, 'L');
            load_delete = new Button(new Vector2(back.Width - 220, back.Height - 140), "Delete", 200, 'D');
            all_back = new Button(new Vector2(back.Width - 220, back.Height - 80), "Back", 200, 'B');

            //Initialize option buttons
            option_musicPlus = new Button(new Vector2(back.Width - 125, 167), "+", 50, null);
            option_musicMinus = new Button(new Vector2(back.Width - 75, 167), "-", 50, null);

            option_soundPlus = new Button(new Vector2(back.Width - 125, 227), "+", 50, null);
            option_soundMinus = new Button(new Vector2(back.Width - 75, 227), "-", 50, null);

            option_fullscreen = new Button(new Vector2(25, back.Height / 2 - 295), "Fullscreen: " + graphics.IsFullScreen, 250, null);
            option_details = new Button(new Vector2(25, back.Height / 2 - 235), "Details: " + GetDetail(), 250, null);
            option_cameye = new Button(new Vector2(25, back.Height / 2 - 175), "CamEye Corner: " + GameEngine.gameCamera.eyeCorner, 250, null);
            option_lens = new Button(new Vector2(25, back.Height / 2 - 115), "Lens Flare: " + PlanetoidGame.lens.Enabled, 250, null);
            option_resolution = new Button(new Vector2(25, back.Height / 2 - 55), "Resolution: " + currentResolution, 250, null);
            option_orbits = new Button(new Vector2(25, back.Height / 2 + 5), "Orbits: " + currentOrbit, 250, null);

            option_shortNames = new Button(new Vector2(25, back.Height / 2 + 65), "Show Info: " + HUDManager.button_names.shortcut, 250, null);
            option_shortBuild = new Button(new Vector2(25, back.Height / 2 + 125), "Build Menu: " + HUDManager.button_build.shortcut, 250, null);
            option_shortDiplomacy = new Button(new Vector2(25, back.Height / 2 + 185), "Diplomacy Menu: " + HUDManager.button_diplomacy.shortcut, 250, null);
            option_shortStrategy = new Button(new Vector2(25, back.Height / 2 + 245), "Strategy View: " + HUDManager.button_strategy.shortcut, 250, null);

            //Initialize players buttons for the lobby
            lobby_players = new Button[8];
            for (int a = 0; a < 8; a++) {
                lobby_players[a] = new Button(new Vector2(90, back.Height / 2 - 200 + (a * 50)), "None", 220, null);
            }

            lobby_mode = new Button(new Vector2(back.Width - 220, back.Height - 260), "Tutorial Mode", 200, null);
            lobby_play = new Button(new Vector2(back.Width - 220, back.Height - 200), "Play!", 200, 'P');
            lobby_reset = new Button(new Vector2(back.Width - 220, back.Height - 140), "Reset", 200, 'R');
            lobby_name = new Button(new Vector2(back.Width / 2 - 250, 50), "Game Name: ", 500, null);
            lobby_next = new Button(new Vector2(back.Width / 2 + 75, 180), ">>", 50, null);
            lobby_prev = new Button(new Vector2(back.Width / 2 - 125, 180), "<<", 50, null);
            all_back = new Button(new Vector2(back.Width - 220, back.Height - 80), "Back", 200, 'B');


            //Pages
            texture_page = GameEngine.Game.Content.Load<Texture2D>("Pages//old_page");
            content_page = new Dictionary<int, string>();
            texture_pages = new Dictionary<int, Texture2D>();

            document.Load("Content//Pages//pages.xml");
            foreach (XmlNode node in document.GetElementsByTagName("Page")) {
                content_page.Add(int.Parse(node.ChildNodes[0].InnerText), node.ChildNodes[1].InnerText);
                texture_pages.Add(int.Parse(node.ChildNodes[0].InnerText), GameEngine.Game.Content.Load<Texture2D>("Pages//page" + node.ChildNodes[0].InnerText));
            }

            pauseScreen = new RenderTarget2D(graphics.GraphicsDevice, Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
            IsGameplayPaused = false;

            GameEngine.currentGameName = GameEngine.RandomName();

            //Set up the panels
            main_matrix = Matrix.Invert(Matrix.CreateLookAt(new Vector3(1, 0, 0), Vector3.Zero, Vector3.Up));
            main_matrix.Translation = new Vector3(400, 0, 0);
            control_matrix = Matrix.Invert(Matrix.CreateLookAt(new Vector3(0, 0, -1), Vector3.Zero, Vector3.Up));
            control_matrix.Translation = new Vector3(0, 0, -400);
            option_matrix = Matrix.Invert(Matrix.CreateLookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.Up));
            option_matrix.Translation = new Vector3(0, 0, 400);
            lobby_matrix = Matrix.Invert(Matrix.CreateLookAt(new Vector3(-1, 0, 0), Vector3.Zero, Vector3.Up));
            lobby_matrix.Translation = new Vector3(-400, 0, 0);

            //ROBA PER IL DEBUG COSI SALTO LA MERDA INIZIALE
            //currentPanel = 3;
            //nextPanel = 3;
            //camera.horizontalAngle = -MathHelper.Pi;
            camera.zoom = 8000;
            camera.verticalAngle = MathHelper.PiOver2;
        }

        /// <summary>
        /// Get the path to the settings file
        /// </summary>
        public static string GetSettingPath() {
            return GetBasePath() + "//settings.xnb";
        }

        /// <summary>
        /// Get the path to the game folder
        /// </summary>
        public static string GetBasePath() {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "//Planetoid 3D";
        }

        /// <summary>
        /// Get the path to the savedgames folder
        /// </summary>
        public static string GetSavePath(string filename) {
            return GetBasePath() + "//SavedGames//" + filename;
        }

        public static XmlDocument document;

        public static void SetViewport() {
            switch (currentResolution) {
                case Resolution.Auto:
                    graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                    break;
                default:
                    string[] vals = currentResolution.ToString().Substring(1).Split(new char[] { 'x' });
                    graphics.PreferredBackBufferWidth = int.Parse(vals[0]);
                    graphics.PreferredBackBufferHeight = int.Parse(vals[1]);
                    break;
            }
            graphics.ApplyChanges();
            Util.InitializeQuadVertices();
            MessageBox.AdaptPosition();
            GameEngine.gameCamera.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), GameEngine.Game.GraphicsDevice.Viewport.AspectRatio, 1.0f, 100000.0f);
        }

        //Variables
        private static GraphicsDeviceManager graphics;
        public static int currentPanel;
        public static int nextPanel;
        private static float translationSpeed;
        private static float panelTranslation;

        private static readonly float[] angles = new float[] { 0, -MathHelper.PiOver2, MathHelper.PiOver2, -MathHelper.Pi, MathHelper.PiOver2 };

        private static bool hover;
        private static float infoFade;
        private static string infoText;

        private static float yposload;
        private static float yspdload;

        //Scene variables
        public static Sun actor;
        public static Camera camera;
        private static Hominid lobby_actor;
        private static Camera lobby_camera;

        //Panels variables, the menu is divided in four different panels:
        //Main Panel
        private static Button main_newgame;
        private static Button main_continue;
        private static Button main_load;
        private static Button main_options;
        private static Button main_quests;
        private static Button main_extra;
        private static Button main_quit;

        private static Button[] main_questList;

        private static float questShow;
        private static int currentQuestPage = -1;
        private static int nextQuestPage = -1;

        //Load Panel
        private static List<Button> load_slots;
        private static List<FileInfo> load_slotsDetails;
        private static int load_selected = -1;
        private static Button load_load;
        private static Button load_delete;

        //Options panel
        private static Button option_musicPlus;
        private static Button option_musicMinus;

        private static Button option_soundPlus;
        private static Button option_soundMinus;

        private static Button option_fullscreen;
        private static Button option_details;
        private static Button option_cameye;
        private static Button option_lens;

        private static Button option_resolution;
        private static Resolution currentResolution;
        private static Button option_orbits;
        public static Orbit currentOrbit;

        private static Button option_shortNames;
        private static Button option_shortBuild;
        private static Button option_shortDiplomacy;
        private static Button option_shortStrategy;

        //Lobby panel
        private static float postProcessDark;
        private static Button[] lobby_players;
        public static int lobby_hovered;

        private static Button lobby_play;
        private static Button lobby_reset;
        private static Button lobby_name;
        private static Button lobby_next;
        private static Button lobby_prev;

        private static Button lobby_mode;

        public static Button all_back;

        //Pages panel
        private static Texture2D texture_page;
        private static Dictionary<int, string> content_page;
        private static Dictionary<int, Texture2D> texture_pages;

        private static int index_page;
        private static int next_page;
        private static int temp_page;
        private static float fade_page;

        //Render targets
        private static RenderTarget2D back;
        private static RenderTarget2D main_panel;
        private static Matrix main_matrix;
        private static RenderTarget2D control_panel;
        private static Matrix control_matrix;
        private static RenderTarget2D option_panel;
        private static Matrix option_matrix;
        private static RenderTarget2D lobby_panel;
        private static Matrix lobby_matrix;

        public static RenderTarget2D pauseScreen;

        //Main switch to tell the game if a game is open
        private static bool IsGameplayPaused;

        public static void GetInPauseMenu() {
            IsGameplayPaused = true;
            lobby_play.text = "Save Game";
            lobby_reset.text = "End Game";
            lobby_mode.text = "Resume Game";
            currentPanel = 3;//lobby
            nextPanel = 3;
            camera.horizontalAngle = angles[3];
            camera.UpdateViewMatrix();
        }

        public static void ResetMenu() {
            currentPanel = 0;
            nextPanel = 0;
            panelTranslation = 0;
            camera.horizontalAngle = 0;
        }

        private static string GetDetail() {
            return (PlanetoidGame.details == 0 ? "Low" : (PlanetoidGame.details == 1 ? "Normal" : "High"));
        }

        //Methods
        public static bool Update(GameTime gameTime) {
            //Update the 3d scene
            actor.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            hover = false;

            #region Update panel fading in the menu
            if (nextPanel != currentPanel) {
                if (panelTranslation == 0) {
                    AudioManager.Play("menu_panel_fade");
                }
                //The background becomes darker if we are entering in the lobby
                //So the race name and description will be readable
                if (nextPanel == 3) {
                    postProcessDark = panelTranslation / 2f;
                } else {
                    postProcessDark *= 0.98f;
                }
                //If the menu is fading from a panel to another
                if (panelTranslation < 1) {
                    //Accelerates and after half, decelerates
                    if (panelTranslation < 0.745f) {
                        if (translationSpeed < 0.01f) {
                            translationSpeed += 0.0002f;
                        }
                    } else {
                        if (translationSpeed > 0) {
                            translationSpeed -= 0.0002f;
                        }
                    }
                    //Update translation
                    panelTranslation += translationSpeed;//*10 only during debug
                    //And also camera
                    camera.horizontalAngle = MathHelper.Lerp(angles[currentPanel], angles[nextPanel], panelTranslation);
                } else {
                    //The translation is finished, set values stable
                    currentPanel = nextPanel;
                    camera.horizontalAngle = angles[currentPanel];
                    panelTranslation = 0;
                    translationSpeed = 0;
                }
            }
            #endregion

            else {

                switch (currentPanel) {
                    #region MAIN PANEL
                    case 0:
                        main_newgame.Update();
                        if (main_newgame.IsHovered) {
                            hover = true;
                            infoText = "Enter in the world of Planetoid!";
                            if (main_newgame.IsClicked()) {
                                if (IsGameplayPaused) {
                                    MessageBox.ShowDialog("Warning", "Creating a new game will delete your unsaved data!\nContinue?", 0);
                                } else {
                                    //Bypass the MessageBox
                                    MessageBox.ByPass("Warning", MessageBoxResult.Ok);
                                }
                            }
                        }

                        //The continue button only works when game is paused
                        if (IsGameplayPaused) {
                            main_continue.Update();
                            if (main_continue.IsHovered) {
                                hover = true;
                                infoText = "Return to your open game!";
                                if (main_continue.IsClicked()) {
                                    //Go to lobby screen
                                    nextPanel = 3;
                                }
                            }
                        } else {
                            main_continue.KeepBlocked();
                        }

                        //Go to the load screen
                        main_load.Update();
                        if (main_load.IsHovered) {
                            hover = true;
                            infoText = "Go to the load screen and choose a game!";
                            if (main_load.IsClicked()) {
                                //Go to load screen
                                GetSavedGames();
                                nextPanel = 1;
                            }
                        }

                        //Go to the options screen
                        main_options.Update();
                        if (main_options.IsHovered) {
                            hover = true;
                            infoText = "Set up options as you prefer!";
                            if (main_options.IsClicked()) {
                                //Go to options screen
                                nextPanel = 2;
                            }
                        }

                        //Show quest list
                        main_quests.Update();
                        if (main_quests.IsHovered) {
                            hover = true;
                            infoText = "Show/Hide the quest list";
                            if (main_quests.IsClicked()) {
                                //Go to options screen
                                nextQuestPage++;
                                main_quests.text = "Quests (" + (currentQuestPage + 2) + "/" + QuestManager.quests.Length / 10 + ")";
                                if (nextQuestPage >= QuestManager.quests.Length / 10) {
                                    nextQuestPage = -1;
                                    main_quests.text = "Quests";
                                }
                            }
                        }
                        if (currentQuestPage == nextQuestPage) {
                            questShow += (1 - questShow) / 10f;
                        } else {
                            questShow -= questShow / 10f;
                            if (questShow <= 0.1f) {
                                currentQuestPage = nextQuestPage;
                            }
                        }
                        if (currentQuestPage > -1) {
                            for (int a = 0; a < main_questList.Length; a++) {
                                main_questList[a].position.Y = (back.Height / 2 - 300 + (a * 60)) - (900 * (1 - questShow));
                                // main_questList[a].position.Y = -800 + (925 + (a * 60)) * questShow;

                                main_questList[a].text = QuestManager.GetQuest(a + (currentQuestPage * 10));

                                main_questList[a].Update();
                                if (main_questList[a].IsHovered) {
                                    hover = true;
                                    infoText = QuestManager.quests[a + (currentQuestPage * 10)].description;
                                    /*if (a == 9 && main_questList[9].IsClicked() && QuestManager.quests[19].actual==1)
                                    {
                                        MessageBox.ShowDialog("Finished", "You completed all the quests!\nWould you like to insert your name", 0);
                                        MessageBox.Input = "?";
                                    }*/
                                }
                            }

                            /*if (MessageBox.Title.Equals("Finished"))
                            {
                                if (MessageBox.lastResult == MessageBoxResult.Ok)
                                {
                                    if (MessageBox.Input == "?")
                                    {
                                        MessageBox.Reset();
                                        MessageBox.ShowDialog("Finished", "Insert your name: ", 1);
                                    }
                                    else if (MessageBox.Input != "")
                                    {
                                        try
                                        {
                                            WebClient objWebClient = new WebClient();

                                            System.Collections.Specialized.NameValueCollection values = new System.Collections.Specialized.NameValueCollection();
                                            values.Add("name", MessageBox.Input);

                                            byte[] bytes = objWebClient.UploadValues("http://www.metalsoulstudios.net/addHighscore.php", "POST", values);
                                            MessageBox.Reset();
                                            MessageBox.ShowDialog("Result", Encoding.ASCII.GetString(bytes), 0);
                                            if ((char)bytes[0] != 'P')
                                            {
                                                QuestManager.quests[19].actual = 2;
                                            }
                                            
                                        }
                                        catch (WebException e)
                                        {
                                            MessageBox.ShowDialog("Result", "Planetoid 3D Error #003", 0);
                                        }
                                    }
                                }
                            }*/
                        }

                        //Get to the extras menu
                        main_extra.Update();
                        if (main_extra.IsHovered) {
                            hover = true;
                            infoText = "Know more about the planetoid universe!";
                            if (main_extra.IsClicked()) {
                                nextPanel = 4;
                                index_page = -1;
                                next_page = -1;
                                fade_page = 0;
                            }
                        }

                        //Quit the game
                        main_quit.Update();
                        if (main_quit.IsHovered) {
                            hover = true;
                            infoText = "Leave";
                            if (main_quit.IsClicked()) {
                                MessageBox.Reset();
                                MessageBox.ShowDialog("Quit", "Are you sure you want to exit?", 0);
                            }
                        }

                        //Execute MessageBox results
                        if (MessageBox.lastResult == MessageBoxResult.Ok) {
                            if (MessageBox.Title == "Quit") {
                                MessageBox.Reset();
                                return true;
                            } else if (MessageBox.Title == "Warning") {
                                if (IsGameplayPaused) {
                                    //Delete stuff
                                    IsGameplayPaused = false;
                                    lobby_play.text = "Play!";
                                    lobby_reset.text = "Reset";
                                    lobby_mode.text = GameEngine.gameMode + " Mode";
                                    AudioManager.ToggleMusic(AudioStopOptions.Immediate);
                                }
                                //Go to lobby screen
                                nextPanel = 3;
                                GameEngine.currentGameName = GameEngine.RandomName();
                                MessageBox.Reset();
                            }
                        }

                        break;
                    #endregion
                    #region LOAD PANEL
                    case 1:
                        //The load and delete buttons only work if a game slot is selected
                        if (load_selected > -1) {
                            //Load the selected game
                            load_load.Update();
                            if (load_load.IsHovered) {
                                hover = true;
                                infoText = "Play this game!";
                                if (load_load.IsClicked()) {
                                    //Select the game to load
                                    GameEngine.gameToLoad = load_slotsDetails[load_selected].Name.Split('.')[0];

                                    bool succeded = true;
                                    string fn = MenuManager.GetSavePath(GameEngine.gameToLoad + ".xml");
                                    try {
                                        DataManager.Decrypt(fn);
                                        MenuManager.document.Load(fn);
                                        DataManager.Encrypt(fn);
                                    } catch (XmlException) {
                                        succeded = false;
                                        MessageBox.ShowDialog("Nooo", "This save game appears to be corrupted :(\nPress OK to delete it.", 0);
                                    }

                                    if (succeded) {
                                        //Go to the loading screen
                                        PlanetoidGame.game_screen_next = GameScreen.Loading;

                                        if (IsGameplayPaused == false) {
                                            AudioManager.Play("start_game");
                                            AudioManager.ToggleMusic(AudioStopOptions.Immediate);
                                        }
                                    }
                                }
                            }
                            //Delete the selected game
                            load_delete.Update();
                            if (load_delete.IsHovered) {
                                hover = true;
                                infoText = "Delete this game";
                                if (load_delete.IsClicked()) {
                                    MessageBox.ShowDialog("Sure?", "Are you sure you want to delete this game file?\nEverything about it will be lost!", 0);
                                }
                            }
                        } else {
                            load_load.KeepBlocked();
                            load_delete.KeepBlocked();
                        }



                        yspdload *= 0.98f;
                        yposload += yspdload;

                        if (yposload < graphics.PreferredBackBufferHeight / 2 - load_slots.Count * 60 && yspdload < 0) {
                            yspdload += 0.5f;
                        } else if (yposload > 200 && yspdload > 0) {
                            yspdload -= 0.5f;
                        } else if (Math.Abs(GameEngine.ms.Y - graphics.PreferredBackBufferHeight / 2) > graphics.PreferredBackBufferHeight / 3 && Math.Abs(GameEngine.ms.X - graphics.PreferredBackBufferWidth / 2) < 200) {
                            yspdload -= (GameEngine.ms.Y - graphics.PreferredBackBufferHeight / 2) / 2000f;
                        }

                        //Update the game slots
                        for (int a = 0; a < load_slots.Count; a++) {
                            load_slots[a].position.Y += yspdload;
                            load_slots[a].Update();
                            if (load_slots[a].IsHovered) {
                                hover = true;
                                infoText = "Load this game";
                                if (load_slots[a].IsClicked()) {
                                    //Set up the selected flag to this game slot
                                    load_selected = a;
                                }
                            }
                        }

                        //The button to return to the main menu
                        all_back.Update();
                        if (all_back.IsHovered) {
                            hover = true;
                            infoText = "Return to main menu";
                            if (all_back.IsClicked()) {
                                nextPanel = 0;
                            }
                        }

                        if ((MessageBox.Title == "Sure?" || MessageBox.Title == "Nooo") && MessageBox.lastResult == MessageBoxResult.Ok && load_selected != -1) {
                            //REMOVE THE SLOT AND MOVE ALL THE NEXT SLOT 60px UP
                            for (int a = load_selected; a < load_slots.Count; a++) {
                                load_slots[a].position.Y -= 60;
                            }
                            //Also delete the file!
                            File.Delete(load_slotsDetails[load_selected].FullName);
                            load_slots.RemoveAt(load_selected);
                            load_slotsDetails.RemoveAt(load_selected);
                            load_selected = -1;
                            //GetSavedGames();
                            MessageBox.Reset();
                        }
                        break;
                    #endregion
                    #region OPTION PANEL
                    case 2:
                        //When you get back from the options menu all the modified options are saved
                        //This would result annoying for the player, but it works in this way for security reasons
                        all_back.Update();
                        if (all_back.IsHovered) {
                            hover = true;
                            infoText = "Save and return to main menu";
                            if (all_back.IsClicked()) {
                                DataManager.Decrypt(GetSettingPath());
                                //save settings
                                document.Load(GetSettingPath());
                                document.GetElementsByTagName("Music")[0].FirstChild.Value = AudioManager.music_volume.ToString();
                                document.GetElementsByTagName("Sound")[0].FirstChild.Value = AudioManager.sound_volume.ToString();
                                document.GetElementsByTagName("Fullscreen")[0].FirstChild.Value = graphics.IsFullScreen.ToString();
                                document.GetElementsByTagName("Details")[0].FirstChild.Value = PlanetoidGame.details.ToString();
                                document.GetElementsByTagName("Cameye")[0].FirstChild.Value = GameEngine.gameCamera.eyeCorner.ToString();
                                document.GetElementsByTagName("Lens")[0].FirstChild.Value = PlanetoidGame.lens.Enabled.ToString();
                                document.GetElementsByTagName("ShortNames")[0].FirstChild.Value = HUDManager.button_names.shortcut.ToString();
                                document.GetElementsByTagName("ShortBuild")[0].FirstChild.Value = HUDManager.button_build.shortcut.ToString();
                                document.GetElementsByTagName("ShortDiplomacy")[0].FirstChild.Value = HUDManager.button_diplomacy.shortcut.ToString();
                                document.GetElementsByTagName("ShortStrategy")[0].FirstChild.Value = HUDManager.button_strategy.shortcut.ToString();
                                document.GetElementsByTagName("Resolution")[0].FirstChild.Value = "" + (int)currentResolution;
                                document.GetElementsByTagName("Orbits")[0].FirstChild.Value = "" + (int)currentOrbit;
                                document.Save(GetSettingPath());
                                nextPanel = 0;
                                DataManager.Encrypt(GetSettingPath());
                            }
                        }

                        //Buttons for adjusting music volume
                        option_musicPlus.Update();
                        if (option_musicPlus.IsClicked()) {
                            if (AudioManager.music_volume < 10) {
                                AudioManager.music_volume++;
                                AudioManager.ChangeVolume("Music", AudioManager.music_volume);
                            }
                        }
                        option_musicMinus.Update();
                        if (option_musicMinus.IsClicked()) {
                            if (AudioManager.music_volume > 0) {
                                AudioManager.music_volume--;
                                AudioManager.ChangeVolume("Music", AudioManager.music_volume);
                            }
                        }

                        //Buttons for adjusting sound volume
                        option_soundPlus.Update();
                        if (option_soundPlus.IsClicked()) {
                            if (AudioManager.sound_volume < 10) {
                                AudioManager.sound_volume++;
                                AudioManager.ChangeVolume("Sound", AudioManager.sound_volume);
                            }
                        }
                        option_soundMinus.Update();
                        if (option_soundMinus.IsClicked()) {
                            if (AudioManager.sound_volume > 0) {
                                AudioManager.sound_volume--;
                                AudioManager.ChangeVolume("Sound", AudioManager.sound_volume);
                            }
                        }

                        //Button to toggle fullscreen
                        option_fullscreen.Update();
                        if (option_fullscreen.IsClicked()) {
                            //Save pausescreen content
                            Color[] safe = new Color[pauseScreen.Width * pauseScreen.Height];
                            pauseScreen.GetData(safe);
                            //Toggle fullscreen
                            graphics.ToggleFullScreen();
                            graphics.ApplyChanges();
                            option_fullscreen.text = "Fullscreen: " + graphics.IsFullScreen;
                            //Load pausescreen content, toggling fullscreen will discard the content of the RenderTarget
                            pauseScreen.SetData(safe);
                        }

                        //Main switch for game details
                        option_details.Update();
                        if (option_details.IsClicked()) {
                            PlanetoidGame.details++;
                            if (PlanetoidGame.details == 3)
                                PlanetoidGame.details = 0;
                            option_details.text = "Details: " + GetDetail();
                            Util.InitializeVertices();
                        }

                        //Cam eye mode
                        //If this setting is on, in the gameplay the field of view of the camera
                        //will be significatively affected by zoom.
                        //The more the player will zoom out, the wider the field of view will result.
                        //The final result is a fish eye cam with higher out-zooming.
                        option_cameye.Update();
                        if (option_cameye.IsHovered) {
                            hover = true;
                            infoText = "The zoom will affect the field of view of the camera.";
                            if (option_cameye.IsClicked()) {
                                GameEngine.gameCamera.eyeCorner = !GameEngine.gameCamera.eyeCorner;
                                option_cameye.text = "CamEye Corner: " + GameEngine.gameCamera.eyeCorner;
                            }
                        }

                        //Toggle lens flare for the Ingame Sun
                        option_lens.Update();
                        if (option_lens.IsHovered) {
                            hover = true;
                            infoText = "Toggle lens flare, very cool during GamePlay.. don't forget to wear sunglasses!";
                            if (option_lens.IsClicked()) {
                                PlanetoidGame.lens.Enabled = !PlanetoidGame.lens.Enabled;
                                option_lens.text = "Lens Flare: " + PlanetoidGame.lens.Enabled;
                            }
                        }

                        //Switch between reoslutions
                        option_resolution.Update();
                        if (option_resolution.IsHovered) {
                            hover = true;
                            infoText = "Change the resolution of the game window, REQUIRES GAME RESTART TO TAKE EFFECT";
                            if (option_resolution.IsClicked()) {
                                if ((int)(++currentResolution) == Enum.GetValues(typeof(Resolution)).Length) {
                                    currentResolution = 0;
                                }

                                option_resolution.text = "Resolution: " + currentResolution;
                            }
                        }

                        //Switch between orbits types
                        option_orbits.Update();
                        if (option_orbits.IsHovered) {
                            hover = true;
                            infoText = "Change the orbits opacity";
                            if (option_orbits.IsClicked()) {
                                if ((int)(++currentOrbit) == 4) {
                                    currentOrbit = 0;
                                }

                                option_orbits.text = "Orbits: " + currentOrbit;
                            }
                        }

                        //Set the shortcuts for the game
                        option_shortNames.Update();
                        if (option_shortNames.IsClicked()) {
                            MessageBox.ShowDialog("Names", "Press the new char for the Names button", 2);
                        }
                        option_shortBuild.Update();
                        if (option_shortBuild.IsClicked()) {
                            MessageBox.ShowDialog("Build", "Press the new char for the Names button", 2);
                        }
                        option_shortDiplomacy.Update();
                        if (option_shortDiplomacy.IsClicked()) {
                            MessageBox.ShowDialog("Diplomacy", "Press the new char for the Names button", 2);
                        }
                        option_shortStrategy.Update();
                        if (option_shortStrategy.IsClicked()) {
                            MessageBox.ShowDialog("Strategy", "Press the new char for the Names button", 2);
                        }

                        /*option_port.Update();
                        if (option_port.IsHovered)
                        {
                            hover = true;
                            infoText = "Change the port used to play online (be sure to have the same of your friends!)";
                            if (option_port.IsClicked())
                            {
                                MessageBox.ShowDialog("Port", "Change the port currently used: ", 1);
                                MessageBox.Input = "" + OnlineManager.port;
                            }
                        }*/

                        //Execute MessageBox result
                        if (MessageBox.lastResult == MessageBoxResult.Ok) {
                            /*if (MessageBox.Title == "Port")
                            {
                                int port = OnlineManager.port;
                                if (int.TryParse(MessageBox.Input, out OnlineManager.port) == false)
                                {
                                    OnlineManager.port = port;
                                }
                                option_port.text = "Online Port: " + OnlineManager.port;

                                MessageBox.Reset();
                            }
                            else*/
                            if (MessageBox.Title == "Names") {
                                HUDManager.button_names.shortcut = MessageBox.Input[0];
                                option_shortNames.text = "Show Info: " + HUDManager.button_names.shortcut;
                                MessageBox.Reset();
                            } else if (MessageBox.Title == "Build") {
                                HUDManager.button_build.shortcut = MessageBox.Input[0];
                                option_shortBuild.text = "Build Menu: " + HUDManager.button_build.shortcut;
                                MessageBox.Reset();
                            } else if (MessageBox.Title == "Diplomacy") {
                                HUDManager.button_diplomacy.shortcut = MessageBox.Input[0];
                                option_shortDiplomacy.text = "Diplomacy Menu: " + HUDManager.button_diplomacy.shortcut;
                                MessageBox.Reset();
                            } else if (MessageBox.Title == "Strategy") {
                                HUDManager.button_strategy.shortcut = MessageBox.Input[0];
                                option_shortStrategy.text = "Strategy View: " + HUDManager.button_strategy.shortcut;
                                MessageBox.Reset();
                            }
                        }
                        break;
                    #endregion
                    #region LOBBY PANEL / PAUSE SCREEN
                    case 3:
                        //The lobby acts as lobby if the game is unpaused or as a pause menu if the game is paused
                        if (IsGameplayPaused) {
                            //The player can return to the game
                            lobby_mode.Update();
                            if (lobby_mode.IsHovered) {
                                hover = true;
                                infoText = "Get back into action!";
                            }

                            //The player can end the game
                            lobby_reset.Update();
                            if (lobby_reset.IsHovered) {
                                hover = true;
                                infoText = "End the current game";
                                if (lobby_reset.IsClicked()) {
                                    for (int a = 0; a < PlayerManager.players.Length - 1; a++) {
                                        PlayerManager.players[a].CalculateScore();
                                    }
                                    PlayerManager.CalculateResults();
                                    IsGameplayPaused = false;
                                    lobby_play.text = "Play!";
                                    lobby_reset.text = "Reset";
                                    lobby_mode.text = GameEngine.gameMode + " Mode";
                                    PlanetoidGame.game_screen = GameScreen.Gameplay;
                                    PlanetoidGame.game_screen_next = GameScreen.Result;
                                }
                            }

                            //The player resume the game
                            if (lobby_mode.IsClicked() || GameEngine.ks.IsKeyDown(Keys.Escape) && GameEngine.pks.IsKeyUp(Keys.Escape)) {
                                IsGameplayPaused = false;
                                lobby_play.text = "Play!";
                                lobby_reset.text = "Reset";
                                lobby_mode.text = GameEngine.gameMode + " Mode";
                                PlanetoidGame.game_screen = GameScreen.Gameplay;
                                PlanetoidGame.game_screen_next = GameScreen.Gameplay;
                            }

                            //The player can save the game
                            lobby_play.Update();
                            if (lobby_play.IsHovered) {
                                hover = true;
                                infoText = "Save game data!";
                                if (lobby_play.IsClicked()) {
                                    //SAVE GAME STATE
                                    GameEngine.SaveGameState(GameEngine.currentGameName);
                                }
                            }

                            //The player can return to the menu and leave the game open
                            all_back.Update();
                            if (all_back.IsHovered) {
                                hover = true;
                                infoText = "You can return to the menu, your game is still safe :D";
                                if (all_back.IsClicked()) {
                                    nextPanel = 0;
                                }
                            }
                        } else {
                            //Update the lobby, basically
                            UpdateLobby();
                        }
                        break;
                    #endregion
                    #region EXTRA PANEL
                    case 4:
                        //The button to return to the main menu
                        all_back.Update();
                        if (all_back.IsHovered) {
                            hover = true;
                            infoText = "Return to main menu";
                            if (all_back.IsClicked()) {
                                nextPanel = 0;
                            }
                        }
                        //Update extra page
                        temp_page = index_page;
                        //Page fading
                        if (index_page != next_page) {
                            if (next_page == -1) {
                                if (fade_page > 0.1f) {
                                    fade_page -= fade_page / 15f;
                                } else {
                                    index_page = next_page;
                                    fade_page = 0;
                                }
                            } else {
                                if (fade_page < 0.999f) {
                                    fade_page += (1.05f - fade_page) / 15f;
                                } else {
                                    index_page = next_page;
                                    fade_page = 1;
                                }
                            }
                        } else if (index_page == -1) {
                            for (int a = 0; a < content_page.Keys.Count; a++) {
                                if (QuestManager.QuestUnlocked(content_page.Keys.ElementAt(a))) {
                                    if (Math.Abs(GameEngine.ms.Y - (graphics.PreferredBackBufferHeight / 2 - 150 + a * 30)) < 25) {
                                        temp_page = a;
                                    }
                                }
                            }
                        } else {
                            if (Math.Abs(GameEngine.ms.Y - (graphics.PreferredBackBufferHeight / 2 + 260)) < 25) {
                                temp_page = -1;
                            }
                        }
                        if (GameEngine.ms.LeftButton == ButtonState.Pressed && temp_page != index_page) {
                            next_page = temp_page;
                            if (next_page > -1) {
                                AudioManager.Play("page_in");
                            } else {
                                AudioManager.Play("page_out");
                            }
                        }

                        break;
                        #endregion
                }
            }

            //Update the fading of the info text
            if (hover) {
                if (infoFade < 1) {
                    infoFade += 0.05f;
                }
            } else {
                if (infoFade > 0) {
                    infoFade -= 0.05f;
                }
            }

            //Rotation
            camera.zoom = 800 + (camera.zoom - 800) * 0.97f;
            camera.verticalAngle *= 0.98f;
            //camera.zoom = 800;//this is for debugging too
            camera.UpdateViewMatrix();

            return false;
        }

        public static void Draw(GameTime gameTime, GraphicsDevice GraphicsDevice, SpriteFont font) {
            //Set the render target to scene
            GraphicsDevice.SetRenderTarget(PlanetoidGame.scene);
            //Clear every graphic with black color
            GraphicsDevice.Clear(Color.Black);

            //Draw background scene
            //Sun
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
            actor.DrawPlanet(camera, RenderManager.planetShader);
            //Atmosphere
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            GraphicsDevice.BlendState = BlendState.Additive;
            Util.DrawCircle(actor.matrix.Translation, actor.radius * 2, actor.color, camera);
            //Particles
            GameEngine.fireParticles.SetCamera(camera);
            GameEngine.fireParticles.Draw(gameTime);

            //Use postprocess
            GraphicsDevice.SetRenderTarget(back);
            PlanetoidGame.spriteBatch.Begin(0, BlendState.Opaque, null, null, null, RenderManager.postProcessEffect);
            RenderManager.postProcessEffect.Parameters["cameraSpeed"].SetValue(10.0f);
            RenderManager.postProcessEffect.Parameters["cameraTarget"].SetValue(new Vector2(0.5f));
            RenderManager.postProcessEffect.Parameters["CinemaAmount"].SetValue(0.0f);
            RenderManager.postProcessEffect.Parameters["Fade"].SetValue(true);
            RenderManager.postProcessEffect.Parameters["Color"].SetValue(new Vector4(0, 0, 0, 1));
            RenderManager.postProcessEffect.Parameters["Amount"].SetValue(postProcessDark);
            PlanetoidGame.spriteBatch.Draw(PlanetoidGame.scene, Vector2.Zero, Color.White);
            PlanetoidGame.spriteBatch.End();

            //Now draw panels
            //Draw main panel
            if (panelTranslation > 0 || currentPanel == 0) {
                GraphicsDevice.SetRenderTarget(main_panel);
                GraphicsDevice.Clear(Color.Transparent);

                PlanetoidGame.spriteBatch.Begin();
                main_newgame.Draw(font);
                main_continue.Draw(font);
                main_load.Draw(font);
                main_options.Draw(font);
                main_quests.Draw(font);
                main_extra.Draw(font);
                main_quit.Draw(font);

                if (currentQuestPage > -1) {
                    for (int a = 0; a < main_questList.Length; a++) {
                        main_questList[a].Draw(font, QuestManager.quests[a + (10 * currentQuestPage)].actual / (float)QuestManager.quests[a + (10 * currentQuestPage)].maximum);
                    }
                }

                PlanetoidGame.spriteBatch.End();
            }

            //Draw load panel
            if (panelTranslation > 0 || currentPanel == 1) {
                GraphicsDevice.SetRenderTarget(control_panel);
                GraphicsDevice.Clear(Color.Transparent);

                PlanetoidGame.spriteBatch.Begin();
                if (load_slots.Count > 0) {
                    for (int a = 0; a < load_slots.Count; a++) {
                        load_slots[a].Draw(font, (load_selected == a ? Color.LawnGreen : Color.White));
                    }
                } else {
                    Util.DrawCenteredText(font, "There are no saved games :(", new Vector2(back.Width / 2, 100), Color.White);
                }
                load_load.Draw(font);
                load_delete.Draw(font);
                all_back.Draw(font);
                PlanetoidGame.spriteBatch.End();
            }

            if (currentPanel == 4 || nextPanel == 4) {
                //Draw pages panel
                GraphicsDevice.SetRenderTarget(option_panel);
                GraphicsDevice.Clear(Color.Transparent);
                PlanetoidGame.spriteBatch.Begin();
                //background
                PlanetoidGame.spriteBatch.Draw(texture_page, new Vector2(graphics.PreferredBackBufferWidth / 2 - 300, graphics.PreferredBackBufferHeight / 2 - 320), Color.White);
                //Draw basic page
                Util.DrawCenteredText(PlanetoidGame.pageFont, "These Are The Pages Of The Ancient Hominidum Book\n(or what you got so far)", new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2 - 260), Color.Black);
                for (int a = 0; a < content_page.Keys.Count; a++) {
                    if (QuestManager.QuestUnlocked(content_page.Keys.ElementAt(a))) {
                        Util.DrawCenteredText(PlanetoidGame.pageFont, content_page.Values.ElementAt(a).Split(new char[] { '\n' })[0], new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2 - 150 + a * 30), (temp_page == a ? Color.Red : Color.Black));
                    }
                }
                //Draw further page
                PlanetoidGame.spriteBatch.Draw(texture_page, new Vector2(graphics.PreferredBackBufferWidth / 2 - 300, graphics.PreferredBackBufferHeight / 2 - 320 - (1 - fade_page) * 1000), Color.White);

                if (next_page > -1 || index_page > -1) {
                    int fixIndex = (next_page > -1 ? next_page : index_page);
                    PlanetoidGame.spriteBatch.Draw(texture_pages.Values.ElementAt(fixIndex), new Vector2((fixIndex % 2 == 0 ? 60 : graphics.PreferredBackBufferWidth - 240), graphics.PreferredBackBufferHeight / 2 - 180 - (1 - fade_page) * 1000), Color.White);
                    Util.DrawCenteredText(PlanetoidGame.pageFont, content_page.Values.ElementAt(fixIndex), new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2 - 260 - (1 - fade_page) * 1000), Color.Black, 490);
                }
                Util.DrawCenteredText(PlanetoidGame.pageFont, "Back", new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2 + 280 - (1 - fade_page) * 1000), (temp_page == -1 ? Color.Red : Color.Black));
                //back button
                all_back.Draw(font);
                PlanetoidGame.spriteBatch.End();
            } else {
                //Draw option panel
                if (panelTranslation > 0 || currentPanel == 2) {
                    GraphicsDevice.SetRenderTarget(option_panel);
                    GraphicsDevice.Clear(Color.Transparent);

                    PlanetoidGame.spriteBatch.Begin();
                    PlanetoidGame.spriteBatch.DrawString(font, "Music Volume: " + AudioManager.music_volume, new Vector2(back.Width - 300, 180), Color.White);
                    option_musicPlus.Draw(font);
                    option_musicMinus.Draw(font);

                    PlanetoidGame.spriteBatch.DrawString(font, "Sound Volume: " + AudioManager.sound_volume, new Vector2(back.Width - 300, 240), Color.White);
                    option_soundPlus.Draw(font);
                    option_soundMinus.Draw(font);

                    option_fullscreen.Draw(font);
                    option_details.Draw(font);
                    option_cameye.Draw(font);
                    option_lens.Draw(font);
                    option_resolution.Draw(font);
                    option_orbits.Draw(font);

                    option_shortNames.Draw(font);
                    option_shortBuild.Draw(font);
                    option_shortDiplomacy.Draw(font);
                    option_shortStrategy.Draw(font);

                    //option_port.Draw(spriteBatch, font);

                    all_back.Draw(font);
                    PlanetoidGame.spriteBatch.End();
                }
            }

            //Draw lobby panel
            if (panelTranslation > 0 || currentPanel == 3) {
                GraphicsDevice.SetRenderTarget(lobby_panel);
                if (IsGameplayPaused) {
                    GraphicsDevice.Clear(Color.Black);
                    PlanetoidGame.spriteBatch.Begin();
                    PlanetoidGame.spriteBatch.Draw(pauseScreen, Vector2.Zero, Color.DimGray);
                    lobby_mode.Draw(font);
                    lobby_play.Draw(font);
                    lobby_reset.Draw(font);
                    all_back.Draw(font);
                } else {
                    GraphicsDevice.Clear(Color.Transparent);
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;


                    if (RaceManager.IsUnlocked(PlayerManager.GetRace(lobby_hovered))) {
                        lobby_actor.owner = lobby_hovered;
                        lobby_actor.matrix = Matrix.CreateRotationX(-MathHelper.PiOver2);
                        lobby_actor.matrix *= Matrix.CreateRotationY(PlanetoidGame.elapsed);
                        PlanetoidGame.elapsed += 0.04f;
                        lobby_actor.matrix.Translation = new Vector3(0, 0, back.Height / 15f);
                        lobby_camera.eyeCorner = GameEngine.gameCamera.eyeCorner;
                        lobby_actor.DrawLobby(lobby_camera);
                    }

                    PlanetoidGame.spriteBatch.Begin();

                    lobby_play.Draw(font, Color.LawnGreen);
                    lobby_reset.Draw(font, Color.OrangeRed);
                    lobby_name.text = "Game Name: " + GameEngine.currentGameName;
                    lobby_name.Draw(font);
                    lobby_next.Draw(font);
                    lobby_prev.Draw(font);
                    all_back.Draw(font);

                    lobby_mode.Draw(font, Color.CadetBlue);

                    //lobby_onlineType.Draw(spriteBatch, font);
                    //lobby_connect.Draw(spriteBatch, font);

                    Util.DrawCenteredText(font, "Choose " + (lobby_hovered == 0 ? "your" : "Player " + (1 + lobby_hovered)) + " race: ", new Vector2(back.Width / 2, 140), Color.White);

                    if (RaceManager.IsUnlocked(PlayerManager.GetRace(lobby_hovered))) {
                        Util.DrawCenteredText(font, RaceManager.GetRace(PlayerManager.GetRace(lobby_hovered)) + "\n\n\n" + RaceManager.GetDescription(PlayerManager.GetRace(lobby_hovered)) + "\n\n\nSpecial Ability: " + RaceManager.GetAbilityText(PlayerManager.GetRace(lobby_hovered)), new Vector2(back.Width / 2, 300), Color.White, 350);
                    } else {
                        Util.DrawCenteredText(font, "???\n\n\nThis is a secret race, complete quests to unlock!", new Vector2(back.Width / 2, 300), Color.White, 350);
                    }
                    Util.DrawCenteredText(font, "Select players:", new Vector2(200, back.Height / 2 - 240), Color.White);
                    for (int a = 0; a < 8; a++) {
                        if (PlayerManager.GetState(a) < PlayerState.Human) {
                            lobby_players[a].Draw(font);
                        } else {
                            lobby_players[a].Draw(font, RaceManager.GetColor(PlayerManager.GetRace(a)));
                        }
                    }
                }

                PlanetoidGame.spriteBatch.End();
            }
            GraphicsDevice.SetRenderTarget(null);

            //Draw all the scene: background+panels
            PlanetoidGame.spriteBatch.Begin();
            PlanetoidGame.spriteBatch.Draw(back, Vector2.Zero, Color.White);
            //draw info text
            PlanetoidGame.spriteBatch.DrawString(font, infoText, new Vector2(10, back.Height - 30), new Color(new Vector3(infoFade)));
            PlanetoidGame.spriteBatch.End();
            /*using (Stream stream = File.OpenWrite("cat.png"))
            {
                back.SaveAsPng(stream, 1024, 768);
            }
            return;*/
            //Draw title
            double fixTime = gameTime.TotalGameTime.TotalSeconds / 200;
            while (fixTime > MathHelper.TwoPi) {
                fixTime -= MathHelper.TwoPi;
            }

            RenderManager.textEffect.Parameters["time"].SetValue((float)Math.Sin(fixTime) * 100f);

            PlanetoidGame.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, RenderManager.textEffect);
            PlanetoidGame.spriteBatch.Draw(RenderManager.titleText, new Vector2(back.Width / 2 - 240, 60 - MathHelper.ToDegrees(Math.Abs(camera.horizontalAngle)) * 10), Color.White);
            PlanetoidGame.spriteBatch.Draw(RenderManager.titleNumber, new Vector2(back.Width / 2 + 320, 130 - MathHelper.ToDegrees(Math.Abs(camera.horizontalAngle)) * 10), null, Color.White, (float)Math.Sin(PlanetoidGame.elapsed / 1.5f) / 10, new Vector2(128, 64), 0.75f + (float)Math.Abs(Math.Cos(PlanetoidGame.elapsed / 2)) / 4, SpriteEffects.None, 0.5f);
            PlanetoidGame.spriteBatch.End();

            //Draw panels
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            if (panelTranslation > 0 || currentPanel == 0) {
                Util.DrawQuadTexture(main_matrix, main_panel, camera);
            }
            if (panelTranslation > 0 || currentPanel == 1) {
                Util.DrawQuadTexture(control_matrix, control_panel, camera);
            }
            if (panelTranslation > 0 || currentPanel == 2 || currentPanel == 4) {
                Util.DrawQuadTexture(option_matrix, option_panel, camera);
            }
            if (panelTranslation > 0 || currentPanel == 3) {
                Util.DrawQuadTexture(lobby_matrix, lobby_panel, camera);
            }
        }

        private static void UpdateLobby() {
            //Start the game!
            lobby_play.Update();
            if (lobby_play.IsHovered) {
                hover = true;
                infoText = "No more panels, now the real game will start!";
                if (lobby_play.IsClicked()) {
                    //Can't start if there are locked race in the lobby
                    if (PlayerManager.players.Count(p => !RaceManager.IsUnlocked(p.race)) == 0) {
                        GetSavedGames();
                        if (load_slotsDetails.Exists(f => f.Name == GameEngine.currentGameName) == false) {
                            if (PlanetoidGame.game_screen_next == PlanetoidGame.game_screen) {
                                PlanetoidGame.game_screen_next = GameScreen.Loading;
                                PlanetoidGame.game_screen_fader = 1;
                                GameEngine.gameToLoad = null;
                                AudioManager.Play("start_game");
                                AudioManager.ToggleMusic(AudioStopOptions.Immediate);
                            }
                        } else {
                            MessageBox.ShowDialog("Existing Game", "Change your game name or delete the existing saved slot!", 0);
                        }
                    } else {
                        MessageBox.ShowDialog("Locked Race", "You can't start a game with locked races!", 0);
                    }
                }
            }

            //Reset all lobby state
            lobby_reset.Update();
            if (lobby_reset.IsHovered) {
                hover = true;
                infoText = "Reset all slots data";
                if (lobby_reset.IsClicked()) {
                    //First reset the lobby screen
                    PlayerManager.Initialize();
                    //Then get a new name for this game
                    GameEngine.currentGameName = GameEngine.RandomName();
                }
            }

            //Change the name of the game
            lobby_name.Update();
            if (lobby_name.IsHovered) {
                hover = true;
                infoText = "Change the name of the game, this will be the name of the save file";
                if (lobby_name.IsClicked()) {
                    MessageBox.ShowDialog("Game Name", "Insert game name: ", 1);
                    MessageBox.Input = GameEngine.currentGameName;
                }
            }

            lobby_mode.Update();
            if (lobby_mode.IsHovered) {
                hover = true;
                switch (GameEngine.gameMode) {
                    case GameMode.Tutorial:
                        infoText = "Start a short tutorial.";
                        break;
                    case GameMode.Skirmish:
                        infoText = "Play against other races.";
                        break;
                    case GameMode.Countdown:
                        infoText = "Skirmish Mode: You are playing in an unstable solar system, lets see how fast you are!";
                        break;
                    case GameMode.Giant:
                        infoText = "Skirmish Mode: A 2x size solar system!";
                        break;
                    case GameMode.Astrorain:
                        infoText = "Skirmish Mode: Prepare for massive keldanyum rain!";
                        break;
                    case GameMode.Hyperbuild:
                        infoText = "Skirmish Mode: Instant buildings and researches for fast gameplay!";
                        break;
                }
                if (lobby_mode.IsClicked()) {
                    switch (GameEngine.gameMode) {
                        case GameMode.Tutorial:
                            GameEngine.gameMode = GameMode.Skirmish;
                            break;
                        case GameMode.Skirmish:
                            if (QuestManager.QuestUnlocked(13)) {
                                GameEngine.gameMode = GameMode.Countdown;
                            } else if (QuestManager.QuestUnlocked(8)) {
                                GameEngine.gameMode = GameMode.Giant;
                            } else if (QuestManager.QuestUnlocked(1)) {
                                GameEngine.gameMode = GameMode.Astrorain;
                            } else if (QuestManager.QuestUnlocked(5)) {
                                GameEngine.gameMode = GameMode.Hyperbuild;
                            } else {
                                GameEngine.gameMode = GameMode.Tutorial;
                            }
                            break;
                        case GameMode.Countdown:
                            if (QuestManager.QuestUnlocked(8)) {
                                GameEngine.gameMode = GameMode.Giant;
                            } else if (QuestManager.QuestUnlocked(1)) {
                                GameEngine.gameMode = GameMode.Astrorain;
                            } else if (QuestManager.QuestUnlocked(5)) {
                                GameEngine.gameMode = GameMode.Hyperbuild;
                            } else {
                                GameEngine.gameMode = GameMode.Tutorial;
                            }
                            break;
                        case GameMode.Giant:
                            if (QuestManager.QuestUnlocked(1)) {
                                GameEngine.gameMode = GameMode.Astrorain;
                            } else if (QuestManager.QuestUnlocked(5)) {
                                GameEngine.gameMode = GameMode.Hyperbuild;
                            } else {
                                GameEngine.gameMode = GameMode.Tutorial;
                            }
                            break;
                        case GameMode.Astrorain:
                            if (QuestManager.QuestUnlocked(5)) {
                                GameEngine.gameMode = GameMode.Hyperbuild;
                            } else {
                                GameEngine.gameMode = GameMode.Tutorial;
                            }
                            break;
                        case GameMode.Hyperbuild:
                            GameEngine.gameMode = GameMode.Tutorial;
                            break;
                    }
                    lobby_mode.text = GameEngine.gameMode + " Mode";
                }
            }

            //Return to main menu
            all_back.Update();
            if (all_back.IsHovered) {
                hover = true;
                infoText = "Return to main menu";
                if (all_back.IsClicked()) {
                    nextPanel = 0;
                }
            }

            //Execute MessageBox result
            if (MessageBox.lastResult == MessageBoxResult.Ok) {
                if (MessageBox.Title == "Game Name") {
                    if (MessageBox.Input != "") {
                        GameEngine.currentGameName = MessageBox.Input;
                        if (MessageBox.Input.ToLower() == "cyclop") {
                            QuestManager.QuestCall(16);
                        }
                    }
                    MessageBox.Reset();
                }
            }

            if (GameEngine.gameMode != GameMode.Tutorial) {
                //Select next free race
                lobby_next.Update();
                if (lobby_next.IsHovered) {
                    hover = true;
                    infoText = "Select next race";
                    if (lobby_next.IsClicked()) {
                        PlayerManager.GetNextRace(lobby_hovered);
                    }
                }


                //Select previous free race
                lobby_prev.Update();
                if (lobby_prev.IsHovered) {
                    hover = true;
                    infoText = "Select previous race";
                    if (lobby_prev.IsClicked()) {
                        PlayerManager.GetPreviousRace(lobby_hovered);
                    }
                }
            } else {
                lobby_prev.KeepBlocked();
                lobby_next.KeepBlocked();
                lobby_hovered = 0;
            }

            //Players slots update
            for (int a = 0; a < lobby_players.Length; a++) {
                if (RaceManager.IsUnlocked(PlayerManager.GetRace(a))) {
                    lobby_players[a].text = RaceManager.GetRace(PlayerManager.GetRace(a));
                } else {
                    lobby_players[a].text = "???";
                }
                switch (PlayerManager.GetState(a)) {
                    case PlayerState.Close:
                        lobby_players[a].text = "Close";
                        break;
                    /*case PlayerState.Open:
                        lobby_players[a].text = "Open";
                        break;*/
                    case PlayerState.Human:
                        if (a == 0) {
                            lobby_players[a].text += " - You";
                        }
                        break;
                    case PlayerState.Dumb:
                        lobby_players[a].text += " - Dumb";
                        break;
                    case PlayerState.Normal:
                        lobby_players[a].text += " - Normal";
                        break;
                    case PlayerState.Challenging:
                        lobby_players[a].text += " - Challenging";
                        break;
                    case PlayerState.Hardcore:
                        lobby_players[a].text += " - Hard";
                        break;
                }


                if (GameEngine.gameMode == GameMode.Tutorial) {
                    PlayerManager.players[0].race = 0;
                    if (a > 0) {
                        lobby_players[a].KeepBlocked();
                        PlayerManager.players[a].state = PlayerState.Close;
                        lobby_players[a].text = "Locked";
                    }
                    continue;
                }

                lobby_players[a].Update();
                if (lobby_players[a].IsHovered) {
                    if (PlayerManager.GetState(a) > PlayerState.Close) {
                        lobby_hovered = a;
                    }
                    if (lobby_players[a].IsClicked() && a != 0) {
                        switch (PlayerManager.GetState(a)) {
                            case PlayerState.Close:
                                PlayerManager.SetState(a, PlayerState.Dumb);
                                //Get a race different from all previous
                                PlayerManager.SetRace(a, PlayerManager.GetUniqueRace(a));
                                break;
                            case PlayerState.Dumb:
                                PlayerManager.SetState(a, PlayerState.Normal);
                                lobby_hovered = 0;
                                break;
                            case PlayerState.Normal:
                                PlayerManager.SetState(a, PlayerState.Challenging);
                                lobby_hovered = 0;
                                break;
                            case PlayerState.Challenging:
                                PlayerManager.SetState(a, PlayerState.Hardcore);
                                break;
                            case PlayerState.Hardcore:
                                PlayerManager.SetState(a, PlayerState.Close);
                                lobby_hovered = 0;
                                break;

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get all the saved game files in the Saves folder
        /// </summary>
        private static void GetSavedGames() {
            if (load_slots.Count == 0) {
                DirectoryInfo dir = new DirectoryInfo(GetSavePath(""));
                load_slotsDetails = dir.GetFiles().ToList();

                //Array.Sort(, (x, y) =>y.CreationTimeUtc.CompareTo(x.CreationTimeUtc))
                load_slots.Clear();

                for (int a = 0; a < load_slotsDetails.Count; a++) {
                    load_slots.Add(new Button(new Vector2(back.Width / 2 - 250, 200 + a * 60), load_slotsDetails[a].Name.Split('.')[0] + " - " + load_slotsDetails[a].CreationTimeUtc.ToString(), 500, null));
                }
                load_selected = -1;
                yspdload = 0;
                yposload = 200;
            }
        }
    }
}
