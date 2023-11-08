using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using System.Xml.Serialization;

namespace Planetoid3D
{
    [Serializable]
    public class Sun : Planet
    {
        public Sun()
        {
            matrix = Matrix.Identity;
            spinSpeed = 0.05f;
            radius = 150;
            name = "Sun";
            axis = Vector3.Up;
            atmosphere_level = 100;
            atmosphere = Atmosphere.Oxygen;
            color = new Color(255, 200, 0);
            realColor = Color.White;
            texture_index = 0;
            timer = 0;
            shotTimer = 10;
            planet = null;
            sunShots = new List<Sunshot>();
        }

        public List<Sunshot> sunShots;
        public float timer;
        private float shotTimer;
        public Color realColor;

        new public void Update(float elapsed)
        {
            //Rotate
            matrix *= Matrix.CreateRotationY(spinSpeed * elapsed);

            //Update color change
            if (timer > 0 || GameEngine.gameMode == GameMode.Countdown)
            {
                color = Color.Lerp(color, Color.Lime, 0.02f);
                realColor = Color.Lerp(realColor, Color.Lime, 0.02f);
                timer -= 0.2f;
                if (radius < 300)
                {
                    radius += 0.05f;
                }
            }
            else
            {
                shotTimer -= elapsed;
                if (shotTimer <= 0)
                {
                    shotTimer = 20 + Util.random.Next(10);
                    sunShots.Add(new Sunshot(Util.RandomPointOnSphere(radius)));
                }
                for (int a = 0; a < sunShots.Count; a++)
                {
                    if (sunShots[a].Update(elapsed))
                    {
                        sunShots.RemoveAt(a);
                        a--;
                    }
                }
                
                color = Color.Lerp(color, new Color(255, 200, 0), 0.02f);
                realColor = Color.Lerp(realColor, Color.White, 0.02f);
                if (radius > 150)
                {
                    radius -= 0.05f;
                    Vector3 position = Util.RandomPointOnSphere(radius);
                    GameEngine.planetoidParticles.AddParticle(position, position / 100f);
                }
            }

            //Add particles
            GameEngine.fireParticles.SetColor(color);
            for (int a = 0; a < 10 + (20 * PlanetoidGame.details); a++)
            {
                GameEngine.fireParticles.AddParticle(Util.RandomPointOnSphere(radius + 5), Vector3.Zero);
            }
            if (deathTimer > 0)
            {
                //Update Sun shots
                for (int a = 0; a < sunShots.Count; a++)
                {
                    if (sunShots[a].Update(elapsed))
                    {
                        sunShots.RemoveAt(a);
                        a--;
                    }
                }
                deathTimer -= 0.001f;
            }
            GameEngine.fireParticles.SetColor(Color.White);
        }
    }
}
