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
            stars = new VertexPositionColorTexture[starAmount*4];
            indices = new short[starAmount * 6];

            //Initialize star positions
            Random rand = new Random();

            Vector3 position;
            Color color;
            float radius;

            for (int current = 0; current < starAmount * 4; current += 4)
            {
                position = Util.RandomPointOnSphere(12000 + rand.Next(250));

                //Radius in pixels, the quad is expanded around the star position by the shader
                radius = 1 + (float)Util.random.NextDouble() * 2;

                color = new Color(0.7f, 0.7f, 0.7f + (float)Util.random.NextDouble() / 2, 0.5f + (float)Util.random.NextDouble() / 2);

                stars[current] = new VertexPositionColorTexture(position, color, new Vector2(-radius, -radius));
                stars[current + 1] = new VertexPositionColorTexture(position, color, new Vector2(radius, -radius));
                stars[current + 2] = new VertexPositionColorTexture(position, color, new Vector2(radius, radius));
                stars[current + 3] = new VertexPositionColorTexture(position, color, new Vector2(-radius, radius));
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
        private static VertexPositionColorTexture[] stars;
        private static short[] indices;

        /// <summary>
        /// Render the starfield
        /// </summary>
        public static void RenderStars(Game Game,Matrix view,Matrix projection)
        {
            starEffect.Parameters["World"].SetValue(Matrix.Identity);
            starEffect.Parameters["View"].SetValue(view);
            starEffect.Parameters["Projection"].SetValue(projection);
            starEffect.Parameters["PixelSize"].SetValue(new Vector2(2f / Game.GraphicsDevice.Viewport.Width, 2f / Game.GraphicsDevice.Viewport.Height));

            //Stars are the background, don't depend on the states left by the previous frame
            GraphicsDevice device = Game.GraphicsDevice;
            BlendState blendState = device.BlendState;
            DepthStencilState depthStencilState = device.DepthStencilState;
            RasterizerState rasterizerState = device.RasterizerState;
            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.None;
            device.RasterizerState = RasterizerState.CullNone;

            foreach (EffectPass pass in starEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }

            device.DrawUserIndexedPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, stars, 0, stars.Length, indices, 0, indices.Length / 3);

            device.BlendState = blendState;
            device.DepthStencilState = depthStencilState;
            device.RasterizerState = rasterizerState;
            //Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, stars, 0, stars.Length / 3);
        }
    }
}
