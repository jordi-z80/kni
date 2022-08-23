// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework
{
    partial class GamePlatform
    {
        internal static GamePlatform PlatformCreate(Game game)
        {
#if IOS
            return new iOSGamePlatform(game);
#elif ANDROID
            return new AndroidGamePlatform(game);
#endif
        }
    }
}
