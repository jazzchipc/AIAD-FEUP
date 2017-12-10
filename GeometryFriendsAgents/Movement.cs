namespace GeometryFriendsAgents
{
    /// <summary>
    /// Class to implement methods that determine whether or not an agent can reach a diamond with his movements and
    /// whether cooperative movements are necessary or not.
    /// </summary>
    public class MovementAnalyser
    {
        // magic numbers
        public const int CIRCLE_JUMP_MAX_HEIGHT = 240;
        public const int RECTANGLE_MAX_MORPH_HEIGHT = 150;
        public const int CIRCLE_LAUNCH_MAX_HEIGHT = 650;

        public const int GAME_AREA_WIDTH = 1200;
        public const int GAME_AREA_HEIGHT = 760;


        Matrix matrix;

        public MovementAnalyser(Matrix matrix)
        {
            this.matrix = matrix;
        }

        public bool canCircleGet(Node circleNode, Node diamondToGetNode)
        {
            return this.circleReachesHeightWithJump(circleNode, diamondToGetNode);
        }

        public bool circleReachesHeightWithJump(Node circleNode, Node diamondToGetNode)
        {
            int deltaY = -(diamondToGetNode.location.Y - GAME_AREA_HEIGHT); // symmetric value because of inverted Y-axis
            if (deltaY > CIRCLE_JUMP_MAX_HEIGHT)
            {
                return false;
            }

            return true;
        }

        public bool canRectangleGet(Node rectangleNode, Node diamondToGetNode)
        {
            return (this.rectangleReachesWithMorphUp(rectangleNode, diamondToGetNode) &&
                       this.rectangleBlockedByPlatform(rectangleNode, diamondToGetNode));
        }

        public bool rectangleReachesWithMorphUp(Node rectangleNode, Node diamondToGetNode)
        {
            int deltaY = -(diamondToGetNode.location.Y - GAME_AREA_HEIGHT); // symmetric value because of inverted Y-axis

            if (deltaY > RECTANGLE_MAX_MORPH_HEIGHT)
            {
                return false;
            }

            return true;
        }
        public bool rectangleBlockedByPlatform(Node rectangleNode, Node diamondToGetNode)
        {
 
            for(int i = System.Math.Min(rectangleNode.location.X, diamondToGetNode.location.X); i < System.Math.Max(rectangleNode.location.X, diamondToGetNode.location.X); i++)
            {
                if(this.matrix.getPixel(i, rectangleNode.location.Y).type == Pixel.Type.CirclePlatform)
                {
                    return false;
                }
            }

            return true;
        }


        public bool canCircleAndRectangleGet(Node diamondToGet)
        {
            return coopLaunchReaches(diamondToGet);
        }

        public bool coopLaunchReaches(Node diamondToGet)
        {
            int diamondTrueHeight = GAME_AREA_HEIGHT - diamondToGet.location.Y;  // inverted Y-axis

            if (diamondTrueHeight <= CIRCLE_LAUNCH_MAX_HEIGHT)
            {
                return true;
            }

            return false;
        }
    }
}