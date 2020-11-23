using UltraMapper.CommandLine.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
