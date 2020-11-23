using UltraMapper.CommandLine.Mappers;
using UltraMapper.CommandLine.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using UltraMapper.Internals;

namespace UltraMapper.CommandLine
{
    /// <summary>
    /// The parser does not know about the structure of the target type T. 
    /// If a SimpleParam maps to a non primitive type then we need to encapsulate the SimpleParam in a ComplexParam.
    /// </summary>
    public class ParsedParametersAdapter
    {
        private static readonly ParsedCommandSpecificChecks _commandChecks = new ParsedCommandSpecificChecks();
        private static readonly ComplexParamSpecificChecks _complexParamChecks = new ComplexParamSpecificChecks();

        public IEnumerable<ParsedCommand> AdaptParsedCommandsToTargetType<T>( IEnumerable<ParsedCommand> commands )
        {
            var definition = DefinitionHelper.GetCommandDefinitions( typeof( T ) ).ToArray();
            _commandChecks.CheckThrowCommandsExist( commands, typeof( T ), definition );

            foreach( var command in commands )
            {
                //adapt
                var def = definition.First( d => d.Name.ToLower() == command.Name.ToLower() );
                command.Param = Internal( command.Param, def );

                if( command.Param != null )
                {
                    //named command with only 1 nameless param: the name of the param is the name of the command
                    if( def.SubParams.Length <= 1 && String.IsNullOrEmpty( command.Param.Name ) )
                        command.Param.Name = command.Name;
                }

                _commandChecks.Checks( command, typeof( T ), definition );
                yield return command;
            }
        }

        private IParsedParam Internal( IParsedParam param, ParameterDefinition def )
        {
            if( param == null ) return null;

            if( param is SimpleParam sp )
            {
                if( def.Type.IsBuiltIn( false ) )
                {
                    return sp;
                }
                else
                {
                    return new ComplexParam()
                    {
                        Name = def.Name,
                        Index = 0,
                        SubParams = new[] { sp }
                    };
                }
            }
            else if( param is ComplexParam cp )
            {
                _complexParamChecks.Checks( cp, def.Type, def.SubParams );

                var subParamsDef = def.SubParams;
                if( subParamsDef.All( s => s.Type.IsBuiltIn( false ) ) )
                    return cp;
                else
                {
                    //fare il check di ogni parametro contro la definizione cercando i possibili match.
                    //E' possibile effettuare tutti i vari check dei parametri qui.

                    IEnumerable<IParsedParam> getSubparams()
                    {
                        for( int i = 0; i < cp.SubParams.Length; i++ )
                        {
                            var item = cp.SubParams[ i ];
                            var newitem = Internal( item, def.SubParams[ i ] );
                            yield return newitem;
                        }
                    };

                    var subparams = getSubparams().ToArray();

                    return new ComplexParam()
                    {
                        Name = def.Name,
                        Index = 0,
                        SubParams = subparams
                    };
                }
            }
            else if( param is ArrayParam )
                return param;

            throw new NotSupportedException();
        }

        private class ParsedCommandSpecificChecks
        {
            public void Checks( ParsedCommand param, Type target, ParameterDefinition[] paramsDef )
            {
                CheckThrowCommandExists( param, target, paramsDef );
                CheckThrowNameCollisions( target, paramsDef );
                CheckThrowParamNumber( param, paramsDef );
                //CheckThrowNamedParamsExist( param, target, paramsDef );
                //CheckThrowNamedParamsOrder( param );
            }

            public void CheckThrowCommandsExist( IEnumerable<ParsedCommand> param, Type target, ParameterDefinition[] paramsDef )
            {
                foreach( var item in param )
                    CheckThrowCommandExists( item, target, paramsDef );
            }

            public void CheckThrowCommandExists( ParsedCommand param, Type target, ParameterDefinition[] paramsDef )
            {
                if( !paramsDef.Any( p => p.Name.ToLower() == param.Name.ToLower() ) )
                    throw new UndefinedParameterException( target, "Command does not exist" );
            }

            /// <summary>
            /// Check and throw if any namedparam is used more than once
            /// (automatic assigned names and explicitly assigned names are checked to be unique)
            /// </summary>
            private static void CheckThrowNameCollisions( Type target, ParameterDefinition[] paramsDef )
            {
                var nameCollisions = paramsDef.GroupBy( param => param.Name.ToLower() )
                    .Where( group => group.Count() > 1 );

                foreach( var collision in nameCollisions )
                {
                    //overload checking is possible but i don't think it's worth the complexity

                    throw new ArgumentException( $"Type '{target}' defines multiple commands named '{collision.Key}'. " +
                        "Please use the Option attribute to either assign distinct names to different commands via the Name property or Ignore the command via the IsIgnored param." );
                }
            }

            /// <summary>
            /// Check and throw if any unsupported namedparam is provided
            /// (can be a mispelling of a named param)
            /// </summary>
            protected void CheckThrowNamedParamsExist( ComplexParam param, Type target, ParameterDefinition[] paramsDef )
            {
                var longNames = paramsDef.Where( l => !String.IsNullOrWhiteSpace( l.Name ) )
                    .Select( l => l.Name.ToLower() );

                var availableParamNames = longNames
                    .Where( i => !String.IsNullOrWhiteSpace( i ) );

                var providedParams = param.SubParams.Where( l => !String.IsNullOrWhiteSpace( l.Name ) )
                    .Select( p => p.Name.ToLower() );

                foreach( var providedParam in providedParams )
                {
                    var isCorrectParam = availableParamNames.Contains( providedParam );
                    if( !isCorrectParam )
                        throw new UndefinedParameterException( target, providedParam );
                }
            }

            protected void CheckThrowNamedParamsOrder( ComplexParam param )
            {
                int lastNamedParamIndex = -1;
                int lastNonNamedParamIndex = -1;

                int i = 0;
                foreach( var subparam in param.SubParams )
                {
                    if( String.IsNullOrWhiteSpace( subparam.Name ) )
                        lastNonNamedParamIndex = i;
                    else
                        lastNamedParamIndex = i;

                    i++;
                }

                if( (lastNamedParamIndex > -1 && lastNonNamedParamIndex > -1) &&
                        lastNonNamedParamIndex > lastNamedParamIndex )
                {
                    throw new ArgumentException( "Named parameters must appear after all non-named parameters" );
                }
            }

            protected void CheckThrowParamNumber( ParsedCommand param, ParameterDefinition[] paramsDef )
            {
                var paramDef = paramsDef.FirstOrDefault( p => p.Name.ToLower() == param.Name.ToLower() );

                int requiredParams = paramDef.SubParams.Count( cmdi => cmdi.Options.IsRequired );
                int paramsCount = paramDef.SubParams.Length;

                //implicit set for booleans
                if( param.Param == null && requiredParams > 0 && paramDef.Type != typeof( bool ) )
                {
                    string errorMsg = $"Wrong number of parameters for command " +
                     $"'{String.Join( ",", param.Name )}'";

                    throw new ArgumentException( errorMsg );
                }

                if( param.Param is SimpleParam )
                {

                }
                else if( param.Param is ComplexParam cp )
                {
                    if( cp.SubParams.Length < requiredParams ||
                        cp.SubParams.Length > paramsCount )
                    {
                        string errorMsg = $"Wrong number of parameters for command " +
                            $"'{String.Join( ",", param.Name )}'";

                        throw new ArgumentException( errorMsg );
                    }
                }
            }
        }

        private class ComplexParamSpecificChecks
        {
            public void Checks( ComplexParam param, Type target, ParameterDefinition[] paramsDef )
            {
                CheckThrowNameCollisions( target, paramsDef );
                CheckThrowParamNumber( param, paramsDef );
                CheckThrowNamedParamsExist( param, target, paramsDef );
                CheckThrowNamedParamsOrder( param );
            }

            /// <summary>
            /// Check and throw if any namedparam is used more than once
            /// (automatic assigned names and explicitly assigned names are checked to be unique)
            /// </summary>
            private static void CheckThrowNameCollisions( Type target, ParameterDefinition[] paramsDef )
            {
                var nameCollisions = paramsDef.GroupBy( param => param.Name.ToLower() )
                    .Where( group => group.Count() > 1 );

                foreach( var collision in nameCollisions )
                {
                    //overload checking is possible but i don't think it's worth the complexity

                    throw new ArgumentException( $"Type '{target}' defines multiple commands named '{collision.Key}'. " +
                        "Please use the Option attribute to either assign distinct names to different commands via the Name property or Ignore the command via the IsIgnored param." );
                }
            }

            /// <summary>
            /// Check and throw if any unsupported namedparam is provided
            /// (can be a mispelling of a named param)
            /// </summary>
            protected void CheckThrowNamedParamsExist( ComplexParam param, Type target, ParameterDefinition[] paramsDef )
            {
                var longNames = paramsDef.Where( l => !String.IsNullOrWhiteSpace( l.Name ) )
                    .Select( l => l.Name.ToLower() );

                var availableParamNames = longNames
                    .Where( i => !String.IsNullOrWhiteSpace( i ) );

                var providedParams = param.SubParams.Where( l => !String.IsNullOrWhiteSpace( l.Name ) )
                    .Select( p => p.Name.ToLower() );

                foreach( var providedParam in providedParams )
                {
                    var isCorrectParam = availableParamNames.Contains( providedParam );
                    if( !isCorrectParam )
                        throw new UndefinedParameterException( target, providedParam );
                }
            }

            protected void CheckThrowNamedParamsOrder( ComplexParam param )
            {
                int lastNamedParamIndex = -1;
                int lastNonNamedParamIndex = -1;

                int i = 0;
                foreach( var subparam in param.SubParams )
                {
                    if( String.IsNullOrWhiteSpace( subparam.Name ) )
                        lastNonNamedParamIndex = i;
                    else
                        lastNamedParamIndex = i;

                    i++;
                }

                if( (lastNamedParamIndex > -1 && lastNonNamedParamIndex > -1) &&
                        lastNonNamedParamIndex > lastNamedParamIndex )
                {
                    throw new ArgumentException( "Named parameters must appear after all non-named parameters" );
                }
            }

            protected void CheckThrowParamNumber( ComplexParam param, ParameterDefinition[] paramsDef )
            {
                int requiredParams = paramsDef.Count( cmdi => cmdi.Options?.IsRequired ?? false );

                if( param.SubParams.Length < requiredParams ||
                    param.SubParams.Length > paramsDef.Length )
                {
                    string errorMsg = $"Wrong number of parameters for command " +
                        $"'{String.Join( ",", param.Name )}'";

                    throw new ArgumentException( errorMsg );
                }
            }
        }
    }
}
