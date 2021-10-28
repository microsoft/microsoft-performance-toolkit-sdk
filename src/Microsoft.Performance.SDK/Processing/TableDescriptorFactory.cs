// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Performance.SDK.Extensibility;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides a method of generating <see cref="TableDescriptor"/>s
    ///     from <see cref="Type"/>s that represent tables.
    /// </summary>
    public static class TableDescriptorFactory
    {
        /// <summary>
        ///     If possible, creates a <see cref="TableDescriptor"/> from the
        ///     given <see cref="Type"/>. A <see cref="Type"/> is considered to
        ///     represent a table if the following are true:
        ///     <para />
        ///     <list type="bullet">
        ///         <item>
        ///             The <see cref="Type"/> is concrete. That is, the
        ///             <see cref="Type"/> is not abstract, is not static, and
        ///             is not an interface.
        ///         </item>
        ///         <item>
        ///             The <see cref="Type"/> is decorated with the
        ///             <see cref="TableAttribute"/> attribute.
        ///         </item>       
        ///     </list>
        ///     <para/>
        /// </summary>
        /// <param name="type">
        ///     The <see cref="Type"/> from which to extract a
        ///     <see cref="TableDescriptor"/>.
        /// </param>
        /// <param name="tableConfigSerializer">
        ///     The serializer object that is used to deserialize the 
        ///     pre-built table configuration JSON files.
        /// </param>
        /// <param name="tableDescriptor">
        ///     If <paramref name="type"/> is a valid table type, then this is
        ///     populated with the created <see cref="TableDescriptor"/>.
        ///     Otherwise, this parameter will be set to <c>null</c>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="type"/> is a valid table,
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool TryCreate(
            Type type,
            ISerializer tableConfigSerializer,
            out TableDescriptor tableDescriptor)
        {
            return TryCreate(type, tableConfigSerializer, out tableDescriptor, out var buildTableAction);
        }

        /// <summary>
        ///     If possible, creates a <see cref="TableDescriptor"/> from the
        ///     given <see cref="Type"/>. A <see cref="Type"/> is considered to
        ///     represent a table if the following are true:
        ///     <para />
        ///     <list type="bullet">
        ///         <item>
        ///             The <see cref="Type"/> is concrete. That is, the
        ///             <see cref="Type"/> is not abstract, is not static, and
        ///             is not an interface.
        ///         </item>
        ///         <item>
        ///             The <see cref="Type"/> is decorated with the
        ///             <see cref="TableAttribute"/> attribute.
        ///         </item>       
        ///     </list>
        ///     <para/>
        /// </summary>
        /// <param name="type">
        ///     The <see cref="Type"/> from which to extract a
        ///     <see cref="TableDescriptor"/>.
        /// </param>
        /// <param name="tableConfigSerializer">
        ///     The serializer object that is used to deserialize the 
        ///     pre-built table configuration JSON files.
        /// </param>
        /// <param name="tableDescriptor">
        ///     If <paramref name="type"/> is a valid table type, then this is
        ///     populated with the created <see cref="TableDescriptor"/>.
        ///     Otherwise, this parameter will be set to <c>null</c>.
        /// </param>
        /// <param name="buildTableAction">
        ///     If the type contains a build table action, it is returned here.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="type"/> is a valid table,
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool TryCreate(
            Type type,
            ISerializer tableConfigSerializer,
            out TableDescriptor tableDescriptor,
            out Action<ITableBuilder, IDataExtensionRetrieval> buildTableAction)
        {
            return TryCreate(type, tableConfigSerializer, out tableDescriptor, out buildTableAction, out var isDataAvailableFunc);
        }

        /// <summary>
        ///     If possible, creates a <see cref="TableDescriptor"/> from the
        ///     given <see cref="Type"/>. A <see cref="Type"/> is considered to
        ///     represent a table if the following are true:
        ///     <para />
        ///     <list type="bullet">
        ///         <item>
        ///             The <see cref="Type"/> is concrete. That is, the
        ///             <see cref="Type"/> is not abstract, is not static, and
        ///             is not an interface.
        ///         </item>
        ///         <item>
        ///             The <see cref="Type"/> is decorated with the
        ///             <see cref="TableAttribute"/> attribute.
        ///         </item>       
        ///     </list>
        ///     <para/>
        /// </summary>
        /// <param name="type">
        ///     The <see cref="Type"/> from which to extract a
        ///     <see cref="TableDescriptor"/>.
        /// </param>
        /// <param name="tableConfigSerializer">
        ///     The serializer object that is used to deserialize the 
        ///     pre-built table configuration JSON files.
        /// </param>
        /// <param name="tableDescriptor">
        ///     If <paramref name="type"/> is a valid table type, then this is
        ///     populated with the created <see cref="TableDescriptor"/>.
        ///     Otherwise, this parameter will be set to <c>null</c>.
        /// </param>
        /// <param name="buildTableAction">
        ///     If the type contains a build table action, it is returned here.
        /// </param>
        /// <param name="isDataAvailableFunc">
        ///     If the type contains a has data func, it is returned here.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="type"/> is a valid table,
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool TryCreate(
            Type type,
            ISerializer tableConfigSerializer,
            out TableDescriptor tableDescriptor,
            out Action<ITableBuilder, IDataExtensionRetrieval> buildTableAction,
            out Func<IDataExtensionRetrieval, bool> isDataAvailableFunc)
        {
            return TryCreate(
                type,
                tableConfigSerializer,
                null,
                out tableDescriptor,
                out buildTableAction,
                out isDataAvailableFunc);
        }

        /// <summary>
        ///     If possible, creates a <see cref="TableDescriptor"/> from the
        ///     given <see cref="Type"/>. A <see cref="Type"/> is considered to
        ///     represent a table if the following are true:
        ///     <para />
        ///     <list type="bullet">
        ///         <item>
        ///             The <see cref="Type"/> is concrete. That is, the
        ///             <see cref="Type"/> is not abstract, is not static, and
        ///             is not an interface.
        ///         </item>
        ///         <item>
        ///             The <see cref="Type"/> is decorated with the
        ///             <see cref="TableAttribute"/> attribute.
        ///         </item>       
        ///     </list>
        ///     <para/>
        /// </summary>
        /// <param name="type">
        ///     The <see cref="Type"/> from which to extract a
        ///     <see cref="TableDescriptor"/>.
        /// </param>
        /// <param name="tableConfigSerializer">
        ///     The serializer object that is used to deserialize the 
        ///     pre-built table configuration JSON files.
        /// </param>
        /// <param name="logger">
        ///     Used to log any relevant messages.
        /// </param>
        /// <param name="tableDescriptor">
        ///     If <paramref name="type"/> is a valid table type, then this is
        ///     populated with the created <see cref="TableDescriptor"/>.
        ///     Otherwise, this parameter will be set to <c>null</c>.
        /// </param>
        /// <param name="buildTableAction">
        ///     If the type contains a build table action, it is returned here.
        /// </param>
        /// <param name="isDataAvailableFunc">
        ///     If the type contains a has data func, it is returned here.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="type"/> is a valid table,
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool TryCreate(
            Type type,
            ISerializer tableConfigSerializer,
            ILogger logger,
            out TableDescriptor tableDescriptor,
            out Action<ITableBuilder, IDataExtensionRetrieval> buildTableAction,
            out Func<IDataExtensionRetrieval, bool> isDataAvailableFunc)
        {
            tableDescriptor = null;
            buildTableAction = null;
            isDataAvailableFunc = null;

            if (type is null)
            {
                return false;
            }

            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            if (tableAttribute == null)
            {
                return false;
            }

            if (!type.IsStatic() && !type.IsConcrete())
            {
                return false;
            }

            buildTableAction = GetTableBuildAction(type, tableAttribute.BuildTableActionMethodName);
            isDataAvailableFunc = GetIsDataAvailableFunc(type, tableAttribute.IsDataAvailableMethodName);
            tableDescriptor = GetTableDescriptor(
                type,
                tableAttribute.TableDescriptorPropertyName,
                tableConfigSerializer,
                logger);

            if (tableDescriptor == null)
            {
                return false;
            }

            if (buildTableAction == null && !tableDescriptor.IsInternalTable)
            {
                logger?.Error(
                    "The table {0} must add a buld table method or be marked Internal.",
                    type.FullName);
                return false;
            }

            foreach (var cooker in type.GetCustomAttributes<RequiresSourceCookerAttribute>())
            {
                tableDescriptor.AddRequiredDataCooker(cooker.RequiredDataCookerPath);
            }

            foreach (var cooker in type.GetCustomAttributes<RequiresCompositeCookerAttribute>())
            {
                tableDescriptor.AddRequiredDataCooker(cooker.RequiredDataCookerPath);
            }

            // todo:should we make this optional, or does it makes sense to always include this?
            // we could add an "IncludeType" bool or something on the table attribute if we don't want
            // this to always be set.
            tableDescriptor.ExtendedData["Type"] = type;

            return true;
        }

        private static TableDescriptor GetTableDescriptor(
            Type type,
            string propertyName,
            ISerializer tableConfigSerializer,
            ILogger logger)
        {
            Debug.Assert(type != null, $"Parameter {nameof(type)} cannot be null.");
            Debug.Assert(!string.IsNullOrEmpty(propertyName), $"Parameter {nameof(propertyName)} cannot be null or empty.");
            Debug.Assert(tableConfigSerializer != null, $"Parameter {nameof(tableConfigSerializer)} cannot be null.");

            TableDescriptor tableDescriptor = null;
            IEnumerable<RequiresSourceCookerAttribute> sourceCookerAttributes = null;
            IEnumerable<RequiresCompositeCookerAttribute> compositeCookerAttributes = null;

            var tableDescriptorPropertyInfo = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
            if (tableDescriptorPropertyInfo != null)
            {
                var getMethodInfo = tableDescriptorPropertyInfo.GetMethod;
                if (!getMethodInfo.IsPublic)
                {
                    logger?.Warn(
                        $"Type {type} appears to be a Table, but the " +
                        $"{nameof(TableAttribute.TableDescriptorPropertyName)}.get method is not found as public.");
                    return null;
                }

                if (getMethodInfo.ReturnType != typeof(TableDescriptor))
                {
                    logger?.Warn(
                        $"Type {type} appears to be a Table, but the " +
                        $"{nameof(TableAttribute.TableDescriptorPropertyName)}.get method must return type " +
                        $"{nameof(TableDescriptor)}.");
                    return null;
                }

                // Allow [RequiresSourceCooker] and/or [RequiresCompositeCooker] attribute on the property that returns the TableDescriptor
                sourceCookerAttributes = tableDescriptorPropertyInfo.GetCustomAttributes<RequiresSourceCookerAttribute>();
                compositeCookerAttributes = tableDescriptorPropertyInfo.GetCustomAttributes<RequiresCompositeCookerAttribute>();
                tableDescriptor = (TableDescriptor)getMethodInfo.Invoke(null, null);
            }
            else
            {
                // The table descriptor property didn't exist, check for a field instead.

                var tableDescriptorFieldInfo = type.GetField(propertyName, BindingFlags.Public | BindingFlags.Static);
                if (tableDescriptorFieldInfo == null)
                {
                    logger?.Warn(
                        $"Type {type} appears to be a class, but the " +
                        $"{nameof(TableAttribute.TableDescriptorPropertyName)} property is not found as public and " +
                        "static.");
                    return null;
                }

                // Allow [RequiresSourceCooker] and/or [RequiresCompositeCooker] attribute on the property that returns the TableDescriptor
                sourceCookerAttributes = tableDescriptorFieldInfo.GetCustomAttributes<RequiresSourceCookerAttribute>();
                compositeCookerAttributes = tableDescriptorFieldInfo.GetCustomAttributes<RequiresCompositeCookerAttribute>();
                tableDescriptor = tableDescriptorFieldInfo.GetValue(null) as TableDescriptor;
            }

            if (tableDescriptor == null)
            {
                return null;
            }

            tableDescriptor.Type = type;
            tableDescriptor.PrebuiltTableConfigurations = TableConfigurations.GetPrebuiltTableConfigurations(
                type,
                tableDescriptor.Guid,
                tableConfigSerializer,
                logger);

            foreach (var cooker in sourceCookerAttributes)
            {
                tableDescriptor.AddRequiredDataCooker(cooker.RequiredDataCookerPath);
            }

            foreach (var cooker in compositeCookerAttributes)
            {
                tableDescriptor.AddRequiredDataCooker(cooker.RequiredDataCookerPath);
            }

            return tableDescriptor;
        }

        private static Action<ITableBuilder, IDataExtensionRetrieval> GetTableBuildAction(Type type, string methodName)
        {
            Guard.NotNull(type, nameof(type));

            if (string.IsNullOrWhiteSpace(methodName))
            {
                return null;
            }

            var creationMethodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            if (creationMethodInfo == null)
            {
                //DebugUtils.WriteLine($"Type {type} appears to be a class, but the {nameof(TableFactoryAttribute.FactoryMethodName)} method is not found as public and static.");
                return null;
            }

            var creationParameterInfo = creationMethodInfo.GetParameters();
            if (creationParameterInfo.Length != 2)
            {
                //DebugUtils.WriteLine($"Type {type} appears to be a class, but the {nameof(TableFactoryAttribute.FactoryMethodName)} method needs to take two parameters (ITableBuilder, object).");
                return null;
            }

            if (creationParameterInfo[0].ParameterType != typeof(ITableBuilder))
            {
                //DebugUtils.WriteLine($"Type {type} appears to be a class, but the {nameof(TableFactoryAttribute.FactoryMethodName)} method's first parameter should be of type {nameof(ITableBuilder)}.");
                return null;
            }

            if (creationParameterInfo[1].ParameterType != typeof(IDataExtensionRetrieval))
            {
                //DebugUtils.WriteLine($"Type {type} appears to be a class, but the {nameof(TableFactoryAttribute.FactoryMethodName)} method's second parameter should be of type {nameof(IDataExtensionRetrieval)}.");
                return null;
            }

            return ((builder, o) =>
            {
                var parameters = new object[] { builder, o };
                creationMethodInfo.Invoke(null, parameters);
            });
        }

        private static Func<IDataExtensionRetrieval, bool> GetIsDataAvailableFunc(Type type, string methodName)
        {
            Guard.NotNull(type, nameof(type));

            if (string.IsNullOrWhiteSpace(methodName))
            {
                return null;
            }

            var creationMethodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            if (creationMethodInfo == null)
            {
                //DebugUtils.WriteLine($"Type {type} appears to be a class, but the {nameof(TableFactoryAttribute.FactoryMethodName)} method is not found as public and static.");
                return null;
            }

            if (creationMethodInfo.ReturnType != typeof(bool))
            {
                //DebugUtils.WriteLine($"Type {type} appears to be a class, but the {nameof(TableFactoryAttribute.FactoryMethodName)} method needs to return a bool.");
                return null;
            }

            var creationParameterInfo = creationMethodInfo.GetParameters();
            if (creationParameterInfo.Length != 1)
            {
                //DebugUtils.WriteLine($"Type {type} appears to be a class, but the {nameof(TableFactoryAttribute.FactoryMethodName)} method needs to take one parameters (object).");
                return null;
            }

            if (creationParameterInfo[0].ParameterType != typeof(IDataExtensionRetrieval))
            {
                //DebugUtils.WriteLine($"Type {type} appears to be a class, but the {nameof(TableFactoryAttribute.FactoryMethodName)} method's second parameter should be of type {nameof(IDataExtensionRetrieval)}.");
                return null;
            }

            return ((o) =>
            {
                var parameters = new object[] { o };
                return (bool)creationMethodInfo.Invoke(null, parameters);
            });
        }
    }
}
