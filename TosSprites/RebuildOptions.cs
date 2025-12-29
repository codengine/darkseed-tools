using CommandLine;
using JetBrains.Annotations;

#pragma warning disable CS8618

namespace TosSprites;

[Verb("rebuild", HelpText = "Rebuild .gif files to an .NSP file")]
[UsedImplicitly]
internal class RebuildOptions
{
    [Option('i', "in", Required = true, HelpText = "Path to input files")]
    public string InputPath { get; set; }
        
    [Option('p', "prefix", Required = true, HelpText = "Input filename prefix")]
    public string InputFilePrefix { get; set; }
        
    [Option('o', "out", Required = true, HelpText = "Path to output file")]
    public string OutputFile { get; set; }
}