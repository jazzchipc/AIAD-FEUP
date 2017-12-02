using GeometryFriends.AI.Perceptions.Information;
using GeometryFriendsAgents;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace GeometryFriendsAgents
{
    // Using http://blog.two-cats.com/2014/06/a-star-example/ tutorial
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
            /// Represents a circle only obstacle
            /// </summary>
            CirclePlatform,
            /// <summary>
            /// Represents a rectangle only obstacle
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
        public Type type { get; private set; }
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
        /// </summary>
        public Node parentNode
        {
            get { return this.parentNode; }
            set
            {
                // When setting the parent, also calculate the traversal cost from the start node to here (the 'G' value)
                this.parentNode = value;
                this.gCost = this.parentNode.gCost + GetTraversalCost(this.location, this.parentNode.location);
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
    }

    public class Matrix
    {
        public Node[,] nodes;

        public int getHeight()
        {
            return this.nodes.Length;
        }

        public int getWidth()
        {
            return this.nodes.GetLength(0);
        }

        public Matrix(int numOfRows, int numOfCols)
        {
            this.nodes = new Node[numOfRows + 2, numOfCols + 2];
        }

        public void addNode(Node node)
        {
            this.nodes[node.location.Y, node.location.X] = node;
        }

        public Node getNode(int x, int y)
        {
            return this.nodes[y, x];
        }

        public static Matrix generateMatrixFomGameInfo(RectangleRepresentation rI, CircleRepresentation cI, ObstacleRepresentation[] oI, ObstacleRepresentation[] rPI, ObstacleRepresentation[] cPI, CollectibleRepresentation[] colI, Rectangle area)
        {
            Matrix matrix = new Matrix(area.Height, area.Width);

            int rectangleX = (int)rI.X;
            int rectangleY = (int)rI.Y;
            int rectangleWidth = (int)rI.Height; // TODO: calculate width from height

            for (int x = rectangleX - 10; x < rectangleX + 10; x++)
            {
                for (int y = rectangleY - 10; y < rectangleY + 10; y++)
                {
                    matrix.addNode(new Node(x, y, Node.Type.Rectangle, new Point((int)colI[0].X, (int)colI[0].Y)));
                }
            }

            return matrix;
        }

    }
}

/// <summary>
/// Defines the parameters which will be used to find a path across a section of the map
/// </summary>
public class SearchParameters
{
    public Point StartLocation { get; set; }

    public Point EndLocation { get; set; }

    public Matrix matrix;

    public SearchParameters(Point startLocation, Point endLocation, Matrix matrix)
    {
        this.StartLocation = startLocation;
        this.EndLocation = endLocation;
        this.matrix = matrix;
    }
}
public class PathFinder
{
    private Matrix matrix;
    private Node startNode;
    private Node endNode;
    private SearchParameters searchParameters;

    /// <summary>
    /// Create a new instance of PathFinder
    /// </summary>
    /// <param name="searchParameters"></param>
    public PathFinder(SearchParameters searchParameters)
    {
        this.searchParameters = searchParameters;
        InitializeNodes(searchParameters.matrix);
        this.startNode = this.matrix.nodes[searchParameters.StartLocation.X, searchParameters.StartLocation.Y];
        this.startNode.state = Node.State.Open;
        this.endNode = this.matrix.nodes[searchParameters.EndLocation.X, searchParameters.EndLocation.Y];
    }

    /// <summary>
    /// Attempts to find a path from the start location to the end location based on the supplied SearchParameters
    /// </summary>
    /// <returns>A List of Points representing the path. If no path was found, the returned list is empty.</returns>
    public List<Point> FindPath()
    {
        // The start node is the first entry in the 'open' list
        List<Point> path = new List<Point>();
        bool success = Search(startNode);
        if (success)
        {
            // If a path was found, follow the parents from the end node to build a list of locations
            Node node = this.endNode;
            while (node.parentNode != null)
            {
                path.Add(node.location);
                node = node.parentNode;
            }

            // Reverse the list so it's in the correct order when returned
            path.Reverse();
        }

        return path;
    }

    /// <summary>
    /// Builds the node grid from a simple grid of booleans indicating areas which are and aren't walkable
    /// </summary>
    /// <param name="map">A boolean representation of a grid in which true = walkable and false = not walkable</param>
    private void InitializeNodes(Matrix matrix)
    {
        this.matrix.nodes = new Node[this.matrix.getWidth(), this.matrix.getHeight()];
        for (int y = 0; y < this.matrix.getHeight(); y++)
        {
            for (int x = 0; x < this.matrix.getWidth(); x++)
            {
                this.matrix.nodes[x, y] = new Node(x, y, matrix.nodes[x, y].type, this.searchParameters.EndLocation);
            }
        }
    }

    /// <summary>
    /// Attempts to find a path to the destination node using <paramref name="currentNode"/> as the starting location
    /// </summary>
    /// <param name="currentNode">The node from which to find a path</param>
    /// <returns>True if a path to the destination has been found, otherwise false</returns>
    private bool Search(Node currentNode)
    {
        // Set the current node to Closed since it cannot be traversed more than once
        currentNode.state = Node.State.Closed;
        List<Node> nextNodes = GetAdjacentWalkableNodes(currentNode);

        // Sort by F-value so that the shortest possible routes are considered first
        nextNodes.Sort((node1, node2) => node1.fCost.CompareTo(node2.fCost));
        foreach (var nextNode in nextNodes)
        {
            // Check whether the end node has been reached
            if (nextNode.location == this.endNode.location)
            {
                return true;
            }
            else
            {
                // If not, check the next set of nodes
                if (Search(nextNode)) // Note: Recurses back into Search(Node)
                    return true;
            }
        }

        // The method returns false if this path leads to be a dead end
        return false;
    }

    /// <summary>
    /// Returns any nodes that are adjacent to <paramref name="fromNode"/> and may be considered to form the next step in the path
    /// </summary>
    /// <param name="fromNode">The node from which to return the next possible nodes in the path</param>
    /// <returns>A list of next possible nodes in the path</returns>
    private List<Node> GetAdjacentWalkableNodes(Node fromNode)
    {
        List<Node> walkableNodes = new List<Node>();
        IEnumerable<Point> nextLocations = GetAdjacentLocations(fromNode.location);

        foreach (var location in nextLocations)
        {
            int x = location.X;
            int y = location.Y;

            // Stay within the grid's boundaries
            if (x < 0 || x >= this.matrix.getWidth() || y < 0 || y >= this.matrix.getHeight())
                continue;

            Node node = this.matrix.nodes[x, y];
            // Ignore non-walkable nodes
            // TODO: SOLVE THIS
            /*if (!node.IsWalkable)
                continue;*/

            // Ignore already-closed nodes
            if (node.state == Node.State.Closed)
                continue;

            // Already-open nodes are only added to the list if their G-value is lower going via this route.
            if (node.state == Node.State.Open)
            {
                float traversalCost = Node.GetTraversalCost(node.location, node.parentNode.location);
                float gTemp = fromNode.gCost + traversalCost;
                if (gTemp < node.gCost)
                {
                    node.parentNode = fromNode;
                    walkableNodes.Add(node);
                }
            }
            else
            {
                // If it's untested, set the parent and flag it as 'Open' for consideration
                node.parentNode = fromNode;
                node.state = Node.State.Open;
                walkableNodes.Add(node);
            }
        }

        return walkableNodes;
    }

    /// <summary>
    /// Returns the eight locations immediately adjacent (orthogonally and diagonally) to <paramref name="fromLocation"/>
    /// </summary>
    /// <param name="fromLocation">The location from which to return all adjacent points</param>
    /// <returns>The locations as an IEnumerable of Points</returns>
    private static IEnumerable<Point> GetAdjacentLocations(Point fromLocation)
    {
        return new Point[]
        {
                new Point(fromLocation.X-1, fromLocation.Y-1),
                new Point(fromLocation.X-1, fromLocation.Y  ),
                new Point(fromLocation.X-1, fromLocation.Y+1),
                new Point(fromLocation.X,   fromLocation.Y+1),
                new Point(fromLocation.X+1, fromLocation.Y+1),
                new Point(fromLocation.X+1, fromLocation.Y  ),
                new Point(fromLocation.X+1, fromLocation.Y-1),
                new Point(fromLocation.X,   fromLocation.Y-1)
        };
    }
}
