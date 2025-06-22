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
    public enum Specialization
    {
        Normal,
        Builder,
        Soldier,
        Pilot
    }

    [Serializable]
    [XmlInclude(typeof(Troglother))]
    public class Hominid : BaseObjectOwned
    {
        public Hominid() { }
        //Basic constructor
        public Hominid(Planet myPlanet, int Owner)
        {
            planet = myPlanet;

            GetRandomPosition(ref matrix);
            matrix.Translation += planet.matrix.Translation;
            GetRandomPosition(ref goal);
            tempGoal = goal;

            life = (Ability == 8 ? 120 : 100);

            timer = Util.random.Next(60);

            owner = Owner;

            SmokeMark();

            rotation = 0;
            if (planet is Sun)
            {
                life = 0;
            }

            toleratingAtmosphere = RaceManager.GetAtmosphere(Race);
        }

        public Hominid(Planet myPlanet, int Owner, Matrix initial)
        {
            planet = myPlanet;

            if (Vector3.Distance(initial.Translation, planet.matrix.Translation) < planet.radius * 2)
            {
                matrix = Matrix.Invert(Matrix.CreateLookAt(initial.Translation, planet.matrix.Translation, Vector3.Up));
                matrix.Translation = planet.matrix.Translation + matrix.Backward * (planet.radius + 3);
            }
            else
            {
                matrix = initial;
            }

            owner = Owner;

            GetRandomPosition(ref goal);
            life = 100;

            timer = Util.random.Next(60);

            SmokeMark();

            if (planet.hominids.Count >= planet.maxPopulation * 3)
            {
                QuestManager.QuestCall(8);
            }

            rotation = 0;

            if (planet is Sun)
            {
                life = 0;
            }
            toleratingAtmosphere = RaceManager.GetAtmosphere(Race);
        }

        protected Vector3 realPosition;
        protected Vector3 crossVector;
        public Matrix goal;
        public Matrix tempGoal;
        protected float timer;
        public bool flying;
        public Specialization specialization;
        public BaseObjectOwned target;
        private BaseObject[] colliders = new BaseObject[3];
        public Atmosphere toleratingAtmosphere;
        private float dist;
        protected float rotation;

        public int Race
        {
            get { return PlayerManager.GetRace(owner); }
        }

        public int Ability
        {
            get { return RaceManager.GetAbility(PlayerManager.GetRace(owner)); }
        }

        public override void InSerialization()
        {
            //Unfortunately actually we need to delete the target
            //But since the target is found looking at the situation, in an instant the situation can't change
            //The Hominid will immediately find it's target after the save/load
            target = null;
            colliders[0] = null;
            colliders[1] = null;
            colliders[2] = null;
            base.InSerialization();
        }

        public override void OutSerialization()
        {
            timer = (float)Util.random.NextDouble() * 10;
            base.OutSerialization();
        }

        public override string GetHudText()
        {
            return "A "+specialization + " "+RaceManager.GetRace(Race) + base.GetHudText();
        }

        public void Speak(SpeechType speech)
        {
            Balloon myB=GameEngine.balloons.Find(b => b.myCaller == this);
            if (myB==null)
            {
                GameEngine.balloons.Add(new Balloon(this, speech));
            }
            else if (speech == SpeechType.Hate)
            {
                myB = new Balloon(this, speech);
            }
        }

        /// <summary>
        /// Create a smoke emission of the race color
        /// </summary>
        public void SmokeMark()
        {
            GameEngine.tsmokeParticles.SetColor(RaceManager.GetColor(Race));
            for (int a = 0; a < 20; a++)
            {
                GameEngine.tsmokeParticles.AddParticle(matrix.Translation + Util.RandomPointOnSphere(4), Vector3.Zero);
            }
            GameEngine.tsmokeParticles.SetColor(Color.White);
        }

        protected void GetRandomPosition(ref Matrix matrix)
        {
            Vector3 pos;
            do
            {
                pos = Util.RandomPointOnSphere(planet.radius + 3);
            }
            while (GetCollisionObjects(pos));
            matrix = Matrix.Invert(Matrix.CreateLookAt(pos,Vector3.Zero, Vector3.Up));
            matrix.Translation = pos;
        }

        public void SetGoal(Vector3 goalPosition, Vector3 pivot)
        {
            goal = Matrix.Invert(Matrix.CreateLookAt(goalPosition, pivot, Vector3.Up));
            goal.Translation = goalPosition-pivot;
        }

        protected void UpdateGoal(float elapsed)
        {
            //Rotate with planet
            goal *= Matrix.CreateFromAxisAngle(planet.axis, planet.spinSpeed * elapsed);
            tempGoal*= Matrix.CreateFromAxisAngle(planet.axis, planet.spinSpeed * elapsed);

            //Find new cross Vector
            crossVector = Vector3.Normalize(Vector3.Cross(realPosition, tempGoal.Translation));
        }

        public virtual bool Update(float elapsed)
        {
            if (flying)
            {
                //Apply gravity
                speed += Vector3.Normalize(planet.matrix.Translation - matrix.Translation) * elapsed;

                //Flying in the planet atmosphere
                matrix.Translation += speed;
                //Avoid infinite flying
                matrix.Translation += Vector3.Normalize(planet.matrix.Translation - matrix.Translation)/5f;
                dist = Vector3.Distance(matrix.Translation, planet.matrix.Translation);
                //You flew off the planet's atmosphere, die
                if (dist > planet.radius * 2.5f)
                {
                    life = 0;
                    return true;
                }

                //Update matrix
                matrix  = Matrix.Invert(Matrix.CreateLookAt(matrix.Translation, planet.matrix.Translation, Vector3.Up));
                matrix *= Matrix.CreateFromAxisAngle(matrix.Backward, rotation);

                //Land on the planet
                if (dist < planet.radius + 3.1f)
                {
                    dist = planet.radius + 3;
                    //matrix.Translation = planet.matrix.Translation + (matrix.Backward * dist);
                    //If you are not the target of the drag, continue flying
                    if (GameEngine.dragIndex == -1 || planet.hominids.Count > GameEngine.dragIndex && planet.hominids[GameEngine.dragIndex] != this)
                    {
                        flying = false;
                        speed = Vector3.Zero;
                    }
                }

                //Get correct position
                matrix.Translation = planet.matrix.Translation + matrix.Backward * dist;
            }
            else
            {
                dist = Vector3.Distance(matrix.Translation, planet.matrix.Translation);
                //If too far from surface, start flying
                if (dist > planet.radius + 4)
                {
                    flying = true;
                    speed = Vector3.Zero;
                    return false;
                }
                else if (dist < planet.radius + 2.8f)
                {
                    //flying = false;
                    //speed = matrix.Backward;
                    matrix = Matrix.Invert(Matrix.CreateLookAt(matrix.Translation, planet.matrix.Translation, Vector3.Up));
                    matrix.Translation = planet.matrix.Translation + matrix.Backward * (planet.radius + 3);
                    //flying = true;

                    //return false;
                }

                //Decrease breath timer
                timer -= elapsed;
                if (timer <= 0)
                {
                    timer = (Ability == 4 ? 2.5f : 5);
                    if (Ability == 11)
                    {
                        PlayerManager.ChangeEnergy(owner, 0.5f);
                    }
                    else if (Ability == 12)
                    {
                        PlayerManager.ChangeKeldanyum(owner, 0.5f);
                    }
                    if (toleratingAtmosphere==planet.atmosphere)// RaceManager.Tolerate(Race, planet.atmosphere))
                    {
                        if (planet.atmosphere_level < 1)
                        {
                            life -= 10;
                            Speak(SpeechType.Oxygen);
                            //COULD PLANT A NEW TREE (NOT SURE IF KEEPING THIS OR WHAT)
                            if (planet.trees.Count <= planet.hominids.Count / 2)
                            {
                                if (Util.random.Next(20) == 0)
                                {
                                    planet.trees.Add(new Tree(planet, matrix));
                                    planet.trees.Last().matrix.Translation = planet.matrix.Translation + matrix.Backward * planet.radius;
                                    Speak(SpeechType.Tree);
                                }
                            }
                        }
                        else
                        {
                            planet.atmosphere_level -= (Ability == 1 ? 0.5f : 1);
                            if (life < (Ability == 8 ? 120 : 100))
                            {
                                life += (Ability == 0 ? 2 : 1);
                            }
                        }
                    }
                    else
                    {
                        if (Ability == 7)
                        {
                            if (Util.random.Next(100) < 2)
                            {
                                toleratingAtmosphere = planet.atmosphere;
                            }
                        }
                        life -= 10;
                        Speak(SpeechType.Oxygen);
                    }
                }

                //Get the nearest enemy stuff on the planet
                target = NearestStuff(/*, true*/);

                //If it existd, set the goal to the stuff's position
                if (target != null)
                {
                    CheckForBattleMusic(target);
                    SetGoal(target.matrix.Translation, planet.matrix.Translation);
                }
                else
                {
                    float temp;
                    float minDist = 100;
                    target = null;
                    for (int b = 0; b < planet.buildings.Count(); b++)
                    {
                        if (planet.buildings[b] is PreBuilding == true)
                        {
                            //Same XOR statement used in "NearestStuff"
                            if (PlayerManager.GetFriendship(owner, planet.buildings[b].owner) >= 0.5f)
                            {
                                temp = Vector3.Distance(matrix.Translation, planet.buildings[b].matrix.Translation);
                                if (temp < minDist)
                                {
                                    minDist = temp;
                                    target = planet.buildings[b];
                                }
                            }
                        }
                    }

                    //If it exists,  go build it
                    if (target != null)
                    {
                        SetGoal(target.matrix.Translation, planet.matrix.Translation);
                    }
                }
                //Otherwise, no enemies, if your are a Builder check for damaged buildings
                if (target == null && specialization == Specialization.Builder)
                {
                    NearestRepairable(100+planet.radius,ref target);
                    //If it exists, go to repair it
                    if (target != null && target.life < 100)
                    {
                        SetGoal(target.matrix.Translation, planet.matrix.Translation);
                    }
                }

                //Check for obstacles on the way
                ArcCollision(matrix, goal, planet.matrix.Translation);

                //No objects between me and the goal
                int a = 0;
                for (a = 0; a < 3; a++)
                {
                    if (colliders[a] != null && colliders[a]!=target)
                    {
                        //There is somekind of obstacle,must avoid it
                        tempGoal = colliders[a].matrix;// Matrix.Invert(Matrix.CreateLookAt(colliders[a].matrix.Translation, planet.matrix.Translation, Vector3.Up));
                        tempGoal.Translation = colliders[a].matrix.Translation - planet.matrix.Translation;
                        tempGoal *= Matrix.CreateFromAxisAngle(Vector3.Normalize(goal.Translation + planet.matrix.Translation - matrix.Translation)/*matrix.Down*/, 0.5f*(Vector3.Distance(matrix.Translation,colliders[a].matrix.Translation+matrix.Left*10)<Vector3.Distance(matrix.Translation,colliders[a].matrix.Translation+matrix.Right*10)?1:-1));
                        break;
                    }
                }
                if (a == 3)
                {
                    //Directly go to goal
                    tempGoal = goal;
                }

                //Take matrix.Translation to relative
                matrix.Translation -= planet.oldPosition;
                //Rotate with planet
                matrix *= Matrix.CreateFromAxisAngle(planet.axis, planet.spinSpeed * elapsed);
                //Save realPosition
                realPosition = matrix.Translation;

                //Take a temp...
                //Matrix temp = matrix;
                //Rotate facing direction
                float dot=Vector3.Dot(matrix.Right, tempGoal.Backward) / 10;
                matrix *= Matrix.CreateFromAxisAngle(matrix.Backward, dot);
                rotation += dot;
                //If there is enough distance from the actual position to the goal position
                if (Vector3.Distance(realPosition, goal.Translation) > 10)
                {
                    //Update goal position following planet's rotation
                    UpdateGoal(elapsed);

                    //Use timer to slice time, and move only if facing the right direction (avoid moonwalking)
                    if ((int)(timer * 10) % 7 == 0 && Vector3.Dot(matrix.Right, crossVector) > 0.5f)
                    {
                        //Move to goalPosition
                        matrix *= Matrix.CreateFromAxisAngle(crossVector, MathHelper.PiOver4 * elapsed*(Ability == 4 ? 2 : 1));
                    }
                }
                else if (tempGoal == goal && colliders[1] is PreBuilding == false)
                {
                    //Get a new goal
                    GetRandomPosition(ref goal);
                }

                //Take matrix.Translation to absolute
                matrix.Translation += planet.matrix.Translation;

                //Collision with everything, check events
                for (a = 0; a < 3; a++)
                {
                    if (colliders[a] != null && Vector3.Distance(matrix.Translation, colliders[a].matrix.Translation) < 20)
                    {
                        if (target != colliders[a] && Vector3.Distance(matrix.Translation, colliders[a].matrix.Translation) < 8)
                        {
                            //STUCK CASE
                            flying = true;
                            speed = (matrix.Backward + Vector3.Normalize(matrix.Translation - colliders[a].matrix.Translation))/3f;
                            GetRandomPosition(ref goal);
                        }
                        else
                        {
                            switch (a)
                            {
                                case 0:
                                    if (PlayerManager.GetFriendship(owner, ((Hominid)colliders[a]).owner) >= 0.5f)
                                    {
                                        Speak(SpeechType.Greeting);
                                    }
                                    else
                                    {
                                        if (GameEngine.gameCamera.target != planet && ((Hominid)colliders[a]).owner == 0)
                                        {
                                            TextBoard.AddMessage("We are under attack on " + planet.name + "!!");
                                        }
                                        Speak(SpeechType.Hate);
                                        //FIGHT
                                        GameEngine.tsmokeParticles.AddParticle(colliders[a].matrix.Translation + Util.RandomPointOnSphere(5), Vector3.Zero);
                                        //Deal damages to the opponent
                                        if ((int)(timer * 10) % 7 == 0)
                                        {
                                            //Soldier specialization deals more damages
                                            colliders[a].Damage(this, (2 + (specialization == Specialization.Soldier ? 1 : 0)) * (Ability == 5 ? 1 + (float)Util.random.NextDouble() * 0.4f : 1));
                                            if (((Hominid)colliders[a]).Ability == 13)
                                            {
                                                life -= 0.5f;
                                            }
                                            //There is the chance to kick the opponent
                                            if (Util.random.Next(100) == 0)
                                            {
                                                AudioManager.Play3D(this, "punch");
                                                ((Hominid)colliders[a]).flying = true;
                                                ((Hominid)colliders[a]).speed = (colliders[a].matrix.Backward / 2f + Vector3.Normalize(colliders[a].matrix.Translation - matrix.Translation) / 10f);
                                            }
                                            else if ((int)colliders[a].life % 6 == 0)
                                            {
                                                AudioManager.Play3D(this, "slap");
                                            }
                                        }
                                        //Stay on your place
                                        if (colliders[a].life > 0)
                                        {
                                            goal = colliders[a].matrix;
                                            goal.Translation -= planet.matrix.Translation;
                                        }
                                        else
                                        {
                                            PlayerManager.players[owner].KilledHominid();
                                            if (owner == 0)
                                            {
                                                if (colliders[a] is Troglother)
                                                {
                                                    QuestManager.QuestCall(0);
                                                }
                                                else
                                                {
                                                    QuestManager.QuestCall(2);
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case 1:
                                    if (PlayerManager.GetFriendship(owner, ((BaseObjectBuilding)colliders[a]).owner) >= 0.5f)
                                    {
                                        //REPAIR DAMAGED BUILDING
                                        if (colliders[a].life < 100 && specialization == Specialization.Builder)
                                        {
                                            GameEngine.tsmokeParticles.AddParticle(colliders[a].matrix.Translation + Util.RandomPointOnSphere(5), Vector3.Zero);
                                            if ((int)(timer * 10) % 7 == 0)
                                            {
                                                target = (BaseObjectOwned)colliders[a];
                                                target.life += 0.2f;
                                                if (target.owner > 0 && target.owner != owner)
                                                {
                                                    //AI will be glad if someone helps it
                                                    PlayerManager.ChangeFriendship(target.owner, owner, 0.0002f);
                                                    PlayerManager.ChangeTrust(target.owner, owner, 0.0001f);
                                                }
                                            }
                                            goal = matrix;
                                            goal.Translation -= planet.matrix.Translation;

                                        }
                                        else if (colliders[a] is PreBuilding == false)
                                        {
                                            GetRandomPosition(ref goal);
                                        }
                                    }
                                    else
                                    {
                                        //DAMAGE BUILDING
                                        GameEngine.tsmokeParticles.AddParticle(matrix.Translation + Util.RandomPointOnSphere(5), Vector3.Zero);
                                        if ((int)(timer * 10) % 7 == 0)
                                        {
                                            if (GameEngine.gameCamera.target != planet && ((BaseObjectBuilding)colliders[a]).owner == 0)
                                            {
                                                TextBoard.AddMessage("We are under attack on " + planet.name + "!!");
                                            }
                                            colliders[a].Damage(this, 0.25f + (specialization == Specialization.Builder ? 0.5f : 0) * (Ability == 5 ? 1 + (float)Util.random.NextDouble() * 0.4f : 1));
                                            Speak(SpeechType.Hate);
                                            if (Util.random.Next(100) == 0)
                                            {
                                                AudioManager.Play3D(this, "impact_huge");
                                                colliders[a].Burst(5, Color.Orange, 5);
                                                flying = true;
                                                speed = (colliders[a].matrix.Backward / 2f + Vector3.Normalize(colliders[a].matrix.Translation - matrix.Translation) / 2f);
                                            }
                                            else if ((int)colliders[a].life % 8 == 0)
                                            {
                                                AudioManager.Play3D(this, "impact_soft");
                                            }
                                        }
                                        //Stay on your place
                                        if (colliders[a].life > 0)
                                        {
                                            goal = colliders[a].matrix;
                                            goal.Translation -= planet.matrix.Translation;
                                        }
                                        else
                                        {
                                            PlayerManager.players[owner].DestroyedBuilding();
                                        }
                                    }
                                    break;
                                case 2:
                                    GetRandomPosition(ref goal);
                                    break;
                            }
                        }
                    }
                }
            }
            return (life <= 0);
        }

        /// <summary>
        /// Get the BaseObject on the arc going from "start" to "end" with center "offset"
        /// </summary>
        protected void ArcCollision(Matrix start, Matrix end,Vector3 offset)
        {
            start.Translation -=offset;
            Vector3 pivot = Vector3.Cross(Vector3.Normalize(start.Translation), Vector3.Normalize(end.Translation));
            start *= Matrix.CreateFromAxisAngle(pivot, 0.2f);
            start.Translation +=offset;
            GetCollisionObjects(start.Translation);
        }
      
        /// <summary>
        /// Update the drag effect
        /// </summary>
        public void Drag(float distance)
        {
            flying = true;

            Vector3 near = GameEngine.Game.GraphicsDevice.Viewport.Unproject(
                new Vector3(GameEngine.ms.X, GameEngine.ms.Y, 0),
                GameEngine.gameCamera.projectionMatrix,
                GameEngine.gameCamera.viewMatrix,
                Matrix.Identity);

            Vector3 far = GameEngine.Game.GraphicsDevice.Viewport.Unproject(
                new Vector3(GameEngine.ms.X, GameEngine.ms.Y, 1),
                GameEngine.gameCamera.projectionMatrix,
                GameEngine.gameCamera.viewMatrix,
                Matrix.Identity);

            Vector3 direction = Vector3.Normalize(far - near);

            speed = (near + (direction * Math.Min(Vector3.Distance(GameEngine.gameCamera.position,planet.matrix.Translation)-planet.radius,distance))) - matrix.Translation;
        }

        /// <summary>
        /// Get the object I'm colliding with
        /// </summary>
        public bool GetCollisionObjects(Vector3 position)
        {
            int found = 0;
            colliders[0] = (target is Hominid ? target : null);
            for (int h = 0; h < planet.hominids.Count; h++)
            {
                if (planet.hominids[h] != this && planet.hominids[h] != target)
                {
                    if (Vector3.Distance(position, planet.hominids[h].matrix.Translation) <= 10)
                    {
                       colliders[0]=planet.hominids[h];
                       found++;
                        if (PlayerManager.GetFriendship(planet.hominids[h].owner, owner) < 0.5f)
                        {
                            break;
                        }
                    }
                }
            }
            colliders[1] = (target is BaseObjectBuilding ? target : null);
            for (int b = 0; b < planet.buildings.Count; b++)
            {
                if (planet.buildings[b] != target)
                {
                    if (Vector3.Distance(position, planet.buildings[b].matrix.Translation) <= 10)
                    {
                        colliders[1] = planet.buildings[b];
                        found++;
                        if (PlayerManager.GetFriendship(planet.buildings[b].owner, owner) < 0.5f)
                        {
                            break;
                        }
                    }
                }
            }
            colliders[2] = planet.trees.Find(t => Vector3.Distance(position, t.matrix.Translation) <= 10);
            return (found > 0 || colliders[2] != null);
        }

        /// <summary>
        /// Get the nearest stuff (building, hominid) on a planet
        /// </summary>
        public BaseObjectOwned NearestStuff(/*, bool enemy*/)
        {
            BaseObjectOwned stuff = null;
            float temp;
            float minDist = 80;
            for (int a = 0; a < planet.hominids.Count(); a++)
            {
                //XOR USAGE
                //object not friendly and looking for allied: FALSE
                //object not friendly and looking for enemy: TRUE
                //object friendly and looking for allied: TRUE
                //object friendly and looking for enemy: FALSE
                if (PlayerManager.GetFriendship(owner, planet.hominids[a].owner) < 0.5f) //>= 0.5f ^ enemy)
                {
                    temp = Vector3.Distance(matrix.Translation, planet.hominids[a].matrix.Translation);
                    if (temp < minDist)
                    {
                        minDist = temp;
                        stuff = planet.hominids[a];
                    }
                }
            }
            /*if (stuff != null)
            {
                return stuff;
            }*/
            for (int a = 0; a < planet.buildings.Count(); a++)
            {
                if (PlayerManager.GetFriendship(owner, planet.buildings[a].owner) < 0.5f)// >= 0.5f ^ enemy)
                {
                    if (planet.buildings[a].flying == false)
                    {
                        temp = Vector3.Distance(matrix.Translation, planet.buildings[a].matrix.Translation);
                        if (temp < minDist)
                        {
                            minDist = temp;
                            stuff = planet.buildings[a];
                        }
                    }
                }
            }
            return stuff;
        }

        public override void Draw()
        {
            //Util.DrawLine(matrix.Translation, tempGoal.Translation+planet.matrix.Translation, Color.Lime, Color.Red, GameEngine.gameCamera);
            //Util.DrawLine(tempGoal.Translation+planet.matrix.Translation, goal.Translation+planet.matrix.Translation, Color.Red, Color.Lime, GameEngine.gameCamera);
            /*if (PlanetoidGame.game_screen == GameScreen.Gameplay && this==HUD.lastTargetObject && flying==false)
            {
                Util.DrawArc(realPosition, tempGoal.Translation, Color.Yellow, planet.matrix.Translation, GameEngine.gameCamera);
                Util.DrawArc(tempGoal.Translation, goal.Translation, Color.Yellow, planet.matrix.Translation, GameEngine.gameCamera);
                Util.DrawArc(realPosition, goal.Translation, Color.Lime, planet.matrix.Translation, GameEngine.gameCamera);
            }*/
            //Util.DrawMatrixAxis(matrix, GameEngine.gameCamera, 10);
            //Draw me
            Model model = RenderManager.GetModel(this);

            foreach (ModelMesh mm in model.Meshes)
            {
                foreach (BasicEffect effect in mm.Effects)
                {
                    //if (PlanetoidGame.game_screen == GameScreen.Gameplay)
                    //{
                    //    effect.DiffuseColor = Vector3.One * (1-GameEngine.gameCamera.GetPositionHiddenValue(matrix.Translation,Vector3.Zero)/2f);
                    //}
                    //else
                    //{
                        effect.DiffuseColor = Vector3.One;
                    //}
                    effect.Texture = RaceManager.GetTexture(Race);
                    effect.TextureEnabled = true;

                    effect.World = matrix;
                    effect.Projection = GameEngine.gameCamera.projectionMatrix;
                    effect.View = GameEngine.gameCamera.viewMatrix;
                }
                mm.Draw();
            }
            
            if (specialization == Specialization.Normal)
            {
                if (RaceManager.GetFeature(Race) > 0)
                {
                    foreach (ModelMesh mm in RenderManager.features[RaceManager.GetFeature(Race) - 1].Meshes)
                    {
                        foreach (BasicEffect effect in mm.Effects)
                        {
                            effect.DiffuseColor = RaceManager.GetColor(Race).ToVector3();
                            effect.World = matrix;
                            effect.Projection = GameEngine.gameCamera.projectionMatrix;
                            effect.View = GameEngine.gameCamera.viewMatrix;
                        }
                        mm.Draw();
                    }
                }
            }
            else
            {
                foreach (ModelMesh mm in RenderManager.GetHat(specialization).Meshes)
                {
                    foreach (BasicEffect effect in mm.Effects)
                    {
                        effect.World = matrix;
                        effect.Projection = GameEngine.gameCamera.projectionMatrix;
                        effect.View = GameEngine.gameCamera.viewMatrix;
                    }
                    mm.Draw();
                }
            }
        }

        public void DrawLobby(Camera camera)
        {
            Model model = RenderManager.GetModel(this);

            foreach (ModelMesh mm in model.Meshes)
            {
                foreach (BasicEffect effect in mm.Effects)
                {
  
                    effect.DiffuseColor = Vector3.One;
                    //}
                    effect.Texture = RaceManager.GetTexture(Race);
                    effect.TextureEnabled = true;

                    effect.World = matrix;
                    effect.Projection = camera.projectionMatrix;
                    effect.View = camera.viewMatrix;
                }
                mm.Draw();
            }
            if (RaceManager.GetFeature(Race) > 0)
            {
                foreach (ModelMesh mm in RenderManager.features[RaceManager.GetFeature(Race) - 1].Meshes)
                {
                    foreach (BasicEffect effect in mm.Effects)
                    {
                        effect.DiffuseColor = RaceManager.GetColor(Race).ToVector3();
                        effect.World = matrix;
                        effect.Projection = camera.projectionMatrix;
                        effect.View = camera.viewMatrix;
                    }
                    mm.Draw();
                }
            }
        }
    }
}