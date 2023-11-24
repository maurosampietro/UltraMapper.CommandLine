using System;

namespace UltraMapper.CommandLine
{
    public sealed class OptionAttribute : UltraMapper.Parsing.OptionAttribute
    {
        public string HelpText { get; set; } = "<Additional info is missing>";       
    }
}
