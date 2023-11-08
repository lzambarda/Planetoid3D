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
    public enum PlayerState
    {
        Close,
        Open,
        Human,
        Dumb,
        Normal,
        Challenging,
        Hardcore
    }

    public enum DualState
    {
        Normal,
        War,
        Alliance
    }

    [Serializable]
    public static class PlayerManager
    {
        public static void Initialize()
        {
            players = new Player[9];
            for (int a = 0; a < 9; a++)
            {
                players[a] = new Player();
            }
            //The first is and will always be the human player
            players[0].state = PlayerState.Human;
            //Tell the game that trogloid native race will play
            players[8].state = PlayerState.Normal;
            players[8].race = 19;
        }

        //Variables
        public static Player[] players;
        public static int first;
        public static int second;
        public static int third;

        public static SerializablePlayerManager InSerialization()
        {
            for (int a = 0; a < 8; a++)
            {
                if (players[a].state > PlayerState.Human)
                {
                    players[a].cpuController.InSerialization();
                }
            }
            return new SerializablePlayerManager() { players = PlayerManager.players };
        }

        public static void OutSerialization(SerializablePlayerManager spm)
        {
            players = spm.players;
            for (int a = 0; a < 8; a++)
            {
                if (players[a].state > PlayerState.Human)
                {
                    players[a].cpuController.OutSerialization();
                }
            }
        }


        /// <summary>
        /// Update the state of all the players
        /// </summary>
        public static void UpdatePlayers(float elapsed)
        {
            for (int p = 0; p < 8; p++)
            {
                if (players[p].state>PlayerState.Open)
                {
                    if (DeathRisk(p) > 95)
                    {
                        if (IsDefeated(p))
                        {
                            RemovePlayer(p);
                            continue;
                        }
                    }
                    if (players[p].state > PlayerState.Human)
                    {
                        players[p].cpuController.TakeDecision(elapsed);
                    }
                }
            }
        }

        /// <summary>
        /// Get the owner of a given race
        /// </summary>
        public static int GetRaceOwner(int race)
        {
            for (int a = 0; a < players.Length; a++)
            {
                if (GetRace(a) == race)
                {
                    return a;
                }
            }
            return -1;
        }

        /// <summary>
        /// Get the Keldanyum of a player
        /// </summary>
        public static float GetKeldanyum(int index)
        {
            return players[index].keldanyum;

        }

        /// <summary>
        /// Get the Energy of a player
        /// </summary>
        public static float GetEnergy(int index)
        {
            return players[index].energy;
        }

        /// <summary>
        /// Get the Race of a player
        /// </summary>
        public static int GetRace(int index)
        {
            return players[index].race;
        }

        /// <summary>
        /// Get the slot state of a player
        /// </summary>
        public static PlayerState GetState(int index)
        {
            return players[index].state;
        }

        /// <summary>
        /// Set the Keldanyum amount of a player
        /// </summary>
        public static void SetKeldanyum(int index, float keldanyum)
        {
            players[index].keldanyum = keldanyum;
        }

        /// <summary>
        /// Set the Energy amount of a player
        /// </summary>
        public static void SetEnergy(int index, float energy)
        {
            players[index].energy = energy;
        }

        /// <summary>
        /// Set the Race of a player
        /// </summary>
        public static void SetRace(int index, int race)
        {
            players[index].race = race;
        }

        /// <summary>
        /// Set the slot state of a player
        /// </summary>
        public static void SetState(int index, PlayerState state)
        {
            players[index].state = state;
        }

        /// <summary>
        /// Set the DualState between two players, no matter the order of the players indices
        /// </summary>
        public static void SetDualState(int player1, int player2,DualState state)
        {
            players[player1].allianceState[player2] = state;
            players[player2].allianceState[player1] = state;
        }

        /// <summary>
        /// Get the Friendship between two players
        /// </summary>
        public static float GetFriendship(int player1, int player2)
        {
            if (player1 == -1 || player2 == -1)
            {
                return 0;
            }
            if (player1 == player2)
            {
                return 1;
            }
            if (player1 == 8 || player2 == 8)
            {
                return 0;
            }
            return players[player1].friendState[player2];
        }

        /// <summary>
        /// Set both Friendship and Trust between two players
        /// </summary>
        public static void SetAll(int player1, int player2, int amount)
        {
            players[player1].friendState[player2] = amount;
            players[player1].trustState[player2] = amount;
        }

        /// <summary>
        /// Modify the Friendship between two players
        /// </summary>
        public static void ChangeFriendship(int player1, int player2,float change)
        {
            players[player1].friendState[player2] += change;
            players[player1].friendState[player2] = MathHelper.Clamp(players[player1].friendState[player2], -1, 1);
        }

        /// <summary>
        /// Get the Trust between two players
        /// </summary>
        public static float GetTrust(int player1, int player2)
        {
            if (player1 == player2)
            {
                return 1;
            }
            if (player1 == 8 || player2 == 8)
            {
                return 0;
            }
            return players[player1].trustState[player2];
        }

        /// <summary>
        /// Modify the Trust between two players
        /// </summary>
        public static void ChangeTrust(int player1, int player2, float change)
        {
            players[player1].trustState[player2] += change;
            players[player1].trustState[player2] = MathHelper.Clamp(players[player1].trustState[player2], -1, 1);
        }

        /// <summary>
        /// Modify the Keldanyum of a players
        /// </summary>
        public static void ChangeKeldanyum(int index, float keldanyum)
        {
            players[index].keldanyum = MathHelper.Clamp(players[index].keldanyum + keldanyum, 0, 99999);
        }

        /// <summary>
        /// Modify the Energy of a players
        /// </summary>
        public static void ChangeEnergy(int index, float energy)
        {

            players[index].energy = MathHelper.Clamp(players[index].energy + energy, 0, 99999);
        }

        /// <summary>
        /// Get the DualState between two players, no matter the order of the players indices
        /// </summary>
        public static DualState GetDualState(int player1, int player2)
        {
            if (player1 == -1 || player2 == -1)
            {
                return DualState.Normal;
            }
            return players[player1].allianceState[player2];
        }

        /// <summary>
        /// Place players in the game
        /// </summary>
        public static void PlacePlayers()
        {
            for (int a = 0; a < 8; a++)
            {
                if (GetState(a) > PlayerState.Human)
                {
                    players[a].cpuController = new AIPlayer((float)Util.random.NextDouble(), (float)Util.random.NextDouble(), (float)Util.random.NextDouble(), a);
                }
                else if (GetState(a) == PlayerState.Human)
                {
                    //Add players
                    Planet planet= Player.PickFreePlanet(a);
                    PlayerManager.SetKeldanyum(a, 2000);
                    PlayerManager.SetEnergy(a, 1500);
                    if (a == 0)
                    {
                        GameEngine.gameCamera.target = planet;
                    }
                }
            }
        }

        /// <summary>
        /// Create the diplomacies, this mean setting up friendship and trust among players
        /// </summary>
        public static void CreateDiplomacies()
        {
            for (int a = 0; a < players.Length; a++)
            {
                CreateDiplomacy(a);
            }
        }

        /// <summary>
        /// Create the diplomacy for the given player
        /// </summary>
        public static void CreateDiplomacy(int player)
        {
            if (GetState(player) > PlayerState.Open)
            {
                players[player].scoreCreatedHominids = 3;
                players[player].friendState = new float[players.Length];
                players[player].trustState = new float[players.Length];
                players[player].allianceState = new DualState[players.Length];
                players[player].messageFlags = new float[players.Length * 8];

                for (int b = 0; b < players.Length; b++)
                {
                    //this refers to trogloid race, which is a race hating everyone, will not build anything
                    if (player == players.Length - 1 || b == 8)
                    {
                        players[player].friendState[b] = -1f;
                        players[player].trustState[b] = -1f;
                        players[player].allianceState[b] = DualState.War;
                    }
                    else
                    {
                        if (GetState(player) < PlayerState.Dumb)
                        {
                            players[player].friendState[b] = 1f;
                            players[player].trustState[b] = 1f;
                        }
                        else
                        {
                            players[player].friendState[b] = 0.4f;
                            players[player].trustState[b] = 0.1f;
                        }
                        players[player].allianceState[b] = DualState.Normal;
                    }
                }
                players[player].active_deals = new List<Deal>();
            }
        }
        
        /// <summary>
        /// Get an unique race, that is a race not used
        /// </summary>
        public static int GetUniqueRace(int player)
        {
            int race;
            do
            {
                race = Util.random.Next(RaceManager.TotalRaces-1);
            }
            while (IsRaceTaken(player, race) || !RaceManager.IsUnlocked(race));
            return race;
        }

        public static int GetNextRace(int player)
        {
            do
            {
                if (++players[player].race >= RaceManager.TotalRaces)
                {
                    players[player].race = 0;
                }
            }
            while (IsRaceTaken(player, players[player].race));
            return players[player].race;
        }

        public static int GetPreviousRace(int player)
        {
            do
            {
                if (--players[player].race < 0)
                {
                    players[player].race = RaceManager.TotalRaces - 1;
                }
            }
            while (IsRaceTaken(player, players[player].race));
            return players[player].race;
        }

        /// <summary>
        /// Returns if a race is currently being used by someone
        /// </summary>
        public static bool IsRaceTaken(int player,int race)
        {
            for (int a = 0; a < players.Length; a++)
            {
                //if the player is not the claimer and the slot is not closed or open only
                if (a != player && players[a].state > PlayerState.Open)
                {
                    if (GetRace(a) == race)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the Death Risk of a player
        /// </summary>
        public static int DeathRisk(int player)
        {
            int risk = 0;
            for (int a = 0; a < GameEngine.planets.Count; a++)
            {
                if (GameEngine.planets[a].DominatingRace() == (int)GetRace(player))
                {
                    //Count hominids
                    risk += GameEngine.planets[a].hominids.Count;
                    //Count planet life
                    risk -= (int)(3 - GameEngine.planets[a].life);
                }
            }
            return (100-risk*2);
        }

        /// <summary>
        /// Returns if the player has been defeated
        /// </summary>
        public static bool IsDefeated(int player)
        {
            for (int a = 0; a < GameEngine.planets.Count; a++)
            {
                //The player still owns a planet
                for (int h = 0; h < GameEngine.planets[a].hominids.Count; h++)
                {
                    if (GameEngine.planets[a].hominids[h].owner == player)
                    {
                        return false;
                    }
                }
                BaseObjectBuilding b;
                for (int bb = 0; bb < GameEngine.planets[a].buildings.Count; bb++)
                {
                    b = GameEngine.planets[a].buildings[bb];
                    if (b.owner == player)
                    {
                        if (b is House)
                        {
                            return false;
                        }
                        if (b is School && ((School)b).student!=null)
                        {
                            return false;
                        }
                        if (b is Rocket && ((Rocket)b).passengersCount > 0)
                        {
                            return false;
                        }
                        if (b is Hunter && ((Hunter)b).pilot!=null)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Remove a player from the game
        /// </summary>
        public static void RemovePlayer(int player)
        {
            TextBoard.AddMessage(RaceManager.GetRace(PlayerManager.GetRace(player)).ToUpper() + " HAS BEEN DEFEATED!");
            //Set it to close
            players[player].Dispose();

            if (players[0].scoreLostHominids == 0)
            {
                QuestManager.QuestCall(18);
            }

            //Also delete all diplomacy actions this player ever sent to other players
            for (int p= 0; p < players.Length; p++)
            {
                if (players[p]!=null && PlayerManager.GetState(player)>PlayerState.Open && p != player)
                {
                    for (int d = 0; d < players[p].active_deals.Count; d++)
                    {
                        if (players[p].active_deals[d].claimer == player)
                        {
                            players[p].active_deals.RemoveAt(d);
                            d--;
                        }
                    }
                }
            }

            //If the player was the human
            int humans = 0;
            bool allAllied = true;
            for (int a = 0; a < 8; a++)
            {
                if (players[a].state > PlayerState.Open)
                {
                    if (players[a].state == PlayerState.Human)
                    {
                        humans++;
                    }
                    if (GetDualState(a, player) != DualState.Alliance)
                    {
                        allAllied = false;
                    }
                }
            }

            if (humans == 0 || allAllied)
            {
                for (int a = 0; a < 8; a++)
                {
                    if (players[a].state != PlayerState.Close)
                    {
                        players[a].CalculateScore();
                    }
                }
                PlanetoidGame.game_screen_next = GameScreen.Result;
                PlanetoidGame.elapsed = 0;
                AudioManager.battleIsHappening = 0;
                CalculateResults();
            }
            else if (GameEngine.gameMode!=GameMode.Tutorial)
            {
                //check if only the player and his allies are together
                allAllied = true;
                for (int a = 1; a < 8; a++)
                {
                    if (players[a].state > PlayerState.Open)
                    {
                        if (GetDualState(a, 0) != DualState.Alliance)
                        {
                            allAllied = false;
                        }
                    }
                }
                if (allAllied)
                {
                    MessageBox.ShowDialog("Finished", "It seems that there are no more enemies!\nPress OK to end game or cancel to continue playing!", 0);
                }
            }
        }

        /// <summary>
        /// Get a value indicating the general relationship level between two players
        /// </summary>
        public static string GetGeneralRelationLevel(int p1, int p2)
        {
            float t = GetTrust(p1, p2);
            float f = GetFriendship(p1, p2);
            switch (GetDualState(p1, p2))
            {
                case DualState.Alliance:
                    t += 0.4f;
                    f += 0.2f;
                    break;
                case DualState.War:
                    t -= 0.4f;
                    f -= 0.2f;
                    break;
            }
            if (t >= 0.5)
            {
                if (f >= 1)
                {
                    return "Like Brothers!";
                }
                else if (f >= 0.75f)
                {
                    return "Friends!";
                }
                else if (f >= 0.5f)
                {
                    return "Very Good!";
                }
                else if (f >= 0.25f)
                {
                    return "Could be better";
                }
                else if (f >= 0f)
                {
                    return "Possible Friend";
                }
                else if (f >= -0.25f)
                {
                    return "What's the problem?";
                }
                else if (f >= -0.5f)
                {
                    return "There's Hope!";
                }
                else
                {
                    return "Weird";
                }
            }
            else if (t>=0)
            {
                if (f >= 1)
                {
                    return "Happy :)";
                }
                else if (f >= 0.5f)
                {
                    return "Could be better!";
                }
                else if (f > 0)
                {
                    return "Normal";
                }
                else if (f >= -0.25f)
                {
                    return "Don't trust them";
                }
                else if (f >= -0.5f)
                {
                    return "Ouch!";
                }
                else
                {
                    return "Are you mad?";
                }
            }
            else
            {
                if (f >= 1)
                {
                    return "Traytor!";
                }
                else if (f >= 0.5f)
                {
                    return "Bad Situation";
                }
                else if (f >= 0)
                {
                    return "Very Bad";
                }
                else if (f >= -0.5f)
                {
                    return ":(";
                }
            }
            return "Nemesis!";
        }

        public static Point NeededResources(int player)
        {
            Point res = Point.Zero;
            int c = GameEngine.planets.Count(p => p.DominatingRace() == PlayerManager.GetRace(player));
            res.X = (400 * c);
            res.Y = (300 * c);
            return res;
        }

        /// <summary>
        /// Administrates diplomacy events following an attack
        /// </summary>
        public static void PlayerAttack(int attacker,int attacked,float friendchange,float trustchange)
        {  
            //Decrease stats
            if (GetState(attacked) > PlayerState.Human)
            {
                ChangeFriendship(attacked, attacker, friendchange);
                ChangeTrust(attacked, attacker, trustchange);
            }
            //Modify relationships
            for (int a = 0; a < players.Length; a++)
            {
                if (players[a] != null && players[a].state>PlayerState.Human && a != attacked && a != attacker)
                {
                    switch (GetDualState(a, attacked))
                    {
                        //If it is allied with the attacked entity, it's relationship gets worse
                        case DualState.Alliance:
                            ChangeFriendship(a, attacker, friendchange);
                            ChangeTrust(a, attacker, trustchange);
                            break;
                        //Otherwise it gets better
                        case DualState.War:
                            ChangeFriendship(a, attacker, -friendchange);
                            ChangeTrust(a, attacker, -trustchange);
                            break;
                    }
                }
            }
        }

        public static void CalculateResults()
        {
            if (players[0].scoreFinal >= 10000)
            {
                QuestManager.QuestCall(14);
            }
            first = -1;
            second = -1;
            third = -1;
            for (int a = 0; a < 8; a++)
            {
                if (players[a].active_deals != null)
                {
                    if (first == -1 || players[a].scoreFinal >= players[first].scoreFinal)
                    {
                        third = second;
                        second = first;
                        first = a;
                    }
                    else if (second == -1 || players[a].scoreFinal >= players[second].scoreFinal)
                    {
                        third = second;
                        second = a;
                    }
                    else if (third == -1 || players[a].scoreFinal >= players[third].scoreFinal)
                    {
                        third = a;
                    }
                }
            }
            if (players.Count(p => p.active_deals != null) > 2)
            {
                if (first == 0 && second > -1)
                {
                    QuestManager.QuestCall(3);
                }
            }
        }

        public static void DrawResultScreen(SpriteFont font)
        {
            PlanetoidGame.spriteBatch.Begin();
            Vector2 position = new Vector2(GameEngine.Game.GraphicsDevice.Viewport.Width / 2, 50);
            Util.DrawCenteredText( font, "Game Finished!\nBattle Results:", position, Color.White);

            PlanetoidGame.spriteBatch.DrawString(font,
                "Created Buildings: " +
                "\n\nCreated Hominids: " +
                "\n\nKilled Hominids: " +
                "\n\nDestroyed Buildings: " +
                "\n\nDestroyed Asteroids: " +
                "\n\nHominids Lost: " +
                "\n\nSurvived Time: " +
                "\n\nFinal Score: ", new Vector2(20, 192), Color.White);

            position += new Vector2(150 - (50 * players.Count(p => p.active_deals != null)), 100);
            for (int a = 0; a < 8; a++)
            {
                //A player which has played at least 1 second will have this variable greater than 0
                if (players[a].scoreSurvivedTime>0)
                {
                    //spriteBatch.DrawString(textFont, , position, RaceManager.GetColor(PlayerManager.GetRace(a)));
                    //position.Y += 50;
                    PlanetoidGame.spriteBatch.DrawString(font,
                        RaceManager.GetRace(players[a].race) + "\n\n" +
                        players[a].scoreCreatedBuildings +
                        "\n\n" + players[a].scoreCreatedHominids +
                        "\n\n" + players[a].scoreKilledHominids +
                        "\n\n" + players[a].scoreDestroyedBuildings +
                        "\n\n" + players[a].scoreDestroyedAsteroids +
                        "\n\n" + players[a].scoreLostHominids +
                        "\n\n" + players[a].scoreSurvivedTime +
                        "\n\n" + players[a].scoreFinal
                        , position + new Vector2(100 * a, 0), RaceManager.GetColor(players[a].race));
                }
            }

            if (players.Count(p => p.active_deals != null) > 2)
            {
                position.X -= 25;
                PlanetoidGame.spriteBatch.Draw(RenderManager.medalGold, position + new Vector2(100 * first, 380), Color.White);
                if (second > -1)
                {
                    PlanetoidGame.spriteBatch.Draw(RenderManager.medalSilver, position + new Vector2(100 * second, 380), Color.White);
                }
                if (third > -1)
                {
                    PlanetoidGame.spriteBatch.Draw(RenderManager.medalBronze, position + new Vector2(100 * third, 380), Color.White);
                }
            }
            MenuManager.all_back.Draw(font);
            PlanetoidGame.spriteBatch.End();
        }
    }
}
