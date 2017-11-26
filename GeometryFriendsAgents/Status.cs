
using GeometryFriends;
using GeometryFriends.AI.Perceptions.Information;
using System;

namespace GeometryFriendsAgents
{
    //Each Agent will have a state machine, which functions according to the next diamond to catch
    public class Status
    {
        private bool Left_From_Target;
        private bool Right_From_Target;
        private bool Above_Target;
        private bool Below_Target;

        private bool With_Obstacle_Between;

        private bool Above_Other_Agent;
        private bool Below_Other_Agent;
        private bool Right_From_Other_Agent;
        private bool Left_From_Other_Agent;
        private bool Near_Other_Agent;

        private bool Moving;
        private bool Other_Agent_Moving;

        private float margin = 10;
        private float circle_radius;
        private float rectangle_height;
        //When in the form of a square, height = width = 100

        public Status()
        {

        }

        public bool LEFT_FROM_TARGET { get => Left_From_Target; set => Left_From_Target = value; }
        public bool RIGHT_FROM_TARGET { get => Right_From_Target; set => Right_From_Target = value; }
        public bool ABOVE_TARGET { get => Above_Target; set => Above_Target = value; }
        public bool BELOW_TARGET { get => Below_Target; set => Below_Target = value; }
        public bool WITH_OBSTACLE_BETWEEN { get => With_Obstacle_Between; set => With_Obstacle_Between = value; }
        public bool ABOVE_OTHER_AGENT { get => Above_Other_Agent; set => Above_Other_Agent = value; }
        public bool BELOW_OTHER_AGENT { get => Below_Other_Agent; set => Below_Other_Agent = value; }
        public bool RIGHT_FROM_OTHER_AGENT { get => Right_From_Other_Agent; set => Right_From_Other_Agent = value; }
        public bool LEFT_FROM_OTHER_AGENT { get => Left_From_Other_Agent; set => Left_From_Other_Agent = value; }
        public bool NEAR_OTHER_AGENT { get => Near_Other_Agent; set => Near_Other_Agent = value; }
        public bool MOVING { get => Moving; set => Moving = value; }
        public bool OTHER_AGENT_MOVING { get => Other_Agent_Moving; set => Other_Agent_Moving = value; }

        public void Update(CircleRepresentation circle, RectangleRepresentation rectangle, CollectibleRepresentation diamondToCatch, AgentType thisAgent)
        {
            this.circle_radius = circle.Radius;
            this.rectangle_height = rectangle.Height;
            if (thisAgent == AgentType.Circle)
            {
                compareAgentWithTarget(circle.X, circle.Y, diamondToCatch.X, diamondToCatch.Y);
                compareAgents(circle.X, circle.Y, rectangle.X, rectangle.Y);
                checkMovement(circle.VelocityX, circle.VelocityY, rectangle.VelocityX, rectangle.VelocityY);
            }
            else
            {
                compareAgentWithTarget(rectangle.X, rectangle.Y, diamondToCatch.X, diamondToCatch.Y);
                compareAgents(rectangle.X, rectangle.Y, circle.X, circle.Y);
                checkMovement(rectangle.VelocityX, rectangle.VelocityY, circle.VelocityX, circle.VelocityY);
            }
        }

        private void compareAgentWithTarget(float agentXposition, float agentYposition, float targetXposition, float targetYposition)
        {
            //X Axis
            //Agent is right from the target diamond
            if (agentXposition > targetXposition + margin)
            {
                this.LEFT_FROM_TARGET = false;
                this.RIGHT_FROM_TARGET = true;
            }
            //Agent is left from the target diamond
            else if (agentXposition < targetXposition - margin)
            {
                this.LEFT_FROM_TARGET = true;
                this.RIGHT_FROM_TARGET = false;
            }
            //Agent is aligned vertically with target diamond
            else
            {
                this.LEFT_FROM_TARGET = false;
                this.RIGHT_FROM_TARGET = false;
            }


            //Y Axis (inverted)
            //Agent is above the target diamond
            if (agentYposition < targetYposition - margin)
            {
                this.ABOVE_TARGET = true;
                this.BELOW_TARGET = false;
            }
            //Agent is below the target diamond
            else if (agentYposition > targetYposition + margin)
            {
                this.ABOVE_TARGET = false;
                this.BELOW_TARGET = true;
            }
            //Agent is aligned horizontally with target diamond
            else
            {
                this.ABOVE_TARGET = false;
                this.BELOW_TARGET = false;
            }
        }

        //TODO Should we consider the size of the agents? (and use it like a margin)
        private void compareAgents(float agent1Xposition, float agent1Yposition, float agent2Xposition, float agent2Yposition)
        {
            float minimum_distance_from_agent_centres_X = circle_radius + Utils.getRectangleWidth(rectangle_height)/2;
            float minimum_distance_from_agent_centres_Y = circle_radius + rectangle_height/2;
            //X Axis
            if (agent1Xposition > agent2Xposition + minimum_distance_from_agent_centres_X + margin)
            {
                this.RIGHT_FROM_OTHER_AGENT = true;
                this.LEFT_FROM_OTHER_AGENT = false;
            }
            else if (agent1Xposition < agent2Xposition - minimum_distance_from_agent_centres_X - margin)
            {
                this.RIGHT_FROM_OTHER_AGENT = false;
                this.LEFT_FROM_OTHER_AGENT = true;
            }
            else
            {
                this.RIGHT_FROM_OTHER_AGENT = false;
                this.LEFT_FROM_OTHER_AGENT = false;
            }


            //Y Axis
            if (agent1Yposition < agent2Yposition - minimum_distance_from_agent_centres_Y - margin)
            {
                this.ABOVE_OTHER_AGENT = true;
                this.BELOW_OTHER_AGENT = false;
            }
            else if (agent1Yposition > agent2Yposition + minimum_distance_from_agent_centres_Y + margin)
            {
                this.ABOVE_OTHER_AGENT = false;
                this.BELOW_OTHER_AGENT = true;
            }
            else
            {
                this.ABOVE_OTHER_AGENT = false;
                this.BELOW_OTHER_AGENT = false;
            }

            if(!this.ABOVE_OTHER_AGENT && !this.BELOW_OTHER_AGENT && !this.RIGHT_FROM_OTHER_AGENT && !this.LEFT_FROM_OTHER_AGENT)
            {
                this.NEAR_OTHER_AGENT = true;
            }
            else
            {
                this.NEAR_OTHER_AGENT = false;
            }
        }

        private void checkMovement(float agent1Xvel, float agent1Yvel, float agent2Xvel, float agent2Yvel)
        {
            if(Math.Abs(agent1Xvel) > 0 || Math.Abs(agent1Yvel) > 0)
            {
                this.MOVING = true;
            }
            else
            {
                this.MOVING = false;
            }

            if(Math.Abs(agent2Xvel) > 0 || Math.Abs(agent2Yvel) > 0)
            {
                this.OTHER_AGENT_MOVING = true;
            }
            else
            {
                this.OTHER_AGENT_MOVING = false;
            }
        }
        public override string ToString()
        {
            return "LEFT FROM TARGET: " + Left_From_Target.ToString() + " | "
                + "RIGHT FROM TARGET: " + Right_From_Target.ToString() + " | "
                + "ABOVE TARGET: " + Above_Target.ToString() + " | "
                + "BELOW TARGET: " + Below_Target.ToString() + " | "
                + "WITH OBSTACLE BETWEEN: " + With_Obstacle_Between.ToString() + " | "
                + "ABOVE OTHER AGENT: " + Above_Other_Agent.ToString() + " | "
                + "BELOW OTHER AGENT: " + Below_Other_Agent.ToString() + " | "
                + "RIGHT FROM OTHER AGENT: " + Right_From_Other_Agent.ToString() + " | "
                + "LEFT FROM OTHER AGENT: " + Left_From_Other_Agent.ToString() + " | "
                + "NEAR OTHER AGENT: " + Near_Other_Agent.ToString() + " | "
                + "CIRCLE_RADIUS: " + circle_radius.ToString() + " | "
                + "RECTANGLE_HEIGHT: " + rectangle_height.ToString();
    }
    }

}