using System;
using System.Linq;
using UltraMapper.CommandLine.Extensions;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.MappingExpressionBuilders;
using UltraMapper.Parsing.Extensions;

namespace UltraMapper.CommandLine
{
    public class CommandLine
    {
        public ICommandLineParser Parser { get; private set; }
        public IHelpProvider HelpProvider { get; private set; }
        public ParsedParametersAdapter ParamsAdapter { get; private set; }

        public static CommandLine Instance = new CommandLine( new DefaultParser() );

        public readonly Mapper Mapper = new Mapper();

        public CommandLine( ICommandLineParser parser )
            : this( parser, new DefaultHelpProvider() ) { }

        public CommandLine( ICommandLineParser parser,
            IHelpProvider helpProvider )
        {
            this.ParamsAdapter = new ParsedParametersAdapter();

            this.Parser = parser;
            this.HelpProvider = helpProvider;

            this.InitializeMapper( helpProvider );
        }

        public void InitializeMapper( IHelpProvider helpProvider )
        {
            this.Mapper.MappingConfiguration.IsReferenceTrackingEnabled = false;
            this.Mapper.MappingConfiguration.ReferenceBehavior = ReferenceBehaviors.CREATE_NEW_INSTANCE;

            int index = this.Mapper.MappingConfiguration.Mappers.FindIndex( m => m is ReferenceMapper );

            this.Mapper.MappingConfiguration.Mappers.InsertRange( index, new IMappingExpressionBuilder[]
            {
                new ParsedCommandsExpressionBuilder( this.Mapper.MappingConfiguration ),
                new ParsedCommandExpressionBuilder( this.Mapper.MappingConfiguration, helpProvider ),
                new ArrayParamExpressionBuilder( this.Mapper.MappingConfiguration ),
                new ComplexParamExpressionBuilder( this.Mapper.MappingConfiguration ){ CanMapByIndex = true },
                new SimpleParamExpressionBuilder( this.Mapper.MappingConfiguration )
            } );

            //TODO: converters and multiple converters
            //try
            //{

            //    _mapper.MappingConfiguration.MapTypes<SimpleParam, object>()
            //        .MapMember( source => source.Value, target => target );
            //}
            //catch( Exception ex)
            //{
            //}

            //TODO
            //_mapper.MappingConfiguration.MapTypes<string, bool>( str => IntToBoolConverter( str ) );

            //bool IntToBoolConverter( string str )
            //{
            //    str = str.Trim();

            //    if( String.Compare( str.Trim(), Boolean.FalseString, true ) == 0 ) return false;
            //    if( String.Compare( str.Trim(), Boolean.TrueString, true ) == 0 ) return true;

            //    if( str == "0" ) return false;
            //    if( str == "1" ) return true;

            //    throw new FormatException();
            //}
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
                return instance;

            this.HelpProvider.Initialize( typeof( T ) );

            var commands = this.Parser.Parse( str ).ToList();
            commands = this.ParamsAdapter.Adapt<T>( commands ).ToList();

            this.Mapper.Map( commands, instance );
            return instance;
        }
    }
}