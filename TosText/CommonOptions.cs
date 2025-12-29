using CommandLine;
#pragma warning disable CS8618

namespace DarkSeedTools;

public class CommonOptions
{
    [Option('i', "in", Required = true, HelpText = "Path to input file")]
    public string InputFile { get; set; }
        
    [Option('o', "out", Required = true, HelpText = "Path to output file")]
    public string OutputFile { get; set; }
        
    [Option("mb", Required = false, Default = false, HelpText = "Toggle multibyte character encoding/decoding")]
    public bool UseMultibyte { get; set; }
}