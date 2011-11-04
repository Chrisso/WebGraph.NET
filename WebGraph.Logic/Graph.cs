using System;
using System.Collections.Generic;

namespace WebGraph.Logic
{
	/// <summary>Callback to be invoked on graph nodes</summary>
	/// <param name="node">Current node</param>
	public delegate void NodeCallback(Node node);

	/// <summary>Callback to be invoked on graph edges </summary>
	/// <param name="edge">Current edge</param>
	public delegate void EdgeCallback(Edge edge);

	/// <summary>Callback to be invoked on node pairs</summary>
	/// <param name="n1">First node of pair</param>
	/// <param name="n2">Second node of pair</param>
	public delegate void NodePairCallback(Node n1, Node n2);

	/// <summary>A webgraph consisting of nodes an edges</summary>
	public class Graph
	{
		private int _NodeRegistry;
		private List<Node> _Nodes;
		private List<Edge> _Edges;

		/// <summary>Get or set custom data</summary>
		public object Tag { get; set; }

		/// <summary>Default ctor</summary>
		public Graph()
		{
			_NodeRegistry = 0;
			_Nodes = new List<Node>();
			_Edges = new List<Edge>();
		}

		/// <summary>Creates a new node and adds it to the graph </summary>
		/// <param name="label">Label of the new node</param>
		/// <param name="magnet">Movement speed</param>
		/// <returns>the newly created node</returns>
		public Node AddNode(string label, double magnet = 60.0)
		{
			Node result = FindNode(label);
			if (result != null)	// already existing
				return result;

			result = new Node(++_NodeRegistry, label, magnet);
			_Nodes.Add(result);
			return result;
		}

		/// <summary>Creates a new egde and adds it to the graph</summary>
		/// <param name="from">Starting node</param>
		/// <param name="to">Ending node</param>
		/// <returns>the newly created edge</returns>
		public Edge AddEdge(Node from, Node to)
		{
			Edge result = new Edge(from, to);
			_Edges.Add(result);
			return result;
		}

		/// <summary>Searches for a node by Id</summary>
		/// <param name="id">Id to search for</param>
		/// <returns>node with id if found or null otherwise</returns>
		public Node FindNode(int id)
		{
			foreach (Node candidate in _Nodes)
				if (id == candidate.Id)
					return candidate;
			return null;
		}

		/// <summary>Searches for a node by Id</summary>
		/// <param name="label">Label to search for</param>
		/// <returns>node with label if found or null otherwise</returns>
		public Node FindNode(string label)
		{
			foreach (Node candidate in _Nodes)
				if (string.Compare(label, candidate.Label) == 0)
					return candidate;
			return null;
		}

		/// <summary>Invokes a callback on each node</summary>
		/// <param name="cb">Callbacke to be invoked</param>
		public void ForAllNodes(NodeCallback cb)
		{
			foreach (Node node in _Nodes)
				cb(node);
		}

		/// <summary>Invokes a callback on each edge</summary>
		/// <param name="cb">Callbacke to be invoked</param>
		public void ForAllEdges(EdgeCallback cb)
		{
			foreach (Edge edge in _Edges)
				cb(edge);
		}

		/// <summary>Invokes a callback on each node pair</summary>
		/// <param name="cb">Callbacke to be invoked</param>
		public void ForAllNodePairs(NodePairCallback cb)
		{
			for (int i = 0; i < _Nodes.Count; i++)
				for (int j = i + 1; j < _Nodes.Count; j++)
					cb(_Nodes[i], _Nodes[j]);
		}
	}
}
