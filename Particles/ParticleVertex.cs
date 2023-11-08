#region File Description
//-----------------------------------------------------------------------------
// ParticleVertex.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
#endregion

namespace Planetoid3D
{
    /// <summary>
    /// Custom vertex structure for drawing particles.
    /// </summary>
    struct ParticleVertex
    {


         // Stores which corner of the particle quad this vertex represents.
        public Vector2 Corner;

        // Stores the starting position of the particle.
        public Vector3 Position;

        // Stores the starting velocity of the particle.
        public Vector3 Velocity;

        // Four random values, used to make each particle look slightly different.
        public Color Random;

        //A value for changing the color of the particle
        public Color Color;

        // The time (in seconds) at which this particle was created.
        public float Time;

        //A value spacing from 0 to 1 and determinating the particle scaling factor
        public float Size;

        // Describe the layout of this vertex structure.
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
                                                                                         /* new VertexElement(0, VertexElementFormat.Short2,
                                                                                                               VertexElementUsage.Position, 0),

                                                                                          new VertexElement(4, VertexElementFormat.Vector3,
                                                                                                               VertexElementUsage.Position, 1),

                                                                                          new VertexElement(16, VertexElementFormat.Vector3,
                                                                                                                VertexElementUsage.Normal, 0),

                                                                                          new VertexElement(28, VertexElementFormat.Color,
                                                                                                                VertexElementUsage.Color, 0),

                                                                                          new VertexElement(32, VertexElementFormat.Single,
                                                                                                                VertexElementUsage.TextureCoordinate, 0),

                                                                                          new VertexElement(36, VertexElementFormat.Single,
                                                                                                                VertexElementUsage.TextureCoordinate, 1),

                                                                                          new VertexElement(40, VertexElementFormat.Color,
                                                                                                                VertexElementUsage.Color, 1)*/

            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Vector3,VertexElementUsage.Position, 1),
            new VertexElement(20, VertexElementFormat.Vector3,VertexElementUsage.Normal, 1),
            new VertexElement(32, VertexElementFormat.Color,VertexElementUsage.Color, 0),
            new VertexElement(36, VertexElementFormat.Color, VertexElementUsage.Color, 1),
            new VertexElement(40, VertexElementFormat.Single,VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(44, VertexElementFormat.Single,VertexElementUsage.TextureCoordinate, 1)
           

        );
        /* float3 Position : SV_POSITION;
    float2 Corner : NORMAL0;
    float3 Velocity : NORMAL1;
    float4 Random : COLOR0;
	float4 Color : COLOR1;
    float Time : TEXCOORD0;
	float Size : TEXCOORD1;*/

       /*float2 Corner : POSITION0;
    float3 Position : POSITION1;
    float3 Velocity : NORMAL0;
    float4 Random : COLOR0;
	float4 Color : COLOR1;
    float Time : TEXCOORD0;
	float Size : TEXCOORD1;*/


        // Describe the size of this vertex structure.
        public const int SizeInBytes = 48;//44;//36
    }
}
