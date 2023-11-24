using System;
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

            foreach(var command in commands)
            {
                //check the command first, then the params
                _commandChecks.Checks( command, typeof( T ), definition );

                //adapt
                var def = definition.First( d => d.Name.ToLower() == command.Name.ToLower() );
                command.Param = Internal( command.Param, def );
                command.Param = InternalOptionalMethodParams( command.Param, def );

                if(command.Param != null)
                {
                    //named command with only 1 nameless param: the name of the param is the name of the command
                    if(def.SubParams.Length <= 1 && String.IsNullOrEmpty( command.Param.Name ))
                        command.Param.Name = command.Name;
                }

                yield return command;
            }
        }

        private IParsedParam InternalOptionalMethodParams( IParsedParam param, ParameterDefinition def )
        {
            if(def.MemberType != MemberTypes.METHOD) return param;

            ResolveNameOfUnnamedParams( param, def );

            var optionalParams = def.SubParams.Where( p => !p.Options.IsRequired ).ToList();

            if(optionalParams.Count == 0)
                return param;

            var requiredParams = def.SubParams.Except( optionalParams );

            if(param is not ComplexParam)
            {
                var newCp = new ComplexParam();

                if(param != null)
                {
                    if(param is SimpleParam paramSp)
                        newCp.Simple.Add( paramSp );
                    else if(param is ArrayParam arrayp)
                        newCp.Array.Add( arrayp );
                }

                for(int paramIndex = 0; paramIndex < def.SubParams.Length; paramIndex++)
                {
                    var sub = def.SubParams[ paramIndex ];

                    if(param?.Name == sub.Name)
                        continue;

                    //if( param?.Index == sub.Options.Order )
                    //    continue;

                    if(!sub.Options.IsRequired)
                    {
                        if(sub.Type.IsBuiltIn( false ))
                            newCp.Simple.Add( new SimpleParam() { Name = sub.Name, Index = sub.Options.Order, Value = sub.DefaultValue?.ToString() } );
                        else if(sub.Type.IsEnumerable())
                            newCp.Array.Add( new ArrayParam() { Name = sub.Name, Index = sub.Options.Order } );
                        else
                            newCp.Complex.Add( new ComplexParam() { Name = sub.Name, Index = sub.Options.Order } );
                    }
                }

                return newCp;
            }
            else if(((ComplexParam)param).SubParams.Count < def.SubParams.Length)
            {
                var paramsSubs = new List<IParsedParam>();
                paramsSubs.AddRange( ((ComplexParam)param).SubParams );

                for(int paramIndex = 0; paramIndex < def.SubParams.Length; paramIndex++)
                {
                    var defSub = def.SubParams[ paramIndex ];

                    if(paramsSubs.Any( s => s.Name == defSub.Name ))
                        continue;

                    //if( paramIndex < paramsSubs.Count &&
                    //    paramsSubs[ paramIndex ].Index == defSub.Options.Order )
                    //    continue;

                    if(!defSub.Options.IsRequired)
                    {
                        if(defSub.Type.IsBuiltIn( false ))
                            paramsSubs.Add( new SimpleParam() { Name = defSub.Name, Index = defSub.Options.Order, Value = defSub.DefaultValue?.ToString() } );
                        else if(defSub.Type.IsEnumerable())
                            paramsSubs.Add( new ArrayParam() { Name = defSub.Name, Index = defSub.Options.Order } );
                        else
                            paramsSubs.Add( new ComplexParam() { Name = defSub.Name, Index = defSub.Options.Order } );
                    }
                }

                var newcp = new ComplexParam();
                foreach(var item in paramsSubs)
                {
                    if(item is ComplexParam cp)
                        newcp.Complex.Add( cp );
                    else if(item is SimpleParam sp)
                        newcp.Simple.Add( sp );
                    else if(item is ArrayParam ap)
                        newcp.Array.Add( ap );
                }

                return newcp;
            }

            return param;
        }

        private IParsedParam Internal( IParsedParam param, ParameterDefinition def )
        {
            if(param == null) return null;

            switch(param)
            {
                case ArrayParam _: return param;

                case SimpleParam sp:

                    if(def.Type?.IsBuiltIn( true ) == true || def.Type == typeof( DateTime ))
                        return sp;

                    var newcp = new ComplexParam()
                    {
                        Name = def.Name,
                        Index = 0,

                    };

                    newcp.Simple.Add( sp );

                    return newcp;

                case ComplexParam cp:
                {
                    var subParamsDef = def.SubParams;
                    if(subParamsDef.All( s => s.Type.IsBuiltIn( true ) ))
                    {
                        _complexParamChecks.Checks( cp, def.Type, def.SubParams );
                        return cp;
                    }

                    IEnumerable<IParsedParam> getSubparams( ComplexParam localcp )
                    {
                        for(int i = 0; i < localcp.SubParams.Count; i++)
                        {
                            var item = localcp.SubParams[ i ];

                            var defSub = def.SubParams.FirstOrDefault( k => k.Name.ToLower() == item.Name.ToLower() );
                            if(defSub == null)
                                defSub = def.SubParams.FirstOrDefault( k => k.Options.Order == item.Index );

                            var newitem = Internal( item, defSub );
                            yield return newitem;
                        }
                    };

                    if(def.SubParams.Length == 1)
                    {
                        //nesting just to check properly
                        var temp = new ComplexParam()
                        {
                            Name = "",
                        };

                        temp.Complex.Add( cp );

                        _complexParamChecks.Checks( temp, def.Type, def.SubParams );

                        var subparams = getSubparams( temp ).ToArray();
                        return subparams.First();
                    }
                    else
                    {
                        _complexParamChecks.Checks( cp, def.Type, def.SubParams );

                        var subparams = getSubparams( cp ).ToArray();

                        var newcp2 = new ComplexParam()
                        {
                            Name = def.Name,
                            Index = cp.Index,
                        };

                        foreach(var item in subparams)
                        {
                            if(item is ComplexParam cp2)
                                newcp2.Complex.Add( cp2 );
                            else if(item is SimpleParam sp)
                                newcp2.Simple.Add( sp );
                            else if(item is ArrayParam ap)
                                newcp2.Array.Add( ap );
                        }

                        return newcp2;
                    }
                }
            }

            throw new NotSupportedException();
        }

        private void ResolveNameOfUnnamedParams( IParsedParam param, ParameterDefinition def )
        {
            //add name to unnamed params
            if(param is ComplexParam cp)
            {
                for(int i = 0; i < cp.SubParams.Count && i < def.SubParams.Length; i++)
                {
                    var subParam = cp.SubParams[ i ];
                    var subDef = def.SubParams.OrderBy( f => f.Options.Order ).ToArray()[ i ];
                    if(subDef.Type.IsBuiltIn( true ) || subDef.Type.IsEnumerable())
                    {
                        if(String.IsNullOrWhiteSpace( subParam.Name ))
                            subParam.Name = subDef.Name;
                    }
                    else
                        ResolveNameOfUnnamedParams( subParam, subDef );
                }
            }
            else if(param is ArrayParam ap)
            {
                var subDef = def.SubParams.OrderBy( f => f.Options.Order ).ToArray()[ 0 ];
                if(String.IsNullOrWhiteSpace( param.Name ))
                    param.Name = subDef.Name;
            }
        }
    }
}
