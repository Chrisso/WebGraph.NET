using System;
using System.Data;
using System.Data.SQLite;

namespace WebGraph.Data
{
	/// <summary>Database cache for text-data retrieved from the web</summary>
	public class WebCache
	{
		private string _File;

		/// <summary>Initialization ctor</summary>
		/// <param name="file">Name of database file, will be created if not existing</param>
		public WebCache(string file = "Cache.dat")
		{
			_File = file;
			if (!System.IO.File.Exists(_File))
			{
				SQLiteConnection connection = new SQLiteConnection("UseUTF16Encoding=True;Data Source=" + _File);
				try
				{
					connection.Open();

					string[] inits = new string[]{ 
						"CREATE TABLE [InetCache] ([Id] INTEGER PRIMARY KEY, [Timestamp] INTEGER, [Keyword] TEXT NOT NULL, [Content] TEXT NOT NULL)", 
						"CREATE INDEX [IDX_InetCache] ON [InetCache] ([Keyword])"
					};

					SQLiteCommand command = connection.CreateCommand();
					foreach (string cmd in inits)
					{
						command.CommandText = cmd;
						command.ExecuteNonQuery();
					}
				}
				finally
				{
					connection.Close();
				}
			}
		}

		/// <summary>Attemps to load cached data</summary>
		/// <param name="key">URL to load data for</param>
		/// <returns>cached data if existing, null otherwise</returns>
		public string Load(string key)
		{
			SQLiteConnection connection = new SQLiteConnection("UseUTF16Encoding=True;Data Source=" + _File);
			try
			{
				connection.Open();

				SQLiteCommand command = connection.CreateCommand();
				command.CommandText = "SELECT [Content] FROM [InetCache] WHERE [Keyword]=?";
				command.Parameters.Add("kw", DbType.String).Value = key;

				object result = command.ExecuteScalar();
				return (result == null) ? null : result.ToString();
			}
			finally
			{
				connection.Close();
			}
		}

		/// <summary>Write data with a given key to cache</summary>
		/// <param name="key">Key of data to be stored</param>
		/// <param name="data">Data to be stored</param>
		public void Store(string key, string data)
		{
			SQLiteConnection connection = new SQLiteConnection("UseUTF16Encoding=True;Data Source=" + _File);
			try
			{
				DateTime now = DateTime.Now; 
				connection.Open();

				SQLiteCommand command = connection.CreateCommand();
				command.CommandText = "DELETE FROM [InetCache] WHERE [Keyword]=?";
				command.Parameters.Add("kw", DbType.String).Value = key;
				command.ExecuteNonQuery();	// out with the old

				command = connection.CreateCommand();
				command.CommandText = "INSERT INTO [InetCache] ([Timestamp], [Keyword], [Content]) VALUES (?, ?, ?)";
				command.Parameters.Add("ts", DbType.Int32).Value = now.Year * 10000 + now.Month * 100 + now.Day;
				command.Parameters.Add("kw", DbType.String).Value = key;
				command.Parameters.Add("dt", DbType.String).Value = data;
				command.ExecuteNonQuery();	// in with the new
			}
			finally
			{
				connection.Close();
			}
		}
	}
}
