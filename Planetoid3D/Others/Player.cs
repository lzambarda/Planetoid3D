using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Planetoid3D
{
    public class Player
    {
        public int race;
        public float keldanyum;
        public float energy;
        public PlayerState state;
        public float[] friendState;
        public float[] trustState;
        public DualState[] allianceState;
        public List<Deal> active_deals;
        public AIPlayer cpuController;
        public int[] researchLevels;
        //We use a single dimension because multidimensional arrays are not supported in XMLserialize
        public float[] messageFlags;

        public int radarAmount;
        public int scoreCreatedBuildings;
        public int scoreCreatedHominids;
        public int scoreKilledHominids;
        public int scoreDestroyedBuildings;
        public int scoreLostHominids;
        public int scoreDestroyedAsteroids;
        public long scoreSurvivedTime;
        public long scoreFinal;

        public void BuildingCreated()
        {
            scoreCreatedBuildings++;
        }

        public void CreatedHominid()
        {
            scoreCreatedHominids++;
        }

        public void KilledHominid()
        {
            scoreKilledHominids++;
        }

        public void DestroyedBuilding()
        {
            scoreDestroyedBuildings++;
        }

        public void LostHominid()
        {
            scoreLostHominids++;
        }

        public void DestroyedAsteroid()
        {
            scoreDestroyedAsteroids++;
        }

        public void CalculateScore()
        {
            scoreFinal = (scoreCreatedBuildings * 10);
            scoreFinal += (scoreCreatedHominids * 2);
            scoreFinal += (scoreKilledHominids * 5);
            scoreFinal += (scoreDestroyedBuildings * 15);
            scoreFinal += (scoreDestroyedAsteroids * 8);
            scoreFinal -= (scoreLostHominids * 5);
            scoreFinal += (scoreSurvivedTime-1000)*2;
        }

        public static Planet PickFreePlanet(int player)
        {
            Planet planet  = GameEngine.planets[1 + Util.random.Next(GameEngine.planets.Count - 1)];
            while (planet.hominids.Count > 0 || planet.planet is Sun == false)
            {
                planet = GameEngine.planets[1 + Util.random.Next(GameEngine.planets.Count - 1)];
            }
            //Populate the planet
            planet.Populate(player, 3);
            return planet;
        }

        /// <summary>
        /// Free almost all memory used by this player and set its state to Close
        /// </summary>
        public void Dispose()
        {
            state = PlayerState.Close;
            cpuController = null;
            active_deals.Clear();
            CalculateScore();
        }   
    }
}
