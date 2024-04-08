// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform
{
    sealed class ConcreteGame : GameStrategy
    {
        public ConcreteGame(Game game) : base(game)
        {

        }

        public override void RunOneFrame()
        {
            throw new PlatformNotSupportedException();
        }

        internal override void Run()
        {
            throw new PlatformNotSupportedException();
        }

        public override void TickExiting()
        {
            throw new PlatformNotSupportedException();
        }

        internal override void OnPresentationChanged(PresentationParameters pp)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
