using System;
using System.Drawing;

namespace GeometryFriendsAgents
{
    public class Pixel
    {
        public enum Type
        {
            /// <summary>
            /// Represents an open space for the circle and rectangle
            /// </summary>
            Space,
            /// <summary>
            /// Represents a circle pixel
            /// </summary>
            Circle,
            /// <summary>
            /// Represents a rectangle pixel
            /// </summary>
            Rectangle,
            /// <summary>
            /// Represents a diamond (collectible) pixel
            /// </summary>
            Diamond,
            /// <summary>
            /// Represents an obstacle for the circle and rectangle
            /// </summary>
            Obstacle,
            /// <summary>
            /// Represents a platform which only the circle can go through
            /// </summary>
            CirclePlatform,
            /// <summary>
            /// Represents a platform which only the rectangle can go through
            /// </summary>
            RectanglePlatform
        };

        /// <summary>
        /// The pixel's location in the grid
        /// </summary>
        public Point location { get; private set; }

        /// <summary>
        /// The type of object that the pixel represents.
        /// </summary>
        public Type type { get; set; }

        public Pixel(int x, int y, Type type)
        {
            this.location = new Point(x, y);
            this.type = type;
        }

        public override string ToString()
        {
            return string.Format("Pixel[X: {0}, Y: {1}, Type: {2}]", this.location.X, this.location.Y, this.type);
        }

        public Boolean isWalkable(AgentType agentType)
        {
            if (agentType == AgentType.Circle)
            {
                switch (this.type)
                {
                    case Type.Obstacle:
                    case Type.RectanglePlatform:
                        return false;
                    default:
                        return true;
                }
            }
            else
            {
                switch (this.type)
                {
                    case Type.Obstacle:
                    case Type.CirclePlatform:
                        return false;
                    default:
                        return true;
                }
            }
        }
    }
}