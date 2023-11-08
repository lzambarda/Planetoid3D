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

namespace Planetoid3D {
    public enum Atmosphere {
        Oxygen,//no fading
        Metane,//fading blue
        Sulphur,//fading yellow
        Cyanide,//fading brown
        Ammonia,//fading pink
        Chlorine,//fading green
        None//fading gray
    }

    [Serializable]
    [XmlInclude(typeof(Sun))]
    public class Planet : BaseObject {
        public Planet() {
            life = 10;

            name = RandomName();

            trees = new List<Tree>();
            hominids = new List<Hominid>();
            buildings = new List<BaseObjectBuilding>();
        }

        public Planet(int index, int Radius, Planet orbitAround, float keldanyum_suppressor) {
            name = RandomName();
            radius = Radius;
            texture_index = 1 + Util.random.Next(6);

            orbitSpeed = (float)(MathHelper.Pi / (1000 + index * 50));
            spinSpeed = (float)(MathHelper.Pi / 7000) * 120 / (1.5f + index);
            planet = orbitAround;

            if (planet.name == "Sun") {
                distance = (int)(orbitAround.radius * 6 + Radius + (orbitAround.radius * 3 * index));
                matrix = Matrix.CreateTranslation(new Vector3(distance, 0, 0));
                matrix *= Matrix.CreateRotationZ(
                    MathHelper.ToRadians((float)(10 - (Util.random.NextDouble() * 20)))
                );
            } else {
                distance = (int)(orbitAround.radius * 2 + Radius + (orbitAround.radius * 2 * index));
                matrix = Matrix.CreateTranslation(new Vector3(distance, 0, 0));
                matrix *= Matrix.CreateRotationZ(
                MathHelper.ToRadians((float)(45 - (Util.random.NextDouble() * 90)))
                );
                //Moons orbitates and rotates faster than planets
                spinSpeed *= 1.2f;
                orbitSpeed *= 2;
            }
            matrix.Translation += planet.matrix.Translation;
            axis = matrix.Up;

            hominids = new List<Hominid>();
            trees = new List<Tree>();
            buildings = new List<BaseObjectBuilding>();

            //give it orbit history
            Update((float)Util.random.NextDouble() * 2000);
            oldPosition = matrix.Translation;

            //set up planet stats
            life = (int)Math.Sqrt(Radius * Radius / 8);
            maxPopulation = (int)(radius / 8);
            //atmosphere
            if (Util.random.Next(3) < 2 && GameEngine.blackHole == null) {
                atmosphere = (Atmosphere)Util.random.Next(6);
            } else {
                atmosphere = Atmosphere.None;
            }
            atmosphere_level = 100;
            color = new Color(50 + Util.random.Next(206), 50 + Util.random.Next(206), 50 + Util.random.Next(206));

            //Calculate available keldanyum
            available_keldanyum = (12 * MathHelper.Pi * radius * radius / 2) * keldanyum_suppressor;

            AddTrees();
        }

        public float orbitSpeed;
        public float spinSpeed;
        public float radius;
        public Color color;
        public int distance;
        public string name;
        public int texture_index;
        public Vector3 axis;

        public Vector3 oldPosition;
        public int maxPopulation;
        public Atmosphere atmosphere;
        public float atmosphere_level;
        public float atmosphere_graphic;
        public Vector3 transformed_position;
        public float deathTimer;
        public List<Hominid> hominids;
        public List<BaseObjectBuilding> buildings;
        public List<Tree> trees;
        public float available_keldanyum;

        public override void InSerialization() {
            base.InSerialization();

            //InSerialize linked objects
            foreach (Hominid hominid in hominids) {
                hominid.InSerialization();
            }
            foreach (Tree tree in trees) {
                tree.InSerialization();
            }
            foreach (BaseObjectBuilding building in buildings) {
                building.InSerialization();
            }
        }

        public override void OutSerialization() {
            base.OutSerialization();

            //OutSerialize linked objects
            foreach (Hominid hominid in hominids) {
                hominid.OutSerialization();
            }
            foreach (Tree tree in trees) {
                tree.OutSerialization();
            }
            foreach (BaseObjectBuilding building in buildings) {
                building.OutSerialization();
            }
        }

        public Vector3 ParticleSpeedFix {
            get { return (matrix.Translation - oldPosition) * 50; }
        }

        /// <summary>
        /// Get a random name
        /// </summary>
        private string RandomName() {
            int count = 2 + Util.random.Next(3);
            int chosen = 0;
            char[] vocals = new char[] { 'a', 'e', 'i', 'o', 'u', 'y' };
            string name = "";
            for (int a = 0; a < count; a++) {
                chosen = Util.random.Next(20);
                switch (chosen) {
                    case 0: name += "b" + vocals[Util.random.Next(6)]; break;
                    case 1: name += "c" + vocals[Util.random.Next(6)]; break;
                    case 2: name += "d" + vocals[Util.random.Next(6)]; break;
                    case 3: name += "f" + vocals[Util.random.Next(6)]; break;
                    case 4: name += "g" + vocals[Util.random.Next(6)]; break;
                    case 5: name += "h" + vocals[Util.random.Next(6)]; break;
                    case 6: name += "j" + vocals[Util.random.Next(6)]; break;
                    case 7: name += "k" + vocals[Util.random.Next(6)]; break;
                    case 8: name += "l" + vocals[Util.random.Next(6)]; break;
                    case 9: name += "m" + vocals[Util.random.Next(6)]; break;
                    case 10: name += "n" + vocals[Util.random.Next(6)]; break;
                    case 11: name += "p" + vocals[Util.random.Next(6)]; break;
                    case 12: name += "q" + vocals[Util.random.Next(6)]; break;
                    case 13: name += "r" + vocals[Util.random.Next(6)]; break;
                    case 14: name += "d" + vocals[Util.random.Next(6)]; break;
                    case 15: name += "t" + vocals[Util.random.Next(6)]; break;
                    case 16: name += "v" + vocals[Util.random.Next(6)]; break;
                    case 17: name += "w" + vocals[Util.random.Next(6)]; break;
                    case 18: name += "x" + vocals[Util.random.Next(6)]; break;
                    case 19: name += "z" + vocals[Util.random.Next(6)]; break;
                }
            }
            name = name.Insert(0, name[0].ToString().ToUpper());
            name = name.Remove(1, 1);
            return name;
        }

        /// <summary>
        /// Get planet's color, consider atmosphere type and level
        /// </summary>
        public Color GetColor() {
            if (HUDManager.strategyMode) {
                int dr = DominatingRace();
                return (dr == -1 ? Color.DimGray : RaceManager.GetColor(dr));
            }
            return Color.Lerp(color, GetAtmoshpereColor(atmosphere), 0.7f * (atmosphere_graphic / 100f));
        }

        /// <summary>
        /// Add a variable number of trees
        /// </summary>
        public void AddTrees() {
            trees.Clear();
            if (atmosphere != Atmosphere.None) {
                for (int a = 1 + Util.random.Next((int)radius / 12); a >= 0; a--) {
                    trees.Add(new Tree(this));
                    trees.Last().GrowMax();
                }
            }
        }

        /// <summary>
        /// Get atmosphere color only
        /// </summary>
        public static Color GetAtmoshpereColor(Atmosphere atmosphere) {
            switch (atmosphere) {
                case Atmosphere.Ammonia:
                    return Color.Pink;
                case Atmosphere.Chlorine:
                    return Color.Green;
                case Atmosphere.Cyanide:
                    return Color.Brown;
                case Atmosphere.Sulphur:
                    return Color.Yellow;
                case Atmosphere.Metane:
                    return Color.Aquamarine;
                case Atmosphere.Oxygen:
                    return Color.White;
            }
            return Color.Gray;
        }

        /// <summary>
        /// Populate the planet with the given "amount" of "race" hominids, also add trees and initialize them
        /// </summary>
        public void Populate(int owner, int amount) {
            //Adapt atmosphere
            while (!RaceManager.Tolerate(PlayerManager.GetRace(owner), atmosphere)) {
                atmosphere = (Atmosphere)Util.random.Next(7);
            }
            //Clear trees
            trees.Clear();
            //Generate atmosphere
            atmosphere_level = 100;
            //Generate hominids and trees
            while (amount > 0) {
                hominids.Add(new Hominid(this, owner));
                amount--;
            }
            AddTrees();
        }

        /// <summary>
        /// Get the dominating race (maximum units count) on this planet
        /// </summary>
        public int DominatingRace() {
            int max = 0;
            int index = -1;
            int[] counts = new int[9];

            for (int a = 0; a < hominids.Count; a++) {
                if (++counts[hominids[a].owner] > max) {
                    index = hominids[a].owner;
                    max = counts[index];
                }
            }

            for (int a = 0; a < buildings.Count; a++) {
                if (buildings[a].flying == false) {
                    if ((buildings[a] is House ||
                        (buildings[a] is School && (buildings[a] as School).student != null) ||
                       (buildings[a] is Rocket && (buildings[a] as Rocket).passengersCount > 0) ||
                        (buildings[a] is Hunter && (buildings[a] as Hunter).pilot != null)
                        )
                        && ++counts[buildings[a].owner] > max) {
                        index = buildings[a].owner;
                        max = counts[index];
                    }
                }
            }
            if (index == -1) {
                return -1;
            }
            return PlayerManager.GetRace(index);
        }

        /// <summary>
        /// Get the total population of a planet
        /// </summary>
        /// <returns></returns>
        public int TotalPopulation() {
            int total = 0;
            //Count hominids in buildings
            foreach (BaseObjectBuilding building in buildings) {
                if (building.flying == false) {
                    if (building is Rocket) {
                        total += ((Rocket)building).passengersCount;
                    } else if (building is Hunter) {
                        total += (((Hunter)building).pilot != null ? 1 : 0);
                    } else if (building is School) {
                        total += (((School)building).student != null ? 1 : 0);
                    }
                }
            }
            return hominids.Count(h => h.owner < 8) + total;
        }

        public bool CanBuild(int player) {
            int owner = PlayerManager.GetRaceOwner(DominatingRace());
            return (owner == player || (PlayerManager.GetFriendship(owner, player) >= 0.5f && hominids.Exists(h => h.owner == player)));
        }

        /// <summary>
        /// Destroy all objects that are attached to this planet
        /// </summary>
        private void DestroyChildObjects() {
            int a;
            trees.Clear();
            hominids.Clear();
            for (a = 0; a < buildings.Count(); a++) {
                //Save buildings which are flying, since they're not physically connected to the planet
                if (buildings[a].flying == true) {
                    buildings[a].NearestPlanet(false, ref buildings[a].planet);
                    buildings[a].planet.buildings.Add(buildings[a]);
                    continue;
                } else if (buildings[a] is Radar) {
                    PlayerManager.players[buildings[a].owner].radarAmount--;
                }
            }
            buildings.Clear();

            //Check if the dying planet has moons
            for (a = 0; a < GameEngine.planets.Count(); a++) {
                if (GameEngine.planets[a].planet == this) {
                    //Set the moon free
                    Matrix oldMatrix = GameEngine.planets[a].matrix;
                    GameEngine.planets[a].Update(0.2f);
                    GameEngine.planets[a].speed = (GameEngine.planets[a].matrix.Translation - oldMatrix.Translation) * 75;
                    GameEngine.planets[a].matrix = oldMatrix;
                    GameEngine.planets[a].planet = null;
                    GameEngine.planets[a].distance = 0;
                }
            }
        }

        /// <summary>
        /// Update the planet, every planet, from dieing ones to new born and flying moons MUST use this
        /// </summary>
        public bool Update(float elapsed) {
            //Fix atmosphere level
            atmosphere_level = (int)MathHelper.Clamp(atmosphere_level, 0, 100);
            atmosphere_graphic += (atmosphere_level - atmosphere_graphic) / 100f;
            //Get old position
            oldPosition = matrix.Translation;

            float dist = matrix.Translation.Length();

            //Near to the sun
            if (dist < 400 + GameEngine.planets[0].radius) {
                //All the hominids scream!
                for (int a = 0; a < hominids.Count; a++) {
                    hominids[a].Speak(SpeechType.End);
                }
                //Fire damage
                life -= (1 - dist / (400 + GameEngine.planets[0].radius)) / 200f;
            }
            //If I've a parent planet
            if (planet != null) {
                if (planet is Sun) {
                    if (((Sun)planet).timer > 0 || GameEngine.gameMode == GameMode.Countdown) {
                        //Use normalize to make planets always move at the same "speed"
                        matrix.Translation -= Vector3.Normalize(matrix.Translation) / 15;

                        //Update planet distance
                        distance = (int)Vector3.Distance(matrix.Translation, planet.matrix.Translation);
                    }
                }

                if (Math.Abs(orbitSpeed) > 0.01f) {
                    orbitSpeed *= 0.999f;
                    spinSpeed *= 0.999f;
                }

                //Orbitate
                matrix.Translation -= planet.oldPosition;
                matrix *= Matrix.CreateFromAxisAngle(axis, orbitSpeed * elapsed);
                matrix.Translation += planet.matrix.Translation;
            } else {
                //Never fly away
                if (dist > 4000) {
                    speed -= Vector3.Normalize(matrix.Translation) / 10f;
                }

                //Proceed flying
                matrix.Translation += speed * elapsed;
                //Clamp speed
                /*if (speed.Length() > 50)
                {
                    speed = Vector3.Normalize(speed) * 50;
                }*/

                //If I don't have a planet (I'm a free flying planet)
                if (life > 0) {
                    //Get nearest planet
                    NearestPlanet(true, ref planet);
                    //If the moon is near enough to the nearest planet and the planet's radius is big enough
                    if (planet != null && (planet.planet != null || planet is Sun) && planet.radius - 10 > radius) {
                        //Apply gravity
                        speed += (planet.matrix.Translation - matrix.Translation) / (Vector3.Distance(planet.matrix.Translation, matrix.Translation) * 6);

                        //If I'm in the right distance interval from the planet
                        dist = Vector3.Distance(matrix.Translation, planet.matrix.Translation);
                        if (dist < Math.Pow(planet.radius * (planet is Sun ? 3 : 1), 1.4f) && dist > planet.radius * 2.5f) {
                            float dot = Vector3.Dot(Vector3.Normalize(planet.matrix.Translation - matrix.Translation), Vector3.Normalize(speed));
                            if (Math.Abs(dot) < 0.01f) {
                                //Recalculate distance
                                distance = (int)Vector3.Distance(matrix.Translation, planet.matrix.Translation);

                                //Get direction
                                Vector3 direction = Vector3.Normalize(planet.matrix.Translation - matrix.Translation);

                                //Recalculate axis
                                axis = Vector3.Cross(direction, Vector3.Normalize(speed));

                                //Recalculate orbit and spin Speed
                                orbitSpeed = (float)(speed.Length() / Math.Pow(distance, 1.2f)) * -Math.Sign(dot);
                                //orbitSpeed = (float)(speed.Length() / Math.Pow(distance, 1.2f)) * Math.Sign(dot(p3, theNearestPoint) - dot(p3, theNearestPoint + speed));
                                spinSpeed = orbitSpeed * 2;

                                speed = Vector3.Zero;

                                //Exit, now I'm an orbiting planet
                                return false;
                            }
                        }
                    }

                    //Set parent planet to null to tell the Game I'm still flying
                    planet = null;
                }
            }

            //The planet rotates around itself
            RotateByAxis(axis, (spinSpeed - orbitSpeed) * elapsed);
            /*for (int a = 0; a < buildings.Count; a++)
            {
                buildings[a].matrix.Translation -= oldPosition;
                buildings[a].matrix *= Matrix.CreateFromAxisAngle(axis, spinSpeed * elapsed);
                buildings[a].matrix.Translation += matrix.Translation;
            }*/


            //The planet is alive
            if (life > 0) {
                //Collision between planets
                Planet tempPlanet = planet;
                NearestPlanet(true, ref tempPlanet);
                if (tempPlanet != null) {
                    //Distance must be lower than radius+radius
                    if (Vector3.Distance(matrix.Translation, tempPlanet.matrix.Translation) < radius + tempPlanet.radius) {
                        //Damages are regulated with planets radius
                        Damage(null, tempPlanet.radius / 100);
                        if (life <= 0) {
                            //Directly explode!
                            life = 0;
                            deathTimer = 1f;
                        }
                        //The Sun doesn't receive damages from planets
                        if (tempPlanet is Sun == false) {
                            tempPlanet.Damage(null, radius / 100);
                            if (tempPlanet.life <= 0) {
                                //Directly Explode!
                                tempPlanet.life = 0;
                                tempPlanet.deathTimer = 1f;
                            }
                        }
                    }
                }
            }
            //The planet is dying
            else {
                //Consume Atmosphere
                if (atmosphere_level > 0) {
                    atmosphere_level -= 1;
                }

                if (deathTimer < 1.005f) {
                    deathTimer += 0.005f;

                    //The planet is going to explode
                    if (deathTimer < 1) {
                        //Ground burst
                        for (int a = 0; a < deathTimer * 20; a++) {
                            GameEngine.explosionParticles.AddParticle(matrix.Translation + Util.RandomPointOnSphere(radius), Vector3.Zero);
                        }
                    }
                    //Real planet explosion
                    else {
                        speed = Vector3.Zero;
                        deathTimer += 1;
                        //Delete atmosphere
                        atmosphere = Atmosphere.None;
                        //Shake camera with explosion intensity, regulated by planet's radius
                        GameEngine.gameCamera.shake += (radius * 100f) / Vector3.Distance(matrix.Translation, GameEngine.gameCamera.position);
                        //Stop orbiting
                        orbitSpeed = 0;
                        //Destroy child objects
                        DestroyChildObjects();
                        //Create explosion sphere
                        for (int a = 0; a < 200; a++) {
                            GameEngine.explosionParticles.AddParticle(matrix.Translation + Util.RandomPointOnSphere(radius), Vector3.Zero);
                        }
                        //Create asteroids explosion
                        Vector3 temporary;
                        for (int a = 0; a < radius; a++) {
                            temporary = Util.RandomPointOnSphere(radius);
                            GameEngine.asteroids.Add(new Asteroid(matrix.Translation + temporary, temporary / 4));
                        }
                        AudioManager.Play3D(this, "planet_real_explosion");
                        Flash(1);
                        if (GameEngine.planets.Find(p => p is Sun == false && p.life > 0) == null) {
                            QuestManager.QuestCall(13);
                        }
                    }

                }
                //After the explosion a flaming ring is generated, the ring is the last planet action
                else if (deathTimer < 100) {
                    deathTimer += 0.25f * (elapsed * 100);
                    radius += 0.5f * (elapsed * 100);
                    Vector3 tmp;
                    float angle;
                    //Increase the ring radius and make it burst
                    for (int a = 0; a <= (100 - deathTimer) / 3; a++) {
                        angle = (float)(Util.random.NextDouble() * MathHelper.TwoPi);
                        tmp = new Vector3((float)Math.Cos(angle), 0, (float)Math.Sin(angle)) * radius;

                        tmp = (Matrix.CreateTranslation(tmp) * Matrix.CreateFromAxisAngle(axis, MathHelper.ToRadians(matrix.Translation.X)) * Matrix.CreateRotationZ(MathHelper.ToRadians(a) + MathHelper.PiOver2)).Translation;

                        GameEngine.explosionParticles.AddParticle(matrix.Translation + tmp, Vector3.Zero);
                    }
                } else {
                    //The real death occurs
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Draw the planet using the selected effect
        /// </summary>
        public void DrawPlanet(Camera camera, Effect effect) {
            if (deathTimer < 1) {
                foreach (ModelMesh mesh in RenderManager.GetModel(this).Meshes) {
                    foreach (ModelMeshPart part in mesh.MeshParts) {
                        part.Effect = effect;
                        effect.CurrentTechnique = effect.Techniques["Planet"];
                        effect.CurrentTechnique.Passes[0].Apply();

                        Matrix tmp = matrix;
                        tmp.Translation -= matrix.Translation;
                        tmp *= Matrix.CreateScale(radius);
                        tmp.Translation += matrix.Translation;

                        effect.Parameters["World"].SetValue(tmp);
                        effect.Parameters["View"].SetValue(camera.viewMatrix);
                        effect.Parameters["Projection"].SetValue(camera.projectionMatrix);
                        effect.Parameters["ChangeValue"].SetValue(Math.Max(deathTimer, 0.8f - Math.Min(0.8f, life / 4f)));


                        if (this is Sun) {
                            effect.Parameters["Color"].SetValue((this as Sun).realColor.ToVector4());
                            effect.Parameters["Sun"].SetValue(true);
                        } else {
                            effect.Parameters["Color"].SetValue(GetColor().ToVector4());
                            effect.Parameters["Sun"].SetValue(false);
                            effect.Parameters["LightDir"].SetValue(Vector3.Normalize(matrix.Translation));
                        }

                        effect.Parameters["Texture"].SetValue(RenderManager.textures_planet[texture_index]);
                        //effect.Parameters["NormalTex"].SetValue(RenderManager.textures_normals_planet[texture_index]);
                    }
                    mesh.Draw();
                }
            }
        }
    }
}
