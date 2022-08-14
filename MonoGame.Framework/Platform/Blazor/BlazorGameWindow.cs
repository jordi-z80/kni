﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Size = System.Drawing.Size;
using nkast.Wasm.Canvas;
using nkast.Wasm.Dom;

namespace MonoGame.Framework
{
    // TODO: BlazorGameWindow should be internal
    public class BlazorGameWindow : GameWindow, IDisposable
    {
        private static Dictionary<IntPtr, BlazorGameWindow> _instances = new Dictionary<IntPtr, BlazorGameWindow>();

        internal static BlazorGameWindow FromHandle(IntPtr handle)
        {
            return _instances[handle];
        }

        private Window _window;
        private BlazorGamePlatform _platform;

        private bool _isResizable;
        private bool _isBorderless;
        private bool _isMouseHidden;
        private bool _isMouseInBounds;

        private readonly List<Keys> _keys = new List<Keys>();

        private Point _locationBeforeFullScreen;

        // flag to indicate that we're switching to/from full screen and should ignore resize events
        private bool _switchingFullScreen;




        #region Internal Properties


        #endregion

        #region Public Properties

        public override IntPtr Handle { get { return new IntPtr(_window.Uid); } }

        public override string ScreenDeviceName { get { return String.Empty; } }

        public override Rectangle ClientBounds
        {
            get
            {
                return new Rectangle(0, 0, _canvas.Width, _canvas.Height);
            }
        }

        public override bool AllowUserResizing
        {
            get { return _isResizable; }
            set
            {
                if (_isResizable != value)
                {
                    _isResizable = value;
                }
                else
                    return;
                if (_isBorderless)
                    return;
            }
        }

        public override bool AllowAltF4
        {
             get { return base.AllowAltF4; }
             set
             {
                 base.AllowAltF4 = value;
             }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get { return DisplayOrientation.Default; }
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
        }

        public override bool IsBorderless
        {
            get { return _isBorderless; }
            set
            {
                if (_isBorderless != value)
                    _isBorderless = value;
                else
                    return;
            }
        }

        public bool IsFullScreen { get; private set; }
        public bool HardwareModeSwitch { get; private set; }

        #endregion

        internal Canvas _canvas { get; private set; }
        internal Window wasmWindow { get { return _window; } }

        internal BlazorGameWindow(BlazorGamePlatform platform)
        {
            _platform = platform;

            _window = Window.Current;
            _canvas = _window.Document.GetElementById<Canvas>("theCanvas");
            _instances.Add(this.Handle, this);

            ChangeClientSize(new Size(GraphicsDeviceManager.DefaultBackBufferWidth, GraphicsDeviceManager.DefaultBackBufferHeight));

            SetIcon();

            // Capture mouse events.
            Mouse.WindowHandle = new IntPtr(_window.Uid);
            //Form.MouseEnter += OnMouseEnter;
            //Form.MouseLeave += OnMouseLeave;

            // Capture touch events.
            TouchPanel.PrimaryWindow = this;
            TouchPanel.WindowHandle = new IntPtr(_window.Uid);
            _window.OnTouchStart += (object sender, float x, float y, int identifier) =>
            {
                TouchPanel.AddEvent(identifier, TouchLocationState.Pressed, new Vector2(x, y));
            };
            _window.OnTouchMove += (object sender, float x, float y, int identifier) =>
            {
                TouchPanel.AddEvent(identifier, TouchLocationState.Moved, new Vector2(x, y));
            };
            _window.OnTouchEnd += (object sender, float x, float y, int identifier) =>
            {
                TouchPanel.AddEvent(identifier, TouchLocationState.Released, new Vector2(x, y));
            };
            _window.OnTouchCancel += (object sender) =>
            {
                TouchPanel.PrimaryWindow.TouchPanelState.ReleaseAllTouches();
            };

            // keyboard events
            Keyboard.SetKeys(_keys);
            _window.OnKeyDown += (object sender, char key, int keyCode) =>
            {
                var xnakey = (Keys)keyCode;
                if (!_keys.Contains(xnakey))
                    _keys.Add(xnakey);
            };
            _window.OnKeyUp += (object sender, char key, int keyCode) =>
            {
                var xnakey = (Keys)keyCode;
                if (_keys.Contains(xnakey))
                    _keys.Remove(xnakey);
            };

            //Form.Activated += OnActivated;
            // Form.Deactivate += OnDeactivate;
            // Form.Resize += OnResize;
            //  Form.ResizeBegin += OnResizeBegin;
            //  Form.ResizeEnd += OnResizeEnd;
            _window.OnResize += OnResize;

           // Form.KeyPress += OnKeyPress;
        }

        private void SetIcon()
        {
          
        }

        ~BlazorGameWindow()
        {
            Dispose(false);
        }

        private void OnActivated(object sender, EventArgs eventArgs)
        {
            _platform.IsActive = true;
            //Keyboard.SetActive(true);
        }

        private void OnDeactivate(object sender, EventArgs eventArgs)
        {
        
        }


        private void OnMouseEnter(object sender, EventArgs e)
        {
            _isMouseInBounds = true;
            if (!_platform.IsMouseVisible && !_isMouseHidden)
            {
                _isMouseHidden = true;
                //Cursor.Hide();
            }
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            _isMouseInBounds = false;
            if (_isMouseHidden)
            {
                _isMouseHidden = false;
                //Cursor.Show();
            }
        }

        internal void Initialize(int width, int height)
        {
            ChangeClientSize(new Size(width, height));
        }

        internal void Initialize(PresentationParameters pp)
        {
            ChangeClientSize(new Size(pp.BackBufferWidth, pp.BackBufferHeight));

            if (pp.IsFullScreen)
            {
                EnterFullScreen(pp);
                if (!pp.HardwareModeSwitch)
                    _platform.Game.GraphicsDevice.OnPresentationChanged();
            }
        }

        internal void OnResize(object sender)
        {

            UpdateBackBufferSize();

            OnClientSizeChanged();
        }

        // TODO: move UpdateBackBufferSize() in graphicsDeviceManager
        private void UpdateBackBufferSize()
        {
            var game = _platform.Game;

            var manager = game.graphicsDeviceManager;
            if (manager.GraphicsDevice == null)
                return;

            _canvas.Width = _window.InnerWidth;
            _canvas.Height = _window.InnerHeight;
            Size newSize = new Size(_canvas.Width, _canvas.Height);
            if(newSize.Width == manager.PreferredBackBufferWidth &&
               newSize.Height == manager.PreferredBackBufferHeight)
                return;

            // Set the default new back buffer size
            manager.PreferredBackBufferWidth = newSize.Width;
            manager.PreferredBackBufferHeight = newSize.Height;
            manager.ApplyChanges();
        }

        protected override void SetTitle(string title)
        {
            _window.Document.Title = title;
        }

        internal void RunLoop()
        {
            //Application.Idle += TickOnIdle;
            //Application.Run(Form);
            //Application.Idle -= TickOnIdle;
        }

        // Run game loop when the app becomes Idle.
        private void TickOnIdle(object sender, EventArgs e)
        {
        }


        internal void ChangeClientSize(Size clientBounds)
        {

        }

        #region Public Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
               
            }

            _instances.Remove(this.Handle);
            _canvas = null;

            _platform = null;
            Mouse.WindowHandle = IntPtr.Zero;
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
        }

        public void MouseVisibleToggled()
        {
            if (_platform.IsMouseVisible)
            {
                if (_isMouseHidden)
                {
                    //Cursor.Show();
                    _isMouseHidden = false;
                }
            }
            else if (!_isMouseHidden && _isMouseInBounds)
            {
                //Cursor.Hide();
                _isMouseHidden = true;
            }
        }

        internal void OnPresentationChanged(PresentationParameters pp)
        {
            var raiseClientSizeChanged = false;
            if (pp.IsFullScreen && pp.HardwareModeSwitch && IsFullScreen && HardwareModeSwitch)
            {
                if( _platform.IsActive ) {
                    // stay in hardware full screen, need to call ResizeTargets so the displaymode can be switched
                   // _platform.Game.GraphicsDevice.ResizeTargets();
                } else {
                    // This needs to be called in case the user presses the Windows key while the focus is on the second monitor,
                    //	which (sometimes) causes the window to exit fullscreen mode, but still keeps it visible
                    MinimizeFullScreen();
                }
            }
            else if (pp.IsFullScreen && (!IsFullScreen || pp.HardwareModeSwitch != HardwareModeSwitch))
            {
                EnterFullScreen(pp);
                raiseClientSizeChanged = true;
            }
            else if (!pp.IsFullScreen && IsFullScreen)
            {
                ExitFullScreen();
                raiseClientSizeChanged = true;
            }

            ChangeClientSize(new Size(pp.BackBufferWidth, pp.BackBufferHeight));

            if (raiseClientSizeChanged)
                OnClientSizeChanged();
        }

        #endregion

        private void EnterFullScreen(PresentationParameters pp)
        {

        }


        private void ExitFullScreen()
        {
           
        }

        private void MinimizeFullScreen()
        {
         
        }
    }
}
