// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using nkast.Wasm.Audio;

namespace Microsoft.Xna.Platform.Audio
{
    class ConcreteSoundEffect : SoundEffectStrategy
    {
        private AudioBuffer _audioBuffer;


        #region Initialization

        internal override void PlatformLoadAudioStream(Stream stream, out TimeSpan duration)
        {
            duration = TimeSpan.Zero;
        }

        internal override void PlatformInitializePcm(byte[] buffer, int index, int count, int sampleBits, int sampleRate, int channels, int loopStart, int loopLength)
        {
            ConcreteAudioService ConcreteAudioService = (ConcreteAudioService)AudioService.Current._strategy;

            if (index != 0)
                throw new NotImplementedException();
            if (loopStart != 0)
                throw new NotImplementedException();

            var numOfChannels = (int)channels;
            
            _audioBuffer = ConcreteAudioService.Context.CreateBuffer(numOfChannels, loopLength, sampleRate);

            // convert buffer to float (-1,+1) and set data for each channel.
            unsafe
            {
                fixed (void* pBuffer = buffer)
                {
                    switch (sampleBits)
                    {
                        case 16: // PCM 16bit
                            short* pBuffer16 = (short*)pBuffer;
                            var dest = new float[loopLength];
                            for (int c = 0; c < numOfChannels; c++)
                            {
                                for (int i = 0; i < loopLength; i++)
                                {
                                    dest[i] = (float)pBuffer16[i * numOfChannels] / (float)short.MaxValue;
                                }
                                _audioBuffer.CopyToChannel(dest, c);
                            }                           
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }

        }

        internal override void PlatformInitializeFormat(byte[] header, byte[] buffer, int index, int count, int loopStart, int loopLength)
        {
        }

        internal override void PlatformInitializeXactAdpcm(byte[] buffer, int index, int count, int channels, int sampleRate, int blockAlignment, int loopStart, int loopLength)
        {
            
        }

        #endregion

        internal AudioBuffer GetAudioBuffer()
        { 
            return _audioBuffer;
        }

#region IDisposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _audioBuffer.Dispose();
            }

            _audioBuffer = null;
        }

#endregion

    }
}
