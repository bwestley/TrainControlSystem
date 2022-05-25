using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TrainControlSystem
{

    [Flags]
    public enum DebugFlags : int
    {
        None = 0b000,
        ColoredTrackSegments = 0b001,
        DrawCenter = 0b010,
        ColoredTrainSegments = 0b100
    }

    public class Simulation : ITrainSystem
    {
        public List<Train> Trains;
        public Track Track_;

        public Simulation(List<Train> trains, Track track)
        {
            Trains = trains;
            Track_ = track;
        }

        public void Render(Canvas canvas, double scale, Coordinate origin)
        {
            Render(canvas, scale, origin, DebugFlags.None);
        }

        public void Render(Canvas canvas, double scale, Coordinate origin, DebugFlags debugFlags)
        {
            List<Brush> brushes = new()
            {
                new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                new SolidColorBrush(Color.FromRgb(255, 255, 0)),
                new SolidColorBrush(Color.FromRgb(0, 255, 0)),
                new SolidColorBrush(Color.FromRgb(0, 255, 255)),
                new SolidColorBrush(Color.FromRgb(0, 0, 255)),
                new SolidColorBrush(Color.FromRgb(255, 0, 255)),
                new SolidColorBrush(Color.FromRgb(127, 0, 0)),
                new SolidColorBrush(Color.FromRgb(127, 127, 0)),
                new SolidColorBrush(Color.FromRgb(0, 127, 0)),
                new SolidColorBrush(Color.FromRgb(0, 127, 127)),
                new SolidColorBrush(Color.FromRgb(0, 0, 127)),
                new SolidColorBrush(Color.FromRgb(127, 0, 127))
            };
            foreach (SolidColorBrush brush in brushes)
                brush.Opacity = 0.5;
            int i = 0;
            foreach (TrackSegment trackSegment in Track_.TrackSegments)
            {
                Debug.WriteLine($"Segment {i}:\n" +
                    $"\tType: {trackSegment.Type}\n" +
                    $"\tStartPosition: {trackSegment.StartPosition}\n" +
                    $"\tEndPosition: {trackSegment.EndPosition}\n" +
                    $"\tStartCoordinate: ({trackSegment.StartCoordinate.X}, {trackSegment.StartCoordinate.Y})\n" +
                    $"\tEndCoordinate: ({trackSegment.EndCoordinate.X}, {trackSegment.EndCoordinate.Y})\n" +
                    $"\tCenter: ({trackSegment.Center.X}, {trackSegment.Center.Y})\n" +
                    $"\tLength: {trackSegment.Length}\n" +
                    $"\tDistance: {trackSegment.Distance}\n" +
                    $"\tRadius: {trackSegment.Radius}\n" +
                    $"\tStartAngle: {trackSegment.StartAngle}\n" +
                    $"\tEndAngle: {trackSegment.EndAngle}");

                switch (trackSegment.Type)
                {
                    case TrackSegmentType.Straight:
                        Line line = new()
                        {
                            X1 = trackSegment.StartCoordinate.X * scale + origin.X,
                            Y1 = trackSegment.StartCoordinate.Y * -scale + origin.Y,
                            X2 = trackSegment.EndCoordinate.X * scale + origin.X,
                            Y2 = trackSegment.EndCoordinate.Y * -scale + origin.Y,
                            Stroke = (debugFlags & DebugFlags.ColoredTrackSegments)
                            == DebugFlags.ColoredTrackSegments ? brushes[i%brushes.Count] : Brushes.Gray,
                            StrokeThickness = scale / 10
                        };
                        canvas.Children.Add(line);
                        break;
                    case TrackSegmentType.CWEighthCurve:
                    case TrackSegmentType.CCWEighthCurve:
                    case TrackSegmentType.CWQuarterCurve:
                    case TrackSegmentType.CCWQuarterCurve:
                    case TrackSegmentType.CWHalfCurve:
                    case TrackSegmentType.CCWHalfCurve:
                        Path path = new()
                        {
                            Stroke = (debugFlags & DebugFlags.ColoredTrackSegments)
                            == DebugFlags.ColoredTrackSegments ? brushes[i%brushes.Count] : Brushes.Gray,
                            StrokeThickness = scale / 10
                        };
                        PathGeometry pathGeometry = new();
                        PathFigure pathFigure = new(new Point(
                            trackSegment.StartCoordinate.X * scale + origin.X,
                            trackSegment.StartCoordinate.Y * -scale + origin.Y),
                            new List<PathSegment>(),
                            false);
                        ArcSegment arcSegment = new(
                            new Point(trackSegment.EndCoordinate.X * scale + origin.X,
                                trackSegment.EndCoordinate.Y * -scale + origin.Y),
                            new Size(trackSegment.Radius * scale, trackSegment.Radius * scale),
                            trackSegment.Type.GetAngle() * 180 / Math.PI,
                            false,
                            trackSegment.Type.IsCW() ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                            true);
                        pathFigure.Segments.Add(arcSegment);
                        pathGeometry.Figures.Add(pathFigure);
                        path.Data = pathGeometry;
                        canvas.Children.Add(path);
                        break;
                    default:
                        throw new ArgumentException("Invalid TrackSegmentType.");
                }
                if ((debugFlags & DebugFlags.DrawCenter) == DebugFlags.DrawCenter)
                {
                    Ellipse centerMarker = new()
                    {
                        Fill = brushes[i%brushes.Count],
                        StrokeThickness = scale / 50,
                        Width = 20 - 2 * i,
                        Height = 20 - 2 * i
                    };
                    Canvas.SetLeft(centerMarker, trackSegment.Center.X * scale + origin.X - centerMarker.Width / 2);
                    Canvas.SetTop(centerMarker, trackSegment.Center.Y * -scale + origin.Y - centerMarker.Height / 2);
                    canvas.Children.Add(centerMarker);
                }

                i++;
            }

            foreach (Train train in Trains)
            {
                i = 0;
                foreach ((Coordinate, Coordinate) car in train.CarCoordinates)
                {
                    Line line = new()
                    {
                        X1 = car.Item1.X * scale + origin.X,
                        Y1 = car.Item1.Y * -scale + origin.Y,
                        X2 = car.Item2.X * scale + origin.X,
                        Y2 = car.Item2.Y * -scale + origin.Y,
                        Stroke = (debugFlags & DebugFlags.ColoredTrainSegments)
                            == DebugFlags.ColoredTrainSegments ? brushes[i%brushes.Count] : Brushes.Black,
                        StrokeThickness = scale / 5
                    };
                    canvas.Children.Add(line);
                    i++;
                }
            }

            foreach (Switch sw in Track_.Switches)
            {
                Ellipse switchMarker = new()
                {
                    Fill = sw.Right ? (sw.IsRightDiverging ? Brushes.Yellow : Brushes.Lime) : (sw.IsLeftDiverging ? Brushes.Yellow : Brushes.Green),
                    StrokeThickness = 0,
                    Width = 0.1 * scale,
                    Height = 0.1 * scale
                };
                Canvas.SetLeft(switchMarker, sw.Coordinate_.X * scale + origin.X - switchMarker.Width / 2);
                Canvas.SetTop(switchMarker, sw.Coordinate_.Y * -scale + origin.Y - switchMarker.Height / 2);
                canvas.Children.Add(switchMarker);
            }
        }

        internal void Update()
        {
            foreach (Train train in Trains)
            {
                train.Update();
            }
        }

        public int GetDetectorState(int detectorID)
        {
            throw new NotImplementedException();
        }

        public void SetSwitchPosition(int switchID, bool open)
        {
            throw new NotImplementedException();
        }

        public void SetTrainVelocity(int trainID, double velocity)
        {
            throw new NotImplementedException();
        }

        public void SetTrainVelocity(int trainID, float velocity)
        {
            throw new NotImplementedException();
        }
    }
}
