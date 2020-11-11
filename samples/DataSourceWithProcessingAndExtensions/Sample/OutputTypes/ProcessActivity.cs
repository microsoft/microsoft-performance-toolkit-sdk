using System;
using Microsoft.Performance.SDK;

namespace DataExtensionsSample.OutputTypes
{
    public struct ProcessActivity
        : IEquatable<ProcessActivity>
    {
        public Timestamp StartTime;

        public Timestamp StopTime;

        public string ProcessName;

        public uint ProcessId;

        public bool Equals(ProcessActivity other)
        {
            return
                this.ProcessId == other.ProcessId &&
                this.StartTime == other.StartTime &&
                this.StopTime == other.StopTime &&
                StringComparer.InvariantCultureIgnoreCase.Equals(this.ProcessName, other.ProcessName);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is ProcessActivity))
            {
                return false;
            }

            return Equals((ProcessActivity)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = -303075029;
            hashCode = hashCode * -1521134295 + this.StartTime.GetHashCode();
            hashCode = hashCode * -1521134295 + this.StopTime.GetHashCode();
            hashCode = hashCode * -1521134295 + this.ProcessName.GetHashCode();
            hashCode = hashCode * -1521134295 + this.ProcessId.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(ProcessActivity a, ProcessActivity b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ProcessActivity a, ProcessActivity b)
        {
            return !(a == b);
        }
    }
}