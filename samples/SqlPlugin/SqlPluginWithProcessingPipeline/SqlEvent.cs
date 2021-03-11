// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;
using System;

namespace SqlPluginWithProcessingPipeline
{
    /// <summary>
    ///     A data class that holds all of the raw information from a trace event.
    ///     <para/>
    ///     Events are keyed by their EventClass, which has type string.
    /// </summary>
    public class SqlEvent : IKeyedDataType<string>
    {
        public SqlEvent(string eventClass,
                        string textData,
                        string applicationName,
                        string ntUserName,
                        string loginName,
                        int? cpu,
                        int? reads,
                        int? writes,
                        int? duration,
                        int? clientProcessId,
                        int? spid,
                        DateTime startTime,
                        DateTime? endTime)
        {
            this.EventClass = eventClass;
            this.TextData = textData;
            this.ApplicationName = applicationName;
            this.NTUserName = ntUserName;
            this.LoginName = loginName;
            this.CPU = cpu;
            this.Reads = reads;
            this.Writes = writes;
            this.Duration = duration;
            this.ClientProcessId = clientProcessId;
            this.SPID = spid;
            this.StartTime = startTime;
            this.EndTime = endTime;
        }

        public string EventClass { get; }
        public string TextData { get; }
        public string ApplicationName { get; }
        public string NTUserName { get; }
        public string LoginName { get; }
        public int? CPU { get; }
        public int? Reads { get; }
        public int? Writes { get; }
        public int? Duration { get; }
        public int? ClientProcessId { get; }
        public int? SPID { get; }
        public DateTime StartTime { get; }
        public DateTime? EndTime { get; }

        public int CompareTo(string other)
        {
            return this.EventClass.CompareTo(other);
        }

        public string GetKey()
        {
            return this.EventClass;
        }
    }

    /// <summary>
    ///     An extended SqlEvent that also holds a Timestamp that
    ///     represents how long after the trace start event this event
    ///     occurred.
    /// </summary>
    public class SqlEventWithRelativeTimestamp : SqlEvent
    {
        public SqlEventWithRelativeTimestamp(SqlEvent sqlEvent, Timestamp relativeTimestamp)
            : base(sqlEvent.EventClass,
                   sqlEvent.TextData,
                   sqlEvent.ApplicationName,
                   sqlEvent.NTUserName,
                   sqlEvent.LoginName,
                   sqlEvent.CPU,
                   sqlEvent.Reads,
                   sqlEvent.Writes,
                   sqlEvent.Duration,
                   sqlEvent.ClientProcessId,
                   sqlEvent.SPID,
                   sqlEvent.StartTime,
                   sqlEvent.EndTime)
        {
            this.RelativeTimestamp = relativeTimestamp;
        }

        public Timestamp RelativeTimestamp { get; set; }
    }
}
