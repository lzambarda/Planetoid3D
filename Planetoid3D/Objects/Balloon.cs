using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Planetoid3D
{
    public enum SpeechType
    {
        Greeting,
        Hate,
        Oxygen,
        Pick,
        Trogloid,
        Build,
        Tree,
        Planetoid,
        Flying,
        End
    }
    public class Balloon
    {
        public Balloon(Hominid caller, SpeechType type)
        {
            myCaller = caller;
            life = 8;
            if (caller.owner == 8)
            {
                type = SpeechType.Trogloid;
            }
            content = Content[(int)type, Util.random.Next(8)];
        }

        protected static string[,] Content = new string[,]
        {
            {"Hello!","Hi!","Ciao!","How are you?","What a nice day!","Aloha!",":D","Hey!"},
            {"Die!","Take this!","Waaaaaar","@F%Y!","I'll terminate you","Taste my vertices!","Dragon Punch!","Deallocate!"},
            {"I need air!","I can't breathe","*Glug*","Help!!","HELP ME!","I'm suffocating","My lungs!!","I'm gonna die!"},
            {"Put me DOWN","Watch out!","Be careful!","Mommy!!!","Aaaaah","Jeez!","I'm fragile!","Leave meeeeee"},
            {"Hisss","Destroy","Kill","Annihilate","Erase","Dismantle","Conquer","Devastate"},
            {"Let's BUILD!","Work work work","Setting this up!","Welding that...","Building..","Screwing this..","Helping...","Work never over!"},
            {"I'll save my friends!","Planted!","This will help us!","I'm a hero!","There's hope!","Please, grow!","We will not die!","We'll survive!"},
            {"Planetoid!!","This is the end!","HELP!","All is lost!","The green comet!","The Mass!!","Maya were right!","To the rockets?"},
            {"Yuhuuu!","Ypeee!","Yehaaaa!","Uiiiii!","I'm flying!","Aaaaaaaaaaah!","Suppajump!","*Hop*"},
            {"The end!","This is the end!","HELP!","All is lost!","It's too hot!","I'm melting!","Maya were right!","To the rockets?"}
        };

        private string content;
        public Hominid myCaller;
        private float life;
        private Vector3 position;

        public bool Update(float elapsed)
        {
            life -= elapsed;
            if (life <= 0 || myCaller.life <= 0 || myCaller == null)
            {
                return true;
            }
            return false;
        }

        public void Draw(SpriteFont font)
        {
            float amount = (GameEngine.gameCamera.GetPositionHiddenValue(myCaller.matrix.Translation, GameEngine.gameCamera.position) - MathHelper.Clamp(Vector3.Distance(myCaller.matrix.Translation, GameEngine.gameCamera.position) / 1000f, 0, 1)) * (life / 8f);
            amount *= (1 - Math.Min(1, PlanetoidGame.flash));
            position = GameEngine.Game.GraphicsDevice.Viewport.Project(myCaller.matrix.Translation, GameEngine.gameCamera.projectionMatrix, GameEngine.gameCamera.viewMatrix, Matrix.Identity);

            if (position.Z < 1)
            {
                PlanetoidGame.spriteBatch.Draw(RenderManager.balloonTexture, new Vector2(position.X + 10, position.Y + 10), Color.Lerp(Color.Transparent, RaceManager.GetColor(myCaller.Race), amount));
                Util.DrawCenteredText(font, content, new Vector2(position.X + 90, position.Y + 35), Color.Lerp(Color.Transparent, Color.Black, amount));
            }
        }
    }
}
