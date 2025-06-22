using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
namespace Planetoid3D
{
    /// <summary>
    /// Tutorial Manager, last review on version 1.0.2
    /// </summary>
    public static class TutorialManager
    {
        public static void Initialize()
        {
            MissionIndex = 0;
            CinematicMode = false;
            TextToDisplay = "";
            visible = true;
        }

        public static int MissionIndex;
        public static bool CinematicMode;
        public static float CinematicFade;
        public static bool CameraLocked;

        public static string MissionName;
        public static string MissionDescription;

        private static string TextToDisplay;
        
        private static bool beginMission;
        private static TimeSpan cinematicSpan;

        public static Planet tplanet;

        public static bool visible;

        private static BaseObjectBuilding buildingActor;

        public static void CheckProgresses(GameTime gameTime)
        {
            //Campaigns are precompiled
            switch (MissionIndex)
            {
                    //BUILD A HOUSE
                #region MISSION 0
                case 0:
                    if (beginMission)
                    {
                        MissionName = "Intro";
                        MissionDescription = "|G| - Build a house\n|W| - Use B to enter in the menu mode and place it on the planet.\n - Right Mouse Button to cancel building.\n - Middle Mouse Button to move camera view.";
                        CinematicMode = true;
                        cinematicSpan = new TimeSpan(0, 0, 30);
                        beginMission = false;
                        CameraLocked = true;
                        //Look at sun
                        GameEngine.gameCamera.target = GameEngine.planets[0];
                        GameEngine.gameCamera.verticalAngle = 0.2f;
                        Planet planet = GameEngine.planets.Find(p => p.hominids.Count > 0);
                        planet.hominids.Add(new Hominid(planet, 0));
                        planet.buildings.Add(new Solar(planet, BaseObjectBuilding.GetFullPlacedMatrix(Util.RandomPointOnSphere(planet.radius), planet), 0));
                        planet.buildings.Add(new LKE(planet, BaseObjectBuilding.GetFullPlacedMatrix(Util.RandomPointOnSphere(planet.radius), planet), 0));
                        planet.buildings.Add(new Extractor(planet, BaseObjectBuilding.GetFullPlacedMatrix(Util.RandomPointOnSphere(planet.radius), planet), 0));
                        planet.buildings.Add(new Turbina(planet, BaseObjectBuilding.GetFullPlacedMatrix(Util.RandomPointOnSphere(planet.radius), planet), 0));
                        PlayerManager.players[0].researchLevels[0] = 2;
                        PlayerManager.players[0].researchLevels[2] = 2;
                        PlayerManager.players[0].researchLevels[3] = 2;
                        PlayerManager.players[0].researchLevels[4] = 4;
                        GameEngine.planets[4].atmosphere = Atmosphere.Oxygen;
                        GameEngine.planets[4].AddTrees();
                    }
                    else if ((GameEngine.gameCamera.target as Planet).buildings.Exists(b => b is House))
                    {
                        buildingActor = (GameEngine.gameCamera.target as Planet).buildings.Find(b => b is House);
                        BeginMission(1);
                    }
                    //Check condition
                    else if (cinematicSpan > new TimeSpan(0, 0, 25))
                    {
                        TextToDisplay = "The sun, our fabulous space lamp!";
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 15))
                    {
                        TextToDisplay = "Everything, from trees to the camera we are using now is powered by this amazing nuclear reactor!";
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 5))
                    {
                        TextToDisplay = "But we are not going to watch this fireball forever, aren't we?";
                        if (GameEngine.gameCamera.horizontalAngle < 1)
                        {
                            GameEngine.gameCamera.horizontalAngle += 0.002f;
                            GameEngine.gameCamera.verticalAngle += 0.002f;
                            GameEngine.gameCamera.targetZoom = 500;
                        }
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 0))
                    {
                        TextToDisplay = "This is our planet, so gorgeous and amazing!\nEveryone is happy, but we need a new house!\nHelp us!";
                        GameEngine.gameCamera.target = GameEngine.planets.Find(p => p.hominids.Count > 0);
                        GameEngine.gameCamera.horizontalAngle += 0.01f;
                        GameEngine.gameCamera.targetZoom -= 0.5f;
                    }
                    break;
                #endregion
                    //LAUNCH A ROCKET
                #region MISSION 1
                case 1:
                    if (beginMission)
                    {
                        MissionName = "And we looked upon our heads";
                        MissionDescription = "|G| - Build a rocket and fill it with three hominids!\n|W| - Remember, you can keep pressed C and click on a Hominid to drag it wherever you want!";
                        cinematicSpan = new TimeSpan(0, 0, 20);
                        //Create planetoid
                        GameEngine.planetoid = new Planetoid();
                        beginMission = false;
                        CameraLocked = true;
                        if (Vector3.Distance(GameEngine.planetoid.matrix.Translation, GameEngine.gameCamera.target.matrix.Translation) > 2000)
                        {
                            GameEngine.planetoid.matrix.Translation = GameEngine.gameCamera.target.matrix.Translation + Util.RandomPointOnSphere(2000);
                        }
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 15))
                    {
                        GameEngine.gameCamera.targetZoom -= 1;
                        HUDManager.lastTargetObject = (GameEngine.gameCamera.target as Planet).buildings.Find(b => b is House);
                        TextToDisplay = "Okay, now we have a house!";
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 5))
                    {
                        HUDManager.lastTargetObject = null;
                        GameEngine.gameCamera.target = GameEngine.planetoid;
                        GameEngine.gameCamera.targetZoom = 150;
                        TextToDisplay = "Oh no!\nA planetoid is approaching to our planet!";
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 0))
                    {
                        GameEngine.gameCamera.targetZoom += 2;
                        GameEngine.gameCamera.target = GameEngine.planets.Find(p => p.hominids.Count > 0);
                        TextToDisplay = "Build a rocket and fly away!";
                    }
                    else if ((GameEngine.gameCamera.target as Planet).buildings.Exists(b => b is Rocket && ((Rocket)b).passengersCount == 3))
                    {
                        HUDManager.lastTargetObject = (GameEngine.gameCamera.target as Planet).buildings.Find(b => b is Rocket);
                        Rocket rocket = (Rocket)HUDManager.lastTargetObject;
                        rocket.LiftOff(GameEngine.planets[4]);
                        BeginMission(2);
                    }
                    GameEngine.planetoid.speed = buildingActor.planet.matrix.Translation - GameEngine.planetoid.matrix.Translation;
                    break;
                #endregion
                    //GET A RADAR
                #region MISSION 2
                case 2:
                    if (beginMission)
                    {
                        MissionName = "Again, as the first title";
                        MissionDescription = "|G| - Build a radar.\n|W| - You can access the research menu from the building one.\n - Click on an active research to cancel it and gain half of its used resources.\n - Right Mouse Button on a planet to lock view on it!\n - Scroll wheel to zoom.\n - Right Mouse Button on the locked planet to full zoom on it!\n - Click on buildings or hominids to view info.";
                        
                        beginMission = false;
                        CameraLocked = false;
                    }
                    else if (PlayerManager.players[0].radarAmount>0)
                    {
                        BeginMission(3);
                    }
                    else if (HUDManager.lastTargetObject is Rocket)
                    {
                        TextToDisplay = "Everyone on the rocket was upset...\nAll they wanted was to land safely on an oxygen planet...\nNobody looked back at the tragedy..";
                        //GameEngine.gameCamera.horizontalAngle += 0.001f;
                        cinematicSpan = new TimeSpan(0, 0, 5);
                        HUDManager.spaceshipSelected = false;
                        HUDManager.lastTargetObject.matrix.Translation += HUDManager.lastTargetObject.matrix.Backward;

                        //Vector3 dir = Vector3.Normalize(GameEngine.gameCamera.target.matrix.Translation - HUD.lastTargetObject.matrix.Translation);

                       // Vector3 pos=GameEngine.gameCamera.

                        //Camera position updating
                        /*position.X = tempTar.X + (float)(Math.Cos(horizontalAngle) * zoom);
                        position.Y = tempTar.Y + (float)(verticalAngle * zoom / 3);
                        position.Z = tempTar.Z + (float)(Math.Sin(horizontalAngle) * zoom);
                        */
                        //Camera view updating
                        /*viewMatrix = Matrix.CreateLookAt(
                             position,
                             tempTar,
                             upV);*/

                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 0))
                    {
                        TextToDisplay = "You have to help them rebuild everything...";
                        GameEngine.gameCamera.horizontalAngle += 0.01f;
                    }
                    if (buildingActor!=null && buildingActor.planet.life > 0)
                    {
                        GameEngine.planetoid.speed = buildingActor.planet.matrix.Translation - GameEngine.planetoid.matrix.Translation;
                    }
                    break;
                #endregion
                    //DESTROY THE OPPONENT OR ALLY
                #region MISSION 3
                case 3:
                    if (beginMission)
                    {
                        MissionName = "Why you hate us so much?";
                        MissionDescription = "|G| - Destroy the Aliens or create an alliance with them.\n|W| - Use the diplomacy menu to communicate.\n - When selecting a hominid, you can give orders right-clicking somewhere on the planet!.";
                        cinematicSpan = new TimeSpan(0, 0,30);
                        beginMission = false;
                        CameraLocked = false;

                        PlayerManager.players[1].race = 2;
                        PlayerManager.players[1].state = PlayerState.Dumb;
                        PlayerManager.players[1].cpuController = new AIPlayer(0.4f, 0.1f, 0.5f, 1);
                        PlayerManager.CreateDiplomacies();
                    }
                    else if (PlayerManager.IsDefeated(1) || PlayerManager.GetDualState(0, 1) == DualState.Alliance)
                    {
                        BeginMission(4);
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 20))
                    {
                        GameEngine.gameCamera.target = PlayerManager.players[1].cpuController.myPlanets[0].planet;
                        TextToDisplay = "Hey, there are aliens on that planet!";
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 10))
                    {
                        GameEngine.gameCamera.targetZoom = 150;
                        TextToDisplay = "I remember that they were extremely nasty!!";
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 0))
                    {
                        TextToDisplay = "You must decide what is better for our colony!";
                    }
                    break;
                #endregion
                    //DESTROY THE NEW MENACE
                #region MISSION 4
                case 4:
                    if (beginMission)
                    {
                        MissionName = "Sulphos Rules";
                        MissionDescription = "|G| - Annihilate the Sulpho civilization!";
                        cinematicSpan = new TimeSpan(0, 0, 25);
                        beginMission = false;
                        CameraLocked = false;

                        PlayerManager.players[2].race = 5;//sulpho
                        PlayerManager.players[2].state = PlayerState.Normal;
                        PlayerManager.players[2].cpuController = new AIPlayer(1.0f, 0.5f, 0.0f, 2);
                        PlayerManager.CreateDiplomacy(2);
                        //The new AI is in anger with everyone
                        PlayerManager.SetAll(2, 0, -1);
                        PlayerManager.SetAll(2, 1, -1);
                    }
                    else if (PlayerManager.IsDefeated(2))
                    {
                        BeginMission(5);
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 20))
                    {
                        if (PlayerManager.players[1].state == PlayerState.Close)
                        {
                            TextToDisplay = "Okay, Aliens were really easy to destroy!";
                        }
                        else
                        {
                            TextToDisplay = "Pacifism is always the best choice!\nCongratulations (really!)";
                        }
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 10))
                    {
                        TextToDisplay = "But now there is a new civilization in this system...";
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 5))
                    {
                        GameEngine.gameCamera.target = PlayerManager.players[2].cpuController.myPlanets[0].planet;
                        TextToDisplay = "The Sulphos!";
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 0))
                    {
                        GameEngine.gameCamera.horizontalAngle -= 0.01f;
                        GameEngine.gameCamera.targetZoom -= 0.1f;
                        TextToDisplay = "They will have no qualms, be careful!!";
                    }
                    break;
                #endregion
                    //TROGLOTHERS
                #region MISSION 5
                case 5:
                    if (beginMission)
                    {
                        MissionName = "The Fuchsia Menace";
                        MissionDescription = "|G| - Survive the Troglothers attack\n|W| - Use the PrintScreen key to take screenshots!\n - H to enter in hominidic view!";
                        cinematicSpan = new TimeSpan(0, 0, 15);
                        beginMission = false;
                        CameraLocked = false;
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 10))
                    {
                        TextToDisplay="Sulphos were a real pain in the neck, weren't they?";
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 5))
                    {
                        GameEngine.gameCamera.target = GameEngine.planets[0];
                        TextToDisplay = "Now the real menace is not a civilization, it's something more dangerous!";
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 0))
                    {
                        if (GameEngine.gameCamera.target is Sun)
                        {                    
                            GameEngine.gameCamera.target = GameEngine.planets.Find(p => p.DominatingRace() == 0);
                            //GameEngine.gameCamera.target.life += 5;
                            //GameEngine.asteroids.Add(new Asteroid(GameEngine.gameCamera.target.matrix.Translation + new Vector3(0, 100, 0), Vector3.Zero));
                            //GameEngine.asteroids.Last().trogloted = true;
                            tplanet = (Planet)GameEngine.gameCamera.target;
                            tplanet.hominids.Add(new Troglother(tplanet, null));
                        }
                        TextToDisplay = "I hope you'll survive!";
                    }
                    else if (tplanet == null || tplanet.life <= 0 || tplanet.hominids.Exists(h => h is Troglother || h.Race == 18) == false)
                    {
                        BeginMission(6);
                    }
                    break;
                #endregion
                    //FINAL BATTLE  
                #region MISSION 6
                case 6:
                    if (beginMission)
                    {
                        MissionName = "Final Battle";
                        MissionDescription = "|G| - Destroy all of your opponents!";
                        cinematicSpan = new TimeSpan(0, 0, 40);
                        beginMission = false;
                        CameraLocked = false;

                        //Two powerful AIs
                        PlayerManager.players[3].race = 4;//Rablosh
                        PlayerManager.players[3].state = PlayerState.Challenging;
                        PlayerManager.players[3].cpuController = new AIPlayer(1.0f, 1.0f, 0.0f, 3);
                        PlayerManager.CreateDiplomacy(3);
                        PlayerManager.players[4].race = 8;//Cyclops
                        PlayerManager.players[4].state = PlayerState.Normal;
                        PlayerManager.players[4].cpuController = new AIPlayer(1.0f, 1.0f, 0.0f, 4);
                        PlayerManager.CreateDiplomacy(4);
                        PlayerManager.SetAll(3, 0, -1);
                        PlayerManager.SetAll(3, 1, -1);

                        PlayerManager.SetAll(4, 0, -1);
                        PlayerManager.SetAll(4, 1, -1);

                        //Allied
                        PlayerManager.SetAll(4, 3, 1);
                        PlayerManager.SetAll(3, 4, 1);
                    }
                    else if (PlayerManager.IsDefeated(3) && PlayerManager.IsDefeated(4))
                    {
                        BeginMission(7);
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 35))
                    {
                        TextToDisplay = "That was amazing, I just went away for a coffee and you killed them!!";
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 25))
                    {
                        TextToDisplay = "This tutorial is almost finished... I know, this is sad";
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 20))
                    {
                        TextToDisplay = "But first you still have to prove your skills!";
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 15))
                    {
                        TextToDisplay = "These are the Rabloshes";
                        GameEngine.gameCamera.target = PlayerManager.players[3].cpuController.myPlanets[0].planet;
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 10))
                    {
                        TextToDisplay = "And these are the Cyclops";
                        GameEngine.gameCamera.target = PlayerManager.players[4].cpuController.myPlanets[0].planet;
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 0))
                    {
                        TextToDisplay = "Destroy them and remember... They're allied together!";
                    }

                    break;
                #endregion
                    //CONGRATS
                #region MISSION 7
                case 7:
                    if (beginMission)
                    {
                        MissionName = "The End";
                        MissionDescription = "";
                        cinematicSpan = new TimeSpan(0, 0, 30);
                        beginMission = false;
                        CameraLocked = false;
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 25))
                    {
                        TextToDisplay = "Oh wow, congratulations! You Did it!!";
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 20))
                    {
                        TextToDisplay = "You managed to defeat all the threats you encountered!\nI'm so proud of you!";
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 15))
                    {
                        TextToDisplay = "Unfortunately the tutorial ends here, it wasn't that long right?";
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 10))
                    {
                        TextToDisplay = "Well, now you should play some Skirmish, why not?";
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 5))
                    {
                        TextToDisplay = "There are some amazing game modes I bet you haven't seen yet U_U";
                    }
                    else if (cinematicSpan > new TimeSpan(0, 0, 0))
                    {
                        TextToDisplay = "Enjoy the credits, my companion!";
                    }
                    else
                    {
                        //Go for the credits
                        PlanetoidGame.game_screen_next = GameScreen.Credits;
                        PlanetoidGame.elapsed = 0;
                        QuestManager.QuestCall(17);
                    }
                    break;
                #endregion
            }

            //Update the fading of the cinematic mode
            if (CinematicMode)
            {
                if (GameEngine.gameCamera.hominidViewTranslation > 0)
                {
                    GameEngine.gameCamera.hominidViewTranslation -= 0.02f;
                }
                if (CinematicFade < 1)
                {
                    CinematicFade += 0.01f;
                }
            }
            else if (CinematicFade > 0)
            {
                CinematicFade -= 0.01f;
            }
            RenderManager.postProcessEffect.Parameters["CinemaAmount"].SetValue(CinematicFade);
            //Decrease the timespan
            if (cinematicSpan > TimeSpan.Zero)
            {
                cinematicSpan -= gameTime.ElapsedGameTime;
            }
            else
            {
                CinematicMode = false;
            }
        }

        public static void BeginMission(int mission)
        {
            MissionIndex = mission;
            beginMission = true;
            CinematicMode = true;
            cinematicSpan = new TimeSpan(0, 0, 1);
            if (MissionIndex > 2)
            {
                HUDManager.lastTargetObject = null;
            }
            HUDManager.buildingMode = false;
            HUDManager.diplomacyMode = false;
            HUDManager.spaceshipSelected = false;
            BuildingManager.fade = 0;
            BuildingManager.voice = 0;
            BuildingManager.menu = 0;
        }

        public static void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            //Mission title
            spriteBatch.DrawString(font, "\"" + MissionName + "\"", new Vector2(PlanetoidGame.scene.Width-100 - font.MeasureString(MissionName).X, 100), Color.Lerp(Color.Transparent, Color.Green, CinematicFade));
            //Current text to display
            spriteBatch.DrawString(font, TextToDisplay, new Vector2(40, PlanetoidGame.scene.Height - 100), Color.Lerp(Color.Transparent, Color.White, CinematicFade));
            if (CinematicFade < 0.1f)
            {
                //Mission briefing
                Util.DrawColoredString(font, "\""+MissionName+"\"" + (visible ? " (M to hide)\n" + MissionDescription : " (M to show)"), new Vector2(40, 100), 1-CinematicFade);
            }
        }
    }
}
