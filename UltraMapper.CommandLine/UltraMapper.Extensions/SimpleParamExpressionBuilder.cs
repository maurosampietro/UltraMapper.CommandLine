using CommandLine.AutoParser.Parsers;
using System;
using System.Linq.Expressions;
using UltraMapper;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders;

namespace CommandLine.AutoParser.UltraMapper.Extensions
{
    public class SimpleParamExpressionBuilder : PrimitiveMapperBase
    {
        public SimpleParamExpressionBuilder( Configuration configuration )
               : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return source == typeof( SimpleParam ) && target.IsBuiltIn( false );
        }

        protected override Expression GetValueExpression( MapperContext context )
        {
            var paramValueExp = Expression.Property( context.SourceInstance,
                nameof( SimpleParam.Value ) );
            
            var conversion = MapperConfiguration[ typeof( string ), 
                context.TargetInstance.Type ].MappingExpression;

            return Expression.Invoke( conversion, paramValueExp );
        }
    }
}