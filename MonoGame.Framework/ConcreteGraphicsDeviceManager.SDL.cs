﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Platform
{
    internal class ConcreteGraphicsDeviceManager : GraphicsDeviceManagerStrategy
    {
        internal bool _initialized = false;


        public ConcreteGraphicsDeviceManager(Game game) : base(game)
        {
            var clientBounds = base.Game.Window.ClientBounds;
            base.PreferredBackBufferWidth = clientBounds.Width;
            base.PreferredBackBufferHeight = clientBounds.Height;

        }

        public override bool PreferHalfPixelOffset
        {
            get { return base.PreferHalfPixelOffset; }
            set
            {
                //TODO: move the check in ApplyChanges
                if (base.GraphicsDevice != null)
                    throw new InvalidOperationException("Setting PreferHalfPixelOffset is not allowed after the creation of GraphicsDevice.");

                base.PreferHalfPixelOffset = value;
            }
        }

        public override bool IsFullScreen
        {
            get { return base.IsFullScreen; }
            set { base.IsFullScreen = value; }
        }

        public override DisplayOrientation SupportedOrientations
        {
            get { return base.SupportedOrientations; }
            set { base.SupportedOrientations = value; }
        }

        public override void ToggleFullScreen()
        {
            //base.ApplyChanges();

            base.IsFullScreen = !base.IsFullScreen;
            ApplyChanges();
        }

        public override void ApplyChanges()
        {
            //base.ApplyChanges();

            if (this.GraphicsDevice == null)
            {
                this.CreateDevice();
            }

            this.Game.Window.SetSupportedOrientations(this.SupportedOrientations);

            // populates a gdi with settings in this gdm and allows users to override them with
            // PrepareDeviceSettings event this information should be applied to the GraphicsDevice
            var gdi = this.DoPreparingDeviceSettings();

            if (gdi.GraphicsProfile != GraphicsDevice.GraphicsProfile)
            {
                // if the GraphicsProfile changed we need to create a new GraphicsDevice
                this.GraphicsDevice.Dispose();
                this.GraphicsDevice = null;

                ((ConcreteGraphicsDeviceManager)this).CreateDevice(gdi);
            }
            else
            {
                GraphicsDevice.Reset(gdi.PresentationParameters);
            }
        }

        /// <summary>
        /// This populates a GraphicsDeviceInformation instance and invokes PreparingDeviceSettings to
        /// allow users to change the settings. Then returns that GraphicsDeviceInformation.
        /// Throws NullReferenceException if users set GraphicsDeviceInformation.PresentationParameters to null.
        /// </summary>
        internal GraphicsDeviceInformation DoPreparingDeviceSettings()
        {
            var gdi = new GraphicsDeviceInformation();
            gdi.Adapter = GraphicsAdapter.DefaultAdapter;
            gdi.GraphicsProfile = GraphicsProfile;

            PresentationParameters presentationParameters = this.PreparePresentationParameters();

            gdi.PresentationParameters = presentationParameters;
            var args = new PreparingDeviceSettingsEventArgs(gdi);
            this.OnPreparingDeviceSettings(args);

            if (gdi.PresentationParameters == null || gdi.Adapter == null)
                throw new NullReferenceException("Members should not be set to null in PreparingDeviceSettingsEventArgs");

            return gdi;
        }

        private PresentationParameters PreparePresentationParameters()
        {
            var presentationParameters = new PresentationParameters();
            presentationParameters.BackBufferFormat = this.PreferredBackBufferFormat;
            presentationParameters.BackBufferWidth  = this.PreferredBackBufferWidth;
            presentationParameters.BackBufferHeight   = this.PreferredBackBufferHeight;
            presentationParameters.DepthStencilFormat = this.PreferredDepthStencilFormat;
            presentationParameters.IsFullScreen = this.IsFullScreen;
            presentationParameters.HardwareModeSwitch = this.HardwareModeSwitch;
            presentationParameters.PresentationInterval = this.SynchronizeWithVerticalRetrace ? PresentInterval.One : PresentInterval.Immediate;
            presentationParameters.DisplayOrientation = this.Game.Window.CurrentOrientation;
            presentationParameters.DeviceWindowHandle = this.Game.Window.Handle;

            // always initialize MultiSampleCount to the maximum, if users want to overwrite
            // this they have to respond to the PreparingDeviceSettingsEvent and modify
            // args.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount
            if (this.PreferMultiSampling)
                presentationParameters.MultiSampleCount = (GraphicsDevice != null) ? GraphicsDevice.GraphicsCapabilities.MaxMultiSampleCount : 32;
            else
                presentationParameters.MultiSampleCount = 0;

            return presentationParameters;
        }

        private void PlatformInitialize(PresentationParameters presentationParameters)
        {
            var surfaceFormat = base.Game.graphicsDeviceManager.PreferredBackBufferFormat.GetColorFormat();
            var depthStencilFormat = base.Game.graphicsDeviceManager.PreferredDepthStencilFormat;

            // TODO Need to get this data from the Presentation Parameters
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.RedSize, surfaceFormat.R);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.GreenSize, surfaceFormat.G);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.BlueSize, surfaceFormat.B);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.AlphaSize, surfaceFormat.A);

            switch (depthStencilFormat)
            {
                case DepthFormat.None:
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.DepthSize, 0);
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.StencilSize, 0);
                    break;
                case DepthFormat.Depth16:
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.DepthSize, 16);
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.StencilSize, 0);
                    break;
                case DepthFormat.Depth24:
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.DepthSize, 24);
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.StencilSize, 0);
                    break;
                case DepthFormat.Depth24Stencil8:
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.DepthSize, 24);
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.StencilSize, 8);
                    break;
            }

            Sdl.GL.SetAttribute(Sdl.GL.Attribute.DoubleBuffer, 1);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.ContextMajorVersion, 2);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.ContextMinorVersion, 1);

            if (presentationParameters.MultiSampleCount > 0)
            {
                Sdl.GL.SetAttribute(Sdl.GL.Attribute.MultiSampleBuffers, 1);
                Sdl.GL.SetAttribute(Sdl.GL.Attribute.MultiSampleSamples, presentationParameters.MultiSampleCount);
            }

            ((SdlGameWindow)SdlGameWindow.Instance).CreateWindow();
        }

        public override void CreateDevice()
        {
            //base.CreateDevice();

            if (this.GraphicsDevice != null)
                return;

            var gdi = this.DoPreparingDeviceSettings();

            if (!this._initialized)
            {
                this.Game.Window.SetSupportedOrientations(this.SupportedOrientations);

                this.PlatformInitialize(gdi.PresentationParameters);

                this._initialized = true;
            }

            this.CreateDevice(gdi);
        }

        internal void CreateDevice(GraphicsDeviceInformation gdi)
        {
            this.GraphicsDevice = new GraphicsDevice(gdi.Adapter, gdi.GraphicsProfile, this.PreferHalfPixelOffset, gdi.PresentationParameters);

            // update the touchpanel display size when the graphicsdevice is reset
            this.GraphicsDevice.DeviceReset += GraphicsDevice_DeviceReset_UpdateTouchPanel;
            this.GraphicsDevice.PresentationChanged += this.GraphicsDevice_PresentationChanged_UpdateGamePlatform;

            this.OnDeviceCreated(EventArgs.Empty);
        }

        private void GraphicsDevice_DeviceReset_UpdateTouchPanel(object sender, EventArgs eventArgs)
        {
            TouchPanel.DisplayWidth = this.GraphicsDevice.PresentationParameters.BackBufferWidth;
            TouchPanel.DisplayHeight = this.GraphicsDevice.PresentationParameters.BackBufferHeight;
            TouchPanel.DisplayOrientation = this.GraphicsDevice.PresentationParameters.DisplayOrientation;
        }

        private void GraphicsDevice_PresentationChanged_UpdateGamePlatform(object sender, PresentationEventArgs args)
        {
            base.Game.Platform.OnPresentationChanged(args.PresentationParameters);
        }

    }
}