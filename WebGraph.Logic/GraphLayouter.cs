using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebGraph.Logic
{
	/// <summary>Layouts a graph using a force directed algorithm</summary>
	public class GraphLayouter
	{
		#region layouting attributes

		private bool damping;			// When damping is true, the damper value decreases
		private double damper;			// A low damper value causes the graph to move slowly
		private double maxMotion;		// Keep an eye on the fastest moving node to see if the graph is stabilizing
		private double lastMaxMotion;
		private double motionRatio;		// It's sort of a ratio, equal to lastMaxMotion/maxMotion-1
		private double rigidity;		// Rigidity has the same effect as the damper, except that it's a constant
										// a low rigidity value causes things to go slowly.
										// a value that's too high will cause oscillation
		private double maxMotionA;

		#endregion

		private Graph graph;

		/// <summary>Initialization ctor </summary>
		/// <param name="g">Graph to layout</param>
		/// <param name="dRigidity">Initial layout speed</param>
		public GraphLayouter(Graph g, double dRigidity = 0.25)
		{
			graph = g;
			damping = true;
			rigidity = dRigidity;
		}

		/// <summary>Starts layouting operations</summary>
		public void StartDamper() 
		{ 
			damping = true; 
		}

		/// <summary>Stops current layouting operations</summary>
		public void StopDamper() 
		{ 
			damping = false; 
			damper = 1.0; 
		}

		/// <summary>Resets layouting operations</summary>
		public void ResetDamper() 
		{ 
			damping = true; 
			damper = 1.0; 
		}

		/// <summary>Stops all layouting operations</summary>
		public void StopMotion()
		{
			damping = true;
			if (damper > 0.3)		// stabilize the graph, but do so gently by setting the damper to a low value
				damper = 0.3;
			else
				damper = 0;
		}

		#region layouting calculations
		protected void Damp()
		{
			if (damping)
			{
				if (motionRatio <= 0.001)	  //This is important.  Only damp when the graph starts to move faster
				{  						  //When there is noise, you damp roughly half the time. (Which is a lot)
					//
					//If things are slowing down, then you can let them do so on their own,
					//without damping.

					//If max motion<0.2, damp away
					//If by the time the damper has ticked down to 0.9, maxMotion is still>1, damp away
					//We never want the damper to be negative though
					if ((maxMotion < 0.2 || (maxMotion > 1 && damper < 0.9)) && damper > 0.01) damper -= 0.01;
					//If we've slowed down significanly, damp more aggresively (then the line two below)
					else if (maxMotion < 0.4 && damper > 0.003) damper -= 0.003;
					//If max motion is pretty high, and we just started damping, then only damp slightly
					else if (damper > 0.0001) damper -= 0.0001;
				}
			}

			if (maxMotion < 0.001 && damping)
			{
				damper = 0;
			}
		}

		protected void MoveNodes()
		{
			lastMaxMotion = maxMotion;
			maxMotionA = 0;

			graph.ForAllNodes(new NodeCallback(ForAllNodes));

			maxMotion = maxMotionA;
			if (maxMotion > 0)
				motionRatio = lastMaxMotion / maxMotion - 1;	//subtract 1 to make a positive value mean that
			else											//things are moving faster
				motionRatio = 0;

			Damp();
		}

		public void Relax(int steps = 1)
		{
			if (!(damper < 0.1 && damping && maxMotion < maxMotionA))
			{
				for (int i = 0; i < steps; i++)
				{
					graph.ForAllEdges(new EdgeCallback(ForAllEdges));
					graph.ForAllNodePairs(new NodePairCallback(ForAllNodePairs));
					MoveNodes();
				}
			}

			//tgPanel.repaintAfterMove();
		}
		#endregion

		#region layouting callbacks
		private void ForAllNodes(Node node)
		{
			double dx = node.dx;
			double dy = node.dy;
			dx *= damper;  //The damper slows things down.  It cuts down jiggling at the last moment, and optimizes
			dy *= damper;  //layout.  As an experiment, get rid of the damper in these lines, and make a
			//long straight line of nodes.  It wiggles too much and doesn't straighten out.

			node.dx = dx / 2;   //Slow down, but don't stop.  Nodes in motion store momentum.  This helps when the force
			node.dy = dy / 2;   //on a node is very low, but you still want to get optimal layout.

			double distMoved = Math.Sqrt(dx * dx + dy * dy); //how far did the node actually move?

			if (!node.IsFixed)
			{
				node.x += Math.Max(-30, Math.Min(30, dx));	//don't move faster then 30 units at a time.
				node.y += Math.Max(-30, Math.Min(30, dy));	//I forget when this is important.  Stopping severed nodes from
				//flying away?
			}
			maxMotionA = Math.Max(distMoved, maxMotionA);
		}

		private void ForAllEdges(Edge edge)
		{
			double vx = edge.To.x - edge.From.x;
			double vy = edge.To.y - edge.From.y;
			double len = Math.Sqrt(vx * vx + vy * vy);

			double dx = vx * rigidity;  //rigidity makes edges tighter
			double dy = vy * rigidity;

			dx /= (edge.Length * 100);
			dy /= (edge.Length * 100);

			// Edges pull directly in proportion to the distance between the nodes. This is good,
			// because we want the edges to be stretchy.  The edges are ideal rubberbands.  They
			// They don't become springs when they are too short.  That only causes the graph to
			// oscillate.

			edge.To.dx -= dx * len;
			edge.To.dy -= dy * len;

			edge.From.dx += dx * len;
			edge.From.dy += dy * len;
		}

		private void ForAllNodePairs(Node n1, Node n2)
		{
			double dx = 0;
			double dy = 0;
			double vx = n1.x - n2.x;
			double vy = n1.y - n2.y;
			double len = vx * vx + vy * vy; //so it's length squared

			if (len == 0)
			{
				Random rand = new Random();
				dx = rand.NextDouble(); //If two nodes are right on top of each other, randomly separate
				dy = rand.NextDouble();
			}
			else if (len < 600 * 600)
			{					// 600, because we don't want deleted nodes to fly too far away
				dx = vx / len;  // If it was sqrt(len) then a single node surrounded by many others will
				dy = vy / len;  // always look like a circle.  This might look good at first, but I think
				// it makes large graphs look ugly + it contributes to oscillation.  A
				// linear function does not fall off fast enough, so you get rough edges
				// in the 'force field'
			}

			double repSum = n1.repulsion * n2.repulsion / 100;

			n1.dx += dx * repSum * rigidity;
			n1.dy += dy * repSum * rigidity;

			n2.dx -= dx * repSum * rigidity;
			n2.dy -= dy * repSum * rigidity; 
		}
		#endregion
	}

}
