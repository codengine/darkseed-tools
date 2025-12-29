using CommandLine;
using JetBrains.Annotations;

namespace DarkSeedTools;

[Verb("extract", HelpText = "Extract text from TOSTEXT.BIN to a text file")]
[UsedImplicitly]
public class ExtractOptions : CommonOptions;