using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace UltraMapper.CommandLine.Parsers
{

    public sealed class JsonParser
    {
        public static readonly Regex NameValueSpitter = new Regex( @"(?<Key>""{0,1}[\w]+""{0,1})\s*:\s*""{0,1}(?<Value>[^""]*)""{0,1},",
          RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled, TimeSpan.FromSeconds( 3 ) );
    }
}
