using GeometryFriends.AI.Communication;
using GeometryFriends.AI.Interfaces;
using System;

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

        public Answer(Type type, int idOfRequest) : base()
        {
            this.type = type;
            this.idOfRequest = idOfRequest;
            this.message = new AgentMessage("Answer to request " + this.idOfRequest, this);
        }
    }

    public abstract class Command
    {
        public abstract void execute(RectangleAgent executor);
        public abstract void execute(CircleAgent executor);

        public class MoveLeft : Command
        {
            public override void execute(CircleAgent executor)
            {
                throw new NotImplementedException();
            }

            public override void execute(RectangleAgent executor)
            {
                executor.MoveLeft();
            }

        }
    }
}