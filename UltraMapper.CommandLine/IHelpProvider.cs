using System;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.Parsing;

namespace UltraMapper.CommandLine
{
    public interface IHelpProvider
    {
        ParameterDefinition HelpCommandDefinition { get; }
        bool ShowHelpOnError { get; set; }
        void Initialize( Type type );
        void GetHelp( Type type, IParsedParam helpParam );
    }
}
