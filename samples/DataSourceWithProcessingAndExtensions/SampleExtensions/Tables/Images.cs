using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using SampleExtensions.CompositeCookers;
using SampleExtensions.OutputTypes;
using SampleExtensions.SourceDataCookers;

namespace SampleExtensions.Tables
{
    [Table]
    [PrebuiltConfigurationsFilePath("Resources\\ImagesTableConfiguration.json")]
    public class Images
    {
        public static TableDescriptor TableDescriptor = new TableDescriptor(
            Guid.Parse("{3A6CF5EA-AB9E-4FC7-8AB3-1B8E5F5CDAE0}"),
            "Images",
            "Images with process names",
            requiredDataCookers:new List<DataCookerPath> { ImagesWithProcessInfo.CookerPath });

        private static readonly ColumnConfiguration imagePathColumnConfig =
            new ColumnConfiguration(
                new ColumnMetadata(new Guid("{8ABAC44B-9582-419E-A0C4-14240E905E94}"), "Image Path"),
                new UIHints { Width = 80, });

        private static readonly ColumnConfiguration processIdColumnConfig =
            new ColumnConfiguration(
                new ColumnMetadata(new Guid("{544DC6FC-2B99-4B02-900C-2B6026E9CD17}"), "Pid"),
                new UIHints { Width = 80, });

        private static readonly ColumnConfiguration loadTimeColumnConfig =
            new ColumnConfiguration(
                new ColumnMetadata(new Guid("{A63A086C-2A45-4844-B2C9-0C82B0976572}"), "Load Time"),
                new UIHints { Width = 80, });

        private static readonly ColumnConfiguration unloadTimeColumnConfig =
            new ColumnConfiguration(
                new ColumnMetadata(new Guid("{2D336D80-5201-4C5E-9580-0C73F8FB9507}"), "Unload Time"),
                new UIHints { Width = 80, });

        private static readonly ColumnConfiguration loadAddressColumnConfig =
            new ColumnConfiguration(
                new ColumnMetadata(new Guid("{AFED486E-1E0F-46CB-9E1B-5033E6D6DCFF}"), "Load Address"),
                new UIHints { Width = 80, });

        private static readonly ColumnConfiguration imageSizeColumnConfig =
            new ColumnConfiguration(
                new ColumnMetadata(new Guid("{FDF8E716-3C49-48E0-9530-C7E63BD21FB0}"), "Image Size"),
                new UIHints { Width = 80, });

        private static readonly ColumnConfiguration processnameColumnConfig =
            new ColumnConfiguration(
                new ColumnMetadata(new Guid("{E64B7F42-9A90-4DB6-82C3-A6956B0816EB}"), "Process Name"),
                new UIHints { Width = 120, });

        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval tableData)
        {
            var imagesInfo = tableData.QueryOutput<FullImageInfo>(
                new DataOutputPath(ImagesWithProcessInfo.CookerPath, nameof(ImagesWithProcessInfo.ImageInfoData)));
            if (imagesInfo.ImageLoadInfo.Images.Count == 0)
            {
                return;
            }

            var table = tableBuilder.SetRowCount(imagesInfo.ImageLoadInfo.Images.Count);
            var imageProjection = Projection.CreateUsingFuncAdaptor((i) => imagesInfo.ImageLoadInfo.Images[i]);

            table.AddColumn(imagePathColumnConfig, imageProjection.Compose(image => image.Image.Path));
            table.AddColumn(loadAddressColumnConfig, imageProjection.Compose(image => image.LoadAddress));
            table.AddColumn(imageSizeColumnConfig, imageProjection.Compose(image => image.Image.ImageSize));
            table.AddColumn(processIdColumnConfig, imageProjection.Compose(image => image.ProcessId));
            table.AddColumn(loadTimeColumnConfig, imageProjection.Compose(image => image.LoadTime));
            table.AddColumn(unloadTimeColumnConfig, imageProjection.Compose(image => image.UnloadTime));
            table.AddColumn(processnameColumnConfig, imageProjection.Compose(image => imagesInfo.GetProcessName(image)));
        }
    }
}
