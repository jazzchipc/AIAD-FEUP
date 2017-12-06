using GeometryFriends.AI.Perceptions.Information;
using System.Drawing;
using System;

namespace GeometryFriendsAgents
{
    public class Matrix
    {
        private Pixel[,] pixels;

        public int getWidth()
        {
            return this.pixels.GetLength(0);
        }

        public int getHeight()
        {
            return this.pixels.GetLength(1);
        }

        public Matrix(int numOfRows, int numOfCols)
        {
            this.pixels = new Pixel[numOfRows, numOfCols];  // create array

            for(int i = 0; i < this.pixels.GetLength(0); i++)
            {
                for(int j = 0; j < this.pixels.GetLength(1); j++)
                {
                    this.pixels[i, j] = new Pixel(i, j, Pixel.Type.Space);
                }
            }
        }

        public void addPixel(Pixel pixel)
        {
            this.pixels[pixel.location.X, pixel.location.Y] = pixel;
        }

        public Pixel getPixel(int x, int y)
        {
           return this.pixels[x, y];
        }

        public static Matrix generateMatrixFomGameInfo(RectangleRepresentation rI, CircleRepresentation cI, ObstacleRepresentation[] oI, ObstacleRepresentation[] rPI, ObstacleRepresentation[] cPI, CollectibleRepresentation[] colI, Rectangle area)
        {
            Matrix matrix = new Matrix(area.Width + 1, area.Height + 1);

            int rectangleX = (int)rI.X;
            int rectangleY = (int)rI.Y;
            int rectangleWidth = (int)rI.Height; // TODO: calculate width from height

            int circleX = (int)cI.X;
            int circleY = (int)cI.Y;
            int circleRadius = (int)cI.Radius - 20;

            // RECTANGLE
            for (int x = rectangleX - rectangleWidth / 2; x < rectangleX + rectangleWidth / 2; x++)
            {
                for (int y = rectangleY - 10 / 2; y < rectangleY + 10 / 2; y++)
                {
                    if (!matrix.inBounds(x, y)) // x and y out of bounds
                    {
                        continue;
                    }
                    else
                    {
                        matrix.getPixel(x, y).type = Pixel.Type.Rectangle;
                    }
                }
            }

            // CIRCLE -- let's pretende the circle is a square for now
            for (int x = circleX - circleRadius / 2; x < circleX + circleRadius / 2; x++)
            {
                for (int y = circleY - circleRadius / 2; y < circleY + circleRadius / 2; y++)
                {
                    if (!matrix.inBounds(x,y)) // x and y out of bounds
                    {
                        continue;
                    }
                    else
                    {
                        matrix.getPixel(x, y).type = Pixel.Type.Circle;
                    }
                }
            }

            // OBSTACLE
            for (int i = 0; i < oI.Length; i++)
            {
                ObstacleRepresentation obstacle = oI[i];

                for (int x = (int)(obstacle.X - obstacle.Width / 2); x < (int)(obstacle.X + obstacle.Width / 2); x++)
                {
                    for (int y = (int)(obstacle.Y - obstacle.Height / 2); y < (int)(obstacle.Y + obstacle.Height / 2); y++)
                    {
                        if (!matrix.inBounds(x, y)) // x and y out of bounds
                        {
                            continue;
                        }
                        else
                        {
                            matrix.getPixel(x, y).type = Pixel.Type.Obstacle;
                        }
                    }
                }
            }

            // CIRCLE PLATFORM
            for (int i = 0; i < cPI.Length; i++)
            {
                ObstacleRepresentation circlePlatform = cPI[i];

                for (int x = (int)(circlePlatform.X - circlePlatform.Width / 2); x < (int)(circlePlatform.X + circlePlatform.Width / 2); x++)
                {
                    for (int y = (int)(circlePlatform.Y - circlePlatform.Height / 2); y < (int)(circlePlatform.Y + circlePlatform.Height / 2); y++)
                    {
                        if (!matrix.inBounds(x, y)) // x and y out of bounds
                        {
                            continue;
                        }
                        else
                        {
                            matrix.getPixel(x, y).type = Pixel.Type.CirclePlatform;
                        }
                    }
                }
            }

            // RECTANGLE PLATFORM
            for (int i = 0; i < rPI.Length; i++)
            {
                ObstacleRepresentation rectanglePlatform = rPI[i];

                for (int x = (int)(rectanglePlatform.X - rectanglePlatform.Width / 2); x < (int)(rectanglePlatform.X + rectanglePlatform.Width / 2); x++)
                {
                    for (int y = (int)(rectanglePlatform.Y - rectanglePlatform.Height / 2); y < (int)(rectanglePlatform.Y + rectanglePlatform.Height / 2); y++)
                    {
                        if (!matrix.inBounds(x, y)) // x and y out of bounds
                        {
                            continue;
                        }
                        else
                        {
                            matrix.getPixel(x, y).type = Pixel.Type.RectanglePlatform;
                        }
                    }
                }
            }

            // DIAMONDS
            for (int i = 0; i < colI.Length; i++)
            {
                CollectibleRepresentation diamond = colI[i];
                int diamondWidth = 40;

                for (int x = (int)(diamond.X - diamondWidth / 2); x < (int)(diamond.X + diamondWidth / 2); x++)
                {
                    for (int y = (int)(diamond.Y - diamondWidth / 2); y < (int)(diamond.Y + diamondWidth / 2); y++)
                    {
                        if (!matrix.inBounds(x, y)) // x and y out of bounds
                        {
                            continue;
                        }
                        else
                        {
                            matrix.getPixel(x, y).type = Pixel.Type.Diamond;
                        }
                    }
                }
            }

            return matrix;
        }

        /// <summary>
        /// Check if a point "views" the other (if there is a straight line between them with no obstacles)
        /// </summary>
        /// <param name="beginPoint"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public bool walkableLine(Point beginPoint, Point endPoint, AgentType agentType)
        {
            int deltaX = endPoint.X - beginPoint.X;
            int deltaY = endPoint.Y - beginPoint.Y;

            if (deltaX != 0)
            {
                float m = (float)deltaY / (float)deltaX;
                int signX = Math.Sign(deltaX);

                for (int x = 0; x < Math.Abs(deltaX); x++)
                {
                    int stepX = x * signX;
                    float currentY = m * stepX + beginPoint.Y;
                    int currentYFloor = (int)Math.Floor(currentY);
                    int currentYCeiling = (int)Math.Ceiling(currentY);

                    int currentX = beginPoint.X + stepX;

                    if (!xInBounds(currentX) || !this.yInBounds(currentYFloor) || !yInBounds(currentYCeiling))
                    {
                        continue;
                    }

                    // since the line is not really straight, we must for more than one point and use a 'wide' line
                    if (!this.pixels[currentX, (int)Math.Floor(currentY)].isWalkable(agentType) ||
                        !this.pixels[currentX, (int)Math.Ceiling(currentY)].isWalkable(agentType))
                    {
                        return false;
                    }
                }
            }
            else
            {
                int signY = Math.Sign(deltaY);

                for (int y = 0; y < Math.Abs(deltaY); y++)
                {
                    int trueY = beginPoint.Y + (y * signY);

                    // since the line is not really straight, we must for more than one point and use a 'wide' line
                    if (!this.pixels[beginPoint.X, trueY].isWalkable(agentType))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public bool xInBounds(int x)
        {
            return (x < this.getWidth() && x > 0);
        }

        public bool yInBounds(int y)
        {
            return (y < this.getHeight() && y > 0);
        }

        public bool inBounds(int x, int y)
        {
            return (xInBounds(x) && yInBounds(y));
        }

        public bool inBounds(Point point)
        {
            return (xInBounds(point.X) && yInBounds(point.Y));
        }
    }
}