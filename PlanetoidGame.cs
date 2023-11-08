using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Threading;

using System.Net;
using System.Text;

namespace Planetoid3D
{
    public enum GameScreen
    {
        Intro,
        Menu,
        Loading,
        Gameplay,
        Result,
        Credits
    }
    public class PlanetoidGame : Game
    {
        #region Initialization

        readonly GraphicsDeviceManager graphics;
        public static SpriteBatch spriteBatch;

        public static SpriteFont textFont;
        public static SpriteFont pageFont;

        Texture2D[] texturesCredits;
        int creditsIndex;
        float creditsFade;
        Vector2 creditsPosition;

        public static RenderTarget2D scene;

        // VideoPlayer introVideo;

        Texture2D mouse_standard;
        Texture2D mouse_target;

        //Utility variables
        public const string GAME_VERSION = "1.0.7";

        //GAME VARIABLES
        public static Thread loader;
        public static GameScreen game_screen = GameScreen.Intro;
        public static GameScreen game_screen_next = GameScreen.Intro;
        public static float game_screen_fader = 1;
        public static bool loaded = true;
        public static float elapsed;

        /// <summary>
        /// 0 = Low, 1 = Medium, 2 = High
        /// </summary>
        public static short details;
        public static LensFlare lens;
        public static float flash;

        public PlanetoidGame()
        {
            //Initialize Screen
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1024,
                PreferredBackBufferHeight = 768
            };

            //Initialize Particles
            GameEngine.fireParticles = new FireParticleSystem(this, Content);
            Components.Add(GameEngine.fireParticles);

            GameEngine.explosionParticles = new ExplosionParticleSystem(this, Content);
            Components.Add(GameEngine.explosionParticles);

            GameEngine.planetoidParticles = new PlanetoidParticleSystem(this, Content);
            Components.Add(GameEngine.planetoidParticles);

            GameEngine.tsmokeParticles = new TSmokeParticleSystem(this, Content);
            Components.Add(GameEngine.tsmokeParticles);

            GameEngine.smokeParticles = new SmokeParticleSystem(this, Content);
            Components.Add(GameEngine.smokeParticles);

            //Initialize Lens
            lens = new LensFlare(this);
            Components.Add(lens);

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            graphics.PreferMultiSampling = true;
            GraphicsDevice.PresentationParameters.MultiSampleCount = 16;
            RasterizerState state = new RasterizerState();
            state.MultiSampleAntiAlias=true;
            GraphicsDevice.RasterizerState=state;
            graphics.ApplyChanges();

            //Initialize Game Engine
            GameEngine.InitializeGameEngine(this);

            //Initialize Utils
            Util.Initialize(GraphicsDevice, Content);

            //Initialize Audio Manager
            AudioManager.Initialize(Content);

            //Initialize Menu
            MenuManager.Initialize(Window,graphics);

            //Initialize Stars
            StarManager.Initialize(this, (details * 350) + 400);

            //Initialize Debug Manager
            DebugManager.Initialize(this);

            base.Initialize();
        }
        #endregion

        #region Un/Load Content
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Initialize Quest Engine
            QuestManager.Initialize();

            //Load all the races
            RaceManager.LoadRaces();

            //Prepare HUD
            HUDManager.Initialize();

            scene = new RenderTarget2D(GraphicsDevice, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

            //Font loading
            textFont = Content.Load<SpriteFont>("Arial");
            pageFont = Content.Load<SpriteFont>("Pages");

            //Cursor loading
            mouse_standard = Content.Load<Texture2D>("mouse_standard");
            mouse_target = Content.Load<Texture2D>("mouse_target");

            PlayerManager.Initialize();

            TextBoard.Initialize(graphics);

            RenderManager.LoadModels(this);

            DataSectorSubdivider.Initialize(1000);

            //Prepare video!
            //introVideo = new VideoPlayer();
            //introVideo.Play(Content.Load<Video>("Intro"));
        }

        protected override void UnloadContent()
        {
        }

        #endregion

        #region Update
        protected override void Update(GameTime gameTime)
        {
           /* Vector3 zaxis = Vector3.Normalize(Vector3.Subtract(Vector3.Zero, new Vector3(100, 100, 100)));
            Console.WriteLine(zaxis);
            Vector3 xaxis = Vector3.Cross(Vector3.UnitY, zaxis);
            Console.WriteLine(xaxis);
            Vector3 yaxis = Vector3.Cross(zaxis, xaxis);
            Console.WriteLine(yaxis);
            Console.WriteLine(Matrix.CreateLookAt(new Vector3(100, 100, 100), Vector3.Zero, Vector3.UnitY));
            this.Exit();*/
           
            if (HUDManager.showDebug)
            {
                DebugManager.StartCounting();
            }

            //Input state update
            GameEngine.GetNextInputState();

            if (gameTime.TotalGameTime.TotalHours>=2)
            {
                QuestManager.QuestCall(4);
            }

            //Toggle debug stuff
            if (GameEngine.ks.IsKeyDown(Keys.F2) && GameEngine.pks.IsKeyUp(Keys.F2))
            {
                HUDManager.showDebug = !HUDManager.showDebug;
            }

            if (MessageBox.IsActive == false)
            {
                switch (game_screen)
                {

                    //##########INTRO VIDEO##########
                    case GameScreen.Intro:
                        /*if (game_screen_next == GameScreen.Intro)
                        {
                            if (introVideo.State == MediaState.Stopped || Keyboard.GetState().IsKeyDown(Keys.Enter) || Keyboard.GetState().IsKeyDown(Keys.Escape))
                            {
                                introVideo.Stop();
                                AudioManager.StartMusic();
                                game_screen_next = GameScreen.Menu;
                            }
                        }*/
                        AudioManager.StartMusic();
                        game_screen_next = GameScreen.Menu;
                        break;
                    //##########MAIN MENU##########
                    case GameScreen.Menu:
                        elapsed = (float)gameTime.TotalGameTime.TotalSeconds;
                        if (MenuManager.Update(gameTime))
                        {
                            this.Exit();
                        }
                        break;
                    //##########LOADING##########
                    case GameScreen.Loading:
                        if (game_screen_next == GameScreen.Loading)
                        {
                            if (loader == null)
                            {
                                loaded = false;
                                loader = new Thread(GameEngine.InitializeAll);
                                loader.Start();
                            }
                            else if (loaded)
                            {
                                game_screen_next = GameScreen.Gameplay;
                                //Initialize this now because some strings contains the PlayerRace name
                                BuildingManager.Initialize();
                                loader = null;
                            }
                        }
                        break;
                    //##########IN GAME##########
                    case GameScreen.Gameplay:
                        //Get correct elapsed
                        elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

                        //Save the screenshot
                        if (GameEngine.ks.IsKeyDown(Keys.PrintScreen) && GameEngine.pks.IsKeyUp(Keys.PrintScreen))
                        {
                            using System.IO.Stream stream = System.IO.File.OpenWrite(MenuManager.GetBasePath() + "//screen" + System.IO.Directory.GetFiles(MenuManager.GetBasePath()).Length + ".png");
                            MenuManager.pauseScreen.SaveAsPng(stream, MenuManager.pauseScreen.Width, MenuManager.pauseScreen.Height);
                            TextBoard.AddMessage("Screenshot saved in Documents//Planetoid3D");
                        }

                        GameEngine.HandleInput(elapsed);

                        if (GameEngine.gameMode == GameMode.Tutorial)
                        {
                            TutorialManager.CheckProgresses(gameTime);
                        }

                        HUDManager.Update();

                        BuildingManager.UpdateReserchList();

                        //Update all players
                        PlayerManager.UpdatePlayers(elapsed);

                        //Update the game
                        GameEngine.UpdateGame(elapsed);

                        base.Update(gameTime);

                        if (HUDManager.diplomacyMode)
                        {
                            HUDManager.diplomacyMode = DiplomacyManager.Update();
                        }

                        //Camera Main Update
                        GameEngine.gameCamera.UpdateCamera();
                        break;
                    case GameScreen.Result:
                        MenuManager.all_back.Update();
                        if (MenuManager.all_back.IsClicked())
                        {
                            AudioManager.StopPlanetoid();
                            HUDManager.lastTargetObject = null;
                            game_screen_next = GameScreen.Menu;
                            MenuManager.ResetMenu();
                            //First reset the lobby screen
                            PlayerManager.Initialize();
                            //And return to the menu music
                            AudioManager.ToggleMusic(AudioStopOptions.AsAuthored);
                        }
                        break;
                    //##########FINAL "MOVIE" WITH CREDITS##########
                    case GameScreen.Credits:
                        if (GameEngine.ks.IsKeyDown(Keys.Escape) && GameEngine.pks.IsKeyUp(Keys.Escape))
                        {
                            game_screen_next = GameScreen.Menu;
                            loaded = true;
                            MenuManager.ResetMenu();
                            AudioManager.StartMusic();
                        }
                        if (creditsIndex < 18)
                        {
                            if (elapsed == 0)
                            {
                                texturesCredits = new Texture2D[17];
                                for (int a = 0; a < 17; a++)
                                {
                                    texturesCredits[a] = Content.Load<Texture2D>("Credits//credit" + a);
                                }
                                creditsIndex = 1;
                                creditsPosition = Vector2.Zero;
                                creditsPosition.Y = GraphicsDevice.Viewport.Height - 320;
                                creditsFade = 0;
                                loaded = false;
                                AudioManager.ToggleMusic(AudioStopOptions.AsAuthored);
                                //Reset the Players
                                PlayerManager.Initialize();
                                GameEngine.blackHole = null;
                                GameEngine.planetoid = null;
                            }
                            elapsed += 0.002f;
                            if (elapsed < creditsIndex)
                            {
                                if (creditsFade < 1)
                                {
                                    creditsFade += 0.01f;
                                }
                            }
                            else
                            {
                                if (creditsFade > 0)
                                {
                                    creditsFade -= 0.01f;
                                }
                                else
                                {

                                    creditsIndex++;
                                    switch (creditsIndex)
                                    {
                                        case 2:
                                            creditsPosition.X = 100;
                                            creditsPosition.Y -= 80;
                                            break;
                                        case 3:
                                            creditsPosition.X = 150;
                                            break;
                                        case 4:
                                            creditsPosition.X = 200;
                                            break;
                                        case 5:
                                            creditsPosition.Y = 200;
                                            creditsPosition.X = 200;
                                            break;
                                        case 6:
                                            creditsPosition.X = 250;
                                            break;
                                        case 7:
                                            creditsPosition.Y = 300;
                                            creditsPosition.X = 100;
                                            break;
                                        case 8:
                                            creditsPosition.Y = GraphicsDevice.Viewport.Height - 400;
                                            creditsPosition.X = 200;
                                            break;
                                        case 9:
                                            creditsPosition.X = 0;
                                            break;
                                        case 10:
                                            creditsPosition.X = GraphicsDevice.Viewport.Width - 640; ;
                                            break;
                                        case 11:
                                            creditsPosition.Y = 250;
                                            creditsPosition.X = 200;
                                            break;
                                        case 12:
                                            creditsPosition.Y = 200;
                                            creditsPosition.X = 250;
                                            break;
                                        case 13:
                                            creditsPosition.Y = 100;
                                            creditsPosition.X = 100;
                                            break;
                                        case 14:
                                            creditsPosition.Y = 300;
                                            creditsPosition.X = 400;
                                            break;
                                        case 15:
                                            creditsPosition.Y = 200;
                                            creditsPosition.X = 200;
                                            break;
                                        case 16:
                                            creditsPosition.Y = 300;
                                            creditsPosition.X = 250;
                                            break;
                                        case 17:
                                            creditsPosition.Y = GraphicsDevice.Viewport.Height / 2 - 8;
                                            creditsPosition.X = GraphicsDevice.Viewport.Width / 2 - 68;
                                            break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (creditsIndex == 18)
                            {
                                AudioManager.StopMusic();
                                elapsed = 0;
                                creditsIndex++;
                            }

                            elapsed += 0.25f;
                            if (elapsed > 900 && game_screen_next == GameScreen.Credits)
                            {
                                game_screen_next = GameScreen.Menu;
                                loaded = true;
                                MenuManager.ResetMenu();
                                AudioManager.StartMusic();
                            }
                        }
                        break;
                }
            }
            else
            {
                MessageBox.Update(textFont);
                if (game_screen == GameScreen.Menu)
                {
                    MenuManager.actor.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                }
            }

            GameEngine.SaveLastInputState();
            if (HUDManager.showDebug)
            {
                DebugManager.EndCountingUpdate();
            }
        }
        #endregion

        #region Draw
        protected override void Draw(GameTime gameTime)
        {
            if (HUDManager.showDebug)
            {
                DebugManager.StartCounting();
            }
            GameEngine.UpdateFPS();
            switch (game_screen)
            {
                case GameScreen.Intro:
                    /*spriteBatch.Begin();
                    spriteBatch.Draw(introVideo.GetTexture(), new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight + 5), RaceManager.todayIntroColor);
                    spriteBatch.End();*/
                    break;
                case GameScreen.Menu:
                    GraphicsDevice.Clear(Color.Black);
                    MenuManager.Draw(gameTime, GraphicsDevice, textFont);
                    base.Update(gameTime);
                    break;
                case GameScreen.Loading:
                    GraphicsDevice.Clear(Color.Black);
                    TextBoard.Draw(spriteBatch, textFont);
                    spriteBatch.Begin();
                    Util.DrawCenteredText(textFont, "Hang on, it's loading!", new Vector2(scene.Width / 2, scene.Height / 2), Color.White);
                    spriteBatch.End();
                    break;
                case GameScreen.Gameplay:
                    //Set the render target to scene
                    GraphicsDevice.SetRenderTarget(MenuManager.pauseScreen);

                    //Clear every graphic with black color
                    GraphicsDevice.Clear(Color.Black);

                    //RenderManager.DrawSkySphere();
                    StarManager.RenderStars(this, GameEngine.gameCamera.viewMatrix, GameEngine.gameCamera.projectionMatrix);

                    GameEngine.DrawGame(elapsed);
                    GameEngine.SetParticlesForDrawing();
                    base.Draw(gameTime);

                    GraphicsDevice.SetRenderTarget(scene);
                    GraphicsDevice.Clear(Color.Black);


                    //APPLY GRAVITY EFFECT
                    if (GameEngine.blackHole != null)
                    {
                        spriteBatch.Begin(0, BlendState.Opaque, null, null, null, RenderManager.gravityShader);
                        GameEngine.blackHole.Draw();
                        spriteBatch.Draw(MenuManager.pauseScreen, Vector2.Zero, Color.White);
                        spriteBatch.End();
                    }
                    else
                    {
                        spriteBatch.Begin();
                        spriteBatch.Draw(MenuManager.pauseScreen, Vector2.Zero, Color.White);
                        spriteBatch.End();
                    }


                    GraphicsDevice.SetRenderTarget(MenuManager.pauseScreen);
                    GraphicsDevice.Clear(Color.Black);

                    //APPLY POSTPROCESS
                    spriteBatch.Begin(0, BlendState.Opaque, null, null, null, RenderManager.postProcessEffect);
                    //PlanetoidGame.postProcessEffect.Parameters["CinemaAmount"].SetValue(1);
                    //PlanetoidGame.postProcessEffect.Parameters["Blur"].SetValue(false);
                    if (flash > 0)
                    {
                        flash -= 0.01f;
                        RenderManager.postProcessEffect.Parameters["Fade"].SetValue(true);
                        RenderManager.postProcessEffect.Parameters["Color"].SetValue(Vector4.One);
                        RenderManager.postProcessEffect.Parameters["Amount"].SetValue(flash);
                    }
                    else
                    {
                        RenderManager.postProcessEffect.Parameters["Fade"].SetValue(false);
                    }
                    RenderManager.postProcessEffect.Parameters["cameraSpeed"].SetValue((HUDManager.lastTargetObject is Planet || HUDManager.lastTargetObject is Planetoid || HUDManager.lastTargetObject is BlackHole ? GameEngine.gameCamera.speed.Length() : 0));
                    Vector3 pos = GraphicsDevice.Viewport.Project(GameEngine.gameCamera.target.matrix.Translation, GameEngine.gameCamera.projectionMatrix, GameEngine.gameCamera.viewMatrix, Matrix.Identity);
                    RenderManager.postProcessEffect.Parameters["cameraTarget"].SetValue(new Vector2(pos.X / graphics.PreferredBackBufferWidth, pos.Y / graphics.PreferredBackBufferHeight));
                    spriteBatch.Draw(scene, Vector2.Zero, Color.White);
                    spriteBatch.End();

                    //Draw information relative to the tutorial, like what you have to do and so on
                    if (GameEngine.gameMode == GameMode.Tutorial)
                    {
                        spriteBatch.Begin();
                        TutorialManager.Draw(spriteBatch, textFont);
                        spriteBatch.End();
                    }

                    if (GameEngine.gameMode != GameMode.Tutorial || TutorialManager.CinematicMode == false)
                    {
                        HUDManager.Draw(textFont);
                    }

                    //Events draw
                    TextBoard.Draw(spriteBatch, textFont);

                    GraphicsDevice.SetRenderTarget(null);
                    GraphicsDevice.Clear(Color.Black);
                    spriteBatch.Begin();
                    spriteBatch.Draw(MenuManager.pauseScreen, Vector2.Zero, Color.White);
                    spriteBatch.End();
                    break;
                case GameScreen.Result:
                    GraphicsDevice.Clear(Color.Black);
                    PlayerManager.DrawResultScreen(textFont);
                    break;
                case GameScreen.Credits:
                    GraphicsDevice.Clear(Color.Black);
                    spriteBatch.Begin();
                    if (creditsIndex < 18)
                    {
                        spriteBatch.Draw(texturesCredits[creditsIndex - 1], creditsPosition, Color.Lerp(Color.Transparent, Color.White, creditsFade));
                    }
                    else
                    {
                        Util.DrawCenteredText(textFont, "Planetoid 3D\n\n\n- Original Idea and Development -\nLuca Zambarda\n\n- Music -\nCaelestis\n\n- Ancient Pages & Credits Artwork - \nCaelestis\n\n- Winners of Custom Race Contest -\nKarhu05\nKerlc\nDude759\nCaleb\nShockerz\nOdi-696\nHotspot\nEnergywelder\n\n- Debugging and Suggestions -\nJokerMat\nLuciano Foxtrot\nMy Parents\n\n- Technical Help -\nSteve Hazen\n\nSpecial thanks to the Planetoid Community!\nGuys you're doing a great job, thanks!", new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height + 50 - elapsed), Color.White);
                    }
                    spriteBatch.End();
                    break;
            }
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            QuestManager.Draw(textFont);
            //Draw Cursor
            GameEngine.DrawFader(spriteBatch);
            if (MessageBox.IsActive)
            {
                MessageBox.Draw(textFont);
            }

            if ((game_screen > 0 || MessageBox.IsActive) && loaded)
            {
                if (!HUDManager.spaceshipSelected)
                {
                    spriteBatch.Draw(mouse_standard, new Vector2(GameEngine.ms.X, GameEngine.ms.Y), RaceManager.GetColor(PlayerManager.GetRace(0)));
                }
                else
                {
                    spriteBatch.Draw(mouse_target, new Vector2(GameEngine.ms.X, GameEngine.ms.Y), null, RaceManager.GetColor(PlayerManager.GetRace(0)), (float)gameTime.TotalGameTime.TotalMilliseconds / 100f, new Vector2(24), 1, SpriteEffects.None, 0.5f);
                }
                if (HUDManager.showDebug)
                {
                    HUDManager.DebugText(textFont);
                }
            }

            spriteBatch.End();
            if (HUDManager.showDebug)
            {
                DebugManager.EndCountingDraw();
            }

        }

        #endregion
    }
}
