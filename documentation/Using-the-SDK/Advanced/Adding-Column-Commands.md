# Adding Column Commands

Column commands let a plugin advertise operations that a host can perform on individual column values. A host may expose these operations in its user interface, such as in a context menu. Support is host-dependent, so a table must remain usable when a host does not expose column commands.

Commands are collected in a `DataColumnCommands` instance and attached to a `DataColumn<T>`, a `HierarchicalDataColumn<T>`, or an individual [column variant](./Adding-Column-Variants.md). Columns without commands expose `DataColumnCommands.Empty` through `IDataColumnWithCommands`.

## Downloading Source Code

`DownloadSourceCodeCommand` is a column command for retrieving the source code represented by a row value. Implement it for the value type projected by the column:

```cs
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.ColumnCommands;

public sealed class DownloadSourceCommand
    : DownloadSourceCodeCommand
{
    private static readonly HttpClient httpClient = new HttpClient();

    public DownloadSourceCommand()
        : base("Download source code")
    {
    }

    public override bool CanExecute(object value, string downloadPath)
    {
        return value is Uri sourceUri
            && (sourceUri.Scheme == Uri.UriSchemeHttp || sourceUri.Scheme == Uri.UriSchemeHttps)
            && !string.IsNullOrWhiteSpace(downloadPath);
    }

    public override async Task<DownloadSourceCodeResult[]> ExecuteAsync(
        object value,
        string downloadPath,
        CancellationToken cancellationToken)
    {
        if (!CanExecute(value, downloadPath))
        {
            return new[]
            {
                new DownloadSourceCodeResult(
                    "The selected value does not identify downloadable source code.",
                    value as Uri),
            };
        }

        var sourceUri = (Uri)value;
        var fileName = Path.GetFileName(sourceUri.LocalPath);
        var destinationPath = Path.Combine(downloadPath, fileName);

        try
        {
            Directory.CreateDirectory(downloadPath);

            using (HttpResponseMessage response =
                await httpClient.GetAsync(sourceUri, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();

                using (Stream source = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                using (var destination = File.Create(destinationPath))
                {
                    await source.CopyToAsync(destination, 81920, cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            return new[]
            {
                new DownloadSourceCodeResult(new Uri(destinationPath)),
            };
        }
        catch (Exception error) when (!(error is OperationCanceledException))
        {
            return new[]
            {
                new DownloadSourceCodeResult(error.Message, sourceUri),
            };
        }
    }
}
```

The host passes the projected row value and a local download directory to `CanExecute`. Return `false` for values the command cannot resolve or paths it cannot use. A host should call `CanExecute` before invoking `ExecuteAsync`, but implementations should still validate or safely reject their inputs.

`ExecuteAsync` controls the file layout beneath `downloadPath`. A row value may resolve to multiple source files, such as when it represents a stack frame containing inlined functions. Return one `DownloadSourceCodeResult` for each attempted download. The returned array may contain both successful and failed results: each success contains the URI of a downloaded file, while each failure contains its own error message and, optionally, the corresponding remote source URI. Allow cancellation to propagate as an `OperationCanceledException`.

Command implementations can be invoked on an arbitrary thread. They must be thread-safe, avoid accessing UI-thread state, honor the cancellation token, and use asynchronous I/O for downloads.

## Attaching Commands to a Column

Create the command collection once and pass it to the column:

```cs
var commands = new DataColumnCommands(new DownloadSourceCommand());

tableBuilderWithRowCount.AddColumn(
    new DataColumn<Uri>(sourceColumnConfiguration, sourceProjection, commands));
```

The strongly typed `ColumnBuilderBuilder<T>` can be used with `ITableBuilderWithRowCount` to add a column to a table:

```cs
tableBuilderWithRowCount.AddColumn(
    new ColumnBuilder<Uri>(sourceColumnConfiguration, sourceProjection)
        .WithCommands(commands));
```

`ColumnBuilder<T>` is mutable: `WithCommands` returns the same builder instance and may be chained as shown above. This differs from the functional builders used inside `AddColumnWithVariants` callbacks, where every method returns a new builder that must be returned or chained.

## Commands on Column Variants

Commands belong to the specific base column or variant to which they are attached. They are not inherited by related variants. To attach commands to a variant, use the builder overloads that supply a *variant builder* and call `WithCommands` on it. `WithToggleableBuilder` configures a toggle through a `ToggleableVariantBuilder`, and `WithModalBuilder` configures a mode through a `ModalVariantBuilder`:

```cs
tableBuilderWithRowCount.AddColumnWithVariants(
    sourceColumnConfiguration,
    sourceProjection,
    builder => builder.WithToggleableBuilder(
        localSourceDescriptor,
        localSourceProjection,
        variantBuilder => variantBuilder.WithCommands(commands)));
```

For a mode with child variants, chain `WithCommands` with `WithBuilder` on the `ModalVariantBuilder`. `WithBuilder` adds the nested toggles; `WithCommands` attaches the commands to the mode itself:

```cs
return modesBuilder.WithModalBuilder(
    sourceModeDescriptor,
    sourceProjection,
    variantBuilder => variantBuilder
        .WithCommands(commands)
        .WithBuilder(modeBuilder => modeBuilder.WithToggle(
            alternateSourceDescriptor,
            alternateSourceProjection)));
```

Hierarchical variants use the same pattern through `WithHierarchicalToggleableBuilder` and `WithHierarchicalModalBuilder`, which additionally take an `ICollectionInfoProvider<T>`. Attach commands only to variants whose projected values the command understands.

## Hierarchical Columns

For a `HierarchicalDataColumn<T>`, the value supplied to `CanExecute` and `ExecuteAsync` is the value displayed for the selected row. When the column uses an `ICollectionAccessProvider<T, TElement>`, this may be a `TElement` rather than the column's declared `T`. A command for a hierarchical column should therefore handle every displayed value type on which it can operate and return `false` from `CanExecute` for unsupported values.

## Host Discovery

A host discovers commands by testing whether an `IDataColumn` implements `IDataColumnWithCommands`, then querying its `Commands` property:

```cs
if (column is IDataColumnWithCommands columnWithCommands &&
    columnWithCommands.Commands.TryGetDownloadSourceCodeCommand(out var command) &&
    command.CanExecute(value, downloadPath))
{
    DownloadSourceCodeResult[] results =
        await command.ExecuteAsync(value, downloadPath, cancellationToken);

    foreach (DownloadSourceCodeResult result in results)
    {
        if (result.Success)
        {
            Open(result.Uri);
        }
        else
        {
            ShowError(result.ErrorMessage);
        }
    }
}
```

Hosts should use `CommandName` as the user-facing action name and process every returned result independently. A failed result's `Uri` may identify the remote source, but it is not a successfully downloaded resource and should not be opened as one.