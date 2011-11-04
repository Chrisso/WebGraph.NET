using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebGraph.Logic
{
	/// <summary>Edge within a webgraph</summary>
	public class Edge
	{
		#region Properties

		/// <summary>Node where this edge begins</summary>
		public Node From { get; set; }

		/// <summary>Node where this edge ends</summary>
		public Node To { get; set; }

		/// <summary>Virtual length of this edge</summary>
		public double Length { get; set; }

		/// <summary>Get or set custom data</summary>
		public Object Tag { get; set; }

		#endregion

		/// <summary>Initialization ctor</summary>
		/// <param name="from">Node where this edge begins</param>
		/// <param name="to">Node where this edge ends</param>
		/// <param name="len">Virtual length of this edge</param>
		public Edge(Node from, Node to, double len = 40.0)
		{
			From   = from;
			To     = to;
			Length = len;
		}
	}
}
