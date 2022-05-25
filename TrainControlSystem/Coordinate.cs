namespace TrainControlSystem
{
    public class Coordinate
    {
        public double X;
        public double Y;
        public Coordinate(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Coordinate Copy()
        {
            return new Coordinate(X, Y);
        }
    }
}
