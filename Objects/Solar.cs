using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Planetoid3D
{
    [Serializable]
    public class Solar : BaseObjectBuilding
    {
        public Solar() { }
        public Solar(Planet Planet, Matrix initial, int Owner)
        {
            planet = Planet;
            matrix = initial;
            type = BuildingType.Solar;
            timer = 2;
            owner = Owner;
            angle = 0;
        }

        private float timer;
        public float angle;
        private Ray ray;

        public override string GetHudText()
        {
            if (timer < 2)
            {
                return "Status: Producing " + base.GetHudText();
            }
            return "Status: Stopped" + base.GetHudText();
        }

        public override bool Update(float elapsed)
        {
            //Check if the planet is not covering this panel
            ray.Position = planet.matrix.Translation + (matrix.Backward * (planet.radius + 10f));
            ray.Direction = -Vector3.Normalize(matrix.Translation);
            if (ray.Intersects(new BoundingSphere(planet.matrix.Translation, planet.radius)) == null)
            {
                //Rotate to follow the sun

                matrix *= Matrix.CreateFromAxisAngle(matrix.Backward, Vector3.Dot(matrix.Right, ray.Direction) / 100f);
                angle += Vector3.Dot((matrix * Matrix.CreateFromAxisAngle(matrix.Right, angle)).Down, ray.Direction) / 100f;
                matrix.Translation = planet.matrix.Translation + matrix.Backward * (planet.radius);
                angle = MathHelper.Clamp(angle, -MathHelper.ToRadians(60), MathHelper.ToRadians(60));

                timer -= elapsed;
                //Simply increase owner resources
                if (timer <= 0)
                {
                    PlayerManager.ChangeEnergy(owner, 5 + PlayerManager.players[owner].researchLevels[0] / 4f);
                    timer = 2.5f;
                }
            }
            else
            {
                timer = 2;
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
            temp.Translation = planet.matrix.Translation + (matrix.Backward * (planet.radius + 10f));
            model.Bones[2].Transform = temp;

            base.DrawUsingBones();
        }
    }
}
