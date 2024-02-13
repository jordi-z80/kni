// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Effect
    {
        private class EffectReader09 : BinaryReader
        {
            private readonly GraphicsDevice _graphicsDevice;
            private readonly MGFXHeader _header;

            public EffectReader09(MemoryStream stream, GraphicsDevice graphicsDevice, MGFXHeader header) : base(stream)
            {
                this._header = header;
                this._graphicsDevice = graphicsDevice;
            }

            private int ReadPackedInt()
            {
                unchecked
                {
                    // read zigzag encoded int
                    int zzint = Read7BitEncodedInt();
                    return ((int)((uint)zzint >> 1) ^ (-(zzint & 1)));
                }
            }

            internal Effect ReadEffect()
            {
                var effect = new Effect(_graphicsDevice);

                effect.ConstantBuffers = ReadConstantBuffers();
                effect._shaders = ReadShaders();
                effect.Parameters = ReadParameters();
                effect.Techniques = ReadTechniques(effect);

                effect.CurrentTechnique = effect.Techniques[0];

                return effect;
            }

            private ConstantBuffer[] ReadConstantBuffers()
            {
                var buffersCount = (int)ReadByte();
                var constantBuffers = new ConstantBuffer[buffersCount];
                for (var c = 0; c < buffersCount; c++)
                    constantBuffers[c] = ReadConstantBuffer();
                return constantBuffers;
            }

            ConstantBuffer ReadConstantBuffer()
            {
                var name = ReadString();

                // Create the backing system memory buffer.
                var sizeInBytes = (int)ReadInt16();

                // Read the parameter index values.
                var parameters = new int[ReadByte()];
                var offsets = new int[parameters.Length];
                for (var i = 0; i < parameters.Length; i++)
                {
                    parameters[i] = (int)ReadByte();
                    offsets[i] = (int)ReadUInt16();
                }

                var buffer = new ConstantBuffer(_graphicsDevice,
                                                name,
                                                parameters,
                                                offsets,
                                                sizeInBytes,
                                                _header.Profile);
                return buffer;
            }

            private Shader[] ReadShaders()
            {
                var shadersCount = (int)ReadByte();
                var shaders = new Shader[shadersCount];
                for (var s = 0; s < shadersCount; s++)
                    shaders[s] = ReadShader();
                return shaders;
            }

            private Shader ReadShader()
            {
                ShaderStage shaderStage = (ShaderStage)ReadByte();

                var shaderLength = ReadInt32();
                var shaderBytecode = ReadBytes(shaderLength);

                var samplerCount = (int)ReadByte();
                var samplers = new SamplerInfo[samplerCount];
                for (var s = 0; s < samplerCount; s++)
                {
                    samplers[s].type = (SamplerType)ReadByte();
                    samplers[s].textureSlot = ReadByte();
                    samplers[s].samplerSlot = ReadByte();

                    if (ReadBoolean())
                        samplers[s].state = ReadSamplerState();

                    samplers[s].GLsamplerName = ReadString();
                    samplers[s].textureParameter = ReadByte();
                }

                var cbufferCount = (int)ReadByte();
                var cBuffers = new int[cbufferCount];
                for (var c = 0; c < cbufferCount; c++)
                    cBuffers[c] = ReadByte();

                var attributeCount = (int)ReadByte();
                var attributes = new VertexAttribute[attributeCount];
                for (var a = 0; a < attributeCount; a++)
                {
                    attributes[a].name = ReadString();
                    attributes[a].usage = (VertexElementUsage)ReadByte();
                    attributes[a].index = ReadByte();
                    attributes[a].location = ReadInt16();
                }

                switch (shaderStage)
                {
                    case ShaderStage.Vertex:
                        return new VertexShader(_graphicsDevice,
                                  shaderBytecode,
                                  samplers, cBuffers, attributes,
                                  _header.Profile);
                    case ShaderStage.Pixel:
                        return new PixelShader(_graphicsDevice,
                                  shaderBytecode,
                                  samplers, cBuffers, attributes,
                                  _header.Profile);

                    default:
                        throw new InvalidOperationException("stage");
                }
            }

            private SamplerState ReadSamplerState()
            {
                var state = new SamplerState();
                state.AddressU = (TextureAddressMode)ReadByte();
                state.AddressV = (TextureAddressMode)ReadByte();
                state.AddressW = (TextureAddressMode)ReadByte();
                state.BorderColor = new Color(
                    ReadByte(),
                    ReadByte(),
                    ReadByte(),
                    ReadByte());
                state.Filter = (TextureFilter)ReadByte();
                state.MaxAnisotropy = ReadInt32();
                state.MaxMipLevel = ReadInt32();
                state.MipMapLevelOfDetailBias = ReadSingle();
                return state;
            }

            private EffectParameterCollection ReadParameters()
            {
                // fallback to version 8
                int count = (_header.Version == 8)
                          ? (int)ReadByte()
                          : Read7BitEncodedInt();

                if (count == 0)
                    return EffectParameterCollection.Empty;

                var parameters = new EffectParameter[count];
                for (var i = 0; i < count; i++)
                    parameters[i] = ReadParameter();

                return new EffectParameterCollection(parameters);
            }

            private EffectParameter ReadParameter()
            {
                var class_ = (EffectParameterClass)ReadByte();
                var type = (EffectParameterType)ReadByte();
                var name = ReadString();
                var semantic = ReadString();
                var annotations = ReadAnnotations();
                var rowCount = (int)ReadByte();
                var columnCount = (int)ReadByte();

                var elements = ReadParameters();
                var structMembers = ReadParameters();

                object data = null;
                if (elements.Count == 0 && structMembers.Count == 0)
                {
                    switch (type)
                    {
                        case EffectParameterType.Bool:
                            if (_header.Profile == ShaderProfileType.OpenGL_Mojo)
                            {
                                // MojoShader stores Booleans in a float type.
                                var buffer = new float[rowCount * columnCount];
                                for (var j = 0; j < buffer.Length; j++)
                                    buffer[j] = ReadInt32();
                                data = buffer;
                            }
                            else
                            {
                                // Booleans are stored in an integer type.
                                var buffer = new int[rowCount * columnCount];
                                for (var j = 0; j < buffer.Length; j++)
                                    buffer[j] = ReadInt32();
                                data = buffer;
                            }
                            break;

                        case EffectParameterType.Int32:
                            if (_header.Profile == ShaderProfileType.OpenGL_Mojo)
                            {
                                // MojoShader stores Integers in a float type.
                                var buffer = new float[rowCount * columnCount];
                                for (var j = 0; j < buffer.Length; j++)
                                    buffer[j] = ReadInt32();
                                data = buffer;
                            }
                            else
                            {
                                var buffer = new int[rowCount * columnCount];
                                for (var j = 0; j < buffer.Length; j++)
                                    buffer[j] = ReadInt32();
                                data = buffer;
                            }
                            break;

                        case EffectParameterType.Single:
                            {
                                var buffer = new float[rowCount * columnCount];
                                for (var j = 0; j < buffer.Length; j++)
                                    buffer[j] = ReadSingle();
                                data = buffer;
                            }
                            break;

                        case EffectParameterType.String:
                            throw new NotImplementedException();

                        default:
                            {
                                Debug.WriteLine("Parameter {0} of type {1} is ignored", name, type.ToString());
                                // throw new NotImplementedException();
                            }
                            break;
                    }
                }

                return new EffectParameter(
                    class_, type, name, rowCount, columnCount, semantic,
                    annotations, elements, structMembers, data, _header.Profile);
            }

            private EffectTechniqueCollection ReadTechniques(Effect effect)
            {
                var techniqueCount = (int)ReadByte();

                var techniques = new EffectTechnique[techniqueCount];
                for (var t = 0; t < techniqueCount; t++)
                {
                    var name = ReadString();
                    var annotations = ReadAnnotations();
                    var passes = ReadPasses(effect);
                    techniques[t] = new EffectTechnique(effect, name, passes, annotations);
                }

                return new EffectTechniqueCollection(techniques);
            }

            private EffectPassCollection ReadPasses(Effect effect)
            {
                var passesCount = (int)ReadByte();
                var passes = new EffectPass[passesCount];
                for (var i = 0; i < passesCount; i++)
                    ReadEffectPass(effect, passes, i);
                return new EffectPassCollection(passes);
            }

            private void ReadEffectPass(Effect effect, EffectPass[] passes, int i)
            {
                var name = ReadString();
                var annotations = ReadAnnotations();

                // Get the vertex and pixel shader.
                VertexShader vertexShader = null;
                PixelShader pixelShader = null;
                {
                    var vertexShaderIndex = (int)ReadByte();
                    var pixelShaderIndex = (int)ReadByte();
                    if (vertexShaderIndex != 255)
                        vertexShader = (VertexShader)effect._shaders[vertexShaderIndex];
                    if (pixelShaderIndex != 255)
                        pixelShader = (PixelShader)effect._shaders[pixelShaderIndex];
                }

                BlendState blend = ReadBoolean() ? ReadBlendState() : null;
                DepthStencilState depth = ReadBoolean() ? ReadDepthStencilState() : null;
                RasterizerState rasterizer = ReadBoolean() ? ReadRasterizerState() : null;

                passes[i] = new EffectPass(effect, name, vertexShader, pixelShader, blend, depth, rasterizer, annotations);
            }

            private BlendState ReadBlendState()
            {
                return new BlendState
                {
                    AlphaBlendFunction = (BlendFunction)ReadByte(),
                    AlphaDestinationBlend = (Blend)ReadByte(),
                    AlphaSourceBlend = (Blend)ReadByte(),
                    BlendFactor = new Color(ReadByte(), ReadByte(), ReadByte(), ReadByte()),
                    ColorBlendFunction = (BlendFunction)ReadByte(),
                    ColorDestinationBlend = (Blend)ReadByte(),
                    ColorSourceBlend = (Blend)ReadByte(),
                    ColorWriteChannels = (ColorWriteChannels)ReadByte(),
                    ColorWriteChannels1 = (ColorWriteChannels)ReadByte(),
                    ColorWriteChannels2 = (ColorWriteChannels)ReadByte(),
                    ColorWriteChannels3 = (ColorWriteChannels)ReadByte(),
                    MultiSampleMask = ReadInt32(),
                };
            }

            private DepthStencilState ReadDepthStencilState()
            {
                return new DepthStencilState
                {
                    CounterClockwiseStencilDepthBufferFail = (StencilOperation)ReadByte(),
                    CounterClockwiseStencilFail = (StencilOperation)ReadByte(),
                    CounterClockwiseStencilFunction = (CompareFunction)ReadByte(),
                    CounterClockwiseStencilPass = (StencilOperation)ReadByte(),
                    DepthBufferEnable = ReadBoolean(),
                    DepthBufferFunction = (CompareFunction)ReadByte(),
                    DepthBufferWriteEnable = ReadBoolean(),
                    ReferenceStencil = ReadInt32(),
                    StencilDepthBufferFail = (StencilOperation)ReadByte(),
                    StencilEnable = ReadBoolean(),
                    StencilFail = (StencilOperation)ReadByte(),
                    StencilFunction = (CompareFunction)ReadByte(),
                    StencilMask = ReadInt32(),
                    StencilPass = (StencilOperation)ReadByte(),
                    StencilWriteMask = ReadInt32(),
                    TwoSidedStencilMode = ReadBoolean(),
                };
            }

            private RasterizerState ReadRasterizerState()
            {
                return new RasterizerState
                {
                    CullMode = (CullMode)ReadByte(),
                    DepthBias = ReadSingle(),
                    FillMode = (FillMode)ReadByte(),
                    MultiSampleAntiAlias = ReadBoolean(),
                    ScissorTestEnable = ReadBoolean(),
                    SlopeScaleDepthBias = ReadSingle(),
                };
            }

            private EffectAnnotationCollection ReadAnnotations()
            {
                var count = (int)ReadByte();
                if (count == 0)
                    return EffectAnnotationCollection.Empty;

                var annotations = new EffectAnnotation[count];

                // TODO: Annotations are not implemented!

                return new EffectAnnotationCollection(annotations);
            }

        }
    }
}