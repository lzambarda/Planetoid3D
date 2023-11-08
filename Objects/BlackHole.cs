using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
//using Microsoft.Xna.Framework.Net;
//using Microsoft.Xna.Framework.Storage;

namespace Planetoid3D
{
    [Serializable]
    public class BlackHole: BaseObject
    {
        public BlackHole()
        {
            matrix = Matrix.CreateTranslation(Util.RandomPointOnSphere(4000));
            speed = -matrix.Translation;
            life = 1;
            AudioManager.Play("blackhole_appear");
        }

        public override void InSerialization()
        {
            planet = null;
        }

        public Vector3 centerCoord;

        public bool Update(float elapsed)
        {
            matrix.Translation += speed*elapsed*20;

            if (Vector3.Distance(matrix.Translation, Vector3.Zero) > 4000)
            {
                speed -= Vector3.Normalize(matrix.Translation);
            }

            NearestPlanet(true,ref planet);

            //Planet repulsion
            if (planet != null)
            {
                speed += 5 * Vector3.Normalize(matrix.Translation - planet.matrix.Translation) / Vector3.Distance(matrix.Translation, planet.matrix.Translation);
            }
            if (GameEngine.planetoid != null)
            {
                speed += 5 * Vector3.Normalize(matrix.Translation - GameEngine.planetoid.matrix.Translation) / Vector3.Distance(matrix.Translation, GameEngine.planetoid.matrix.Translation);
            }
            speed.Normalize();
            speed *=3;
            life += 0.00001f;
            if (life > 15)
            {
                Flash(2);
                GameEngine.planets.Add(new Planet((int)(Vector3.Distance(Vector3.Zero,matrix.Translation)/200),50,GameEngine.planets[0],1));
                GameEngine.planets.Last().matrix.Translation = matrix.Translation;
                GameEngine.planets.Last().planet = null;
                GameEngine.planets.Last().speed = speed*15;
                GameEngine.planets.Last().color = Color.Lerp(Color.LightGray, Color.DimGray, (float)Util.random.NextDouble());
                GameEngine.planets.Last().life *= 1.5f;
                if (GameEngine.gameCamera.target == this)
                {
                    GameEngine.gameCamera.target = GameEngine.planets.Last();
                }
                AudioManager.Play3D(this, "blackhole_die");
                return true;
            }
            return false;
        }

        public new void Draw()
        {
            float hidden = GameEngine.gameCamera.GetPositionHiddenValue(matrix.Translation, GameEngine.gameCamera.position);
            RenderManager.gravityShader.CurrentTechnique = RenderManager.gravityShader.Techniques["Gravity"];
            RenderManager.gravityShader.Parameters["ScreenTexture"].SetValue(MenuManager.pauseScreen);
            RenderManager.gravityShader.Parameters["screenSize"].SetValue(new Vector2(MenuManager.pauseScreen.Width, MenuManager.pauseScreen.Height));
            centerCoord = GameEngine.Game.GraphicsDevice.Viewport.Project(matrix.Translation, GameEngine.gameCamera.projectionMatrix, GameEngine.gameCamera.viewMatrix, Matrix.Identity);
            centerCoord.X /= MenuManager.pauseScreen.Width;
            centerCoord.Y /= MenuManager.pauseScreen.Height;
            RenderManager.gravityShader.Parameters["centerCoord"].SetValue(new Vector2(centerCoord.X, centerCoord.Y));
            RenderManager.gravityShader.Parameters["distanceFromCam"].SetValue((Util.random.Next(20 + (int)life) + Vector3.Distance(matrix.Translation, GameEngine.gameCamera.position)) / (life * 2 * (0.01f + hidden)));
        }
    }
}
