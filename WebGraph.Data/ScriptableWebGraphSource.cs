using System;
using System.IO;

namespace WebGraph.Data
{
	/// <summary>A data source customizable with JavaScript</summary>
	public class ScriptableWebGraphSource : IWebGraphSource
	{
		private Jint.JintEngine _Script;

		/// <summary>Initializes the graph source</summary>
		/// <param name="scriptfile">Filename of a JavaScript file</param>
		public ScriptableWebGraphSource(string scriptfile)
		{
			string source = File.ReadAllText(scriptfile);
			_Script = new Jint.JintEngine();
			_Script.Run(source);
		}

		/// <summary>Get the URL to load the data from</summary>
		/// <param name="root">name of the root node to load data for</param>
		/// <returns>URL of raw data</returns>
		public string GetContentUrl(string root)
		{
			return _Script.CallFunction("getContentUrl", root).ToString();
		}

		/// <summary>Extract a small teaser text from raw data</summary>
		/// <param name="data">Raw data</param>
		/// <returns>Teaser text</returns>
		public string GetTeaser(string data)
		{
			return _Script.CallFunction("getTeaser", data).ToString();
		}

		/// <summary>Get all node names connected to a given root</summary>
		/// <param name="data">Raw data of the root node</param>
		/// <returns>Names of connected nodes</returns>
		public string[] GetKeywords(string data)
		{
			Jint.Native.JsArray result = (Jint.Native.JsArray)_Script.CallFunction("getKeywords", data);

			string[] keywords = new string[result.Length];
			for (int i = 0; i < result.Length; i++)
				keywords[i] = result.get(i).ToString();

			return keywords;
		}

		/// <summary>Gets the self-description of the script calling its getPluginTitle function</summary>
		/// <returns>Description of the script</returns>
		public override string ToString()
		{
			return _Script.CallFunction("getPluginTitle").ToString();
		}
	}
}
