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
    class TSmokeParticleSystem : ParticleSystem
    {
        public TSmokeParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "Particles//smokeToon";

            settings.MaxParticles = 2000;

            settings.Duration = TimeSpan.FromSeconds(2);

            settings.DurationRandomness = 0.5f;

            settings.MinColor = new Color(255, 255, 255, 255);
            settings.MaxColor = new Color(255, 255, 255, 255);

            settings.MinStartSize = 4;
            settings.MaxStartSize = 8;

            settings.MinEndSize = 0;
            settings.MaxEndSize = 0;

            settings.MinRotateSpeed = 0;
            settings.MaxRotateSpeed = 0;

            // Use additive blending.
            settings.BlendState = BlendState.AlphaBlend;
        }
    }
}
