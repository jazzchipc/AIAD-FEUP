using GeometryFriends;
using GeometryFriends.AI;
using GeometryFriends.AI.Communication;
using GeometryFriends.AI.Debug;
using GeometryFriends.AI.Interfaces;
using GeometryFriends.AI.Perceptions.Information;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace GeometryFriendsAgents
{
    /// <summary>
    /// A rectangle agent implementation for the GeometryFriends game that demonstrates simple random action selection.
    /// </summary>
    public class RectangleAgent : AbstractRectangleAgent
    {
        //agent implementation specificiation
        private bool implementedAgent;
        private string agentName = "AIADRectangle";

        //auxiliary variables for agent action
        private Moves currentAction;
        private List<Moves> possibleMoves;
        private long lastMoveTime;
        private Random rnd;

        //Sensors Information
        private CountInformation numbersInfo;
        private RectangleRepresentation rectangleInfo;
        private CircleRepresentation circleInfo;

        private ObstacleRepresentation[] obstaclesInfo;
        private ObstacleRepresentation[] rectanglePlatformsInfo;
        private ObstacleRepresentation[] circlePlatformsInfo;
        private CollectibleRepresentation[] collectiblesInfo;

        private int nCollectiblesLeft;

        private List<AgentMessage> messages;

        private List<Node> diamondsToCatch;

        //Area of the game screen
        protected Rectangle area;

        //Debug
        DebugInformation[] debugInfo = null;

        //Custom settings
        AgentType type = AgentType.Rectangle;

        Matrix matrix;
        Graph graph;

        // Diamond to get
        int nextDiamondIndex;
        Path nextDiamondPath;

        // Movement restrictions
        MovementRestrictions movementRestrictions;

        public RectangleAgent()
        {
            //Change flag if agent is not to be used
            implementedAgent = true;

            lastMoveTime = DateTime.Now.Second;
            currentAction = Moves.NO_ACTION;
            rnd = new Random();

            //prepare the possible moves  
            possibleMoves = new List<Moves>();
            possibleMoves.Add(Moves.MOVE_LEFT);
            possibleMoves.Add(Moves.MOVE_RIGHT);
            possibleMoves.Add(Moves.MORPH_UP);
            possibleMoves.Add(Moves.MORPH_DOWN);

            //messages exchange
            messages = new List<AgentMessage>();
            diamondsToCatch = new List<Node>();
        }


        // See Setup@CicleAgent.cs for parameter details
        public override void Setup(CountInformation nI, RectangleRepresentation rI, CircleRepresentation cI, ObstacleRepresentation[] oI, ObstacleRepresentation[] rPI, ObstacleRepresentation[] cPI, CollectibleRepresentation[] colI, Rectangle area, double timeLimit)
        {
            numbersInfo = nI;
            nCollectiblesLeft = nI.CollectiblesCount;
            rectangleInfo = rI;
            circleInfo = cI;
            obstaclesInfo = oI;
            rectanglePlatformsInfo = rPI;
            circlePlatformsInfo = cPI;
            collectiblesInfo = colI;
            this.area = area;

            //send a message to the rectangle informing that the circle setup is complete and show how to pass an attachment: a pen object
            messages.Add(new AgentMessage("Setup complete, testing to send an object as an attachment.", new Pen(Color.BlanchedAlmond)));

            this.runAStar(rI, cI, oI, rPI, cPI, colI, area);

            this.movementRestrictions = new MovementRestrictions(this.matrix);

            InitDiamondsToCatch();

            //DebugSensorsInfo();
        }

        // See SensorsUpdated@CicleAgent.cs for parameter details
        public override void SensorsUpdated(int nC, RectangleRepresentation rI, CircleRepresentation cI, CollectibleRepresentation[] colI)
        {
            nCollectiblesLeft = nC;

            rectangleInfo = rI;
            circleInfo = cI;
            collectiblesInfo = colI;

            if(!Utils.AIAD_DEMO_A_STAR_INITIAL_PATHS)
                this.updateAStar(rI, cI);
        }

        //implements abstract rectangle interface: signals if the agent is actually implemented or not
        public override bool ImplementedAgent()
        {
            return implementedAgent;
        }

        //implements abstract rectangle interface: provides the name of the agent to the agents manager in GeometryFriends
        public override string AgentName()
        {
            return agentName;
        }

        //simple algorithm for choosing a random action for the rectangle agent
        private void RandomAction()
        {
            /*
             Rectangle Actions
             MOVE_LEFT = 5
             MOVE_RIGHT = 6
             MORPH_UP = 7
             MORPH_DOWN = 8
            */

            //currentAction = possibleMoves[rnd.Next(possibleMoves.Count)];

            //send a message to the circle agent telling what action it chose
            messages.Add(new AgentMessage("Going to :" + currentAction));
        }

        //implements abstract rectangle interface: GeometryFriends agents manager gets the current action intended to be actuated in the enviroment for this agent
        public override Moves GetAction()
        {
            if (!Utils.AIAD_DEMO_A_STAR_INITIAL_PATHS)
                return currentAction;
            else
                return Moves.NO_ACTION;
        }

        //implements abstract rectangle interface: updates the agent state logic and predictions
        public override void Update(TimeSpan elapsedGameTime)
        {
            if (lastMoveTime == 60)
                lastMoveTime = 0;

            if ((lastMoveTime) <= (DateTime.Now.Second) && (lastMoveTime < 60))
            {
                if (!(DateTime.Now.Second == 59))
                {
                    RandomAction();
                    lastMoveTime = lastMoveTime + 1;
                    //DebugSensorsInfo();
                }
                else
                    lastMoveTime = 60;
            }

            //prepare all the debug information to be passed to the agents manager
            List<DebugInformation> newDebugInfo = new List<DebugInformation>();

            // see nodes considered by A*
            // No need to show duplicated nodes
            // Graph.ShowNodes(newDebugInfo, this.graph);
            // see path created by A*
            this.graph.showAllKnownPaths(newDebugInfo, this.type);

            // see current path
            if(this.nextDiamondPath != null)
                Graph.showPath(newDebugInfo, this.nextDiamondPath.path, this.type);

            //List<DebugInformation> debug = new List<DebugInformation>();
            //debug.AddRange(this.debugInfo);
            //debug.AddRange(newDebugInfo.ToArray());

            //PARA TESTAR
            if (this.diamondsToCatch.Count > this.nCollectiblesLeft)
            {
                Node node = this.graph.diamondNodes[nextDiamondIndex];
                catchNextDiamond(node);
            }

            this.debugInfo = newDebugInfo.ToArray();

        }

        //typically used console debugging used in previous implementations of GeometryFriends
        protected void DebugSensorsInfo()
        {
            Log.LogInformation("Rectangle Agent - " + numbersInfo.ToString());

            Log.LogInformation("Rectangle Agent - " + rectangleInfo.ToString());

            Log.LogInformation("Rectangle Agent - " + circleInfo.ToString());

            foreach (ObstacleRepresentation i in obstaclesInfo)
            {
                Log.LogInformation("Rectangle Aagent - " + i.ToString("Obstacle"));
            }

            foreach (ObstacleRepresentation i in rectanglePlatformsInfo)
            {
                Log.LogInformation("Rectangle Aagent - " + i.ToString("Rectangle Platform"));
            }

            foreach (ObstacleRepresentation i in circlePlatformsInfo)
            {
                Log.LogInformation("Rectangle Aagent - " + i.ToString("Circle Platform"));
            }

            foreach (CollectibleRepresentation i in collectiblesInfo)
            {
                Log.LogInformation("Rectangle Aagent - " + i.ToString());
            }
        }

        //implements abstract rectangle interface: signals the agent the end of the current level
        public override void EndGame(int collectiblesCaught, int timeElapsed)
        {
            Log.LogInformation("RECTANGLE - Collectibles caught = " + collectiblesCaught + ", Time elapsed - " + timeElapsed);
        }

        //implememts abstract agent interface: send messages to the circle agent
        public override List<GeometryFriends.AI.Communication.AgentMessage> GetAgentMessages()
        {
            List<AgentMessage> toSent = new List<AgentMessage>(messages);
            messages.Clear();
            return toSent;
        }

        //implememts abstract agent interface: receives messages from the circle agent
        public override void HandleAgentMessages(List<GeometryFriends.AI.Communication.AgentMessage> newMessages)
        {
            foreach (AgentMessage item in newMessages)
            {
                Log.LogInformation("Rectangle: received message from circle: " + item.Message);
                if (item.Attachment != null)
                {
                    Log.LogInformation("Received message has attachment: " + item.Attachment.ToString());
                    if (item.Attachment.GetType() == typeof(Pen))
                    {
                        Log.LogInformation("The attachment is a pen, let's see its color: " + ((Pen)item.Attachment).Color.ToString());
                    }

                    if (item.Attachment.GetType() == typeof(Request))
                    {
                        RequestHandler((Request)item.Attachment);
                    }
                }

            }
        }

        //implements abstract circle interface: gets the debug information that is to be visually represented by the agents manager
        public override DebugInformation[] GetDebugInformation()
        {
            return debugInfo;
        }



        /// <summary>
        /// Generates matrix describing the level and runs A star to find paths to every diamond
        /// </summary>
        private void runAStar(RectangleRepresentation rI, CircleRepresentation cI, ObstacleRepresentation[] oI, ObstacleRepresentation[] rPI, ObstacleRepresentation[] cPI, CollectibleRepresentation[] colI, Rectangle area)
        {
            // create game matrix
            this.matrix = Matrix.generateMatrixFomGameInfo(rI, cI, oI, rPI, cPI, colI, area);

            // create node graph
            this.graph = new Graph(this.type, this.matrix);
            this.graph.generateNodes(rI, cI, oI, rPI, cPI, colI, this.type);
            this.graph.generateAdjacencyMatrix(this.matrix);

            for (int i = 0; i < this.graph.diamondNodes.Count; i++)    // find shortest path to every node
            {
                SearchParameters searchParameters = new SearchParameters(this.graph.rectangleNode.index, this.graph.diamondNodes[i].index, this.graph);
                PathFinder pathFinder = new PathFinder(searchParameters, this.type);
                Path knownPath = pathFinder.FindPath();
                if (knownPath != null)
                {
                    System.Diagnostics.Debug.WriteLine("A* found a path between: [" + searchParameters.startNode + " and " + searchParameters.endNode + ".");
                    this.graph.knownPaths.Add(knownPath);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("A* did NOT find a path between: [" + searchParameters.startNode + " and " + searchParameters.endNode + ".");
                }
            }

        }

        private void updateAStar(RectangleRepresentation rI, CircleRepresentation cI)
        {
            this.matrix.updateMatrix(rI, cI);
            this.graph.updateGraph(rI, cI);

            SearchParameters searchParameters = new SearchParameters(this.graph.rectangleNode.index, this.graph.diamondNodes[nextDiamondIndex].index, this.graph);
            PathFinder pathFinder = new PathFinder(searchParameters, this.type);
            this.nextDiamondPath = pathFinder.FindPath();
        }

        /// <summary>
        /// Main function for request handling.
        /// </summary>
        /// <param name="request">Request object received inside an AgentMessage.</param>
        private void RequestHandler(Request request)
        {
            bool validRequest = ValidRequest(request);
            Answer.Type answerType;
            Object attachment = null;

            if (validRequest)
            {
                attachment = request.command.execute(this);
                answerType = Answer.Type.YES;
            }
            else
            {
                answerType = Answer.Type.NO;
            }

            Answer answer = new Answer(answerType, request.id, attachment);
            this.messages.Add(answer.message);
        }

        private bool ValidRequest(Request request)
        {
            if (true) //TODO WTF?
            {
                return true;
            }

            return false;
        }

        /**
         * Functions below describe what effects a command has.
         */
        public void MoveLeft()
        {
            this.currentAction = Moves.MOVE_LEFT;
        }

        public void MoveRight()
        {
            this.currentAction = Moves.MOVE_RIGHT;
        }

        internal void NoAction()
        {
            this.currentAction = Moves.NO_ACTION;
        }

        internal void MorphDown()
        {
            this.currentAction = Moves.MORPH_DOWN;
        }

        internal void MorphUp()
        {
            this.currentAction = Moves.MORPH_UP;
        }

        public Path getCheapestPath()
        {
            return this.graph.getCheapestPath();
        }

        public void catchDiamond(Node node)
        {
            System.Diagnostics.Debug.WriteLine("Retangulo - Vou apanhar o diamante: " + node.location);
            int index = this.graph.diamondNodes.IndexOf(node);
            nextDiamondIndex = index;
        }

        public void catchNextDiamond(Node node)
        {
            System.Diagnostics.Debug.WriteLine("Retangulo - Vou apagar o diamante: " + node.location);
            diamondsToCatch.Remove(node);

            this.graph.removeFromKnownPaths(node);
            
            Path path = this.graph.getCheapestPath();
            if (path != null)
                catchDiamond(path.getGoalNode());
        }

        public void InitDiamondsToCatch()
        {
            List<Path> paths = this.graph.knownPaths;
            Node node = null;
            foreach (Path path in paths)
            {
                node = path.getGoalNode();
                if (node.type == Node.Type.Diamond)
                {
                    if (Utils.AIAD_DEMO_A_STAR_INITIAL_PATHS)
                    {
                        diamondsToCatch.Add(node);
                    }
                    else if(this.movementRestrictions.canRectangleGet(this.graph.rectangleNode, node))
                    {
                        diamondsToCatch.Add(node);
                    }
                }
            }
        }
    }

}