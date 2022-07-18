using System.Collections.Generic;
using System.Linq;
using System;

namespace UltraMapper.CommandLine.Internals
{
    internal static class StringExtensions
    {
        internal static IEnumerable<string> SplitKeepDelimiter( this string str, string separator,
            string subObjStartChars = null, string subObjEndChars = null, char quoteChar = '\"' )
        {
            if( subObjStartChars.Any( ch => subObjEndChars.Contains( ch ) ) )
                throw new ArgumentException( $"{nameof( subObjStartChars )} and {nameof( subObjEndChars )} must have no char in common" );

            //find possible separators indentifying a command
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

            //exclude command identifiers which are quoted or contained in parameter values
            int subParamsCount = 0;
            int startParam = -1;
            int startQuotation = -1;

            if( !String.IsNullOrWhiteSpace( subObjStartChars )
                && !String.IsNullOrWhiteSpace( subObjEndChars ) )
            {
                bool isQuotation = false;

                for( int i = 0; i < str.Length; i++ )
                {
                    //non-escaped quote char is found
                    if( str[ i ] == quoteChar && i > 0 && str[ i - 1 ] != '\\' )
                    {
                        isQuotation = !isQuotation;
                        if( isQuotation )
                            startQuotation = i;
                        else
                        {
                            for( int k = 0; k < indexes.Count; k++ )
                            {
                                if( indexes[ k ] >= startQuotation && indexes[ k ] <= i )
                                {
                                    indexes.RemoveAt( k );
                                    k--;
                                }
                            }

                            startQuotation = -1;
                        }
                    }
                    else if( subObjStartChars.Contains( str[ i ] ) )
                    {
                        if( startParam == -1 )
                            startParam = i;
                        subParamsCount++;
                    }
                    else if( subObjEndChars.Contains( str[ i ] ) )
                    {
                        subParamsCount--;
                        if( subParamsCount == 0 )
                        {
                            for( int k = 0; k < indexes.Count; k++ )
                            {
                                if( indexes[ k ] >= startParam && indexes[ k ] <= i )
                                {
                                    indexes.RemoveAt( k );
                                    k--;
                                }
                            }

                            startParam = -1;
                        }
                    }
                }
            }

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

    }
}
