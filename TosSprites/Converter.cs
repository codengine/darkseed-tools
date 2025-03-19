using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace TosSprites;

public static class Converter
{
    private static readonly GifEncoder Encoder;
    
    private static readonly Dictionary<(byte R, byte G, byte B), byte> ColorToIndexMap = new();
    private static readonly Rgba32[] PaletteColors = new Rgba32[16];
    
    static Converter()
    {
        for (byte i = 0; i < 15; i++)
        {
            var gray = (byte)(i * 255 / 14);
            PaletteColors[i] = new Rgba32(gray, gray, gray, 255);
            ColorToIndexMap[(gray, gray, gray)] = i;
        }
        
        // Transparent (0xFF in original file)
        PaletteColors[15] = new Rgba32(0, 0, 0, 0);
        
        var colors = new Color[16];
        for (byte i = 0; i < 16; i++)
        {
            colors[i] = new Color(PaletteColors[i]);
        }
        
        Encoder = new GifEncoder
        {
            ColorTableMode = GifColorTableMode.Local,
            Quantizer = new PaletteQuantizer(
                new ReadOnlyMemory<Color>(colors), 
                new QuantizerOptions { Dither = null })
        };
    }

    public static void ToGif(string outputPath, Sprite sprite)
    {
        // Special case for empty sprites
        if (sprite.IsEmptySprite)
        {
            SaveEmptySprite(outputPath);
            return;
        }
        
        // We only care for visible pixels
        using var image = new Image<Rgba32>(sprite.Width, sprite.Height);
        
        for (var y = 0; y < sprite.Height; y++)
        {
            for (var x = 0; x < sprite.Width; x++)
            {
                var pixelValue = sprite.GetPixel(x, y);
                
                if (pixelValue == Sprite.TransparentPixel)
                {
                    image[x, y] = new Rgba32(0, 0, 0, 0);
                }
                else if (pixelValue < PaletteColors.Length)
                {
                    image[x, y] = PaletteColors[pixelValue];
                }
                else
                {
                    image[x, y] = new Rgba32(0, 0, 0, 255);
                }
            }
        }
        
        image.Save(outputPath, Encoder);
    }

    private static void SaveEmptySprite(string outputPath)
    {
        using var emptyImage = new Image<Rgba32>(1, 1);
        emptyImage[0, 0] = new Rgba32(0, 0, 0, 0); // Transparent
        emptyImage.Save(outputPath);
    }

    public static Sprite FromGif(string inputPath)
    {
        using var image = Image.Load<Rgba32>(inputPath);
        
        if (image is { Width: 1, Height: 1 } && image[0, 0].A == 0)
        {
            return Sprite.CreateEmpty();
        }
        
        var width = (ushort)image.Width;
        var height = (ushort)image.Height;
        var pitch = (ushort)(width + (width & 1));

        var sprite = new Sprite(width, height, pitch);
        
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var color = image[x, y];
                
                if (color.A == 0)
                {
                    sprite.SetPixel(x, y, Sprite.TransparentPixel);
                }
                else
                {
                    var colorIndex = GetExactColorIndex(color, inputPath, x, y);
                    sprite.SetPixel(x, y, colorIndex);
                }
            }
            
            if (width != pitch)
            {
                // For odd widths, explicitly set the padding pixel to transparent
                sprite.SetPixel(width, y, Sprite.TransparentPixel);
            }
        }
        
        return sprite;
    }
    
    private static byte GetExactColorIndex(Rgba32 color, string filePath, int x, int y)
    {
        if (color.A == 0) return Sprite.TransparentPixel;
        
        if (ColorToIndexMap.TryGetValue((color.R, color.G, color.B), out var index))
        {
            return index;
        }
        
        throw new Exception($"No exact color match found for RGB({color.R},{color.G},{color.B}) at position ({x},{y}) in file {Path.GetFileName(filePath)}");
    }
}