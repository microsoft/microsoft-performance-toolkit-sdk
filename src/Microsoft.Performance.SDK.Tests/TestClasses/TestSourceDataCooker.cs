// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;
using Microsoft.Performance.SDK.Tests.DataTypes;
using Microsoft.Performance.SDK.Tests.TestClasses;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    public class TestSourceDataCookerContext
    {
        private List<(DataCookerPath, string)> methodCalls = new List<(DataCookerPath, string)>();

        public int CountOfTestRecordsReceived;

        public void RecordMethodCall(DataCookerPath cooker, [CallerMemberName] string method = "")
        {
            this.methodCalls.Add((cooker, method));
        }

        public IList<(DataCookerPath, string)> MethodCallOrder => this.methodCalls;
    }

    public class TestSourceDataCooker
        : ISourceDataCooker<TestRecord, TestParserContext, int>
    {
        public TestSourceDataCooker()
        {
            this.Context = new TestSourceDataCookerContext();
        }

        public TestSourceDataCooker(TestSourceDataCookerContext context)
        {
            this.Context = context;
        }

        public string Description { get; set; }

        public T QueryOutput<T>(DataOutputPath identifier)
        {
            if (Reflector == null)
            {
                throw new NotImplementedException();
            }

            return Reflector.QueryOutput<T>(identifier);
        }

        public object QueryOutput(DataOutputPath identifier)
        {
            if (Reflector == null)
            {
                throw new NotImplementedException();
            }

            return Reflector.QueryOutput(identifier);
        }

        public bool TryQueryOutput<T>(DataOutputPath identifier, out T result)
        {
            if (Reflector == null)
            {
                throw new NotImplementedException();
            }

            return Reflector.TryQueryOutput<T>(identifier, out result);
        }

        public bool TryQueryOutput(DataOutputPath identifier, out object result)
        {
            if (Reflector == null)
            {
                throw new NotImplementedException();
            }

            return Reflector.TryQueryOutput(identifier, out result);
        }

        public IReadOnlyCollection<DataOutputPath> OutputIdentifiers { get; set; }

        public IReadOnlyCollection<DataCookerPath> RequiredDataCookers { get; set; }

        public IReadOnlyDictionary<DataCookerPath, DataCookerDependencyType> DependencyTypes { get; set; }

        public DataProductionStrategy DataProductionStrategy { get; set; }

        public ReadOnlyHashSet<int> DataKeys { get; set; }

        public SourceDataCookerOptions Options { get; set; }

        public int BeginDataCookingCallCount { get; private set; } = 0;
        public Action<CancellationToken> BeginDataCookingAction { get; set; } = null;
        public void BeginDataCooking(ICookedDataRetrieval dependencyRetrieval, CancellationToken cancellationToken)
        {
            this.Context.RecordMethodCall(this.Path);

            this.BeginDataCookingCallCount++;
            this.BeginDataCookingAction?.Invoke(cancellationToken);
        }

        public int CookDataElementCallCount { get; private set; } = 0;
        public Func<TestRecord, TestParserContext, CancellationToken, DataProcessingResult> CookDataElementFunc { get; set; } = null;
        public DataProcessingResult CookDataElementResult { get; set; } = DataProcessingResult.Processed;
        public Dictionary<TestRecord, int> ReceivedRecords = new Dictionary<TestRecord, int>();
        public DataProcessingResult CookDataElement(TestRecord data, TestParserContext context, CancellationToken cancellationToken)
        {
            this.Context.RecordMethodCall(this.Path);

            this.ReceivedRecords.Add(data, this.Context.CountOfTestRecordsReceived);
            this.Context.CountOfTestRecordsReceived++;

            this.CookDataElementCallCount++;
            if (this.CookDataElementFunc != null)
            {
                return this.CookDataElementFunc(data, context, cancellationToken);
            }

            return this.CookDataElementResult;
        }

        public int EndDataCookingCallCount { get; private set; } = 0;
        public Action<CancellationToken> EndDataCookingAction { get; set; } = null;
        public void EndDataCooking(CancellationToken cancellationToken)
        {
            this.Context.RecordMethodCall(this.Path);

            this.EndDataCookingCallCount++;
            this.EndDataCookingAction?.Invoke(cancellationToken);
        }

        public DataCookerPath Path { get; set; }

        //public int CountOfTestRecordsReceived = 0;

        public TestSourceDataCookerContext Context { get; set; }


        public TestCookedDataReflector Reflector { get; private set; }

        public void SetCookedDataReflector(TestCookedDataReflector reflector)
        {
            Reflector = reflector;
            OutputIdentifiers = reflector.OutputIdentifiers;
        }
    }
}
