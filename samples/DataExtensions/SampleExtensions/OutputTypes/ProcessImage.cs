using System;
using Microsoft.Performance.SDK;

namespace SampleExtensions.OutputTypes
{
    public struct ProcessImage
        : IEquatable<ProcessImage>
    {
        public ImageDescription Image;

        public uint ProcessId;

        public Timestamp LoadTime;

        public Timestamp UnloadTime;

        public uint LoadAddress;

        public bool Equals(ProcessImage other)
        {
            return
                this.LoadAddress == other.LoadAddress &&
                this.ProcessId == other.ProcessId &&
                this.LoadTime == other.LoadTime &&
                this.UnloadTime == other.UnloadTime &&
                this.Image.Equals(other.Image);
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            if(!(obj is ProcessImage))
            {
                return false;
            }

            return Equals((ProcessImage)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = -303075029;
            hashCode = hashCode * -1521134295 + this.Image.GetHashCode();
            hashCode = hashCode * -1521134295 + this.LoadTime.GetHashCode();
            hashCode = hashCode * -1521134295 + this.UnloadTime.GetHashCode();
            hashCode = hashCode * -1521134295 + this.ProcessId.GetHashCode();
            hashCode = hashCode * -1521134295 + this.LoadAddress.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(ProcessImage a, ProcessImage b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ProcessImage a, ProcessImage b)
        {
            return !(a == b);
        }
    }
}
