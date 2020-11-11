using System.Collections.Generic;
using System.Linq;

namespace SampleExtensions.OutputTypes
{
    public sealed class ImageLoadInfo
    {
        private List<ProcessImage> loadedImages;

        internal ImageLoadInfo(HashSet<ProcessImage> loadedImages)
        {
            this.loadedImages = loadedImages.ToList();
            this.Images = this.loadedImages.AsReadOnly();
        }

        public IReadOnlyList<ProcessImage> Images { get; }
    }
}
