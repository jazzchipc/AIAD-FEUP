using System;
using System.Drawing;

namespace GeometryFriendsAgents
{
    public class AStarNode : Node
    {

        public enum State
        {
            /// <summary>
            /// The node has not yet been considered in any possible paths
            /// </summary>
            Untested,
            /// <summary>
            /// The node has been identified as a possible step in a path
            /// </summary>
            Open,
            /// <summary>
            /// The node has already been included in a path and will not be considered again
            /// </summary>
            Close
        };

        /// <summary>
        /// Cost (length of the path) from the start node to this node 
        /// </summary>
        public float gCost { get; private set; }
        /// <summary>
        /// Cost (straight-lina distance) from this node to the end node
        /// </summary>
        public float hCost { get; private set; }
        /// <summary>
        /// An estimate of the total distance if taking the current route. It's calculated by summing gCost and hCost.
        /// </summary>
        public float fCost { get { return this.gCost + this.hCost; } }

        /// <summary>
        /// State of the node in the current search.
        /// </summary>
        public State nodeState { get; set; }

        public AStarNode(Type type, int x, int y) : base(type, x, y)
        {
        }

        public static float GetTraversalCost(Point location, Point otherLocation)
        {
            float deltaX = otherLocation.X - location.X;
            float deltaY = otherLocation.Y - location.Y;
            return (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}: {2}", this.x, this.y, this.nodeState);
        }

    }

    /// <summary>
    /// Defines the parameters which will be used to find a path across a section of the map
    /// </summary>
    public class SearchParameters
    {
        public Point StartLocation { get; set; }

        public Point EndLocation { get; set; }

        public bool[,] Map { get; set; }

        public SearchParameters(Point startLocation, Point endLocation, bool[,] map)
        {
            this.StartLocation = startLocation;
            this.EndLocation = endLocation;
            this.Map = map;
        }
    }
}
