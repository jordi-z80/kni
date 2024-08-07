﻿// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public interface IPlatformOcclusionQuery
    {
        OcclusionQueryStrategy Strategy { get; }
    }

    public abstract class OcclusionQueryStrategy : GraphicsResourceStrategy
    {
        protected readonly GraphicsContextStrategy _contextStrategy;


        public abstract int PixelCount { get; }
        public abstract bool IsComplete { get; }

        protected OcclusionQueryStrategy(GraphicsContextStrategy contextStrategy)
            : base(contextStrategy)
        {
            _contextStrategy = contextStrategy;
        }

        public abstract void PlatformBegin();

        public abstract void PlatformEnd();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }

    }
}
