// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing;

/// <summary>
///     Base type for the result of a table command executed via
///     <see cref="ITableBuilder.AddTableCommand3(string, System.Predicate{TableCommandContext2}, TableCommandCallback2)"/>.
///     <para/>
///     Concrete result types are defined by the SDK (for example
///     <see cref="VoidTableCommandResult"/> and <see cref="OpenUrisTableCommandResult"/>).
///     Hosts inspect the returned <see cref="TableCommandResult"/> to decide what
///     follow-up action, if any, to perform on the plugin's behalf.
/// </summary>
public abstract record TableCommandResult;
