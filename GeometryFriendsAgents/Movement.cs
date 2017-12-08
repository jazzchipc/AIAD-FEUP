namespace GeometryFriendsAgents
{
    /// <summary>
    /// Class to implement methods that determine whether or not an agent can reach a diamond with his movements and
    /// whether cooperative movements are necessary or not.
    /// </summary>
    public class MovementRestrictions
    {
        public const int CIRCLE_JUMP_MAX_HEIGHT = 240;
        public const int RECTANGLE_MAX_MORPH_HEIGHT = 100;

        Matrix matrix;

        public MovementRestrictions(Matrix matrix)
        {
            this.matrix = matrix;
        }

        public bool canCircleGet(Node circleNode, Node diamondToGetNode)
        {
            return this.circleReachesHeightWithJump(circleNode, diamondToGetNode);
        }

        public bool canRectangleGet(Node rectangleNode, Node diamondToGetNode)
        {
            int deltaY = -(diamondToGetNode.location.Y - rectangleNode.location.Y); // symmetric value because of inverted Y-axis

            if (deltaY > RECTANGLE_MAX_MORPH_HEIGHT)
            {
                return false;
            }

            return true;
        }

        private bool circleReachesHeightWithJump(Node circleNode, Node diamondToGetNode)
        {
            int deltaY = -(diamondToGetNode.location.Y - circleNode.location.Y); // symmetric value because of inverted Y-axis
            if (deltaY > CIRCLE_JUMP_MAX_HEIGHT)
            {
                return false;
            }

            return true;
            
        }

    }
}