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
    public enum BuildingType
    {
        None,
        Extractor,
        Solar,
        LKE,
        Reactor,
        House,
        School,
        Turbina,
        Radar,
        Rocket,
        Hunter,
        Catapult,
        Turret,
        SAgun,
        Repulser
    }

    public struct MenuVoice
    {
        public Texture2D icon;
        public string title;
        public string description;
        public Point cost;
        public int seconds;
        public int[] researchLevels;
        public int modelIndex;
    }

    public class Research
    {
        public Texture2D icon;
        public string title;
        public string description;
        public Point cost;
        /// <summary>
        /// Approximative research time
        /// </summary>
        public int seconds;
        public int index;
    }

    /// <summary>
    /// Building Manager, last review on version 1.0.2
    /// </summary>
    public static class BuildingManager
    {
        //Building Variables
        private static Texture2D backIcon;
        private static Texture2D lockIcon;
        private static Texture2D baseIcon;
        public static MenuVoice[][] menuVoices;
        public static Research[] researches;

        public static BaseObjectBuilding previewBuilding;
        public static Vector3 preBuildPosition;
        public static bool placedBuildingNowRotateIt;

        private static Vector2 center;
        private static float rotation;
        private static float moveDir;
        public static int menu;
        private static int menuNext;
        public static int menuPrev;
        public static float fade;
        public static int voice;
        public static int lastVoice;
        public static float panelFade;
        public static MenuVoice menuVoice;
        public static bool canBuild;
        public static int currentSecond;
        public static int totalSeconds;

        public static List<ResearchButton> researchList;

        public static int[] ResearchLevels(int player)
        {
            return PlayerManager.players[player].researchLevels;
        }

        public static MenuVoice GetMenuVoice(BuildingType type)
        {
            if (type < BuildingType.House)
            {
                return BuildingManager.menuVoices[3][(int)type - 1];
            }
            else if (type < BuildingType.Rocket)
            {
                return BuildingManager.menuVoices[4][(int)type - 5];
            }
            else if (type < BuildingType.Turret)
            {
                return BuildingManager.menuVoices[5][(int)type - 9];
            }
            return BuildingManager.menuVoices[6][(int)type - 12];
        }

        /// <summary>
        /// Load the necessary contents for the correct builing system handling
        /// </summary>
        public static void Initialize()
        {
            center = new Vector2(GameEngine.Game.GraphicsDevice.Viewport.Width - 164, GameEngine.Game.GraphicsDevice.Viewport.Height - 148);

            backIcon = GameEngine.Game.Content.Load<Texture2D>("Icons//backIcon");
            lockIcon = GameEngine.Game.Content.Load<Texture2D>("Icons//lockIcon");
            baseIcon = GameEngine.Game.Content.Load<Texture2D>("Icons//baseIcon");

            menuVoices = new MenuVoice[7][];

            //STARTING MENU
            menuVoices[0] = new MenuVoice[2];
            menuVoices[0][0].title = "Build";
            menuVoices[0][1].title = "Research";

            menuVoices[0][0].description = "Access the building menu with all\nof its features.";
            menuVoices[0][1].description = "Allow you to unlock new buildings\nor upgrade the ones you have.";

            menuVoices[0][0].icon = GameEngine.Game.Content.Load<Texture2D>("Icons//build");
            menuVoices[0][1].icon = GameEngine.Game.Content.Load<Texture2D>("Icons//research");

            //INTO PRE-BUILING MENU MENU
            menuVoices[1] = new MenuVoice[4];
            menuVoices[1][0].title = "Resources Production";
            menuVoices[1][1].title = "Support Buildings";
            menuVoices[1][2].title = "Spaceships";
            menuVoices[1][3].title = "Planetary Defense";

            menuVoices[1][0].description = "Access the resources menu, for\nproduction buildings.";
            menuVoices[1][1].description = "Access the support menu, for\nhominid buildings.";
            menuVoices[1][2].description = "Access the spaceships menu with\nall of its features.";
            menuVoices[1][3].description = "Access the defense menu with\nall the defensive structures.";

            menuVoices[1][0].icon = GameEngine.Game.Content.Load<Texture2D>("Icons//production");
            menuVoices[1][1].icon = GameEngine.Game.Content.Load<Texture2D>("Icons//support");
            menuVoices[1][2].icon = GameEngine.Game.Content.Load<Texture2D>("Icons//ships");
            menuVoices[1][3].icon = GameEngine.Game.Content.Load<Texture2D>("Icons//defense");

            //RESEARCH MENU
            if (GameEngine.gameToLoad == null)
            {
                researchList = new List<ResearchButton>();
            }
            researches = new Research[6];
            researches[0] = new Research
            {
                title = "Energetic Reasearch",
                description = "Take a look into the energy world!\n(Production Bonus!)",
                icon = GameEngine.Game.Content.Load<Texture2D>("Icons//Research//energy"),
                cost = new Point(125, 50),
                seconds = 10,
                index = 0
            };

            researches[1] = new Research
            {
                title = "Laser Research",
                description = "Prepare for some real damage!\n(Bonus to laser structures!)",
                icon = GameEngine.Game.Content.Load<Texture2D>("Icons//Research//laser"),
                cost = new Point(125, 200),
                seconds = 10,
                index = 1
            };

            researches[2] = new Research
            {
                title = "Structures Research",
                description = "The science of building stuff with\nlego.\n(Bonus to School!)",
                icon = GameEngine.Game.Content.Load<Texture2D>("Icons//Research//structure"),
                cost = new Point(250, 0),
                seconds = 10,
                index = 2
            };

            researches[3] = new Research
            {
                title = "Polymers",
                description = "Start researching more complex\nstructures!\n(Bonus to Turbina!)",
                icon = GameEngine.Game.Content.Load<Texture2D>("Icons//Research//polymers"),
                cost = new Point(75, 200),
                seconds = 10,
                index = 3
            };

            researches[4] = new Research
            {
                title = "Engines Research",
                description = "Control is nothing without enough\npower!\n(Production Bonus!)",
                icon = GameEngine.Game.Content.Load<Texture2D>("Icons//Research//engines"),
                cost = new Point(250, 100),
                seconds = 10,
                index = 4
            };

            researches[5] = new Research
            {
                title = "Graviton Research",
                description = "Learn how to manipulate graviton\nfluxes and power fields!",
                icon = GameEngine.Game.Content.Load<Texture2D>("Icons//Research//graviton"),
                cost = new Point(100, 250),
                seconds = 10,
                index = 5
            };

            menuVoices[2] = new MenuVoice[researches.Length];

            for (int b = 0; b < menuVoices[2].Length; b++)
            {
                menuVoices[2][b] = new MenuVoice
                {
                    icon = researches[b].icon,
                };
            }

            //PRODUCTION MENU
            menuVoices[3] = new MenuVoice[4];

            menuVoices[3][0] = new MenuVoice
            {
                title = "Extractor",
                description = "A powerful machinery to extract\nkeldanyum from the ground.",
                icon = GameEngine.Game.Content.Load<Texture2D>("Icons//extractor"),
                cost = new Point(250, 300),
                seconds = 25,
                modelIndex = 0
            };

            menuVoices[3][1] = new MenuVoice
            {
                title = "Solar Panel",
                description = "Gather the light power with this\nmachinery.",
                icon = GameEngine.Game.Content.Load<Texture2D>("Icons//solar"),
                cost = new Point(300, 250),
                seconds = 40,
                modelIndex = 1

            };

            menuVoices[3][2] = new MenuVoice
            {
                title = "L.K.E.",
                description = "(Laser Keldanyum Extractor)\nWith its powerful laser it can\nextract keldanyum from asteroids.",
                icon = GameEngine.Game.Content.Load<Texture2D>("Icons//LKE"),
                cost = new Point(350, 350),
                seconds = 20,
                modelIndex = 2,
                researchLevels = new int[] { 1, 2, 2, 1 }
            };

            menuVoices[3][3] = new MenuVoice
            {
                title = "Reactor",
                description = "Uses graviton particles to interact\nwith Fuchsia matter...\nHigh energy, high risk!",
                icon = GameEngine.Game.Content.Load<Texture2D>("Icons//reactor"),
                cost = new Point(350, 350),
                seconds = 20,
                modelIndex = 3,
                researchLevels = new int[] { 4, 0, 2, 2,2,3 }
                //ene,laser,struct,poly,engine,gravit
            };


            //SUPPORT MENU
            menuVoices[4] = new MenuVoice[4];
            string name = RaceManager.GetRace(PlayerManager.GetRace(0));
            name += (name.Last() == 's' ? "'" : "s");

            menuVoices[4][0] = new MenuVoice
            {
                title = "House",
                description = "The house will help your " + name + "\npopulating the planet.",
                icon = GameEngine.Game.Content.Load<Texture2D>("Icons//house"),
                cost = new Point(300, 50),
                seconds = 30,
                modelIndex = 4,
                researchLevels = new int[] { 0, 0, 1 }
            };
            menuVoices[4][1] = new MenuVoice
            {
                title = "School",
                description = "The school will help you\nteaching to your " + name + "...\nAww school is in games too!",
                icon = GameEngine.Game.Content.Load<Texture2D>("Icons//school"),
                cost = new Point(500, 75),
                seconds = 45,
                modelIndex = 5,
                researchLevels = new int[] { 0, 0, 1, 1 }
            };
            menuVoices[4][2] = new MenuVoice
            {
                title = "Turbina",
                description = "The latest space vacuum!\nThis gigantic vent can modify the\natmosphere of a planet!",
                icon = GameEngine.Game.Content.Load<Texture2D>("Icons//turbina"),
                cost = new Point(100, 500),
                seconds = 60,
                modelIndex = 6,
                researchLevels = new int[] { 1, 0, 3 }
            };

            menuVoices[4][3] = new MenuVoice
            {
                title = "Radar",
                description = "You can't understand aliens?\nBefore declaring war, use this!",
                icon = GameEngine.Game.Content.Load<Texture2D>("Icons//radar"),
                cost = new Point(300, 300),
                seconds = 60,
                modelIndex = 7,
                researchLevels = new int[] { 0, 1, 1, 1 }
            };

            //SPACESHIPS MENU
            menuVoices[5] = new MenuVoice[3];
            menuVoices[5][0] = new MenuVoice
            {
                title = "Rocket",
                description = "Which way to send your " + name + "\nis better than using a rocket?",
                icon = GameEngine.Game.Content.Load<Texture2D>("Icons//rocket"),
                cost = new Point(400, 100),
                seconds = 45,
                modelIndex = 8,
                researchLevels = new int[] { 1, 0, 0, 1, 3 }
            };

            menuVoices[5][1] = new MenuVoice
            {
                title = "Hunter",
                description = "The standard Space Hunter KTA\nready for your commands.",
                icon = GameEngine.Game.Content.Load<Texture2D>("Icons//hunter"),
                cost = new Point(350, 350),
                seconds = 35,
                modelIndex = 9,
                researchLevels = new int[] { 1, 2, 0, 2, 4 }
            };

            menuVoices[5][2] = new MenuVoice
            {
                title = "Catapult",
                description = "Uses graviton fluxes to manipulate\nasteroids, caution, very powerful!",
                icon = GameEngine.Game.Content.Load<Texture2D>("Icons//catapult"),
                cost = new Point(2000, 1400),
                seconds = 50,
                modelIndex = 10,
                researchLevels = new int[] { 2, 0, 2, 3, 3, 4 }
            };

            //DEFENSE MENU
            menuVoices[6] = new MenuVoice[3];
            menuVoices[6][0] = new MenuVoice
            {
                title = "Turret",
                description = "An automatic turret to defend\nyour planets from space threats.",
                icon = GameEngine.Game.Content.Load<Texture2D>("Icons//turret"),
                cost = new Point(100, 300),
                seconds = 25,
                modelIndex = 11,
                researchLevels = new int[] { 1, 3, 0, 2 }
            };
            menuVoices[6][1] = new MenuVoice
            {
                title = "S.A. Gun",
                description = "(Surface-to-Air Gun)\nVery efficient planet defense\nsystem, aims for the engine!",
                icon = GameEngine.Game.Content.Load<Texture2D>("Icons//sagun"),
                cost = new Point(300, 100),
                seconds = 60,
                modelIndex = 12,
                researchLevels = new int[] { 1, 4, 1, 3 }
            };
            menuVoices[6][2] = new MenuVoice
            {
                title = "Repulser",
                description = "Use graviton fields to deflect\nspace threats!",
                icon = GameEngine.Game.Content.Load<Texture2D>("Icons//repulser"),
                cost = new Point(200, 500),
                seconds = 120,
                modelIndex = 13,
                researchLevels = new int[] { 2, 0, 1, 1, 1, 2 }
            };

            if (GameEngine.gameToLoad == null)
            {
                for (int a = 0; a < PlayerManager.players.Length; a++)
                {
                    PlayerManager.players[a].researchLevels = new int[researches.Length];
                    for (int r = 0; r < researches.Length; r++)
                    {
                        PlayerManager.players[a].researchLevels[r] = 1;
                    }
                }
            }
            fade = 0;
            menu = 0;
            menuNext = 0;
            menuPrev = 0;
            Clear();
            GameEngine.gameToLoad = null;
        }

        /// <summary>
        /// Initialize the preview building to allow it correct building drawing
        /// </summary>
        public static BaseObjectBuilding InitializePreviewModel(BuildingType type, Planet planet, Matrix matrix, int owner)
        {
            BaseObjectBuilding prevBuild;
            switch (type)
            {
                case BuildingType.Extractor:
                    prevBuild = new Extractor(null, Matrix.Identity, owner);
                    break;
                case BuildingType.Solar:
                    prevBuild = new Solar(null, Matrix.Identity, owner);
                    break;
                case BuildingType.Turbina:
                    prevBuild = new Turbina(null, Matrix.Identity, owner);
                    break;
                case BuildingType.Turret:
                    prevBuild = new Turret(null, Matrix.Identity, owner);
                    break;
                case BuildingType.SAgun:
                    prevBuild = new SAgun(null, Matrix.Identity, owner);
                    break;
                case BuildingType.Radar:
                    prevBuild = new Radar(null, Matrix.Identity, owner);
                    break;
                case BuildingType.Catapult:
                    prevBuild = new Catapult(null, Matrix.Identity, owner);
                    break;
                case BuildingType.Reactor:
                    prevBuild = new Reactor(null, Matrix.Identity, owner);
                    break;
                case BuildingType.Rocket:
                    prevBuild = new Rocket(null, Matrix.Identity, owner);
                    break;
                default:
                    prevBuild = new BaseObjectBuilding();
                    prevBuild.type = type;
                    prevBuild.owner = owner;
                    break;
            }
            prevBuild.planet = planet;
            prevBuild.matrix = matrix;
            return prevBuild;
        }

        /// <summary>
        /// Check if the position on the planet is collision-free
        /// </summary>
        public static bool FreeBuildingPosition(Planet planet, Vector3 position)
        {
            for (int a = 0; a < planet.buildings.Count(); a++)
            {
                if (planet.buildings[a].matrix.Translation != position)
                {
                    if (Vector3.Distance(position, planet.buildings[a].matrix.Translation) < 20)
                    {
                        return false;
                    }
                }
            }

            for (int a = 0; a < planet.trees.Count(); a++)
            {
                if (!planet.trees[a].gaseous && Vector3.Distance(position, planet.trees[a].matrix.Translation) < 15)
                {
                    return false;
                }
            }

            return true;
        }

        public static void ManageBuilding(float elapsed)
        {
            canBuild = false;
            //Get the position on the planet, if there is one..
            if (placedBuildingNowRotateIt == false)
            {
                preBuildPosition = GameEngine.gameCamera.GetClickPositionOnPlanet((Planet)GameEngine.gameCamera.target);
            }
            //There is a position, get the matrix
            if (preBuildPosition != Vector3.Zero)
            {
                preBuildPosition -= previewBuilding.planet.oldPosition;
                preBuildPosition = Vector3.Transform(preBuildPosition, Matrix.CreateFromAxisAngle(previewBuilding.planet.axis, previewBuilding.planet.spinSpeed * elapsed));
                preBuildPosition += previewBuilding.planet.matrix.Translation;

                previewBuilding.matrix = Matrix.Invert(Matrix.CreateLookAt(preBuildPosition, GameEngine.gameCamera.target.matrix.Translation, Vector3.Up));
                previewBuilding.matrix.Translation = GameEngine.gameCamera.target.matrix.Translation + previewBuilding.matrix.Backward * (((Planet)GameEngine.gameCamera.target).radius + previewBuilding.SurfaceOffset);
                canBuild = true;
                //If the building has been placed, rotate it following the mouse
                if (placedBuildingNowRotateIt == true)
                {
                    Vector3 temporary = previewBuilding.matrix.Translation;
                    previewBuilding.matrix.Translation = Vector3.Zero;
                    Vector3 temp = GameEngine.Game.GraphicsDevice.Viewport.Project(temporary, GameEngine.gameCamera.projectionMatrix, GameEngine.gameCamera.viewMatrix, Matrix.Identity);
                    previewBuilding.matrix *= Matrix.CreateFromAxisAngle(previewBuilding.matrix.Forward, (float)Math.Atan2(GameEngine.ms.Y - temp.Y, GameEngine.ms.X - temp.X));
                    previewBuilding.matrix.Translation = temporary;
                }
            }

            if (canBuild)
            {
                //Check if the position on the planet is valid
                if (FreeBuildingPosition(((Planet)GameEngine.gameCamera.target), previewBuilding.matrix.Translation))
                {
                    //It is valid
                    if (GameEngine.ms.LeftButton == ButtonState.Pressed && GameEngine.pms.LeftButton == ButtonState.Released)
                    {
                        if (placedBuildingNowRotateIt == false)
                        {
                            if (PlayerManager.GetKeldanyum(0) >= menuVoice.cost.X && PlayerManager.GetEnergy(0) >= menuVoice.cost.Y)
                            {
                                placedBuildingNowRotateIt = true;
                            }
                            else
                            {
                                AudioManager.Play("error");
                            }
                        }
                        else
                        {

                            AudioManager.Play("building_placed");

                            //Place the building
                            StartBuilding(previewBuilding.type, (Planet)GameEngine.gameCamera.target, previewBuilding.matrix, 0);
                            //If the player has pressed shift during placing, let him to place other buildings
                            if (GameEngine.ks.IsKeyDown(Keys.LeftShift))
                            {
                                placedBuildingNowRotateIt = false;
                                BuildingManager.previewBuilding = BuildingManager.InitializePreviewModel(previewBuilding.type, (Planet)GameEngine.gameCamera.target, previewBuilding.matrix, 0);
                            }
                            else
                            {
                                Clear();
                            }
                        }
                    }
                }
                else
                {
                    if (GameEngine.ms.LeftButton == ButtonState.Pressed && GameEngine.pms.LeftButton == ButtonState.Released)
                    {
                        AudioManager.Play("error");
                    }
                    canBuild = false;
                }
            }
        }

        /// <summary>
        /// Administrate the events occurring when a building is placed
        /// </summary>
        public static void AddBuilding(BuildingType type, Planet planet, Matrix matrix, int Owner)
        {
            PlayerManager.players[Owner].BuildingCreated();
            switch (type)
            {
                case BuildingType.Extractor:
                    planet.buildings.Add(new Extractor(planet, matrix, Owner));
                    break;
                case BuildingType.House:
                    planet.buildings.Add(new House(planet, matrix, Owner));
                    break;
                case BuildingType.School:
                    planet.buildings.Add(new School(planet, matrix, Owner));
                    break;
                case BuildingType.Solar:
                    planet.buildings.Add(new Solar(planet, matrix, Owner));
                    break;
                case BuildingType.Turret:
                    planet.buildings.Add(new Turret(planet, matrix, Owner));
                    break;
                case BuildingType.SAgun:
                    planet.buildings.Add(new SAgun(planet, matrix, Owner));
                    break;
                case BuildingType.LKE:
                    planet.buildings.Add(new LKE(planet, matrix, Owner));
                    break;
                case BuildingType.Turbina:
                    //AI players should get really really angry if you build turbinas on their planets
                    int race = planet.DominatingRace();
                    if (race > -1)
                    {
                        race = PlayerManager.GetRaceOwner(race);
                        if (race > 0)
                        {
                            PlayerManager.ChangeFriendship(race, Owner, -0.1f);
                            PlayerManager.ChangeTrust(race, Owner, -0.4f);
                        }
                    }
                    planet.buildings.Add(new Turbina(planet, matrix, Owner));
                    break;
                case BuildingType.Rocket:
                    planet.buildings.Add(new Rocket(planet, matrix, Owner));
                    break;
                case BuildingType.Hunter:
                    planet.buildings.Add(new Hunter(planet, matrix, Owner));
                    break;
                case BuildingType.Radar:
                    //Increase the radar counter for the owning player
                    PlayerManager.players[Owner].radarAmount++;
                    planet.buildings.Add(new Radar(planet, matrix, Owner));
                    break;
                case BuildingType.Repulser:
                    planet.buildings.Add(new Repulser(planet, matrix, Owner));
                    break;
                case BuildingType.Catapult:
                    planet.buildings.Add(new Catapult(planet, matrix, Owner));
                    break;
                case BuildingType.Reactor:
                    planet.buildings.Add(new Reactor(planet, matrix, Owner));
                    break;
            }
        }

        /// <summary>
        /// Create a PreBuilding object for the given owner
        /// </summary>
        public static void StartBuilding(BuildingType type, Planet planet, Matrix matrix, int Owner)
        {
            previewBuilding = InitializePreviewModel(type, planet, matrix, Owner);
            planet.buildings.Add(new PreBuilding(previewBuilding));
            PlayerManager.SetKeldanyum(Owner, PlayerManager.GetKeldanyum(Owner) - menuVoice.cost.X);
            PlayerManager.SetEnergy(Owner, PlayerManager.GetEnergy(Owner) - menuVoice.cost.Y);
        }

        /// <summary>
        /// Start building interface
        /// </summary>
        public static void Start()
        {
            HUDManager.displayNames = false;
            if (HUDManager.lastTargetObject != null && HUDManager.lastTargetObject is Planet == false)
            {
                if (HUDManager.lastTargetObject.planet != null && Vector3.Distance(GameEngine.gameCamera.position, HUDManager.lastTargetObject.planet.matrix.Translation) < Vector3.Distance(GameEngine.gameCamera.position, GameEngine.gameCamera.target.matrix.Translation))
                {
                    GameEngine.gameCamera.target = HUDManager.lastTargetObject.planet;
                }
                HUDManager.lastTargetObject = null;
            }

            menu = 0;
            menuNext = 0;
            menuPrev = -1;
            fade = 1;
            moveDir = MathHelper.PiOver2;
            panelFade = 0;
            GameEngine.gameCamera.tempZoom = GameEngine.gameCamera.zoom;
            rotation = 0;

            //Reset building variables
            placedBuildingNowRotateIt = false;
            previewBuilding = null;
            AudioManager.Play("swish_in");
        }

        /// <summary>
        /// End building interface
        /// </summary>
        public static void End()
        {
            moveDir = -MathHelper.PiOver2;
            menuPrev = -2;
            fade = 1;

            GameEngine.gameCamera.targetZoom = GameEngine.gameCamera.tempZoom;
            GameEngine.gameCamera.zoomSpeed = 0;

            AudioManager.Play("swish_out");
        }

        /// <summary>
        /// Clear all the building interface values
        /// </summary>
        public static void Clear()
        {
            previewBuilding = null;
            voice = 0;
            placedBuildingNowRotateIt = false;
            preBuildPosition = Vector3.Zero;
        }

        /// <summary>
        /// Update the information panel fading amount
        /// </summary>
        public static void FadingUpdate()
        {
            //Information panel update
            if (voice > 0)
            {
                if (panelFade < 1)
                {
                    panelFade += 0.05f;
                }
                menuVoice = menuVoices[BuildingManager.menu][voice - 1];
            }
            else
            {
                if (panelFade > 0)
                {
                    panelFade -= 0.05f;
                }
                else if (menuVoice.cost.Y != -1)
                {
                    menuVoice.cost.Y = -1;
                }
            }
        }

        public static void UpdateBuildingMenu()
        {
            //Ingame angle update
            if (Math.Abs(GameEngine.ms.X - center.X) > 100)
            {
                rotation += MathHelper.ToRadians((GameEngine.ms.X - center.X) / 200f);
            }
            if (rotation > MathHelper.TwoPi)
            {
                rotation -= MathHelper.TwoPi;
            }
            else if (rotation < 0)
            {
                rotation += MathHelper.TwoPi;
            }

            //Ingame fade update
            //Move the menu from "menu" to "menuNext"
            //Administrate the values for the animations
            if (menu != menuNext || menuPrev < 0)
            {
                voice = -1;
                panelFade *= 0.9f;
                fade *= 0.9f;
                fade -= 0.01f;
                if (fade <= 0)
                {
                    fade = 0;
                    if (menuPrev == -2)
                    {
                        HUDManager.buildingMode = false;
                        return;
                    }
                    switch (menuNext)
                    {
                        case 0:
                        case 1:
                        case 2:
                            menuPrev = 0;
                            break;
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                            menuPrev = 1;
                            break;
                    }
                    menu = menuNext;
                }
            }

            if (menu == 2)
            {
                int level;
                //LOAD STUFF
                for (int b = 0; b < menuVoices[menu].Length; b++)
                {
                    level = ResearchLevels(0)[b];
                    menuVoices[menu][b].title = researches[b].title + " level " + level;
                    menuVoices[menu][b].description = researches[b].description;
                    menuVoices[menu][b].seconds = researches[b].seconds;
                    menuVoices[menu][b].cost = new Point(researches[b].cost.X * level, researches[b].cost.Y * level);
                }
            }

            //Button selection
            float angle = MathHelper.TwoPi / (menuVoices[menu].Length);
            Vector2 position;
            voice = -1;
            //currentBuilding = 0;
            //Back button
            if (Vector2.Distance(new Vector2(GameEngine.ms.X, GameEngine.ms.Y), center) < 40)
            {
                voice = 0;
                if (GameEngine.ms.LeftButton == ButtonState.Pressed && GameEngine.pms.LeftButton == ButtonState.Released)
                {
                    //If first menu, then close the interface
                    if (menu == 0)
                    {
                        End();
                    }
                    //Otherwise return to the previous menu
                    else
                    {
                        AudioManager.Play("swish_out");
                        menuNext = menuPrev;
                        fade = 1;
                    }
                }
                return;
            }

            //Other buttons
            for (int a = 0; a < menuVoices[menu].Length; a++)
            {
                position = center + new Vector2(
                    (float)Math.Cos(a * angle + rotation) * 95,
                    (float)Math.Sin(a * angle + rotation) * 95);

                if (Vector2.Distance(new Vector2(GameEngine.ms.X, GameEngine.ms.Y), position) < 40)
                {
                    voice = a + 1;
                    lastVoice = voice;
                    if (GameEngine.ms.LeftButton == ButtonState.Pressed && GameEngine.pms.LeftButton == ButtonState.Released)
                    {
                        moveDir = rotation;
                        switch (menu)
                        {
                            //STANDARD MENU
                            case 0:
                                if (a + 1 == 2)
                                {
                                    if (TutorialManager.MissionIndex < 2 && GameEngine.gameMode == GameMode.Tutorial)
                                    {
                                        TextBoard.AddMessage("Not so fast!");
                                        AudioManager.Play("error");
                                        break;
                                    }
                                }
                                else if ((GameEngine.gameCamera.target as Planet).CanBuild(0)==false)
                                {
                                    TextBoard.AddMessage("You can build only on your planets!");
                                    AudioManager.Play("error");
                                    break;
                                }
                                else if ((GameEngine.gameCamera.target as Planet).planet == null)
                                {
                                    TextBoard.AddMessage("You can't build on flying planets!");
                                    AudioManager.Play("error");
                                    break;
                                }
                                menuNext = (a + 1);
                                AudioManager.Play("swish_in");
                                fade = 1;
                                break;
                            //BUILD MENU
                            case 1:

                                AudioManager.Play("swish_in");
                                menuNext = (a + 3);
                                fade = 1;
                                break;
                            //RESEARCH MENU
                            case 2:
                                if (researchList.Count < 6 && researchList.Find(r => r.text.Contains(researches[a].title)) == null)
                                {
                                    if (ResearchLevels(0)[a] < 5)
                                    {
                                        if (PlayerManager.GetKeldanyum(0) >= researches[a].cost.X * ResearchLevels(0)[a] &&
                                            PlayerManager.GetEnergy(0) >= researches[a].cost.Y * ResearchLevels(0)[a]
                                            )
                                        {
                                            PlayerManager.ChangeKeldanyum(0, -researches[a].cost.X * ResearchLevels(0)[a]);
                                            PlayerManager.ChangeEnergy(0, -researches[a].cost.Y * ResearchLevels(0)[a]);
                                            //ADD RESEARCH TO RESEARCH LIST
                                            researchList.Add(new ResearchButton(researches[a]));
                                            AudioManager.Play("button_click");
                                        }
                                        else
                                        {
                                            AudioManager.Play("error");
                                        }
                                    }
                                    else
                                    {
                                        AudioManager.Play("error");
                                    }
                                }
                                else
                                {
                                    AudioManager.Play("error");
                                }
                                break;
                            //BUILD MENU - PRODUCTION
                            case 3:
                                if (SatisfyResearches(0, (BuildingType)(a + 1)))
                                {
                                    AudioManager.Play("button_click");
                                    previewBuilding = InitializePreviewModel((BuildingType)(a + 1), (Planet)GameEngine.gameCamera.target, Matrix.Identity, 0);

                                }
                                else
                                {
                                    AudioManager.Play("error");
                                }
                                break;
                            //BUILD MENU - SUPPORT
                            case 4:
                                if (SatisfyResearches(0, (BuildingType)(a + 5)))
                                {
                                    previewBuilding = InitializePreviewModel((BuildingType)(a + 5), (Planet)GameEngine.gameCamera.target, Matrix.Identity, 0);
                                    AudioManager.Play("button_click");
                                }
                                else
                                {
                                    AudioManager.Play("error");
                                }
                                break;
                            //BUILD MENU - SHIPS
                            case 5:
                                if (SatisfyResearches(0, (BuildingType)(a + 9)))
                                {
                                    previewBuilding = InitializePreviewModel((BuildingType)(a + 9), (Planet)GameEngine.gameCamera.target, Matrix.Identity, 0);
                                    AudioManager.Play("button_click");
                                }
                                else
                                {
                                    AudioManager.Play("error");
                                }
                                break;
                            //BUILD MENU - SHIPS
                            case 6:
                                if (SatisfyResearches(0, (BuildingType)(a + 12)))
                                {
                                    previewBuilding = InitializePreviewModel((BuildingType)(a + 12), (Planet)GameEngine.gameCamera.target, Matrix.Identity, 0);
                                    AudioManager.Play("button_click");
                                }
                                else
                                {
                                    AudioManager.Play("error");
                                }
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retrieve if "player" has researches high enough to build an object
        /// </summary>
        public static bool SatisfyResearches(int player, BuildingType type)
        {
            MenuVoice voice = GetMenuVoice(type);
            if (voice.researchLevels == null)
            {
                return true;
            }
            for (int a = 0; a < voice.researchLevels.Length; a++)
            {
                if (voice.researchLevels[a] > ResearchLevels(player)[a] - 1)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Draw (and update) the menu showing system
        /// </summary>
        public static void DrawBuilingMenu()
        {
            //Draw the various building icons
            Color color;
            float angle = MathHelper.TwoPi / (menuVoices[menu].Length);

            Vector2 position;

            Vector2 otherPos = center;
            if (menuPrev != -1)
            {
                if (fade > 0)
                {
                    otherPos -= new Vector2(
                        (float)Math.Cos(moveDir) * ((1 - fade) * 400),
                        (float)Math.Sin(moveDir) * ((1 - fade) * 400));

                }
                for (int a = 0; a < menuVoices[menu].Length; a++)
                {
                    position = otherPos + new Vector2(
                        (float)Math.Cos(a * angle + rotation) * 95,
                        (float)Math.Sin(a * angle + rotation) * 95);

                    if (a + 1 == voice)
                    {
                        color = Color.Lime;
                    }
                    else
                    {
                        color = Color.White;
                    }

                    if (fade > 0)
                    {
                        color = Color.Lerp(Color.Transparent, color, fade);
                    }

                    PlanetoidGame.spriteBatch.Draw(baseIcon, position - new Vector2(35, 35), color);
                    PlanetoidGame.spriteBatch.Draw(menuVoices[menu][a].icon, position - new Vector2(25, 25), color);
                    if (menuVoices[menu][a].researchLevels != null && SatisfyResearches(0, (BuildingType)(menuVoices[menu][a].modelIndex + 1)) == false)
                    {
                        PlanetoidGame.spriteBatch.Draw(lockIcon, position - new Vector2(35, 35), color);
                    }
                }
                color = (voice == 0 ? Color.Lime : Color.White);
                if (fade > 0)
                {
                    color = Color.Lerp(Color.Transparent, color, fade);
                }
                PlanetoidGame.spriteBatch.Draw(backIcon, otherPos - new Vector2(35, 35), color);
            }
            if (menuPrev != -2)
            {
                if (fade > 0)
                {
                    color = Color.Lerp(Color.White, Color.Transparent, fade);

                    otherPos = center + new Vector2(
                         (float)Math.Cos(moveDir) * (fade * 400),
                         (float)Math.Sin(moveDir) * (fade * 400));

                    angle = MathHelper.TwoPi / (menuVoices[menuNext].Length);

                    for (int a = 0; a < menuVoices[menuNext].Length; a++)
                    {
                        position = otherPos + new Vector2(
                            (float)Math.Cos(a * angle + rotation) * 95,
                            (float)Math.Sin(a * angle + rotation) * 95);


                        PlanetoidGame.spriteBatch.Draw(baseIcon, position - new Vector2(35, 35), color);
                        PlanetoidGame.spriteBatch.Draw(menuVoices[menuNext][a].icon, position - new Vector2(25, 25), color);
                        if (menuVoices[menuNext][a].researchLevels != null && SatisfyResearches(0, (BuildingType)(menuVoices[menuNext][a].modelIndex + 1)) == false)
                        {
                            PlanetoidGame.spriteBatch.Draw(lockIcon, position - new Vector2(35, 35), color);
                        }
                    }
                    PlanetoidGame.spriteBatch.Draw(backIcon, otherPos - new Vector2(35, 35), color);
                }
            }
        }

        public static void UpdateReserchList()
        {
            if (researchList.Count > 0)
            {
                if (currentSecond != DateTime.Now.Second)
                {
                    totalSeconds += (RaceManager.GetAbility(PlayerManager.GetRace(0)) == 2 ? 2 : 1);
                    currentSecond = DateTime.Now.Second;
                }
                if (GameEngine.gameMode == GameMode.Hyperbuild)
                {
                    totalSeconds = 1000;
                }
                if (totalSeconds >= researchList[0].research.seconds * ResearchLevels(0)[researchList[0].research.index] && researchList[0].fadeOut == 1)
                {
                    //Update researche values
                    ResearchLevels(0)[researchList[0].research.index]++;

                    //THROW RESEARCH FINISHED EVENT
                    researchList[0].fadeOut = 0.1f;

                    researches[researchList[0].research.index].seconds += (researches[researchList[0].research.index].seconds / 2);

                    if (PlayerManager.players[0].researchLevels.All(r => r == 5))
                    {
                        QuestManager.QuestCall(10);
                    }
                }
                //Update the research buttons
                for (int a = 0; a < researchList.Count; a++)
                {
                    if (researchList[a].UpdateResearch())
                    {
                        if (a == 0 && researchList[0].fadeOut == 1)
                        {
                            totalSeconds = 0;
                        }
                        researchList.RemoveAt(a);
                        a--;
                    }
                }
            }
        }

        public static void DrawResearchList(SpriteFont font)
        {
            for (int a = 0; a < researchList.Count; a++)
            {
                researchList[a].position.Y = 50 + (a * 60);

                researchList[a].position.X = GameEngine.Game.GraphicsDevice.Viewport.Width - 310 + (600 * researchList[a].fadeIn);
                if (researchList[a].fadeOut < 1)
                {
                    researchList[a].position.X += (600 * researchList[a].fadeOut);
                }
                for (int b = a - 1; b >= 0; b--)
                {
                    if (researchList[b].fadeOut < 1)
                    {
                        researchList[a].position.Y -= (50 * researchList[b].fadeOut * researchList[b].fadeOut);
                        break;
                    }
                }
                researchList[a].DrawResearch(font, (a == 0 ? totalSeconds : 0));
            }
        }
    }
}
