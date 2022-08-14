﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Platform.Audio
{
    public sealed class ConcreteDynamicSoundEffectInstance : ConcreteSoundEffectInstance
        , IDynamicSoundEffectInstanceStrategy
    {
        private int _sampleRate;
        private int _channels;

        public event EventHandler<EventArgs> OnBufferNeeded;

        internal ConcreteDynamicSoundEffectInstance(AudioServiceStrategy audioServiceStrategy, int sampleRate, int channels, float pan)
            : base(audioServiceStrategy, null, pan)
        {
            _sampleRate = sampleRate;
            _channels = channels;

        }

        public int DynamicPlatformGetPendingBufferCount()
        {
            throw new NotImplementedException();
        }

        internal override void PlatformPause()
        {
            throw new NotImplementedException();
        }

        internal override void PlatformPlay(bool isLooped)
        {
            throw new NotImplementedException();
        }

        internal override void PlatformResume(bool isLooped)
        {
            throw new NotImplementedException();
        }

        internal override void PlatformStop()
        {
            throw new NotImplementedException();
        }

        internal override void PlatformRelease(bool isLooped)
        {
            System.Diagnostics.Debug.Assert(isLooped == false);

            throw new NotImplementedException();
        }

        public void DynamicPlatformSubmitBuffer(byte[] buffer, int offset, int count, SoundState state)
        {
            throw new NotImplementedException();
        }

        public void DynamicPlatformClearBuffers()
        {
            throw new NotImplementedException();
        }

        public void DynamicPlatformUpdateBuffers()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            base.Dispose(disposing);
        }

    }
}