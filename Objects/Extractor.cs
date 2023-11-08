using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Planetoid3D
{
    [Serializable]
    public class Extractor : BaseObjectBuilding
    {
        public Extractor() { }
        public Extractor(Planet Planet, Matrix initial, int Owner)
        {
            planet = Planet;
            matrix = initial;
            type = BuildingType.Extractor;
            angle = 0;
            grow = true;
            owner = Owner;
            active = true;
        }

        public float angle;
        public bool grow;
        public bool active;


        public override string SecondHUDLabel
        {
            get
            {
                return (active ? "Disable" : "Enable");
            }
        }

        public override void DoSecondHUDAction()
        {
            active = !active;
        }

        public override string GetHudText()
        {
            if (active)
            {
                if (PlayerManager.GetEnergy(owner) > 5)
                {
                    if (planet.available_keldanyum >0)
                    {
                        return "Status: Producing" + base.GetHudText();
                    }
                    else
                    {
                        return "Status: Stopped, keldanyum depleted!" + base.GetHudText();
                    }
                }
                else
                {
                    return "Status: Stopped, need energy!" + base.GetHudText();
                }
            }
            return "Status: Stopped" + base.GetHudText();
        }

        public override bool Update(float elapsed)
        {
            //Update my owner resources
            //Decrease energy every "grow" switch
            //Increase keldanyum every two "grow" switches
            if (PlayerManager.GetEnergy(owner) > 5 && active && planet.available_keldanyum>0)
            {
                if (grow)
                {
                    if (angle < 0.4f)
                    {
                        angle += 0.005f;
                    }
                    else
                    {
                        grow = false;
                        PlayerManager.ChangeEnergy(owner, - 5);
                    }
                }
                else
                {
                    if (angle > -0.4f)
                    {
                        angle -= 0.005f;
                    }
                    else
                    {
                        grow = true;
                        PlayerManager.ChangeKeldanyum(owner, 10 + PlayerManager.players[owner].researchLevels[4]/2);
                        planet.available_keldanyum -= (10 + PlayerManager.players[owner].researchLevels[4] / 2);
                        if (planet.available_keldanyum < 0)
                        {
                            planet.available_keldanyum = 0;
                            if (owner==0)
                            {
                                TextBoard.AddMessage("There is no keldanyum left on " + planet.name + "!");
                            }
                        }
                        if (owner == 0)
                        {
                            QuestManager.QuestCall(5);
                        }
                        PlayerManager.ChangeEnergy(owner, -5);
                    }
                }
            }

            return base.Update(elapsed);
        }

        public override void Draw()
        {
            Model model = RenderManager.GetModel(this);

            model.Bones[1].Transform = matrix;

            Matrix temp = matrix;
            temp.Translation = Vector3.Zero;
            temp *= Matrix.CreateFromAxisAngle(matrix.Right, angle);
            temp.Translation = planet.matrix.Translation + (matrix.Backward * (planet.radius + 9f));
            model.Bones[2].Transform = temp;

            base.DrawUsingBones();
        }
    }
}
