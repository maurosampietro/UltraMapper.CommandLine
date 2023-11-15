using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UltraMapper.CommandLine.Internals;
using UltraMapper.Parsing;

namespace UltraMapper.CommandLine.Parsers
{
    /// <summary>
    /// Analyzes a string and identifies commands and parameters
    /// </summary>
    public sealed class DefaultParser : ICommandLineParser
    {
        public const string COMMAND_IDENTIFIER = "--";
        public const string PARAMETER_NAME_VALUE_SEPARATOR = "="; //avoid semicolon cause it can be used in unquoted paths like C:\
        public const string OBJECT_START_IDENTIFIER = "(";
        public const string OBJECT_END_IDENTIFIER = ")";
        public const string ARRAY_START_IDENTIFIER = "[";
        public const string ARRAY_END_IDENTIFIER = "]";

        public static readonly Regex CommandRegex = new Regex( @"
       
        ^--(?<command>\w+)\s*
        
        (           
            (?<params>([\w\.]+\s*=\s*){0,1}
                \(                            # opening (
                    (?>                           # non-backtracking atomic group
                        (?>                         # non-backtracking atomic group
                            "" (?: [^""\\] + | \\. )* ""  # double quoted string with escapes
                            | [^""\(\)]+                  # literals, spaces, etc
                            | (?<open>       \( )       # open += 1
                            | (?<close-open> \) )       # open -= 1, only if open > 0 (balancing group)
                        )*
                    )
                    (?(open) (?!) )               # fail if open > 0
                \)                            # final )
            )\s*

            |  #OR 
            
            (?<params>([\w\.]+\s*=\s*){0,1}
                \[                          # opening [
                    (?>                           # non-backtracking atomic group
                        (?>                         # non-backtracking atomic group
                            "" (?: [^""\\] + | \\. )* ""  # double quoted string with escapes
                            | [^""\[\]]+                  # literals, spaces, etc
                            | (?<open>       \[ )       # open += 1
                            | (?<close-open> \] )       # open -= 1, only if open > 0 (balancing group)
                        )*
                    )
                    (?(open) (?!) )               # fail if open > 0                
                \]                            # final ]
            )\s*               # final ]

            |

            (?<params>([\w\.]+\s*=\s*){0,1}""(?: [^""]+ | \\. )* "")\s*  # double quoted string with escapes
            
            |   #OR

            (?<params>([\w\.]+\s*=\s*){0,1}[^\s\(\)\[\]\""]+)\s*

        )* (?(open) (?!) )               # fail if open > 0
        $",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled, TimeSpan.FromSeconds( 3 ) );

        public static readonly Regex BalancedParenthesesRegex = new Regex( @"                
        (         
            (?<params>([\w\.]+\s*=\s*){0,1}
                \(                            # opening (
                    (?>                           # non-backtracking atomic group
                        (?>                         # non-backtracking atomic group
                            "" (?: [^""\\] + | \\. )* ""  # double quoted string with escapes
                            | [^""\(\)]+                  # literals, spaces, etc
                            | (?<open>       \( )       # open += 1
                            | (?<close-open> \) )       # open -= 1, only if open > 0 (balancing group)
                        )*
                    )
                    (?(open) (?!) )               # fail if open > 0
                \)                            # final )
            )\s*

            |  #OR 
            
            (?<params>([\w\.]+\s*=\s*){0,1}
                \[ # opening [
                   (?>                           # non-backtracking atomic group
                        (?>                         # non-backtracking atomic group
                            "" (?: [^""\\] + | \\. )* ""  # double quoted string with escapes
                            | [^""\[\]]+                  # literals, spaces, etc
                            | (?<open>       \[ )       # open += 1
                            | (?<close-open> \] )       # open -= 1, only if open > 0 (balancing group)
                        )*
                    )
                    (?(open) (?!) )               # fail if open > 0                
                \]                            # final ]
            )\s*

            | #OR

            (?<params>([\w\.]+\s*=\s*){0,1}[^\s\(\)\[\]\""]+)\s*

            |

            (?<params>([\w\.]+\s*=\s*){0,1}""(?: [^""\\]+ | \\. )* "")\s*  # double quoted string with escapes
            
            |   #OR
  
            (?<params>([\w\.]+\s*=\s*){0,1}""(?: [^""]+ | \\. )* "")\s*  # double quoted string with escapes
            
            |   #OR

            (?<params>([\w\.]+\s*=\s*){0,1}[^\s\(\)\[\]\""]+)\s*

        )+? (?(open) (?!) )               # fail if open > 0",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled, TimeSpan.FromSeconds( 3 ) );

        public static readonly Regex NameValueSpitter = new Regex( @"^((?<paramName>[\w\.]+)\s*=\s*){0,1}(?<paramValue>.*)",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled, TimeSpan.FromSeconds( 3 ) );

        public IEnumerable<ParsedCommand> Parse( string[] args )
        {
            return this.Parse( String.Join( " ", args ) );
        }

        public IEnumerable<ParsedCommand> Parse( string commandLine )
        {
            if(String.IsNullOrWhiteSpace( commandLine ))
                throw new ArgumentNullException( nameof( commandLine ), "Null or empty command" );

            var commands = commandLine.SplitKeepDelimiter( COMMAND_IDENTIFIER, "([", ")]" ).ToArray();
            if(commands.Length == 0)
            {
                //try implicit notation
                {
                    var match = BalancedParenthesesRegex.Match( commandLine );
                    if(!match.Success)
                        throw new SyntaxErrorException();

                    var parsedParams = match.Groups[ "params" ]
                        .Captures.Cast<Capture>()
                        .Select( c => c.Value );
                }

                throw new SyntaxErrorException();
            }

            int paramIndex = 0;
            foreach(var command in commands)
            {
                var match = CommandRegex.Match( command );
                if(!match.Success)
                    throw new SyntaxErrorException();

                var cmdName = match.Groups[ "command" ].Value;

                var parsedParams = match.Groups[ "params" ]
                   .Captures.Cast<Capture>()
                   .Select( c => c.Value );

                var parameters = GetCommandParams( parsedParams ).ToList();

                IParsedParam theparam = null;
                if(parameters.Count > 1)
                {
                    var newcp = new ComplexParam()
                    {
                        //Name = cmdName,
                        Index = paramIndex,
                        //SubParams = parameters.ToArray()
                    };

                    theparam = newcp;

                    foreach(var item in parameters)
                    {
                        if(item is ComplexParam cp)
                            newcp.Complex.Add( cp );
                        else if(item is SimpleParam sp)
                            newcp.Simple.Add( sp );
                        else if(item is ArrayParam ap)
                            newcp.Array.Add( ap );
                    }

                }
                else
                {
                    var p = parameters.FirstOrDefault();

                    if(p == null)
                        theparam = null;
                    else
                    {
                        p.Index = paramIndex;
                        theparam = p;
                    }
                }

                yield return new ParsedCommand()
                {
                    Name = cmdName,
                    Param = theparam
                };

                paramIndex++;
            }
        }

        private IEnumerable<IParsedParam> GetCommandParams( IEnumerable<string> parameters )
        {
            int paramIndex = 0;
            foreach(var item in parameters)
            {
                var match = NameValueSpitter.Match( item );
                var paramName = match.Groups[ "paramName" ].Value;
                var paramValue = match.Groups[ "paramValue" ].Value;

                if(paramValue.StartsWith( ARRAY_START_IDENTIFIER ) &&
                    paramValue.EndsWith( ARRAY_END_IDENTIFIER ))
                {
                    paramValue = paramValue.Substring( 1, paramValue.Length - 2 );

                    var subMatches = BalancedParenthesesRegex.Matches( paramValue );
                    // if( subMatches.Count > 1 )
                    {
                        var subParams = subMatches.Cast<Match>()
                            .Select( c => c.Groups[ "params" ].Value );

                        var arrayParam = new ArrayParam()
                        {
                            Name = paramName,
                            Index = paramIndex
                        };

                        foreach(var subParam in GetCommandParams( subParams ))
                        {
                            if(subParam is SimpleParam sp)
                                arrayParam.Simple.Add( sp );
                            else if( subParam is ComplexParam cp)
                                arrayParam.Complex.Add(cp );
                            else if (subParam is ArrayParam ap )
                                arrayParam.Array.Add( ap );
                        }

                        yield return arrayParam;
                    }
                }
                else if(paramValue.StartsWith( OBJECT_START_IDENTIFIER ) &&
                    paramValue.EndsWith( OBJECT_END_IDENTIFIER ))
                {
                    paramValue = paramValue.Substring( 1, paramValue.Length - 2 );

                    var subMatches = BalancedParenthesesRegex.Matches( paramValue );
                    var subParams = subMatches.Cast<Match>()
                        .Select( c => c.Groups[ "params" ].Value );

                    var subparamList = new List<IParsedParam>();

                    foreach(var subParam in GetCommandParams( subParams ))
                        subparamList.Add( subParam );

                    var newcp = new ComplexParam()
                    {
                        Name = paramName,
                        Index = paramIndex,
                        //SubParams = subparamList.ToArray()
                    };

                    foreach(var item2 in subparamList)
                    {
                        if(item2 is ComplexParam cp)
                            newcp.Complex.Add( cp );
                        else if(item2 is SimpleParam sp)
                            newcp.Simple.Add( sp );
                        else if(item2 is ArrayParam ap)
                            newcp.Array.Add( ap );
                    }

                    yield return newcp;
                }
                else if(paramValue.StartsWith( @"""" ) && paramValue.EndsWith( @"""" ))
                {
                    paramValue = paramValue.Substring( 1, paramValue.Length - 2 );
                    if(paramValue == "null") paramValue = null;

                    yield return new SimpleParam()
                    {
                        Name = paramName,
                        Value = paramValue,
                        Index = paramIndex
                    };
                }
                else
                {
                    if(paramValue == "null") paramValue = null;

                    yield return new SimpleParam()
                    {
                        Name = paramName,
                        Value = paramValue,
                        Index = paramIndex
                    };
                }

                paramIndex++;
            }
        }
    }
}