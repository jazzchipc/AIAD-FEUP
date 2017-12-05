using GeometryFriends.AI.Debug;
using GeometryFriends.AI.Perceptions.Information;
using GeometryFriendsAgents;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GeometryFriendsAgents
{
    // Using http://blog.two-cats.com/2014/06/a-star-example/ tutorial


    /// <summary>
    /// Defines the parameters which will be used to find a path across a section of the map
    /// </summary>
    public class SearchParameters
    {
        public Node startNode { get; set; }

        public Node endNode { get; set; }

        public Graph graph;

        public SearchParameters(Node startNode, Node goalNode, Graph graph)
        {
            this.startNode = startNode;
            this.endNode = endNode;
            this.graph = graph;
        }
    }
    public class PathFinder
    {
        private Graph graph;
        private SearchParameters searchParameters;
        public AgentType agentType;

        List<Node> openSet;
        List<Node> closedSet;

        /// <summary>
        /// Create a new instance of PathFinder
        /// </summary>
        /// <param name="searchParameters"></param>
        public PathFinder(Graph graph, SearchParameters searchParameters, AgentType agentType)
        {
            this.graph = graph;
            this.searchParameters = searchParameters;
            this.agentType = agentType;
        }

        /// <summary>
        /// Attempts to find a path from the start location to the end location based on the supplied SearchParameters
        /// </summary>
        /// <returns>A List of Points representing the path. If no path was found, the returned list is empty.</returns>
        public List<Point> FindPath()
        {
            // The start node is the first entry in the 'open' list
            List<Point> path = new List<Point>();
            bool success = this.Search();
            if (success)
            {
                // If a path was found, follow the parents from the end node to build a list of locations
                Node node = this.searchParameters.endNode;
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
                System.Diagnostics.Debug.WriteLine("Open Set: " + openSet.Count + " nodes.");
                System.Diagnostics.Debug.WriteLine("Closed Set: " + closedSet.Count + " nodes.");

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

                List<Node> nextNodes = this.graph.getAdjacentNodes(current.index);

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
                    float tentativeGCost = nextNode.gCost + traversalCost;

                    if (tentativeGCost < nextNode.gCost)
                    {
                        // This is the best path, so save it
                        nextNode.parentNode = current;
                        nextNode.gCost = tentativeGCost;
                    }
                }
            }

            // The method returns false if this path leads to be a dead end
            return false;
        }

        /// <summary>
        /// Displays the map and path as a simple grid to the game with a yellow line
        /// </summary>
        public static void ShowPath(List<DebugInformation> agentDebugList, List<Point> path)
        {
            for(int i = 0; i < path.Count - 1; i++)
            {
                agentDebugList.Add(DebugInformationFactory.CreateLineDebugInfo(path[i], path[i+1], GeometryFriends.XNAStub.Color.Yellow));
            }
        }
    }

}