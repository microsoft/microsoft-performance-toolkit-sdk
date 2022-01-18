using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Insights
{
	public sealed class Insight
	{
		public static IDictionary<Guid, ICollection<Insight>> Insights = new Dictionary<Guid, ICollection<Insight>>();

		/// <summary>
		///   Initializes a new instance of the <see cref="Insight"/> class.
		/// </summary>
		/// <param name="title"> The title of the insight</param>
		/// <param name="description"> The description of the Insight </param>
		/// <param name="action"> A callback to do some calculations with the table if necessary. </param>
		public Insight(string title, string description, WpaUiAction action)
		{
			this.Title = title;
			this.Description = description;
			this.Tag = action;
		}

		// optionally an image
		public string Title { get; }
		public string Description { get; }
		public WpaUiAction Tag { get; } // This action is how WPA will react.
										// Maybe we can have a small library of actions with an enum 

	}

	public enum WpaUiAction
	{
		ZoomIn,
		ZoomOut,
		FilterTo,
		AnnotateSelection,
		CopyCellValue,
		LoadSymbols,
		HighlightTimeRange,
		SortTable,
		OpenWindow,
		OpenDialog,
		OpenContextMenu
	}
}
