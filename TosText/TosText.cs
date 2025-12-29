using System.Text;
using CommandLine;

namespace DarkSeedTools;

internal abstract class Program
{
    private static int Main(string[] args)
    {
        return Parser.Default.ParseArguments<ExtractOptions, RebuildOptions>(args)
            .MapResult(
                (ExtractOptions opts) => RunExtract(opts),
                (RebuildOptions opts) => RunRebuild(opts),
                _ => 1);
    }

    private static int RunExtract(ExtractOptions opts)
    {
        try
        {
            Console.WriteLine($"Extracting {opts.InputFile} to {opts.OutputFile}...");
                
            var strings = new List<string>();
                
            using (var file = File.OpenRead(opts.InputFile))
            using (var reader = new BinaryReader(file))
            {
                var firstOffset = TosTextUtility.ReadUInt16Le(reader);
                var numEntries = firstOffset / 2;
                    
                Console.WriteLine($"File size: {file.Length} bytes");
                Console.WriteLine($"Number of entries: {numEntries}");
                    
                if (firstOffset > file.Length)
                {
                    Console.WriteLine("Error: Invalid first offset (larger than file)");
                    return 1;
                }
                    
                file.Seek(0, SeekOrigin.Begin);
                    
                var offsets = new List<ushort>(numEntries);
                for (var i = 0; i < numEntries; i++)
                {
                    var offset = TosTextUtility.ReadUInt16Le(reader);
                    offsets.Add(offset);
                }
                    
                for (var i = 0; i < numEntries; i++)
                {
                    var offset = offsets[i];
                    ushort length;
                        
                    if (i == numEntries - 1)
                    {
                        length = (ushort)(file.Length - offset - 2); // Subtract 2 bytes for 1AFF
                    }
                    else
                    {
                        length = (ushort)(offsets[i + 1] - offset);
                    }
                        
                    try
                    {
                        file.Seek(offset, SeekOrigin.Begin);
                        var stringContent = TosTextUtility.ReadString(reader, length, opts.UseMultibyte);
                        stringContent = stringContent.TrimEnd('\r', '\n');
                            
                        strings.Add(stringContent);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading string at index {i}, offset 0x{offset:X4}: {ex.Message}");
                        strings.Add($"[ERROR READING STRING {i}]");
                    }
                }
            }
                
            using (var writer = new StreamWriter(opts.OutputFile, false, Encoding.UTF8))
            {
                foreach (var str in strings)
                {
                    writer.WriteLine(str);
                    writer.WriteLine("-----");
                }
            }
                
            Console.WriteLine($"Successfully extracted {strings.Count} strings.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during extraction: {ex.Message}");
            return 1;
        }
    }

    private static int RunRebuild(RebuildOptions opts)
    {
        try
        {
            Console.WriteLine($"Rebuilding {opts.InputFile} to {opts.OutputFile}...");
                
            var strings = new List<string>();
                
            using (var reader = new StreamReader(opts.InputFile, Encoding.UTF8))
            {
                var currentString = new StringBuilder();
                var isFirstLine = true;

                while (reader.ReadLine() is { } line)
                {
                    if (line == "-----")
                    {
                        strings.Add(currentString.ToString());
                        currentString.Clear();
                        isFirstLine = true;
                    }
                    else
                    {
                        if (!isFirstLine)
                            currentString.AppendLine();
                            
                        currentString.Append(line);
                        isFirstLine = false;
                    }
                }
                    
                if (currentString.Length > 0)
                    strings.Add(currentString.ToString());
            }
                
            Console.WriteLine($"Read {strings.Count} strings from input file");
                
            using (var file = File.Create(opts.OutputFile))
            using (var writer = new BinaryWriter(file))
            {
                var numEntries = strings.Count;
                    
                var encodedStrings = new List<byte[]>();
                var totalDataSize = 0;
                    
                foreach (var str in strings)
                {
                    var stringWithLineEnding = str + "\r\n";
                    var encoded = TosTextUtility.EncodeString(stringWithLineEnding, opts.UseMultibyte);
                    encodedStrings.Add(encoded);
                    totalDataSize += encoded.Length;
                }
                    
                var offsetTableSize = numEntries * 2;
                    
                var offsets = new List<ushort>();
                var currentOffset = (ushort)offsetTableSize;
                    
                offsets.Add(currentOffset);
                    
                for (var i = 1; i < numEntries; i++)
                {
                    currentOffset += (ushort)encodedStrings[i-1].Length;
                    offsets.Add(currentOffset);
                }
                    
                foreach (var offset in offsets)
                {
                    TosTextUtility.WriteUInt16Le(writer, offset);
                }
                    
                foreach (var data in encodedStrings)
                {
                    writer.Write(data);
                }
                    
                writer.Write((byte)0x1A);
                writer.Write((byte)0xFF);
                    
                Console.WriteLine($"Successfully rebuilt {opts.OutputFile} with {strings.Count} strings");
                Console.WriteLine($"Total size: {file.Length} bytes (Offset table: {offsetTableSize} bytes, Data: {totalDataSize} bytes, Trailer: 2 bytes)");
            }
                
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during rebuild: {ex.Message}");
            return 1;
        }
    }
}