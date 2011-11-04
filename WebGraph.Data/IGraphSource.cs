using System;

namespace WebGraph.Data
{
	/// <summary>Basic graph source to get all nodes connected with a root</summary>
	public interface IGraphSource
	{
		/// <summary>Get all node names connected to a given root</summary>
		/// <param name="root">Name of the root node</param>
		/// <returns>Names of connected nodes</returns>
		string[] GetKeywords(string root);
	}

	/// <summary>Graph source with data from the web</summary>
	public interface IWebGraphSource : IGraphSource
	{
		/// <summary>Get the URL to load the data from</summary>
		/// <param name="root">name of the root node to load data for</param>
		/// <returns>URL of raw data</returns>
		string GetContentUrl(string root);

		/// <summary>Extract a small teaser text from raw data</summary>
		/// <param name="data">Raw data</param>
		/// <returns>Teaser text</returns>
		string GetTeaser(string data);
	}
}
