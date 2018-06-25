// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Tests;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Dithering;
using SixLabors.ImageSharp.Processing.Quantization;
using SixLabors.ImageSharp.Advanced;

namespace SixLabors.ImageSharp.Benchmarks.Processing
{
    public class Dither : BenchmarkBase
    {
        private Image<Rgba32> image;
        private Image<Rgba32> quantizedImage;
        private Rgba32[] halfPalette;
        private Rgba32[] palette;

        [GlobalSetup]
        public void LoadImages()
        {
            if (this.image == null)
            {
                var path = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, TestImages.Jpeg.Baseline.Lake);
                this.image= Image.Load<Rgba32>(path);

                var largerPaletteFrameQuantizer = new OctreeQuantizer(KnownDiffusers.FloydSteinberg, 128).CreateFrameQuantizer<Rgba32>();
                this.halfPalette = largerPaletteFrameQuantizer.QuantizeFrame(this.image.Frames[0]).Palette;

                var frameQuantizer = new PaletteQuantizer(false).CreateFrameQuantizer<Rgba32>();
                var quantizedFrame = frameQuantizer.QuantizeFrame(this.image.Frames[0]);
                this.quantizedImage = this.QuantizedFrameToImage(quantizedFrame);
                this.palette = quantizedFrame.Palette;
            }
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            this.image.Dispose();
            this.quantizedImage.Dispose();
        }

        [Benchmark(Description = "Diffuse Floyd-Steinberg")]
        public void DitherFloydSteinberg()
        {
            this.image.Clone(c => c.Diffuse(KnownDiffusers.FloydSteinberg, .5f));
        }

        [Benchmark(Description = "Diffuse Floyd-Steinberg Half Palette")]
        public void DitherFloydSteinbergHalfPalette()
        {
            this.quantizedImage.Clone(c => c.Diffuse(KnownDiffusers.FloydSteinberg, .5f, this.halfPalette));
        }

        [Benchmark(Description = "Diffuse Floyd-Steinberg Full Palette")]
        public void DitherFloydSteinbergFullPalette()
        {
            this.quantizedImage.Clone(c => c.Diffuse(KnownDiffusers.FloydSteinberg, .5f, this.palette));
        }

        private Image<Rgba32> QuantizedFrameToImage(QuantizedFrame<Rgba32> frame)
        {
            var img = new Image<Rgba32>(frame.Width, frame.Height);
            int paletteCount = frame.Palette.Length - 1;

            for (int y = 0; y < img.Height; y++)
            {
                Span<Rgba32> row = img.GetPixelRowSpan(y);
                int yy = y * img.Width;

                for (int x = 0; x < img.Width; x++)
                {
                    int i = x + yy;
                    Rgba32 color = frame.Palette[Math.Min(paletteCount, frame.Pixels[i])];
                    row[x] = color;
                }
            }

            return img;
        }
    }
}
