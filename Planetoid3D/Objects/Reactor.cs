using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Planetoid3D
{
    [Serializable]
    public class Reactor : BaseObjectBuilding
    {
        public Reactor() { }
        public Reactor(Planet Planet, Matrix initial, int Owner)
        {
            planet = Planet;
            matrix = initial;
            type = BuildingType.Reactor;
            angle = 0;
            owner = Owner;
        }

        public float angle;
        public float timer;
        public bool sold;

        public override string GetHudText()
        {
            return "Status: "+(life<25?"DANGER!!":(life<50?"Warning!":(life<75?"Must be repaired":"Good"))) + base.GetHudText();
        }

        public override void Sell()
        {
            sold = true;
            base.Sell();
        }

        public override bool Update(float elapsed)
        {
            angle += (elapsed * (float)Math.Pow(2 - life / 100f, 2));
            if (angle > MathHelper.TwoPi)
            {
                angle -= MathHelper.TwoPi;
            }
            timer -= elapsed;

            //Simply increase owner resources
            if (timer <= 0)
            {
                PlayerManager.ChangeEnergy(owner, 1.5f);
                timer = 0.25f;
                //Become more unstable
                life -= 0.05f;
            }

            //Leave some troglother smoke, just to warn about the incomin menace
            if (life < 10 && life>0 && Util.random.Next((int)life)<2)
            {
                Burst(1, Color.Purple, 10);
            }

            //Explode
            if (life <= 0 && sold==false)
            {
                Flash(100 / Vector3.Distance(matrix.Translation, GameEngine.gameCamera.position));
                AudioManager.Play3D(this, "planet_real_explosion");
                if (GameEngine.gameCamera.target != planet)
                {
                    TextBoard.AddMessage("A reactor exploded on " + planet.name + "!!!");
                }

                for (int a=0;a<25;a++)
                {
                    GameEngine.explosionParticles.SetColor(Color.Purple);
                    GameEngine.explosionParticles.AddParticle(matrix.Translation , Vector3.Transform(matrix.Left, Matrix.CreateFromAxisAngle(matrix.Backward, (float)Util.random.NextDouble() * MathHelper.TwoPi)) * (float)Util.random.NextDouble() *3);
                    GameEngine.explosionParticles.SetColor(Color.OrangeRed);
                    GameEngine.explosionParticles.AddParticle(matrix.Translation , Vector3.Transform(matrix.Left, Matrix.CreateFromAxisAngle(matrix.Backward, (float)Util.random.NextDouble() * MathHelper.TwoPi)) * (float)Util.random.NextDouble()*3);
                    GameEngine.explosionParticles.SetColor(Color.White);
                }
                
                float dist;
                //Damage nearby buildings
                for (int a = 0; a < planet.buildings.Count; a++)
                {
                    if (planet.buildings[a] != this)
                    {
                        dist = Vector3.Distance(matrix.Translation, planet.buildings[a].matrix.Translation);
                        if (dist < 50)
                        {
                            planet.buildings[a].life -= (150-dist);
                        }
                    }
                }
                //Damage nearby trees
                for (int a = 0; a < planet.trees.Count; a++)
                {
                    if (Vector3.Distance(matrix.Translation, planet.trees[a].matrix.Translation) < 50)
                    {
                        planet.trees[a].life = 0;
                        planet.trees[a].Burst(5, Color.Purple, 10);
                    }
                }
                //Transform nearby hominids in troglothers
                for (int a = 0; a < planet.hominids.Count; a++)
                {
                    if (Vector3.Distance(matrix.Translation, planet.hominids[a].matrix.Translation) < 50)
                    {
                        planet.hominids[a].owner = 8;
                        planet.hominids[a].SmokeMark();
                    }
                }
            }

            return base.Update(elapsed);
        }

        public override void Draw()
        {
            Model model = RenderManager.GetModel(this);

            model.Bones["Reactor"].Transform = matrix;

            Matrix temp = matrix;
            temp.Translation = Vector3.Zero;
            temp *= Matrix.CreateFromAxisAngle(matrix.Right, (float)(Math.Sin(angle*2f) * Math.Pow(1 - life / 100f,2)));
            temp *= Matrix.CreateFromAxisAngle(matrix.Up, (float)(Math.Cos(angle) * Math.Pow(1 - life / 100f,2)));
            temp *= Matrix.CreateFromAxisAngle(matrix.Backward, angle);
            temp.Translation = planet.matrix.Translation + (matrix.Backward * (planet.radius + 10f));
            model.Bones["Ring"].Transform = temp;

            base.DrawUsingBones();
        }
    }
}
