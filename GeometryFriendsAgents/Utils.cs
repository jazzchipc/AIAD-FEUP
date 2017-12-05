using System;
using System.Drawing;

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


        /// <summary>
        /// Gets the distance between two points using the Pythagoras theorem
        /// </summary>
        public static float GetTraversalCost(Point location, Point otherLocation)
        {
            float deltaX = otherLocation.X - location.X;
            float deltaY = otherLocation.Y - location.Y;
            return (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }
    }
}