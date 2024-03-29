﻿using System;
using System.Collections.Generic;
using System.Linq;
using UltraMapper.CommandLine.Mappers;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.Internals;
using UltraMapper.Parsing;

namespace UltraMapper.CommandLine
{
    /// <summary>
    /// The parser does not know about the structure of the target type T. 
    ///
    /// An adapter is needed to prepare the args for the mapping process by manipulating
    /// them so that they conform to the structure ot the target type T.
    /// 
    /// If a SimpleParam maps to a non-primitive type then we need to encapsulate the SimpleParam in a ComplexParam.
    /// </summary>
    public class ParsedParametersAdapter
    {
        private static readonly ParsedCommandSpecificChecks _commandChecks = new ParsedCommandSpecificChecks();
        private static readonly ComplexParamSpecificChecks _complexParamChecks = new ComplexParamSpecificChecks();

        public IEnumerable<ParsedCommand> Adapt<T>( IEnumerable<ParsedCommand> commands )
        {
            var definition = DefinitionHelper.GetCommandDefinitions( typeof( T ) ).ToArray();

            foreach( var command in commands )
            {
                //check the command first, then the params
                _commandChecks.Checks( command, typeof( T ), definition );

                //adapt
                var def = definition.First( d => d.Name.ToLower() == command.Name.ToLower() );
                command.Param = Internal( command.Param, def );
                command.Param = InternalOptionalMethodParams( command.Param, def );

                if( command.Param != null )
                {
                    //named command with only 1 nameless param: the name of the param is the name of the command
                    if( def.SubParams.Length <= 1 && String.IsNullOrEmpty( command.Param.Name ) )
                        command.Param.Name = command.Name;
                }

                yield return command;
            }
        }

        private IParsedParam InternalOptionalMethodParams( IParsedParam param, ParameterDefinition def )
        {
            if( def.MemberType != MemberTypes.METHOD ) return param;

            ResolveNameOfUnnamedParams( param, def );

            var optionalParams = def.SubParams.Where( p => !p.Options.IsRequired ).ToList();

            if( optionalParams.Count == 0 )
                return param;

            var requiredParams = def.SubParams.Except( optionalParams );

            if( !(param is ComplexParam) )
            {
                var subs = new List<IParsedParam>();


                if( param != null )
                    subs.Add( param );

                for( int paramIndex = 0; paramIndex < def.SubParams.Length; paramIndex++ )
                {
                    var sub = def.SubParams[ paramIndex ];

                    if( param?.Name == sub.Name )
                        continue;

                    //if( param?.Index == sub.Options.Order )
                    //    continue;

                    if( !sub.Options.IsRequired )
                    {
                        if( sub.Type.IsBuiltIn( false ) )
                            subs.Add( new SimpleParam() { Name = sub.Name, Index = sub.Options.Order, Value = sub.DefaultValue?.ToString() } );
                        else if( sub.Type.IsEnumerable() )
                            subs.Add( new ArrayParam() { Name = sub.Name, Index = sub.Options.Order } );
                        else
                            subs.Add( new ComplexParam() { Name = sub.Name, Index = sub.Options.Order } );
                    }
                }

                return new ComplexParam()
                {
                    SubParams = subs.ToArray()
                };
            }
            else if( ((ComplexParam)param).SubParams.Count < def.SubParams.Length )
            {
                var paramsSubs = new List<IParsedParam>();
                paramsSubs.AddRange( ((ComplexParam)param).SubParams );

                for( int paramIndex = 0; paramIndex < def.SubParams.Length; paramIndex++ )
                {
                    var defSub = def.SubParams[ paramIndex ];

                    if( paramsSubs.Any( s => s.Name == defSub.Name ) )
                        continue;

                    //if( paramIndex < paramsSubs.Count &&
                    //    paramsSubs[ paramIndex ].Index == defSub.Options.Order )
                    //    continue;

                    if( !defSub.Options.IsRequired )
                    {
                        if( defSub.Type.IsBuiltIn( false ) )
                            paramsSubs.Add( new SimpleParam() { Name = defSub.Name, Index = defSub.Options.Order, Value = defSub.DefaultValue?.ToString() } );
                        else if( defSub.Type.IsEnumerable() )
                            paramsSubs.Add( new ArrayParam() { Name = defSub.Name, Index = defSub.Options.Order } );
                        else
                            paramsSubs.Add( new ComplexParam() { Name = defSub.Name, Index = defSub.Options.Order } );
                    }
                }

                return new ComplexParam()
                {
                    SubParams = paramsSubs.ToArray()
                };
            }

            return param;
        }

        private static void ResolveNameOfUnnamedParams( IParsedParam param, ParameterDefinition def )
        {
            //add name to unnamed params
            if( param is ComplexParam cp )
            {
                for( int i = 0; i < cp.SubParams.Count && i < def.SubParams.Length; i++ )
                {
                    var subParam = cp.SubParams[ i ];
                    var subDef = def.SubParams.OrderBy( f => f.Options.Order ).ToArray()[ i ];
                    if( subDef.Type.IsBuiltIn( true ) || subDef.Type.IsEnumerable() )
                    {
                        if( String.IsNullOrWhiteSpace( subParam.Name ) )
                            subParam.Name = subDef.Name;
                    }
                    else
                        ResolveNameOfUnnamedParams( subParam, subDef );
                }
            }
            else if( param is ArrayParam ap )
            {
                var subDef = def.SubParams.OrderBy( f => f.Options.Order ).ToArray()[ 0 ];
                if( String.IsNullOrWhiteSpace( param.Name ) )
                    param.Name = subDef.Name;
            }
        }

        private IParsedParam Internal( IParsedParam param, ParameterDefinition def )
        {
            if( param == null ) return null;

            switch( param )
            {
                case ArrayParam _: return param;

                case SimpleParam sp:

                    if( def.Type?.IsBuiltIn( true ) == true )
                        return sp;

                    return new ComplexParam()
                    {
                        Name = def.Name,
                        Index = 0,
                        SubParams = new[] { sp }
                    };

                case ComplexParam cp:
                {
                    var subParamsDef = def.SubParams;
                    if( subParamsDef.All( s => s.Type.IsBuiltIn( true ) ) )
                    {
                        _complexParamChecks.Checks( cp, def.Type, def.SubParams );
                        return cp;
                    }

                    IEnumerable<IParsedParam> getSubparams( ComplexParam localcp )
                    {
                        for( int i = 0; i < localcp.SubParams.Count; i++ )
                        {
                            var item = localcp.SubParams[ i ];

                            var defSub = def.SubParams.FirstOrDefault( k => k.Name.ToLower() == item.Name.ToLower() );
                            if( defSub == null )
                                defSub = def.SubParams.FirstOrDefault( k => k.Options.Order == item.Index );

                            var newitem = Internal( item, defSub );
                            yield return newitem;
                        }
                    };

                    if( def.SubParams.Length == 1 )
                    {
                        //nesting just to check properly
                        var temp = new ComplexParam()
                        {
                            Name = "",
                            SubParams = new[] { cp }
                        };

                        _complexParamChecks.Checks( temp, def.Type, def.SubParams );

                        var subparams = getSubparams( temp ).ToArray();
                        return subparams.First();
                    }
                    else
                    {
                        _complexParamChecks.Checks( cp, def.Type, def.SubParams );

                        var subparams = getSubparams( cp ).ToArray();

                        return new ComplexParam()
                        {
                            Name = def.Name,
                            Index = 0,
                            SubParams = subparams
                        };
                    }
                }
            }

            throw new NotSupportedException();
        }
    }

    internal class ParsedCommandSpecificChecks
    {
        private readonly ComplexParamSpecificChecks _complexTypeChecks =
            new ComplexParamSpecificChecks();

        public void Checks( ParsedCommand param, Type target, ParameterDefinition[] paramsDef )
        {
            CheckThrowCommandExists( param, target, paramsDef );
            CheckThrowNameCollisions( param, target, paramsDef );
            CheckThrowParamNumber( param, paramsDef );
            CheckThrowNamedParamsExist( param, target, paramsDef );
            CheckThrowNamedParamsOrder( param );
            //CheckThrowParamsTypeOrder (if cp provided and sp is expected number is still ok but should throw)
        }

        public void CheckThrowCommandExists( ParsedCommand command, Type target, ParameterDefinition[] commandDefs )
        {
            if( !commandDefs.Any( p => p.Name.ToLower() == command.Name.ToLower() ) )
                throw new UndefinedCommandException( target, command.Name );
        }

        /// <summary>
        /// Check and throw if any namedparam is used more than once
        /// (automatic assigned names and explicitly assigned names are checked to be unique)
        /// </summary>
        private static void CheckThrowNameCollisions( ParsedCommand param, Type target, ParameterDefinition[] paramsDef )
        {
            var nameCollisions = paramsDef.GroupBy( p => p.Name.ToLower() )
                .Where( group => group.Count() > 1 );

            foreach( var collision in nameCollisions )
            {
                //overload checking is possible but i don't think it's worth the complexity
                throw new DuplicateCommandException( target, param );
            }
        }

        /// <summary>
        /// Check and throw if any provided namedparam does not exist in the definition of the command
        /// (the namedparam may be mispelled)
        /// </summary>
        protected void CheckThrowNamedParamsExist( ParsedCommand command, Type target, ParameterDefinition[] commandsDef )
        {
            var commandDef = commandsDef.First( pd => pd.Name.ToLower() == command.Name.ToLower() );

            var longNames = commandDef.SubParams
                .Where( l => !String.IsNullOrWhiteSpace( l.Name ) )
                .Select( l => l.Name.ToLower() );

            var availableParamNames = longNames
                .Where( i => !String.IsNullOrWhiteSpace( i ) );

            if( !String.IsNullOrEmpty( command.Param?.Name ) )
            {
                var isCorrectParam = availableParamNames.Contains( command.Param.Name );
                if( !isCorrectParam )
                    throw new UndefinedParameterException( target, command.Param.Name );
            }

            //if( command.Param is ComplexParam cp )
            //{
            //    var providedParams = cp.SubParams.Where( l => !String.IsNullOrWhiteSpace( l.Name ) )
            //        .Select( p => p.Name.ToLower() );

            //    foreach( var providedParam in providedParams )
            //    {
            //        var isCorrectParam = availableParamNames.Contains( providedParam );
            //        if( !isCorrectParam )
            //            throw new UndefinedParameterException( target, providedParam );
            //    }
            //}
            //else
            //if( command.Param != null )
            //{
            //    if( !String.IsNullOrWhiteSpace( command.Param.Name ) )
            //    {
            //        var isCorrectParam = availableParamNames.Contains( command.Param.Name.ToLower() );
            //        if( !isCorrectParam )
            //            throw new UndefinedParameterException( target, command.Param.Name );
            //    }
            //}
        }

        protected void CheckThrowNamedParamsOrder( ParsedCommand command )
        {
            if( command.Param is ComplexParam cp )
                _complexTypeChecks.CheckThrowNamedParamsOrder( cp );
        }

        protected void CheckThrowParamNumber( ParsedCommand param, ParameterDefinition[] paramsDef )
        {
            var paramDef = paramsDef.FirstOrDefault( p => p.Name.ToLower() == param.Name.ToLower() );

            int requiredParams = paramDef.SubParams.Count( cmdi => cmdi.Options?.IsRequired ?? false );
            int paramsCount = paramDef.SubParams.Length;

            //implicit set for booleans
            if( param.Param == null && requiredParams > 0 && paramDef.Type != typeof( bool ) )
            {
                throw new ArgumentNumberException( param );
            }
            else if( param.Param is SimpleParam sp )
            {
                if( paramDef.MemberType == MemberTypes.METHOD )
                {
                    if( requiredParams > 1 )
                        throw new ArgumentNumberException( param );

                    if( paramsCount == 0 )
                        throw new ArgumentNumberException( param );
                }
                else
                {
                    if( requiredParams > 1 )
                        throw new ArgumentNumberException( param );
                }
            }
            else if( param.Param is ComplexParam cp )
            {
                var tempDef = paramDef.SubParams?.FirstOrDefault();
                if( paramDef.SubParams.Length == 1 && tempDef != null && !tempDef.Type.IsBuiltIn( false ) )
                {
                    requiredParams = tempDef.SubParams
                        .Count( cmdi => cmdi.Options?.IsRequired ?? false );

                    if( cp.SubParams.Count < requiredParams ||
                        cp.SubParams.Count > tempDef.SubParams.Length )
                    {
                        throw new ArgumentNumberException( param );
                    }
                }
                else
                {
                    if( cp.SubParams.Count < requiredParams ||
                        cp.SubParams.Count > paramsCount )
                    {
                        throw new ArgumentNumberException( param );
                    }
                }
            }
        }
    }

    internal class ComplexParamSpecificChecks
    {
        public void Checks( ComplexParam param, Type target, ParameterDefinition[] paramsDef )
        {
            CheckThrowParameterNameCollisions( target, paramsDef );
            CheckThrowArgumentNameCollisions( param );

            CheckThrowParamNumber( param, paramsDef );
            CheckThrowNamedParamsExist( param, target, paramsDef );
            CheckThrowNamedParamsOrder( param );
            //CheckThrowParamsTypeOrder (if cp provided and sp is expected number is still ok but should throw)
        }

        private void CheckThrowArgumentNameCollisions( ComplexParam param )
        {
            //anonymous params (empty name) are not a problem
            var groups = param.SubParams.Where( p => !String.IsNullOrEmpty( p.Name ) )
                .GroupBy( p => p.Name.ToLower() );

            foreach( var item in groups )
            {
                if( item.Count() > 1 )
                    throw new DuplicateArgumentException( item.Key );
            }
        }

        /// <summary>
        /// Check and throw if any namedparam is used more than once
        /// (automatic assigned names and explicitly assigned names are checked to be unique)
        /// </summary>
        private static void CheckThrowParameterNameCollisions( Type target, ParameterDefinition[] paramsDef )
        {
            var nameCollisions = paramsDef.GroupBy( param => param.Name.ToLower() )
                .Where( group => group.Count() > 1 );

            foreach( var collision in nameCollisions )
            {
                //overload checking is possible but i don't think it's worth the complexity
                throw new DuplicateParameterException( collision.Key );
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

        public void CheckThrowNamedParamsOrder( ComplexParam param )
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
                throw new MisplacedNamedParamException( param );
            }
        }

        protected void CheckThrowParamNumber( ComplexParam param, ParameterDefinition[] paramsDef )
        {
            int requiredParams = paramsDef.Count( cmdi => cmdi.Options?.IsRequired ?? false );

            if( param.SubParams.Count < requiredParams ||
                param.SubParams.Count > paramsDef.Length )
            {
                throw new ArgumentNumberException( param );
            }
        }
    }
}
