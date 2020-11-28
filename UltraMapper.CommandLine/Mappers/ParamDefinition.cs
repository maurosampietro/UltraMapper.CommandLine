using System;

namespace UltraMapper.CommandLine
{
    public enum MemberTypes { UNDEFINED, PROPERTY, METHOD, METHOD_PARAM }

    public class ParameterDefinition
    {
        public string Name { get; internal set; }
        public Type Type { get; set; }
        public ParameterDefinition[] SubParams { get; set; }
        public OptionAttribute Options { get; internal set; }
        public MemberTypes MemberType { get; set; } = MemberTypes.UNDEFINED;

        public override string ToString()
        {
            return Options.IsRequired == true ? $"[Required] {Name}"
                : $"[Optional] {Name}";
        }
    }
}
