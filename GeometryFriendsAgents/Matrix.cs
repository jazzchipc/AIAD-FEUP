using System.Collections.Generic;
using System;
using GeometryFriendsAgents;
using GeometryFriends.AI.Perceptions.Information;
using System.Drawing;

namespace GeometryFriendsAgents
{
    public class Node
    {
        public enum Type { Space, Circle, Rectangle, Diamond, Obstacle, CirclePlatform, RectanglePlatform};

        public Type type {  get; private set;  }
        public int x { get; private set; }
        public int y { get; private set; }

        public List<Edge> adj { get; private set; }

        public Node(Type type, int x, int y)
        {
            this.adj = new List<Edge>();
            this.type = type;
            this.x = x;
            this.y = y;
        }

        public void addEdge(Edge edge)
        {
            this.adj.Add(edge);
        }

        public void clearAllEdges()
        {
            this.adj.Clear();
        }

    }

    public class Edge
    {
        public Tuple<Node, Node> nodes { get; private set; }
        public float weight { get; private set; }
        public bool blocked { get; private set; } 

        public Edge(Node node1, Node node2, float weight)
        {
            this.nodes = new Tuple<Node, Node>(node1, node2);
            this.weight = weight;
        }
    }

    public class Matrix
    {
        public Node[,] nodes;

        public Matrix(int numOfRows, int numOfCols)
        {
            this.nodes = new Node[numOfRows + 2, numOfCols + 2];
        }

        public void addNode(Node node)
        {
            this.nodes[node.y, node.x] = node;
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

            for(int x = rectangleX - 10; x < rectangleX + 10; x++)
            {
                for(int y = rectangleY - 10; y < rectangleY + 10; y++)
                {
                    matrix.addNode(new Node(Node.Type.Rectangle, x, y));
                }
            }
            
            return matrix;
        }

    }

    

}