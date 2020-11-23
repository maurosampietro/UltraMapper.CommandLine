using System;
using System.Linq;

namespace UltraMapper.CommandLine
{
    public static class TypeExtensions
    {
        /// <summary>
        /// If a type is generic, gets a prettified name.
        /// If a type is not generic returns its name (type.Name)
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Prettified name of the type</returns>
        public static string GetPrettifiedName( this Type type )
        {
            if( type.IsGenericType )
            {
                var mainType = type.Name.Substring( 0, type.Name.IndexOf( '`' ) );
                var genericArgs = type.GenericTypeArguments.Select( GetPrettifiedName );

                return $"{mainType}<{String.Join( ", ", genericArgs )}>";
            }

            return type.Name;
        }
    }
}
