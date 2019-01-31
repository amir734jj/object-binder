using System;
using System.Collections.Immutable;
using System.Linq.Expressions;
using core.Interfaces;

namespace core.Builders
{
    public static class ObjectBinderBuilder
    {
        public static ObjectBinderBuilder<TSource, TCommon> New<TSource, TCommon>()
        {
            return new ObjectBinderBuilder<TSource, TCommon>();
        }
    }

    public class ObjectBinderBuilder<TSource, TCommon> : IObjectBinderBuilder<TSource, TCommon>
    {
        private ImmutableDictionary<Expression<Func<TSource, object>>, Expression<Func<TCommon, object>>> _map;

        private Expression<Func<TCommon, TSource>> _sourceRefExpr;

        public ObjectBinderBuilder()
        {
            _map = ImmutableDictionary<Expression<Func<TSource, object>>, Expression<Func<TCommon, object>>>.Empty;
        }
        
        public IObjectBinderBuilder<TSource, TCommon> Bind(Expression<Func<TSource, object>> prop1Expr, Expression<Func<TCommon, object>> prop2Expr)
        {
            _map = _map.Add(prop1Expr, prop2Expr);

            return this;
        }
        
        public IObjectBinderBuilder<TSource, TCommon> SourceRef(Expression<Func<TCommon, TSource>> sourceRefExpr)
        {
            _sourceRefExpr = sourceRefExpr;

            return this;
        }

        public IObjectBinder Build()
        {
            return new ObjectBinder<TSource, TCommon>(_map, _sourceRefExpr);
        }
    }
}