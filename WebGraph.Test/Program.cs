using System;

namespace WebGraph.Test
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				WebGraph.Data.IWebGraphSource source= new WebGraph.Data.ScriptableWebGraphSource("Script.es");
				Console.WriteLine(source.ToString());
				Console.WriteLine(source.GetContentUrl("Test"));
				Console.WriteLine(source.GetKeywords("test").Length);

				WebGraph.Data.WebCache cache = new Data.WebCache();
				cache.Store("Test", "Cached");
				Console.WriteLine(cache.Load("Test"));

				string load = WebGraph.Data.WebLoader.Get("http://de.wikipedia.org/wiki/Spezial:Exportieren/Abstraktionsprinzip");
				Console.WriteLine(load.Substring(0, 78));
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine(exc.GetType().FullName);
				Console.Error.WriteLine(exc.Message);
			}

			Console.WriteLine("Any key to exit.");
			Console.ReadKey();
		}
	}
}
