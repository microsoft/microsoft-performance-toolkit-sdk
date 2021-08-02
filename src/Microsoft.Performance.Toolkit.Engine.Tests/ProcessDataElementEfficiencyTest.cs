using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections.Generic;

/// <summary>
///  Sanity Check Benchmark testing to see if IEnumerable is more efficient than using a list.
///  <see cref="BaseSourceProcessingSession.ProcessDataElement(T, TContext, System.Threading.CancellationToken)"/>
/// </summary>
public class ProcessForLoopEfficiency
{
	private const int NumCookers = 1000;

	private readonly List<int> _list;
	private readonly IEnumerable<int> _enumerable;

	public ProcessForLoopEfficiency()
	{
		_list = new List<int>();

		for (int cooker = 0; cooker < NumCookers; cooker++)
		{
			// create a new ISourceDataCooker
			//TestSourceDataCooker cooker = new TestSourceDataCooker();
			_list.Add(cooker);
		}

		_enumerable = (IEnumerable<int>)_list;
	}


	[Benchmark]
	public void EnumerableProcessDataCookers()
	{
		foreach (var cooker in _enumerable)
		{
			var newCooker = cooker;
			var anotherCooker = newCooker;
		}
	}

	[Benchmark]
	public void ListProcessDataCookers()
	{
		for (int cookerIdx = 0; cookerIdx < _list.Count; cookerIdx++)
		{
			var sourceDataCooker = _list[cookerIdx];
			var newCooker = sourceDataCooker;
			var anotherCooker = newCooker;
		}
	}

	static void Main(string[] args)
	{
		BenchmarkRunner.Run<ProcessForLoopEfficiency>();
	}
}

