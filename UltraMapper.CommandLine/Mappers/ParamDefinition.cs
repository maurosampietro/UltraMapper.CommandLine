using System;

namespace UltraMapper.CommandLine
{
    public enum MemberTypes { UNDEFINED, PROPERTY, METHOD, METHOD_PARAM }

    public class ParameterDefinition
    {
        private string _name;
        public string Name
        {
            get
            {
                if( !String.IsNullOrWhiteSpace( Options?.Name ) )
                    return Options.Name;

                return _name;
            }

            set { _name = value; }
        }

        public Type Type { get; set; }
        public ParameterDefinition[] SubParams { get; set; }
        public OptionAttribute Options { get; set; }
        public MemberTypes MemberType { get; set; } = MemberTypes.UNDEFINED;

        public ParameterDefinition()
        {
            this.Options = new OptionAttribute();
            this.SubParams = Array.Empty<ParameterDefinition>();
        }

        public override string ToString()
        {
            return Options.IsRequired == true ? $"[Required] {Name}"
                : $"[Optional] {Name}";
        }
    }
}
