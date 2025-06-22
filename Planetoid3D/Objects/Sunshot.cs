using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Planetoid3D
{
    [Serializable]
    public class Sunshot : BaseObject
    {
        public Sunshot() { }
        public Sunshot(Vector3 Position)
        {
            //Get a good shoot direction
            matrix = Matrix.Identity;
            matrix.Translation = Position;
            speed = Util.RandomPointOnSphere(1f+(float)(Util.random.NextDouble()*4f));
            speed += Position / 150f;
            life = 1;
        }

        public bool Update(float elapsed)
        {
            //Don't mind main menu, without this condition the game would cause NullReferenceException
            if (PlanetoidGame.game_screen==GameScreen.Menu)
            {
                return false;
            }
            //If in collision with a planet's surface
            for (int a = 0; a < GameEngine.planets.Count; a++)
            {
                if (Vector3.Distance(matrix.Translation, GameEngine.planets[a].matrix.Translation) < GameEngine.planets[a].radius - 5)//You should use GameEngine.planets[0].radius-5 but this will cause NullReferenceException in the main menu
                {
                    //Die in the planet
                    if (a > 0)
                    {
                        GameEngine.planets[a].life -= 1;
                        AudioManager.Play3D(this, "asteroid_death");
                    }
                    return true;
                }
            }

            GameEngine.fireParticles.AddParticle(matrix.Translation + Util.RandomPointOnSphere(10), Vector3.Zero);

            //Apply gravity to return in the sun
            speed += -matrix.Translation / 10000;
            matrix.Translation += speed * elapsed * 36;

            return false;
        }
    }
}
