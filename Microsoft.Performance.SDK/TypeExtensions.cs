// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods for interacting
    ///     with <see cref="Type"/> instances.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        ///     Determines whether the current <see cref="Type"/> is
        ///     assignable to an instance of the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type">
        ///     The current type.
        /// </param>
        /// <param name="baseType">
        ///     The target type.
        /// </param>
        /// <returns>
        ///     <c>true</c> if an instance of <see cref="Type"/> <paramref name="type"/>
        ///     is assignable to an instance of <see cref="Type"/> <paramref name="baseType"/>;
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="type"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="baseType"/> is <c>null</c>.
        /// </exception>
        public static bool IsAssignableTo(this Type type, Type baseType)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (baseType == null)
            {
                throw new ArgumentNullException(nameof(baseType));
            }

            return baseType.IsAssignableFrom(type);
        }

        /// <summary>
        ///     Determines whether the current type is <see cref="Nullable"/>.
        /// </summary>
        /// <param name="type">
        ///     The type to check.
        /// </param>
        /// <returns>
        ///     <c>trye</c> if <paramref name="type"/> represents a nullable
        ///     type; <c>false</c> otherwise.
        /// </returns>
        public static bool IsNullableType(this Type type)
        {
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        /// <summary>
        ///     Gets the collection of <see cref="Type"/> from the given collection that are
        ///     assignable to <see cref="Type"/> <typeparamref name="TBase"/>.
        /// </summary>
        /// <typeparam name="TBase">
        ///     The target <see cref="Type"/>.
        /// </typeparam>
        /// <param name="items">
        ///     The <see cref="Type"/>s to check.
        /// </param>
        /// <returns>
        ///     All <see cref="Type"/> from <paramref name="items"/> that are assignable to
        ///     <typeparamref name="TBase"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="items"/> is <c>null</c>.
        /// </exception>
        public static IEnumerable<Type> WhereAssignableFrom<TBase>(this IEnumerable<Type> items)
        {
            return WhereAssignableFrom(items, typeof(TBase));
        }

        /// <summary>
        ///     Gets the collection of <see cref="Type"/> from the given collection that are
        ///     assignable to <see cref="Type"/> <paramref name="baseType"/>.
        /// </summary>
        /// <param name="items">
        ///     The <see cref="Type"/>s to check.
        /// </param>
        /// <param name="baseType">
        ///     The target <see cref="Type"/>.
        /// </param>
        /// <returns>
        ///     All <see cref="Type"/> from <paramref name="items"/> that are assignable to
        ///     <paramref name="baseType"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="items"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="baseType"/> is <c>null</c>.
        /// </exception>
        public static IEnumerable<Type> WhereAssignableFrom(this IEnumerable<Type> items, Type baseType)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (baseType == null)
            {
                throw new ArgumentNullException(nameof(baseType));
            }

            return WhereAssignableFromImpl(items, baseType);
        }

        private static IEnumerable<Type> WhereAssignableFromImpl(IEnumerable<Type> items, Type baseType)
        {
            foreach (Type item in items)
            {
                if (item != null && baseType.IsAssignableFrom(item))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        ///     Determines if the current <see cref="Type"/> is assignable to the
        ///     given generic <see cref="Type"/>.
        /// </summary>
        /// <param name="type">
        ///     The current type.
        /// </param>
        /// <param name="generic">
        ///     The generic type.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="type"/> is assignable to <paramref name="generic"/>;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool IsAssignableToGenericType(this Type type, Type generic)
        {
            return new[] { type }.Concat(type.GetInterfaces()).Any(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == generic) ||
                                 (type.BaseType?.IsAssignableToGenericType(generic) ?? false);
        }

        /// <summary>
        ///     Determines whether the current <see cref="Type"/> is
        ///     assignable to an instance of the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="self">
        ///     The current type.
        /// </param>
        /// <param name="other">
        ///     The target type.
        /// </param>
        /// <returns>
        ///     <c>true</c> if an instance of <see cref="Type"/> <paramref name="self"/>
        ///     is assignable to an instance of <see cref="Type"/> <paramref name="other"/>;
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="self"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="other"/> is <c>null</c>.
        /// </exception>
        public static bool Is(
            this Type self,
            Type other)
        {
            Guard.NotNull(self, nameof(self));
            Guard.NotNull(other, nameof(other));

            return other.IsAssignableFrom(self);
        }

        /// <summary>
        ///     Determines whether the current <see cref="Type"/> is
        ///     assignable to an instance of the specified <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">
        ///     The target type.
        /// </typeparam>
        /// <param name="self">
        ///     The current type.
        /// </param>
        /// <returns>
        ///     <c>true</c> if an instance of <see cref="Type"/> <paramref name="self"/>
        ///     is assignable to an instance of <see cref="Type"/> <typeparamref name="T"/>.
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="self"/> is <c>null</c>.
        /// </exception>
        public static bool Is<T>(
            this Type self)
        {
            return self.Is(typeof(T));
        }

        /// <summary>
        ///     Determines whether the current <see cref="Type"/> implements
        ///     the given interface <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="TInterface">
        ///     The interface <see cref="Type"/>.
        /// </typeparam>
        /// <param name="type">
        ///     The current <see cref="Type"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="type"/> implements the given interface <see cref="Type"/>;
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="type"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        ///     <typeparamref name="TInterface"/> does not represent an interface <see cref="Type"/>.
        /// </exception>
        public static bool Implements<TInterface>(this Type type)
        {
            return type.Implements(typeof(TInterface));
        }

        /// <summary>
        ///     Determines whether the current <see cref="Type"/> implements
        ///     the given interface <see cref="Type"/>.
        /// </summary>
        /// <param name="type">
        ///     The current <see cref="Type"/>.
        /// </param>
        /// <param name="interfaceType">
        ///     The interface <see cref="Type"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="type"/> implements the given interface;
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="type"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="interfaceType"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        ///     <paramref name="interfaceType"/> does not represent an interface <see cref="Type"/>.
        /// </exception>
        public static bool Implements(this Type type, Type interfaceType)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (interfaceType == null)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }

            if (!interfaceType.IsInterface)
            {
                throw new InvalidOperationException(
                    string.Format("{0} must represent an interface type.", nameof(interfaceType)));
            }

            // interfaces do not inherit from interfaces, so we can avoid a lot of
            // checks that IsAssignableFrom does by implementing the internal method
            // "ImplementsInterface" directly here.
            var currentType = type;
            while (currentType != null)
            {
                var interfaces = currentType.GetInterfaces();
                if (interfaces != null)
                {
                    for (var i = 0; i < interfaces.Length; ++i)
                    {
                        if ((interfaces[i] == interfaceType) ||
                            (interfaces[i] != null && interfaces[i].Implements(interfaceType)))
                        {
                            return true;
                        }
                    }
                }

                currentType = currentType.BaseType;
            }

            return false;
        }

        /// <summary>
        ///     Determines whether the current <see cref="Type"/> is static.
        /// </summary>
        /// <param name="self">
        ///     The current <see cref="Type"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="self"/> is static;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool IsStatic(this Type self)
        {
            Guard.NotNull(self, nameof(self));

            return self.IsAbstract && self.IsSealed;
        }

        /// <summary>
        ///     Determines whether the current <see cref="Type"/> is instantiable.
        ///     Instantiable <see cref="Type"/>s are concrete.
        /// </summary>
        /// <param name="self">
        ///     The current <see cref="Type"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="self"/> is instantiable;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool IsInstantiatable(this Type self)
        {
            Guard.NotNull(self, nameof(self));

            // Static types are abstract and sealed, so this
            // check will avoid static types by virtue of the
            // !IsAbstract clause.
            return !self.IsInterface &&
                   !self.IsAbstract;
        }

        /// <summary>
        ///     Determines whether the current <see cref="Type"/> is concrete.
        ///     Concrete <see cref="Type"/>s can be instantiated.
        /// </summary>
        /// <param name="self">
        ///     The current <see cref="Type"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="self"/> is concrete;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool IsConcrete(
            this Type self)
        {
            Guard.NotNull(self, nameof(self));

            // Static types are abstract and sealed, so this
            // check will avoid static types by virtue of the
            // !IsAbstract clause.
            return !self.IsInterface &&
                   !self.IsAbstract;
        }

        /// <summary>
        ///     Determines whether the given <see cref="Type"/> is considered to be
        ///     public or nested public.
        /// </summary>
        /// <param name="self">
        ///     The current <see cref="Type"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="self"/> is public or
        ///     nested public; <c>false</c> otherwise.
        /// </returns>
        public static bool IsPublic(this Type self)
        {
            Guard.NotNull(self, nameof(self));

            return self.IsPublic || self.IsNestedPublic;
        }

        /// <summary>
        ///     Gets the default value for the given <see cref="Type"/>.
        /// </summary>
        /// <param name="self">
        ///     The current <see cref="Type"/>.
        /// </param>
        /// <returns>
        ///     The default value for instances of the <see cref="Type"/>
        ///     referred to by <paramref name="self"/>.
        /// </returns>
        public static object Default(this Type self)
        {
            return self.IsValueType
                ? Activator.CreateInstance(self)
                : null;
        }

        /// <summary>
        ///  Determines whether the given <see cref="Type"/> is a numeric type
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsNumeric(this Type self)
        {
            return  (self == typeof(Int32)) ||
                    (self == typeof(UInt32)) ||
                    (self == typeof(Int64)) ||
                    (self == typeof(UInt64)) ||
                    (self == typeof(Int16)) ||
                    (self == typeof(UInt16)) ||
                    (self == typeof(decimal)) ||
                    (self == typeof(float)) ||
                    (self == typeof(double)) ||
                    (self == typeof(byte)) ||
                    (self == typeof(sbyte));
        }
    }
}
