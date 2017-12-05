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
        
    }
}