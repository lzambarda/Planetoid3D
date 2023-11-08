using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Planetoid3D {
    /// <summary>
    /// Render Manager, last review on version 1.0.2
    /// </summary>
    public static class RenderManager {
        //Model load
        public static void LoadModels(Game Game) {
            game = Game;
            //Models
            planetModel = game.Content.Load<Model>("Models//planet");
            shieldModel = game.Content.Load<Model>("Models//shield");
            asteroidModel = game.Content.Load<Model>("Models//asteroid");

            treeModel = new Model[4];
            treeModel[0] = game.Content.Load<Model>("Models//Trees//tree");
            treeModel[1] = game.Content.Load<Model>("Models//Trees//metane_rock");
            treeModel[2] = game.Content.Load<Model>("Models//Trees//sulphur_rock");
            treeModel[3] = game.Content.Load<Model>("Models//Trees//cyanide_rock");
            treeTextures = new Texture2D[treeModel.Length];
            for (int a = 0; a < treeModel.Length; a++) {
                treeTextures[a] = treeModel[a].Meshes[0].Effects[0].Parameters["Texture"].GetValueTexture2D();
            }

            hatModel = new Model[3];
            hatModel[0] = game.Content.Load<Model>("Models//buildHat");
            hatModel[1] = game.Content.Load<Model>("Models//warHat");
            hatModel[2] = game.Content.Load<Model>("Models//pilotHat");

            hominidModel = game.Content.Load<Model>("Models//hominid");
            troglotherModel = game.Content.Load<Model>("Models//troglother");
            shotModel = game.Content.Load<Model>("Models//laser_shot");
            planetoidModel = game.Content.Load<Model>("Models//planetoid");

            //skysphereModel = game.Content.Load<Model>("Models//skysphere");

            buildingsModels = new Model[14];
            buildingsModels[0] = game.Content.Load<Model>("Models//extractor");
            buildingsModels[1] = game.Content.Load<Model>("Models//solar");
            buildingsModels[2] = game.Content.Load<Model>("Models//LKE");
            buildingsModels[3] = game.Content.Load<Model>("Models//reactor");
            buildingsModels[4] = game.Content.Load<Model>("Models//house");
            buildingsModels[5] = game.Content.Load<Model>("Models//school");
            buildingsModels[6] = game.Content.Load<Model>("Models//turbina");
            buildingsModels[7] = game.Content.Load<Model>("Models//radar");
            buildingsModels[8] = game.Content.Load<Model>("Models//rocket");
            buildingsModels[9] = game.Content.Load<Model>("Models//hunter");
            buildingsModels[10] = game.Content.Load<Model>("Models//catapult");
            buildingsModels[11] = game.Content.Load<Model>("Models//turret");
            buildingsModels[12] = game.Content.Load<Model>("Models//sagun");
            buildingsModels[13] = game.Content.Load<Model>("Models//repulser");

            buildingsTextures = new Texture2D[buildingsModels.Length][];
            for (int a = 0; a < buildingsModels.Length; a++) {
                buildingsTextures[a] = new Texture2D[buildingsModels[a].Meshes.Count];

                for (int b = 0; b < buildingsTextures[a].Length; b++) {
                    buildingsTextures[a][b] = buildingsModels[a].Meshes[b].Effects[0].Parameters["Texture"].GetValueTexture2D();
                }
            }

            features = new Model[3];
            features[0] = game.Content.Load<Model>("antenna1");
            features[1] = game.Content.Load<Model>("antenna2");
            features[2] = game.Content.Load<Model>("antenna3");

            planetShader = game.Content.Load<Effect>("PlanetShader");
            postProcessEffect = game.Content.Load<Effect>("PostProcess");
            buildingShader = game.Content.Load<Effect>("BuildingShader");
            gravityShader = game.Content.Load<Effect>("GravityShader");
            textEffect = game.Content.Load<Effect>("Title//TextShader");
            asteroidShader = game.Content.Load<Effect>("AsteroidShader");
            shieldShader = game.Content.Load<Effect>("ShieldShader");

            titleText = game.Content.Load<Texture2D>("Title//text");
            titleBackground = game.Content.Load<Texture2D>("Title//background");
            titleNumber = game.Content.Load<Texture2D>("Title//number");

            textEffect.CurrentTechnique = textEffect.Techniques["Text"];
            textEffect.Parameters["background"].SetValue(titleBackground);

            medalBronze = game.Content.Load<Texture2D>("bronze_medal");
            medalSilver = game.Content.Load<Texture2D>("silver_medal");
            medalGold = game.Content.Load<Texture2D>("gold_medal");

            //Planets textures loading
            textures_planet = new Texture2D[8];
            //textures_normals_planet = new Texture2D[8];
            for (int a = 0; a < textures_planet.Length; a++) {
                textures_planet[a] = game.Content.Load<Texture2D>("Textures//terrain" + a);
                //textures_normals_planet[a] = game.Content.Load<Texture2D>("Textures//terrain" + a+"_Normal");
            }

            asteroid_texture = asteroidModel.Meshes[0].Effects[0].Parameters["Texture"].GetValueTexture2D();

            //Set the texture for the planet's death on the effect
            planetShader.Parameters["ChangeTexture"].SetValue(textures_planet[7]);
            balloonTexture = game.Content.Load<Texture2D>("Panels//panel_balloon");
        }


        private static Game game;
        //Models
        public static Model planetModel;
        public static Model shieldModel;
        public static Model asteroidModel;
        //public static Model skysphereModel;
        public static Model[] treeModel;
        public static Texture2D[] treeTextures;
        public static Model hominidModel;
        public static Model troglotherModel;
        public static Model[] hatModel;
        public static Model shotModel;
        public static Model planetoidModel;
        public static Model[] buildingsModels;
        public static Texture2D[][] buildingsTextures;
        public static Model[] features;

        public static Effect planetShader;
        public static Effect postProcessEffect;
        public static Effect buildingShader;
        public static Effect gravityShader;
        public static Effect textEffect;
        public static Effect shieldShader;
        public static Effect asteroidShader;

        public static Texture2D titleText;
        public static Texture2D titleBackground;
        public static Texture2D titleNumber;

        public static Texture2D[] textures_planet;
        //public static Texture2D[] textures_normals_planet;

        public static Texture2D asteroid_texture;

        public static Texture2D medalBronze;
        public static Texture2D medalSilver;
        public static Texture2D medalGold;

        private static Matrix[] instancingTransforms;
        private static DynamicVertexBuffer instanceVertexBuffer;

        /// <summary>
        /// To store instance transform matrices in a vertex buffer, we use this custom
        /// vertex type which encodes 4x4 matrices as a set of four Vector4 values.
        /// </summary>
        private static VertexDeclaration instanceVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
        );

        public static Texture2D balloonTexture;

        /// <summary>
        /// Get the correct model based on the given object
        /// </summary>
        public static Model GetModel(BaseObject baseObject) {
            if (baseObject is Troglother) {
                return troglotherModel;
            }
            if (baseObject is Hominid) {
                return hominidModel;
            }
            if (baseObject is Asteroid) {
                return asteroidModel;
            }
            if (baseObject is Planet) {
                return planetModel;
            }
            if (baseObject is BaseObjectBuilding) {
                return buildingsModels[(int)(baseObject as BaseObjectBuilding).type - 1];
            }
            if (baseObject is Tree) {
                return treeModel[(int)((Tree)baseObject).myAtmosphere];
            }
            if (baseObject is Planetoid) {
                return planetoidModel;
            }
            if (baseObject is Shot) {
                return shotModel;
            }
            return null;
        }

        /// <summary>
        /// Get the correct hat for each specialization
        /// </summary>
        public static Model GetHat(Specialization specialization) {
            return hatModel[(int)specialization - 1];
        }

        /// <summary>
        /// Draw all the asteroids in game with a single draw call, use the hardware instancing
        /// </summary>
        public static void DrawAllAsteroids() {
            if (GameEngine.asteroids.Count == 0)
                return;

            //Copy asteroid data in a single array
            instancingTransforms = new Matrix[GameEngine.asteroids.Count];
            for (int a = 0; a < instancingTransforms.Length; a++) {
                instancingTransforms[a] = GameEngine.asteroids[a].matrix * Matrix.CreateScale(GameEngine.asteroids[a].life * 1.5f);
                instancingTransforms[a].Translation = GameEngine.asteroids[a].matrix.Translation;
            }

            // If we have more instances than room in our vertex buffer, grow it to the neccessary size.
            if (instanceVertexBuffer == null || instancingTransforms.Length > instanceVertexBuffer.VertexCount) {
                if (instanceVertexBuffer != null)
                    instanceVertexBuffer.Dispose();

                instanceVertexBuffer = new DynamicVertexBuffer(game.GraphicsDevice, instanceVertexDeclaration,
                                                               instancingTransforms.Length, BufferUsage.WriteOnly);
            }

            // Transfer the latest instance transform matrices into the instanceVertexBuffer.
            instanceVertexBuffer.SetData(instancingTransforms, 0, instancingTransforms.Length, SetDataOptions.Discard);

            foreach (ModelMesh mesh in asteroidModel.Meshes) {
                foreach (ModelMeshPart meshPart in mesh.MeshParts) {
                    // Tell the GPU to read from both the model vertex buffer plus our instanceVertexBuffer.
                    game.GraphicsDevice.SetVertexBuffers(
                        new VertexBufferBinding(meshPart.VertexBuffer, meshPart.VertexOffset, 0),
                        new VertexBufferBinding(instanceVertexBuffer, 0, 1)
                    );

                    game.GraphicsDevice.Indices = meshPart.IndexBuffer;

                    // Set up the instance rendering effect.

                    meshPart.Effect = asteroidShader;

                    asteroidShader.CurrentTechnique = asteroidShader.Techniques["HardwareInstancing"];

                    asteroidShader.Parameters["World"].SetValue(Matrix.Identity);//modelBones[mesh.ParentBone.Index]);
                    asteroidShader.Parameters["View"].SetValue(GameEngine.gameCamera.viewMatrix);
                    asteroidShader.Parameters["Projection"].SetValue(GameEngine.gameCamera.projectionMatrix);
                    asteroidShader.Parameters["Texture"].SetValue(asteroid_texture);
                    asteroidShader.Parameters["LightColor"].SetValue(GameEngine.planets[0].color.ToVector3());

                    // Draw all the instance copies in a single call.
                    foreach (EffectPass pass in asteroidShader.CurrentTechnique.Passes) {
                        pass.Apply();

                        game.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList,
                                                               meshPart.NumVertices, meshPart.StartIndex,
                                                               meshPart.PrimitiveCount, instancingTransforms.Length);
                    }
                }
            }
        }

        public static void DrawShield(Repulser repulser) {
            foreach (ModelMesh mesh in shieldModel.Meshes) {
                foreach (ModelMeshPart part in mesh.MeshParts) {
                    part.Effect = shieldShader;
                    shieldShader.CurrentTechnique = shieldShader.Techniques["Shield"];
                    Matrix m = Matrix.CreateScale(repulser.planet.radius * 2);
                    m.Translation = repulser.planet.matrix.Translation;
                    shieldShader.Parameters["xWorld"].SetValue(m);
                    shieldShader.Parameters["xView"].SetValue(GameEngine.gameCamera.viewMatrix);
                    shieldShader.Parameters["xProjection"].SetValue(GameEngine.gameCamera.projectionMatrix);
                    shieldShader.Parameters["hitPosition"].SetValue(repulser.hitPosition);
                    shieldShader.Parameters["sourcePosition"].SetValue(repulser.matrix.Backward);
                    shieldShader.Parameters["hitTimer"].SetValue(repulser.hitTimer);
                    shieldShader.Parameters["timer"].SetValue((float)DateTime.Now.TimeOfDay.TotalSeconds);
                    shieldShader.Parameters["presence"].SetValue(repulser.presence);
                    //shieldShader.Parameters["color"].SetValue(RaceManager.GetColor(PlayerManager.GetRace(repulser.owner)).ToVector4());
                    //shieldShader.CurrentTechnique.Passes[0].Apply();
                }
                mesh.Draw();
            }
        }

        /*public static void DrawSkySphere()
        {
            foreach (ModelMesh mesh in skysphereModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = Matrix.Identity;
                    effect.View = GameEngine.gameCamera.viewMatrix;
                    effect.Projection = GameEngine.gameCamera.projectionMatrix;
                }
                mesh.Draw();
            }
        }*/
    }
}
