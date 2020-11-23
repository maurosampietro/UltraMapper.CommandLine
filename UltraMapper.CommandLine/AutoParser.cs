using CommandLine.AutoParser.Mappers;
using CommandLine.AutoParser.Parsers;
using CommandLine.AutoParser.UltraMapper.Extensions;
using System;

namespace CommandLine.AutoParser
{
    public class AutoParser
    {
        public ICommandParser Parser { get; private set; }
        public IMapper Mapper { get; private set; }
        public IHelpProvider HelpProvider { get; private set; }
        public ParsedParametersAdapter ParamsAdapter { get; private set; }

        public static AutoParser Instance = new AutoParser(
            new CommandParser(), new UltraMapperBinding() );

        public AutoParser( ICommandParser parser, IMapper mapper )
            : this( parser, mapper, new DefaultHelpProvider() ) { }

        public AutoParser( ICommandParser parser, IMapper mapper,
            IHelpProvider helpProvider )
        {
            this.ParamsAdapter = new ParsedParametersAdapter();

            this.Parser = parser;
            this.Mapper = mapper;
            this.HelpProvider = helpProvider;

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