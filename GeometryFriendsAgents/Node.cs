using System;
using System.Drawing;

namespace GeometryFriendsAgents
{
    public class Node
    {
        private const int INFINITY = 2000 * 2000; // larger than any possibility on the matrix, since the matrix is ~1200*780
        private static int numberOfNodes = 0;
        /// <summary>
        /// The index of the node in the graph he was inserted into
        /// </summary>
        public int index;
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

        /// <summary>
        /// The node's location in the grid
        /// </summary>
        public Point location { get; set; }

        /// <summary>
        /// The type of pixel that the node represents.
        /// </summary>
        public Type type { get; set; }

        /// <summary>
        /// Cost (length of the path) from the start node to this node 
        /// </summary>
        public float gCost { get; set; }
        /// <summary>
        /// Cost (straight-line distance) from this node to the end node
        /// </summary>
        public float hCost { get; set; }
        /// <summary>
        /// An estimate of the total distance if taking the current route. It's calculated by summing gCost and hCost.
        /// </summary>
        public float fCost { get { return this.gCost + this.hCost; } }

        /// <summary>
        /// The parent of the node (the previous node in a path)
        /// The private field holds the information
        /// The public field allows you to call get and set without making stack exceptions
        /// </summary>
    
        public Node parentNode { get; set; }

        /// <summary>
        /// State of the node in the current search.
        /// </summary>

        /// <summary>
        /// Creates a new instance of Node.
        /// </summary>
        /// <param name="x">The node's location along the X axis</param>
        /// <param name="y">The node's location along the Y axis</param>
        /// <param name="isWalkable">True if the node can be traversed, false if the node is a 'wall' for the agent</param>
        /// <param name="endLocation">The location of the destination node</param>
        public Node(int x, int y, Type type)
        {
            this.index = numberOfNodes;

            this.location = new Point(x, y);
            this.type = type;
            this.gCost = INFINITY;  // start gCost at 'infinity'

            numberOfNodes++;
        }

        public override string ToString()
        {
            return string.Format("Node[X: {0}, Y: {1}, Type: {2}]", this.location.X, this.location.Y, this.type);
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

        public static void resetNumberOfNodes()
        {
            numberOfNodes = 0;
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Node n = obj as Node;
            if (n == null)
            {
                return false;
            }

            return Equals(n);

        }

        public bool Equals(Node n)
        {
            // If parameter is null return false:
            if (n == null)
            {
                return false;
            }

            // Return false if the fields don't match:
            if (this.location.X != n.location.X || this.location.Y != n.location.Y)
            {
                return false;
            }

            if(this.type != n.type)
            {
                return false;
            }

            return true;

        }

        public override int GetHashCode()
        {
            return location.X ^ location.Y;
        }
    }
}