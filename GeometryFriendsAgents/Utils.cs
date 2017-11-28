namespace GeometryFriendsAgents
{
    public class Utils
    {
        public static float getRectangleWidth(float height)
        {
            float squareWidth = 100;
            return (squareWidth * squareWidth) / height;
        }

        public enum Quantifier { NONE, SLIGHTLY, A_BIT, A_LOT};
        public enum Direction { RIGHT, LEFT };
    }
}