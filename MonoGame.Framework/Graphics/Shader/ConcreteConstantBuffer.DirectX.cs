﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{

    internal sealed class ConcreteConstantBufferStrategy : ConstantBufferStrategy
    {
        internal SharpDX.Direct3D11.Buffer _cbuffer;


        public ConcreteConstantBufferStrategy(GraphicsDevice graphicsDevice, string name, int[] parameters, int[] offsets, int sizeInBytes)
            : base(graphicsDevice, name, parameters, offsets, sizeInBytes)
        {
            _cbuffer = CreateD3D11Buffer();
        }

        private ConcreteConstantBufferStrategy(ConcreteConstantBufferStrategy source)
            : base(source)
        {
            _cbuffer = CreateD3D11Buffer();
        }

        public override object Clone()
        {
            return new ConcreteConstantBufferStrategy(this);
        }

        private SharpDX.Direct3D11.Buffer CreateD3D11Buffer()
        {
            // Allocate the hardware constant buffer.
            var desc = new SharpDX.Direct3D11.BufferDescription();
            desc.Usage = SharpDX.Direct3D11.ResourceUsage.Default;
            desc.BindFlags = SharpDX.Direct3D11.BindFlags.ConstantBuffer;
            desc.CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None;
            desc.SizeInBytes = Buffer.Length;
            lock (GraphicsDevice._d3dContext)
                return new SharpDX.Direct3D11.Buffer(GraphicsDevice._d3dDevice, desc);
        }

        internal override void PlatformClear()
        {
            SharpDX.Utilities.Dispose(ref _cbuffer);
            Dirty = true;
        }

        internal unsafe override void PlatformApply(ShaderStage stage, int slot)
        {
            if (_cbuffer == null)
                _cbuffer = CreateD3D11Buffer();

            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.
            var d3dContext = GraphicsDevice._d3dContext;

            // Update the hardware buffer.
            if (Dirty)
            {
                d3dContext.UpdateSubresource(Buffer, _cbuffer);
                Dirty = false;
            }

            // Set the buffer to the right stage.
            switch (stage)
            {
                case ShaderStage.Pixel: d3dContext.PixelShader.SetConstantBuffer(slot, _cbuffer); break;
                case ShaderStage.Vertex: d3dContext.VertexShader.SetConstantBuffer(slot, _cbuffer); break;
                default: throw new System.ArgumentException();
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SharpDX.Utilities.Dispose(ref _cbuffer);
            }

            base.Dispose(disposing);
        }
    }
}