#region File Description
//-----------------------------------------------------------------------------
// FireParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Planetoid3D
{
    /// <summary>
    /// Custom particle system for creating a flame effect.
    /// </summary>
    class FireParticleSystem : ParticleSystem
    {
        public FireParticleSystem(Game game, ContentManager content)
            : base(game, content) { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "Particles//fire";

            // The sun alone keeps up to ~6000 particles alive on High details (50 per frame, 2s duration),
            // leave room for rockets, asteroids, explosions... (the 16 bit index buffer allows up to 16383)
            settings.MaxParticles = 10000;

            settings.Duration = TimeSpan.FromSeconds(2);

            settings.DurationRandomness = 0.5f;

            settings.MinColor = new Color(255, 255, 255, 200);
            settings.MaxColor = new Color(255, 255, 255, 255);

            settings.MinStartSize = 10;
            settings.MaxStartSize = 20;

            settings.MinEndSize = 20;
            settings.MaxEndSize = 80;

            settings.MinRotateSpeed = 0;
            settings.MaxRotateSpeed = 1;

            // Use additive blending.
            settings.BlendState = BlendState.Additive;
        }
    }
}
