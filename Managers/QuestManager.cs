using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Xml;

namespace Planetoid3D
{
    public struct Quest
    {
        public string name;
        public string description;
        public int actual;
        public int maximum;
    }

    /// <summary>
    /// Quest Manager, last review on version 1.0.2
    /// </summary>
    public static class QuestManager
    {
        public static void Initialize()
        {
            questSign = GameEngine.Game.Content.Load<Texture2D>("questSign");
            timer = -1;
        }

        private static Texture2D questSign;
        private static int currentQuest;
        private static float timer;
        private static bool completed;
        private static Vector2 signPosition;

        public static Quest[] quests =
            new Quest[]{
                //FIRST PAGE
                //trogloid pages 0
                new Quest{name="Death to the Fuchsia Chicken!",description="Kill 50 Troglother - UNLOCK TROGLOID PAGES",actual=0,maximum=50},
                //astrorain mode 1
                new Quest{name="Asteroids? Don't make me laugh!",description="Destroy 2000 asteroids with laser shots - UNLOCK NUMBY RACE, ASTRORAIN MODE",actual=0,maximum=2000},
                //zombie, hominid pages 2
                new Quest{name="Slaughter Master",description="Kill 250 hostile hominids during brawls - UNLOCK ZOMBIE RACE, HOMINIDS PAGES",actual=0,maximum=250},
                //vertex race 3
                new Quest{name="Seriously",description="Win 20 games - UNLOCK VERTEX RACE",actual=0,maximum=20},
                //sommo, creation pages 4 
                new Quest{name="What's the time?",description="Play for two hours... nonstop - UNLOCK SOMMO RACE, CREATION PAGES",actual=0,maximum=1},
                //hyperbuild, resources pages 5
                new Quest{name="My Job Is MINING!",description="Extract 50000 of keldanyum only with extractors - UNLOCK HYPERBUILD MODE, RESOURCES PAGES",actual=0,maximum=50000},
                //planetoid pages 6
                new Quest{name="Who's there?",description="Discover the secret of the Planetoid - UNLOCK PLANETOID PAGES",actual=0,maximum=1},
                //astronaute, ships pages 7
                new Quest{name="It's easier together",description="Create 50 alliances - UNLOCK ASTRONAUT RACE, SHIPS PAGES",actual=0,maximum=50},
                //giant mode, planet pages 8
                new Quest{name="Overcrowding",description="Make a planet have 3 times it's maximum population - UNLOCK GIANT MODE, PLANET PAGES",actual=0,maximum=1},
                //cleaved mode 9
                new Quest{name="Spartan training",description="Train 300 soldiers - UNLOCK CLEAVED RACE",actual=0,maximum=300},
                //SECOND PAGE
                //cosence 10
                new Quest{name="Scientist",description="Complete all researches during one game - UNLOCK COSENCE RACE",actual=0,maximum=1},
                //black hole pages 11
                new Quest{name="We Love Turbinas!",description="Change 50 planets' atmospheres - UNLOCK BLACK HOLE PAGES",actual=0,maximum=50},
                //generic pages 12
                new Quest{name="Hunters? Yummy",description="Destroy 25 Hunters - UNLOCK TRIVIA PAGES",actual=0,maximum= 25},
                //countdown mode 13
                new Quest{name="Armageddon",description="See all planets die - UNLOCK COUNTDOWN MODE, SUN PAGES",actual=0,maximum=1},
                //14
                new Quest{name="What!?",description="Score 10000 or more",actual=0,maximum=1},
                //15
                new Quest{name="Were we going to die?",description="20 Rockets return home",actual=0,maximum=20},
                //16
                new Quest{name="Guess?",description="Guess my favourite race... (write in game title)",actual=0,maximum=1},
                //ginger race 17
                new Quest{name="I'm so powerful!",description="Finish the tutorial - UNLOCK GINGER RACE",actual=0,maximum=1},
                //18
                new Quest{name="This Must Be Wrong",description="Finish a game without losing any hominid! (your planet must explode)",actual=0,maximum=1},
                //19
                new Quest{name="NERD",description="Complete ALL Quests",actual=0,maximum=1}
            };

        /// <summary>
        /// Return the string containing the quest current status
        /// </summary>
        public static string GetQuest(int _quest)
        {
            if (quests[_quest].actual == quests[_quest].maximum)
            {
                return (quests[_quest].name + " COMPLETED!"/*+(_quest==19?" CLICK ME!":"")*/);
            }
            else if (quests[_quest].maximum == 1)
            {
                return (quests[_quest].name + " NOT COMPLETED!");
            }
            return (quests[_quest].name + " (" + quests[_quest].actual + "/" + quests[_quest].maximum + ")");
        }

        /// <summary>
        /// Set the value of an quest
        /// </summary>
        public static void SetQuest(int quest, int value)
        {
            quests[quest].actual = value;
        }

        /// <summary>
        /// Call the quest, increasing its counter and then eventually calling the draw method
        /// </summary>
        public static void QuestCall(int quest)
        {
            //This quest still need to be completed
            if (quests[quest].actual < quests[quest].maximum)
            {
                quests[quest] = new Quest { name = quests[quest].name, description = quests[quest].description, actual = (quests[quest].actual + 1), maximum = quests[quest].maximum };
                if (quests[quest].actual == quests[quest].maximum)
                {
                    //Quest unlocked
                    timer = 0;
                    currentQuest = quest;
                    completed = true;
                    signPosition = new Vector2(GameEngine.Game.GraphicsDevice.Viewport.Width / 2 - 200, -50);
                    AudioManager.Play("quest_completed");
                    //All quests unlocked
                    if (quests.All(a => a.actual >= a.maximum || a.name == "Nerd"))
                    {
                        QuestCall(19);
                    }
                }
                else if (quests[quest].actual % 25 == 0)
                {
                    //Quest unlocked
                    timer = 0;
                    currentQuest = quest;
                    completed = false;
                    signPosition = new Vector2(GameEngine.Game.GraphicsDevice.Viewport.Width / 2 - 200, -50);
                }
            }
            //Update data
            DataManager.Decrypt(MenuManager.GetSettingPath());
            MenuManager.document.Load(MenuManager.GetSettingPath());
            MenuManager.document.GetElementsByTagName("Quest")[quest].InnerText = quests[quest].actual.ToString();
            MenuManager.document.Save(MenuManager.GetSettingPath());
            DataManager.Encrypt(MenuManager.GetSettingPath());
        }

        /// <summary>
        /// Returns if an quest has been unlocked
        /// </summary>
        public static bool QuestUnlocked(int quest)
        {
            return (quests[quest].actual >= quests[quest].maximum);
        }

        /// <summary>
        /// Draw the quest info
        /// </summary>
        public static void Draw(SpriteFont font)
        {
            if (timer >= 0)
            {
                if (timer < 40)
                {
                    timer += 0.1f;
                    signPosition.Y -= signPosition.Y / 10f;
                }
                else if (timer < 70)
                {
                    timer += 0.1f;
                    signPosition.Y += (-100 - signPosition.Y) / 10f;
                }
                else
                {
                    timer = -1;
                }
                PlanetoidGame.spriteBatch.Draw(questSign, signPosition, (completed ? Color.Lime : Color.White));
                Util.DrawCenteredText(font, quests[currentQuest].name + "\n" + (completed ? " COMPLETED!" : " in progress.." + (quests[currentQuest].maximum > 1 ? " " + quests[currentQuest].actual + "/" + quests[currentQuest].maximum : "")) + (currentQuest == 19 ? "\nClick on the NERD quest in the menu!" : ""), signPosition + new Vector2(200, 25), Util.PanelGray);
            }
        }
    }
}
