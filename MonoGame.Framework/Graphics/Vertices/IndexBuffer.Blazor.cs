﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class IndexBuffer
    {
        IWebGLRenderingContext GL { get { return this.GraphicsDevice._glContext; } }

        internal WebGLBuffer ibo { get; private set; }

        private void PlatformConstruct(IndexElementSize indexElementSize, int indexCount)
        {

            GenerateIfRequired();
        }

        private void PlatformGraphicsDeviceResetting()
        {
            throw new NotImplementedException();
        }

        void GenerateIfRequired()
        {
            if (ibo == null)
            {
                var sizeInBytes = IndexCount * (this.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

                ibo = GL.CreateBuffer();
                GraphicsExtensions.CheckGLError();
                GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, ibo);
                GraphicsExtensions.CheckGLError();
                GL.BufferData(WebGLBufferType.ELEMENT_ARRAY,
                              sizeInBytes, _isDynamic ? WebGLBufferUsageHint.STREAM_DRAW : WebGLBufferUsageHint.STATIC_DRAW);
                GraphicsExtensions.CheckGLError();
            }
        }

        private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            throw new NotImplementedException();
        }

        private void PlatformSetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options)
            where T : struct
        {
            GenerateIfRequired();

            var elementSizeInByte = ReflectionHelpers.SizeOf<T>.Get();
            var sizeInBytes = elementSizeInByte * elementCount;

            var bufferSize = IndexCount * (IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

            GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, ibo);
            GraphicsExtensions.CheckGLError();

            if (options == SetDataOptions.Discard)
            {
                // By assigning NULL data to the buffer this gives a hint
                // to the device to discard the previous content.
                throw new NotImplementedException();
            }

            GL.BufferSubData(WebGLBufferType.ELEMENT_ARRAY, offsetInBytes, data, elementCount);
            GraphicsExtensions.CheckGLError();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ibo.Dispose();
                ibo = null;
            }

            base.Dispose(disposing);
        }
    }
}