// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using nkast.Wasm.XHR;


namespace Microsoft.Xna.Framework
{
    partial class TitleContainer
    {
        private static Stream PlatformOpenStream(string safeName)
        {
            var request = new XMLHttpRequest();

            request.Open("GET", safeName, false);
            request.OverrideMimeType("text/plain; charset=x-user-defined");
            request.Send();

            if (request.Status == 200)
            {
                var responseText = request.ResponseText;

                byte[] buffer = new byte[responseText.Length];
                for (int i = 0; i < responseText.Length; i++)
                    buffer[i] = (byte)(responseText[i] & 0xff);

                var ms = new MemoryStream(buffer);

                return ms;
            }
            else
            {
                throw new IOException("HTTP request failed. Status:" + request.Status);
            }
        }
    }
}
