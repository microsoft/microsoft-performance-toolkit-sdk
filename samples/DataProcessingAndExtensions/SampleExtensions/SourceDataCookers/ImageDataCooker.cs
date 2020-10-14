using System.Collections.Generic;
using System.Threading;
using DataExtensionsSample;
using DataExtensionsSample.SourceDataCookers;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using SampleExtensions.DataTypes;
using SampleExtensions.Helpers;
using SampleExtensions.OutputTypes;

namespace SampleExtensions.SourceDataCookers
{
    public sealed class ImageDataCooker
        : SampleSourceDataCooker
    {
        public const string Identifier = "Image";
        public static readonly DataCookerPath CookerPath = new DataCookerPath(CustomSourceParser.SourceId, Identifier);

        private static class ImageDataKeys
        {
            public const string Load = "ImageLoad";
            public const string Unload = "ImageUnload";
        }

        private static readonly HashSet<string> dataKeys = new HashSet<string>
        {
            ImageDataKeys.Load,
            ImageDataKeys.Unload,
        };

        private List<ProcessImage> activeImages = new List<ProcessImage>();
        private HashSet<ProcessImage> processedImages = new HashSet<ProcessImage>();

        public ImageDataCooker()
            : base(Identifier)
        {
        }

        public override string Description => "Provides data about processes.";

        /// <summary>
        /// This cooker needs to see all events. So rather than specifying specific event keys here,
        /// the "Options" property is overriden below to receive all events.
        /// </summary>
        public override ReadOnlyHashSet<string> DataKeys => new ReadOnlyHashSet<string>(dataKeys);

        [DataOutput]
        public ImageLoadInfo ImagesInfo { get; private set; }

        /// <summary>
        /// This is called for each data element this data cooker has registered to receive.
        /// </summary>
        public override DataProcessingResult CookDataElement(
            SampleEvent data,
            ISampleEventContext context,
            CancellationToken cancellationToken)
        {
            switch(data.Name)
            {
                case ImageDataKeys.Load:
                    {
                        var image = JsonBinary.ToObject<ImageLoad>(data.Value);
                        var imageDescription = new ImageDescription
                        {
                            ImageSize = image.ImageSize,
                            Path = image.Path
                        };

                        this.activeImages.Add(new ProcessImage
                        {
                            Image = imageDescription,
                            ProcessId = image.ProcessId,
                            LoadTime = data.Timestamp,
                            LoadAddress = image.LoadAddress,
                        });
                    }
                    return DataProcessingResult.Processed;

                case ImageDataKeys.Unload:
                    {
                        var imageUnload = JsonBinary.ToObject<ImageUnload>(data.Value);
                        var imageDescription = new ImageDescription
                        {
                            ImageSize = imageUnload.ImageSize,
                            Path = imageUnload.Path
                        };

                        int index = this.activeImages.FindIndex(i => i.Image.Equals(imageDescription) && i.ProcessId == imageUnload.ProcessId && i.LoadTime < data.Timestamp);
                        if (index == -1)
                        {
                            // This image was loaded before this data set started recording

                            this.processedImages.Add(new ProcessImage
                            {
                                Image = imageDescription,
                                ProcessId = imageUnload.ProcessId,
                                LoadTime = Timestamp.MinValue,
                                LoadAddress = imageUnload.LoadAddress,
                                UnloadTime = data.Timestamp
                            });
                        }
                        else
                        {
                            var image = this.activeImages[index];
                            this.activeImages.RemoveAt(index);

                            image.UnloadTime = data.Timestamp;
                            this.processedImages.Add(image);
                        }

                        return DataProcessingResult.Processed;
                    }
            }

            return DataProcessingResult.Ignored;
        }

        public override void EndDataCooking(CancellationToken cancellationToken)
        {
            for (int x = 0; x < this.activeImages.Count; x++)
            {
                var image = this.activeImages[x];
                image.UnloadTime = Timestamp.MaxValue;
                this.processedImages.Add(image);
            }
            this.ImagesInfo = new ImageLoadInfo(this.processedImages);
        }
    }
}
