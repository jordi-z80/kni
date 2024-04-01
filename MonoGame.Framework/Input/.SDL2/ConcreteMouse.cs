﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021-2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteMouse : MouseStrategy
    {
        internal GameWindow PrimaryWindow;

        private Sdl SDL { get { return Sdl.Current; } }

        internal int ScrollX;
        internal int ScrollY;

        public override IntPtr PlatformGetWindowHandle()
        {
            return PrimaryWindow.Handle;
        }

        public override void PlatformSetWindowHandle(IntPtr windowHandle)
        {
        }

        public override bool PlatformIsRawInputAvailable()
        {
            return true;
        }

        public override MouseState PlatformGetState()
        {
            throw new NotImplementedException();
        }

        public MouseState PlatformGetState(GameWindow window)
        {
            int x, y;
            var winFlags = SDL.WINDOW.GetWindowFlags(window.Handle);
            var state = SDL.MOUSE.GetGlobalState(out x, out y);
            var clientBounds = window.ClientBounds;

            window.MouseState.LeftButton = (state & Sdl.Mouse.Button.Left) != 0 ? ButtonState.Pressed : ButtonState.Released;
            window.MouseState.MiddleButton = (state & Sdl.Mouse.Button.Middle) != 0 ? ButtonState.Pressed : ButtonState.Released;
            window.MouseState.RightButton = (state & Sdl.Mouse.Button.Right) != 0 ? ButtonState.Pressed : ButtonState.Released;
            window.MouseState.XButton1 = (state & Sdl.Mouse.Button.X1Mask) != 0 ? ButtonState.Pressed : ButtonState.Released;
            window.MouseState.XButton2 = (state & Sdl.Mouse.Button.X2Mask) != 0 ? ButtonState.Pressed : ButtonState.Released;

            window.MouseState.HorizontalScrollWheelValue = ScrollX;
            window.MouseState.ScrollWheelValue = ScrollY;

            window.MouseState.X = x - clientBounds.X;
            window.MouseState.Y = y - clientBounds.Y;

            return window.MouseState;
        }

        public override void PlatformSetPosition(int x, int y)
        {
            PrimaryWindow.MouseState.X = x;
            PrimaryWindow.MouseState.Y = y;

            SDL.MOUSE.WarpInWindow(PrimaryWindow.Handle, x, y);
        }

        public override void PlatformSetCursor(MouseCursor cursor)
        {
            SDL.MOUSE.SetCursor(cursor.Handle);
        }

    }
}