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

namespace Planetoid3D {
    [Serializable]
    public class Camera {
        public Camera() { }
        public Camera(Vector3 Position) {
            position = Position;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), GameEngine.Game.GraphicsDevice.Viewport.AspectRatio, 1.0f, 100000.0f);
            Console.WriteLine(projectionMatrix);
            viewMatrix = Matrix.CreateLookAt(position, tempTar, Vector3.Up);
            targetZoom = 100f;
            realVerticalAngle = 0;
            realHorizontalAngle = 0;
            verticalAngle = 0;
            horizontalAngle = 0;
            //upVector = Vector3.UnitY;
        }

        //Camera variables
        public Vector3 position;
        public Matrix projectionMatrix;
        public Matrix viewMatrix;
        public BaseObject target;
        public Vector3 speed;
        public int index_target;
        public Vector3 tempTar;
        public float zoom;
        public float targetZoom;
        public float verticalAngle;
        public float horizontalAngle;
        private float realVerticalAngle;
        private float realHorizontalAngle;
        public float zoomSpeed;
        public float tempZoom;
        public bool eyeCorner;
        public float hominidViewTranslation;
        //public Vector3 upVector;

        public float shake;

        public void InSerialization() {
            if (target is Planetoid) {
                index_target = -1;
            } else if (target is BlackHole) {
                index_target = -2;
            } else if (target is Planet) {
                index_target = GameEngine.planets.IndexOf(((Planet)target));
            } else {
                if (target is Hominid) {
                    target = ((Hominid)target).planet;
                } else {
                    target = ((BaseObjectBuilding)target).planet;
                }
                index_target = GameEngine.planets.IndexOf(((Planet)target));
            }

            target = null;
        }

        public void OutSerialization() {
            switch (index_target) {
                case -2:
                    target = GameEngine.blackHole;
                    break;
                case -1:
                    target = GameEngine.planetoid;
                    break;
                default:
                    target = GameEngine.planets[index_target];
                    break;
            }
        }

        /// <summary>
        /// Updates camera position and get new view matrix
        /// </summary>
        public void UpdateViewMatrix() {
            Vector3 temporary = tempTar;
            Vector3 upV = Vector3.Up;

            //Camera position updating
            position.X = tempTar.X + (float)(Math.Cos(horizontalAngle) * zoom);
            position.Y = tempTar.Y + (float)(verticalAngle * zoom / 3);
            position.Z = tempTar.Z + (float)(Math.Sin(horizontalAngle) * zoom);

            /*Vector3 upV = Vector3.Transform(Vector3.Up, Matrix.CreateRotationX(modAngle));

            Vector3 modifier = new Vector3(
                (float)(Math.Cos(horizontalAngle) * zoom),
                (float)(verticalAngle * zoom / 3),
                (float)(Math.Sin(horizontalAngle) * zoom)
                );

            //Camera position updating
            position = tempTar + Vector3.Transform(modifier, Matrix.CreateRotationX(modAngle));*/

            if (hominidViewTranslation > 0 && HUDManager.lastTargetObject != null) {
                //LookAt
                tempTar = Vector3.Lerp(tempTar, HUDManager.lastTargetObject.matrix.Translation + HUDManager.lastTargetObject.matrix.Backward + HUDManager.lastTargetObject.matrix.Down * 10, hominidViewTranslation);
                //Position
                position = Vector3.Lerp(position, HUDManager.lastTargetObject.matrix.Translation + HUDManager.lastTargetObject.matrix.Backward * 3 + HUDManager.lastTargetObject.matrix.Down * 6, hominidViewTranslation);
                //UpVector
                upV = Vector3.Lerp(upV, HUDManager.lastTargetObject.matrix.Backward, hominidViewTranslation);
            }

            //Camera view updating
            viewMatrix = Matrix.CreateLookAt(
                 position,
                 tempTar,
                 upV);
        }

        /// <summary>
        /// This method make the camera follow the selected position
        /// </summary>
        public void UpdateCamera() {
            //If the eyeCorner options is set on, the camera's field of view will be influenced by zoom
            if (eyeCorner) {
                projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(Math.Max(90, zoom / 30f)), GameEngine.Game.GraphicsDevice.Viewport.AspectRatio, 1.0f, 100000.0f);
            } else if (zoomSpeed > 0.1f) {
                projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), GameEngine.Game.GraphicsDevice.Viewport.AspectRatio, 1.0f, 100000.0f);
            }

            /*if (GameEngine.ks.IsKeyDown(Keys.Q))
            {
                upVector = Vector3.Transform(upVector, Matrix.CreateRotationX(-0.1f));
            }
            if (GameEngine.ks.IsKeyDown(Keys.E))
            {
                upVector = Vector3.Transform(upVector, Matrix.CreateRotationX(0.1f));
            }*/

            //The player cannot move the camera while cinematic mode is on
            if (GameEngine.gameMode != GameMode.Tutorial || TutorialManager.CinematicMode == false) {
                //Zoom speed handle
                if (!HUDManager.buildingMode && GameEngine.dragIndex == -1) {
                    if (GameEngine.ms.ScrollWheelValue != GameEngine.pms.ScrollWheelValue) {
                        float difference = GameEngine.pms.ScrollWheelValue - GameEngine.ms.ScrollWheelValue;
                        zoomSpeed += difference / 10f;
                        targetZoom += zoomSpeed;
                    } else {
                        zoomSpeed *= 0.95f;
                    }
                } else {
                    zoomSpeed = 0;
                }

                //Rotation
                if (GameEngine.ms.MiddleButton == ButtonState.Pressed || GameEngine.ks.IsKeyDown(Keys.LeftAlt)) {
                    if (GameEngine.ms.X != GameEngine.pms.X) {
                        realHorizontalAngle -= (GameEngine.pms.X - GameEngine.ms.X) * 0.01f;
                    }
                    if (GameEngine.ms.Y != GameEngine.pms.Y) {
                        realVerticalAngle -= (GameEngine.pms.Y - GameEngine.ms.Y) * 0.01f;
                    }
                    if (realVerticalAngle > 3.0f) { realVerticalAngle = 3.0f; }
                    if (realVerticalAngle < -3.0f) { realVerticalAngle = -3.0f; }
                }
                horizontalAngle += (realHorizontalAngle - horizontalAngle) / 5f;
                verticalAngle += (realVerticalAngle - verticalAngle) / 5f;
            }

            int min = 150;
            if (target is Planet) {
                min = Math.Max(150, (int)((Planet)target).radius * 3);
            }

            //Zoom adjust
            if (HUDManager.buildingMode && BuildingManager.fade == 0) {
                if (targetZoom > min) {
                    targetZoom -= 50;
                } else if (targetZoom < min) {
                    targetZoom = min;
                }
            } else {
                if (HUDManager.displayNames) {
                    if (targetZoom < 300) {
                        targetZoom = 300;
                    }
                } else {
                    if (targetZoom < min) {
                        targetZoom = min;
                    }
                }

                if (targetZoom > 3000) {
                    targetZoom = 3000;
                }
            }

            zoom += (targetZoom - zoom) / 25;


            //Target following
            if (hominidViewTranslation <= 0) {
                if (HUDManager.lastTargetObject != null && HUDManager.lastTargetObject is BaseObjectBuilding) {
                    if (tempTar != HUDManager.lastTargetObject.matrix.Translation) {
                        ApproachToPosition(HUDManager.lastTargetObject);
                    }
                } else if (tempTar != target.matrix.Translation) {
                    ApproachToPosition(target);
                }
            } else {
                speed = Vector3.Zero;
            }
            if (shake > 0) {
                tempTar += Util.RandomPointOnSphere(shake);
                shake *= 0.98f;
                shake -= 0.01f;
            }

            UpdateViewMatrix();
            AudioManager.UpdateListener();
        }

        private void ApproachToPosition(BaseObject targ) {
            speed = (targ.matrix.Translation - tempTar) / Math.Max(1, 25 - targ.speed.Length() / 20);
            tempTar += speed;
            float distance = Vector3.Distance(tempTar, targ.matrix.Translation);
            if (distance > 10 && !AudioManager.IsSwishPlaying()) {
                AudioManager.StartSwish();
            } else if (distance < 25) {
                AudioManager.StopSwish();
            } else {
                AudioManager.ChangeVolume("Default", (int)MathHelper.Clamp((targ.matrix.Translation - tempTar).Length() / 50f, 0, 5));
            }
        }

        /// <summary>
        /// If the player is clicking on a planet and "lastTargetObject" is a hominid, the clicked position is the target position for the hominid.
        /// If the player clicks outside the planet a new planet is searched
        /// </summary>
        public void SelectPositionOrPlanet() {
            if (TutorialManager.CameraLocked == false) {
                Vector3 pos = Vector3.Zero;
                if (HUDManager.lastTargetObject != null) {
                    pos = GetClickPositionOnPlanet((Planet)target);
                    if (pos != Vector3.Zero) {
                        ((Hominid)HUDManager.lastTargetObject).SetGoal(pos, target.matrix.Translation);
                    }
                }
                if (pos == Vector3.Zero) {
                    BaseObject oldTarget = target;

                    target = GetTargetObject(typeof(Planet));

                    if (target == oldTarget) {
                        targetZoom = 150;
                    } else if (target == null && oldTarget != null) {
                        HUDManager.lastTargetObject = null;
                        target = oldTarget;
                    }
                    //Zoom remains the same for higher distances
                    //Zoom change for lower distances
                    float dist = Vector3.Distance(position, target.matrix.Translation);
                    if (dist < targetZoom) {
                        targetZoom = dist;
                    }
                }
            }
        }

        public BaseObject GetTargetObject(Type objectType) {
            BaseObject oldTarget = target;
            //Vector3 nearPoint = GameEngine.Game.GraphicsDevice.Viewport.Unproject(new Vector3(GameEngine.ms.X, GameEngine.ms.Y, 0f), projectionMatrix, viewMatrix, Matrix.Identity);

            Vector3 farPoint = GameEngine.Game.GraphicsDevice.Viewport.Unproject(new Vector3(GameEngine.ms.X, GameEngine.ms.Y, 1f), projectionMatrix, viewMatrix, Matrix.Identity);

            // Create a ray from the near clip plane to the far clip plane.
            Vector3 direction = farPoint - position;
            direction.Normalize();
            Ray pickRay = new Ray(position, direction);
            float min = 100000;
            BaseObject Target = null;
            Nullable<float> tmp;

            //START CYCLING THROUGH ALL MATCHING OBJECTS
            if (objectType == typeof(Planet)) {
                foreach (Planet planet in GameEngine.planets) {
                    if (planet.life > 0) {
                        if (HUDManager.displayNames) {
                            tmp = Vector2.Distance(new Vector2(GameEngine.ms.X, GameEngine.ms.Y), new Vector2(planet.transformed_position.X, planet.transformed_position.Y));
                            if (tmp > 50) {
                                tmp = null;
                            }
                        } else {
                            tmp = pickRay.Intersects(new BoundingSphere(planet.matrix.Translation, planet.radius));
                        }
                        if (tmp != null) {
                            if (tmp < min) {
                                min = (float)tmp;
                                Target = planet;
                            }
                        }
                    }
                }
                if (GameEngine.planetoid != null && Vector3.Distance(Vector3.Zero, GameEngine.planetoid.matrix.Translation) < 6000) {
                    tmp = pickRay.Intersects(new BoundingSphere(GameEngine.planetoid.matrix.Translation, 25));
                    if (tmp != null) {
                        if (tmp < min) {
                            min = (float)tmp;
                            Target = GameEngine.planetoid;
                        }
                    }
                }
                if (GameEngine.blackHole != null) {
                    tmp = pickRay.Intersects(new BoundingSphere(GameEngine.blackHole.matrix.Translation, 30));
                    if (tmp != null) {
                        if (tmp < min) {
                            min = (float)tmp;
                            Target = GameEngine.blackHole;
                        }
                    }
                }
            }
            if (target is Planet) {
                Nullable<float> fixDist = pickRay.Intersects(new BoundingSphere(target.matrix.Translation, ((Planet)target).radius));

                if (objectType == typeof(Hominid)) {
                    foreach (Hominid hominid in ((Planet)target).hominids) {
                        if (hominid.Race == (int)PlayerManager.GetRace(0)) {
                            tmp = pickRay.Intersects(new BoundingSphere(hominid.matrix.Translation, 5));
                            if (tmp != null) {
                                if (fixDist == null || tmp < fixDist) {
                                    if (tmp < min) {
                                        min = (float)tmp;
                                        Target = hominid;
                                    }
                                }
                            }
                        }
                    }
                }

                if (objectType == typeof(BaseObjectBuilding)) {
                    foreach (Planet planet in GameEngine.planets) {
                        foreach (BaseObjectBuilding building in planet.buildings) {
                            if (/*building.GetType() != typeof(PreBuilding) &&*/ building.owner == 0) {
                                tmp = pickRay.Intersects(new BoundingSphere(building.matrix.Translation, 10));
                                if (tmp != null) {
                                    if (fixDist == null || tmp < fixDist) {
                                        if (tmp < min) {
                                            min = (float)tmp;
                                            Target = building;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (oldTarget == Target && objectType == typeof(Planet) && GameEngine.ms.RightButton == ButtonState.Pressed) {
                targetZoom = 150;
            }
            if (Target != null && HUDManager.spaceshipSelected == false) {
                HUDManager.lastTargetObject = Target;
                GameEngine.pms = GameEngine.ms;
            }
            return Target;
        }

        /// <summary>
        /// Get the position of intersection between "planet" and the mouse ray
        /// </summary>
        public Vector3 GetClickPositionOnPlanet(Planet planet) {
            Vector3 nearPoint = GameEngine.Game.GraphicsDevice.Viewport.Unproject(new Vector3(GameEngine.ms.X, GameEngine.ms.Y, 0f), projectionMatrix, viewMatrix, Matrix.Identity);

            Vector3 farPoint = GameEngine.Game.GraphicsDevice.Viewport.Unproject(new Vector3(GameEngine.ms.X, GameEngine.ms.Y, 1f), projectionMatrix, viewMatrix, Matrix.Identity);

            // Create a ray from the near clip plane to the far clip plane.
            Ray pickRay = new Ray(nearPoint, Vector3.Normalize(farPoint - nearPoint));
            Nullable<float> tmp;
            BuildingManager.preBuildPosition = Vector3.Zero;
            tmp = pickRay.Intersects(new BoundingSphere(target.matrix.Translation, planet.radius));
            if (tmp != null) {
                return pickRay.Position + (pickRay.Direction * (float)tmp);
            }
            return Vector3.Zero;
        }

        /// <summary>
        /// Returns 1 if the object is hidden by a planet, 0 if none
        /// </summary>
        public float GetPositionHiddenValue(Vector3 objectPosition, Vector3 pivot) {
            //Check if the position is in the camera view
            float obstruct = Vector3.Dot(Vector3.Normalize(objectPosition - pivot), Vector3.Normalize(tempTar - pivot));

            //Check for occluding objects
            Nullable<float> length = 0;
            Ray ray = new Ray(pivot, Vector3.Normalize(objectPosition - pivot));
            Vector3 pos = Vector3.Zero;
            Vector3 p;

            obstruct = Math.Abs(1 - obstruct) / 2;
            for (int a = (objectPosition == Vector3.Zero ? 1 : 0); a < GameEngine.planets.Count; a++) {
                length = ray.Intersects(new BoundingSphere(GameEngine.planets[a].matrix.Translation, GameEngine.planets[a].radius));
                if (length != null && Vector3.Distance(GameEngine.planets[a].matrix.Translation, pivot) < Vector3.Distance(objectPosition, pivot)) {
                    //Get projected distance
                    p = GameEngine.Game.GraphicsDevice.Viewport.Project(GameEngine.planets[a].matrix.Translation, projectionMatrix, viewMatrix, Matrix.Identity);

                    float distance = Vector3.Distance(p,
                                GameEngine.Game.GraphicsDevice.Viewport.Project(ray.Position + (ray.Direction * (float)length),
                                    projectionMatrix,
                                    viewMatrix,
                                    Matrix.Identity)
                                );

                    //Normalize Distance
                    //Normalize dividing by the planet diameter
                    //Also normalize using "length" which is the distance between the camera and the planet, useful to normalize the phenomenon with every zoom value
                    distance /= (GameEngine.planets[a].radius * (1000 / (float)length));

                    distance = (float)Math.Pow(distance, 8);

                    if (distance > 1) {
                        distance = 1;
                    }
                    //Get inverse: light will become stronger with smaller values of distance, invert it to have the right result
                    obstruct = (1 - distance);
                }
            }

            return 1 - obstruct;
        }
    }
}
