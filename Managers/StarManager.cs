using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Planetoid3D
{
    /// <summary>
    /// Star Manager, last review on version 1.0.2
    /// </summary>
    public static class StarManager
    {
        /// <summary>
        /// Initialize the Star Manager and all the stars with it
        /// </summary>
        public static void Initialize(Game Game, int starAmount)
        {
            //Load stuff
            starEffect = Game.Content.Load<Effect>("StarEffect");

            //Set up
            starEffect.CurrentTechnique = starEffect.Techniques["StarField"];
            stars = new VertexPositionColor[starAmount*4];
            indices = new short[starAmount * 6];

            //Initialize star positions
            Random rand = new Random();

            Vector3 position;
            Vector3 up;
            Vector3 right;
            Color color;
            float amount;

            for (int current = 0; current < starAmount * 4; current += 4)
            {
                position = Util.RandomPointOnSphere(12000 + rand.Next(250));

                amount = 4 + (float)Util.random.NextDouble() * 8;
                right = amount * Vector3.Normalize(Vector3.Cross(Vector3.Down, Vector3.Normalize(position)));
                up = amount * Vector3.Normalize(Vector3.Cross(Vector3.Normalize(position), right));

                color = new Color(0.7f, 0.7f, 0.7f + (float)Util.random.NextDouble() / 2, 0.5f + (float)Util.random.NextDouble() / 2);

                stars[current] = new VertexPositionColor(position - right - up, color);
                stars[current + 1] = new VertexPositionColor(position + right - up, color);
                stars[current + 2] = new VertexPositionColor(position + right + up, color);
                stars[current + 3] = new VertexPositionColor(position - right + up, color);
            }
           /* for (int current = 0; current < starAmount * 4; current += 4)
            {
                theta = (float)(rand.NextDouble() * MathHelper.Pi);
                phi = (float)(rand.NextDouble() * MathHelper.TwoPi);

                radius = 12000 + rand.Next(250);

                //Use position to create the color
                position.Z = 0.5f + (float)rand.NextDouble() / 2f;
                position.X = position.Z - (float)rand.NextDouble() / 4f;
                position.Y = position.X;               
                color = new Color(position);

                position = radius * new Vector3(
                    (float)(Math.Sin(theta) * Math.Cos(phi)),
                    (float)(Math.Sin(theta) * Math.Sin(phi)),
                    (float)Math.Cos(theta));

                stars[current].Position = position;
                stars[current].Color = color;

                theta += 0.003f;
                position = radius * new Vector3(
                   (float)(Math.Sin(theta) * Math.Cos(phi)),
                   (float)(Math.Sin(theta) * Math.Sin(phi)),
                   (float)Math.Cos(theta));

                stars[current + 1].Position = position;
                stars[current + 1].Color = color;

                phi += 0.003f;
                position = radius * new Vector3(
                   (float)(Math.Sin(theta) * Math.Cos(phi)),
                   (float)(Math.Sin(theta) * Math.Sin(phi)),
                   (float)Math.Cos(theta));

                stars[current + 2].Position = position;
                stars[current + 2].Color = color;

                theta -= 0.003f;
                position = radius * new Vector3(
                   (float)(Math.Sin(theta) * Math.Cos(phi)),
                   (float)(Math.Sin(theta) * Math.Sin(phi)),
                   (float)Math.Cos(theta));

                stars[current + 3].Position = position;
                stars[current + 3].Color = color;
            }*/

            int counter = 0;
            for (int current = 0; current < starAmount * 6; current += 6)
            {
                indices[current] = (short)(counter);
                indices[current + 1] = (short)(counter + 1);
                indices[current + 2] = (short)(counter + 2);
                indices[current + 3] = (short)(counter);
                indices[current + 4] = (short)(counter + 2);
                indices[current + 5] = (short)(counter + 3);
                counter += 4;
            }
        }

        private static Effect starEffect;
        private static VertexPositionColor[] stars;
        private static short[] indices;

        /// <summary>
        /// Render the starfield
        /// </summary>
        public static void RenderStars(Game Game,Matrix view,Matrix projection)
        {
            starEffect.Parameters["World"].SetValue(Matrix.Identity);
            starEffect.Parameters["View"].SetValue(view);
            starEffect.Parameters["Projection"].SetValue(projection);

            foreach (EffectPass pass in starEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }

            Game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, stars, 0, stars.Length, indices, 0, indices.Length / 3);
            //Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, stars, 0, stars.Length / 3);
        }
    }
}
