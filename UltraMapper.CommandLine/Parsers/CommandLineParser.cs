namespace UltraMapper.CommandLine.Parsers
{
    //public class NewCommandLineParser : ICommandLineParser
    //{
    //    private IParser _objectParser = new CommandLineObjectParser();
    //    public const string COMMAND_IDENTIFIER = "--";

    //    public IEnumerable<ParsedCommand> Parse( string commandLine )
    //    {
    //        if( String.IsNullOrWhiteSpace( commandLine ) )
    //            throw new ArgumentNullException( nameof( commandLine ), "Null or empty command" );

    //        var commands = commandLine.SplitKeepDelimiter( COMMAND_IDENTIFIER );
    //        foreach( var command in commands )
    //        {
    //            var cmdIdIndex = command.IndexOf( COMMAND_IDENTIFIER );
    //            var paramsIndex = command.Select( ( ch, index ) => new { ch, index } )
    //                .Skip( cmdIdIndex + COMMAND_IDENTIFIER.Length )
    //                .First( param => Char.IsWhiteSpace( param.ch ) ).index;

    //            var cmdName = command.Substring( cmdIdIndex + COMMAND_IDENTIFIER.Length, 
    //                paramsIndex - cmdIdIndex - COMMAND_IDENTIFIER.Length );

    //            var @params = command.Substring( paramsIndex );
    //            var parsedParam = _objectParser.Parse( @params );

    //            yield return new ParsedCommand()
    //            {
    //                Name = cmdName,
    //                Param = parsedParam
    //            };
    //        }
    //    }

    //    public IEnumerable<ParsedCommand> Parse( string[] commands )
    //    {
    //        //just rejoin and resplit in case a single string in the array contains more than one command
    //        //or commands span more than one string.
    //        return this.Parse( String.Join( " ", commands ) );
    //    }
    //}

    //public class CommandLineObjectParser : IParser
    //{
    //    private enum ParseObjectState { PARAM_NAME, PARAM_VALUE }

    //    private const char OBJECT_START_SYMBOL = '(';
    //    private const char OBJECT_END_SYMBOL = ')';
    //    private const char ARRAY_START_SYMBOL = '[';
    //    private const char ARRAY_END_SYMBOL = ']';
    //    private const char PARAM_NAME_VALUE_DELIMITER = '=';
    //    private const char PARAMS_DELIMITER = ' ';
    //    private const char QUOTE_SYMBOL = '"';
    //    private const char ESCAPE_SYMBOL = '\\';

    //    private readonly StringBuilder _paramValue = new StringBuilder( 64 );
    //    private readonly StringBuilder _paramName = new StringBuilder( 64 );
    //    private readonly StringBuilder _itemValue = new StringBuilder( 64 );
    //    private readonly StringBuilder _quotedText = new StringBuilder( 64 );

    //    private char _currentChar;

    //    public IParsedParam Parse( string text )
    //    {
    //        for( int i = 0; true; i++ )
    //        {
    //            _currentChar = text[ i ];

    //            if( Char.IsWhiteSpace( _currentChar ) )
    //                continue;

    //            switch( _currentChar )
    //            {
    //                case OBJECT_START_SYMBOL:
    //                {
    //                    i++;
    //                    return ParseObject( text, ref i, ParseObjectState.PARAM_NAME );
    //                }

    //                case ARRAY_START_SYMBOL:
    //                {
    //                    i++;
    //                    return ParseArray( text, ref i );
    //                }

    //                default:
    //                    return ParseObject( text, ref i, ParseObjectState.PARAM_NAME );
    //            }
    //        }
    //    }

    //    private ComplexParam ParseObject( string text,
    //        ref int i, ParseObjectState state )
    //    {
    //        var parsedParams = new List<IParsedParam>();
    //        bool isAdded = false;

    //        for( ; true; i++ )
    //        {
    //            _currentChar = text[ i ];

    //            if( Char.IsWhiteSpace( _currentChar ) )
    //                continue;

    //            switch( state )
    //            {
    //                case ParseObjectState.PARAM_NAME:
    //                {
    //                    for( ; state == ParseObjectState.PARAM_NAME; i++ )
    //                    {
    //                        _currentChar = text[ i ];

    //                        if( Char.IsWhiteSpace( _currentChar ) )
    //                            continue;

    //                        switch( _currentChar )
    //                        {
    //                            case QUOTE_SYMBOL:
    //                            {
    //                                i++;
    //                                ParseQuotation( text, ref i, _paramName );

    //                                for( ; true; i++ )
    //                                {
    //                                    _currentChar = text[ i ];

    //                                    if( Char.IsWhiteSpace( _currentChar ) )
    //                                        continue;

    //                                    if( _currentChar == PARAM_NAME_VALUE_DELIMITER )
    //                                        break;
    //                                }

    //                                state = ParseObjectState.PARAM_VALUE;
    //                                break;
    //                            }

    //                            case OBJECT_END_SYMBOL:
    //                            {
    //                                return new ComplexParam()
    //                                {
    //                                    Name = _paramName.ToString(),
    //                                    SubParams = null
    //                                };
    //                            }

    //                            case PARAMS_DELIMITER:
    //                            case OBJECT_START_SYMBOL:
    //                            case ARRAY_START_SYMBOL:
    //                                throw new Exception( $"Unexpected symbol '{_currentChar}' at position {i}" );

    //                            default:
    //                            {
    //                                _paramName.Append( _currentChar );

    //                                for( i++; true; i++ )
    //                                {
    //                                    _currentChar = text[ i ];

    //                                    if( Char.IsWhiteSpace( _currentChar ) )
    //                                        continue;

    //                                    if( _currentChar == PARAM_NAME_VALUE_DELIMITER )
    //                                        break;

    //                                    _paramName.Append( _currentChar );
    //                                }

    //                                state = ParseObjectState.PARAM_VALUE;
    //                                i--;
    //                                break;
    //                            }
    //                        }
    //                    }

    //                    break;
    //                }

    //                case ParseObjectState.PARAM_VALUE:
    //                {
    //                    for( ; state == ParseObjectState.PARAM_VALUE; i++ )
    //                    {
    //                        _currentChar = text[ i ];

    //                        if( Char.IsWhiteSpace( _currentChar ) )
    //                            continue;

    //                        switch( _currentChar )
    //                        {
    //                            case QUOTE_SYMBOL:
    //                            {
    //                                i++;
    //                                ParseQuotation( text, ref i, _paramValue );

    //                                var simpleParam = new SimpleParam()
    //                                {
    //                                    Name = _paramName.ToString(),
    //                                    Value = _paramValue.ToString()
    //                                };

    //                                _paramName.Clear();
    //                                _paramValue.Clear();

    //                                parsedParams.Add( simpleParam );

    //                                for( i++; true; i++ )
    //                                {
    //                                    _currentChar = text[ i ];

    //                                    if( Char.IsWhiteSpace( _currentChar ) )
    //                                        continue;

    //                                    if( _currentChar == OBJECT_END_SYMBOL )
    //                                    {
    //                                        return new ComplexParam()
    //                                        {
    //                                            Name = String.Empty,
    //                                            SubParams = parsedParams.ToArray()
    //                                        };
    //                                    }
    //                                    else if( _currentChar == PARAMS_DELIMITER )
    //                                        continue;
    //                                    else
    //                                    {
    //                                        isAdded = false;
    //                                        state = ParseObjectState.PARAM_NAME;
    //                                        i -= 2;
    //                                        break;
    //                                    }
    //                                }

    //                                break;
    //                            }

    //                            case PARAMS_DELIMITER:
    //                            {
    //                                if( _paramName.Length > 0 )
    //                                {
    //                                    var simpleParam = new SimpleParam()
    //                                    {
    //                                        Name = _paramName.ToString(),
    //                                        Value = _paramValue.ToString()
    //                                    };

    //                                    _paramName.Clear();
    //                                    _paramValue.Clear();

    //                                    parsedParams.Add( simpleParam );
    //                                }

    //                                for( i++; true; i++ )
    //                                {
    //                                    _currentChar = text[ i ];

    //                                    if( Char.IsWhiteSpace( _currentChar ) )
    //                                        continue;

    //                                    if( _currentChar == OBJECT_END_SYMBOL )
    //                                    {
    //                                        return new ComplexParam()
    //                                        {
    //                                            Name = String.Empty,
    //                                            SubParams = parsedParams.ToArray()
    //                                        };
    //                                    }
    //                                    else if( _currentChar == PARAMS_DELIMITER )
    //                                        continue;
    //                                    else
    //                                    {
    //                                        isAdded = false;
    //                                        state = ParseObjectState.PARAM_NAME;
    //                                        i -= 2;
    //                                        break;
    //                                    }
    //                                }
    //                                break;
    //                            }

    //                            case OBJECT_START_SYMBOL:
    //                            {
    //                                i++;
    //                                string paramName2 = _paramName.ToString();
    //                                _paramName.Clear();

    //                                var result = ParseObject( text, ref i, ParseObjectState.PARAM_NAME );
    //                                parsedParams.Add( new ComplexParam()
    //                                {
    //                                    Name = paramName2,
    //                                    SubParams = result.SubParams
    //                                } );

    //                                isAdded = true;
    //                                break;
    //                            }

    //                            case OBJECT_END_SYMBOL:
    //                            {
    //                                if( !isAdded )
    //                                {
    //                                    parsedParams.Add( new SimpleParam()
    //                                    {
    //                                        Name = _paramName.ToString(),
    //                                        Value = _paramValue.ToString()
    //                                    } );
    //                                }

    //                                return new ComplexParam()
    //                                {
    //                                    Name = String.Empty,
    //                                    SubParams = parsedParams.ToArray()
    //                                };
    //                            }

    //                            case ARRAY_START_SYMBOL:
    //                            {
    //                                i++;

    //                                string paramname2 = _paramName.ToString();
    //                                _paramName.Clear();

    //                                var result = ParseArray( text, ref i );
    //                                result.Name = paramname2;
    //                                parsedParams.Add( result );

    //                                isAdded = true;
    //                                break;
    //                            }

    //                            default:
    //                            {
    //                                _paramValue.Append( _currentChar );
    //                                break;
    //                            }
    //                        }
    //                    }

    //                    break;
    //                }
    //            }
    //        }

    //        throw new Exception( $"Expected symbol '{OBJECT_END_SYMBOL}'" );
    //    }

    //    private ArrayParam ParseArray( string text, ref int i )
    //    {
    //        var items = new ArrayParam();

    //        for( ; true; i++ )
    //        {
    //            _currentChar = text[ i ];

    //            if( Char.IsWhiteSpace( _currentChar ) )
    //                continue;

    //            switch( _currentChar )
    //            {
    //                case OBJECT_START_SYMBOL:
    //                {
    //                    i++;

    //                    var obj = ParseObject( text, ref i, ParseObjectState.PARAM_NAME );
    //                    items.Add( obj );

    //                    break;
    //                }

    //                case ARRAY_START_SYMBOL:
    //                {
    //                    i++;

    //                    var result = ParseArray( text, ref i );
    //                    items.Add( result );

    //                    break;
    //                }

    //                case QUOTE_SYMBOL:
    //                {
    //                    i++;

    //                    ParseQuotation( text, ref i, _quotedText );
    //                    items.Add( new SimpleParam() { Value = _quotedText.ToString() } );

    //                    break;
    //                }

    //                case PARAMS_DELIMITER:
    //                {
    //                    if( _itemValue.Length > 0 )
    //                    {
    //                        items.Add( new SimpleParam() { Value = _itemValue.ToString() } );
    //                        _itemValue.Clear();
    //                    }

    //                    break;
    //                }

    //                case ARRAY_END_SYMBOL:
    //                {
    //                    if( _itemValue.Length > 0 )
    //                    {
    //                        items.Add( new SimpleParam() { Value = _itemValue.ToString() } );
    //                        _itemValue.Clear();
    //                    }

    //                    return items;
    //                }

    //                default:
    //                {
    //                    _itemValue.Append( _currentChar );
    //                    break;
    //                }
    //            }
    //        }

    //        throw new Exception( $"Expected symbol '{ARRAY_END_SYMBOL}'" );
    //    }

    //    private void ParseQuotation( string text, ref int i, StringBuilder _quotedText )
    //    {
    //        _quotedText.Clear();

    //        for( ; true; i++ )
    //        {
    //            _currentChar = text[ i ];

    //            switch( _currentChar )
    //            {
    //                case ESCAPE_SYMBOL:
    //                {
    //                    _currentChar = text[ ++i ];

    //                    switch( _currentChar )
    //                    {
    //                        case 'b': _quotedText.Append( '\b' ); break;
    //                        case 'f': _quotedText.Append( '\f' ); break;
    //                        case 'n': _quotedText.Append( '\n' ); break;
    //                        case 'r': _quotedText.Append( '\r' ); break;
    //                        case 't': _quotedText.Append( '\t' ); break;
    //                        case 'u':
    //                        {
    //                            i++;

    //                            string unicodeLiteral = text.Substring( i, 4 );

    //                            i += 3;

    //                            int code = Int32.Parse( unicodeLiteral, System.Globalization.NumberStyles.HexNumber );
    //                            string unicodeChar = Char.ConvertFromUtf32( code );
    //                            _quotedText.Append( unicodeChar );

    //                            break;
    //                        }

    //                        default:
    //                        {
    //                            _quotedText.Append( _currentChar );
    //                            break;
    //                        }
    //                    }

    //                    break;
    //                }

    //                case QUOTE_SYMBOL:
    //                {
    //                    return;
    //                }

    //                default:
    //                {
    //                    _quotedText.Append( _currentChar );
    //                    break;
    //                }
    //            }
    //        }

    //        throw new Exception( $"Expected symbol '{QUOTE_SYMBOL}'" );
    //    }
    //}
}
