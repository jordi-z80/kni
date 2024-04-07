// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;


namespace Microsoft.Xna.Platform
{
    sealed class ConcreteGame : GameStrategy
    {
        //internal static string LaunchParameters;

        private BlazorGameWindow _gameWindow;

        public ConcreteGame(Game game) : base(game)
        {
            _gameWindow = new BlazorGameWindow(this);
            base.Window = _gameWindow;
            if (TouchPanel.WindowHandle == IntPtr.Zero)
                TouchPanel.WindowHandle = base.Window.Handle;
        }

        internal override void Run()
        {
            if (!_initialized)
            {
                Game.DoInitialize();
                _initialized = true;
            }

            Game.DoBeginRun();
            Timer = Stopwatch.StartNew();
            // XNA runs one Update even before showing the window
            Game.DoUpdate(new GameTime());
            IsActive = _gameWindow.wasmWindow.Document.HasFocus();

            _gameWindow.RunLoop();

            //Game.DoEndRun();
            //Game.DoExiting();
        }

        public override void Tick()
        {
            base.Tick();
        }

        public override bool IsMouseVisible
        {
            get { return base.IsMouseVisible; }
            set
            {
                if (base.IsMouseVisible != value)
                {
                    base.IsMouseVisible = value;
                    _gameWindow.MouseVisibleToggled();
                }
            }
        }

        public override void BeforeInitialize()
        {
            var gdm = this.GraphicsDeviceManager;
            if (gdm == null)
            {
                _gameWindow.Initialize(GraphicsDeviceManager.DefaultBackBufferWidth, GraphicsDeviceManager.DefaultBackBufferHeight);
            }
            else
            {
                var pp = this.GraphicsDevice.PresentationParameters;
                _gameWindow.Initialize(pp);
            }
        }

        public override void Exit()
        {
            throw new PlatformNotSupportedException("BlazorGL platform does not allow programmatically closing.");
        }
        
        public override void TickExiting()
        {
            // BlazorGL games do not "exit" or shut down.
            throw new PlatformNotSupportedException();
        }

        public override void Android_BeforeUpdate()
        {
        }

        internal override void OnPresentationChanged(PresentationParameters pp)
        {
            _gameWindow.OnPresentationChanged(pp);
        }
        
        protected override void Dispose(bool disposing)
        {
            if (_gameWindow != null)
            {
                if (TouchPanel.WindowHandle == _gameWindow.Handle)
                    TouchPanel.WindowHandle = IntPtr.Zero;
            }

            if (disposing)
            {
                if (_gameWindow != null)
                {
                    _gameWindow.Dispose();
                    _gameWindow = null;
                    Window = null;
                }
            }

            base.Dispose(disposing);

        }
    }
}