using System;
using System.Linq.Expressions;

namespace core.Interfaces
{
    public interface IObjectBinderBuilder<TSource, TCommon>
    {
        IObjectBinderBuilder<TSource, TCommon> Bind(Expression<Func<TSource, object>> prop1Expr,
            Expression<Func<TCommon, object>> prop2Expr);

        IObjectBinder Build();
    }
}