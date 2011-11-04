using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Threading;

namespace WebGraph
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private WebGraph.Logic.Graph Graph;
		private WebGraph.Logic.GraphLayouter Layouter;
		private string FileName;

		public MainWindow()
		{
			InitializeComponent();

			Graph = new WebGraph.Logic.Graph();
			Layouter = new WebGraph.Logic.GraphLayouter(Graph);
			FileName = string.Empty;
			CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			foreach (string plugin in System.IO.Directory.GetFiles(path, "*.es"))
			{
				try
				{
					// initialize datasources dropdown
					WebGraph.Data.ScriptableWebGraphSource src = new Data.ScriptableWebGraphSource(plugin);
					comboBoxPlugins.Items.Add(src);
				}
				catch (Exception exc)
				{
					// there was ja javascript error in this plugin
					System.Diagnostics.Debug.WriteLine(exc.GetType().FullName);
					System.Diagnostics.Debug.WriteLine(exc.Message);
				}
			}
			if (comboBoxPlugins.Items.Count != 0)
			{
				comboBoxPlugins.SelectedIndex = 0;
				statusText.Text = string.Format("Bereit ({0} Datenquellen geladen)", comboBoxPlugins.Items.Count);
			}
		}

		#region file menu
		private void OnFileNew(object sender, ExecutedRoutedEventArgs e)
		{
			// throw away internal data
			Graph = new WebGraph.Logic.Graph();
			Layouter = new WebGraph.Logic.GraphLayouter(Graph);
			FileName = string.Empty;

			// and update display to show nothing
			graphCanvas.Children.Clear();
		}

		private void OnFileSave(object sender, ExecutedRoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(FileName))
				WebGraph.Logic.GraphXmlSerializer.Serialize(Graph, FileName);
			else
				OnFileSaveAs(this, null);
		}

		private void OnFileSaveAs(object sender, ExecutedRoutedEventArgs e)
		{
			Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
			sfd.RestoreDirectory = true;
			sfd.DefaultExt = ".sgx";
			sfd.Filter = "GraphXML (.sgx)|*.sgx|All Files|*.*";

			if (sfd.ShowDialog() == true)
			{
				FileName = sfd.FileName;
				WebGraph.Logic.GraphXmlSerializer.Serialize(Graph, FileName);
			}
		}

		private void OnFileOpen(object sender, ExecutedRoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
			ofd.RestoreDirectory = true;
			ofd.DefaultExt = ".sgx";
			ofd.Filter = "GraphXML (.sgx)|*.sgx|All Files|*.*";

			if (ofd.ShowDialog() == true)
			{
				graphCanvas.Children.Clear();
				try
				{
					FileName = ofd.FileName;
					Graph = WebGraph.Logic.GraphXmlSerializer.Deserialize(FileName);
					Layouter = new WebGraph.Logic.GraphLayouter(Graph);

					Brush nodeFg = new SolidColorBrush(Properties.Settings.Default.NodeForeground);
					Brush nodeBg = new SolidColorBrush(Properties.Settings.Default.NodeBackground);

					Graph.ForAllEdges(edge =>
						{
							Line line = new Line();
							line.Stroke = nodeBg;
							edge.Tag = line;
							graphCanvas.Children.Add(line);
						});

					Graph.ForAllNodes(node =>
						{
							TextBlock text = new TextBlock();
							text.Text = node.Label;
							text.Foreground = nodeFg;
							text.Background = nodeBg;
							text.Padding = new Thickness(4, 0, 4, 0);
							text.FontSize = 12;
							node.Tag = text;
							graphCanvas.Children.Add(text);
						});					
				}
				catch (Exception exc)
				{
					OnFileNew(this, null);	// force a complete reset
					MessageBox.Show(exc.Message, exc.GetType().FullName);
				}

				graphCanvas.UpdateLayout();
				Graph.ForAllNodes(new WebGraph.Logic.NodeCallback(CenterNodes));
				Graph.ForAllNodes(new WebGraph.Logic.NodeCallback(UpdateNodeSize));
				Layouter.ResetDamper();
			}
		}

		private void OnClose(object sender, ExecutedRoutedEventArgs e)
		{
			Close();
		}
		#endregion

		#region help menu
		private void OnHelp(object sender, ExecutedRoutedEventArgs e)
		{
			MessageBox.Show("WenGraph.NET v1.0 (c) 2011 Christoph Stoepel\nhttp://christoph.stoepel.net");
		}
		#endregion

		#region asynchronous loading

		class LoaderState
		{
			public int Depth { get; set; }
			public string Root { get; set; }
			public string Url { get; set; }
			public string Result { get; set; }
			public bool Cached { get; set; }
			public WebGraph.Data.IWebGraphSource Source { get; set; }
			public Exception Exception { get; set; }
		}

		/// <summary>Worker-Thread for loading data from the web</summary>
		/// <param name="state">Combined paramters and results</param>
		private void OnWebDataLoad(object state)
		{
			LoaderState ls = (LoaderState)state;
			
			try
			{
				System.Diagnostics.Debug.WriteLine(string.Format("Loading data from {0}.", ls.Url));
				ls.Result = WebGraph.Data.WebLoader.Get(ls.Url);	// this is the timeconsuming part
				ls.Cached = false;	// which should be avoided next time
				System.Diagnostics.Debug.WriteLine(string.Format("Finished loading {0}.", ls.Url));
			}
			catch (Exception exc)
			{
				System.Diagnostics.Debug.WriteLine(exc.GetType().FullName);
				System.Diagnostics.Debug.WriteLine(exc.Message);
				ls.Exception = exc;
			}

			// return result to gui-thread (will be queued)
			Dispatcher.BeginInvoke(new WaitCallback(OnWebDataLoaded), ls);
		}

		/// <summary>Processes loaded data (from web or cache)</summary>
		/// <param name="state">Combined paramters and results</param>
		private void OnWebDataLoaded(object state)
		{
			buttonGo.IsEnabled = true;
			statusText.Text = "Bereit.";	// TODO: localized resource
			LoaderState ls = (LoaderState)state;

			if (ls.Exception != null)
			{
				MessageBox.Show(ls.Exception.Message, ls.Exception.GetType().FullName);
				return;	// no way to continue
			}

			if (ls.Result == null)
				return;	// an unexpected error occured

			if (!ls.Cached)
			{
				System.Diagnostics.Debug.WriteLine(string.Format("Caching item {0}.", ls.Url));
				new WebGraph.Data.WebCache().Store(ls.Url, ls.Result);
			}

			Brush nodeFg = new SolidColorBrush(Properties.Settings.Default.NodeForeground);
			Brush nodeBg = new SolidColorBrush(Properties.Settings.Default.NodeBackground);

			WebGraph.Logic.Node root = Graph.FindNode(ls.Root);
			if (root == null) // is this a new graph?
			{
				TextBlock text = new TextBlock();
				text.Text = ls.Root;
				text.Foreground = nodeFg;
				text.Background = nodeBg;
				text.Padding = new Thickness(4, 0, 4, 0);
				text.FontSize = 12;
				Canvas.SetZIndex(text, Int32.MaxValue);	// always on top
				graphCanvas.Children.Add(text);				

				root = Graph.AddNode(ls.Root);
				root.Tag = text;
			}

			try
			{
				string[] keywords = ls.Source.GetKeywords(ls.Result);
				for (int i=0; i<Math.Min(12, keywords.Length); i++)	// add childnode
				{
					if (Graph.FindNode(keywords[i]) == null)	// no circles please
					{
						Line line = new Line();
						line.Stroke = nodeBg;

						TextBlock text = new TextBlock();
						text.Text = keywords[i];
						text.Foreground = nodeFg;
						text.Background = nodeBg;
						text.Padding = new Thickness(4, 0, 4, 0);
						text.FontSize = 12;

						graphCanvas.Children.Add(line);
						graphCanvas.Children.Add(text);

						WebGraph.Logic.Node node = Graph.AddNode(keywords[i]);
						WebGraph.Logic.Edge edge = Graph.AddEdge(root, node);
						node.Tag = text;
						edge.Tag = line;

						if (ls.Depth < Properties.Settings.Default.MaxRecursionDepth)
							LoadSubGraph(keywords[i], ls.Depth + 1);	// recurse into subgraphs
					}
				}
			}
			catch (Exception exc)
			{
				System.Diagnostics.Debug.WriteLine(exc.GetType().FullName);
				System.Diagnostics.Debug.WriteLine(exc.Message);
			}

			graphCanvas.UpdateLayout();
			Graph.ForAllNodes(new WebGraph.Logic.NodeCallback(CenterNodes));
			Graph.ForAllNodes(new WebGraph.Logic.NodeCallback(UpdateNodeSize));
			Layouter.ResetDamper();
		}

		#endregion

		#region custom layouting callbacks
		
		private void CenterNodes(WebGraph.Logic.Node node)
		{
			node.drawx = node.x + graphCanvas.Width / 2;
			node.drawy = node.y + graphCanvas.Height / 2;
		}

		private void MoveVisualNodes(WebGraph.Logic.Node node)
		{
			Canvas.SetLeft((TextBlock)node.Tag, node.drawx - node.Width / 2);
			Canvas.SetTop((TextBlock)node.Tag, node.drawy - node.Height / 2);
		}

		private void MoveVisualEdges(WebGraph.Logic.Edge edge)
		{
			Line line = (Line)edge.Tag;

			line.X1 = edge.From.drawx;
			line.Y1 = edge.From.drawy;

			line.X2 = edge.To.drawx;
			line.Y2 = edge.To.drawy;
		}

		private void UpdateNodeSize(WebGraph.Logic.Node node)
		{
			node.Width = ((TextBlock)node.Tag).ActualWidth;
			node.Height = ((TextBlock)node.Tag).ActualHeight;
		}

		#endregion

		/// <summary>This is the main rendering function, animations costly stuff happens here</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void CompositionTarget_Rendering(object sender, EventArgs e)
		{
			Layouter.Relax();	// do positioning
			Graph.ForAllNodes(new WebGraph.Logic.NodeCallback(CenterNodes));
			Graph.ForAllNodes(new WebGraph.Logic.NodeCallback(MoveVisualNodes));
			Graph.ForAllEdges(new WebGraph.Logic.EdgeCallback(MoveVisualEdges));
		}

		/// <summary>Loads data for a graph either from cache or starts a download thread</summary>
		/// <param name="root">Name of root node</param>
		/// <param name="depth">Depth of this graph for resursion counting</param>
		private void LoadSubGraph(string root, int depth)
		{
			WebGraph.Data.IWebGraphSource source = comboBoxPlugins.SelectedItem as WebGraph.Data.IWebGraphSource;
			WebGraph.Data.WebCache cache = new WebGraph.Data.WebCache();

			Graph.Tag = source.ToString();

			LoaderState state = new LoaderState();
			state.Depth = depth;
			state.Source = source;
			state.Root = root;
			state.Url = source.GetContentUrl(state.Root);
			state.Result = cache.Load(state.Url);

			if (state.Result != null)	// cache hit
			{
				System.Diagnostics.Debug.WriteLine(string.Format("Using cached item {0}.", state.Url));
				state.Cached = true;
				OnWebDataLoaded(state);	// just go one
			}
			else	// cache miss
			{
				buttonGo.IsEnabled = false;	// no new graphs until this one is ready
				statusText.Text = "Lade " + state.Url;
				// this one gets expensive, so do ít async
				ThreadPool.QueueUserWorkItem(new WaitCallback(OnWebDataLoad), state);
			}
		}

		/// <summary>Loads a new graph for a given root</summary>
		/// <param name="sender">Sender</param>
		/// <param name="e">Arguments</param>
		private void ButtonGoClick(object sender, RoutedEventArgs e)
		{
			if (textBoxSearch.Text.Length == 0)
				return;	// nothing to search

			if (comboBoxPlugins.SelectedIndex == -1 || comboBoxPlugins.Items.Count == 0)
				return;	// no data source

			try
			{
				// tabula rasa
				Graph    = new WebGraph.Logic.Graph();
				Layouter = new WebGraph.Logic.GraphLayouter(Graph);
				graphCanvas.Children.Clear();

				// lets go	
				LoadSubGraph(textBoxSearch.Text, 0);
			}
			catch (Exception exc)
			{
				MessageBox.Show(exc.Message, exc.GetType().FullName);
			}
		}

		private void ButtonZoomInClick(object sender, RoutedEventArgs e)
		{
			Graph.ForAllNodes(node => node.repulsion = Math.Min(node.repulsion + 10, 200));
			Layouter.ResetDamper();
		}

		private void ButtonZoomOutClick(object sender, RoutedEventArgs e)
		{
			Graph.ForAllNodes(node => node.repulsion = Math.Max(node.repulsion - 10, 20));
			Layouter.ResetDamper();
		}
	}
}
