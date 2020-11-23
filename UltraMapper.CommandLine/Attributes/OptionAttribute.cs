using System;

namespace CommandLine.AutoParser
{
    public sealed class OptionAttribute : Attribute
    {
        public string Name { get; set; } = "";
        public int Order { get; set; } = -1;
        public string HelpText { get; set; } = "<Additional info is missing>";
        public bool IsRequired { get; set; } = true;
        public bool IsIgnored { get; set; } = false;
    }
}
