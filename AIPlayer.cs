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
    public enum PlanetAction
    {
        Live,
        Defend,
        Attack
    }

    public class PlanetInterface
    {
        public Planet planet;
        public PlanetAction action;
        public float danger;
    }

    
    [Serializable]
    public class AIPlayer
    {
        public AIPlayer() { }
        /// <summary>
        /// Create a new AIPlayer, specify Egoism, Skill and Pacifism values from 0.0f to 1.0f, also the AIplayer needs the playerIndex from PlayerManager
        /// </summary>
        public AIPlayer(float Egoism, float Skill, float Pacifism,int playerIndex)
        {
            stat_egoism = Egoism;
            stat_pacifism = Pacifism;

            //Modify stats basing on the CPU type
            switch (PlayerManager.GetState(playerIndex))
            {
                case PlayerState.Dumb:
                    stat_skill = MathHelper.Clamp(Skill / 2f, 0, 0.25f);
                    break;
                case PlayerState.Normal:
                    stat_skill = MathHelper.Clamp(Skill, 0.25f, 0.5f);
                    break;
                case PlayerState.Challenging:
                    stat_skill = MathHelper.Clamp(Skill * 2f,0.5f,0.75f);
                    stat_pacifism /= 1.2f;
                    break;
                case PlayerState.Hardcore:
                    stat_skill = 1;
                    //stat_pacifism /= 2;
                    stat_egoism /= 2;
                    break;
            }

            myPlayerIndex = playerIndex;

            researchTimer = 4 * (2 + stat_skill*2);

            //Get the race chosen for you
            int race = PlayerManager.GetRace(myPlayerIndex);

            //Initialize resources
            MyEnergy = 1000 + (1 - stat_skill) * 1000;
            MyKeldanyum = 2000 + (1 - stat_skill) * 1500;

            //Pick a random planet to start playing
            myPlanets = new List<PlanetInterface>();
            myPlanets.Add(new PlanetInterface());
            myPlanets[0].planet = Player.PickFreePlanet(myPlayerIndex);
            myPlanets[0].action = PlanetAction.Live;
        }

        /// <summary>
        /// The AI Player has three main values to modify it's behaviour
        /// The "egoism" parameter acts on every decision which involves other players
        /// </summary>
        public float stat_egoism;
        /// <summary>
        ///The "skill" parameter acts on:
        ///Decisions
        ///Answer speed (to deals)
        ///correct decision (following the "best pattern")
        /// </summary>
        public float stat_skill;
        /// <summary>
        /// The "pacifism" parameter acts joined with "egoism" on some political decisions
        /// </summary>
        public float stat_pacifism;

        /// <summary>
        /// This list contains all planets interested by the AI
        /// Planets are sorted by importance
        /// It can't be empty, an empty list means a defeated AI
        /// It starts with the born planet
        /// </summary>
        public List<PlanetInterface> myPlanets;
        public List<int> indices_planets;

        /// <summary>
        /// This index refers to (in "PlayerManager.Players.cpuController")
        /// </summary>
        public int myPlayerIndex;

        /// <summary>
        ///This list contains all the actions the AI take in consideration during the TakeDecision() call
        ///The list is cleared every time;
        ///List<AIAction> actions;
        /// </summary>
        int decision;
        int decisionPriority;
        
        /// <summary>
        /// The timer which tells the AI whenever a deal can be read
        /// Without this all the deals would be analyze immediately, just a little delay
        /// </summary>
        public float dealTimer;

        /// <summary>
        /// Avoid the AI to research too much in too little time
        /// </summary>
        private float researchTimer;

        public float decisionTimer;

        private int MyRace
        {
            get { return PlayerManager.players[myPlayerIndex].race; }
        }

        private float MyKeldanyum
        {
            get { return PlayerManager.players[myPlayerIndex].keldanyum; }
            set { PlayerManager.players[myPlayerIndex].keldanyum = value; }
        }

        private float MyEnergy
        {
            get { return PlayerManager.players[myPlayerIndex].energy; }
            set { PlayerManager.players[myPlayerIndex].energy = value; }
        }

        public void InSerialization()
        {
            indices_planets = new List<int>();
            for (int a = 0; a < myPlanets.Count; a++)
            {
                indices_planets.Add(GameEngine.planets.IndexOf(myPlanets[a].planet));
            }
            myPlanets.Clear();
        }

        public void OutSerialization()
        {
            myPlanets = new List<PlanetInterface>();
            for (int a = 0; a < indices_planets.Count; a++)
            {
                myPlanets.Add(new PlanetInterface { planet = GameEngine.planets[indices_planets[a]] });
            }
            indices_planets.Clear();
        }

        public PlanetInterface GetInterface(Planet planet)
        {
            return (myPlanets.Find(p => p.planet == planet));
        }

        /// <summary>
        /// Get an adjective describing this race
        /// </summary>
        public string Adjective
        {
            get
            {
                string returner="";
                if (stat_skill < 0.2f)
                {
                    returner = "Dumb";
                }
                else if (stat_egoism < 0.3f)
                {
                    if (stat_pacifism < 0.3f)
                    {
                        returner = "Careless";
                    }
                    else if (stat_pacifism < 0.6f)
                    {
                        returner = "Dubious";
                    }
                    else
                    {
                        returner = "Peaceful";
                    }
                }
                else if (stat_egoism<0.6f)
                {
                    if (stat_pacifism < 0.3f)
                    {
                        returner = "Conqueror";
                    }
                    else if (stat_pacifism < 0.6f)
                    {
                        returner = "Dubious";
                    }
                    else
                    {
                        returner = "Weird";
                    }
                }
                else
                {
                    if (stat_pacifism < 0.3f)
                    {
                        returner = "Disgraceful";
                    }
                    else if (stat_pacifism < 0.6f)
                    {
                        returner ="Opportunist";
                    }
                    else
                    {
                        returner = "Selfish";
                    }
                }
                return returner;
            }
        }

        private PlanetInterface currentInterface;
        private int index;

        /// <summary>
        /// Ask the AI player to take a decision about the current game state
        /// </summary>
        public void TakeDecision(float elapsed)
        {
            //Wait
            decisionTimer -= elapsed;
            if (researchTimer > 0)
            {
                researchTimer -= elapsed;
            }
            #region planet actions
            if (decisionTimer <= 0)
            {
                //Set the timer for the next decision
                decisionTimer = 8 + (1 - stat_skill) * 4;

                //Always give something to the cpu
                PlayerManager.ChangeEnergy(myPlayerIndex, stat_skill * 5);
                PlayerManager.ChangeKeldanyum(myPlayerIndex, stat_skill * 5);

                //And take it out of troubles...
                if (MyEnergy < 0) PlayerManager.SetEnergy(myPlayerIndex, 50);
                if (MyKeldanyum < 0) PlayerManager.SetKeldanyum(myPlayerIndex, 50);
                if (stat_skill == 1)
                {
                    if (MyEnergy > MyKeldanyum + 1000)
                    {
                        MyEnergy -= 250;
                        MyKeldanyum += 250;
                    }
                    else if (MyKeldanyum > MyEnergy + 1000)
                    {
                        MyKeldanyum -= 250;
                        MyEnergy += 250;
                    }
                }

                //CPU skill stat will grow constantly
                if (stat_skill < 1)
                {
                    stat_skill += 0.00025f;
                }

                index = 0;

                //First , the planets owned by this AI must be safe
                //So start a cycle on them to check their conditions
                for (int a = 0; a < myPlanets.Count; a++)
                {
                    //Reset the decision, waiting is considered if nothing has to be done
                    decision = -1;
                    decisionPriority = 0;

                    currentInterface = myPlanets[a];
                    //If the current planet is dead, remove it from the list
                    if (currentInterface.planet.life <= 0)
                    {
                        myPlanets.RemoveAt(a);
                        a--;
                        continue;
                    }

                    //ATMOSPHERE CONTROL
                    //This is important in all of the planets actions so keep out of the switch
                    //If the race doesn't tolerate the planet's atmosphere
                    if (!RaceManager.Tolerate(MyRace, currentInterface.planet.atmosphere))
                    {
                        if (CountBuildings(BuildingType.House, false) == 0)
                        {
                            EvaluateAction(BuildingType.House, 40);
                        }
                        //If presents turbina are not enough to preserve the atmosphere
                        else if (CountBuildings(BuildingType.Turbina, false) < 2 + currentInterface.planet.trees.Count / 2)
                        {
                            //This planet needs a Turbina
                            EvaluateAction(BuildingType.Turbina, 40);
                        }
                        else
                        {
                            //If enough turbina are present on this planet, switch them on
                            SwitchTurbina(true);
                        }
                    }
                    //Manage danger level
                    //this is really important, if a planet's danger level is too high
                    //the AI will adapt a different building scheme, with different priorities
                    if (currentInterface.danger > 0)
                    {
                        if (currentInterface.danger > 1) currentInterface.danger = 1;
                        currentInterface.danger -= 0.05f;
                        if (currentInterface.danger > 0.5f)
                        {
                            if (currentInterface.planet.DominatingRace() != MyRace)
                            {
                                currentInterface.action = PlanetAction.Attack;
                            }
                            else
                            {
                                currentInterface.action = PlanetAction.Defend;
                            }
                        }
                    }
                    else
                    {
                        currentInterface.action = PlanetAction.Live;
                    }

                    //If the planet is owned by this AI

                    switch (currentInterface.action)
                    {
                        case PlanetAction.Live:
                            ManageResources(5);
                            ManageTurrets(3);
                            ManageHouse(4);
                            ManageSchool(3, 3, 3 - (int)(stat_pacifism * 2), 2);
                            ManageRockets(2, 0, 0);
                            ManageShields(2);
                            ManageRadars(3);
                            break;
                        case PlanetAction.Defend:
                            ManageResources(1);
                            ManageTurrets(5);
                            ManageHouse(3);
                            ManageSchool(-2, 4, 4, 0);
                            ManageRockets(0, 4, 0);
                            ManageShields(4);
                            ManageRadars(0);
                            break;
                        case PlanetAction.Attack:
                            ManageResources(2);
                            ManageTurrets(2);
                            ManageHouse(4);
                            ManageSchool(2, 2, 6, 0);
                            ManageRockets(5, 5, 5);
                            ManageShields(2);
                            ManageRadars(0);
                            break;
                    }

                    //Make the decision take effect
                    if (decision > -1)
                    {
                        //TextBoard.AddMessage(RaceManager.GetRace(PlayerManager.GetRace(myPlayerIndex)) + " wants to build: " + (BuildingType)decision+" on "+currentInterface.planet.name);
                        Build((BuildingType)decision);
                    }
                }
            }
            #endregion

            //Decrease message flags, which determinate the time interval between messages
            for (int p = 0; p < 64; p++)
            {
                if (PlayerManager.players[myPlayerIndex].messageFlags[p] > 0)
                {
                    PlayerManager.players[myPlayerIndex].messageFlags[p] -= 0.035f;
                }
            }

            if (PlayerManager.players[myPlayerIndex].radarAmount>0)
            {
                float friend;
                float trust;
                DualState state;
                dealTimer -= elapsed;
                if (dealTimer <= 0)
                {
                    dealTimer = 25 + (15 * (1 - stat_skill));
                    //Read the first deal
                    Deal deal = DiplomacyManager.GetFirstDeal(myPlayerIndex);
                    //INBOX MANAGEMENT
                    if (deal != null)
                    {
                        //Remove it from the list
                        DiplomacyManager.RemoveFirstDeal(myPlayerIndex);
                        //Get values (only to not use functions every time)
                        friend = (float)PlayerManager.GetFriendship(myPlayerIndex, deal.claimer);
                        trust = PlayerManager.GetTrust(myPlayerIndex, deal.claimer);
                        state = PlayerManager.GetDualState(myPlayerIndex, deal.claimer);


                        //Trust influences friendship
                        PlayerManager.ChangeFriendship(myPlayerIndex, deal.claimer, trust / 100f);

                        //If the deal is a response from a deal I sent
                        #region ANSWER INBOX
                        if (deal.result > DealResult.None)
                        {
                            switch (deal.action)
                            {
                                //Someone answered to a your help ask
                                //"pacifism" is used
                                //"egoism" is used
                                case DealAction.Ask_Help:
                                    if (deal.result == DealResult.Yes)
                                    {
                                        //Someone gave you help!
                                        PlayerManager.ChangeFriendship(myPlayerIndex, deal.claimer, 0.5f + stat_pacifism / 4f - stat_egoism / 4f);
                                        PlayerManager.ChangeTrust(myPlayerIndex, deal.claimer, 0.2f + stat_pacifism / 4f - stat_egoism / 4f);
                                    }
                                    else
                                    {
                                        //Someone didn't care about you
                                        PlayerManager.ChangeFriendship(myPlayerIndex, deal.claimer, -0.5f + stat_pacifism / 4f - stat_egoism / 4f);
                                        PlayerManager.ChangeTrust(myPlayerIndex, deal.claimer, -0.2f + stat_pacifism / 4f - stat_egoism / 4f);
                                    }
                                    break;
                                case DealAction.Ask_Status:
                                    break;
                                case DealAction.Claim_Alliance:
                                    if (deal.result == DealResult.Yes)
                                    {
                                        PlayerManager.ChangeFriendship(myPlayerIndex, deal.claimer, 1);
                                        PlayerManager.ChangeTrust(myPlayerIndex, deal.claimer, 1);
                                        PlayerManager.SetDualState(myPlayerIndex, deal.claimer, DualState.Alliance);
                                    }
                                    else
                                    {
                                        PlayerManager.ChangeFriendship(myPlayerIndex, deal.claimer, -0.5f);
                                        PlayerManager.ChangeTrust(myPlayerIndex, deal.claimer, -0.6f);
                                    }
                                    break;
                                case DealAction.Claim_Peace:
                                    if (deal.result == DealResult.Yes)
                                    {
                                        PlayerManager.SetDualState(myPlayerIndex, deal.claimer, DualState.Normal);
                                        PlayerManager.ChangeFriendship(myPlayerIndex, deal.claimer, 0.2f);
                                        PlayerManager.ChangeTrust(myPlayerIndex, deal.claimer, 0.5f);
                                    }
                                    else
                                    {
                                        PlayerManager.ChangeFriendship(myPlayerIndex, deal.claimer, -0.3f);
                                        PlayerManager.ChangeTrust(myPlayerIndex, deal.claimer, -0.2f);
                                    }
                                    break;
                                case DealAction.Declare_War:
                                    break;
                                case DealAction.Greeting:
                                    if (deal.result == DealResult.Yes)
                                    {
                                        //A low trust can influence the raising of a friendship
                                        PlayerManager.ChangeFriendship(myPlayerIndex, deal.claimer, ((friend - 0.4f) / 7 + stat_pacifism - stat_skill / 2) / (2 + (1 - trust)));
                                        //This can also raise trust
                                        PlayerManager.ChangeTrust(myPlayerIndex, deal.claimer, ((friend - 0.4f) / 7 + stat_pacifism - stat_skill / 2) / 5);
                                    }
                                    else
                                    {
                                        PlayerManager.ChangeTrust(myPlayerIndex, deal.claimer, -0.05f);
                                        PlayerManager.ChangeFriendship(myPlayerIndex, deal.claimer, 0.1f);
                                    }
                                    break;
                                case DealAction.Hate:
                                    break;
                                //Accepting the trade will result in a increase of friendship
                                //Denying it will result in a decrease of general direction (trust)
                                case DealAction.Trade:
                                    if (deal.result == DealResult.Yes)
                                    {
                                        PlayerManager.ChangeFriendship(myPlayerIndex, deal.claimer, 0.1f + stat_pacifism / 5f);
                                        PerformTrade(deal.claimer,myPlayerIndex);
                                    }
                                    else
                                    {
                                        PlayerManager.ChangeTrust(myPlayerIndex, deal.claimer, -0.2f + stat_pacifism / 10f);
                                    }
                                    break;
                            }
                        }
                        #endregion
                        #region INBOX
                        else
                        {
                            switch (deal.action)
                            {
                                //Someone is requesting help to this AI
                                //"pacifism" is used
                                //"egoism" is used
                                //"skill" is used
                                case DealAction.Ask_Help:
                                    if (friend >= 0.5f - stat_pacifism / 2f && stat_pacifism > 0.2f)
                                    {
                                        if (trust > stat_egoism)
                                        {
                                            //Okay, help the claimer
                                            DiplomacyManager.AnswerDeal(myPlayerIndex, deal.claimer, DealAction.Ask_Help, DealResult.Yes);
                                            //Give to the claimer some resources
                                            if (MyKeldanyum > 400 * myPlanets.Count && MyEnergy > 150 * myPlanets.Count)
                                            {
                                                Point resources;
                                                resources.X = (int)(200 + friend * 50 - stat_egoism * 50 + stat_pacifism * 5 + trust * 5);
                                                resources.Y = (int)(175 + friend * 25 - stat_egoism * 40 + trust * 5);
                                                if (deal.claimer == 0)
                                                {
                                                    TextBoard.AddMessage("You received " + resources.X + " of keldanyum and " + resources.Y + " of energy from " + Adjective + " " + RaceManager.GetRace(MyRace));
                                                    PlayerManager.ChangeKeldanyum(myPlayerIndex, -resources.X);
                                                    PlayerManager.ChangeEnergy(myPlayerIndex, -resources.Y);

                                                    PlayerManager.ChangeKeldanyum(deal.claimer, resources.X);
                                                    PlayerManager.ChangeEnergy(deal.claimer, resources.Y);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //Don't do anything
                                            DiplomacyManager.AnswerDeal(myPlayerIndex, deal.claimer, DealAction.Ask_Help, DealResult.No);
                                        }
                                    }
                                    else if (stat_skill > 0.5f && stat_egoism > 0.5f)
                                    {
                                        //Say yes but go and attack
                                        DiplomacyManager.AnswerDeal(myPlayerIndex, deal.claimer, DealAction.Ask_Help, DealResult.Yes);
                                        //DO ATTACK CODE HERE
                                        //Search for the planet owned by "deal.claimer"
                                        int p = 0;
                                        for (; p < GameEngine.planets.Count; p++)
                                        {
                                            if (GotPlanet(GameEngine.planets[p]) == false && GameEngine.planets[p].DominatingRace() == PlayerManager.GetRace(deal.claimer))
                                            {
                                                myPlanets.Add(new PlanetInterface { action = PlanetAction.Attack, planet = GameEngine.planets[p] });
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //Say no
                                        DiplomacyManager.AnswerDeal(myPlayerIndex, deal.claimer, DealAction.Ask_Help, DealResult.No);
                                    }
                                    break;
                                //Ask the status to the AI:
                                //The AI could answer in different ways
                                //If trust is not high enough or there is a war on route the AI could lie
                                //If the friendship is not high enough the AI could not answer
                                case DealAction.Ask_Status:
                                    if (stat_egoism < 0.6f && state != DualState.War)
                                    {
                                        //"skill" is used                           
                                        if (trust - stat_skill < friend)
                                        {
                                            //lie, say that you feel good
                                            DiplomacyManager.AnswerDeal(myPlayerIndex, deal.claimer, DealAction.Ask_Status, DealResult.Yes);
                                        }
                                        else
                                        {
                                            //say the truth
                                            //very stupid AI will never lie about its situation
                                            Point needs = PlayerManager.NeededResources(myPlayerIndex);
                                            if (needs.X > 300 || needs.Y > 300 || PlayerManager.DeathRisk(myPlayerIndex) > 85)
                                            {
                                                //you need resources
                                                DiplomacyManager.AnswerDeal(myPlayerIndex, deal.claimer, DealAction.Ask_Status, DealResult.No);
                                            }
                                            else
                                            {
                                                //you don't need anything, you feel good
                                                DiplomacyManager.AnswerDeal(myPlayerIndex, deal.claimer, DealAction.Ask_Status, DealResult.Yes);
                                            }
                                        }
                                    }
                                    break;
                                //Ask the AI to set up an alliance
                                //The AI can accept if the friendship and trust stat are high enough
                                //Alliance can be broken with military performances or bad deals
                                //"pacifism" is used
                                //"egoism" is used
                                //"skill" is used
                                case DealAction.Claim_Alliance:
                                    if (stat_egoism < 0.7f && stat_pacifism > 0.4f && friend > 0.6 - stat_pacifism / 5f && trust > 0.5f - stat_skill / 2f && state != DualState.War)
                                    {
                                        PlayerManager.SetDualState(myPlayerIndex, deal.claimer, DualState.Alliance);
                                        PlayerManager.ChangeFriendship(myPlayerIndex, deal.claimer, 0.1f);
                                        PlayerManager.ChangeTrust(myPlayerIndex, deal.claimer, (1 - stat_skill) / 5f);
                                        DiplomacyManager.AnswerDeal(myPlayerIndex, deal.claimer, DealAction.Claim_Alliance, DealResult.Yes);
                                        if (deal.claimer == 0)
                                        {
                                            QuestManager.QuestCall(7);
                                        }
                                    }
                                    else
                                    {
                                        PlayerManager.ChangeFriendship(myPlayerIndex, deal.claimer, -0.1f);
                                        DiplomacyManager.AnswerDeal(myPlayerIndex, deal.claimer, DealAction.Claim_Alliance, DealResult.No);
                                    }
                                    break;
                                //Ask the AI to stop war
                                //Works only if the DualState is on war
                                //"skill" is used
                                //"pacifism" is used
                                //"egoism" is used
                                case DealAction.Claim_Peace:
                                    if (trust > stat_skill / 2f && state == DualState.War && stat_pacifism > stat_egoism)
                                    {
                                        PlayerManager.ChangeTrust(myPlayerIndex, deal.claimer, 0.5f);
                                        //for now it is so, it will not be so easy though
                                        PlayerManager.SetDualState(myPlayerIndex, deal.claimer, DualState.Normal);
                                        DiplomacyManager.AnswerDeal(myPlayerIndex, deal.claimer, DealAction.Claim_Peace, DealResult.Yes);
                                    }
                                    else
                                    {
                                        DiplomacyManager.AnswerDeal(myPlayerIndex, deal.claimer, DealAction.Claim_Peace, DealResult.No);
                                        PlayerManager.ChangeTrust(myPlayerIndex, deal.claimer, -0.02f);
                                    }
                                    break;
                                //Declare war to the AI
                                //Decreases lot of trust, especially if the DualState is alliance
                                //"pacifism" is used
                                case DealAction.Declare_War:
                                    PlayerManager.ChangeFriendship(myPlayerIndex, deal.claimer, -0.4f);
                                    if (state == DualState.Alliance)
                                    {
                                        PlayerManager.ChangeTrust(myPlayerIndex, deal.claimer, -0.8f);
                                        if (stat_pacifism > 0.5f)
                                        {
                                            stat_pacifism -= 0.1f;
                                        }
                                    }
                                    else
                                    {
                                        PlayerManager.ChangeTrust(myPlayerIndex, deal.claimer, -0.5f);
                                    }
                                    PlayerManager.SetDualState(myPlayerIndex, deal.claimer, DualState.War);
                                    break;
                                //Say hello to the AI
                                //"egoism" is used
                                //"pacifism" is used
                                //"skill" is used
                                case DealAction.Greeting:
                                    if (stat_egoism < 0.7f)
                                    {
                                        if (friend > 0.2f)
                                        {
                                            if (friend < 1)
                                            {
                                                //A low trust can influence the raising of a friendship
                                                PlayerManager.ChangeFriendship(myPlayerIndex, deal.claimer, ((friend - 0.4f) / 5 + stat_pacifism) / (2 + (1 - trust)));
                                                PlayerManager.ChangeTrust(myPlayerIndex, deal.claimer, ((friend - 0.4f) / 5 + stat_pacifism) / 8);
                                                DiplomacyManager.AnswerDeal(myPlayerIndex, deal.claimer, DealAction.Greeting, DealResult.Yes);
                                            }
                                            else
                                            {
                                                //This can also raise trust
                                                PlayerManager.ChangeTrust(myPlayerIndex, deal.claimer, ((friend - 0.4f) / 5 + stat_pacifism ) / 4);
                                                DiplomacyManager.AnswerDeal(myPlayerIndex, deal.claimer, DealAction.Greeting, DealResult.Yes);
                                            }
                                        }
                                        //No, not enough friendship
                                        else
                                        {
                                            PlayerManager.ChangeTrust(myPlayerIndex, deal.claimer, 0.15f - friend);
                                            DiplomacyManager.AnswerDeal(myPlayerIndex, deal.claimer, DealAction.Greeting, DealResult.No);
                                        }
                                    }
                                    //So egoist that no back answer is given
                                    break;
                                //Say bad things to the AI
                                case DealAction.Hate:
                                    PlayerManager.ChangeFriendship(myPlayerIndex, deal.claimer, (0.5f - friend) / 5);
                                    PlayerManager.ChangeTrust(myPlayerIndex, deal.claimer, -0.2f + (1 - stat_skill) / 5f);
                                    break;
                                //Trade with the AI
                                case DealAction.Trade:
                                    if (trust > 0.3f)
                                    {
                                        Point need = PlayerManager.NeededResources(myPlayerIndex);
                                        if (need.X > 400 || need.Y > 300)
                                        {
                                            PlayerManager.ChangeFriendship(myPlayerIndex, deal.claimer, 0.1f);
                                            PlayerManager.ChangeTrust(myPlayerIndex, deal.claimer, 0.1f);
                                            DiplomacyManager.AnswerDeal(myPlayerIndex, deal.claimer, DealAction.Trade, DealResult.Yes);
                                            PerformTrade(myPlayerIndex, deal.claimer);
                                        }
                                    }
                                    else
                                    {
                                        DiplomacyManager.AnswerDeal(myPlayerIndex, deal.claimer, DealAction.Trade, DealResult.No);
                                    }
                                    break;
                            }
                        }
                        #endregion
                    }
                    #region OUTBOX
                    //OUTBOX MANAGEMENT
                    int risk = PlayerManager.DeathRisk(myPlayerIndex);

                    for (int c = 0; c < PlayerManager.players.Length; c++)
                    {
                        //The player is not this AI and it is alive
                        if (c != myPlayerIndex && PlayerManager.GetState(c) > PlayerState.Open)
                        {
                            friend = PlayerManager.GetFriendship(myPlayerIndex, c);
                            trust = PlayerManager.GetTrust(myPlayerIndex, c);
                            state = PlayerManager.GetDualState(myPlayerIndex, c);

                            //ALL STAT ARE USED
                            //EGOISM
                            //PACIFISM
                            //SKILL

                            //#### ASK HELP + TRADE #####
                            //Ask help only if the risk of death is high enough
                            if (state!=DualState.War && risk / 100f >= friend - trust - stat_egoism / 2f && trust > 0.3f)
                            {
                                Point need = PlayerManager.NeededResources(myPlayerIndex);
                                if (need.X > 400 || need.Y > 300)
                                {
                                    DiplomacyManager.AddDeal(myPlayerIndex, c, DealAction.Trade);
                                }
                                else
                                {
                                    DiplomacyManager.AddDeal(myPlayerIndex, c, DealAction.Ask_Status);
                                }
                                continue;
                            }

                            //ASK STATUS
                            //To ask the status to another AI, this AI must not be egoist
                            //The friendship must be at least 0.3f, trust must be greater than skill
                            //And most of all, there must not be a war in progress!
                            if (state != DualState.War && stat_egoism < stat_pacifism && friend >= 0.3f)
                            {
                                //Prevent the AI to send too many of these
                                if (DateTime.Now.Second % 3 == 0)
                                {
                                    DiplomacyManager.AddDeal(myPlayerIndex, c, DealAction.Ask_Status);
                                    continue;
                                }
                            }

                            //CLAIM ALLIANCE
                            //We use the same conditions for the acceptance of the alliance request
                            if (state != DualState.Alliance && stat_egoism < 0.7f && stat_pacifism > 0.4f && friend > 0.6 - stat_pacifism / 5f && trust > 0.5f - stat_skill / 2f)
                            {
                                DiplomacyManager.AddDeal(myPlayerIndex, c, DealAction.Claim_Alliance);
                                continue;
                            }

                            //CLAIM PEACE
                            if (state == DualState.War && risk > 95 && trust > stat_pacifism && friend < 0.2f && stat_egoism > stat_skill / 2f)
                            {
                                DiplomacyManager.AddDeal(myPlayerIndex, c, DealAction.Claim_Peace);
                                continue;
                            }

                            //DECLARE WAR
                            if (state != DualState.War && trust < stat_pacifism && friend < 0.2f)
                            {
                                PlayerManager.SetDualState(myPlayerIndex, c, DualState.War);
                                DiplomacyManager.AddDeal(myPlayerIndex, c, DealAction.Declare_War);
                                continue;
                            }

                            //GREETING
                            if (trust > 0.4f - stat_pacifism/2f && state != DualState.War && stat_egoism < 0.7f /*&& friend <= stat_pacifism*/)
                            {
                                DiplomacyManager.AddDeal(myPlayerIndex, c, DealAction.Greeting);
                                continue;
                            }

                            //HATE
                            //now it is used only when received a bad message
                            if ((deal!=null && deal.action<DealAction.Greeting && deal.claimer==c) && state != DualState.Alliance && stat_egoism < 0.7f && friend < stat_pacifism)
                            {
                                //Prevent the AI to send too many of these
                                PlayerManager.ChangeFriendship(myPlayerIndex, c, -0.05f);
                                PlayerManager.ChangeTrust(myPlayerIndex, c, -0.05f);
                                DiplomacyManager.AddDeal(myPlayerIndex, c, DealAction.Hate);
                                continue;
                            }
                        }
                    }

                    #endregion
                }
            }
        }

        public void ManageRadars(int priority)
        {
            if (PlayerManager.players[myPlayerIndex].radarAmount<1)
            {
                EvaluateAction(BuildingType.Radar, (int)(4 - stat_egoism * 2 + stat_pacifism * 5)+priority);
            }
        }

        public void ManageTurrets(int priority)
        {
            if (CountBuildings(BuildingType.Turret,false) < currentInterface.planet.life / 10f + stat_skill * 2)
            {
                EvaluateAction(BuildingType.Turret, (int)(currentInterface.planet.life / 10f + stat_skill * 2) - CountBuildings(BuildingType.Turret, false));
            }
            if (CountBuildings(BuildingType.SAgun, false) < currentInterface.planet.radius / 10f - stat_pacifism * 5 + stat_egoism * 2)
            {
                EvaluateAction(BuildingType.SAgun, (int)(currentInterface.planet.radius / 10f - stat_pacifism * 5 + stat_egoism * 2) + (currentInterface.action == PlanetAction.Defend ? 20 : 0));
            }
        }

        public void ManageShields(int priority)
        {
            if (CountBuildings(BuildingType.Repulser, false) * 3 + MyEnergy / 2000 < currentInterface.planet.life / 10f + stat_skill * 2)
            {
                EvaluateAction(BuildingType.Repulser, (int)(currentInterface.planet.life / 10f + stat_skill * 2 + MyEnergy / 2000 - 3 * CountBuildings(BuildingType.Repulser, false)));
            }
            else
            {
                SwitchShield((MyEnergy > 500 || currentInterface.action == PlanetAction.Defend));
            }
        }

        public void ManageSchool(int priority,int builders,int soldiers,int pilots)
        {
            int num = CountBuildings(BuildingType.School, false);
            if (num < 2)
            {
                EvaluateAction(BuildingType.School, (int)(2 - num + stat_skill * 2));
                return;
            }
            School school=(School)FirstBuilding(BuildingType.School,false);
            if (school!=null)
            {
                ManageHominids(school,builders, soldiers, pilots);
            }

        }

        public void ManageHouse(int priority)
        {
            if (currentInterface.planet.hominids.Count < currentInterface.planet.maxPopulation / 2f + priority && CountBuildings(BuildingType.House,false) < currentInterface.planet.radius / 20)
            {
                EvaluateAction(BuildingType.House, (int)(currentInterface.planet.maxPopulation / 2f + priority));
            }
        }

        public void ManageResources(int priority)
        {
            if (CountBuildings(BuildingType.Extractor, false) <= currentInterface.planet.radius / 15f)
            {
                EvaluateAction(BuildingType.Extractor, (int)((4000 - MyKeldanyum) / 200f) + priority);
            }
            if (CountBuildings(BuildingType.Solar, false) <= currentInterface.planet.radius / 15f)
            {
                EvaluateAction(BuildingType.Solar, (int)((4000 - MyEnergy) / 200f) + priority);
            }
            if (currentInterface.planet.available_keldanyum < 2000 && CountBuildings(BuildingType.LKE, false) <= currentInterface.planet.radius / 10)
            {
                EvaluateAction(BuildingType.LKE, (int)((2000 - currentInterface.planet.available_keldanyum) / 100f) + priority);
            }
            if (currentInterface.planet.available_keldanyum < 2000 && CountBuildings(BuildingType.Reactor, false) <= currentInterface.planet.radius / 10 && CountBuildings(BuildingType.School,false)>0)
            {
                EvaluateAction(BuildingType.Reactor, (int)((2000 - currentInterface.planet.available_keldanyum) / 80f) + priority);
            }
            //Try recycling
            if (currentInterface.planet.available_keldanyum < 1000)
            {
                if (CountBuildings(BuildingType.Radar, false) > 0 && CountBuildings(BuildingType.Radar, true) > 1)
                {
                    BaseObjectBuilding radar = FirstBuilding(BuildingType.Radar, false);
                    if (radar!=null)
                    {
                        radar.Sell();
                    }
                }
                if (currentInterface.planet.available_keldanyum < 500)
                {
                    if (CountBuildings(BuildingType.Extractor, false) > currentInterface.planet.available_keldanyum/10)
                    {
                        BaseObjectBuilding extractor = FirstBuilding(BuildingType.Extractor, false);
                        if (extractor!=null)
                        {
                            extractor.Sell();
                        }
                    }
                }
            }
        }

        public void ManageHominids(School school,int builders, int soldiers, int pilots)
        {
            int count;
            bool send = false;
            //BUILDERS FIRST
            count = CountHominids(Specialization.Builder);
            if (count < builders + (int)currentInterface.planet.radius / 40)
            {
                send=true;
                school.selectedSpecialization = Specialization.Builder;
            }
            else
            {
                //SOLDIERS SECOND
                count = CountHominids(Specialization.Soldier);
                if (count < soldiers + (int)currentInterface.planet.radius / 40)
                {
                    send=true;
                    school.selectedSpecialization = Specialization.Soldier;
                }
                else
                {
                    //PILOTS LAST
                    count = CountHominids(Specialization.Pilot);
                    if (count < pilots + (int)currentInterface.planet.radius / 40)
                    {
                        send=true;
                        school.selectedSpecialization = Specialization.Pilot;
                    }
                }
            }
            if (send)
            {
                SendNearestHominid(school);              
            }
        }

        public void ManageRockets(int hunterPriority,int rocketPriority,int catapultPriority)
        {
            if (CountBuildings(BuildingType.House, false) == 0 || (currentInterface.planet.hominids.Count(h=>h.owner==myPlayerIndex) == 0 && currentInterface.planet.TotalPopulation()==currentInterface.planet.maxPopulation) || currentInterface.action==PlanetAction.Defend)
            {
                for (int a = 0; a < currentInterface.planet.buildings.Count; a++)
                {
                    if (currentInterface.planet.buildings[a].owner==myPlayerIndex && currentInterface.planet.buildings[a].flying == false)
                    {
                        currentInterface.planet.buildings[a].LeaveHominid();
                    }
                }
            }
            else
            {
                BaseObjectBuilding active = FirstBuilding(BuildingType.Hunter, false);
                //If on war, you can use hunter to invade planets
                if (active != null)
                {
                    if ((active as Hunter).pilot==null)
                    {
                        SendNearestHominid(active);
                    }
                    else
                    {
                        int ind = -1;
                        int own;
                        for (int d = 1; d < GameEngine.planets.Count; d++)
                        {
                            //If the planet is mine and it is under attack
                            //Or the planet is not mine but I'm in war with its race
                            own = PlayerManager.GetRaceOwner(GameEngine.planets[d].DominatingRace());
                            if ((GotPlanet(GameEngine.planets[d]) && GetInterface(GameEngine.planets[d]).action != PlanetAction.Live) || (own > -1 && PlayerManager.GetDualState(own, myPlayerIndex) == DualState.War))
                            {
                                ind = d;
                                break;
                            }
                        }
                        if (ind > -1)
                        {
                            active.LiftOff(GameEngine.planets[ind]);
                        }
                        else
                        {
                            active.LeaveHominid();
                        }
                    }
                }
                else
                {
                    EvaluateAction(BuildingType.Hunter, 1 - CountBuildings(BuildingType.Hunter, false) - (int)(stat_pacifism * 3) + hunterPriority);
                }
                //if you have a rocket
                active = FirstBuilding(BuildingType.Rocket, false);
                if (active != null)
                {
                    if ((active as Rocket).passengersCount < 3)
                    {
                        SendNearestHominid(active);
                    }
                    else
                    {
                        //NEARBY INTERESTING PLANETS
                        int priority = 0;
                        int maximum = 0;
                        int target = -1;
                        int inhabitants;
                        for (int b = 1; b < GameEngine.planets.Count; b++)
                        {
                            if (GameEngine.planets[b] != active.planet)
                            {
                                inhabitants = GameEngine.planets[b].DominatingRace();
                                priority = (int)(Vector3.Distance(currentInterface.planet.matrix.Translation, GameEngine.planets[b].matrix.Translation) / 2000f + GameEngine.planets[b].life / 10f + GameEngine.planets[b].radius / 10);
                                if (GameEngine.gameMode == GameMode.Countdown)
                                {
                                    priority += GameEngine.planets[b].distance;
                                }
                                //If the planet is not under my control
                                if (!GotPlanet(GameEngine.planets[b]) || inhabitants != MyRace)
                                {
                                    //"pacifism" is used
                                    //"egoism" is used
                                    //the more this player is pacifist, the higher the chance to not invade a planet increases
                                    if (inhabitants > -1)
                                    {
                                        if (stat_pacifism < 0.7f && stat_egoism < 0.4f)
                                        {
                                            continue;
                                        }
                                        int player = PlayerManager.GetRaceOwner(GameEngine.planets[b].DominatingRace());
                                        switch (PlayerManager.GetDualState(myPlayerIndex, player))
                                        {
                                            case DualState.Alliance:
                                                //Don't go there, there would be an atmosphere conflict
                                                if (!RaceManager.Tolerate(MyRace, GameEngine.planets[b].atmosphere))
                                                {
                                                    continue;
                                                }
                                                break;
                                            case DualState.Normal:
                                                //If too pacifist don't go there
                                                if (stat_pacifism > 0.5f)
                                                {
                                                    continue;
                                                }
                                                break;
                                            case DualState.War:
                                                //Raise priority!
                                                priority *= (int)(2 + stat_egoism * 4 - stat_pacifism * 4);
                                                break;
                                        }
                                    }
                                    //The planet is inhabitated, invading priority increase
                                    else
                                    {
                                        priority *= 2;
                                    }
                                }
                                //If the planet action is attack or defend, increase priority
                                else if (myPlanets.Find(p => p.planet == GameEngine.planets[b]).action != PlanetAction.Live)
                                {
                                    priority *= 4;
                                }
                                else
                                {
                                    priority /= 2;
                                }
                                //Prefer planets which atmosphere is tolerated
                                if (RaceManager.Tolerate(MyRace, GameEngine.planets[b].atmosphere))
                                {
                                    priority += 6;
                                }
                                else
                                {
                                    priority -= 3;
                                }
                                if (priority > maximum)
                                {
                                    maximum = priority;
                                    target = b;
                                }
                            }
                        }
                        if (target > -1)
                        {
                            active.LiftOff(GameEngine.planets[target]);
                        }
                        else
                        {
                            active.LeaveHominid();
                        }
                    }
                }
                else
                {
                    EvaluateAction(BuildingType.Rocket, 2 - CountBuildings(BuildingType.Rocket, false) + (GameEngine.gameMode == GameMode.Countdown ? 4 : 0) + rocketPriority);
                }
                active = FirstBuilding(BuildingType.Catapult, false);
                //If on war, you can use hunter to invade planets
                if (active != null)
                {
                    int ind = -1;
                    int own;
                    for (int d = 1; d < GameEngine.planets.Count; d++)
                    {
                        //If the planet is mine and it is under attack
                        //Or the planet is not mine but I'm in war with its race
                        own = PlayerManager.GetRaceOwner(GameEngine.planets[d].DominatingRace());
                        if (PlayerManager.GetDualState(own, myPlayerIndex) == DualState.War)
                        {
                            ind = d;
                            break;
                        }
                    }
                    if (ind > -1)
                    {
                        active.LiftOff(GameEngine.planets[ind]);
                        active.DoSecondHUDAction();
                    }
                }
                else
                {
                    EvaluateAction(BuildingType.Catapult, 1 - CountBuildings(BuildingType.Catapult, false) - (int)(stat_pacifism * 5) + catapultPriority);
                }
            }

        }

        /// <summary>
        /// Send a hominid toward a building
        /// </summary>
        /// <param name="building"></param>
        private void SendNearestHominid(BaseObjectBuilding building)
        {
            int hominid = NearestHominid(building.matrix.Translation, Specialization.Normal, true);
            if (hominid > -1)
            {
                currentInterface.planet.hominids[hominid].SetGoal(building.matrix.Translation, currentInterface.planet.matrix.Translation);
                currentInterface.planet.hominids[hominid].target = building;
            }
        }

        /// <summary>
        /// Check and eventually switch a new action type with a priority
        /// </summary>
        private void EvaluateAction(BuildingType type, int priority)
        {
            //These lines are almost the most importan in all this class
            //the AI will check if it can build something, if not, needed researches will be done
            MenuVoice voice = BuildingManager.GetMenuVoice(type);
            if (voice.researchLevels != null)
            {
                for (int a = 0; a < voice.researchLevels.Length; a++)
                {
                    if (voice.researchLevels[a] > BuildingManager.ResearchLevels(myPlayerIndex)[a] - 1)
                    {
                        if (researchTimer<=0 && MyKeldanyum >= BuildingManager.researches[a].cost.X * BuildingManager.ResearchLevels(myPlayerIndex)[a] &&
                            MyEnergy >= BuildingManager.researches[a].cost.Y * BuildingManager.ResearchLevels(myPlayerIndex)[a] && CountBuildings(BuildingType.Extractor, true) > 2 && CountBuildings(BuildingType.Solar, true) > 2)
                        {
                            PlayerManager.ChangeKeldanyum(myPlayerIndex, -BuildingManager.researches[a].cost.X * BuildingManager.ResearchLevels(myPlayerIndex)[a]);
                            PlayerManager.ChangeEnergy(myPlayerIndex, -BuildingManager.researches[a].cost.Y * BuildingManager.ResearchLevels(myPlayerIndex)[a]);

                            //Try researching that!
                            BuildingManager.ResearchLevels(myPlayerIndex)[a]++;

                            //Set the timer
                            researchTimer = (1 + BuildingManager.ResearchLevels(myPlayerIndex)[a]) * (3 - stat_skill*stat_skill);
                            a--;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
            //"skill" is used
            //the evaluation would be perfect, but the "skill" stat insert an error amount
            if (priority > decisionPriority && Util.random.NextDouble() <= stat_skill)
            {
                decisionPriority = priority;
                decision = (int)type;
            }
        }

        /// <summary>
        /// switch on or off alle the turbinas on a planet
        /// </summary>
        private void SwitchTurbina(bool active)
        {
            for (int a = 0; a < currentInterface.planet.buildings.Count; a++)
            {
                if (currentInterface.planet.buildings[a] is Turbina && currentInterface.planet.buildings[a].owner == myPlayerIndex)
                {
                    currentInterface.planet.buildings[a].Switch(active);
                }
            }
        }

        /// <summary>
        /// switch on or off alle the turbinas on a planet
        /// </summary>
        private void SwitchShield(bool active)
        {
            for (int a = 0; a < currentInterface.planet.buildings.Count; a++)
            {
                if (currentInterface.planet.buildings[a] is Repulser && currentInterface.planet.buildings[a].owner==myPlayerIndex)
                {
                    currentInterface.planet.buildings[a].Switch(active);
                    break;
                }
            }
        }

        /// <summary>
        /// Count buildings this AI owns with specific parameters
        /// </summary>
        private int CountBuildings(BuildingType type,bool all)
        {
            if (all)
            {
                int total = 0;
                for (int a = 0; a < myPlanets.Count; a++)
                {
                    total += myPlanets[a].planet.buildings.Count(b => b.type == type && b.owner==myPlayerIndex);
                }
                return total;
            }
            return currentInterface.planet.buildings.Count(b => b.type == type && b.owner == myPlayerIndex);
        }

        /// <summary>
        /// Count hominids this AI owns with specific specialization
        /// </summary>
        private int CountHominids(Specialization specialization)
        {
            return currentInterface.planet.hominids.Count(h => h.specialization == specialization && h.Race == MyRace);
        }

        /// <summary>
        /// Get the first building satisfying the requisite
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private BaseObjectBuilding FirstBuilding(BuildingType type, bool all)
        {
            Planet planet;
                for (int a = 0; a < myPlanets.Count; a++)
                {
                    planet=(all?myPlanets[a].planet:currentInterface.planet);
                    for (int b = 0; b < planet.buildings.Count; b++)
                    {
                        if (planet.buildings[b] is PreBuilding==false &&  planet.buildings[b].type==type && planet.buildings[b].owner==myPlayerIndex && planet.buildings[b].flying==false)
                        {
                            if (planet.buildings[b] is School && (planet.buildings[b] as School).student!=null)
                            {
                                continue;
                            }
                            return planet.buildings[b];
                        }
                    }
                    if (all)
                    {
                        break;
                    }
                }
                return null;
        }

        /// <summary>
        /// Get the nearest hominid on a planet near to a position
        /// </summary>
        private int NearestHominid(Vector3 position,Specialization specialization,bool specializationCount)
        {
            float min = float.MaxValue;
            float dist;
            int ind = -1;
            for (int a = 0; a < currentInterface.planet.hominids.Count; a++)
            {
                //skip hominids with no specialization matching
                if (specializationCount)
                {
                    if (currentInterface.planet.hominids[a].specialization != specialization)
                    {
                        continue;
                    }
                }
                if (currentInterface.planet.hominids[a].owner == myPlayerIndex)
                {
                    dist = Vector3.Distance(currentInterface.planet.hominids[a].matrix.Translation, position);
                    if (dist < min)
                    {
                        min = dist;
                        ind = a;
                    }
                }
            }
            return ind;
        }

        /// <summary>
        /// Returns if the player can afford a specified building
        /// </summary>
        private bool CanAfford(BuildingType type)
        {
            Point cost = BuildingManager.GetMenuVoice(type).cost;
            return (MyKeldanyum >= cost.X && MyEnergy >= cost.Y);
        }

        /// <summary>
        /// Add a planet to the "interested" ones
        /// </summary>
        /// <param name="planet"></param>
        public void RegisterPlanet(Planet planet)
        {
            //The new planet is insert in position 0 so all the decision will be first taken for this planet
            //The newest planet has always the priority on older ones
            if (myPlanets.Exists(i => i.planet == planet) == false)
            {
                myPlanets.Insert(0, new PlanetInterface { planet = planet, action = PlanetAction.Live });
            }
        }

        /// <summary>
        /// Build the object of type "type" on the planet with index "planet"
        /// </summary>
        private void Build(BuildingType type)
        {
            if (CanAfford(type) == false || currentInterface.planet.CanBuild(myPlayerIndex) == false)
            {
                return;
            }

            //Search a valid position
            Vector3 testPosition = Util.RandomPointOnSphere(currentInterface.planet.radius);
            int attempts = 0;
            while (attempts++ < 10 && !BuildingManager.FreeBuildingPosition(currentInterface.planet, currentInterface.planet.matrix.Translation + testPosition))
            {
                testPosition = Util.RandomPointOnSphere(currentInterface.planet.radius);
            }
            if (attempts >= 10)
            {
                return;
            }

            //Get condition
            //MenuVoice tempVoice = BuildingManager.menuVoice;
            BaseObjectBuilding tempBuilding = BuildingManager.previewBuilding;
            //Add the PreBuilding
            Matrix m = BaseObjectBuilding.GetFullPlacedMatrix(testPosition, currentInterface.planet);
            BuildingManager.previewBuilding = BuildingManager.InitializePreviewModel(type, currentInterface.planet, m, myPlayerIndex);
            m.Translation += m.Backward * BuildingManager.previewBuilding.SurfaceOffset;
            BuildingManager.StartBuilding(type, currentInterface.planet, m, myPlayerIndex);
            //Restore condition
            BuildingManager.previewBuilding = tempBuilding;
        }

        public bool GotPlanet(Planet planet)
        {
            return (myPlanets.Find(p => p.planet == planet) != null);
        }

        public void PerformTrade(int A, int B)
        {
            //Can use maximum half of the player resources
            Point resA = new Point((int)PlayerManager.GetKeldanyum(A)/2, (int)PlayerManager.GetEnergy(A)/2);
            Point resB = new Point((int)PlayerManager.GetKeldanyum(B)/2, (int)PlayerManager.GetEnergy(B)/2);

            Point needA=PlayerManager.NeededResources(A);
            Point needB=PlayerManager.NeededResources(B);

            Point toA;
            Point toB;

            if (resA.X > resB.X)
            {
                //Player A has more keldanyum resources
                toB.X = Math.Min(needB.X, resA.X);
                toA.Y = Math.Min(needA.Y, resB.Y);
                toA.X = -toB.X;
                toB.Y = -toA.Y;
            }
            else
            {
                //Player B has more keldanyum resources
                toA.X = Math.Min(needA.X, resB.X);
                toB.Y = Math.Min(needB.Y, resA.Y);
                toB.X = -toA.X;
                toA.Y = -toB.Y;
            }

            PlayerManager.ChangeKeldanyum(A, toA.X);
            PlayerManager.ChangeEnergy(A, toA.Y);

            PlayerManager.ChangeKeldanyum(B, toB.X);
            PlayerManager.ChangeEnergy(B, toB.Y);

            if (A==0)
            {
                TextBoard.AddMessage("    You gave: " + (toB.X > toB.Y ? toB.X + " of keldanyum" : toB.Y + " of energy") + " for " + (toA.X > toA.Y ? toA.X + " of keldanyum" : toA.Y + " of energy"));
                TextBoard.AddMessage("Trade with " + RaceManager.GetRace(PlayerManager.GetRace(B)) + " successfull!");
            }
            else if (B == 0)
            {
                TextBoard.AddMessage("    You gave: " + (toA.X > toA.Y ? toA.X + " of keldanyum" : toA.Y + " of energy") + " for " + (toB.X > toB.Y ? toB.X + " of keldanyum" : toB.Y + " of energy"));
                TextBoard.AddMessage("Trade with " + RaceManager.GetRace(PlayerManager.GetRace(A)) + " successfull!");
            }
        }
    }
}
