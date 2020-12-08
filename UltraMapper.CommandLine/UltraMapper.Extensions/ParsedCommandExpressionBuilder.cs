using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.CommandLine.Extensions
{
    public class ParsedCommandExpressionBuilder : ReferenceMapper
    {
        private readonly IHelpProvider _helpProvider;
        private readonly Expression<Action<Type, IParsedParam>> _helperCall;

        public ParsedCommandExpressionBuilder( Configuration configuration, IHelpProvider helpProvider )
            : base( configuration )
        {
            _helpProvider = helpProvider;
            _helperCall = ( type, param ) => _helpProvider.GetHelp( type, param );
        }

        public override bool CanHandle( Type source, Type target )
        {
            return source == typeof( ParsedCommand ) &&
                target != typeof( ParsedCommand ); //allow cloning
        }

        public override LambdaExpression GetMappingExpression( Type source, Type target, IMappingOptions options )
        {
            var context = this.GetMapperContext( source, target, options );

            var targetMembers = target.GetMembers( BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly )
                .Where( m => (m is MethodInfo mi && mi.IsPublic && !mi.IsStatic && !mi.IsSpecialName) ||
                    (m is PropertyInfo pi && pi.GetSetMethod() != null) )
                .Select( m => new
                {
                    Member = m,
                    Options = m.GetCustomAttribute<OptionAttribute>() ?? new OptionAttribute()
                } )
                .Where( m => !m.Options.IsIgnored )
                .OrderByDescending( info => info.Options.IsRequired )
                .ThenBy( info => info.Options.Order )
                .Select( m => m.Member )
                .ToArray();

            var subParam = Expression.Parameter( typeof( IParsedParam ), "param" );

            var memberAssign = new MemberExpressionBuilder( _mapper.MappingConfiguration )
                .GetMemberAssignments( context, targetMembers, subParam, MapperConfiguration ).ToList();      

            var help = Expression.IfThen
            (
                Expression.Equal( Expression.Constant( "help" ),
                    Expression.Call( Expression.Property( context.SourceInstance,
                    nameof( ParsedCommand.Name ) ), nameof( String.ToLower ), null, null ) ),

                Expression.Invoke( _helperCall, Expression.Constant( context.TargetInstance.Type ),
                    Expression.Property( context.SourceInstance, nameof( ParsedCommand.Param ) ) )
            );

            var expression = Expression.Block
            (
                new[] { subParam },

                Expression.Assign( subParam, Expression.Property( context.SourceInstance, nameof( ParsedCommand.Param ) ) ),
                Expression.Block( memberAssign ),
                help
            );

            var delegateType = typeof( Action<,,> ).MakeGenericType( context.ReferenceTracker.Type,
               context.SourceInstance.Type, context.TargetInstance.Type );

            return Expression.Lambda( delegateType, expression,
                context.ReferenceTracker, context.SourceInstance, context.TargetInstance );
        }
    }
}
