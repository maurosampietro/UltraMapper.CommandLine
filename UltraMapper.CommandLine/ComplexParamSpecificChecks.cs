using System;
using System.Linq;
using UltraMapper.Parsing;

namespace UltraMapper.CommandLine
{
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

            foreach(var item in groups)
            {
                if(item.Count() > 1)
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

            foreach(var collision in nameCollisions)
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

            foreach(var providedParam in providedParams)
            {
                var isCorrectParam = availableParamNames.Contains( providedParam );
                if(!isCorrectParam)
                    throw new UndefinedParameterException( target, providedParam );
            }
        }

        public void CheckThrowNamedParamsOrder( ComplexParam param )
        {
            int lastNamedParamIndex = -1;
            int lastNonNamedParamIndex = -1;

            int i = 0;
            foreach(var subparam in param.SubParams)
            {
                if(String.IsNullOrWhiteSpace( subparam.Name ))
                    lastNonNamedParamIndex = i;
                else
                    lastNamedParamIndex = i;

                i++;
            }

            if((lastNamedParamIndex > -1 && lastNonNamedParamIndex > -1) &&
                    lastNonNamedParamIndex > lastNamedParamIndex)
            {
                throw new MisplacedNamedParamException( param );
            }
        }

        protected void CheckThrowParamNumber( ComplexParam param, ParameterDefinition[] paramsDef )
        {
            int requiredParams = paramsDef.Count( cmdi => cmdi.Options?.IsRequired ?? false );

            if(param.SubParams.Count < requiredParams ||
                param.SubParams.Count > paramsDef.Length)
            {
                throw new ArgumentNumberException( param );
            }
        }
    }
}
