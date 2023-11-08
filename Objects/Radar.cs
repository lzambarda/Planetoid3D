using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Planetoid3D
{
     [Serializable]
    public class Radar : BaseObjectBuilding
    {
         public Radar() { }
        public Radar(Planet Planet, Matrix initial, int Owner)
        {
            planet = Planet;
            matrix = initial;
            type = BuildingType.Radar;
            headAngle = 0;
            owner = Owner;
        }

        public float headAngle;

        public override bool Update(float elapsed)
        {

            headAngle += elapsed/2;
            if (headAngle >= MathHelper.TwoPi)
            {
                headAngle -= MathHelper.TwoPi;
            }

            return base.Update(elapsed);
        }


        public override void Draw()
        {
            Model model = RenderManager.GetModel(this);
            model.Bones[1].Transform = matrix;
            Matrix temp = matrix;
            temp.Translation = Vector3.Zero;
            temp *= Matrix.CreateFromAxisAngle(temp.Right, 0.5f);
            temp *= Matrix.CreateFromAxisAngle(matrix.Backward, headAngle);
            temp.Translation = matrix.Translation + matrix.Backward * 11;
            model.Bones[2].Transform = temp;
            base.DrawUsingBones();
        }
    }
}
