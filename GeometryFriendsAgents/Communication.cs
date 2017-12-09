using GeometryFriends.AI.Communication;
using GeometryFriends.AI.Interfaces;
using System;
using System.Drawing;

namespace GeometryFriendsAgents
{
    public class Message
    {
        protected static int count = 0;
        public int id { get; protected set; }
        public AgentMessage message { get; protected set; }

        public Message()
        {
            this.id = count;

            if(count >= int.MaxValue - 1){
                count = 0;
            }
            else{
                count++;
            }
            
        }
    }
    public class Request : Message
    {
        public Command command { get; private set; }

        public Request(Command command) : base()
        {
            this.command = command;
            this.message = new AgentMessage("Request " + this.id.ToString(), this); // creates a new message with the current request as an attachment;
        }
    }

    public class Answer : Message
    {
        public enum Type { YES, NO };

        public Type type;
        public int idOfRequest;
        public Object attachment;

        public Answer(Type type, int idOfRequest, Object attachment) : base()
        {
            this.type = type;
            this.idOfRequest = idOfRequest;
            this.attachment = attachment;
            this.message = new AgentMessage("Answer to request " + this.idOfRequest, this);
        }
    }

    public abstract class Command
    {
        public abstract Object execute(RectangleAgent executor);

        public class MoveLeft : Command
        {
            public override Object execute(RectangleAgent executor)
            {
                executor.MoveLeft();
                return null;
            }
        }

        public class MoveRight : Command
        {
            public override Object execute(RectangleAgent executor)
            {
                executor.MoveRight();
                return null;
            }
        }

        internal class NoAction : Command
        {
            public override Object execute(RectangleAgent executor)
            {
                executor.NoAction();
                return null;
            }
        }

        internal class MorphDown : Command
        {
            public override Object execute(RectangleAgent executor)
            {
                executor.MorphDown();
                return null;
            }
        }

        internal class MorphUp : Command
        {
            public override Object execute(RectangleAgent executor)
            {
                executor.MorphUp();
                return null;
            }
        }

        public class GetCheapestPath : Command
        {
            public override Object execute(RectangleAgent executor)
            {
                return executor.getCheapestPath();
            }
        }

        public class CatchDiamond : Command
        {
            Node node;

            public CatchDiamond(Node node)
            {
                this.node = node;
            }

            public override Object execute(RectangleAgent executor)
            {
                executor.catchDiamond(node);
                return null;
            }
        }

        public class CatchNextDiamond : Command
        {
            Node node;

            public CatchNextDiamond(Node node)
            {
                this.node = node;
            }

            public override Object execute(RectangleAgent executor)
            {
                executor.catchNextDiamond(node);
                return null;
            }
        }

        public class MoveToPosition : Command
        {
            float x;
            GeometryFriends.AI.Moves move;

            public MoveToPosition(float x, GeometryFriends.AI.Moves move)
            {
                this.x = x;
                this.move = move;
            }

            public override Object execute(RectangleAgent executor)
            {
                executor.MoveToPosition(x, move);
                return null;
            }
        }
    }
}