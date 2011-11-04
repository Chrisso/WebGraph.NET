using System;

namespace WebGraph.Logic
{
	/// <summary>Node within a WebGraph</summary>
	public class Node
	{
		#region Properties
		
		/// <summary>Get or set the numerical identifier</summary>
		public int Id { get; set; }
		/// <summary>Get or set the label of this node</summary>
		public string Label { get; set; }

		/// <summary>Get or set custom data</summary>
		public Object Tag { get; set; }

		/// <summary>Get or set width</summary>
		public double Width { get; set; }
		/// <summary>Get or set the height</summary>
		public double Height { get; set; }

		/// <summary>Get or set the horzontal position</summary>
		public double x { get; set; }
		/// <summary>Get or set the vertical position</summary>
		public double y { get; set; }

		/// <summary>Get or set the horzontal position change</summary>
		public double dx { get; set; }
		/// <summary>Get or set the vertical position change</summary>
		public double dy { get; set; }

		/// <summary>Get or set the transformed horzontal position</summary>
		public double drawx { get; set; }
		/// <summary>Get or set the transformed vertical position</summary>
		public double drawy { get; set; }

		/// <summary>Get or set the movement speed</summary>
		public double repulsion { get; set; }

		/// <summary>Get or set whether this node can be moved</summary>
		public bool IsFixed { get; set; }

		#endregion

		/// <summary>Initialization ctor</summary>
		/// <param name="id">internal identifier of this node</param>
		/// <param name="label">Textual label</param>
		/// <param name="magnet">Movement speed within graph</param>
		public Node(int id, string label = "", double magnet = 100.0)
		{
			Id        = id;
			Label     = label;
			repulsion = magnet;
		}
	}
}
