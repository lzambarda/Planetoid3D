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
    class SmokeParticleSystem : ParticleSystem
    {
        public SmokeParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "Particles//smoke";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(4);

            settings.DurationRandomness = 0.5f;

            settings.MinColor = new Color(255, 255, 255, 100);
            settings.MaxColor = new Color(255, 255, 255, 155);

            settings.MinStartSize = 1;
            settings.MaxStartSize = 2;

            settings.MinEndSize = 10;
            settings.MaxEndSize = 20;

            settings.MinRotateSpeed = 0;
            settings.MaxRotateSpeed = 1;

            // Use additive blending.
            settings.BlendState = BlendState.Additive;
        }
    }
}
