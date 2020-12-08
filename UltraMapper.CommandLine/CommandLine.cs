using System;
using UltraMapper.CommandLine.Extensions;
using UltraMapper.CommandLine.Mappers;
using UltraMapper.CommandLine.Parsers;

namespace UltraMapper.CommandLine
{
    public class CommandLine
    {
        public ICommandParser Parser { get; private set; }
        public IMapper Mapper { get; private set; }
        public IHelpProvider HelpProvider { get; private set; }
        public ParsedParametersAdapter ParamsAdapter { get; private set; }

        public static CommandLine Instance = new CommandLine( new DefaultParser() );

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

        public T Parse<T>( string str ) where T : class, new()
        {
            return this.Parse( str, new T() );
        }

        public T Parse<T>( string[] args ) where T : class, new()
        {
            return this.Parse( String.Join( " ", args ), new T() );
        }

        public T Parse<T>( string[] args, T instance ) where T : class
        {
            return this.Parse( String.Join( " ", args ), instance );
        }

        public T Parse<T>( string str, T instance ) where T : class
        {
            if( String.IsNullOrWhiteSpace( str ) )
                return default;

            this.HelpProvider.Initialize( typeof( T ) );

            var commands = this.Parser.Parse( str );
            commands = this.ParamsAdapter.AdaptParsedCommandsToTargetType<T>( commands );

            return this.Mapper.Map( commands, instance );
        }
    }
}