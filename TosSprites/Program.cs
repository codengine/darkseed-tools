using CommandLine;
using NaturalSort.Extension;

namespace TosSprites;

internal abstract class Program
{
    // Extract operation class
    [Verb("convert", HelpText = "Convert a .NSP file to .gif files")]
    public class ConvertOptions
    {
        [Option('i', "in", Required = true, HelpText = "Path to the input file")]
        public string InputPath { get; set; }
        
        [Option('o', "out", Required = true, HelpText = "Path where the gif files are stored")]
        public string OutputPath { get; set; }
    }

    // Rebuild operation class
    [Verb("rebuild", HelpText = "Rebuild .gif files to an .NSP file")]
    public class RebuildOptions
    {
        [Option('i', "in", Required = true, HelpText = "Path to input files")]
        public string InputPath { get; set; }
        
        [Option('p', "prefix", Required = true, HelpText = "Input filename prefix")]
        public string InputFilePrefix { get; set; }
        
        [Option('o', "out", Required = true, HelpText = "Path to output file")]
        public string OutputFile { get; set; }
    }
    
    private static int Main(string[] args)
    {
        return Parser.Default.ParseArguments<ConvertOptions, RebuildOptions>(args)
            .MapResult(
                (ConvertOptions opts) => RunExtract(opts),
                (RebuildOptions opts) => RunRebuild(opts),
                _ => 1);
    }

    private static int RunExtract(ConvertOptions opts)
    {
        if (!File.Exists(opts.InputPath))
        {
            Console.Error.WriteLine($"Input file not found: {opts.InputPath}");
            return -1;
        }

        if (!Directory.Exists(opts.OutputPath))
        {
            Directory.CreateDirectory(opts.OutputPath);            
        }
        
        // Create the base filename for sprites (e.g., CPLAYER_NSP)
        var baseFileName = Path.GetFileNameWithoutExtension(opts.InputPath);
        
        Console.WriteLine($"Processing {opts.InputPath}...");
        
        try
        {
            using var file = File.OpenRead(opts.InputPath);
            using var reader = new BinaryReader(file);
            
            var spriteCount = file.Length > 0 ? 0xC0 / 2 : 0;
            var sprites = new Sprite[spriteCount];
            
            for (var i = 0; i < sprites.Length; i++)
            {
                int width = reader.ReadByte();
                int height = reader.ReadByte();
                var pitch = width + (width & 1); // Add padding if width is odd
                sprites[i] = new Sprite((ushort)width, (ushort)height, (ushort)pitch);
            }
            
            for (var i = 0; i < sprites.Length; i++)
            {
                sprites[i].Load(reader);
                var gifPath = Path.Combine(opts.OutputPath, $"{baseFileName}_{i}.gif");
                Converter.ToGif(gifPath, sprites[i]);
            }
            
            Console.WriteLine($"Converted {sprites.Length} sprites to GIF in {opts.OutputPath}");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return -1;
        }
    }
    
    private static int RunRebuild(RebuildOptions opts)
    {
        if (!Directory.Exists(opts.InputPath))
        {
            Console.Error.WriteLine($"Input directory not found: {opts.InputPath}");
            return -1;
        }

        var sprites = Directory
            .EnumerateFiles(opts.InputPath, $"{opts.InputFilePrefix}_*.gif", SearchOption.TopDirectoryOnly)
            .OrderBy(x => x, StringComparison.OrdinalIgnoreCase.WithNaturalSort())
            .ToDictionary(file => file, Converter.FromGif);
            
        if (sprites.Count == 0)
        {
            Console.Error.WriteLine($"No input files found at {opts.InputPath}");
            return -1;
        }

        if (File.Exists(opts.OutputFile))
        {
            File.Delete(opts.OutputFile);
        }
        
        // Step 3: Create a new file by loading the GIFs back
        using var outFile = File.Create(opts.OutputFile);
        using var writer = new BinaryWriter(outFile);
        
        foreach (var sprite in sprites.Select(kvp => kvp.Value))
        {
            writer.Write((byte)sprite.Width);
            writer.Write((byte)sprite.Height);
        }
        
        foreach (var kvp in sprites)
        {
            try
            {
                kvp.Value.Save(writer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing sprite {kvp.Key}: {ex.Message}");
                throw;
            }
        }
            
        Console.WriteLine($"Successfully created {opts.OutputFile}");
        return 1;
    }
}