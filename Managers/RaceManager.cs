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

namespace Planetoid3D
{
    /// <summary>
    /// Race Manager, last review on version 1.0.2
    /// </summary>
    public static class RaceManager
    {
        /// <summary>
        /// Load all races informations
        /// </summary>
        public static void LoadRaces()
        {
            names = new List<string>();
            descriptions = new List<string>();
            atmospheres = new List<Atmosphere>();
            textures = new List<Texture2D>();
            colors = new List<Color>();
            features = new List<int>();
            abilities = new List<int>();

            //Load informations from xml file
            XmlDocument document = new XmlDocument();
            document.Load("Content//Races//Races.xml");


            foreach (XmlNode race in document.GetElementsByTagName("Race"))
            {
                //Name
                names.Add(race.ChildNodes[0].InnerText);
                //Description
                descriptions.Add(race.ChildNodes[2].InnerText.Replace("\\n","\n"));
                //Atmosphere
                atmospheres.Add((Atmosphere)Enum.Parse(typeof(Atmosphere), race.ChildNodes[1].InnerText));
                //Texture
                textures.Add(GameEngine.Game.Content.Load<Texture2D>("Races//" + names.Last()));
                //textures.Add(Texture2D.FromStream(GameEngine.Game.GraphicsDevice,new StreamReader("Content//Races//" + names.Last() + ".png").BaseStream));
                //Color
                Color[] myUint = new Color[1];
                textures.Last().GetData(0, new Rectangle(47, 77, 1, 1), myUint, 0, 1);
                colors.Add(myUint[0]);
                //Features
                features.Add(int.Parse(race.ChildNodes[3].InnerText));
                //Abilities
                abilities.Add(int.Parse(race.ChildNodes[4].InnerText));
            }

            //Read custom races
            if (Directory.Exists(MenuManager.GetBasePath() + "//CustomRaces"))
            {
                Atmosphere atmosphere;
                int ability;
                string description;
                foreach (string dir in Directory.GetDirectories(MenuManager.GetBasePath() + "//CustomRaces"))
                {
                    if (File.Exists(dir + "//data.xml") && File.Exists(dir + "//texture.png"))
                    {
                        try
                        {
                            document.Load(dir + "//data.xml");
                            atmosphere = (Atmosphere)Enum.Parse(typeof(Atmosphere), document.GetElementsByTagName("Atmosphere")[0].InnerText);
                            ability = (int)MathHelper.Clamp(int.Parse(document.GetElementsByTagName("Ability")[0].InnerText), 0, abilityNames.Length - 1);
                            description = document.GetElementsByTagName("Description")[0].InnerText.Replace("\\n", "\n");

                            //The custom race is valid!
                            //Name
                            names.Add(new DirectoryInfo(dir).Name);
                            //Description
                            descriptions.Add(description);
                            //Atmosphere
                            atmospheres.Add(atmosphere);
                            //Ability
                            abilities.Add(ability);
                            //Texture
                            textures.Add(Texture2D.FromStream(GameEngine.Game.GraphicsDevice, File.OpenRead(dir + "//texture.png")));
                            //Color
                            Color[] myUint = new Color[1];
                            textures.Last().GetData(0, new Rectangle(47, 77, 1, 1), myUint, 0, 1);
                            colors.Add(myUint[0]);
                            //Features
                            features.Add(0);
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }
            }

            //Every time you start the game, the initial video has a different color :D
            todayIntroColor = Color.Lerp(colors[Util.random.Next(colors.Count)],Color.White,0.25f);
        }

        private static List<string> names;
        private static List<string> descriptions;
        private static List<Atmosphere> atmospheres;
        private static List<Texture2D> textures;
        private static List<Color> colors;
        private static List<int> features;
        private static List<int> abilities;
        public static Color todayIntroColor;

        private static string[] abilityNames = new string[]
        {
            "Fast Regeneration",//0 health
            "Efficient Lungs",//1 breathing
            "Sharp Wits",//2 research
            "Advanced Carpentry",//3 building
            "Faster Metabolism",//4 everything speed up
            "Brute Strength",//5 damages x 1.1~1.4
            "Born To Fly",//6 spaceship speed
            "Adapting Lungs",//7 hominids adapt to new atmospheres
            "Longevity",//8 health = 120
            "Energy Link",//9 gives 40 energy on death
            "Death Worship",//10 gives 40 keldanyum on death
            "Electric Heartbeat",//11 hominids produce energy
            "Magnetic Feet",//12 hominids produce keldanyum
            "Hard Shape"//13 damage returned to opponent
        };

        private static string[] abilityDescription = new string[]
        {
            "Health regenerates twice faster",
            "Atmosphere consumption halved",
            "Research speed increased",
            "Building speed increased to 150%",
            "Increased hominid speed",
            "Damage modifier up to 140%",
            "All spaceships have pilot-speed",
            "2% of chance to adapt to toxic atmospheres",
            "120% of health",
            "Hominids give 40 energy on death",
            "Hominids give 40 keldanyum on death",
            "Hominids produce energy",
            "Hominids gather keldanyum",
            "Hominids' skin is so hard that the opponent will hurt himself on each hit"
        };

        public static int TotalRaces
        {
            get { return names.Count; }
        }

        public static string GetRace(int index)
        {
            return names[index];
        }

        public static string GetDescription(int index)
        {
            return descriptions[index];
        }

        public static Texture2D GetTexture(int index)
        {
            return textures[index];
        }

        public static Color GetColor(int index)
        {
            return colors[index];
        }

        public static int GetFeature(int index)
        {
            return features[index];
        }

        public static Atmosphere GetAtmosphere(int index)
        {
            return atmospheres[index];
        }

        public static string GetAbilityText(int index)
        {
            return abilityNames[abilities[index]]+"\n("+abilityDescription[abilities[index]]+")";
        }

        public static string GetAbilityName(int index)
        {
            return abilityNames[abilities[index]];
        }

        public static int GetAbility(int index)
        {
            return abilities[index];
        }

        public static bool ExistRace(string name)
        {
            return names.Contains(name);
        }

        public static bool IsUnlocked(int index)
        {
            switch (index)
            {
                //UNLOCK WITH
                case 11://ginger
                    return QuestManager.QuestUnlocked(17);
                case 12://cosence
                    return QuestManager.QuestUnlocked(10);
                case 13://sommo
                    return QuestManager.QuestUnlocked(4);
                case 14://astronaute
                    return QuestManager.QuestUnlocked(7);
                case 15://vertex
                    return QuestManager.QuestUnlocked(3);
                case 16://zombie
                    return QuestManager.QuestUnlocked(2);
                case 17://numby
                    return QuestManager.QuestUnlocked(1);
                case 18://cleaved
                    return QuestManager.QuestUnlocked(9);
            }
            return true;
        }

        /// <summary>
        /// Returns if a given race can tolerate a given atmosphere
        /// </summary>
        public static bool Tolerate(int index, Atmosphere type)
        {
            return (atmospheres[index] == type) || atmospheres[index]==Atmosphere.None;
        }
    }
}
