// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;

namespace SixLabors.ImageSharp.Processing.Dithering.Processors
{
    /// <summary>
    /// The base class for dither and diffusion processors that consume a palette.
    /// </summary>
    internal abstract class PaletteDitherProcessorBase<TPixel> : ImageProcessor<TPixel>
        where TPixel : struct, IPixel<TPixel>
    {
        private readonly Dictionary<TPixel, TPixel> cache = new Dictionary<TPixel, TPixel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PaletteDitherProcessorBase{TPixel}"/> class.
        /// </summary>
        /// <param name="palette">The palette to select substitute colors from.</param>
        protected PaletteDitherProcessorBase(TPixel[] palette)
        {
            Guard.NotNull(palette, nameof(palette));
            this.Palette = palette;
        }

        /// <summary>
        /// Gets the palette to select substitute colors from.
        /// </summary>
        public TPixel[] Palette { get; }

        /// <summary>
        /// Returns the closest color from the palette to the given color by calculating the Euclidean distance.
        /// </summary>
        /// <param name="pixel">The color.</param>
        /// <param name="colorPalette">The color palette.</param>
        /// <returns>The closest pixel in the palette.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TPixel GetClosestPixel(TPixel pixel, TPixel[] colorPalette)
        {
            // Check if the color is in the lookup table
            if (this.cache.TryGetValue(pixel, out var result))
            {
                return result;
            }

            // Not found - loop through the palette and find the nearest match.
            TPixel color = default;
            float leastDistance = int.MaxValue;
            var vector = pixel.ToVector4();

            for (int index = 0; index < colorPalette.Length; index++)
            {
                TPixel tmp = colorPalette[index];
                float distance = Vector4.Distance(vector, tmp.ToVector4());

                // Greater... Move on.
                if (distance >= leastDistance)
                {
                    continue;
                }

                color = tmp;
                leastDistance = distance;

                // And if it's an exact match, exit the loop
                if (MathF.Abs(distance) < Constants.Epsilon)
                {
                    break;
                }
            }

            this.cache.Add(pixel, color);

            return color;
        }
    }
}