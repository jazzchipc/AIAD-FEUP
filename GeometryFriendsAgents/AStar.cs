using GeometryFriends.AI.Debug;
using GeometryFriends.AI.Perceptions.Information;
using GeometryFriendsAgents;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GeometryFriendsAgents
{
    /// <summary>
    /// Defines the parameters which will be used to find a path across a section of the map
    /// </summary>
    public class SearchParameters
    {
        public Node startNode { get; set; }

        public Node endNode { get; set; }

        public Graph graph;

        public SearchParameters(int startNodeIndex, int endNodeIndex, Graph graph)
        {
            this.graph = graph;
            this.startNode = graph.getNode(startNodeIndex);
            this.endNode = graph.getNode(endNodeIndex);
        }
    }
    public class PathFinder
    {
        private SearchParameters searchParameters;
    
        public void setSearchParameteres(SearchParameters searchParameters)
        {
            this.searchParameters = searchParameters;
        }

        public AgentType agentType;

        List<Node> openSet;
        List<Node> closedSet;

        /// <summary>
        /// Create a new instance of PathFinder
        /// </summary>
        /// <param name="searchParameters"></param>
        public PathFinder(SearchParameters searchParameters, AgentType agentType)
        {
            this.searchParameters = searchParameters;
            this.agentType = agentType;
        }

        /// <summary>
        /// Attempts to find a path from the start location to the end location based on the supplied SearchParameters
        /// </summary>
        /// <returns>A List of Points representing the path. If no path was found, the returned list is empty.</returns>
        public List<Node> FindPath()
        {
            // The start node is the first entry in the 'open' list
            List<Node> path = new List<Node>();
            bool success = this.Search();
            if (success)
            {
                // If a path was found, follow the parents from the end node to build a list of locations
                Node node = this.searchParameters.endNode;

                while (node.parentNode != null)
                {
                    path.Add(node);
                    node = node.parentNode;
                }

                path.Add(node); // add starting node

                // Reverse the list so it's in the correct order when returned
                path.Reverse();

                return path;
            }

            return null;
        }

        /// <summary>
        /// Attempts to find a path to the destination node using startNode as the starting location
        /// </summary>
        /// <returns>True if a path to the destination has been found, otherwise false</returns>
        private bool Search()
        {
            this.openSet = new List<Node>();
            this.closedSet = new List<Node>();

            Node startNode = this.searchParameters.startNode;
            Node endNode = this.searchParameters.endNode;

            openSet.Add(startNode);

            // distance from start node to itself is 0 
            startNode.gCost = 0;  

            // heuristic is distance in straight line. It is admissible, because it never overestimates the real cost.
            startNode.hCost = Utils.GetTraversalCost(startNode.location, endNode.location); 
            

            while(openSet.Count > 0)
            {
                // order openSet by fCost
                openSet.Sort((node1, node2) => node1.fCost.CompareTo(node2.fCost));

                // node with lowest fCost in open set
                Node current = openSet[0]; 

                // Check whether the end node has been reached
                if (current.location == endNode.location)
                {
                    return true;
                }

                openSet.Remove(current);
                closedSet.Add(current);

                List<Node> nextNodes = this.searchParameters.graph.getAdjacentNodes(current.index);

                // Sort by F-value so that the shortest possible routes are considered first
                nextNodes.Sort((node1, node2) => node1.fCost.CompareTo(node2.fCost));
                foreach (var nextNode in nextNodes)
                {
                    // Ignore already-closed nodes
                    if (this.closedSet.Contains(nextNode))
                    {
                        continue;
                    }

                    // Check whether the end node has been reached
                    if (!this.openSet.Contains(nextNode))
                    {
                        openSet.Add(nextNode);
                    }

 
                    float traversalCost = Utils.GetTraversalCost(current.location, nextNode.location);
                    float tentativeGCost = current.gCost + traversalCost;

                    if (tentativeGCost < nextNode.gCost)
                    {
                        // This is the best path, so save it
                        nextNode.parentNode = current;
                        nextNode.gCost = tentativeGCost;
                        nextNode.hCost = Utils.GetTraversalCost(nextNode.location, endNode.location);
                        // fCost return gCost + hCost so no need to update it
                    }
                }
            }

            // The method returns false if this path leads to be a dead end
            return false;
        }
               
    }

}