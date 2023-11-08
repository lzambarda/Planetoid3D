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
    public static class DataSectorSubdivider
    {
        public static void Initialize(int SectorSize)
        {
            sectorsContainedObjects=new List<BaseObject>();
            sectorSize = SectorSize;
            size = 14000 / sectorSize;
            sectors = new List<BaseObject>[size, size, size];
            vertices = new VertexPositionColor[8 * size * size * size];
            indices = new short[24 * size * size * size];
            Vector3 center;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        sectors[x, y, z] = new List<BaseObject>();
                        center = new Vector3(x - size / 2, y - size / 2, z - size / 2) * sectorSize;
                        vertices[x * 8 + y * size * 8 + z * size * size * 8 + 0] = new VertexPositionColor(center + new Vector3(sectorSize / 2, sectorSize / 2, sectorSize / 2), Color.OrangeRed);
                        vertices[x * 8 + y * size * 8 + z * size * size * 8 + 1] = new VertexPositionColor(center + new Vector3(sectorSize / 2, sectorSize / 2, -sectorSize / 2), Color.OrangeRed);
                        vertices[x * 8 + y * size * 8 + z * size * size * 8 + 2] = new VertexPositionColor(center + new Vector3(-sectorSize / 2, sectorSize / 2, sectorSize / 2), Color.OrangeRed);
                        vertices[x * 8 + y * size * 8 + z * size * size * 8 + 3] = new VertexPositionColor(center + new Vector3(-sectorSize / 2, sectorSize / 2, -sectorSize / 2), Color.OrangeRed);

                        vertices[x * 8 + y * size * 8 + z * size * size * 8 + 4] = new VertexPositionColor(center + new Vector3(sectorSize / 2, -sectorSize / 2, sectorSize / 2), Color.OrangeRed);
                        vertices[x * 8 + y * size * 8 + z * size * size * 8 + 5] = new VertexPositionColor(center + new Vector3(sectorSize / 2, -sectorSize / 2, -sectorSize / 2), Color.OrangeRed);
                        vertices[x * 8 + y * size * 8 + z * size * size * 8 + 6] = new VertexPositionColor(center + new Vector3(-sectorSize / 2, -sectorSize / 2, sectorSize / 2), Color.OrangeRed);
                        vertices[x * 8 + y * size * 8 + z * size * size * 8 + 7] = new VertexPositionColor(center + new Vector3(-sectorSize / 2, -sectorSize / 2, -sectorSize / 2), Color.OrangeRed);
                    }
                }
            }
            int counter = 0;
            for (int a = 0; a < indices.Length; a += 24)
            {
                indices[a + 0] = (short)(counter + 0);
                indices[a + 1] = (short)(counter + 1);
                indices[a + 2] = (short)(counter + 0);
                indices[a + 3] = (short)(counter + 2);
                indices[a + 4] = (short)(counter + 3);
                indices[a + 5] = (short)(counter + 1);
                indices[a + 6] = (short)(counter + 3);
                indices[a + 7] = (short)(counter + 2);
                indices[a + 8] = (short)(counter + 0);
                indices[a + 9] = (short)(counter + 4);
                indices[a + 10] =(short)(counter + 2);
                indices[a + 11] =(short)(counter + 6);
                indices[a + 12] =(short)(counter + 3);
                indices[a + 13] =(short)(counter + 7);
                indices[a + 14] =(short)(counter + 1);
                indices[a + 15] =(short)(counter + 5);
                indices[a + 16] =(short)(counter + 4);
                indices[a + 17] =(short)(counter + 5);
                indices[a + 18] =(short)(counter + 4);
                indices[a + 19] =(short)(counter + 6);
                indices[a + 20] =(short)(counter + 5);
                indices[a + 21] =(short)(counter + 7);
                indices[a + 22] =(short)(counter + 7);
                indices[a + 23] =(short)(counter + 6);
                counter += 8;
            }
        }

        private static List<BaseObject>[, ,] sectors;
        public static VertexPositionColor[] vertices;
        public static short[] indices;
        private static int sectorSize;
        private static int size;
        public static List<BaseObject> sectorsContainedObjects;

        public static void ResetSectorsData()
        {
            /*for (int a = 0; a < vertices.Length; a++)
            {
                vertices[a].Color = new Color(0.0f, 1.0f, 0.0f, 0.05f);
            }*/
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        sectors[x, y, z].Clear();
                    }
                }
            }
        }

        public static void RegisterObject(Asteroid asteroid)
        {
            sectors[size / 2 + (int)Math.Round(asteroid.matrix.Translation.X / sectorSize),
                    size / 2 + (int)Math.Round(asteroid.matrix.Translation.Y / sectorSize),
                    size / 2 + (int)Math.Round(asteroid.matrix.Translation.Z / sectorSize)].Add(asteroid);
        }

        public static void RegisterObject(BaseObjectBuilding building)
        {
            sectors[size / 2 + (int)Math.Round(building.matrix.Translation.X / sectorSize),
                    size / 2 + (int)Math.Round(building.matrix.Translation.Y / sectorSize),
                    size / 2 + (int)Math.Round(building.matrix.Translation.Z / sectorSize)].Add(building);
        }

        public static void UpdateSectorsData()
        {
            Color color;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        color = Color.Lerp(new Color(0.0f,1.0f,0.0f,0.0f), Color.Red, sectors[x, y, z].Count / 20f);
                        vertices[x * 8 + y* size * 8 + z * size * size * 8 + 0].Color =color;
                        vertices[x * 8 + y* size * 8 + z * size * size * 8 + 1].Color =color;
                        vertices[x * 8 + y* size * 8 + z * size * size * 8 + 2].Color =color;
                        vertices[x * 8 + y* size * 8 + z * size * size * 8 + 3].Color =color;
                        vertices[x * 8 + y* size * 8 + z * size * size * 8 + 4].Color =color;
                        vertices[x * 8 + y* size * 8 + z * size * size * 8 + 5].Color =color;
                        vertices[x * 8 + y* size * 8 + z * size * size * 8 + 6].Color =color;
                        vertices[x * 8 + y* size * 8 + z * size * size * 8 + 7].Color =color;
                    }
                }
            }
            /*int xo;
            int yo;
            int zo;
            Vector3 translation;
            for (int a = 0; a < GameEngine.planets.Count; a++)
            {
                translation = GameEngine.planets[a].matrix.Translation;
                xo =size/2+ (int)Math.Round(translation.X / sectorSize);
                yo =size/2+ (int)Math.Round(translation.Y / sectorSize);
                zo =size/2+ (int)Math.Round(translation.Z / sectorSize);
                vertices[xo * 8 + yo * size * 8 + zo * size * size * 8 + 0].Color = Color.Lime;
                vertices[xo * 8 + yo * size * 8 + zo * size * size * 8 + 1].Color = Color.Lime;
                vertices[xo * 8 + yo * size * 8 + zo * size * size * 8 + 2].Color = Color.Lime;
                vertices[xo * 8 + yo * size * 8 + zo * size * size * 8 + 3].Color = Color.Lime;
                vertices[xo * 8 + yo * size * 8 + zo * size * size * 8 + 4].Color = Color.Lime;
                vertices[xo * 8 + yo * size * 8 + zo * size * size * 8 + 5].Color = Color.Lime;
                vertices[xo * 8 + yo * size * 8 + zo * size * size * 8 + 6].Color = Color.Lime;
                vertices[xo * 8 + yo * size * 8 + zo * size * size * 8 + 7].Color = Color.Lime;
            }*/
        }

        public static void FindObjectsInSectors<T>(Vector3 position,float radius)
        {
            sectorsContainedObjects.Clear();

            //find the sector containing "position"
            int xo = size / 2 + (int)Math.Round(position.X / sectorSize);
            int yo = size / 2 + (int)Math.Round(position.Y / sectorSize);
            int zo = size / 2 + (int)Math.Round(position.Z / sectorSize);

            //get the number of sectors covered by the radius
            int covered = (int)Math.Ceiling(radius / sectorSize);

            //load objects contained in the covered sectors!
            for (int x = Math.Max(0,xo-covered); x <= Math.Min(size-1,xo+covered); x++)
            {
                for (int y = Math.Max(0, yo - covered); y <= Math.Min(size-1, yo + covered); y++)
                {
                    for (int z = Math.Max(0, zo - covered); z <= Math.Min(size-1, zo + covered); z++)
                    {

                        sectorsContainedObjects.AddRange(sectors[x, y, z].Where(o=>o is T));
                    }
                }
            }
        }
    }
}
