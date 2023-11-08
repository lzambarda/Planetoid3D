using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Planetoid3D
{
    public class LensFlare : DrawableGameComponent
    {
        public LensFlare(Game game)
            : base(game)
        {
        }

        Texture2D flare1;
        Texture2D flare2;
        Texture2D flare3;
        Texture2D glow;

        SpriteBatch spriteBatch;

        float hiddenAmount;

        Vector2 origin;

        // The lensflare effect is made up from several individual flare graphics,
        // which move across the screen depending on the position of the sun. This
        // helper class keeps track of the position, size, and color for each flare.
        class Flare
        {
            public Flare(float position, float scale, Color color, string textureName)
            {
                Position = position;
                Scale = scale;
                Color = color;
                TextureName = textureName;
            }

            public float Position;
            public float Scale;
            public Color Color;
            public string TextureName;
            public Texture2D Texture;
        }


        // Array describes the position, size, color, and texture for each individual
        // flare graphic. The position value lies on a line between the sun and the
        // center of the screen. Zero places a flare directly over the top of the sun,
        // one is exactly in the middle of the screen, fractional positions lie in
        // between these two points, while negative values or positions greater than
        // one will move the flares outward toward the edge of the screen. Changing
        // the number of flares, or tweaking their positions and colors, can produce
        // a wide range of different lensflare effects without altering any other code.
        Flare[] flares =
        {
            new Flare(-0.25f, 0.6f, new Color( 50,  25,  50), "flare1"),
            new Flare( 0.15f, 0.4f, new Color(100, 255, 200), "flare1"),
            new Flare( 0.6f, 1.0f, new Color(100,  50,  50), "flare1"),
            new Flare( 0.75f, 1.5f, new Color( 50, 100,  50), "flare1"),

            new Flare(-0.15f, 0.7f, new Color(200,  50,  50), "flare2"),
            new Flare( 0.3f, 0.9f, new Color( 50, 100,  50), "flare2"),
            new Flare( 0.35f, 0.4f, new Color( 50, 200, 200), "flare2"),

            new Flare(-0.35f, 0.7f, new Color( 50, 100,  25), "flare3"),
            new Flare( 0.0f, 0.6f, new Color( 25,  25,  25), "flare3"),
            new Flare( 1.0f, 1.4f, new Color( 25,  50, 100), "flare3"),
        };

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            foreach (Flare flare in flares)
            {
                flare.Texture = Game.Content.Load<Texture2D>(flare.TextureName);
            }

            flare1 = Game.Content.Load<Texture2D>("flare1");
            flare2 = Game.Content.Load<Texture2D>("flare1");
            flare3 = Game.Content.Load<Texture2D>("flare1");
            glow = Game.Content.Load<Texture2D>("glow");

            origin = new Vector2(glow.Width, glow.Height) / 2;

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            if (Enabled)
            {
                hiddenAmount = GameEngine.gameCamera.GetPositionHiddenValue(Vector3.Zero, GameEngine.gameCamera.position);

                Vector3 sunPosition = GraphicsDevice.Viewport.Project(Vector3.Zero, GameEngine.gameCamera.projectionMatrix, GameEngine.gameCamera.viewMatrix, Matrix.Identity);

                //DRAW THE GLOW
                spriteBatch.Begin();

                Color color = Color.White * (hiddenAmount / 2f);

                float scale =  MathHelper.Clamp((6000- GameEngine.gameCamera.position.Length()) / 4 / glow.Width,0,10);

                spriteBatch.Draw(glow, new Vector2(sunPosition.X, sunPosition.Y), null, color, 0,
                                 origin, scale, SpriteEffects.None, 0);

                spriteBatch.End();

                Viewport viewport = GraphicsDevice.Viewport;

                // Lensflare sprites are positioned at intervals along a line that
                // runs from the 2D light position toward the center of the screen.

                Vector2 screenCenter = new Vector2(viewport.Width, viewport.Height) / 2;

                Vector2 flareVector = screenCenter - new Vector2(sunPosition.X, sunPosition.Y);

                // Draw the flare sprites using additive blending.
                spriteBatch.Begin(0, BlendState.Additive);

                foreach (Flare flare in flares)
                {
                    // Compute the position of this flare sprite.
                    Vector2 flarePosition = new Vector2(sunPosition.X, sunPosition.Y) + flareVector * flare.Position;

                    // Set the flare alpha based on the previous occlusion query result.
                    Vector4 flareColor = flare.Color.ToVector4();

                    flareColor.W *= hiddenAmount;

                    // Center the sprite texture.
                    Vector2 flareOrigin = new Vector2(flare.Texture.Width,
                                                      flare.Texture.Height) / 2;

                    // Draw the flare.
                    spriteBatch.Draw(flare.Texture, flarePosition, null,
                                     new Color(flareColor), 1, flareOrigin,
                                     flare.Scale, SpriteEffects.None, 0);
                }

                spriteBatch.End();
            }
        }
    }
}
