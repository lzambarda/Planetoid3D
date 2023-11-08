using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Planetoid3D
{
    public static class Util
    {
        public static Color PanelGray = new Color(0.23f, 0.23f, 0.23f);
        public static Color PanelLightGray = new Color(0.46f, 0.46f, 0.46f);

        private static VertexPositionTexture[] quadVertices;
        private static VertexPositionColor[] lineVertices;
        private static VertexPositionColor[] orbitVertices;
        private static VertexPositionColor[] buildVertices;
        private static VertexPositionTexture[] rayVertices;
        private static VertexPositionColor[] atmo_vertices;
        private static short[] buildIndices;
        private static Texture2D rayTexture;
        public static Random random;
        private static GraphicsDevice GraphicsDevice;
        private static Effect effect;

        public static void Initialize(GraphicsDevice device, ContentManager Content)
        {
            random = new Random();
            GraphicsDevice = device;
            effect = Content.Load<Effect>("effects");

            lineVertices = new VertexPositionColor[4];
            rayVertices = new VertexPositionTexture[4];
            quadVertices = new VertexPositionTexture[4];

            //Initialize all the vertices
            InitializeVertices();

            rayTexture = Content.Load<Texture2D>("ray_texture");
        }

        public static void InitializeQuadVertices()
        {
            float amount = 4.64f;
            float xAdjust = (1024f / GraphicsDevice.Viewport.Width) * (GraphicsDevice.Viewport.AspectRatio / 1.333f);
            float yAdjust = (768f / GraphicsDevice.Viewport.Height);

            quadVertices[0] = new VertexPositionTexture(new Vector3(-GraphicsDevice.Viewport.Width / amount * xAdjust, -GraphicsDevice.Viewport.Height / amount * yAdjust, 0), Vector2.UnitY);
            quadVertices[1] = new VertexPositionTexture(new Vector3(-GraphicsDevice.Viewport.Width / amount * xAdjust, GraphicsDevice.Viewport.Height / amount * yAdjust, 0), Vector2.Zero);
            quadVertices[2] = new VertexPositionTexture(new Vector3(GraphicsDevice.Viewport.Width / amount * xAdjust, -GraphicsDevice.Viewport.Height / amount * yAdjust, 0), Vector2.One);
            quadVertices[3] = new VertexPositionTexture(new Vector3(GraphicsDevice.Viewport.Width / amount * xAdjust, GraphicsDevice.Viewport.Height / amount * yAdjust, 0), Vector2.UnitX);


        }

        public static void InitializeVertices()
        {
            //For the atmosphere
            int steps = 13 + (3 * PlanetoidGame.details);
            atmo_vertices = new VertexPositionColor[steps * 3];

            float amount = MathHelper.TwoPi / steps;

            for (int a = 0; a < atmo_vertices.Length; a += 3)
            {
                atmo_vertices[a].Position = Vector3.Zero;
                atmo_vertices[a].Color = Color.White;
                atmo_vertices[a + 1].Position = Vector3.Transform(Vector3.UnitX, Matrix.CreateRotationZ(amount * a));
                atmo_vertices[a + 1].Color = Color.Transparent;
                atmo_vertices[a + 2].Position = Vector3.Transform(Vector3.UnitX, Matrix.CreateRotationZ(amount * (a + 1)));
                atmo_vertices[a + 2].Color = Color.Transparent;
            }
            //For the orbits
            int num = 18 * (PlanetoidGame.details * PlanetoidGame.details + 1);
            orbitVertices = new VertexPositionColor[num + 2];

            //For the building
            buildVertices = new VertexPositionColor[17];
            buildIndices = new short[48];
            amount = MathHelper.TwoPi / 16;
            buildVertices[0] = new VertexPositionColor(Vector3.Zero, new Color(new Vector4(0.5f)));
            int counter = 0;
            for (int a = 1; a < buildVertices.Length; a++)
            {
                buildVertices[a] = new VertexPositionColor(Vector3.Transform(new Vector3(10.5f, 0, 0), Matrix.CreateRotationZ(amount * a)), new Color(new Vector4(0.5f)));

                buildIndices[counter + 0] = 0;
                buildIndices[counter + 2] = (short)a;
                buildIndices[counter + 1] = (short)((a == buildVertices.Length - 1 ? 1 : a + 1));
                counter += 3;
            }

            rayVertices[0] = new VertexPositionTexture(Vector3.Zero, Vector2.Zero);
            rayVertices[1] = new VertexPositionTexture(Vector3.Zero, Vector2.UnitY);
            rayVertices[2] = new VertexPositionTexture(Vector3.Zero, Vector2.One);
            rayVertices[3] = new VertexPositionTexture(Vector3.Zero, Vector2.UnitX);
        }

        /// <summary>
        /// Return a random Vector3 with "radius" length
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Vector3 RandomPointOnSphere(float radius)
        {
            /*Matrix position;
            position = Matrix.CreateTranslation(radius, 0, 0);
            position *= Matrix.CreateRotationX((float)(random.NextDouble() * MathHelper.TwoPi));
            position *= Matrix.CreateRotationY((float)(random.NextDouble() * MathHelper.TwoPi));
            position *= Matrix.CreateRotationZ((float)(random.NextDouble() * MathHelper.TwoPi));*/
            Vector3 v;

            // Pick a random vector that has non-zero length.  
            // It's almost certain that the first one picked will be non-zero,  
            // but double check it to avoid division by 0 in Normalize.  
            do
            {
                v.X = (float)random.NextDouble() - 0.5f;
                v.Y = (float)random.NextDouble() - 0.5f;
                v.Z = (float)random.NextDouble() - 0.5f;
            } while (v.LengthSquared() == 0);

            // Normalize the vector so that its length is 1.  
            // This snaps it to the surface of the sphere.  
            v.Normalize();

            return v * radius;
        }

        /// <summary>
        /// Draw a string in the screen applying a centered modifier
        /// </summary>
        public static void DrawCenteredText(SpriteFont font, string text, Vector2 position, Color color)
        {
            string line = "";
            for (int a = 0; a < text.Length; a++)
            {
                if (text[a] == '\n')
                {
                    PlanetoidGame.spriteBatch.DrawString(font, line, position - font.MeasureString(line) / 2, color);
                    line = "";
                    position.Y += 20;
                }
                else
                {
                    line += text[a];
                }
            }
            PlanetoidGame.spriteBatch.DrawString(font, line, position - font.MeasureString(line) / 2, color);
        }

        /// <summary>
        /// Draw a string in the screen applying a centered modifier
        /// </summary>
        public static void DrawCenteredText(SpriteFont font, string text, Vector2 position, Color color, int maxWidth)
        {
            string line = "";
            for (int a = 0; a < text.Length; a++)
            {
                if (text[a] == '\n' || (font.MeasureString(line + text[a]).X > maxWidth && text[a] == ' '))
                {
                    PlanetoidGame.spriteBatch.DrawString(font, line, position - font.MeasureString(line) / 2, color);
                    line = "";
                    position.Y += 20;
                }
                else
                {
                    line += text[a];
                }
            }
            PlanetoidGame.spriteBatch.DrawString(font, line, position - font.MeasureString(line) / 2, color);
        }

        /// <summary>
        /// Draw a given model
        /// </summary>
        public static void DrawModel(Matrix matrix, Model model, Camera camera, Color color)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.DiffuseColor = color.ToVector3();

                    effect.World = matrix;
                    effect.Projection = camera.projectionMatrix;
                    effect.View = camera.viewMatrix;
                }

                mesh.Draw();
            }
        }

        /// <summary>
        /// Draw all the orbits in the game
        /// </summary>
        public static void DrawOrbits(Camera camera)
        {
            if (MenuManager.currentOrbit > Orbit.No)
            {
                effect.CurrentTechnique = effect.Techniques["Atmosphere"];
                effect.Parameters["xWorld"].SetValue(Matrix.Identity);
                effect.Parameters["xView"].SetValue(camera.viewMatrix);
                effect.Parameters["xProjection"].SetValue(camera.projectionMatrix);

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                }

                int amount;

                Matrix matrix;
                for (int planet = 1; planet < GameEngine.planets.Count(); planet++)
                {
                    if (GameEngine.planets[planet].planet != null)
                    {
                        Color col = GameEngine.planets[planet].GetColor();
                        col.A = (byte)((int)MenuManager.currentOrbit * 60);
                        amount = 360 / (orbitVertices.Length - 2);
                        for (int angle = 0; angle <= 360; angle += amount)
                        {

                            matrix = Matrix.CreateTranslation(GameEngine.planets[planet].matrix.Translation);

                            matrix.Translation -= GameEngine.planets[planet].planet.matrix.Translation;
                            matrix *= Matrix.CreateFromAxisAngle(GameEngine.planets[planet].axis, MathHelper.ToRadians(angle));
                            matrix.Translation += GameEngine.planets[planet].planet.matrix.Translation;


                            orbitVertices[angle / amount].Color = col;
                            orbitVertices[angle / amount].Position = matrix.Translation;
                        }
                        GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, orbitVertices, 0, orbitVertices.Length - 2, VertexPositionColor.VertexDeclaration);
                    }
                }
            }
        }

        /// <summary>
        /// Draw the 3 principal axis of a given matrix (green = Up, blue = Backward, red = Right)
        /// </summary>
        public static void DrawMatrixAxis(Matrix matrix, Camera camera, int size)
        {
            effect.CurrentTechnique = effect.Techniques["Atmosphere"];
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            effect.Parameters["xView"].SetValue(camera.viewMatrix);
            effect.Parameters["xProjection"].SetValue(camera.projectionMatrix);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }

            VertexPositionColor[] vertices = new VertexPositionColor[6];

            vertices[4].Position = vertices[2].Position = vertices[0].Position = matrix.Translation;
            vertices[0].Color = Color.Red;
            vertices[1].Color = Color.Red;
            vertices[1].Position = matrix.Translation + (matrix.Right * size);

            vertices[2].Color = Color.Green;
            vertices[3].Color = Color.Green;
            vertices[3].Position = matrix.Translation + (matrix.Up * size);

            vertices[4].Color = Color.Blue;
            vertices[5].Color = Color.Blue;
            vertices[5].Position = matrix.Translation + (matrix.Backward * size);

            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, vertices.Length / 2, VertexPositionColorTexture.VertexDeclaration);

        }

        /// <summary>
        /// Draw the circle showing the minimal building distance
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="camera"></param>
        /// <param name="size"></param>
        public static void DrawBuildingCircle(BaseObjectBuilding building, Camera camera, Color color)
        {
            effect.CurrentTechnique = effect.Techniques["Atmosphere"];
            Matrix m = building.matrix;
            if (building is Rocket && (building as Rocket).destination == building.planet)  
            {
                Vector3 l = m.Left;
                m.Left = m.Right;
                m.Right = l;
                m.Translation = building.planet.matrix.Translation + (building.matrix.Forward * (building.planet.radius - 0.1f));
            }
            else
            {
                m.Translation = building.planet.matrix.Translation + (building.matrix.Backward * (building.planet.radius - 0.1f));
            }
            effect.Parameters["xWorld"].SetValue(m);
            effect.Parameters["xView"].SetValue(camera.viewMatrix);
            effect.Parameters["xProjection"].SetValue(camera.projectionMatrix);

            switch (BuildingManager.menuPrev)
            {
                case -1:
                    color.A = (byte)((1 - BuildingManager.fade) * 100);
                    break;
                case -2:
                    color.A = (byte)(BuildingManager.fade * 100);
                    break;
                default:
                    color.A = 100;
                    break;
            }

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }

            for (int a = 0; a < buildVertices.Length; a++)
            {
                buildVertices[a].Color = color;
            }

            GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, buildVertices, 0, buildVertices.Length, buildIndices, 0, buildIndices.Length / 3);

        }

        /// <summary>
        /// Draw a line of color "color" from point "A" to point "B"
        /// </summary>
        public static void DrawLine(Vector3 A, Vector3 B, Color colorA, Color colorB, Camera camera)
        {
            effect.CurrentTechnique = effect.Techniques["Atmosphere"];
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            effect.Parameters["xView"].SetValue(camera.viewMatrix);
            effect.Parameters["xProjection"].SetValue(camera.projectionMatrix);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }

            lineVertices[0].Position = A;
            lineVertices[0].Color = colorA;
            lineVertices[1].Position = B;
            lineVertices[1].Color = colorB;
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, lineVertices, 0, 1, VertexPositionColorTexture.VertexDeclaration);
        }

        /// <summary>
        /// Draw a textured line "color" from point "A" to point "B"
        /// </summary>
        public static void DrawTextureLine(Vector3 A, Vector3 B, float endsize, Camera camera, Color filter)
        {
            //DrawLine(A, A + up * 30, Color.Red, Color.Red, camera);
            //DrawLine(A, A + right * 30, Color.Lime, Color.Lime, camera);

            effect.CurrentTechnique = effect.Techniques["Ray"];
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            effect.Parameters["xView"].SetValue(camera.viewMatrix);
            effect.Parameters["xProjection"].SetValue(camera.projectionMatrix);
            effect.Parameters["xTexture"].SetValue(rayTexture);
            effect.Parameters["xFilter"].SetValue(filter.ToVector4());
            effect.Parameters["xTime"].SetValue(-(float)DateTime.Now.TimeOfDay.TotalMilliseconds / 900f);

            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            GraphicsDevice.BlendState = BlendState.Additive;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }

            //Calculate vertices position
            Vector3 front = Vector3.Normalize((A + B) / 2 - camera.position);
            Vector3 right = B - A;
            Vector3 up = Vector3.Cross(front, Vector3.Normalize(right));

            rayVertices[0].Position = A + up * 1.5f;
            rayVertices[1].Position = A - up * 1.5f;
            rayVertices[2].Position = B + up * endsize;
            rayVertices[2].TextureCoordinate = new Vector2(right.Length() / 10, 0);
            rayVertices[3].Position = B - up * endsize;
            rayVertices[3].TextureCoordinate = new Vector2(right.Length() / 10, 1);

            GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, rayVertices, 0, 2);

            effect.Parameters["xFilter"].SetValue(Vector4.Zero);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        }

        /// <summary>
        /// Draw a 2d circle projected to fit camera view
        /// </summary>
        public static void DrawCircle(Vector3 center, float radius, Color color, Camera camera)
        {
            effect.CurrentTechnique = effect.Techniques["Atmosphere"];

            Matrix temp = Matrix.Invert(Matrix.CreateLookAt(center, camera.position, Vector3.Up));
            temp *= Matrix.CreateScale(radius);
            temp.Translation = center;

            effect.Parameters["xWorld"].SetValue(temp);
            effect.Parameters["xView"].SetValue(camera.viewMatrix);
            effect.Parameters["xProjection"].SetValue(camera.projectionMatrix);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }

            for (int a = 0; a < atmo_vertices.Length; a += 3)
            {
                atmo_vertices[a].Color = color;
            }

            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, atmo_vertices, 0, atmo_vertices.Length / 3, VertexPositionColorTexture.VertexDeclaration);
        }

        /// <summary>
        /// Draw a texture in 3dimensional space
        /// </summary>
        public static void DrawQuadTexture(Matrix matrix, Texture2D texture, Camera camera)
        {
            effect.CurrentTechnique = effect.Techniques["TexturedNoShading"];
            effect.Parameters["xWorld"].SetValue(matrix);
            effect.Parameters["xView"].SetValue(camera.viewMatrix);
            effect.Parameters["xProjection"].SetValue(camera.projectionMatrix);
            effect.Parameters["xTexture"].SetValue(texture);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }

            GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, quadVertices, 0, 2);
        }

        /// <summary>
        /// Draw a formatted string using a color escape sequence: |color letter|
        /// </summary>
        public static void DrawColoredString(SpriteFont font, string text, Vector2 position, float alpha)
        {
            string toPlot = "";
            Vector2 screenPosition = position;
            Color current = new Color(new Vector4(1, 1, 1, alpha));
            for (int a = 0; a < text.Length; a++)
            {
                if (text[a] == '\n')
                {
                    PlanetoidGame.spriteBatch.DrawString(font, toPlot, screenPosition, current);
                    screenPosition.X = position.X;
                    screenPosition.Y += font.MeasureString(toPlot).Y;
                    toPlot = "";
                }
                else if (text[a] == '|')
                {
                    PlanetoidGame.spriteBatch.DrawString(font, toPlot, screenPosition, current);
                    screenPosition.X += font.MeasureString(toPlot).X;
                    toPlot = "";
                    a++;
                    switch (text[a])
                    {
                        case 'W':
                            current = Color.White;
                            break;
                        case 'R':
                            current = Color.Red;
                            break;
                        case 'G':
                            current = Color.Green;
                            break;
                        case 'g':
                            current = new Color(100, 100, 100);
                            break;
                        case 'B':
                            current = Color.Black;
                            break;
                        case 'Y':
                            current = Color.Yellow;
                            break;
                        case 'C':
                            current = Color.Cyan;
                            break;
                        case 'P':
                            current = PanelGray;
                            break;
                        case 'O':
                            current = Color.Orange;
                            break;
                        case 'M':
                            current = Color.Maroon;
                            break;
                    }
                    current = Color.Lerp(Color.Transparent, current, alpha);
                    a++;
                }
                else
                {
                    toPlot += text[a];
                }
            }
            PlanetoidGame.spriteBatch.DrawString(font, toPlot, screenPosition, current);
        }

        public static void DrawAdaptablePanel(SpriteFont font, string text, Vector2 position, float fade)
        {
            int height = 320 + text.Count(c => c == '\n') * 25;
            PlanetoidGame.spriteBatch.Draw(HUDManager.panel_info, position + new Vector2(0, -height), new Rectangle(0, 0, 304, 34), new Color(fade, fade, fade, fade));
            PlanetoidGame.spriteBatch.Draw(HUDManager.panel_info, new Rectangle((int)position.X, (int)position.Y - height + 34, 304, height - 354), new Rectangle(0, 34, 304, 1), new Color(fade, fade, fade, fade));
            PlanetoidGame.spriteBatch.Draw(HUDManager.panel_info, position + new Vector2(0, -320), new Rectangle(0, 41, 304, 34), new Color(fade, fade, fade, fade));

            DrawColoredString(font, text, position + new Vector2(10, -height + 5), fade);
        }

        /// <summary>
        /// Draw a line of color "color" from point "A" to point "B"
        /// </summary>
        public static void DrawArc(Vector3 A, Vector3 B, Color color, Vector3 center, Camera camera)
        {
            effect.CurrentTechnique = effect.Techniques["Atmosphere"];
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            effect.Parameters["xView"].SetValue(camera.viewMatrix);
            effect.Parameters["xProjection"].SetValue(camera.projectionMatrix);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }

            VertexPositionColor[] v = new VertexPositionColor[10];
            float angleIncrease = (float)Math.Acos(Vector3.Dot(Vector3.Normalize(A), Vector3.Normalize(B)));
            Vector3 pivot = Vector3.Normalize(Vector3.Cross(A, B));
            angleIncrease /= 10f;
            for (int a = 0; a < 10; a++)
            {
                v[a].Color = color;
                v[a].Position = center + A;
                A = Vector3.Transform(A, Matrix.CreateFromAxisAngle(pivot, angleIncrease));
            }

            /*lineVertices[0].Position = A;
            lineVertices[0].Color = colorA;
            lineVertices[1].Position = B;
            lineVertices[1].Color = colorB;*/
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, v, 0, v.Length / 2, VertexPositionColorTexture.VertexDeclaration);
        }


        public static void DrawGrid(Camera camera)
        {
            effect.CurrentTechnique = effect.Techniques["Atmosphere"];
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            effect.Parameters["xView"].SetValue(camera.viewMatrix);
            effect.Parameters["xProjection"].SetValue(camera.projectionMatrix);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }

            GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.LineList, DataSectorSubdivider.vertices, 0, DataSectorSubdivider.vertices.Length, DataSectorSubdivider.indices, 0, DataSectorSubdivider.indices.Length / 2);
        }
    }
}
