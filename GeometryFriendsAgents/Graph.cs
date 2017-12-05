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

        public Graph()
        {
            this.nodes = new Dictionary<int, Node>();
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

        public void generateNodes(RectangleRepresentation rI, CircleRepresentation cI, ObstacleRepresentation[] oI, ObstacleRepresentation[] rPI, ObstacleRepresentation[] cPI, CollectibleRepresentation[] colI)
        {
            int rectangleX = (int)rI.X;
            int rectangleY = (int)rI.Y;

            int circleX = (int)cI.X;
            int circleY = (int)cI.Y;

            Node rectangle = new Node(rectangleX, rectangleY, Node.Type.Rectangle);
            this.addNode(rectangle);

            Node circle = new Node(circleX, circleY, Node.Type.Circle);
            this.addNode(circle);

            // OBSTACLE
            for (int i = 0; i < oI.Length; i++)
            {
                ObstacleRepresentation obstacle = oI[i];

                Node obstacleNode = new Node((int)obstacle.X, (int)obstacle.Y, Node.Type.Obstacle);
                this.addNode(obstacleNode);
            }

            // CIRCLE PLATFORM
            for (int i = 0; i < cPI.Length; i++)
            {
                ObstacleRepresentation circlePlatform = cPI[i];

                Node circlePlatformNode = new Node((int)circlePlatform.X, (int)circlePlatform.Y, Node.Type.CirclePlatform);
                this.addNode(circlePlatformNode);
            }

            // RECTANGLE PLATFORM
            for (int i = 0; i < rPI.Length; i++)
            {
                ObstacleRepresentation rectanglePlatform = rPI[i];

                Node rectanglePlatformNode = new Node((int)rectanglePlatform.X, (int)rectanglePlatform.Y, Node.Type.RectanglePlatform);
                this.addNode(rectanglePlatformNode);
            }

            // DIAMONDS
            for (int i = 0; i < colI.Length; i++)
            {
                CollectibleRepresentation diamond = colI[i];

                Node diamondNode = new Node((int)diamond.X, (int)diamond.Y, Node.Type.Diamond);
                this.addNode(diamondNode);
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
                        continue;
                    }

                    if(matrix.openSpace(this.nodes[i].location, this.nodes[j].location))
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
        
    }
}