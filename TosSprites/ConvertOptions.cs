using CommandLine;
using JetBrains.Annotations;

#pragma warning disable CS8618

namespace TosSprites;

[Verb("convert", HelpText = "Convert a .NSP file to .gif files")]
[UsedImplicitly]
internal class ConvertOptions
{
    [Option('i', "in", Required = true, HelpText = "Path to the input file")]
    public string InputPath { get; set; }
        
    [Option('o', "out", Required = true, HelpText = "Path where the gif files are stored")]
    public string OutputPath { get; set; }
}