using CommandLine;
using JetBrains.Annotations;

namespace DarkSeedTools;

[Verb("rebuild", HelpText = "Rebuild TOSTEXT.BIN from a text file")]
[UsedImplicitly]
public class RebuildOptions : CommonOptions;