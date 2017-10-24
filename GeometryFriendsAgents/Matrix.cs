using System.Collections.Generic;
using System;

namespace GeometryFriendsAgents
{
    public class Node
    {
        public enum Type { Space, Circle, Rectangle, Diamond, Obstacle,  CircleObstacle, RectangleObstacle};

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
            this.nodes = new Node[numOfRows, numOfCols];
        }

        public void addNode(Node node)
        {
            this.nodes[node.y, node.x] = node;
        }

        public Node getNode(int x, int y)
        {
            return this.nodes[y, x];
        }
    }
}