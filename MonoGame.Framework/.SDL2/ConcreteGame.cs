// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.Framework.Utilities;


namespace Microsoft.Xna.Platform
{
    sealed class ConcreteGame : GameStrategy
    {
        private SdlGameWindow _gameWindow;

        private Sdl SDL { get { return Sdl.Current; } }

        protected override void RunGameLoop()
        {
            SDL.WINDOW.Show(Window.Handle);

            while (true)
            {
                bool isExiting = _gameWindow.SdlRunLoop();
                _isExiting |= isExiting;

                if (!_isExiting)
                    Game.Tick();
                else
                    break;
            }
        }

        private bool _isExiting;

        public ConcreteGame(Game game) : base(game)
        {
            var minVersion = new Sdl.Version(2,0,5);

            if (SDL.version < minVersion)
                Debug.WriteLine("Please use SDL " + minVersion + " or higher.");

            // Needed so VS can debug the project on Windows
            if (SDL.version >= minVersion && CurrentPlatform.OS == OS.Windows && Debugger.IsAttached)
                SDL.SetHint("SDL_WINDOWS_DISABLE_THREAD_NAMING", "1");

            SDL.Init((int)(
                Sdl.InitFlags.Video |
                Sdl.InitFlags.Joystick |
                Sdl.InitFlags.GameController |
                Sdl.InitFlags.Haptic
            ));

            SDL.DisableScreenSaver();

            _gameWindow = new SdlGameWindow(Game);
            base.Window = _gameWindow;
            if (Mouse.WindowHandle == IntPtr.Zero)
                Mouse.WindowHandle = base.Window.Handle;
            if (TouchPanel.WindowHandle == IntPtr.Zero)
                TouchPanel.WindowHandle = base.Window.Handle;
        }

        public override void Initialize()
        {
            // TODO: This should be moved to GraphicsDeviceManager or GraphicsDevice
            {
                GraphicsDevice graphicsDevice = this.GraphicsDevice;
                PresentationParameters pp = graphicsDevice.PresentationParameters;
                graphicsDevice.Viewport = new Viewport(0, 0, pp.BackBufferWidth, pp.BackBufferHeight);

                _gameWindow.EndScreenDeviceChange(string.Empty, pp.BackBufferWidth, pp.BackBufferHeight, pp.IsFullScreen);
            }

            base.Initialize();
        }

        public override bool IsMouseVisible
        {
            get { return base.IsMouseVisible; }
            set
            {
                if (base.IsMouseVisible != value)
                {
                    base.IsMouseVisible = value;
                    _gameWindow.SetCursorVisible(Game.IsMouseVisible);
                }
            }
        }

        internal override void OnPresentationChanged(PresentationParameters pp)
        {
            int displayIndex = SDL.WINDOW.GetDisplayIndex(Window.Handle);
            string displayName = SDL.DISPLAY.GetDisplayName(displayIndex);
            _gameWindow.EndScreenDeviceChange(displayName, pp.BackBufferWidth, pp.BackBufferHeight, pp.IsFullScreen);
        }

        public override void TickExiting()
        {
            _isExiting = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (_gameWindow != null)
            {
                if (Mouse.WindowHandle == _gameWindow.Handle)
                    Mouse.WindowHandle = IntPtr.Zero;
                if (TouchPanel.WindowHandle == _gameWindow.Handle)
                    TouchPanel.WindowHandle = IntPtr.Zero;
            }

            if (_gameWindow != null)
            {
                _gameWindow.Dispose();
                _gameWindow = null;

                SDL.Quit();
            }

            base.Dispose(disposing);
        }
    }
}
