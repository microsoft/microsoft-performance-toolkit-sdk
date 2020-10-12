using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;

namespace DataExtensionsSample.SourceDataCookers
{
    public abstract class SampleSourceDataCooker
        : BaseSourceDataCooker<SampleEvent, ISampleEventContext, string>
    {
        protected SampleSourceDataCooker(string cookerId)
            : base(new DataCookerPath(CustomSourceParser.SourceId, cookerId))
        {
        }
    }
}