using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
	public interface INotifyInsights : ITableService
	{
		//event EventHandler<ICollection<Insights.Insight>> InsightsUpdated;

		ICollection<Insights.Insight> GetInsights();
	}
}
