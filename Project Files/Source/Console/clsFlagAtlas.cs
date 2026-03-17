/*  FlagAtlas.cs

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2020-2026 Richard Samphire MW0LGE

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

The author can be reached by email at

mw0lge@grange-lane.co.uk
*/
//
//============================================================================================//
// Dual-Licensing Statement (Applies Only to Author's Contributions, Richard Samphire MW0LGE) //
// ------------------------------------------------------------------------------------------ //
// For any code originally written by Richard Samphire MW0LGE, or for any modifications       //
// made by him, the copyright holder for those portions (Richard Samphire) reserves the       //
// right to use, license, and distribute such code under different terms, including           //
// closed-source and proprietary licences, in addition to the GNU General Public License      //
// granted above. Nothing in this statement restricts any rights granted to recipients under  //
// the GNU GPL. Code contributed by others (not Richard Samphire) remains licensed under      //
// its original terms and is not affected by this dual-licensing statement in any way.        //
// Richard Samphire can be reached by email at :  mw0lge@grange-lane.co.uk                    //
//============================================================================================//

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json;

namespace Thetis
{
    public static class FlagAtlas
    {
        private static readonly object _sync = new object();
        private static Bitmap _atlas_image;
        private static Dictionary<string, Rectangle> _sprite_lookup = new Dictionary<string, Rectangle>(StringComparer.OrdinalIgnoreCase);

        public static void Init(Image atlas_image, string json)
        {
            if (atlas_image == null)
                throw new ArgumentNullException("atlas_image");

            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON is null or empty.", "json");

            AtlasDefinition atlas_definition = JsonConvert.DeserializeObject<AtlasDefinition>(json);

            if (atlas_definition == null)
                throw new InvalidOperationException("Failed to parse atlas JSON.");

            if (atlas_definition.Sprites == null || atlas_definition.Sprites.Count == 0)
                throw new InvalidOperationException("Atlas JSON contains no sprites.");

            Dictionary<string, Rectangle> new_lookup = new Dictionary<string, Rectangle>(atlas_definition.Sprites.Count, StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, SpriteDefinition> pair in atlas_definition.Sprites)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                    continue;

                if (pair.Value == null)
                    throw new InvalidOperationException("Sprite entry is null for '" + pair.Key + "'.");

                Rectangle rect = new Rectangle(pair.Value.X, pair.Value.Y, pair.Value.Width, pair.Value.Height);

                if (rect.Width <= 0 || rect.Height <= 0)
                    throw new InvalidOperationException("Sprite '" + pair.Key + "' has an invalid size.");

                if (rect.X < 0 || rect.Y < 0 || rect.Right > atlas_image.Width || rect.Bottom > atlas_image.Height)
                    throw new InvalidOperationException("Sprite '" + pair.Key + "' is outside the atlas image bounds.");

                new_lookup[pair.Key] = rect;
            }

            Bitmap new_atlas_image = new Bitmap(atlas_image);

            lock (_sync)
            {
                if (_atlas_image != null)
                    _atlas_image.Dispose();

                _atlas_image = new_atlas_image;
                _sprite_lookup = new_lookup;
            }
        }

        public static Bitmap GetFlag(string flag_name)
        {
            if (string.IsNullOrWhiteSpace(flag_name))
                throw new ArgumentException("Flag name is null or empty.", "flag_name");

            flag_name = flag_name.Trim();

            if (!flag_name.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                flag_name += ".png";

            lock (_sync)
            {
                ensureInitialised();

                Rectangle rect;
                if (!_sprite_lookup.TryGetValue(flag_name, out rect))
                    throw new KeyNotFoundException("Flag not found: " + flag_name);

                return _atlas_image.Clone(rect, PixelFormat.Format32bppArgb);
            }
        }

        public static bool ContainsFlag(string flag_name)
        {
            if (string.IsNullOrWhiteSpace(flag_name))
                return false;

            lock (_sync)
            {
                if (_atlas_image == null)
                    return false;

                return _sprite_lookup.ContainsKey(flag_name);
            }
        }

        public static Rectangle GetFlagBounds(string flag_name)
        {
            if (string.IsNullOrWhiteSpace(flag_name))
                throw new ArgumentException("Flag name is null or empty.", "flag_name");

            lock (_sync)
            {
                ensureInitialised();

                Rectangle rect;
                if (!_sprite_lookup.TryGetValue(flag_name, out rect))
                    throw new KeyNotFoundException("Flag not found: " + flag_name);

                return rect;
            }
        }

        public static void Clear()
        {
            lock (_sync)
            {
                if (_atlas_image != null)
                {
                    _atlas_image.Dispose();
                    _atlas_image = null;
                }

                _sprite_lookup = new Dictionary<string, Rectangle>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static void ensureInitialised()
        {
            if (_atlas_image == null || _sprite_lookup == null || _sprite_lookup.Count == 0)
                throw new InvalidOperationException("clsFlagAtlas has not been initialised.");
        }

        private sealed class AtlasDefinition
        {
            [JsonProperty("sprites")]
            public Dictionary<string, SpriteDefinition> Sprites { get; set; }
        }

        private sealed class SpriteDefinition
        {
            [JsonProperty("x")]
            public int X { get; set; }

            [JsonProperty("y")]
            public int Y { get; set; }

            [JsonProperty("width")]
            public int Width { get; set; }

            [JsonProperty("height")]
            public int Height { get; set; }
        }
    }
}
