
using GeometryFriends;
using GeometryFriends.AI;
using GeometryFriends.AI.Perceptions.Information;
using System;
using static GeometryFriendsAgents.Utils;

namespace GeometryFriendsAgents
{
    //Each Agent will have a state machine, which functions according to the next diamond to catch
    public class Status
    {
        private AgentType agentType;
        private Moves actualMove;
        
        private Quantifier Left_From_Target;
        private Quantifier Right_From_Target;
        private Quantifier Above_Target;
        private Quantifier Below_Target;
        private bool Near_Target;

        private bool With_Obstacle_Between; //TODO

        private Quantifier Above_Other_Agent;
        private Quantifier Below_Other_Agent;
        private Quantifier Right_From_Other_Agent;
        private Quantifier Left_From_Other_Agent;
        private bool Near_Other_Agent;

        private bool Moving;
        private bool Other_Agent_Moving;

        private Quantifier Moving_Right;
        private Quantifier Moving_Left;
        private Quantifier Moving_Towards_Target;
        private Quantifier Moving_Up;
        private Quantifier Moving_Down;

        private bool Blocked; //TODO When Agent is MOVING but his position doesn't change
        //TODO Implement Status Moving Fast, and Moving Slow OR Moving Direction

        private float obstacle_margin_X = 10;
        private float obstacle_margin_Y = 10;
        private float circle_radius;
        private float rectangle_height;

        private float a_lot_distance = 200;
        private float a_bit_distance = 50;

        //TODO CHANGE THE SPEEDS
        private float a_lot_speed = 100;
        private float a_bit_speed = 40;
        private float slight_speed = 10;
        //TODO Implement a velocity margin

        private float penetration_margin = 0.08F;



        public Status()
        {

        }

        public Quantifier LEFT_FROM_TARGET { get => Left_From_Target; set => Left_From_Target = value; }
        public Quantifier RIGHT_FROM_TARGET { get => Right_From_Target; set => Right_From_Target = value; }
        public Quantifier ABOVE_TARGET { get => Above_Target; set => Above_Target = value; }
        public Quantifier BELOW_TARGET { get => Below_Target; set => Below_Target = value; }
        public bool WITH_OBSTACLE_BETWEEN { get => With_Obstacle_Between; set => With_Obstacle_Between = value; }
        public Quantifier ABOVE_OTHER_AGENT { get => Above_Other_Agent; set => Above_Other_Agent = value; }
        public Quantifier BELOW_OTHER_AGENT { get => Below_Other_Agent; set => Below_Other_Agent = value; }
        public Quantifier RIGHT_FROM_OTHER_AGENT { get => Right_From_Other_Agent; set => Right_From_Other_Agent = value; }
        public Quantifier LEFT_FROM_OTHER_AGENT { get => Left_From_Other_Agent; set => Left_From_Other_Agent = value; }
        public bool NEAR_OTHER_AGENT { get => Near_Other_Agent; set => Near_Other_Agent = value; }
        public bool MOVING { get => Moving; set => Moving = value; }
        public bool OTHER_AGENT_MOVING { get => Other_Agent_Moving; set => Other_Agent_Moving = value; }
        public Quantifier MOVING_RIGHT { get => Moving_Right; set => Moving_Right = value; }
        public Quantifier MOVING_LEFT { get => Moving_Left; set => Moving_Left = value; }
        public Quantifier MOVING_TOWARDS_TARGET { get => Moving_Towards_Target; set => Moving_Towards_Target = value; }
        public bool NEAR_TARGET { get => Near_Target; set => Near_Target = value; }
        public Quantifier MOVING_UP { get => Moving_Up; set => Moving_Up = value; }
        public Moves ACTUAL_MOVE { get => actualMove; set => actualMove = value; }
        public Quantifier MOVING_DOWN { get => Moving_Down; set => Moving_Down = value; }
        public bool BLOCKED { get => Blocked; set => Blocked = value; }

        //This function only cares for the current Agent

        public void Update(CircleRepresentation actualCircle, RectangleRepresentation actualRectangle, CollectibleRepresentation diamondToCatch, AgentType thisAgent)
        {
            this.circle_radius = actualCircle.Radius;
            this.rectangle_height = actualRectangle.Height;
            if (thisAgent == AgentType.Circle)
            {
                compareAgentWithTarget(actualCircle.X, actualCircle.Y, diamondToCatch.X, diamondToCatch.Y);
                compareAgents(actualCircle.X, actualCircle.Y, actualRectangle.X, actualRectangle.Y);
                checkMovement(actualCircle.VelocityX, actualCircle.VelocityY, actualRectangle.VelocityX, actualRectangle.VelocityY);
            }
            else
            {
                compareAgentWithTarget(actualRectangle.X, actualRectangle.Y, diamondToCatch.X, diamondToCatch.Y);
                compareAgents(actualRectangle.X, actualRectangle.Y, actualCircle.X, actualCircle.Y);
                checkMovement(actualRectangle.VelocityX, actualRectangle.VelocityY, actualCircle.VelocityX, actualCircle.VelocityY);
            }
        }

        //This function takes into account also the future representation of the agent
        public void Update(CircleRepresentation[] circles, RectangleRepresentation[] rectangles, CollectibleRepresentation diamondToCatch, AgentType thisAgent)
        {
            this.agentType = thisAgent;
            CircleRepresentation actualCircle = circles[0];
            CircleRepresentation futureCircle = circles[1];
            RectangleRepresentation actualRectangle = rectangles[0];
            RectangleRepresentation futureRectangle = rectangles[1];

            
            this.circle_radius = actualCircle.Radius;
            this.rectangle_height = actualRectangle.Height;
            Log.LogInformation("UPDATE Diamond");
            if (thisAgent == AgentType.Circle)
            {
                compareAgentWithTarget(actualCircle.X, actualCircle.Y, diamondToCatch.X, diamondToCatch.Y);
                compareAgents(actualCircle.X, actualCircle.Y, actualRectangle.X, actualRectangle.Y);
                checkMovement(actualCircle.VelocityX, actualCircle.VelocityY, actualRectangle.VelocityX, actualRectangle.VelocityY);
                checkMovementRelativeToTarget(actualCircle.X, actualCircle.VelocityX, diamondToCatch.X);
            }
            else
            {
                compareAgentWithTarget(actualRectangle.X, actualRectangle.Y, diamondToCatch.X, diamondToCatch.Y);
                compareAgents(actualRectangle.X, actualRectangle.Y, actualCircle.X, actualCircle.Y);
                checkMovement(actualRectangle.VelocityX, actualRectangle.VelocityY, actualCircle.VelocityX, actualCircle.VelocityY);
                checkMovementRelativeToTarget(actualRectangle.X, actualRectangle.VelocityX, diamondToCatch.X);
            }
        }

        public void Update(CircleRepresentation circle, RectangleRepresentation rectangle, ObstacleRepresentation obstacle, AgentType thisAgent)
        {
            this.agentType = thisAgent;
            this.obstacle_margin_X = obstacle.Width / 2;
            this.obstacle_margin_Y = obstacle.Height / 2;
            this.circle_radius = circle.Radius;
            this.rectangle_height = rectangle.Height;

            Log.LogInformation("UPDATE Obstacle");
            if(thisAgent == AgentType.Circle)
            {
                compareAgentWithTarget(circle.X, circle.Y, obstacle.X, obstacle.Y);
                compareAgents(circle.X, circle.Y, rectangle.X, rectangle.Y);
                checkMovement(circle.VelocityX, circle.VelocityY, rectangle.VelocityX, rectangle.VelocityY);
                checkMovementRelativeToTarget(circle.X, circle.VelocityX, obstacle.X);
            }
            else
            {
                compareAgentWithTarget(rectangle.X, rectangle.Y, obstacle.X, obstacle.Y);
                compareAgents(rectangle.X, rectangle.Y, circle.X, circle.Y);
                checkMovement(rectangle.VelocityX, rectangle.VelocityY, circle.VelocityX, circle.VelocityY);
                checkMovementRelativeToTarget(rectangle.X, rectangle.VelocityX, obstacle.X);
            }
        }

        public void Update(CircleRepresentation circle, RectangleRepresentation rectangle, ObstacleRepresentation obstacle, 
            AgentType thisAgent, Moves actualMove)
        {
            this.ACTUAL_MOVE = actualMove;
            this.Update(circle, rectangle, obstacle, thisAgent);
            this.checkBlocked();
        }

        public void Update(CircleRepresentation circle, RectangleRepresentation rectangle, CollectibleRepresentation diamondToCatch,
            AgentType thisAgent, Moves actualMove)
        {
            this.ACTUAL_MOVE = actualMove;
            this.Update(circle, rectangle, diamondToCatch, thisAgent);
            this.checkBlocked();
        }


        private void compareAgentWithTarget(float agentXposition, float agentYposition, float targetXposition, float targetYposition)
        {
            float targetRightBound = targetXposition + obstacle_margin_X - penetration_margin;
            float targetLeftBound = targetXposition - obstacle_margin_X + penetration_margin;
            float targetUpperBound = targetYposition - obstacle_margin_Y + penetration_margin; //Because Y is inverted
            float targetLowerBound = targetYposition + obstacle_margin_Y - penetration_margin;

            float agentRightBound;
            float agentLeftBound;
            float agentUpperBound;
            float agentLowerBound;

            if(this.agentType == AgentType.Circle)
            {
                agentRightBound = agentXposition + circle_radius;
                agentLeftBound = agentXposition - circle_radius;
                agentUpperBound = agentYposition - circle_radius; //Because Y is inverted
                agentLowerBound = agentYposition + circle_radius;
            }
            else
            {
                agentRightBound = agentXposition + getRectangleWidth(rectangle_height)/2;
                agentLeftBound = agentXposition - getRectangleWidth(rectangle_height) / 2;
                agentUpperBound = agentYposition - rectangle_height/2; //Because Y is inverted
                agentLowerBound = agentYposition + rectangle_height/2;
            }



            Log.LogInformation("Agent - " + "L:" + agentLeftBound + "|R:" + agentRightBound + "|U:" + agentUpperBound + "|L:" + agentLowerBound);
            Log.LogInformation("Target - " + "L:" + targetLeftBound + "|R:" + targetRightBound + "|U:" + targetUpperBound + "|L:" + targetLowerBound);
            //X Axis
            //Agent is right from the target diamond
            if (agentLeftBound >= targetRightBound)
            {
                //a lot
                if(agentLeftBound >= targetRightBound + a_lot_distance)
                {
                    this.RIGHT_FROM_TARGET = Quantifier.A_LOT;
                }
                //just a bit
                else if(agentLeftBound >= targetRightBound + a_bit_distance)
                {
                    this.RIGHT_FROM_TARGET = Quantifier.A_BIT;
                }
                //slightly
                else
                {
                    this.RIGHT_FROM_TARGET = Quantifier.SLIGHTLY;
                }

                this.LEFT_FROM_TARGET = Quantifier.NONE;
            }
            //Agent is left from the target diamond
            else if (agentRightBound <= targetLeftBound)
            {
                //a lot
                if(agentRightBound <= targetLeftBound - a_lot_distance)
                {
                    this.LEFT_FROM_TARGET = Quantifier.A_LOT;
                }
                //just a bit
                else if(agentRightBound <= targetLeftBound - a_bit_distance)
                {
                    this.LEFT_FROM_TARGET = Quantifier.A_BIT;
                }
                //slightly
                else
                {
                    this.LEFT_FROM_TARGET = Quantifier.SLIGHTLY;
                }

                this.RIGHT_FROM_TARGET = Quantifier.NONE;
            }
            //Agent is aligned vertically with target diamond
            else
            {
                this.LEFT_FROM_TARGET = Quantifier.NONE;
                this.RIGHT_FROM_TARGET = Quantifier.NONE;
            }


            //Y Axis (inverted)
            //Agent is above the target diamond
            if (agentLowerBound <= targetUpperBound)
            {
                //a lot
                if(agentLowerBound <= targetUpperBound - a_lot_distance)
                {
                    this.ABOVE_TARGET = Quantifier.A_LOT;
                }
                //just a bit
                else if(agentLowerBound <= targetUpperBound - a_bit_distance)
                {
                    this.ABOVE_TARGET = Quantifier.A_BIT;
                }
                //slightly
                else
                {
                    this.ABOVE_TARGET = Quantifier.SLIGHTLY;
                }

                this.BELOW_TARGET = Quantifier.NONE;
            }
            //Agent is below the target diamond
            else if (agentUpperBound >= targetLowerBound)
            {
                //a lot
                if(agentUpperBound >= targetLowerBound + a_lot_distance)
                {
                    this.BELOW_TARGET = Quantifier.A_LOT;
                }
                //just a bit
                else if(agentUpperBound >= targetLowerBound + a_bit_distance)
                {
                    this.BELOW_TARGET = Quantifier.A_BIT;
                }
                //slightly
                else
                {
                    this.BELOW_TARGET = Quantifier.SLIGHTLY;
                }

                this.ABOVE_TARGET = Quantifier.NONE;
            }
            //Agent is aligned horizontally with target diamond
            else
            {
                this.ABOVE_TARGET = Quantifier.NONE;
                this.BELOW_TARGET = Quantifier.NONE;
            }

            //Check Corners (using the agent centre)
            if (this.ABOVE_TARGET == Quantifier.NONE &&
                this.BELOW_TARGET == Quantifier.NONE &&
                this.LEFT_FROM_TARGET == Quantifier.NONE &&
                this.RIGHT_FROM_TARGET == Quantifier.NONE)
            {
                if (agentXposition <= targetLeftBound)
                {
                    this.LEFT_FROM_TARGET = Quantifier.SLIGHTLY;
                }
                if (agentXposition >= targetRightBound)
                {
                    this.RIGHT_FROM_TARGET = Quantifier.SLIGHTLY;
                }
                if (agentYposition <= targetUpperBound)
                {
                    this.ABOVE_TARGET = Quantifier.SLIGHTLY;
                }
                if (agentYposition >= targetLowerBound)
                {
                    this.BELOW_TARGET = Quantifier.SLIGHTLY;
                }
            }

            this.NEAR_TARGET = checkNear(new Quantifier[4] { this.ABOVE_TARGET, this.BELOW_TARGET, this.RIGHT_FROM_TARGET, this.LEFT_FROM_TARGET });
        }

        private void compareAgents(float agent1Xposition, float agent1Yposition, float agent2Xposition, float agent2Yposition)
        {
            float minimum_distance_from_agent_centres_X = circle_radius + Utils.getRectangleWidth(rectangle_height)/2;
            float minimum_distance_from_agent_centres_Y = circle_radius + rectangle_height/2;

            float agent2RightBound = agent2Xposition + minimum_distance_from_agent_centres_X - penetration_margin; //Minimum distances between centres for the agents to be next to each other
            float agent2LeftBound = agent2Xposition - minimum_distance_from_agent_centres_X + penetration_margin;
            float agent2UpperBound = agent2Yposition - minimum_distance_from_agent_centres_Y + penetration_margin;
            float agent2LowerBound = agent2Yposition + minimum_distance_from_agent_centres_Y - penetration_margin;

            //X Axis
            if (agent1Xposition > agent2RightBound)
            {
                if(agent1Xposition > agent2RightBound + a_lot_distance)
                {
                    this.RIGHT_FROM_OTHER_AGENT = Quantifier.A_LOT;
                    this.LEFT_FROM_OTHER_AGENT = Quantifier.NONE;
                }
                else if(agent1Xposition > agent2RightBound + a_bit_distance) 
                {
                    this.RIGHT_FROM_OTHER_AGENT = Quantifier.A_BIT;
                    this.LEFT_FROM_OTHER_AGENT = Quantifier.NONE;
                }
                else
                {
                    this.RIGHT_FROM_OTHER_AGENT = Quantifier.SLIGHTLY;
                    this.LEFT_FROM_OTHER_AGENT = Quantifier.NONE;
                }
            }
            else if (agent1Xposition < agent2LeftBound)
            {
                if(agent1Xposition < agent2LeftBound - a_lot_distance)
                {
                    this.RIGHT_FROM_OTHER_AGENT = Quantifier.NONE;
                    this.LEFT_FROM_OTHER_AGENT = Quantifier.A_LOT;
                }
                else if(agent1Xposition < agent2LeftBound - a_bit_distance)
                {
                    this.RIGHT_FROM_OTHER_AGENT = Quantifier.NONE;
                    this.LEFT_FROM_OTHER_AGENT = Quantifier.A_BIT;
                }
                else
                {
                    this.RIGHT_FROM_OTHER_AGENT = Quantifier.NONE;
                    this.LEFT_FROM_OTHER_AGENT = Quantifier.SLIGHTLY;
                }
            }
            else
            {
                this.RIGHT_FROM_OTHER_AGENT = Quantifier.NONE;
                this.LEFT_FROM_OTHER_AGENT = Quantifier.NONE;
            }


            //Y Axis
            if (agent1Yposition < agent2UpperBound)
            {
                if(agent1Yposition < agent2UpperBound - a_lot_distance)
                {
                    this.ABOVE_OTHER_AGENT = Quantifier.A_LOT;
                    this.BELOW_OTHER_AGENT = Quantifier.NONE;
                }
                else if(agent1Yposition < agent2UpperBound - a_bit_distance)
                {
                    this.ABOVE_OTHER_AGENT = Quantifier.A_BIT;
                    this.BELOW_OTHER_AGENT = Quantifier.NONE;
                }
                else
                {
                    this.ABOVE_OTHER_AGENT = Quantifier.SLIGHTLY;
                    this.BELOW_OTHER_AGENT = Quantifier.NONE;
                }
            }
            else if (agent1Yposition > agent2LowerBound)
            {
                if(agent1Yposition > agent2LowerBound + a_lot_distance)
                {
                    this.ABOVE_OTHER_AGENT = Quantifier.NONE;
                    this.BELOW_OTHER_AGENT = Quantifier.A_LOT;
                }
                else if(agent1Yposition > agent2LowerBound + a_bit_distance)
                {
                    this.ABOVE_OTHER_AGENT = Quantifier.NONE;
                    this.BELOW_OTHER_AGENT = Quantifier.A_BIT;
                }
                else
                {
                    this.ABOVE_OTHER_AGENT = Quantifier.NONE;
                    this.BELOW_OTHER_AGENT = Quantifier.SLIGHTLY;
                }
            }
            else
            {
                this.ABOVE_OTHER_AGENT = Quantifier.NONE;
                this.BELOW_OTHER_AGENT = Quantifier.NONE;
            }

            //Check Corners (using the agent centre)
            if (this.ABOVE_OTHER_AGENT == Quantifier.NONE &&
                this.BELOW_OTHER_AGENT == Quantifier.NONE &&
                this.LEFT_FROM_OTHER_AGENT == Quantifier.NONE &&
                this.RIGHT_FROM_OTHER_AGENT == Quantifier.NONE)
            {
                if (agent1Xposition <= agent2LeftBound)
                {
                    this.LEFT_FROM_OTHER_AGENT = Quantifier.SLIGHTLY;
                }
                if (agent1Xposition >= agent2RightBound)
                {
                    this.RIGHT_FROM_OTHER_AGENT = Quantifier.SLIGHTLY;
                }
                if (agent1Yposition <= agent2UpperBound)
                {
                    this.ABOVE_OTHER_AGENT = Quantifier.SLIGHTLY;
                }
                if (agent1Yposition >= agent2LowerBound)
                {
                    this.BELOW_OTHER_AGENT = Quantifier.SLIGHTLY;
                }
            }

            this.NEAR_OTHER_AGENT = checkNear(new Quantifier[4] { this.ABOVE_OTHER_AGENT, this.BELOW_OTHER_AGENT, this.RIGHT_FROM_OTHER_AGENT, this.LEFT_FROM_OTHER_AGENT });

            Log.LogInformation("Agent1 - " + "X:" + agent1Xposition + "|Y:" + agent1Yposition);
            Log.LogInformation("Agent2Bounds - " + "L:" + agent2LeftBound + "|R:" + agent2RightBound + "|U:"
                + agent2UpperBound + "|L:" + agent2LowerBound);
        }

        private void checkMovement(float agent1Xvel, float agent1Yvel, float agent2Xvel, float agent2Yvel)
        {
            if(Math.Abs(agent2Xvel) > 0 || Math.Abs(agent2Yvel) > 0)
            {
                this.OTHER_AGENT_MOVING = true;
            }
            else
            {
                this.OTHER_AGENT_MOVING = false;
            }

            if(agent1Xvel > 0)
            {
                this.MOVING_LEFT = Quantifier.NONE;
                if (agent1Xvel > a_lot_speed)
                {
                    this.MOVING_RIGHT = Quantifier.A_LOT;
                }
                else if(agent1Xvel > a_bit_speed)
                {
                    this.MOVING_RIGHT = Quantifier.A_BIT;
                }
                else if(agent1Xvel > slight_speed)
                {
                    this.MOVING_RIGHT = Quantifier.SLIGHTLY;
                }
                else
                {
                    this.MOVING_RIGHT = Quantifier.NONE;
                }
            }
            else if(agent1Xvel < 0)
            {
                this.MOVING_RIGHT = Quantifier.NONE;
                if (agent1Xvel < -a_lot_speed)
                {
                    this.MOVING_LEFT = Quantifier.A_LOT;
                }
                else if(agent1Xvel < -a_bit_speed)
                {
                    this.MOVING_LEFT = Quantifier.A_BIT;
                }
                else if(agent1Xvel < -slight_speed)
                {
                    this.MOVING_LEFT = Quantifier.SLIGHTLY;
                }
                else
                {
                    this.MOVING_LEFT = Quantifier.NONE;
                }
            }
            else
            {
                this.MOVING_RIGHT = Quantifier.NONE;
                this.MOVING_LEFT = Quantifier.NONE;
            }


            if(agent1Yvel < -a_lot_speed)
            {
                this.MOVING_UP = Quantifier.A_LOT;
            }
            else if(agent1Yvel < -a_bit_speed)
            {
                this.MOVING_UP = Quantifier.A_BIT;
            }
            else if(agent1Yvel < -slight_speed)
            {
                this.MOVING_UP = Quantifier.SLIGHTLY;
            }
            else
            {
                this.MOVING_UP = Quantifier.NONE;
            }

            if(agent1Yvel > a_lot_speed)
            {
                this.MOVING_DOWN = Quantifier.A_LOT;
            }
            else if(agent1Yvel > a_bit_speed)
            {
                this.MOVING_DOWN = Quantifier.A_BIT;
            }
            else if(agent1Yvel > slight_speed)
            {
                this.MOVING_DOWN = Quantifier.SLIGHTLY;
            }
            else
            {
                this.MOVING_DOWN = Quantifier.NONE;
            }

            if(this.MOVING_UP == Quantifier.NONE &&
                this.MOVING_DOWN == Quantifier.NONE &&
                this.MOVING_LEFT == Quantifier.NONE &&
                this.MOVING_RIGHT == Quantifier.NONE)
            {
                this.MOVING = false;
            }
            else
            {
                this.MOVING = true;
            }
        }


        public void checkMovementRelativeToTarget(float agentPosX, float agentVelX, float targetPosX)
        {
            float targetRightBound = targetPosX + obstacle_margin_X;
            float targetLeftBound = targetPosX - obstacle_margin_X;

            if (agentPosX > targetRightBound)
            {
                if(agentVelX < -a_lot_speed)
                {
                    this.MOVING_TOWARDS_TARGET = Quantifier.A_LOT;
                }
                else if(agentVelX < -a_bit_speed)
                {
                    this.MOVING_TOWARDS_TARGET = Quantifier.A_BIT;
                }
                else if(agentVelX < -slight_speed)
                {
                    this.MOVING_TOWARDS_TARGET = Quantifier.SLIGHTLY;
                }
                else
                {
                    this.MOVING_TOWARDS_TARGET = Quantifier.NONE;
                }
            }
            else if(agentPosX < targetLeftBound)
            {
                if(agentVelX > a_lot_speed)
                {
                    this.MOVING_TOWARDS_TARGET = Quantifier.A_LOT;
                }
                else if(agentVelX > a_bit_speed)
                {
                    this.MOVING_TOWARDS_TARGET = Quantifier.A_BIT;
                }
                else if(agentVelX > slight_speed)
                {
                    this.MOVING_TOWARDS_TARGET = Quantifier.SLIGHTLY;
                }
                else
                {
                    this.MOVING_TOWARDS_TARGET = Quantifier.NONE;
                }
            }
            else
            {
                this.MOVING_TOWARDS_TARGET = Quantifier.NONE;
            }
        }

        private void checkBlocked()
        {
            if(this.ACTUAL_MOVE == Moves.MOVE_LEFT || this.ACTUAL_MOVE == Moves.MOVE_RIGHT
                || this.ACTUAL_MOVE == Moves.ROLL_LEFT || this.ACTUAL_MOVE == Moves.ROLL_RIGHT)
            {
                if(!this.MOVING)
                {
                    this.BLOCKED = true;
                }
                else
                {
                    this.BLOCKED = false;
                }
            }
        }

        public override string ToString()
        {
            return "LEFT FROM TARGET: " + Left_From_Target.ToString() + " | "
                + "RIGHT FROM TARGET: " + Right_From_Target.ToString() + " | "
                + "ABOVE TARGET: " + Above_Target.ToString() + " | "
                + "BELOW TARGET: " + Below_Target.ToString() + " | "
                + "NEAR_TARGET: " + Near_Target.ToString() + " | "
                + "WITH OBSTACLE BETWEEN: " + With_Obstacle_Between.ToString() + " | "
                + "ABOVE OTHER AGENT: " + Above_Other_Agent.ToString() + " | "
                + "BELOW OTHER AGENT: " + Below_Other_Agent.ToString() + " | "
                + "RIGHT FROM OTHER AGENT: " + Right_From_Other_Agent.ToString() + " | "
                + "LEFT FROM OTHER AGENT: " + Left_From_Other_Agent.ToString() + " | "
                + "NEAR OTHER AGENT: " + Near_Other_Agent.ToString() + " | "
                + "CIRCLE_RADIUS: " + circle_radius.ToString() + " | "
                + "RECTANGLE_HEIGHT: " + rectangle_height.ToString() + " | "
                + "OBSTACLE_MARGIN_X: " + obstacle_margin_X.ToString() + " | "
                + "OBSTACLE_MARGIN_Y: " + obstacle_margin_Y.ToString() + " | "
                + "MOVING: " + Moving.ToString() + " | "
                + "MOVING LEFT: " + Moving_Left.ToString() + " | "
                + "MOVING RIGHT: " + Moving_Right.ToString() + " | "
                + "MOVING UP: " + Moving_Up.ToString() + " | "
                + "MOVING TOWARDS TARGET: " + Moving_Towards_Target.ToString() + " | "
                + "BLOCKED: " + Blocked.ToString();
        }

        //Number of flags should be 4 (Above, Below, Right, Left)
        private bool checkNear(Quantifier[] flags)
        {
            if(flags.Length != 4)
            {
                throw new ArgumentException("There should be exactly 4 flags");
            }
            else
            {
                int slightly_count = 0;
                int none_count = 0;
                for(int i = 0; i < flags.Length; i++)
                {
                    if(flags[i] == Quantifier.NONE)
                    {
                        none_count++;
                    }
                    else if(flags[i] == Quantifier.SLIGHTLY)
                    {
                        slightly_count++;
                    }
                }

                if((slightly_count == 1 && none_count == 3) || (slightly_count == 2 && none_count == 2)) //To be near, 1 flag should be SLIGHTLY and the other 3 NONE
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }

}