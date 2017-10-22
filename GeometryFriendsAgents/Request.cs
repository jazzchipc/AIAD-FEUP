using GeometryFriends.AI.Communication;
using System;

namespace GeometryFriendsAgents
{
    public class Request
    {
        public enum Type { MOVE_LEFT, MOVE_RIGHT, MORPH_DOWN, MORPH_UP, ROLL_LEFT, ROLL_RIGHT, GROW };

        private static int count = 0;
        private int id;
        public Type type { get; private set; }
        public AgentMessage message { get; private set; }

        public Request(Type type)
        {
            this.type = type;
            this.id = count;
            this.message = new AgentMessage("Request " + this.id.ToString() + ": ", this); // creates a new message with the current request as an attachment;
            count++;
        }
    }
}