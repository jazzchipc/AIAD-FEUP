using GeometryFriends.AI.Perceptions.Information;
using System.Drawing;

namespace GeometryFriendsAgents
{
    public class Matrix
    {
        public Node[,] nodes;

        public Point objective { get; private set; }

        public int getHeight()
        {
            return this.nodes.Length;
        }

        public int getWidth()
        {
            return this.nodes.GetLength(0);
        }

        public Matrix(int numOfRows, int numOfCols, Point objective)
        {
            this.objective = objective;

            this.nodes = new Node[numOfRows + 3, numOfCols + 3];

            for(int i = 0; i < this.nodes.GetLength(0); i++)
            {
                for(int j = 0; j < this.nodes.GetLength(1); j++)
                {
                    this.nodes[i, j] = new Node(i, j, Node.Type.Space, objective);
                }
            }
        }

        public void addNode(Node node)
        {
            this.nodes[node.location.X, node.location.Y] = node;
        }

        public Node getNode(int x, int y)
        {
            return this.nodes[y, x];
        }

        public static Matrix generateMatrixFomGameInfo(RectangleRepresentation rI, CircleRepresentation cI, ObstacleRepresentation[] oI, ObstacleRepresentation[] rPI, ObstacleRepresentation[] cPI, CollectibleRepresentation[] colI, Rectangle area)
        {
            Matrix matrix = new Matrix(area.Width, area.Height, new Point((int)colI[0].X, (int)colI[0].Y));

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
                    matrix.getNode(x, y).type = Node.Type.Rectangle;
                }
            }

            // CIRCLE -- let's pretende the circle is a square for now
            for (int x = circleX - circleRadius / 2; x < circleX + circleRadius / 2; x++)
            {
                for (int y = circleY - circleRadius / 2; y < circleY + circleRadius / 2; y++)
                {
                    matrix.getNode(x, y).type = Node.Type.Circle;
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
                        matrix.getNode(x, y).type = Node.Type.Obstacle;
                    }
                }
            }

            return matrix;
        }

    }


}