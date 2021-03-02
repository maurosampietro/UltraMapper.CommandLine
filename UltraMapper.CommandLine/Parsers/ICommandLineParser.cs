using System.Collections.Generic;

namespace UltraMapper.CommandLine.Parsers
{
    /// <summary>
    /// Analyzes a string and identifies commands and parameters
    /// </summary>
    public interface ICommandLineParser
    {
        IEnumerable<ParsedCommand> Parse( string commandLine );
        IEnumerable<ParsedCommand> Parse( string[] commands );
    }
}
