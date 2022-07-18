//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text.RegularExpressions;
//using UltraMapper.CommandLine.Internals;
//using UltraMapper.Json;
//using UltraMapper.Parsing;

//namespace UltraMapper.CommandLine.Parsers
//{
//    public class JsonCommandLineParser : ICommandLineParser
//    {
//        public const string COMMAND_IDENTIFIER = "--";
//        private readonly IParser _jsonParser = new JsonParser();

//        public IEnumerable<ParsedCommand> Parse( string commandLine )
//        {
//            var commands = commandLine.SplitKeepDelimiter( COMMAND_IDENTIFIER, "{[\"", "}]\"" ).ToArray();

//            foreach( var command in commands )
//            {
//                var commandName = Regex.Match( command, @"^\s*--\w+(?=\s*)" );
//                var param = _jsonParser.Parse( command.Substring( commandName.Length ) );

//                yield return new ParsedCommand()
//                {
//                    Name = commandName.Value.Replace( "--", String.Empty ),
//                    Param = param
//                };
//            }
//        }

//        public IEnumerable<ParsedCommand> Parse( string[] commands )
//        {
//            return this.Parse( String.Join( " ", commands ) );
//        }
//    }
//}
