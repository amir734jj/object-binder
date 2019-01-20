using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using core.Interfaces;
using Dawn;
using LamarCompiler;
using InfoViaLinq;

namespace core
{
    public class ObjectBinder<TSource, TCommon> : IObjectBinder
    {
        // ReSharper disable once StaticMemberInGenericType
        private static int _count;

        public Type BoundType { get; }

        public ObjectBinder(IDictionary<Expression<Func<TSource, object>>, Expression<Func<TCommon, object>>> map)
        {
            var sourceType = typeof(TSource);
            var commonType = typeof(TCommon);

            Guard.Argument(commonType).Require(x => x.IsInterface, _ =>
                "Second type parameter T has to be an interface to avoid property duplication as overriding property is not possible");

            var generator = new AssemblyGenerator();
            generator.ReferenceAssemblyContainingType<TSource>();
            generator.ReferenceAssemblyContainingType<TCommon>();

            var assembly = generator.Generate(opt =>
            {
                var className = $"{commonType.Name}_proxy_{_count++}";
                opt.UsingNamespace<TSource>();
                opt.UsingNamespace<TCommon>();

                opt.Namespace($"LamarGenerated.Proxy.{sourceType.Name}_{commonType.Name}");
                opt.StartClass(className, typeof(TCommon));

                // Shadow field
                opt.Write($"private readonly {sourceType.FullNameInCode()} _source;");
                opt.BlankLine();

                // Constructor
                opt.Write($"BLOCK:public {className} ({sourceType.FullNameInCode()} source)");
                opt.Write("_source = source;");
                opt.FinishBlock();

                foreach (var (key, value) in map)
                {
                    var (prop1, prop2) = (ResolveMemberExpression(key), ResolveMemberExpression(value));

                    Guard.Argument((prop1, prop2))
                        .Require(x => x.Item1 != null)
                        .Require(x => x.Item2 != null)
                        .Require(x => x.Item1.PropertyType == x.Item2.PropertyType, tuple =>
                            $"Property types between {tuple.Item1.PropertyType} and {tuple.Item2.PropertyType} do not match");

                    opt.Write($"public {prop1.PropertyType.FullNameInCode()} {prop2.Name} " +
                              "{" +
                              $@" get {{ return _source.{prop1.Name};  }} " +
                              $@" set {{ _source.{prop1.Name} = value; }} " +
                              "}");
                    
                    opt.BlankLine();
                }

                opt.FinishBlock(); // Finish the class
                opt.FinishBlock(); // Finish the namespace
            });

            BoundType = assembly.GetExportedTypes().Single();
        }

        private static PropertyInfo ResolveMemberExpression<T>(Expression<Func<T, object>> expression)
        {
            var members = InfoViaLinq<T>.New().PropLambda(expression).Members().ToList();

            const string errorMessage = "Nested Properties are not supported";

            Guard.Argument(members)
                .MinCount(1, (_, i) => errorMessage)
                .MaxCount(1, (_, i) => errorMessage);

            return members.Last();
        }
    }
}