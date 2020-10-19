// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.Performance.SDK.Processing
{
    internal static class CodeGenerationContext
    {
        private static readonly ModuleBuilder moduleBuilder;

        private static readonly Cache<MethodInfo, Type> selectorDictionary = new Cache<MethodInfo, Type>(
            methodInfo =>
            {
                Type structSelectorType;
                if (methodInfo.ReturnType == typeof(void))
                {
                    throw new InvalidOperationException();
                }

                structSelectorType = CodeGenerationContext.CreateStructFunctionType(methodInfo);

                return structSelectorType;
            });

        private static readonly MethodInfo method_System_Type_GetTypeFromHandle = 
            typeof(Type).GetMethod(
                "GetTypeFromHandle",
                BindingFlags.Static | BindingFlags.Public);

        private static readonly Type[] Int32TypeArray = new[]
        {
            typeof(int),
        };

        private static readonly CustomAttributeBuilder DefaultMemberItem = 
            new CustomAttributeBuilder(
                typeof(DefaultMemberAttribute).GetConstructor(
                    new Type[] 
                    {
                        typeof(string),
                    }),
                new object[]
                {
                    "Item",
                });

        static CodeGenerationContext()
        {
            var name = "Microsoft.Performance.SDK.CodeGeneration";

            moduleBuilder = AssemblyBuilder.DefineDynamicAssembly(
                    new AssemblyName(name), 
                    AssemblyBuilderAccess.Run)
                .DefineDynamicModule(name);
        }

        public static IProjection<TSource, TResult> CreateStructFunction<TSource, TResult>(Func<TSource, TResult> selector)
        {
            return (IProjection<TSource, TResult>)CodeGenerationContext.CreateStructFunction((Delegate)selector);
        }

        public static IProjectionDescription CreateStructFunction(Delegate selector)
        {
            Guard.NotNull(selector, nameof(selector));

            var invocationList = selector.GetInvocationList();
            if (invocationList.Length != 1)
            {
                throw new ArgumentException($"Delegate {selector.ToString()} must point to a single method.");
            }

            selector = invocationList[0];
            if (selector.Method.ReturnType == typeof(void))
            {
                throw new ArgumentException($"Delegate {selector.ToString()} must return a type other than System.Void.");
            }

            Type structSelectorType = selectorDictionary[selector.Method];

            return (IProjectionDescription)CodeGenerationContext.CreateStructProxyInstance(structSelectorType, selector);
        }

        public static IProjectionDescription CreateStructPropertySelector(PropertyInfo property)
        {
            var funcType = typeof(Func<,>).MakeGenericType(property.GetGetMethod().GetParameters()[0].ParameterType, property.PropertyType);

            var selector = Delegate.CreateDelegate(funcType, property.GetGetMethod());

            return CodeGenerationContext.CreateStructFunction(selector);
        }

        private static object CreateStructProxyInstance(Type structProxyType, Delegate @delegate)
        {
            if (@delegate.Method.IsStatic)
            {
                return Activator.CreateInstance(structProxyType);
            }
            else
            {
                return Activator.CreateInstance(structProxyType, @delegate.Target);
            }
        }

        private static Type CreateStructFunctionType(MethodInfo methodToCall)
        {
            var sourceType = methodToCall.GetParameters()[0].ParameterType;
            var resultType = methodToCall.ReturnType;

            var type = moduleBuilder.DefineType(
                methodToCall.DeclaringType.FullName + "$." + methodToCall.Name,
                TypeAttributes.Public,
                typeof(ValueType),
                new Type[] 
                {
                    typeof(IProjection<,>).MakeGenericType(sourceType, resultType),
                    typeof(IProjectionDescription),
                }
                );

            FieldBuilder field = null;

            if (!methodToCall.IsStatic)
            {
                field = type.DefineField("target", methodToCall.ReflectedType, FieldAttributes.Private);

                var constructor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { methodToCall.ReflectedType });

                {
                    var il = constructor.GetILGenerator();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Stfld, field);
                    il.Emit(OpCodes.Ret);
                }
            }

            var property_SourceType = type.DefineProperty(
                "SourceType",
                PropertyAttributes.None,
                typeof(Type),
                Type.EmptyTypes
                );
            {
                var method_get_SourceType = type.DefineMethod(
                    "get_SourceType",
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                    CallingConventions.HasThis,
                    typeof(Type),
                    Type.EmptyTypes
                    );

                {
                    var il = method_get_SourceType.GetILGenerator();

                    il.Emit(OpCodes.Ldtoken, sourceType);
                    il.EmitCall(OpCodes.Call, method_System_Type_GetTypeFromHandle, null);
                    il.Emit(OpCodes.Ret);
                }

                property_SourceType.SetGetMethod(method_get_SourceType);
            }

            var property_ResultType = type.DefineProperty(
                "ResultType",
                PropertyAttributes.None,
                typeof(Type),
                Type.EmptyTypes
                );
            {
                var method_get_ResultType = type.DefineMethod(
                    "get_ResultType",
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                    CallingConventions.HasThis,
                    typeof(Type),
                    Type.EmptyTypes
                    );

                {
                    var il = method_get_ResultType.GetILGenerator();

                    il.Emit(OpCodes.Ldtoken, resultType);
                    il.EmitCall(OpCodes.Call, method_System_Type_GetTypeFromHandle, null);
                    il.Emit(OpCodes.Ret);
                }

                property_ResultType.SetGetMethod(method_get_ResultType);
            }

            var sourceTypeTypeArray = new Type[] { sourceType };

            var property_Item = type.DefineProperty(
                "Item",
                PropertyAttributes.None,
                resultType,
                sourceTypeTypeArray
                );
            {
                var method_get_Item = type.DefineMethod(
                    "get_Item",
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                    CallingConventions.HasThis,
                    resultType,
                    sourceTypeTypeArray
                    );

                {
                    var il = method_get_Item.GetILGenerator();

                    if (methodToCall.IsStatic)
                    {
                        il.Emit(OpCodes.Ldarg_1);
                        il.EmitCall(OpCodes.Call, methodToCall, null);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, field);
                        il.Emit(OpCodes.Ldarg_1);
                        il.EmitCall(OpCodes.Call, methodToCall, null);
                    }

                    il.Emit(OpCodes.Ret);
                }

                property_Item.SetGetMethod(method_get_Item);
            }

            type.SetCustomAttribute(DefaultMemberItem);

            return type.CreateTypeInfo();
        }
    }
}
