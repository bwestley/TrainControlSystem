using System.Collections.Generic;
using System.Diagnostics;

namespace TrainControlSystem
{
    public class Train
    {
        public Track Track_;
        public double Position; // (m)
        public bool Forwards; // True if the train's position will increase when velocity is positive.
        public List<TrackSegment> Occupying; // Train segments the train is on. The front is on [^1] and the end is on [0]. Necessary because track segments may intersect.
        public double Length; // (m)
        public int Cars; // Total number of cars.
        public List<(Coordinate, Coordinate)> CarCoordinates; // Start and end coordinates of each train car.
        public double Velocity; // (m/s)
        public double Acceleration; // (m/s2)
        public double MaxAcceleration; // (m/s2)

        public Train(Track track, double position, bool forwards, double length, int cars, double maxAcceleration)
        {
            Track_= track;
            Position = position;
            Forwards = forwards;
            Occupying = new();
            Length = length;
            Cars = cars;
            CarCoordinates = new();
            Velocity = 0;
            Acceleration = 0;
            MaxAcceleration = maxAcceleration;

            Update();
        }

        public void Update()
        {
            // Determine the track segments that are occupied by the train from head to tail.
            Occupying.Clear();
            CarCoordinates.Clear();
            double position = Position;
            bool forwards = Forwards;
            double length = Length;
            int carsLeft = Cars - 1;
            Coordinate carStart = ( // The start of the current car.
                Track_.Position2Coordinate(position) ?? throw new System.Exception($"Train position {position} out of range.")
            );
            TrackSegment trackSegment = Track_.Position2TrackSegment(position) ?? throw new System.Exception($"Train position {position} out of range.");
            while (true)
            {
                Debug.WriteLine($"trackSegment.ID: {trackSegment.ID}, position: {position}, forwards: {forwards}, length: {length}, carsLeft: {carsLeft}, carStart: ({carStart.X}, {carStart.Y})");
                Occupying.Add(trackSegment);

                // `forwards` is true if the train travels from the start to the end of `trackSegment` and false otherwise.
                // {END} is the start of `trackSegment` if `forwards` is true and the end of `trackSegment` otherwise.
                length -= forwards ? (position - trackSegment.StartPosition) : (trackSegment.EndPosition - position); // `length` is now the length of the train from {END} to its tail.
                while (length <= Length / Cars * carsLeft && carsLeft >= 0) // The end of the current car is on `trackSegment`.
                {
                    double distanceToCarEnd = Length / Cars * carsLeft - length; // Distance between {END} and the end of the current car.
                    double carEndPosition = forwards ? (trackSegment.StartPosition + distanceToCarEnd) : (trackSegment.EndPosition - distanceToCarEnd);
                    Coordinate carEnd = trackSegment.Position2Coordinate(carEndPosition);
                    CarCoordinates.Add(new(carStart, carEnd));
                    carStart = carEnd.Copy();
                    carsLeft -= 1;
                }
                if (length < 0) break; // The train ends on `trackSegment`.
                Switch? attachedSwitch = Track_.GetAttachedSwitch(trackSegment, forwards); // `forwards` = {END}.isStart
                if (attachedSwitch == null) // The end of `trackSegment` does not attach to a switch.
                {
                    TrackSegment? attachedTrackSegment = Track_.GetAttachedTrackSegment(trackSegment, forwards); // `!forwards` = {END}.isStart
                    if (attachedTrackSegment == null) // {END} does not attach to anything.
                        throw new System.Exception($"Train tail exceeds a track segment's unattached end.");

                    // The end of `trackSegment` attaches to a another track segment.
                    position = forwards ? attachedTrackSegment.EndPosition - 0.0000001 : attachedTrackSegment.StartPosition; // Track segment end that attaches to {END}.
                    trackSegment = attachedTrackSegment;
                    if (trackSegment != Track_.Position2TrackSegment(position)) // `position` and `trackSegment` do not match.
                        throw new System.Exception($"Discrepency between updated position and trackSegment.");
                }
                else // The end of `trackSegment` attaches to a switch.
                {
                    if (attachedSwitch.DirectionChanges(trackSegment))
                        forwards = !forwards; // The train will travel in the "opposite" direction on the next track segment.
                    position = attachedSwitch.DirectPosition(trackSegment);
                    trackSegment = attachedSwitch.DirectTrackSegment(trackSegment);
                    if (trackSegment != Track_.Position2TrackSegment(position)) // `position` and `trackSegment` do not match.
                        throw new System.Exception($"Discrepency between updated position and trackSegment.");
                }
            }
        }
    }
}
