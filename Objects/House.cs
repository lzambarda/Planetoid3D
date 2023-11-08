using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Planetoid3D
{
    [Serializable]
    public class House : BaseObjectBuilding
    {
        public House() { }
        public House(Planet Planet, Matrix initial, int Owner)
        {
            planet = Planet;
            matrix = initial;
            type = BuildingType.House;
            owner = Owner;
            timer = 40;
        }

        float timer;

        public override bool Update(float elapsed)
        {
            //If the oxygen timer is gone
            timer -= elapsed;
            if (timer <= 0)
            {
                if (owner == 0)
                {
                }
                timer = 40;
                if (planet.TotalPopulation() < planet.maxPopulation)
                {
                    //Add one
                    planet.hominids.Add(new Hominid(planet, owner, GetMatrix(20, matrix.Right)));
                    PlayerManager.players[owner].CreatedHominid();
                }
            }
            return base.Update(elapsed);
        }
    }
}
