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
    public enum DealAction
    {
        Declare_War,
        Hate,
        Greeting,
        Claim_Peace,
        Claim_Alliance,
        Ask_Status,
        Trade,
        Ask_Help
    }

    public enum DealResult
    {
        None,
        No,
        Yes
    }

    [Serializable]
    public class Deal
    {
        public Deal()
        {
            index = 0;
        }
        /// <summary>
        /// The player who sent this Deal
        /// </summary>
        public int claimer;
        /// <summary>
        /// The answer to a previous deal: Yes None No
        /// </summary>
        public DealResult result;
        /// <summary>
        /// The action the deal contains
        /// </summary>
        public DealAction action;
        /// <summary>
        /// An index referring to a special phrase to use for each dealAction
        /// </summary>
        public short index;
    }

    /// <summary>
    /// Diplomacy Manager, last review on version 1.0.2
    /// </summary>
    public static class DiplomacyManager
    {
        public static void Initialize()
        {
            center = new Vector2(GameEngine.Game.GraphicsDevice.Viewport.Width / 2, GameEngine.Game.GraphicsDevice.Viewport.Height / 2);
            button_diplomacyPrev = new Button(center + new Vector2(-200, 30), "<<", 100, null);
            button_diplomacyNext = new Button(center + new Vector2(100, 30), ">>", 100, null);

            button_diplomacyAccept = new Button(center + new Vector2(-210, 100), "Accept", 200, 'A');
            button_diplomacyClose = new Button(center + new Vector2(10, 100), "Close", 200, 'B');

            button_diplomacyTreatAs = new Button(center + new Vector2(50, -104), "", 150, null);

            cam = new Camera(new Vector3(0, 5, 80));
        }

        //Variables
        static Button button_diplomacyClose;
        static Button button_diplomacyPrev;
        static Button button_diplomacyNext;
        static Button button_diplomacyAccept;
        static Button button_diplomacyTreatAs;

        static Camera cam;

        static Vector2 center;

        static int diplomacyPhase;
        static int diplomacyTarget;
        static int diplomacyDeal;

        static Deal deal;

        /// <summary>
        /// This double array will contain all the casual messages each specific deal will show to the player
        /// </summary>
        private static string[][] DealMessages = new string[][]
        {
            //DECLARE WAR
            new string[]{"We declare war to you!","WAAAAAR!","This is OUR space!","Prepare for extinction!","Sir, our noble race declare war to yours.\nYour sincere friends"},
            //HATE
            new string[]{"We hate you!","I am the president of our race, I HATE YOU!","You're not the welcome here.","This solar system is meant for us, not for you!"},
            //GREETING
            new string[]{"Hello friend!","Hi dear alien!","Hello :)","We like you :)","Greetings!"},
            //CLAIM PEACE
            new string[]{"We claim peace to you","Please, peace!!","Make love not war!!","We surrender, please stop this..."},
            //CLAIM ALLIANCE
            new string[]{"Our powerful civilization ask you to ally!","Do you want to ally with us?","We think an alliance could benefit both of us."},
            //ASK STATUS
            new string[]{"How are you?","How is going?","What's your status?","How's your planet's atmosphere?","How's your planet's life?"},
            //TRADE
            new string[]{"Do you want to trade with our great civilization?","Would you like to trade?","We would like to trade"},
            //ASK HELP
            new string[]{"Please help us!","Could you help us?","May you help us?","HELP!","We need your help now!!!"}
        };

        /// <summary>
        /// First half of answers are for the No, Second half for the Yes
        /// </summary>
        private static string[][] DealAnswers = new string[][]
        {
            //DECLARE WAR
            new string[]{},
            //HATE
            new string[]{},
            //GREETING
            new string[]{"What do you want from us?","...","Hi!","Hello!"},
            //CLAIM PEACE
            new string[]{"Look at you, miserable!","Why should we?","Okay... peace","Truce, from both!"},
            //CLAIM ALLIANCE
            new string[]{"No way, not now","Does it sound good for you?","Okay.. Comrade!","Fantastic, we accept!"},
            //ASK STATUS
            new string[]{"We are not that good","Things here are going bad :(","We're ok","Everything is going well"},
            //TRADE
            new string[]{"Well... nope","We usually don't trade with strangers","Okay, let's do it","Fine!"},
            //ASK HELP
            new string[]{"Forget it","We're sorry, we can't","Okay, we will help you","Hang On!"}
        };

        public static void Start()
        {
            button_diplomacyAccept.text = "Accept";
            diplomacyTarget = 1;
            diplomacyPhase = 0;
            diplomacyDeal = 0;
            while (PlayerManager.GetState(diplomacyTarget) < PlayerState.Human || diplomacyTarget == 0)
            {
                diplomacyTarget++;
                if (diplomacyTarget == 8)
                {
                    diplomacyTarget = 0;
                }

            }
        }

        public static void ReadMessages()
        {
            button_diplomacyAccept.text = "Delete";
            diplomacyPhase = 3;
        }

        public static bool Update()
        {
            button_diplomacyPrev.Update();
            button_diplomacyNext.Update();
            button_diplomacyAccept.Update();
            button_diplomacyClose.Update();

            switch (diplomacyPhase)
            {
                //CIVILIZATION OVERVIEW
                case 0:
                    //Go to previous race
                    if (button_diplomacyPrev.IsClicked())
                    {
                        do
                        {
                            diplomacyTarget--;
                            if (diplomacyTarget == -1)
                            {
                                diplomacyTarget = 7;
                            }
                        }
                        while (PlayerManager.GetState(diplomacyTarget) < PlayerState.Human || diplomacyTarget == 0);
                    }
                    //Go to next race
                    if (button_diplomacyNext.IsClicked())
                    {
                        do
                        {
                            diplomacyTarget++;
                            if (diplomacyTarget == 8)
                            {
                                diplomacyTarget = 0;
                            }
                        }
                        while (PlayerManager.GetState(diplomacyTarget) < PlayerState.Human || diplomacyTarget == 0);
                    }


                    if (button_diplomacyAccept.IsClicked())
                    {
                        if (PlayerManager.players[0].radarAmount > 0)
                        {
                            if (PlayerManager.players[diplomacyTarget].radarAmount > 0)
                            {
                                diplomacyPhase = 1;
                            }
                            else
                            {
                                TextBoard.AddMessage("The selected civilization doesn't have a Radar!");
                            }
                        }
                        else
                        {
                            TextBoard.AddMessage("You first have to build a Radar!");
                        }
                    }


                    button_diplomacyTreatAs.Update();
                    if (PlayerManager.GetFriendship(0, diplomacyTarget) == 1)
                    {
                        button_diplomacyTreatAs.text = "Treat as: Friend";
                    }
                    else
                    {
                        button_diplomacyTreatAs.text = "Treat as: Enemy";
                    }
                    if (button_diplomacyTreatAs.IsClicked())
                    {
                        if (button_diplomacyTreatAs.text[10] == 'E')
                        {
                            PlayerManager.SetAll(0, diplomacyTarget, 1);
                        }
                        else
                        {
                            PlayerManager.SetAll(0, diplomacyTarget, -1);
                        }
                    }

                    if (button_diplomacyClose.IsClicked())
                    {
                        return false;
                    }
                    break;
                //SELECT MESSAGE
                case 1:
                    if (button_diplomacyNext.IsClicked())
                    {
                        diplomacyDeal++;
                        if (diplomacyDeal > 7)
                        {
                            diplomacyDeal = 0;
                        }
                    }
                    if (button_diplomacyPrev.IsClicked())
                    {
                        diplomacyDeal--;
                        if (diplomacyDeal < 0)
                        {
                            diplomacyDeal = 7;
                        }
                    }
                    if (button_diplomacyAccept.IsClicked())
                    {
                        DiplomacyManager.AddDeal(0, diplomacyTarget, (DealAction)diplomacyDeal);
                        diplomacyPhase = 2;
                        diplomacyDeal = 0;
                    }

                    if (button_diplomacyClose.IsClicked())
                    {
                        diplomacyPhase = 0;
                        diplomacyDeal = 0;
                    }
                    break;
                //OKAY MESSAGE
                case 2:
                    if (button_diplomacyClose.IsClicked())
                    {
                        diplomacyPhase = 0;
                    }
                    break;
                //READ MESSAGES
                case 3:
                    deal = GetFirstDeal(0);
                    if (deal != null)
                    {
                        if (button_diplomacyAccept.IsClicked())
                        {
                            if (deal.result == DealResult.None)
                            {
                                AnswerDeal(0, deal.claimer, deal.action, DealResult.No);
                            }
                            RemoveFirstDeal(0);
                        }
                        if (deal.result == DealResult.None && deal.action >= DealAction.Greeting)
                        {
                            if (button_diplomacyNext.IsClicked())
                            {
                                if (deal.result == DealResult.None)
                                {
                                    AnswerDeal(0, deal.claimer, deal.action, DealResult.No);
                                }
                                RemoveFirstDeal(0);
                            }

                            if (button_diplomacyPrev.IsClicked())
                            {
                                if (deal.result == DealResult.None)
                                {
                                    AnswerDeal(0, deal.claimer, deal.action, DealResult.Yes);
                                    if (deal.action == DealAction.Claim_Alliance)
                                    {
                                        QuestManager.QuestCall(7);
                                    }
                                }
                                RemoveFirstDeal(0);
                            }
                        }
                    }

                    if (button_diplomacyClose.IsClicked())
                    {
                        return false;
                    }
                    break;
            }
            return true;
        }

        public static void Draw(SpriteFont font)
        {
            PlanetoidGame.spriteBatch.Begin();
            //DRAW DIPLOMACY MENU
            PlanetoidGame.spriteBatch.Draw(HUDManager.panel_deal, center - new Vector2(251, 176), Color.White);
            if (diplomacyPhase != 2)
            {
                button_diplomacyAccept.Draw(font, Color.LawnGreen);
            }
            button_diplomacyClose.Draw(font, Color.OrangeRed);

            switch (diplomacyPhase)
            {
                case 0:
                    button_diplomacyPrev.text = "<<";
                    button_diplomacyNext.text = ">>";
                    button_diplomacyPrev.Draw(font);
                    button_diplomacyNext.Draw(font);
                    button_diplomacyTreatAs.Draw(font);

                    Util.DrawCenteredText(font, "Use Accept to send a message:", new Vector2(center.X, center.Y - 150), Util.PanelGray);

                    if (PlayerManager.players[diplomacyTarget].cpuController == null)
                    {
                        diplomacyPhase = 1;
                        break;
                    }
                    Util.DrawCenteredText(font,
                       PlayerManager.players[diplomacyTarget].cpuController.Adjective + " " + RaceManager.GetRace(PlayerManager.GetRace(diplomacyTarget)) + " (" + PlayerManager.GetState(diplomacyTarget) + ")" +
                       "\n\nAbility: "+RaceManager.GetAbilityName(PlayerManager.GetRace(diplomacyTarget))+
                        "\nDiplomacy: " + PlayerManager.GetDualState(diplomacyTarget, 0)
                        + "\nStatus: " + PlayerManager.GetGeneralRelationLevel(diplomacyTarget, 0)
                        , center - new Vector2(0, 40), Util.PanelGray);
                    PlanetoidGame.spriteBatch.End();

                    GameEngine.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    GameEngine.Game.GraphicsDevice.BlendState = BlendState.Opaque;

                    Matrix m = Matrix.CreateRotationX(-MathHelper.PiOver2);
                    m.Translation = new Vector3(0, 7, -20);
                    foreach (ModelMesh mesh in RenderManager.hominidModel.Meshes)
                    {
                        foreach (Effect effect in mesh.Effects)
                        {
                            effect.Parameters["Texture"].SetValue(RaceManager.GetTexture(PlayerManager.GetRace(diplomacyTarget)));
                        }
                    }
                    Util.DrawModel(m, RenderManager.hominidModel, cam, Color.White);

                    break;
                //SELECT THE MESSAGE TO SEND
                case 1:
                    button_diplomacyPrev.Draw(font);
                    button_diplomacyNext.Draw(font);
                    Util.DrawCenteredText(font, "Select the message: (" + diplomacyDeal + "/7)", new Vector2(center.X, center.Y - 100), Util.PanelGray);
                    Util.DrawCenteredText(font, ((DealAction)diplomacyDeal).ToString().Replace('_', ' '), center, Util.PanelGray);
                    PlanetoidGame.spriteBatch.End();
                    break;
                //OKAY MESSAGE
                case 2:
                    Util.DrawCenteredText(font, "The message has been sent!", center, Util.PanelGray);
                    PlanetoidGame.spriteBatch.End();
                    break;
                //READ MESSAGE
                case 3:
                    //No Message
                    if (deal == null)
                    {
                        Util.DrawCenteredText(font, "There are no messages.", center, Util.PanelGray);
                    }
                    //Answer Message
                    else
                    {
                        Util.DrawCenteredText(font, "Message from " + RaceManager.GetRace(PlayerManager.GetRace(deal.claimer)) + (deal.result > DealResult.None ? " in response to " + deal.action.ToString().Replace('_', ' ') + ":" : ":"), new Vector2(center.X, center.Y - 100), Util.PanelGray);
                        if (deal.result > DealResult.None)
                        {
                            Util.DrawCenteredText(font, DealAnswers[(int)deal.action][deal.index], center, Util.PanelGray);
                        }
                        //Question Message
                        //this condition is checked because Declare_War and Hate can only be deleted
                        else
                        {
                            if (deal.action >= DealAction.Greeting)
                            {
                                switch (deal.action)
                                {
                                    case DealAction.Greeting:
                                        button_diplomacyPrev.text = "Hi!";
                                        button_diplomacyNext.text = "...";
                                        break;
                                    case DealAction.Ask_Status:
                                        button_diplomacyPrev.text = "Good";
                                        button_diplomacyNext.text = "Bad";
                                        break;
                                    default:
                                        button_diplomacyPrev.text = "Yes";
                                        button_diplomacyNext.text = "No";
                                        break;
                                }
                                button_diplomacyPrev.Draw(font, Color.LawnGreen);
                                button_diplomacyNext.Draw(font, Color.OrangeRed);
                            }
                            Util.DrawCenteredText(font, DealMessages[(int)deal.action][deal.index], center, Util.PanelGray);
                        }
                    }
                    PlanetoidGame.spriteBatch.End();
                    break;
            }
        }

        public static void AddDeal(int Claimer, int Target, DealAction Action)
        {
            if (Claimer > 0 && PlayerManager.players[Claimer].messageFlags[(Target * 8) + (int)Action] > 0)
            {
                return;
            }
            if (!PlayerManager.players[Target].active_deals.Exists(d => d.claimer == Claimer && d.action == Action)
                && !PlayerManager.players[Claimer].active_deals.Exists(a => a.claimer == Target && a.result > DealResult.None)
                && PlayerManager.players[0].radarAmount > 0)
            {
                PlayerManager.players[Target].active_deals.Add(new Deal
                {
                    claimer = Claimer,
                    action = Action,
                    index = (short)Util.random.Next(DealMessages[(int)Action].Length)
                });
                if (Target == 0)
                {
                    AudioManager.Play("message");
                    HUDManager.message_timer = 60;
                    PlayerManager.players[Claimer].messageFlags[(Target * 8) + (int)Action] = 400;
                }
            }
        }

        public static void AnswerDeal(int Claimer, int Target, DealAction Action, DealResult Result)
        {
            if (PlayerManager.players[Target].active_deals.Find(d => d.claimer == Claimer && d.action == Action && d.result == Result) == null)
            {
                PlayerManager.players[Target].active_deals.Add(new Deal
                {
                    claimer = Claimer,
                    action = Action,
                    result = Result,
                    index = (short)(Util.random.Next(DealAnswers[(int)Action].Length / 2) + (Result == DealResult.No ? 0 : DealAnswers[(int)Action].Length / 2))
                });

                if (Target == 0)
                {
                    AudioManager.Play("message");
                    HUDManager.message_timer = 60;
                }
            }
        }

        /// <summary>
        /// Remove first deal of a player
        /// </summary>
        public static void RemoveFirstDeal(int index)
        {
            PlayerManager.players[index].active_deals.RemoveAt(0);
        }

        /// <summary>
        /// Get the first deal of a player, null if not found
        /// </summary>
        public static Deal GetFirstDeal(int index)
        {
            if (PlayerManager.players[index].active_deals.Count > 0)
            {
                return PlayerManager.players[index].active_deals[0];
            }
            return null;
        }
    }
}
