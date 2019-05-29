using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        public ObjectBinder(IDictionary<Expression<Func<TSource, object>>, Expression<Func<TCommon, object>>> map,
            AssemblyGenerator assemblyGenerator)
        {
            var sourceType = typeof(TSource);
            var commonType = typeof(TCommon);

            Guard.Argument(commonType)
                .Require(x => x.IsInterface || x.GetConstructor(Type.EmptyTypes) != null,
                    x => "Common type must be either an interface or provide parameterless constructor"
                );

            assemblyGenerator.ReferenceAssemblyContainingType<TSource>();
            assemblyGenerator.ReferenceAssemblyContainingType<TCommon>();

            var visitedCommonProperties = ImmutableList<PropertyInfo>.Empty;

            var (shadowField, constructorArg) = ("_sourceRef", "sourceRef");

            var assembly = assemblyGenerator.Generate(opt =>
            {
                var className = $"{commonType.Name}_proxy_{_count++}";
                opt.UsingNamespace<TSource>();
                opt.UsingNamespace<TCommon>();

                opt.Namespace($"LamarGenerated.Proxy.{sourceType.Name}_{commonType.Name}");
                opt.StartClass(className, typeof(TCommon));

                // Shadow field
                opt.Write($"private readonly {sourceType.FullNameInCode()} {shadowField};");
                opt.BlankLine();

                // Constructor
                opt.Write($"BLOCK:public {className} ({sourceType.FullNameInCode()} {constructorArg})");
                opt.Write($"{shadowField} = {constructorArg};");
                opt.FinishBlock();

                foreach (var (key, value) in map)
                {
                    var (prop1, prop2) = (ResolveMemberExpression(key), ResolveMemberExpression(value));

                    visitedCommonProperties = visitedCommonProperties.Add(prop2);

                    Guard.Argument((prop1, prop2))
                        .Require(x => x.Item1 != null)
                        .Require(x => x.Item2 != null)
                        .Require(x => x.Item1.PropertyType == x.Item2.PropertyType,
                            tuple =>
                                $"Property types between {tuple.Item1.PropertyType} and {tuple.Item2.PropertyType} do not match");

                    // If TCommon is not an interface and if property of common is virtual, then we can override it else
                    // it causes property duplication as overriding property is not possible
                    var needsOverride = false;

                    if (!commonType.IsInterface)
                    {
                        needsOverride = prop2.GetGetMethod().IsVirtual
                            ? true
                            : throw new Exception($"TCommon type is not an interface and property {prop2.Name} of" +
                                                  "Common type is not virtual hence it causes property duplication");
                    }

                    opt.Write(
                        $"public {(needsOverride ? "override" : string.Empty)} {prop1.PropertyType.FullNameInCode()} {prop2.Name} " +
                        "{" +
                        $@" get {{ return {shadowField}.{prop1.Name};  }} " +
                        $@" set {{ {shadowField}.{prop1.Name} = value; }} " +
                        "}");

                    opt.BlankLine();
                }

                // If common type is not an interface, hence we need to add the properties, otherwise, they will be inherited
                if (commonType.IsInterface)
                {
                    // Add the rest of properties that do not needed be bound
                    foreach (var propertyInfo in commonType.GetProperties().Except(visitedCommonProperties))
                    {
                        opt.Write(
                            $"public {propertyInfo.PropertyType.FullNameInCode()} {propertyInfo.Name} {{ get; set; }} ");

                        opt.BlankLine();
                    }
                }
                
                opt.FinishBlock(); // Finish the class
                opt.FinishBlock(); // Finish the namespace
            });

            BoundType = assembly.GetExportedTypes().Single();
        }

        private static PropertyInfo ResolveMemberExpression<T, TProperty>(Expression<Func<T, TProperty>> expression)
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