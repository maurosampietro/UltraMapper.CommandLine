using System;
using System.Linq;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.Internals;
using UltraMapper.Parsing;

namespace UltraMapper.CommandLine
{
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
            if(!commandDefs.Any( p => p.Name.ToLower() == command.Name.ToLower() ))
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

            foreach(var collision in nameCollisions)
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

            if(!String.IsNullOrEmpty( command.Param?.Name ))
            {
                var isCorrectParam = availableParamNames.Contains( command.Param.Name );
                if(!isCorrectParam)
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
            if(command.Param is ComplexParam cp)
                _complexTypeChecks.CheckThrowNamedParamsOrder( cp );
        }

        protected void CheckThrowParamNumber( ParsedCommand param, ParameterDefinition[] paramsDef )
        {
            var paramDef = paramsDef.FirstOrDefault( p => p.Name.ToLower() == param.Name.ToLower() );

            int requiredParams = paramDef.SubParams.Count( cmdi => cmdi.Options?.IsRequired ?? false );
            int paramsCount = paramDef.SubParams.Length;

            //implicit set for booleans
            if(param.Param == null && requiredParams > 0 && paramDef.Type != typeof( bool ))
            {
                throw new ArgumentNumberException( param );
            }
            else if(param.Param is SimpleParam sp)
            {
                if(paramDef.MemberType == MemberTypes.METHOD)
                {
                    if(requiredParams > 1)
                        throw new ArgumentNumberException( param );

                    if(paramsCount == 0)
                        throw new ArgumentNumberException( param );
                }
                else
                {
                    if(requiredParams > 1)
                        throw new ArgumentNumberException( param );
                }
            }
            else if(param.Param is ComplexParam cp)
            {
                var tempDef = paramDef.SubParams?.FirstOrDefault();
                if(paramDef.SubParams.Length == 1 && tempDef != null && !tempDef.Type.IsBuiltIn( false ))
                {
                    requiredParams = tempDef.SubParams
                        .Count( cmdi => cmdi.Options?.IsRequired ?? false );

                    if(cp.SubParams.Count < requiredParams ||
                        cp.SubParams.Count > tempDef.SubParams.Length)
                    {
                        throw new ArgumentNumberException( param );
                    }
                }
                else
                {
                    if(cp.SubParams.Count < requiredParams ||
                        cp.SubParams.Count > paramsCount)
                    {
                        throw new ArgumentNumberException( param );
                    }
                }
            }
        }
    }
}
