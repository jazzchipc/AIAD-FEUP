using System.Collections.Generic;
using System;
using GeometryFriendsAgents;
using GeometryFriends.AI.Perceptions.Information;
using System.Drawing;

namespace GeometryFriendsAgents
{
    public class ExNode
    {
        public enum Type { Space, Circle, Rectangle, Diamond, Obstacle, CirclePlatform, RectanglePlatform};

        public Type type {  get; protected set;  }
        public int x { get; protected set; }
        public int y { get; protected set; }

        public List<Edge> adj { get; protected set; }

        public ExNode(Type type, int x, int y)
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
        public Tuple<ExNode, ExNode> nodes { get; private set; }
        public float weight { get; private set; }
        public bool blocked { get; private set; } 

        public Edge(ExNode node1, ExNode node2, float weight)
        {
            this.nodes = new Tuple<ExNode, ExNode>(node1, node2);
            this.weight = weight;
        }
    }

    public class ExMatrix
    {
        public ExNode[,] nodes;

        public ExMatrix(int numOfRows, int numOfCols)
        {
            this.nodes = new ExNode[numOfRows + 2, numOfCols + 2];
        }
            
        public void addNode(ExNode node)
        {
            this.nodes[node.y, node.x] = node;
        }

        public ExNode getNode(int x, int y)
        {
            return this.nodes[y, x];
        }

        public static ExMatrix generateMatrixFomGameInfo(RectangleRepresentation rI, CircleRepresentation cI, ObstacleRepresentation[] oI, ObstacleRepresentation[] rPI, ObstacleRepresentation[] cPI, CollectibleRepresentation[] colI, Rectangle area)
        {
            ExMatrix matrix = new ExMatrix(area.Height, area.Width);

            int rectangleX = (int)rI.X;
            int rectangleY = (int)rI.Y;
            int rectangleWidth = (int)rI.Height; // TODO: calculate width from height

            for(int x = rectangleX - 10; x < rectangleX + 10; x++)
            {
                for(int y = rectangleY - 10; y < rectangleY + 10; y++)
                {
                    matrix.addNode(new ExNode(ExNode.Type.Rectangle, x, y));
                }
            }
            
            return matrix;
        }

    }

    

}