﻿using GeometryFriends;
using GeometryFriends.AI;
using GeometryFriends.AI.ActionSimulation;
using GeometryFriends.AI.Communication;
using GeometryFriends.AI.Debug;
using GeometryFriends.AI.Interfaces;
using GeometryFriends.AI.Perceptions.Information;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace GeometryFriendsAgents
{
    /// <summary>
    /// A circle agent implementation for the GeometryFriends game that demonstrates prediction and history keeping capabilities.
    /// </summary>
    public class CircleAgent : AbstractCircleAgent
    {
        //agent implementation specificiation
        private bool implementedAgent;
        private string agentName = "AIADCircle";

        //auxiliary variables for agent action
        private Moves currentAction = Moves.NO_ACTION;
        private List<Moves> possibleMoves;
        private long lastMoveTime;
        private Random rnd;

        //predictor of actions for the circle
        private ActionSimulator predictor = null;
        private DebugInformation[] debugInfo = null;
        private int debugCircleSize = 20;

        //debug agent predictions and history keeping
        private List<CollectibleRepresentation> caughtCollectibles;
        private List<CollectibleRepresentation> uncaughtCollectibles;
        private object remainingInfoLock = new Object();
        private List<CollectibleRepresentation> remaining;

        //Sensors Information and level state
        private CountInformation numbersInfo;
        private RectangleRepresentation rectangleInfo;
        private CircleRepresentation circleInfo;
        private ObstacleRepresentation[] obstaclesInfo;
        private ObstacleRepresentation[] rectanglePlatformsInfo;
        private ObstacleRepresentation[] circlePlatformsInfo;
        private CollectibleRepresentation[] collectiblesInfo;

        private int nCollectiblesLeft;

        private List<AgentMessage> messages;

        //Area of the game screen
        private Rectangle area;

        //Custom settings
        int iterationCount = 0;
        int moveStep = 4;

        Matrix matrix;
        Graph graph;

        //Communication settings
        Queue<Request> requests;

        //Status tracking
        Status agentStatus;

        public CircleAgent()
        {
            //Change flag if agent is not to be used
            implementedAgent = true;

            //setup for action updates
            lastMoveTime = DateTime.Now.Second;
            currentAction = Moves.NO_ACTION;
            rnd = new Random();

            //prepare the possible moves  
            possibleMoves = new List<Moves>();
            possibleMoves.Add(Moves.ROLL_LEFT);
            possibleMoves.Add(Moves.ROLL_RIGHT);
            possibleMoves.Add(Moves.JUMP);
            possibleMoves.Add(Moves.GROW);          
      
            //history keeping
            uncaughtCollectibles = new List<CollectibleRepresentation>();
            caughtCollectibles = new List<CollectibleRepresentation>();
            remaining = new List<CollectibleRepresentation>();

            //messages exchange
            messages = new List<AgentMessage>();

            //custom
            requests = new Queue<Request>();

            agentStatus = new Status();
        }

        /// <summary>
        /// implements abstract circle interface: used to setup the initial information so that the agent has basic knowledge about the level
        /// </summary>
        /// <param name="nI">This structure contains the number of obstacles, the number of character specific platforms (Circle & Rectangle Platforms) and the total number of purple diamonds within the level.</param>
        /// <param name="rI">This structure contains the current information on the rectangle agent, such as position (X and Y), velocity (X and Y) and its current height.</param>
        /// <param name="cI">This array contains the current information on the circle agent, such as position (X and Y) and velocity (X and Y).</param>
        /// <param name="oI">This array contains all the information about the obstacles (default obstacles, not character specific obstacles) in the level, such as the center coordinates of the platform (X and Y) and the platform’s height and width.</param>
        /// <param name="rPI">This array contains all the information about Rectangle specific platforms in the level, such as the center coordinates of the platform (X and Y) and the platform’s height and width.</param>
        /// <param name="cPI">This array contains all the information about Circle specific platforms in the level, such as the center coordinates of the platform (X and Y) and the platform’s height and width.</param>
        /// <param name="colI">This array contains the information about the coordinates (center X and Y positions) of all the collectibles (purple diamonds) in the level.</param>
        /// <param name="area">Specifies the definition of the rectangle area in which the game unfolds.</param>
        /// <param name="timeLimit">Specifies the amount of time the agent has to solve the level during the competition.</param>
        public override void Setup(CountInformation nI, RectangleRepresentation rI, CircleRepresentation cI, ObstacleRepresentation[] oI, ObstacleRepresentation[] rPI, ObstacleRepresentation[] cPI, CollectibleRepresentation[] colI, Rectangle area, double timeLimit)
        {
            this.numbersInfo = nI;
            this.nCollectiblesLeft = nI.CollectiblesCount;
            this.rectangleInfo = rI;
            this.circleInfo = cI;
            this.obstaclesInfo = oI;
            this.rectanglePlatformsInfo = rPI;
            this.circlePlatformsInfo = cPI;
            this.collectiblesInfo = colI;
            this.uncaughtCollectibles = new List<CollectibleRepresentation>(collectiblesInfo);
            this.area = area;

            //send a message to the rectangle informing that the circle setup is complete and show how to pass an attachment: a pen object
            this.messages.Add(new AgentMessage("Setup complete, testing to send an object as an attachment.", new Pen(Color.AliceBlue)));

            // create game matrix
            this.matrix = Matrix.generateMatrixFomGameInfo(rI, cI, oI, rPI, cPI, colI, area);

            // create node graph
            this.graph = new Graph(AgentType.Circle, this.matrix);
            this.graph.generateNodes(rI, cI, oI, rPI, cPI, colI);
            this.graph.generateAdjacencyMatrix(this.matrix);

            // TODO: make adjacency depend on the type of agent and use 'isWalkable()'
            this.graph.printAdjacency();


            for(int i = 0; i < this.graph.diamondNodes.Count; i++)    // find shortest path to every node
            {
                SearchParameters searchParameters = new SearchParameters(this.graph.circleNode.index, this.graph.diamondNodes[i].index, this.graph);
                PathFinder pathFinder = new PathFinder(searchParameters, AgentType.Circle);
                List<Node> knownPath = pathFinder.FindPath();
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

            DebugSensorsInfo();
        }

        /// <summary>
        /// implements abstract circle interface: registers updates from the agent's sensors that it is up to date with the latest environment information
        /// ***WARNING***: this method is called independently from the agent update - Update(TimeSpan elapsedGameTime) - so care should be taken when using complex 
        /// structures that are modified in both(e.g.see operation on the "remaining" collection)
        /// </summary>
        /// <param name="nC">The current number of collectibles within the level.</param>
        /// <param name="rI">This structure contains the current information on the rectangle agent, such as position (X and Y), velocity (X and Y) and its current height.</param>
        /// <param name="cI">This array contains the current information on the circle agent, such as position (X and Y) and velocity (X and Y).</param>
        /// <param name="colI">This array contains the information about the coordinates (center X and Y positions) of all the collectibles (purple diamonds) in the level.</param>
        public override void SensorsUpdated(int nC, RectangleRepresentation rI, CircleRepresentation cI, CollectibleRepresentation[] colI)
        {
            this.nCollectiblesLeft = nC;

            this.rectangleInfo = rI;
            this.circleInfo = cI;
            this.collectiblesInfo = colI;

            /* The lock keyword ensures that one thread does not enter a critical section of code while another thread is in the critical section. 
            If another thread tries to enter a locked code, it will wait, block, until the object is released. */
            lock (remaining)
            {
                this.remaining = new List<CollectibleRepresentation>(collectiblesInfo);
            }

            //DebugSensorsInfo();
        }

        //implements abstract circle interface: provides the circle agent with a simulator to make predictions about the future level state
        public override void ActionSimulatorUpdated(ActionSimulator updatedSimulator)
        {
            this.predictor = updatedSimulator;
        }

        //implements abstract circle interface: signals if the agent is actually implemented or not
        public override bool ImplementedAgent()
        {
            return this.implementedAgent;
        }

        //implements abstract circle interface: provides the name of the agent to the agents manager in GeometryFriends
        public override string AgentName()
        {
            return this.agentName;
        }

        //simple algorithm for choosing a random action for the circle agent
        private void DecideAction()
        {
            /*
             Circle Actions
             ROLL_LEFT = 1      
             ROLL_RIGHT = 2
             JUMP = 3
             GROW = 4
            */

            //currentAction = possibleMoves[rnd.Next(possibleMoves.Count)];

            //Update Status
            CircleRepresentation[] circles = new CircleRepresentation[] { circleInfo, new CircleRepresentation() };
            RectangleRepresentation[] rectangles = new RectangleRepresentation[] { rectangleInfo, new RectangleRepresentation() };
            this.agentStatus.Update(circles, rectangles, this.collectiblesInfo[0], AgentType.Circle);

            if (this.collectiblesInfo.Length <= 0)
            {
                currentAction = Moves.NO_ACTION;    // collected all
            }
            else
            {
                //currentAction = this.CircleJumpOntoRectangle(this.collectiblesInfo[0]);
                currentAction = this.JumpOntoRectangle();
            }

            //send a message to the rectangle agent telling what action it chose
            messages.Add(new AgentMessage("Going to :" + currentAction));
            Log.LogInformation(this.circleInfo.ToString());
            //Log.LogInformation(this.obstaclesInfo[0].ToString());
        }

        //implements abstract circle interface: GeometryFriends agents manager gets the current action intended to be actuated in the enviroment for this agent
        public override Moves GetAction()
        {
            return currentAction;
        }

        //implements abstract circle interface: updates the agent state logic and predictions
        public override void Update(TimeSpan elapsedGameTime)
        {
            // Execute an action every 'moveStep' cycles
            if(iterationCount == moveStep)
            {
                DecideAction();
                iterationCount = 0;
            }

            iterationCount++;

            //check if any collectible was caught
            lock (remaining)
            {
                if (remaining.Count > 0)
                {
                    List<CollectibleRepresentation> toRemove = new List<CollectibleRepresentation>();
                    foreach (CollectibleRepresentation item in uncaughtCollectibles)
                    {
                        if (!remaining.Contains(item))
                        {
                            caughtCollectibles.Add(item);
                            toRemove.Add(item);
                        }
                    }
                    foreach (CollectibleRepresentation item in toRemove)
                    {
                        uncaughtCollectibles.Remove(item);
                    }
                }
            }

            //predict what will happen to the agent given the current state and current action
            if (predictor != null) //predictions are only possible where the agents manager provided
            {
                /*
                 * 1) simulator can only be properly used when the Circle and Rectangle characters are ready, this must be ensured for smooth simulation
                 * 2) in this implementation we only wish to simulate a future state when whe have a fresh simulator instance, i.e. the generated debug information is empty
                */
                if (predictor.CharactersReady() && predictor.SimulationHistoryDebugInformation.Count == 0)
                {
                    List<CollectibleRepresentation> simCaughtCollectibles = new List<CollectibleRepresentation>();
                    //keep a local reference to the simulator so that it can be updated even whilst we are performing simulations
                    ActionSimulator toSim = predictor;

                    //prepare the desired debug information (to observe this information during the game press F1)
                    toSim.DebugInfo = true;
                    //you can also select the type of debug information generated by the simulator to circle only, rectangle only or both as it is set by default
                    //toSim.DebugInfoSelected = ActionSimulator.DebugInfoMode.Circle;

                    //setup the current circle action in the simulator
                    toSim.AddInstruction(currentAction);

                    //register collectibles that are caught during simulation
                    toSim.SimulatorCollectedEvent += delegate(Object o, CollectibleRepresentation col) { simCaughtCollectibles.Add(col); };
                    
                    //simulate 2 seconds (predict what will happen 2 seconds ahead)
                    toSim.Update(2);

                    //prepare all the debug information to be passed to the agents manager
                    List<DebugInformation> newDebugInfo = new List<DebugInformation>();
                    //clear any previously passed debug information (information passed to the manager is cumulative unless cleared in this way)
                    newDebugInfo.Add(DebugInformationFactory.CreateClearDebugInfo());
                    //add all the simulator generated debug information about circle/rectangle predicted paths
                    newDebugInfo.AddRange(toSim.SimulationHistoryDebugInformation);

                    // see nodes considered by A*
                    Graph.ShowNodes(newDebugInfo, this.graph);
                    // see path created by A*
                    this.graph.showAllKnownPaths(newDebugInfo);
                    

                    //create additional debug information to visualize collectibles that have been predicted to be caught by the simulator
                    foreach (CollectibleRepresentation item in simCaughtCollectibles)
                    {
                        newDebugInfo.Add(DebugInformationFactory.CreateCircleDebugInfo(new PointF(item.X - debugCircleSize / 2, item.Y - debugCircleSize / 2), debugCircleSize, GeometryFriends.XNAStub.Color.Red));
                        newDebugInfo.Add(DebugInformationFactory.CreateTextDebugInfo(new PointF(item.X, item.Y), "Predicted catch!", GeometryFriends.XNAStub.Color.White));
                    }
                    //create additional debug information to visualize collectibles that have already been caught by the agent
                    foreach (CollectibleRepresentation item in caughtCollectibles)
                    {
                        newDebugInfo.Add(DebugInformationFactory.CreateCircleDebugInfo(new PointF(item.X - debugCircleSize / 2, item.Y - debugCircleSize / 2), debugCircleSize, GeometryFriends.XNAStub.Color.GreenYellow));
                    }                 
                    //set all the debug information to be read by the agents manager
                    debugInfo = newDebugInfo.ToArray();                    
                }
            }
        }

        //typically used console debugging used in previous implementations of GeometryFriends
        protected void DebugSensorsInfo()
        {
            Log.LogInformation("Circle Agent - " + numbersInfo.ToString());

            Log.LogInformation("Circle Agent - " + rectangleInfo.ToString());

            Log.LogInformation("Circle Agent - " + circleInfo.ToString());

            foreach (ObstacleRepresentation i in obstaclesInfo)
            {
                Log.LogInformation("Circle Agent - " + i.ToString("Obstacle"));
            }

            foreach (ObstacleRepresentation i in rectanglePlatformsInfo)
            {
                Log.LogInformation("Circle Agent - " + i.ToString("Rectangle Platform"));
            }

            foreach (ObstacleRepresentation i in circlePlatformsInfo)
            {
                Log.LogInformation("Circle Agent - " + i.ToString("Circle Platform"));
            }

            foreach (CollectibleRepresentation i in collectiblesInfo)
            {
                Log.LogInformation("Circle Agent - " + i.ToString());
            }
        }

        //implements abstract circle interface: signals the agent the end of the current level
        public override void EndGame(int collectiblesCaught, int timeElapsed)
        {
            Log.LogInformation("CIRCLE - Collectibles caught = " + collectiblesCaught + ", Time elapsed - " + timeElapsed);
        }

        //implements abstract circle interface: gets the debug information that is to be visually represented by the agents manager
        public override DebugInformation[] GetDebugInformation()
        {
            return debugInfo;
        }

        //implememts abstract agent interface: send messages to the rectangle agent
        public override List<GeometryFriends.AI.Communication.AgentMessage> GetAgentMessages()
        {
            List<AgentMessage> toSent = new List<AgentMessage>(messages);
            messages.Clear();
            return toSent;
        }

        //implememts abstract agent interface: receives messages from the rectangle agent
        public override void HandleAgentMessages(List<GeometryFriends.AI.Communication.AgentMessage> newMessages)
        {
            foreach (AgentMessage item in newMessages)
            {
                Log.LogInformation("Circle: received message from rectangle: " + item.Message);
                if (item.Attachment != null)
                {
                    Log.LogInformation("Received message has attachment: " + item.Attachment.ToString());
                    if (item.Attachment.GetType() == typeof(Pen))
                    {
                        Log.LogInformation("The attachment is a pen, let's see its color: " + ((Pen)item.Attachment).Color.ToString());
                    }

                    if (item.Attachment.GetType() == typeof(Answer))
                    {
                        AnswerHandler((Answer)item.Attachment);
                    }
                }
            }
        }

        /**
         * PHYSICAL MOVES
         * */

        float positionMargin = 10;

        private Moves CircleJumpOntoRectangle(CollectibleRepresentation diamondToCatch)
        {
            Moves move = Moves.NO_ACTION;

            float circleX = this.circleInfo.X;
            float circleY = this.circleInfo.Y;

            float circleVelX = this.circleInfo.VelocityX;
            float circleVelY = this.circleInfo.VelocityY;

            float diamondX = diamondToCatch.X;
            float diamondY = diamondToCatch.Y;

            float positionToDiamondX = circleX - diamondX;

            float positionMargin = 10;

            if(this.predictor != null)
            {
                ActionSimulator sim = predictor;
                sim.Update(1);

                //Representation of the future status
                Status futureStatus = new Status();
                CircleRepresentation futureCircle = new CircleRepresentation(sim.CirclePositionX, sim.CirclePositionY,
                    sim.CircleVelocityX, sim.CircleVelocityY, sim.CircleVelocityRadius);
                RectangleRepresentation futureRectangle = new RectangleRepresentation(sim.RectanglePositionX, sim.RectanglePositionY,
                    sim.RectangleVelocityX, sim.RectangleVelocityY, sim.RectangleHeight);

                
                CircleRepresentation[] circles = new CircleRepresentation[] { futureCircle, new CircleRepresentation() };
                RectangleRepresentation[] rectangles = new RectangleRepresentation[] { futureRectangle, new RectangleRepresentation() };
                futureStatus.Update(circles, rectangles, diamondToCatch, AgentType.Circle);
                
                                

                
                Log.LogInformation(futureStatus.ToString());
                Log.LogInformation(diamondToCatch.ToString());
                Log.LogInformation("Actual Circle: " + circleInfo.ToString());
                Log.LogInformation("Actual Rectangle: " + rectangleInfo.ToString());
                Log.LogInformation("Future Circle: " + futureCircle.ToString());
                Log.LogInformation("Future Rectangle: " + futureRectangle.ToString());


                float rectanglePredictedPositionToDiamondX = sim.RectanglePositionX - diamondX;
                float circlePredictedPositionToRectangleX = sim.CirclePositionX - sim.RectanglePositionX;

                SendRectangleToPosition(diamondToCatch.X + 500, sim.RectanglePositionX);
                
                /*
                if(futureStatus.LEFT_FROM_TARGET != Utils.Quantifier.NONE)
                {
                    move = Moves.ROLL_RIGHT;
                }
                else if(futureStatus.RIGHT_FROM_TARGET != Utils.Quantifier.NONE)
                {
                    move = Moves.ROLL_LEFT;
                }

                if (futureStatus.NEAR_OTHER_AGENT)
                {
                    move = Moves.JUMP;
                }*/

                
                if(futureStatus.MOVING_RIGHT == Utils.Quantifier.A_BIT)
                {
                    move = Moves.NO_ACTION;
                }
                else if(futureStatus.MOVING_RIGHT == Utils.Quantifier.A_LOT)
                {
                    move = Moves.ROLL_LEFT;
                }
                else
                {
                    move = Moves.ROLL_RIGHT;
                }                

            }

            return move;
        }

        private Moves Roll(Utils.Direction direction, Utils.Quantifier speed)
        {
            Moves move = Moves.NO_ACTION;

            Utils.Quantifier Moving_In_Pretended_Direction;
            Moves Roll_Pretended_Direction, Roll_Oposite_Direction;

            //Define directions
            if(direction == Utils.Direction.RIGHT)
            {
                Moving_In_Pretended_Direction = this.agentStatus.MOVING_RIGHT;
                Roll_Pretended_Direction = Moves.ROLL_RIGHT;
                Roll_Oposite_Direction = Moves.ROLL_LEFT;
            }
            else
            {
                Moving_In_Pretended_Direction = this.agentStatus.MOVING_LEFT;
                Roll_Pretended_Direction = Moves.ROLL_LEFT;
                Roll_Oposite_Direction = Moves.ROLL_RIGHT;
            }

            //Decide action
            if(this.predictor != null)
            {
                //TODO Remove this first condition. This is the same as Else condition
                if (Moving_In_Pretended_Direction == speed + 1) //If Agent is moving with a little bit excessive speed
                {
                    Log.LogInformation("SPEED: Good enough - " + Enum.GetName(typeof(Utils.Quantifier), Moving_In_Pretended_Direction).ToString());
                    //move = Moves.NO_ACTION;
                    move = Roll_Oposite_Direction;
                }
                else if (Moving_In_Pretended_Direction <= speed) //When the agent is moving with not enough speed
                {
                    Log.LogInformation("SPEED: Still not enough - " + Enum.GetName(typeof(Utils.Quantifier), Moving_In_Pretended_Direction).ToString());
                    move = Roll_Pretended_Direction;
                }
                else //When the agent is moving with too much speed and needs to brake (> speed + 1)
                {
                    Log.LogInformation("SPEED: Too much! Rolling backwards - " + Enum.GetName(typeof(Utils.Quantifier), Moving_In_Pretended_Direction).ToString());
                    move = Roll_Oposite_Direction;
                }               
            }

            return move;
        }

        //TODO If near Obstacle, but still below, doesnt jump
        private Moves JumpOnto(ObstacleRepresentation obstacle)
        {
            Moves move = Moves.NO_ACTION;

            Status statusCircle = new Status();
            statusCircle.Update(this.circleInfo, this.rectangleInfo, obstacle, AgentType.Circle);

            Utils.Quantifier Distance_From_Obstacle;
            Moves Roll_To_Obstacle;
            Moves Roll_Away_From_Obstacle;
            Utils.Direction Direction_To_Get_To_Obstacle;

            if(statusCircle.LEFT_FROM_TARGET == Utils.Quantifier.NONE) //Circle is on the obstacle's right side
            {
                Distance_From_Obstacle = statusCircle.RIGHT_FROM_TARGET;
                Roll_To_Obstacle = Moves.ROLL_LEFT;
                Roll_Away_From_Obstacle = Moves.ROLL_RIGHT;
                Direction_To_Get_To_Obstacle = Utils.Direction.LEFT;
            }
            else if(statusCircle.RIGHT_FROM_TARGET == Utils.Quantifier.NONE)//Circle is on the obstacle's left side
            {
                Distance_From_Obstacle = statusCircle.LEFT_FROM_TARGET;
                Roll_To_Obstacle = Moves.ROLL_RIGHT;
                Roll_Away_From_Obstacle = Moves.ROLL_LEFT;
                Direction_To_Get_To_Obstacle = Utils.Direction.RIGHT;
            }
            else //Circle is vertically aligned with obstacle
            {
                Distance_From_Obstacle = Utils.Quantifier.NONE;
                Roll_To_Obstacle = Moves.NO_ACTION;
                Roll_Away_From_Obstacle = Moves.NO_ACTION;
                Direction_To_Get_To_Obstacle = Utils.Direction.RIGHT; //Default, not gonna be used
            }

            if(statusCircle.MOVING_TOWARDS_TARGET == Utils.Quantifier.NONE) //When circle is not moving towards the target
            {
                if(statusCircle.NEAR_TARGET)
                {
                    if(statusCircle.ABOVE_TARGET == Utils.Quantifier.SLIGHTLY) //Already Above target
                    {
                        move = Moves.NO_ACTION;
                    }
                    else //Has to roll away from target to gain speed to jump
                    {
                        if(Distance_From_Obstacle == Utils.Quantifier.NONE) //Is directly below target
                        {
                            //TODO CIRCLE MUST FIND A WAY TO TRY TO POSITION HIMSELF TO JUMP
                            move = Moves.NO_ACTION; //TODO PROVISIONAL
                        }
                        else
                        {
                            move = Roll_Away_From_Obstacle;
                        }
                    }
                }
                else //Circle has to start moving in the obstacle's direction, because is far away
                {
                    move = Roll_To_Obstacle;
                }
            }
            else //When the circle is moving towards the target
            {
                if(Distance_From_Obstacle == Utils.Quantifier.SLIGHTLY) //when the circle has to jump now
                {
                    move = Moves.JUMP;
                }
                else if(Distance_From_Obstacle > Utils.Quantifier.A_BIT) //When the circle is very far away, and has to go fast
                {
                    move = Roll(Direction_To_Get_To_Obstacle, Utils.Quantifier.A_BIT);
                }
                else if(Distance_From_Obstacle > Utils.Quantifier.SLIGHTLY) //When the circle is almost in the jumping spot
                {
                    move = Roll(Direction_To_Get_To_Obstacle, Utils.Quantifier.SLIGHTLY);
                }
                else //When an error occurs
                {
                    move = Moves.NO_ACTION;
                }
            }

            Log.LogInformation(statusCircle.ToString());
            Log.LogInformation(move.ToString());
            return move;
        }

        //TODO
        private Moves JumpOntoRectangle()
        {
            Moves move = Moves.NO_ACTION;

            ObstacleRepresentation rectangle = new ObstacleRepresentation(rectangleInfo.X, rectangleInfo.Y, 
                Utils.getRectangleWidth(rectangleInfo.Height), rectangleInfo.Height);

            return JumpOnto(rectangle);
        }

        public void SendRectangleToPosition(float x, float rectanglePredictedPositionX)
        {

            float rectanglePredictedPositionToObjectiveX = rectanglePredictedPositionX - x;

           
            if (rectanglePredictedPositionToObjectiveX < -positionMargin)
            {
                this.SendRequest(new Request(new Command.MoveRight()));
            }
            else if (rectanglePredictedPositionToObjectiveX > positionMargin)
            {
                this.SendRequest(new Request(new Command.MoveLeft()));
            }
            else
            {
                this.SendRequest(new Request(new Command.MorphDown()));
            }
            
        }



        public void SendRequest(Request request)
        {
            this.requests.Enqueue(request);
            this.messages.Add(request.message);
        }

        public void AnswerHandler(Answer answer)
        {
            if(answer.idOfRequest == this.requests.Peek().id)
            {
                Request fulfilled = this.requests.Dequeue();
                System.Diagnostics.Debug.WriteLine("Request " + fulfilled.id + " fulfilled.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Missing an answer");
                this.messages.Add(this.requests.Peek().message);
            }
        }
        
    }
}

