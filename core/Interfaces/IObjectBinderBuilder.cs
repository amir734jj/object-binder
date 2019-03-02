using System;
using System.Linq.Expressions;
using LamarCompiler;

namespace core.Interfaces
{
    public interface IObjectBinderBuilder<TSource, TCommon>
    {
        IObjectBinderBuilder<TSource, TCommon> Bind(Expression<Func<TSource, object>> prop1Expr,
            Expression<Func<TCommon, object>> prop2Expr);

        IObjectBinderBuilder<TSource, TCommon> WithAssemblyGenerator(AssemblyGenerator assemblyGenerator);

        IObjectBinder Build();
    }
}