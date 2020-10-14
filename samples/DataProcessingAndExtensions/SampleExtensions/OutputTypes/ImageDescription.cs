using System;

namespace SampleExtensions.OutputTypes
{
    public struct ImageDescription
        : IEquatable<ImageDescription>
    {
        public string Path;

        public uint ImageSize;

        public bool Equals(ImageDescription other)
        {
            return
                this.ImageSize == other.ImageSize &&
                StringComparer.InvariantCultureIgnoreCase.Equals(this.Path, other.Path);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is ImageDescription))
            {
                return false;
            }

            return Equals((ImageDescription)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = -303075029;
            hashCode = hashCode * -1521134295 + this.Path.GetHashCode();
            hashCode = hashCode * -1521134295 + this.ImageSize.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(ImageDescription a, ImageDescription b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ImageDescription a, ImageDescription b)
        {
            return !(a == b);
        }
    }
}
