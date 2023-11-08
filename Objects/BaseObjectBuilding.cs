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

using System.Xml.Serialization;

namespace Planetoid3D
{
    [Serializable]
    [XmlInclude(typeof(PreBuilding))]
    [XmlInclude(typeof(Extractor))]
    [XmlInclude(typeof(House))]
    [XmlInclude(typeof(Hunter))]
    [XmlInclude(typeof(LKE))]
    [XmlInclude(typeof(Rocket))]
    [XmlInclude(typeof(School))]
    [XmlInclude(typeof(Solar))]
    [XmlInclude(typeof(Turbina))]
    [XmlInclude(typeof(Turret))]
    [XmlInclude(typeof(SAgun))]
    [XmlInclude(typeof(Radar))]
    [XmlInclude(typeof(Repulser))]
    [XmlInclude(typeof(Catapult))]
    [XmlInclude(typeof(Reactor))]
    public class BaseObjectBuilding : BaseObjectOwned
    {
        public BaseObjectBuilding()
        {
            life = MaxLife;
        }

        public BuildingType type;
        public bool flying;
        public const float MaxLife = 100;

        public virtual float SurfaceOffset
        {
            get { return 0; }
        }

        public virtual string FirstHUDLabel
        {
            get { return "Sell"; }
        }

        public virtual string SecondHUDLabel
        {
            get { return ""; }
        }

        public virtual void DoSecondHUDAction()
        {
            return;
        }

        public virtual void DoFirstHUDAction()
        {
            Sell();
        }

        public virtual void Switch(bool active)
        {
            return;
        }

        public virtual void LiftOff(Planet Destination) { }

        /// <summary>
        /// Leave every eventual contained hominid
        /// </summary>
        public virtual bool LeaveHominid()
        {
            return true;
        }

        /// <summary>
        /// Sell the building and gain half of the resources
        /// </summary>
        public virtual void Sell()
        {
            if (LeaveHominid())
            {
                //GameEngine.buildings.Remove(((BaseObjectBuilding)lastTargetObject));
                //Give to the owner 50% of the used resources
                Point resources = BuildingManager.GetMenuVoice(type).cost;
                PlayerManager.ChangeKeldanyum(owner, resources.X / 2);
                PlayerManager.ChangeEnergy(owner, resources.Y / 2);
                //Remove from the game
                life = -10;
                //Play sold sound
                if (owner == 0)
                {
                    AudioManager.Play("building_sold");
                }
                else
                {
                    AudioManager.Play3D(this,"building_sold");
                }
                if (this == HUDManager.lastTargetObject)
                {
                    HUDManager.lastTargetObject = null;
                }
            }
        }

        /// <summary>
        /// Nullify every pointer to the hominid
        /// </summary>
        public void HominidEnteredBuilding(Hominid hominid)
        {
            if (owner == 0 && GameEngine.dragIndex == planet.hominids.IndexOf(hominid))
            {
                GameEngine.dragIndex = -1;
            }
            hominid.SmokeMark();
            if (HUDManager.lastTargetObject == hominid)
            {
                HUDManager.lastTargetObject = null;
            }
            Balloon balloon = GameEngine.balloons.Find(b => b.myCaller == hominid);
            if (balloon != null)
            {
                GameEngine.balloons.Remove(balloon);
            }
            planet.hominids.Remove(hominid);
            hominid = null;
        }

        public Matrix GetMatrix(float len, Vector3 direction)
        {
            Matrix m = matrix;
            m.Translation += direction * len;
            return m;
        }

        public static Matrix GetFullPlacedMatrix(Vector3 position, Planet ground)
        {
            //Create the matrix for the building
            Matrix testMatrix = Matrix.Invert(Matrix.CreateLookAt(position, Vector3.Zero, Vector3.Up));
            //Give it a random rotation
            testMatrix *= Matrix.CreateFromAxisAngle(testMatrix.Backward, (float)Util.random.NextDouble() * MathHelper.TwoPi);
            //Finally fix position
            testMatrix.Translation = ground.matrix.Translation + position;
            return testMatrix;
        }

        /// <summary>
        /// Update the building and perform the basic operations
        /// </summary>
        public virtual bool Update(float elapsed)
        {
            //Get the new position
            matrix.Translation -= planet.oldPosition;
            matrix *= Matrix.CreateFromAxisAngle(planet.axis, planet.spinSpeed * elapsed);
            matrix.Translation += planet.matrix.Translation;
            
            //Generate smoke and flames when my life is below the optimal, I am damaged
            if (life < MaxLife && life > 0)
            {
                if (Util.random.Next((int)life) < 3)
                {
                    if (Util.random.Next((int)life) < 2)
                    {
                        GameEngine.fireParticles.AddParticle(matrix.Translation + matrix.Backward * 4 + Util.RandomPointOnSphere(2), matrix.Backward);
                    }
                    GameEngine.smokeParticles.AddParticle(matrix.Translation + matrix.Backward * 10 + Util.RandomPointOnSphere(5), matrix.Backward * 2 + Util.RandomPointOnSphere(0.1f));
                }
            }

            return (life <= 0);
        }

        public override void Draw()
        {
            Model model = RenderManager.GetModel(this);
            Texture2D texture;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = RenderManager.buildingShader;
                    part.Effect.CurrentTechnique = part.Effect.Techniques["Building"];

                    part.Effect.CurrentTechnique.Passes[0].Apply();

                    part.Effect.Parameters["World"].SetValue(matrix);
                    part.Effect.Parameters["View"].SetValue(GameEngine.gameCamera.viewMatrix);
                    part.Effect.Parameters["Projection"].SetValue(GameEngine.gameCamera.projectionMatrix);

                    texture = RenderManager.buildingsTextures[(int)(type - 1)][model.Meshes.IndexOf(mesh)];
                    if (texture != null)
                    {
                        part.Effect.Parameters["Texture"].SetValue(texture);
                    }
                    part.Effect.Parameters["Color"].SetValue(RaceManager.GetColor(PlayerManager.GetRace(owner)).ToVector4());
                }
                mesh.Draw();
            }
        }

        public void DrawUsingBones()
        {
            Model model = RenderManager.GetModel(this);
            Texture2D texture;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = RenderManager.buildingShader;
                    part.Effect.CurrentTechnique = part.Effect.Techniques["Building"];

                    part.Effect.CurrentTechnique.Passes[0].Apply();

                    part.Effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                    part.Effect.Parameters["View"].SetValue(GameEngine.gameCamera.viewMatrix);
                    part.Effect.Parameters["Projection"].SetValue(GameEngine.gameCamera.projectionMatrix);
                    part.Effect.Parameters["LightDir"].SetValue(Vector3.Normalize(matrix.Translation));

                    texture = RenderManager.buildingsTextures[(int)(type - 1)][model.Meshes.IndexOf(mesh)];

                    part.Effect.Parameters["Texture"].SetValue(texture);

                    part.Effect.Parameters["Color"].SetValue(RaceManager.GetColor(PlayerManager.GetRace(owner)).ToVector4());
                }
                mesh.Draw();
            }
            /*foreach (ModelMesh mm in model.Meshes)
            {
                foreach (BasicEffect effect in mm.Effects)
                {
                    effect.DiffuseColor = Vector3.One;

                    effect.World = mm.ParentBone.Transform;
                    effect.View = GameEngine.gameCamera.viewMatrix;
                    effect.Projection = GameEngine.gameCamera.projectionMatrix;
                }
                mm.Draw();
            }*/
        }

        public virtual void SecondDraw()
        {
            return;
        }
    }
}