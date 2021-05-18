using System;
using System.Globalization;
using System.Linq;
using UltraMapper.CommandLine.Extensions;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.Conventions;
using UltraMapper.MappingExpressionBuilders;
using UltraMapper.Parsing.Extensions;

namespace UltraMapper.CommandLine
{
    public class CommandLine
    {
        public static CommandLine Instance = new( new DefaultParser() );

        public readonly ICommandLineParser Parser;
        public readonly IHelpProvider HelpProvider;
        public readonly Mapper Mapper;
        private readonly ParsedParametersAdapter ParamsAdapter;

        private CultureInfo _cultureInfo = null;
        public CultureInfo CultureInfo
        {
            get => _cultureInfo ?? CultureInfo.CurrentCulture;
            set => _cultureInfo = value;
        }

        public CommandLine( ICommandLineParser parser )
            : this( parser, new DefaultHelpProvider() ) { }

        public CommandLine( ICommandLineParser parser,
            IHelpProvider helpProvider )
        {
            this.ParamsAdapter = new ParsedParametersAdapter();

            this.Parser = parser;
            this.HelpProvider = helpProvider;

            this.Mapper = new Mapper( cfg =>
            {
                cfg.IsReferenceTrackingEnabled = false;
                cfg.ReferenceBehavior = ReferenceBehaviors.CREATE_NEW_INSTANCE;

                cfg.Conventions.GetOrAdd<DefaultConvention>( rule =>
                {
                    rule.SourceMemberProvider.IgnoreFields = true;
                    rule.SourceMemberProvider.IgnoreMethods = true;
                    rule.SourceMemberProvider.IgnoreNonPublicMembers = true;

                    rule.TargetMemberProvider.IgnoreFields = true;
                    rule.TargetMemberProvider.IgnoreMethods = true;
                    rule.TargetMemberProvider.IgnoreNonPublicMembers = true;
                } );

                cfg.Mappers.AddBefore<ReferenceMapper>( new IMappingExpressionBuilder[]
                {
                    new ParsedCommandsExpressionBuilder( cfg ),
                    new ParsedCommandMapper( cfg, helpProvider ),
                    new ArrayParamExpressionBuilder( cfg),
                    new ComplexParamExpressionBuilder( cfg ){ CanMapByIndex = true },
                    new SimpleParamExpressionBuilder( cfg )
                } );

                cfg.MapTypes<string, double>( str => ConvertStringToDouble( str ) );
                cfg.MapTypes<string, bool>( str => ConvertStringToBoolean( str ) );
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
        }

        public T Parse<T>( string str ) where T : class, new()
        {
            return this.Parse( str, new T() );
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

        /// <summary>
        /// Specific for arguments passed from cmd.
        /// (Quotes are preprocessed by the operating system)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public T Parse<T>( string[] args ) where T : class, new()
        {
            return this.Parse( args, new T() );
        }

        /// <summary>
        /// Specific for arguments passed from cmd
        /// (Quotes are preprocessed by the operating system)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public T Parse<T>( string[] args, T instance ) where T : class
        {
            if( args == null || args.Length == 0 )
                return instance;

            this.HelpProvider.Initialize( typeof( T ) );

            var commands = this.Parser.Parse( args ).ToList();
            commands = this.ParamsAdapter.Adapt<T>( commands ).ToList();

            this.Mapper.Map( commands, instance );
            return instance;
        }

        private double ConvertStringToDouble( string str )
        {
            if( String.IsNullOrWhiteSpace( str ) )
                return 0.0;

            return Double.Parse( str, NumberStyles.Any, CultureInfo );
        }

        private bool ConvertStringToBoolean( string str )
        {
            if( str == "1" ) return true;
            if( str == "0" ) return false;

            return Boolean.Parse( str );
        }
    }
}