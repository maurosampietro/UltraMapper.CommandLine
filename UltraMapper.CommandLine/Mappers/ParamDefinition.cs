using System;

namespace CommandLine.AutoParser
{
    public class ParameterDefinition 
    {
        public string Name { get; internal set; }
        public Type Type { get; set; }
        public ParameterDefinition[] SubParams { get; set; }
        public OptionAttribute Options { get; internal set; }

        public override string ToString()
        {
            return Options.IsRequired == true ? $"[Required] {Name}"
                : $"[Optional] {Name}";
        }
    }
}
