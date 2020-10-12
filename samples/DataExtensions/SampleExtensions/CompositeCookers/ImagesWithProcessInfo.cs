using System;
using System.Collections.Generic;
using DataExtensionsSample.OutputTypes;
using DataExtensionsSample.SourceDataCookers;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using SampleExtensions.OutputTypes;
using SampleExtensions.SourceDataCookers;

namespace SampleExtensions.CompositeCookers
{
    public class ImagesWithProcessInfo
        : CookedDataReflector
        , ICompositeDataCookerDescriptor
    {
        public const string Identifier = "ImagesWithProcessInfo";
        public static readonly DataCookerPath CookerPath = new DataCookerPath(Identifier);

        private static readonly HashSet<DataCookerPath> RequiredPaths = new HashSet<DataCookerPath>
        {
            ProcessDataCooker.CookerPath,
            ImageDataCooker.CookerPath
        };

        public ImagesWithProcessInfo()
            : base(CookerPath)
        {
        }

        public string Description => "Combines images with process info.";

        public DataCookerPath Path => CookerPath;

        public IReadOnlyCollection<DataCookerPath> RequiredDataCookers => RequiredPaths;

        public void OnDataAvailable(IDataExtensionRetrieval requiredData)
        {
            var processInfo = requiredData.QueryOutput<ProcessInfo>(
                new DataOutputPath(ProcessDataCooker.CookerPath, "ProcessInfo"));

            var imagesInfo = requiredData.QueryOutput<ImageLoadInfo>(
                new DataOutputPath(ImageDataCooker.CookerPath, nameof(ImageDataCooker.ImagesInfo)));

            this.ImageInfoData = new FullImageInfo(processInfo, imagesInfo);
        }

        [DataOutput]
        public FullImageInfo ImageInfoData { get; private set; }
    }
}
