﻿// Copyright (C)2023 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class ReadonlyBlendStateStrategy : BlendStateStrategy
    {
        public override Color BlendFactor
        {
            get { return base.BlendFactor; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override int MultiSampleMask
        {
            get { return base.MultiSampleMask; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override BlendFunction AlphaBlendFunction
        {
            get { return base.AlphaBlendFunction; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override Blend AlphaDestinationBlend
        {
            get { return base.AlphaDestinationBlend; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override Blend AlphaSourceBlend
        {
            get { return base.AlphaSourceBlend; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override BlendFunction ColorBlendFunction
        {
            get { return base.ColorBlendFunction; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override Blend ColorDestinationBlend
        {
            get { return base.ColorDestinationBlend; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override Blend ColorSourceBlend
        {
            get { return base.ColorSourceBlend; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override ColorWriteChannels ColorWriteChannels
        {
            get { return base.ColorWriteChannels; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override ColorWriteChannels ColorWriteChannels1
        {
            get { return base.ColorWriteChannels; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override ColorWriteChannels ColorWriteChannels2
        {
            get { return base.ColorWriteChannels; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override ColorWriteChannels ColorWriteChannels3
        {
            get { return base.ColorWriteChannels; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }


        public ReadonlyBlendStateStrategy(Blend sourceBlend, Blend destinationBlend)
        {
            //base.ColorSourceBlend = sourceBlend;
            //base.AlphaSourceBlend = sourceBlend;
            //base.ColorDestinationBlend = destinationBlend;
            //base.AlphaDestinationBlend = destinationBlend;
        }
    }
}