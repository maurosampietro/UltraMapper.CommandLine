using System;
using System.Collections.Generic;

namespace UltraMapper.CommandLine.Mappers.Internals
{
    internal static class StringExtensions
    {
        internal static IEnumerable<string> SplitKeepDelimiter( this string str, string separator )
        {
            var indexes = new List<int>();
            int startIndex = 0;
            while( startIndex < str.Length )
            {
                int lastFoundIndex = str.IndexOf( separator, startIndex );
                if( lastFoundIndex == -1 ) break;

                indexes.Add( lastFoundIndex );
                startIndex = lastFoundIndex + separator.Length;
            }

            if( indexes.Count == 0 ) yield break;

            startIndex = 0;
            foreach( var index in indexes )
            {
                if( index - startIndex > 0 )
                    yield return str.Substring( startIndex, index - startIndex );

                startIndex = index;
            }

            if( startIndex < str.Length )
                yield return str.Substring( startIndex );
        }

        public static IEnumerable<int> IndexesOf( this string str, string subStr )
        {
            if( String.IsNullOrEmpty( subStr ) )
                throw new ArgumentNullException( nameof( subStr ), "The string to find should not be null or empty" );

            int startIndex = 0;
            int pos = str.IndexOf( subStr, startIndex );

            while( pos != -1 )
            {
                yield return pos;
                startIndex = pos + subStr.Length;
                pos = str.IndexOf( subStr, startIndex );
            }
        }
    }
}
