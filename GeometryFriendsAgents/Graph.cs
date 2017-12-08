using GeometryFriends.AI.Debug;
using GeometryFriends.AI.Perceptions.Information;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace GeometryFriendsAgents
{
    public class Graph
    {
        private Dictionary<int, Node> nodes;  // associate a node with its index
        private bool[,] adjacencyMatrix;  // [i,j] tells you if node i is connected to node j
        private AgentType agentType;

        private Matrix matrix;

        // Nodes list
        public Node rectangleNode { get; private set; }
        public Node circleNode { get; private set; }
        public List<Node> diamondNodes { get; private set; }

        // Known paths
        public List<Path> knownPaths;

        public Graph(AgentType agentType, Matrix matrix)
        {
            this.nodes = new Dictionary<int, Node>();
            this.agentType = agentType;
            this.matrix = matrix;

            this.diamondNodes = new List<Node>();
            this.knownPaths = new List<Path>();

            Node.resetNumberOfNodes();
        }

        public void addNode(Node node)
        {
            if (this.nodes.ContainsKey(node.index)) {
                throw new Exception("Graph already has a node with that index.");
            }

            this.nodes.Add(node.index, node);
        }

        public Node getNode(int index)
        {
            return this.nodes[index];
        }

        public List<Node> getAdjacentNodes(int index)
        {
            List<Node> adjacentNodes = new List<Node>();

            for (int i = 0; i < this.nodes.Count; i++)
            {
                if (this.adjacencyMatrix[index, i])
                {
                    adjacentNodes.Add(this.nodes[i]);
                }
            }

            return adjacentNodes;
        }

        public void generateNodes(RectangleRepresentation rI, CircleRepresentation cI, ObstacleRepresentation[] oI, ObstacleRepresentation[] rPI, ObstacleRepresentation[] cPI, CollectibleRepresentation[] colI, AgentType agentType)
        {
            int rectangleX = (int)rI.X;
            int rectangleY = (int)rI.Y;

            int circleX = (int)cI.X;
            int circleY = (int)cI.Y;

            Node rectangle = new Node(rectangleX, rectangleY, Node.Type.Rectangle);
            this.addNode(rectangle);
            this.rectangleNode = rectangle;

            Node circle = new Node(circleX, circleY, Node.Type.Circle);
            this.addNode(circle);
            this.circleNode = circle;

            if(agentType == AgentType.Circle)   // add rectangle as if it was an obstacle
            {
                List<Point> corners = new List<Point>();

                // Make the 'corners' with just a little bit of offset, so that walkable line works fine
                Point upLeftCorner = new Point((int)(rI.X - Utils.getRectangleWidth(rI.Height) / 2) - 1, (int)(rI.Y - rI.Height / 2) - 1);
                Point upRightCorner = new Point((int)(rI.X + Utils.getRectangleWidth(rI.Height) / 2) + 1, (int)(rI.Y - rI.Height / 2) - 1);
                Point downLeftCorner = new Point((int)(rI.X - Utils.getRectangleWidth(rI.Height) / 2) - 1, (int)(rI.Y + rI.Height / 2) + 1);
                Point downRightCorner = new Point((int)(rI.X + Utils.getRectangleWidth(rI.Height) / 2) + 1, (int)(rI.Y + rI.Height / 2) + 1);

                corners.Add(upLeftCorner);
                corners.Add(upRightCorner);
                corners.Add(downLeftCorner);
                corners.Add(downRightCorner);

                for (int i = 0; i < corners.Count; i++)
                {
                    Point currentCorner = corners[i];

                    if (this.matrix.inBounds(currentCorner))
                    {
                        Node node = new Node(currentCorner.X, currentCorner.Y, Node.Type.Obstacle);
                        this.addNode(node);
                    }
                }
            }

            // OBSTACLE
            for (int i = 0; i < oI.Length; i++)
            {
                ObstacleRepresentation obstacle = oI[i];

                createNodesForObstacle(obstacle, Node.Type.Obstacle);
            }

            // CIRCLE PLATFORM
            for (int i = 0; i < cPI.Length; i++)
            {
                ObstacleRepresentation circlePlatform = cPI[i];

                createNodesForObstacle(circlePlatform, Node.Type.CirclePlatform);
            }

            // RECTANGLE PLATFORM
            for (int i = 0; i < rPI.Length; i++)
            {
                ObstacleRepresentation rectanglePlatform = rPI[i];

                createNodesForObstacle(rectanglePlatform, Node.Type.RectanglePlatform);
            }

            // DIAMONDS
            for (int i = 0; i < colI.Length; i++)
            {
                CollectibleRepresentation diamond = colI[i];

                Node diamondNode = new Node((int)diamond.X, (int)diamond.Y, Node.Type.Diamond);
                this.addNode(diamondNode);
                this.diamondNodes.Add(diamondNode);
            }
        }

        public void generateAdjacencyMatrix(Matrix matrix)
        {
            if(this.nodes == null)
            {
                return;
            }

            this.adjacencyMatrix = new bool[this.nodes.Count, this.nodes.Count];

            for(int i = 0; i < this.nodes.Count; i++)
            {
                for(int j = 0; j < this.nodes.Count; j++)
                {
                    if(i == j)
                    {
                        this.adjacencyMatrix[i, j] = false;
                    }
                    else if(matrix.walkableLine(this.nodes[i].location, this.nodes[j].location, this.agentType))
                    {
                        this.adjacencyMatrix[i, j] = true;
                    }
                    else
                    {
                        this.adjacencyMatrix[i, j] = false;
                    }
                }
            }
        }

        public void printAdjacency()
        {
            for (int i = 0; i < this.adjacencyMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < this.adjacencyMatrix.GetLength(1); j++)
                {
                    System.Diagnostics.Debug.Write(this.adjacencyMatrix[i, j] + "\t");
                }
                System.Diagnostics.Debug.WriteLine("");
            }
        }

        /// <summary>
        /// Highlights all the nodes in the map
        /// </summary>
        public static void ShowNodes(List<DebugInformation> agentDebugList, Graph graph)
        {
            int nodeDebugRadius = 20;
           
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                Node currentNode = graph.nodes[i];
                Point nodeDebugLocation = new Point(currentNode.location.X - (int)(nodeDebugRadius / 2), currentNode.location.Y - (int)(nodeDebugRadius / 2));

                GeometryFriends.XNAStub.Color color = GeometryFriends.XNAStub.Color.Red;
                
                agentDebugList.Add(DebugInformationFactory.CreateCircleDebugInfo(nodeDebugLocation, nodeDebugRadius, color));
            }
        }

        /// <summary>
        /// Creates nodes for the vertices of an obstacle. Remember it's an inverted Y axis.
        /// </summary>
        /// <param name="obstacle"></param>
        /// <param name="obstacleType"></param>
        /// <returns></returns>
        private void createNodesForObstacle(ObstacleRepresentation obstacle, Node.Type obstacleType)
        {
            List<Point> corners = new List<Point>();

            // Make the 'corners' with just a little bit of offset, so that walkable line works fine
            Point upLeftCorner = new Point((int)(obstacle.X - obstacle.Width / 2) - 1 , (int)(obstacle.Y - obstacle.Height / 2) - 1);
            Point upRightCorner = new Point((int)(obstacle.X + obstacle.Width / 2) + 1, (int)(obstacle.Y - obstacle.Height / 2) - 1);
            Point downLeftCorner = new Point((int)(obstacle.X - obstacle.Width / 2) - 1 , (int)(obstacle.Y + obstacle.Height / 2) + 1);
            Point downRightCorner = new Point((int)(obstacle.X + obstacle.Width / 2) + 1, (int)(obstacle.Y + obstacle.Height / 2) + 1);

            corners.Add(upLeftCorner);
            corners.Add(upRightCorner);
            corners.Add(downLeftCorner);
            corners.Add(downRightCorner);

            for(int i = 0; i < corners.Count; i++)
            {
                Point currentCorner = corners[i];

                if (this.matrix.inBounds(currentCorner))
                {
                    Node node = new Node(currentCorner.X, currentCorner.Y, obstacleType);
                    this.addNode(node);
                }
            }
        }

        public void showAllKnownPaths(List<DebugInformation> agentDebugList, AgentType agentType)
        {
            for(int i = 0; i < this.knownPaths.Count; i++)
            {
                showPath(agentDebugList, this.knownPaths[i].path, agentType);
            }
        }

        /// <summary>
        /// Displays the map and path as a simple grid to the game with a yellow line
        /// </summary>
        public static void showPath(List<DebugInformation> agentDebugList, List<Node> path, AgentType agentType)
        {
            GeometryFriends.XNAStub.Color color;

            if(agentType == AgentType.Circle)
            {
                color = GeometryFriends.XNAStub.Color.Yellow;
            }
            else
            {
                color = GeometryFriends.XNAStub.Color.Blue;
            }

            for (int i = 0; i < path.Count - 1; i++)
            {
                agentDebugList.Add(DebugInformationFactory.CreateLineDebugInfo(path[i].location, path[i + 1].location, color));
            }
        }
        public Path getCheapestPath()
        {
            Path cheapestPath = null;
            foreach (Path path in knownPaths)
            {
                if (cheapestPath == null || cheapestPath.totalCost > path.totalCost)
                    cheapestPath = path;
            }

            return cheapestPath;
        }

        public void removeFromKnownPaths(Node node)
        {
            for (int i = 0; i < knownPaths.Count; i++)
            {
                if (knownPaths[i].getGoalNode().Equals(node)) {
                    knownPaths.Remove(knownPaths[i]);
                }
            }
            System.Diagnostics.Debug.WriteLine(knownPaths);
        }

        public void updateRectanglePosition(RectangleRepresentation rI)
        {
            int rectangleX = (int)rI.X;
            int rectangleY = (int)rI.Y;

            this.rectangleNode.location = new Point(rectangleX, rectangleY);
        }

        public void updateCirclePosition(CircleRepresentation cI)
        {
            int circleX = (int)cI.X;
            int circleY = (int)cI.Y;

            this.circleNode.location = new Point(circleX, circleY);
        }

        public void updateGraph(RectangleRepresentation rI, CircleRepresentation cI)
        {
            this.updateRectanglePosition(rI);
            this.updateCirclePosition(cI);
            this.generateAdjacencyMatrix(this.matrix);
        }

    }

    public class Path
    {
        public float totalCost { private set; get; }
        public List<Node> path { private set; get; }

        public Path(List<Node> path, float totalCost)
        {
            this.path = path;
            this.totalCost = totalCost;
        }

        public Node getStartNode()
        {
            return path[0];
        }

        public Node getGoalNode()
        {
            return path[path.Count - 1];
        }
    }
}