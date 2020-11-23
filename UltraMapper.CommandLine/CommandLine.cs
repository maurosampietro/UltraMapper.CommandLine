using UltraMapper.CommandLine.Mappers;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.CommandLine.Extensions;
using System;

namespace UltraMapper.CommandLine
{
    public class CommandLine
    {
        public ICommandParser Parser { get; private set; }
        public IMapper Mapper { get; private set; }
        public IHelpProvider HelpProvider { get; private set; }
        public ParsedParametersAdapter ParamsAdapter { get; private set; }

        public static CommandLine Instance = new CommandLine( new CommandParser() );

        public CommandLine( ICommandParser parser )
            : this( parser, new DefaultHelpProvider() ) { }

        public CommandLine( ICommandParser parser,
            IHelpProvider helpProvider )
        {
            this.ParamsAdapter = new ParsedParametersAdapter();

            this.Parser = parser;
            this.HelpProvider = helpProvider;

            this.Mapper = new UltraMapperBinding();
            this.Mapper.Initialize( helpProvider );
        }

        public T Parse<T>( string[] args ) where T : class, new()
        {
            return this.Parse<T>( String.Join( " ", args ) );
        }

        public T Parse<T>( string str ) where T : class, new()
        {
            if( String.IsNullOrWhiteSpace( str ) )
                return default;

            this.HelpProvider.Initialize( typeof( T ) );

            var commands = this.Parser.Parse( str );
            commands = this.ParamsAdapter.AdaptParsedCommandsToTargetType<T>( commands );

            return this.Mapper.Map<T>( commands );
        }
    }
}