using System;

namespace TrainControlSystem
{
    public enum TrackSegmentType : int
    {
        // CW (clockwise) and CCW (counterclockwise) refer to the direction the curve proceeds
        // from StartPosition to EndPosition. For example the center of ________
        // from top left to bottom right will be closest to:
        // CWQuarter: top right
        // CCWQuarter: bottom left
        Straight = 0,
        CWEighthCurve = 2,
        CCWEighthCurve = 3,
        CWQuarterCurve = 4,
        CCWQuarterCurve = 5,
        CWHalfCurve = 6,
        CCWHalfCurve = 7
    }

    public static class TrackSegmentTypeExtentions
    {
        public static double GetAngle(this TrackSegmentType trackSegmentType)
        {
            return trackSegmentType switch
            {
                TrackSegmentType.Straight => 0,
                TrackSegmentType.CWEighthCurve or TrackSegmentType.CCWEighthCurve => Math.PI / 4,
                TrackSegmentType.CWQuarterCurve or TrackSegmentType.CCWQuarterCurve => Math.PI / 2,
                TrackSegmentType.CWHalfCurve or TrackSegmentType.CCWHalfCurve => Math.PI,
                _ => throw new ArgumentException("Invalid TrackSegmentType."),
            };
        }

        public static double GetSignedAngle(this TrackSegmentType trackSegmentType)
        {
            return trackSegmentType switch
            {
                TrackSegmentType.Straight => 0,
                TrackSegmentType.CWEighthCurve => -Math.PI / 4,
                TrackSegmentType.CCWEighthCurve => Math.PI / 4,
                TrackSegmentType.CWQuarterCurve => -Math.PI / 2,
                TrackSegmentType.CCWQuarterCurve => Math.PI / 2,
                TrackSegmentType.CWHalfCurve => -Math.PI,
                TrackSegmentType.CCWHalfCurve => Math.PI,
                _ => throw new ArgumentException("Invalid TrackSegmentType."),
            };
        }

        public static bool IsCW(this TrackSegmentType trackSegmentType)
        {
            return (int)trackSegmentType % 2 == 0;
        }

        public static bool IsCCW(this TrackSegmentType trackSegmentType)
        {
            return (int)trackSegmentType % 2 == 1;
        }
    }
    public class TrackSegment
    {
        // Physical parameters of the track segment.
        // Positions are rail distances while coordinates are absolute positions
        public readonly int ID;
        public double StartPosition { get; }
        public double EndPosition { get; }
        public Coordinate StartCoordinate { get; }
        public Coordinate EndCoordinate { get; }
        public TrackSegmentType Type { get; }
        public double Length { get; } // Length of track (startPosition - endPosition)
        public double Distance { get; } // Distance between start and end coordinates
        public double Radius { get; } // Radius of the curve (does not apply to TrackSegmentType.Straight)
        public Coordinate Center { get; } // The center of the track segment
                                          // (equivalent to the midpoint for TrackSegmentType.Straight)
        public double StartAngle { get; } // The angle the track proceeds from the start
                                          // in radians counter-clockwise from right (+X)
        public double EndAngle { get; } // The angle the track proceeds from the end
                                        // in radians counter-clockwise from right (+X)

        public TrackSegment(int id, double startPosition, Coordinate startCoordinate,
            double startAngle, double radius, TrackSegmentType type)
        {
            ID = id;
            StartPosition = startPosition;
            StartCoordinate = startCoordinate;
            StartAngle = startAngle;
            Radius = radius;
            Type = type;

            EndAngle = StartAngle + Type.GetSignedAngle();

            if (Type == TrackSegmentType.Straight)
            {
                Center = new(
                    StartCoordinate.X + Radius * Math.Cos(StartAngle),
                    StartCoordinate.Y + Radius * Math.Sin(StartAngle)
                );
                Distance = Radius * 2;
                Length = Distance;
                EndCoordinate = new(
                    StartCoordinate.X + Distance * Math.Cos(StartAngle),
                    StartCoordinate.Y + Distance * Math.Sin(StartAngle)
                );
            }
            else
            {

                double forwardX = Math.Cos(startAngle);
                double forwardY = Math.Sin(startAngle);
                double leftX = -Math.Sin(startAngle);
                double leftY = Math.Cos(startAngle);
                double trackAngle = Type.GetAngle();
                EndCoordinate = new(
                    StartCoordinate.X + Radius * (Math.Sin(trackAngle) * forwardX + (1 - Math.Cos(trackAngle)) * (Type.IsCW() ? -leftX : leftX)),
                    StartCoordinate.Y + Radius * (Math.Sin(trackAngle) * forwardY + (1 - Math.Cos(trackAngle)) * (Type.IsCW() ? -leftY : leftY))
                );
                Distance = Math.Sqrt(2 * Math.Pow(Radius, 2) * (1 - Math.Cos(trackAngle)));
                Length = trackAngle * Radius;
                Center = new(
                    StartCoordinate.X + Radius * (Type.IsCW() ? -leftX : leftX),
                    StartCoordinate.Y + Radius * (Type.IsCW() ? -leftY : leftY)
                );
            }

            EndPosition = StartPosition + Length;
        }

        public TrackSegment(int id, double startPosition, Coordinate startCoordinate,
            Coordinate endCoordinate, TrackSegmentType type)
        {
            ID = id;
            StartPosition = startPosition;
            StartCoordinate = startCoordinate;
            EndCoordinate = endCoordinate;
            Type = type;

            Distance = Math.Sqrt(Math.Pow(EndCoordinate.X - StartCoordinate.X, 2)
                + Math.Pow(EndCoordinate.Y - StartCoordinate.Y, 2));

            // Calculate radius and length.
            switch (Type)
            {
                case TrackSegmentType.Straight:
                    Length = Distance;
                    Radius = Distance / 2;
                    break;
                case TrackSegmentType.CWEighthCurve:
                case TrackSegmentType.CCWEighthCurve:
                    Radius = Distance / Math.Sqrt(2 - Math.Sqrt(2));
                    Length = Math.PI * Radius / 4;
                    break;
                case TrackSegmentType.CWQuarterCurve:
                case TrackSegmentType.CCWQuarterCurve:
                    Radius = Distance / Math.Sqrt(2);
                    Length = Math.PI * Radius / 2;
                    break;
                case TrackSegmentType.CWHalfCurve:
                case TrackSegmentType.CCWHalfCurve:
                    Radius = Distance / 2;
                    Length = Math.PI * Radius;
                    break;
                default:
                    throw new ArgumentException("Invalid TrackSegmentType.");
            }

            EndPosition = StartPosition + Length;

            // Calculate center coordinate.
            if (Type == TrackSegmentType.Straight)
            {
                Center = new Coordinate(
                    (EndCoordinate.X - StartCoordinate.X) / 2 + StartCoordinate.X,
                    (EndCoordinate.Y - StartCoordinate.Y) / 2 + StartCoordinate.Y
                );
                StartAngle = Math.Atan2(EndCoordinate.Y - StartCoordinate.Y, EndCoordinate.X - StartCoordinate.X);
            }
            else
            {
                // Finding the midpoint of a circle given two points
                // and a radius (https://math.stackexchange.com/q/1781546).

                // The midpoint is halfway between the start and end.
                // a is the distance between the start or end and the midpoint.
                double a = Distance / 2;
                double aX = (EndCoordinate.X - StartCoordinate.X) / 2;
                double aY = (EndCoordinate.Y - StartCoordinate.Y) / 2;
                double midpointX = StartCoordinate.X + aX;
                double midpointY = StartCoordinate.Y + aY;

                // b is the distance between the midpoint and the arc's center.
                double b = Math.Sqrt(Math.Pow(Radius, 2) - Math.Pow(a, 2));

                // Discriminate between two possiable curves: clockwise and counterclockwise.
                Center = Type.IsCW()
                    ? new Coordinate(midpointX + (b * aY) / a, midpointY - (b * aX) / a)
                    : new Coordinate(midpointX - (b * aY) / a, midpointY + (b * aX) / a);

                StartAngle = Type.IsCW()
                    ? Math.Atan2(Center.Y - StartCoordinate.Y, Center.X - StartCoordinate.X) + Math.PI / 2
                    : Math.Atan2(Center.Y - StartCoordinate.Y, Center.X - StartCoordinate.X) - Math.PI / 2;
            }

            EndAngle = StartAngle + Type.GetSignedAngle();
        }

        public Coordinate Position2Coordinate(double position)
        {
            if (position < StartPosition|| position > EndPosition)
                throw new ArgumentOutOfRangeException(nameof(position));
            if (position == StartPosition)
                return new(StartCoordinate.X, StartCoordinate.Y);
            if (position == EndPosition)
                return new(EndCoordinate.X, EndCoordinate.Y);

            double t = (position - StartPosition) / Length;
            if (Type == TrackSegmentType.Straight)
                return new Coordinate(
                    StartCoordinate.X + t * (EndCoordinate.X - StartCoordinate.X),
                    StartCoordinate.Y + t * (EndCoordinate.Y - StartCoordinate.Y)
                );
            else
            {
                double angle = (Type.IsCW() ? StartAngle + Math.PI / 2: StartAngle - Math.PI / 2) + t * Type.GetSignedAngle();
                return new Coordinate(
                    Center.X + Radius * Math.Cos(angle),
                    Center.Y + Radius * Math.Sin(angle)
                );
            }
        }
    }
}
