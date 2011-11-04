using System;
using System.IO;
using System.Net;

namespace WebGraph.Data
{
/// <summary>Loads text-data from the web</summary>
	public static class WebLoader
	{
		/// <summary>Loads data from the given url</summary>
		/// <param name="url">URL to load data from</param>
		/// <returns>Remote text on success, null otherwise</returns>
		public static string Get(string url)
		{
			string result = null;

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = "WebGraph.NET";
			request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			using (Stream stream = response.GetResponseStream())
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					result = reader.ReadToEnd();
				}
			}
			response.Close();

			return result;
		}
	}
}
