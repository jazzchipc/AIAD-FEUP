using System;
using System.Drawing;

namespace GeometryFriendsAgents
{
    public class Node
    {
        public enum Type
        {
            /// <summary>
            /// Represents an open space for the circle and rectangle
            /// </summary>
            Space,
            /// <summary>
            /// Represents a circle pixel
            /// </summary>
            Circle,
            /// <summary>
            /// Represents a rectangle pixel
            /// </summary>
            Rectangle,
            /// <summary>
            /// Represents a diamond (collectible) pixel
            /// </summary>
            Diamond,
            /// <summary>
            /// Represents an obstacle for the circle and rectangle
            /// </summary>
            Obstacle,
            /// <summary>
            /// Represents a platform which only the circle can go through
            /// </summary>
            CirclePlatform,
            /// <summary>
            /// Represents a platform which only the rectangle can go through
            /// </summary>
            RectanglePlatform
        };

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
            Closed
        };

        /// <summary>
        /// The node's location in the grid
        /// </summary>
        public Point location { get; private set; }

        /// <summary>
        /// The type of pixel that the node represents.
        /// </summary>
        public Type type { get; set; }
        /// <summary>
        /// The current state of the node during pathfinding.
        /// </summary>
        public State state { get; set; }

        /// <summary>
        /// Cost (length of the path) from the start node to this node 
        /// </summary>
        public float gCost { get; private set; }
        /// <summary>
        /// Cost (straight-line distance) from this node to the end node
        /// </summary>
        public float hCost { get; private set; }
        /// <summary>
        /// An estimate of the total distance if taking the current route. It's calculated by summing gCost and hCost.
        /// </summary>
        public float fCost { get { return this.gCost + this.hCost; } }

        /// <summary>
        /// The parent of the node (the previous node in a path)
        /// The private field holds the information
        /// The public field allows you to call get and set without making stack exceptions
        /// </summary>
        private Node ParentNode { get; set; }
        public Node parentNode
        {
            get { return this.ParentNode; }
            set
            {
                // When setting the parent, also calculate the traversal cost from the start node to here (the 'G' value)
                this.ParentNode = value;
                this.gCost = this.ParentNode.gCost + GetTraversalCost(this.location, this.ParentNode.location);
            }
        }

        /// <summary>
        /// State of the node in the current search.
        /// </summary>

        /// <summary>
        /// Creates a new instance of Node.
        /// </summary>
        /// <param name="x">The node's location along the X axis</param>
        /// <param name="y">The node's location along the Y axis</param>
        /// <param name="isWalkable">True if the node can be traversed, false if the node is a wall</param>
        /// <param name="endLocation">The location of the destination node</param>
        public Node(int x, int y, Type type, Point endLocation)
        {
            this.location = new Point(x, y);
            this.state = State.Untested;
            this.type = type;
            this.hCost = GetTraversalCost(this.location, endLocation);
            this.gCost = 0;
        }

        /// <summary>
        /// Gets the distance between two points using the Pythagoras theorem
        /// </summary>
        public static float GetTraversalCost(Point location, Point otherLocation)
        {
            float deltaX = otherLocation.X - location.X;
            float deltaY = otherLocation.Y - location.Y;
            return (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        public override string ToString()
        {
            return string.Format("Node[X: {0}, Y: {1}, State: {2}, Type: {3}]", this.location.X, this.location.Y, this.state, this.type);
        }

        public Boolean isWalkable(AgentType agentType)
        {
            if (agentType == AgentType.Circle)
            {
                switch (this.type)
                {
                    case Type.Obstacle:
                    case Type.RectanglePlatform:
                        return false;
                    default:
                        return true;
                }
            }
            else
            {
                switch (this.type)
                {
                    case Type.Obstacle:
                    case Type.CirclePlatform:
                        return false;
                    default:
                        return true;
                }
            }
        }
    }
}