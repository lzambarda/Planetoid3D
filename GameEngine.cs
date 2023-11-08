using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace Planetoid3D {
    public enum GameMode {
        Tutorial,
        Skirmish,
        Countdown,
        Giant,
        Astrorain,
        Hyperbuild
    }

    public static class GameEngine {
        public static void InitializeGameEngine(Game game) {
            Game = game;
            timer_asteroid = 50;
            currentGameName = "";
            gameCamera = new Camera(new Vector3(0.0f, 100.0f, 300.0f));
        }

        public static Game Game;

        //Controls variables
        public static KeyboardState ks;
        public static KeyboardState pks;
        public static MouseState ms;
        public static MouseState pms;

        //Fps variables
        private static int fps;
        private static int seconds;
        public static int FPS;

        //The In-Game camera
        public static Camera gameCamera;

        //Objects declaration
        public static List<Planet> planets;
        public static List<Asteroid> asteroids;
        public static List<Shot> shots;
        public static Planetoid planetoid;
        public static List<Balloon> balloons;
        public static BlackHole blackHole;

        private static float timer_asteroid;
        public static int dragIndex;
        public static float dragDistance;
        public static string gameToLoad = null;

        public static string currentGameName;
        public static GameMode gameMode;

        /// <summary>
        /// Whenever this flag is on, objects selection will not take effect, usually it is turned on by HUD buttons
        /// </summary>
        public static bool flag_mouseInhibition;

        //Particles
        public static ParticleSystem fireParticles;
        public static ParticleSystem planetoidParticles;
        public static ParticleSystem explosionParticles;
        public static ParticleSystem tsmokeParticles;
        public static ParticleSystem smokeParticles;

        public static void GetNextInputState() {
            ms = Mouse.GetState();
            ks = Keyboard.GetState();
        }

        public static void SaveLastInputState() {
            pms = ms;
            pks = ks;
        }

        /// <summary>
        /// Initialize all the objects in the game, call this method when a new gameplay starts
        /// </summary>
        public static void InitializeAll() {
            TextBoard.ResetTextboard();
            //With this flag we say that all the stuff has been initialized and don't need to be re-initialized
            TextBoard.AddMessage("Preparing...");

            DiplomacyManager.Initialize();
            TextBoard.AddMessage("DealManager On!");

            //No initial planetoid, nor black hole
            planetoid = null;
            blackHole = null;

            if (gameMode == GameMode.Tutorial) {
                //Tutorial will always be the same
                Util.random = new Random(6);
            } else {
                Util.random = new Random();
            }

            //Initialize Variables
            HUDManager.buildingMode = false;
            HUDManager.displayNames = false;
            HUDManager.diplomacyMode = false;
            HUDManager.strategyMode = false;

            TextBoard.AddMessage("Variables Set!");

            //Baloons
            balloons = new List<Balloon>();

            //Shots
            shots = new List<Shot>();

            TextBoard.AddMessage("Objects On!");

            //Planets
            planets = new List<Planet>();
            //Sun
            planets.Add(new Sun());
            int rad;
            for (int a = 1; a < 10; a++) {
                rad = 38 + (gameMode == GameMode.Giant ? 50 : 0) + Util.random.Next(30);
                planets.Add(new Planet(a, rad, planets[0], 0.8f - (float)Util.random.NextDouble() * 0.3f));
            }
            TextBoard.AddMessage("Planets Ok!");

            //Moons
            for (int a = 1; a < 6; a++) {
                rad = 25 + (gameMode == GameMode.Giant ? 50 : 0) + Util.random.Next(10);
                planets.Add(new Planet(a, rad, planets[1 + Util.random.Next(planets.Count - 1)], 0.4f - (float)Util.random.NextDouble() * 0.15f));
            }
            TextBoard.AddMessage("Moons Are Orbiting!");

            //Asteroids
            asteroids = new List<Asteroid>();
            for (int a = 0; a < 400; a++) {
                asteroids.Add(new Asteroid(true));
            }
            TextBoard.AddMessage("Asteroids Are Flying!");

            //Initialize tutorial manager
            TutorialManager.Initialize();

            //No game to load
            if (gameToLoad == null) {
                //Get cpu players
                PlayerManager.PlacePlayers();

                TextBoard.AddMessage("Players placed!");

                //Initialize diplomacies values
                PlayerManager.CreateDiplomacies();

                TextBoard.AddMessage("Diplomacies are ok!");

                //Grow all trees to the max
                foreach (Planet p in planets) {
                    foreach (Tree t in p.trees) {
                        t.GrowMax();
                    }
                }

                TextBoard.AddMessage("Trees are grown!");

                if (gameMode == GameMode.Tutorial) {
                    //Start with the tutorial
                    TutorialManager.BeginMission(0);
                }
            } else {
                LoadGameState(gameToLoad);
            }

            //Clear the particles system
            explosionParticles.Clear();
            planetoidParticles.Clear();
            smokeParticles.Clear();
            tsmokeParticles.Clear();
            TextBoard.AddMessage("Particles Cleared...");

            TextBoard.AddMessage("Ready To Rock!");

            //Tell the game that the loading is finished
            PlanetoidGame.loaded = true;
            TextBoard.AddMessage("Game Started!");
        }

        /// <summary>
        /// Handle the input received from the player
        /// </summary> 
        public static void HandleInput(float elapsed) {
            if (GameEngine.gameMode == GameMode.Tutorial && TutorialManager.CinematicMode) {
                //Cannot interact during cinematic mode
                return;
            }

            //Pause game, go to menu
            //Unable to pause the game while online
            if (ks.IsKeyDown(Keys.Escape) && pks.IsKeyUp(Keys.Escape) && PlanetoidGame.game_screen_fader <= 0) {
                MenuManager.GetInPauseMenu();
                PlanetoidGame.game_screen = GameScreen.Menu;
                PlanetoidGame.game_screen_next = GameScreen.Menu;
                pks = ks;
            }

            if (gameMode == GameMode.Tutorial) {
                if (ks.IsKeyDown(Keys.M) && pks.IsKeyUp(Keys.M)) {
                    TutorialManager.visible = !TutorialManager.visible;
                }
            }

            //Display Names
            HUDManager.button_names.Update();
            if (HUDManager.button_names.IsClicked()) {
                HUDManager.displayNames = !HUDManager.displayNames;
            }

            //Display Strategy Mode
            HUDManager.button_strategy.Update();
            if (HUDManager.button_strategy.IsClicked()) {
                HUDManager.strategyMode = !HUDManager.strategyMode;
            }

            //Take camera to hominid view
            if (ks.IsKeyDown(Keys.H) && !HUDManager.diplomacyMode && !HUDManager.spaceshipSelected) {
                if (HUDManager.lastTargetObject is Hominid) {
                    if (gameCamera.hominidViewTranslation < 1) {
                        gameCamera.hominidViewTranslation += (1 - gameCamera.hominidViewTranslation) / 20f;
                    }
                } else {
                    //Emergency reset
                    gameCamera.hominidViewTranslation = 0;
                }
            } else if (gameCamera.hominidViewTranslation > 0) {
                gameCamera.hominidViewTranslation -= 0.02f;
            }


            if (!HUDManager.buildingMode) {
                if (gameMode != GameMode.Tutorial || TutorialManager.CameraLocked == false) {
                    if (!HUDManager.spaceshipSelected) {
                        //No rocket selected
                        if (ms.RightButton == ButtonState.Pressed && pms.RightButton == ButtonState.Released) {
                            if (HUDManager.lastTargetObject is Hominid) {
                                //Hominid, go to the given point!
                                gameCamera.SelectPositionOrPlanet();
                            } else {
                                //Select next planet
                                BaseObject next = gameCamera.GetTargetObject(typeof(Planet));
                                if (next != null) {
                                    gameCamera.target = next;
                                }
                            }
                        }
                    } else if (ms.RightButton == ButtonState.Pressed && pms.RightButton == ButtonState.Released) {
                        //Exit from rocket selection
                        HUDManager.spaceshipSelected = false;
                    } else if (ms.LeftButton == ButtonState.Pressed && pms.LeftButton == ButtonState.Released) {
                        //Get the target for the spaceship
                        BaseObject next = gameCamera.GetTargetObject(typeof(Planet));
                        if (next != null && next is Planet) {
                            if (HUDManager.lastTargetObject != null) {
                                ((BaseObjectBuilding)HUDManager.lastTargetObject).LiftOff((Planet)next);
                            }
                            HUDManager.spaceshipSelected = false;

                            //Disable mouse catch for further selections
                            //withouth this line if you are selecting a target for a rocket
                            //you can click on a building on another planet and while the rocket
                            //lifts off, the camera select the other building
                            pms = ms;
                        }
                    }
                    //If there are more than 2 active players (user and trogloids)
                    if (PlayerManager.players.Count(p => p.state > PlayerState.Open) > 2) {
                        //Display Diplomacy
                        HUDManager.button_diplomacy.Update();
                        if (HUDManager.button_diplomacy.IsClicked()) {
                            HUDManager.diplomacyMode = !HUDManager.diplomacyMode;
                            if (HUDManager.diplomacyMode) {
                                DiplomacyManager.Start();
                            }
                        }

                        //Messages Button
                        HUDManager.button_readMessage.Update();
                    }
                    //There are no other players so everything about diplomacy is disabled
                    else {
                        HUDManager.button_diplomacy.KeepBlocked();
                        HUDManager.button_readMessage.KeepBlocked();
                        HUDManager.button_readMessage.text = "No Players";
                    }
                }
            } else {
                //Update info panel
                BuildingManager.FadingUpdate();
                //No building selected, the menu still rotates etc..
                if (BuildingManager.previewBuilding == null) {
                    BuildingManager.UpdateBuildingMenu();
                }
                //A building is selected, with the right button unselect
                else if (GameEngine.ms.RightButton == ButtonState.Pressed) {
                    BuildingManager.Clear();
                }
                //Place the building on the planet
                else {
                    BuildingManager.ManageBuilding(elapsed);
                }
                HUDManager.button_diplomacy.KeepBlocked();
                HUDManager.button_readMessage.KeepBlocked();
            }

            //Putting the code here prevents the message to not update during build mode
            int messages = PlayerManager.players[0].active_deals.Count;
            if (messages > 0 && PlayerManager.players[0].radarAmount > 0) {
                HUDManager.button_readMessage.text = "Read Message" + (messages > 1 ? "s" : "") + " (" + messages + ")";
                if (HUDManager.button_readMessage.IsClicked()) {
                    HUDManager.diplomacyMode = true;
                    DiplomacyManager.ReadMessages();
                }
            } else {
                HUDManager.button_readMessage.text = "No Messages";
            }

            //Open or close building menu
            if (!HUDManager.diplomacyMode) {
                //After sometimes, the first deal the player has in the messagebox get erased
                //This causes the AI who sent the message to receive a negative answer
                //Whatever the deal was about
                //Just because the player can't avoid answering if he wants to keep up the relationships
                if (PlayerManager.players[0].active_deals.Count > 0) {
                    HUDManager.message_timer -= elapsed * 2;
                    if (HUDManager.message_timer <= 0) {
                        //The time to answer the message has expired
                        HUDManager.message_timer = 30;
                        Deal deal = DiplomacyManager.GetFirstDeal(0);
                        PlayerManager.ChangeTrust(deal.claimer, 0, -0.005f);
                        PlayerManager.ChangeFriendship(deal.claimer, 0, -0.01f);
                        //DiplomacyManager.AnswerDeal(deal.claimer, 0, deal.action, DealResult.No);
                        PlayerManager.players[0].active_deals.RemoveAt(0);
                    }
                }

                //You access building menu only when not in diplomacy menu
                HUDManager.button_build.Update();
                if (HUDManager.button_build.IsClicked()) {
                    //Manage opening/closing of the building menu
                    if (HUDManager.buildingMode == false) {
                        if (BuildingManager.fade <= 0 && GameEngine.gameCamera.target is Planet) {
                            HUDManager.buildingMode = true;
                            BuildingManager.Start();
                        }
                    } else {
                        BuildingManager.End();
                    }
                }
            } else {
                //If you are in diplomacy mode, you cannot enter building mode
                HUDManager.button_build.KeepBlocked();
            }

            if (!HUDManager.spaceshipSelected) {
                //Now if the player is not in diplomacy mode nor building mode, update selection
                if (!HUDManager.diplomacyMode && !HUDManager.buildingMode && flag_mouseInhibition == false && gameCamera.target is Planet) {
                    //Get target planet
                    Planet ground = (Planet)gameCamera.target;

                    if (ms.LeftButton == ButtonState.Pressed && gameCamera.hominidViewTranslation <= 0) {
                        if (pms.LeftButton == ButtonState.Released) {
                            //Hominid selection
                            dragIndex = ground.hominids.IndexOf((Hominid)gameCamera.GetTargetObject(typeof(Hominid)));

                            if (dragIndex == -1) {
                                if (gameMode != GameMode.Tutorial || TutorialManager.CameraLocked == false) {
                                    //No hominid selected, search for a selected building
                                    gameCamera.GetTargetObject(typeof(BaseObjectBuilding));
                                }
                            } else if (ks.IsKeyDown(Keys.C)) {
                                //The first step get distance and hominid eventual index
                                dragDistance = Vector3.Distance(ground.hominids[dragIndex].matrix.Translation, gameCamera.position);
                                ground.hominids[dragIndex].Speak(SpeechType.Pick);
                                AudioManager.Play3D(ground, "hominid_pickup");
                            }
                        }
                        if (ks.IsKeyDown(Keys.C) && dragIndex > -1 && ground.hominids.Count > dragIndex) {
                            //After first step use drag on interested hominid
                            ground.hominids[dragIndex].Drag(dragDistance);
                        } else {
                            dragIndex = -1;
                        }
                    } else {
                        dragIndex = -1;
                    }
                }
            }
        }

        /// <summary>
        /// Update the fps according to the seconds of the date
        /// </summary>
        public static void UpdateFPS() {
            //Update FPS
            if (seconds != DateTime.Now.Second) {
                seconds = DateTime.Now.Second;
                FPS = fps;
                fps = 0;

                if (PlanetoidGame.game_screen == GameScreen.Gameplay) {
                    //Update last resource
                    HUDManager.prevKeldanym = PlayerManager.GetKeldanyum(0);
                    HUDManager.prevEnergy = PlayerManager.GetEnergy(0);

                    for (int a = 0; a < 8; a++) {
                        if (PlayerManager.players[a].state > PlayerState.Open) {
                            PlayerManager.players[a].scoreSurvivedTime++;
                        }
                    }
                }
            } else {
                fps++;
            }
        }

        /// <summary>
        /// Get a random name from the names file
        /// </summary>
        /// <returns></returns>
        public static string RandomName() {
            StreamReader sr = new StreamReader("Content//RandomGameNames.txt");
            string[] all = sr.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            return all[Util.random.Next(all.Length)];
        }

        /// <summary>
        /// Update the state of all the objects in the gameplay
        /// </summary>
        public static void UpdateGame(float elapsed) {

            /*if (ks.IsKeyDown(Keys.W) && pks.IsKeyUp(Keys.W))
            {
                if (HUDManager.lastTargetObject is Rocket)
                {
                    ManualRocket rocko= new ManualRocket(HUDManager.lastTargetObject.planet, HUDManager.lastTargetObject.matrix, 0);
                    HUDManager.lastTargetObject.planet.buildings.Remove((BaseObjectBuilding)HUDManager.lastTargetObject);
                    HUDManager.lastTargetObject = rocko;

                    HUDManager.lastTargetObject.planet.buildings.Add(rocko);
                }
            }*/
            /*if (ks.IsKeyDown(Keys.Y) && pks.IsKeyUp(Keys.Y))
            {
                (planets[0] as Sun).timer = 300;
                /*for (int a = 0; a < 400; a++)
                {
                    asteroids.Add(new Asteroid(true));
                }*/
            //}*/
            /*if (ks.IsKeyDown(Keys.Back) && pks.IsKeyUp(Keys.Back))
            {
                gameCamera.target.life = 0;
            }

            if (ks.IsKeyDown(Keys.R) && pks.IsKeyUp(Keys.R))
            {
                PlayerManager.players[0].researchLevels[0] = 5;
                PlayerManager.players[0].researchLevels[1] = 5;
                PlayerManager.players[0].researchLevels[2] = 5;
                PlayerManager.players[0].researchLevels[3] = 5;
                PlayerManager.players[0].researchLevels[4] = 5;
                PlayerManager.players[0].researchLevels[5] = 5;
                PlayerManager.SetKeldanyum(0, 30000);
                PlayerManager.SetEnergy(0, 30000);
            }

            if (ks.IsKeyDown(Keys.A) && pks.IsKeyUp(Keys.A))
            {
                asteroids.Add(new Asteroid(GameEngine.gameCamera.target.matrix.Translation + new Vector3(0, 200, 0), Vector3.Zero));
                asteroids.Last().trogloted = true;
            }

            if (ks.IsKeyDown(Keys.E) && pks.IsKeyUp(Keys.E))
            {
                ((Planet)gameCamera.target).hominids.Add(new Hominid(((Planet)gameCamera.target), 0));
            }*/

            //Update objects
            timer_asteroid -= elapsed;
            if (timer_asteroid <= 0) {
                //Astrorain mode speeds up the asteroid production
                timer_asteroid = 70 + (gameMode == GameMode.Astrorain ? -40 : Util.random.Next(140));
                //The asteroids number can't be higher than 2000
                if (asteroids.Count < 2000) {
                    //Create flying asteroids
                    for (int a = 0; a < 35; a++) {
                        asteroids.Add(new Asteroid(false));
                    }

                    if (PlayerManager.players[0].scoreSurvivedTime > 300) {
                        //The last of them will be trogloted!
                        for (int a = 1; a < Math.Min(4, 1 + PlayerManager.players[0].scoreSurvivedTime / 300); a++) {
                            if (asteroids.Count - a >= 0) {
                                asteroids[asteroids.Count - a].trogloted = true;
                                asteroids[asteroids.Count - a].life += 1;
                                asteroids[asteroids.Count - a].speed = Vector3.Normalize(planets[Util.random.Next(planets.Count)].matrix.Translation - asteroids[asteroids.Count - a].matrix.Translation) * 14;
                            }
                        }

                        if (gameMode != GameMode.Tutorial) {
                            //Create the planetoid
                            if (planetoid == null && Util.random.Next(3) == 0) {
                                planetoid = new Planetoid();
                            }
                            //Create the black hole
                            else if (blackHole == null && gameMode != GameMode.Giant && Util.random.Next(5) == 0) {
                                blackHole = new BlackHole();
                            }
                        }
                    }
                }
            }

            //Update Planetoid
            if (planetoid != null) {
                if (planetoid.Update(elapsed)) {
                    AudioManager.StopPlanetoid();
                    planetoid = null;
                }
            } else {
                AudioManager.StopPlanetoid();
            }

            //Update BlackHole
            if (blackHole != null) {
                if (blackHole.Update(elapsed)) {
                    blackHole = null;
                }
            }

            //Update Asteroids
            DataSectorSubdivider.ResetSectorsData();
            for (int a = 0; a < asteroids.Count; a++) {
                if (asteroids[a].Update(elapsed)) {
                    asteroids.RemoveAt(a);
                    a--;
                } else {
                    DataSectorSubdivider.RegisterObject(asteroids[a]);
                }
            }

            //Update Sun
            (planets[0] as Sun).Update(elapsed);

            //Update Planets
            Planet p;
            Hominid h;
            for (int a = 1; a < planets.Count(); a++) {
                p = planets[a];
                if (p.Update(elapsed)) {
                    planets.RemoveAt(a);
                    a--;
                } else {
                    //Update Hominids
                    for (int b = 0; b < p.hominids.Count(); b++) {
                        h = p.hominids[b];
                        if (h.Update(elapsed)) {
                            if (h.life <= 0) {
                                //The player loses point
                                PlayerManager.players[h.owner].LostHominid();
                                if (h.owner == 8) {
                                    //The troglothers will scream on death!
                                    AudioManager.Play3D(h, "trogloid_death");
                                    if (h is Troglother) {
                                        AudioManager.Play3D(h, "troglother_death");
                                        GameEngine.tsmokeParticles.SetColor(Color.Fuchsia);
                                        for (int t = 0; t < 100; t++) {
                                            GameEngine.tsmokeParticles.AddParticle(h.matrix.Translation + Util.RandomPointOnSphere(10), Vector3.Zero);
                                        }
                                        GameEngine.tsmokeParticles.SetColor(Color.White);
                                    }
                                } else {
                                    //The hominid screams
                                    AudioManager.Play3D(h, "death_scream");
                                }
                                h.SmokeMark();
                                if (h.Ability == 9) {
                                    PlayerManager.ChangeEnergy(h.owner, 40);
                                } else if (h.Ability == 10) {
                                    PlayerManager.ChangeKeldanyum(h.owner, 40);
                                }
                            }

                            //Exit from dragging
                            if (b == dragIndex) {
                                dragIndex = -1;
                                HUDManager.lastTargetObject = null;
                            }
                            p.hominids.RemoveAt(b);
                            b--;
                        }
                    }

                    //Update Trees
                    for (int b = 0; b < p.trees.Count(); b++) {
                        if (p.trees[b].Update(elapsed)) {
                            p.trees.RemoveAt(b);
                            b--;
                        }
                    }

                    //Update Buildings
                    for (int b = 0; b < p.buildings.Count; b++) {
                        if (p.buildings[b].Update(elapsed)) {
                            if (p.buildings[b].life <= 0) {
                                if (HUDManager.lastTargetObject == p.buildings[b]) {
                                    HUDManager.lastTargetObject = null;
                                }
                                if (p.buildings[b] is Radar) {
                                    PlayerManager.players[p.buildings[b].owner].radarAmount--;
                                }
                                p.buildings[b].LeaveHominid();
                                p.buildings[b].Burst(20, Color.OrangeRed, 10);
                                AudioManager.Play3D(p.buildings[b], "building_explode");
                                gameCamera.shake += 100 / Vector3.Distance(p.buildings[b].matrix.Translation, GameEngine.gameCamera.position);
                            }
                            p.buildings.RemoveAt(b);
                            b--;
                        }
                        /*else
                        {
                            DataSectorSubdivider.RegisterObject(planets[a].buildings[b]);
                        }*/
                    }
                }
            }

            //Update Turrets Shots
            for (int a = 0; a < shots.Count; a++) {
                if (shots[a].Update(elapsed)) {
                    shots.RemoveAt(a);
                    a--;
                }
            }

            //Update Baloons
            for (int a = 0; a < balloons.Count; a++) {
                if (balloons[a].Update(elapsed)) {
                    balloons.RemoveAt(a);
                    a--;
                }
            }

            //DataSectorSubdivider.UpdateSectorsData();

            if (MessageBox.Title == "Finished" && MessageBox.lastResult == MessageBoxResult.Ok) {
                MessageBox.Reset();
                PlanetoidGame.game_screen_next = GameScreen.Result;
                PlanetoidGame.elapsed = 0;
                AudioManager.battleIsHappening = 0;
                for (int a = 0; a < PlayerManager.players.Length - 1; a++) {
                    PlayerManager.players[a].CalculateScore();
                }
                PlayerManager.CalculateResults();
            }
        }

        /// <summary>
        /// Prepare all the particles for being drawn
        /// </summary>
        public static void SetParticlesForDrawing() {
            fireParticles.SetCamera(gameCamera);
            explosionParticles.SetCamera(gameCamera);
            planetoidParticles.SetCamera(gameCamera);
            tsmokeParticles.SetCamera(gameCamera);
            smokeParticles.SetCamera(gameCamera);
        }

        /// <summary>
        /// Save the game state
        /// </summary>
        public static void SaveGameState(string savename) {
            // Get the path of the save game
            savename = MenuManager.GetSavePath(savename + ".xml");
            File.Delete(savename);
            // Open the file, creating it if necessary
            XmlTextWriter writer = new XmlTextWriter(savename, null);
            writer.WriteStartDocument();
            writer.WriteStartElement("SaveGame");

            //<PLANETS>
            writer.WriteStartElement("Planets");
            foreach (Planet planet in planets) {
                planet.InSerialization();
            }
            XmlSerializer serializer = new XmlSerializer(typeof(List<Planet>));
            serializer.Serialize(writer, planets);
            foreach (Planet planet in planets) {
                planet.OutSerialization();
            }
            writer.WriteEndElement();
            //</PLANETS>

            //<BUILDINGS>
            /*writer.WriteStartElement("Buildings");
            foreach (BaseObjectBuilding building in buildings)
            {
                building.InSerialization();
            }
            serializer = new XmlSerializer(typeof(List<BaseObjectBuilding>));
            serializer.Serialize(writer, buildings);
            foreach (BaseObjectBuilding building in buildings)
            {
                building.OutSerialization();
            }
            writer.WriteEndElement();*/
            //</BUILDINGS>

            //<SUNSHOTS>
            writer.WriteStartElement("Sunshots");
            serializer = new XmlSerializer(typeof(List<Sunshot>));
            serializer.Serialize(writer, ((Sun)planets[0]).sunShots);
            writer.WriteEndElement();
            //</SUNSHOTS>

            //<ASTEROIDS>
            writer.WriteStartElement("Asteroids");
            foreach (Asteroid asteroid in asteroids) {
                asteroid.InSerialization();
            }
            serializer = new XmlSerializer(typeof(List<Asteroid>));
            serializer.Serialize(writer, asteroids);
            foreach (Asteroid asteroid in asteroids) {
                asteroid.OutSerialization();
            }
            writer.WriteEndElement();
            //</ASTEROIDS>

            //<PLANETOID>
            writer.WriteStartElement("Planetoid");
            if (planetoid != null) {
                planetoid.InSerialization();
            }
            serializer = new XmlSerializer(typeof(Planetoid));
            serializer.Serialize(writer, planetoid);
            if (planetoid != null) {
                planetoid.OutSerialization();
            }
            writer.WriteEndElement();
            //</PLANETOID>

            //<BLACK HOLE>
            writer.WriteStartElement("Blackhole");
            if (blackHole != null) {
                blackHole.InSerialization();
            }
            serializer = new XmlSerializer(typeof(BlackHole));
            serializer.Serialize(writer, blackHole);
            writer.WriteEndElement();
            //</BLACK HOLE>

            //<SHOTS>
            writer.WriteStartElement("Shots");
            foreach (Shot shot in shots) {
                shot.InSerialization();
            }
            serializer = new XmlSerializer(typeof(List<Shot>));
            serializer.Serialize(writer, shots);
            writer.WriteEndElement();
            //</SHOTS>

            //ALL INGAME OBJECTS HAVE BEEN SAVED
            //NOW PLAYERS

            //<PLAYERS>
            writer.WriteStartElement("Players");
            serializer = new XmlSerializer(typeof(SerializablePlayerManager));
            SerializablePlayerManager spm = PlayerManager.InSerialization();
            serializer.Serialize(writer, spm);
            PlayerManager.OutSerialization(spm);
            writer.WriteEndElement();
            //</PLAYERS>

            //<CAMERA>
            writer.WriteStartElement("Camera");
            gameCamera.InSerialization();
            serializer = new XmlSerializer(typeof(Camera));
            serializer.Serialize(writer, gameCamera);
            gameCamera.OutSerialization();
            writer.WriteEndElement();
            //</CAMERA>

            //<RESEARCHES>
            writer.WriteStartElement("Researches");
            serializer = new XmlSerializer(typeof(List<ResearchButton>));
            serializer.Serialize(writer, BuildingManager.researchList);
            writer.WriteEndElement();
            //</RESEARCHES>

            //<CAMPAIGN>
            writer.WriteStartElement("Story");
            writer.WriteStartAttribute("Index");
            writer.WriteValue(TutorialManager.MissionIndex);
            writer.WriteEndAttribute();
            writer.WriteStartAttribute("Name");
            writer.WriteValue(TutorialManager.MissionName);
            writer.WriteEndAttribute();
            writer.WriteStartAttribute("Descr");
            writer.WriteValue(TutorialManager.MissionDescription);
            writer.WriteEndAttribute();
            writer.WriteStartAttribute("Locked");
            writer.WriteValue(TutorialManager.CameraLocked);
            writer.WriteEndAttribute();
            writer.WriteStartAttribute("TPlanet");
            writer.WriteValue((TutorialManager.tplanet == null ? -1 : planets.IndexOf(TutorialManager.tplanet)));
            writer.WriteEndAttribute();
            writer.WriteEndElement();
            //</CAMPAIGN>

            //<GAME>
            writer.WriteStartElement("Game");
            writer.WriteStartAttribute("mode");
            writer.WriteValue(gameMode.ToString());
            writer.WriteEndAttribute();
            writer.WriteStartAttribute("currSec");
            writer.WriteValue(BuildingManager.currentSecond);
            writer.WriteEndAttribute();
            writer.WriteStartAttribute("totaSec");
            writer.WriteValue(BuildingManager.totalSeconds);
            writer.WriteEndAttribute();
            writer.WriteStartAttribute("races");
            string addedRaces = "";
            for (int a = 20; a < RaceManager.TotalRaces; a++) {
                addedRaces += RaceManager.GetRace(a);
                if (a < RaceManager.TotalRaces - 1) {
                    addedRaces += ":";
                }
            }
            writer.WriteValue(addedRaces);
            writer.WriteEndAttribute();
            writer.WriteEndElement();
            //</GAME>

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();

            //Encrypt File
            DataManager.Encrypt(savename);
            MessageBox.ShowDialog("Saved", "The game has been saved!", 0);
        }

        /// <summary>
        /// Load the game state
        /// </summary>
        public static void LoadGameState(string savename) {
            currentGameName = savename;
            // Get the path of the save game
            savename = MenuManager.GetSavePath(savename + ".xml");

            //Decrypt File
            DataManager.Decrypt(savename);

            // Open the file
            //FileStream stream = File.Open(fullpath, FileMode.OpenOrCreate,FileAccess.Read);
            MenuManager.document.Load(savename);

            //<PLANETS>
            //Declare the serializer type to get
            XmlSerializer serializer = new XmlSerializer(typeof(List<Planet>));
            //Use a string reader to get the stream from the xml element
            StringReader sr = new StringReader(MenuManager.document.GetElementsByTagName("Planets")[0].InnerXml);
            //Deserialize
            planets = (List<Planet>)serializer.Deserialize(sr);
            foreach (Planet planet in planets) {
                planet.OutSerialization();
            }
            //Finally close the stream
            sr.Close();
            //</PLANETS>

            //<BUILDINGS>
            /* serializer = new XmlSerializer(typeof(List<BaseObjectBuilding>));
             sr = new StringReader(MenuManager.document.GetElementsByTagName("Buildings")[0].InnerXml);
            buildings = (List<BaseObjectBuilding>)serializer.Deserialize(sr);
            foreach (BaseObjectBuilding building in buildings)
            {
                building.OutSerialization();
            }
            sr.Close();*/
            //</BUILDINGS>

            //<SUNSHOTS>
            serializer = new XmlSerializer(typeof(List<Sunshot>));
            sr = new StringReader(MenuManager.document.GetElementsByTagName("Sunshots")[0].InnerXml);
            ((Sun)planets[0]).sunShots = (List<Sunshot>)serializer.Deserialize(sr);
            sr.Close();
            //</SUNSHOTS>

            //<ASTEROIDS>
            serializer = new XmlSerializer(typeof(List<Asteroid>));
            sr = new StringReader(MenuManager.document.GetElementsByTagName("Asteroids")[0].InnerXml);
            asteroids = (List<Asteroid>)serializer.Deserialize(sr);
            sr.Close();
            //</ASTEROIDS>

            //<PLANETOID>
            serializer = new XmlSerializer(typeof(Planetoid));
            sr = new StringReader(MenuManager.document.GetElementsByTagName("Planetoid")[0].InnerXml);
            planetoid = (Planetoid)serializer.Deserialize(sr);
            sr.Close();
            //</PLANETOID>

            //<BLACK HOLE>
            serializer = new XmlSerializer(typeof(BlackHole));
            sr = new StringReader(MenuManager.document.GetElementsByTagName("Blackhole")[0].InnerXml);
            blackHole = (BlackHole)serializer.Deserialize(sr);
            sr.Close();
            //</BLACK HOLE>

            //<SHOTS>
            serializer = new XmlSerializer(typeof(List<Shot>));
            sr = new StringReader(MenuManager.document.GetElementsByTagName("Shots")[0].InnerXml);
            shots = (List<Shot>)serializer.Deserialize(sr);
            sr.Close();
            //</SHOTS>

            //ALL INGAME OBJECTS HAVE BEEN LOADED

            //<PLAYERS>
            serializer = new XmlSerializer(typeof(SerializablePlayerManager));
            sr = new StringReader(MenuManager.document.GetElementsByTagName("Players")[0].InnerXml);
            PlayerManager.OutSerialization((SerializablePlayerManager)serializer.Deserialize(sr));
            sr.Close();
            //</PLAYERS>

            //<CAMERA>
            serializer = new XmlSerializer(typeof(Camera));
            sr = new StringReader(MenuManager.document.GetElementsByTagName("Camera")[0].InnerXml);
            gameCamera = (Camera)serializer.Deserialize(sr);
            gameCamera.OutSerialization();
            sr.Close();
            //</CAMERA>

            //<RESEARCHES>
            serializer = new XmlSerializer(typeof(List<ResearchButton>));
            sr = new StringReader(MenuManager.document.GetElementsByTagName("Researches")[0].InnerXml);
            BuildingManager.researchList = (List<ResearchButton>)serializer.Deserialize(sr);
            //</RESEARCHES>

            //<CAMPAIGN>
            TutorialManager.MissionIndex = int.Parse(MenuManager.document.GetElementsByTagName("Story")[0].Attributes[0].Value);
            TutorialManager.MissionName = MenuManager.document.GetElementsByTagName("Story")[0].Attributes[1].Value;
            TutorialManager.MissionDescription = MenuManager.document.GetElementsByTagName("Story")[0].Attributes[2].Value;
            TutorialManager.CameraLocked = bool.Parse(MenuManager.document.GetElementsByTagName("Story")[0].Attributes[3].Value);
            int index = int.Parse(MenuManager.document.GetElementsByTagName("Story")[0].Attributes[4].Value);
            TutorialManager.tplanet = (index == -1 ? null : planets[index]);
            //</CAMPAIGN>

            //<GAME>
            GameEngine.gameMode = (GameMode)Enum.Parse(typeof(GameMode), MenuManager.document.GetElementsByTagName("Game")[0].Attributes[0].Value);
            BuildingManager.currentSecond = int.Parse(MenuManager.document.GetElementsByTagName("Game")[0].Attributes[1].Value);
            BuildingManager.totalSeconds = int.Parse(MenuManager.document.GetElementsByTagName("Game")[0].Attributes[2].Value);

            //FIX RACES
            string[] addedRaces = MenuManager.document.GetElementsByTagName("Game")[0].Attributes[3].Value.Split(new char[] { ':' });
            bool notify = false;
            if (addedRaces.Length > 0) {
                for (int a = 0; a < addedRaces.Length; a++) {
                    if (RaceManager.ExistRace(addedRaces[a]) == false) {
                        for (int b = 0; b < 8; b++) {
                            if (PlayerManager.GetRace(b) == 20 + a) {
                                notify = true;
                                PlayerManager.GetNextRace(b);
                            }
                        }
                    }
                }
            }
            if (notify) {
                MessageBox.ShowDialog("Warning!", "One or more custom races used in this game are missing!\nRandom races will be picked.", 0);
            }
            //</GAME>

            //Encrypt File
            DataManager.Encrypt(savename);
        }

        /// <summary>
        /// Draw all the planets atmospheres
        /// </summary>
        public static void DrawAtmospheres() {
            if (HUDManager.strategyMode == false) {
                Game.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                Game.GraphicsDevice.BlendState = BlendState.Additive;
                foreach (Planet planet in planets) {
                    if (planet.atmosphere != Atmosphere.None) {
                        Util.DrawCircle(planet.matrix.Translation, planet.radius * (1 + planet.atmosphere_graphic / 70f), (planet.name == "Sun" ? planet.color : planet.GetColor()), GameEngine.gameCamera);
                    }
                }
            }
        }

        /// <summary>
        /// Draw all the objects in the gameplay
        /// </summary>
        /// <param name="elapsed"></param>
        public static void DrawGame(float elapsed) {
            DrawAtmospheres();

            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Game.GraphicsDevice.BlendState = BlendState.Opaque;

            RenderManager.DrawAllAsteroids();

            int index;
            //Draw Shots
            for (index = 0; index < shots.Count; index++) {
                shots[index].Draw();
            }

            //Draw Planets
            foreach (Planet planet in planets) {
                if (planet.IsVisible()) {
                    planet.DrawPlanet(gameCamera, RenderManager.planetShader);

                    //Draw Trees
                    foreach (Tree tree in planet.trees) {
                        if (!tree.gaseous) {
                            tree.Draw();
                        }
                    }

                    Game.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

                    //Draw Hominids
                    foreach (Hominid hominid in planet.hominids) {
                        hominid.Draw();
                    }
                }
                //Draw Buildings
                foreach (BaseObjectBuilding building in planet.buildings) {
                    building.Draw();
                }
            }

            Game.GraphicsDevice.BlendState = BlendState.Additive;


            //Draw Planets orbits
            Util.DrawOrbits(GameEngine.gameCamera);

            // Util.DrawGrid(GameEngine.gameCamera);

            //Draw Planetoid
            if (planetoid != null) {
                planetoid.Draw();
            }

            for (int a = 0; a < planets.Count; a++) {
                //Draw Buildings
                for (int b = 0; b < planets[a].buildings.Count; b++) {
                    planets[a].buildings[b].SecondDraw();
                }
            }

            //Draw Building Model
            if (HUDManager.buildingMode) {
                Game.GraphicsDevice.BlendState = BlendState.Additive;
                for (int a = 0; a < (GameEngine.gameCamera.target as Planet).buildings.Count; a++) {
                    if ((GameEngine.gameCamera.target as Planet).buildings[a].flying == false) {
                        Util.DrawBuildingCircle((GameEngine.gameCamera.target as Planet).buildings[a], gameCamera, Color.Lime);
                    }
                }
                //Draw the building model on the planet
                if (BuildingManager.preBuildPosition != Vector3.Zero) {
                    Util.DrawBuildingCircle(BuildingManager.previewBuilding, gameCamera, (!BuildingManager.canBuild ? Color.Red : Color.Lime));
                    Game.GraphicsDevice.BlendState = BlendState.Opaque;
                    /*if (!BuildingManager.canBuild)
                    {
                        Game.GraphicsDevice.BlendState = BlendState.Additive;
                    }*/
                    BuildingManager.previewBuilding.Draw();
                    //Game.GraphicsDevice.BlendState = BlendState.Opaque;
                }
                Game.GraphicsDevice.BlendState = BlendState.Opaque;
            }
        }

        /// <summary>
        /// Draw the fader upon everything
        /// </summary>
        public static void DrawFader(SpriteBatch spriteBatch) {
            if (MessageBox.IsActive) {
                if (PlanetoidGame.game_screen_fader < 0.5f) {
                    PlanetoidGame.game_screen_fader += 0.01f;
                }
            } else {
                //Draw Fader Screen
                if (PlanetoidGame.game_screen != PlanetoidGame.game_screen_next) {
                    if (PlanetoidGame.game_screen_fader < 1) {
                        PlanetoidGame.game_screen_fader += 0.01f;
                    } else {
                        PlanetoidGame.game_screen = PlanetoidGame.game_screen_next;
                    }
                } else {
                    if (PlanetoidGame.game_screen_fader > 0) {
                        PlanetoidGame.game_screen_fader -= 0.01f;
                    }
                }
            }
            if (PlanetoidGame.game_screen_fader > 0) {
                spriteBatch.Draw(HUDManager.panel_info, new Rectangle(-50, -50, PlanetoidGame.scene.Width + 100, PlanetoidGame.scene.Height + 100), new Color(0, 0, 0, PlanetoidGame.game_screen_fader));
            }
        }
    }
}
