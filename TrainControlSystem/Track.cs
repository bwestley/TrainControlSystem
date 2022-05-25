using System.Collections.Generic;

namespace TrainControlSystem
{
    public class Track
    {
        // Track coordinate system (refered to as `position`):
        // * Each track segment has a start and end position.
        // * The difference between the start and end positions is equal
        //   to the track segment's length.
        // * The end position is greater than the start position.
        // * If a track segment's end position is equal to a track segment's
        //   start position than the two are assumed to be joined.
        // * The end position is not considered part of the track segment:
        //   trackSegment = {position | trackSegment.StartPosition <= position
        //   < trackSegment.EndPosition}

        public List<TrackSegment> TrackSegments;
        public List<Switch> Switches;
        int TrackSegmentID;
        int SwitchID;

        public Track()
        {
            TrackSegments = new();
            Switches = new();
            TrackSegmentID = 0;
            SwitchID = 0;
        }

        // Return the track segment that a position is on.
        public TrackSegment? Position2TrackSegment(double position)
        {
            foreach (TrackSegment trackSegment in TrackSegments)
                if (position >= trackSegment.StartPosition && position < trackSegment.EndPosition)
                    return trackSegment;
            return null;
        }

        // Return the cartesian coordinates at a track position.
        public Coordinate? Position2Coordinate(double position)
        {
            return Position2TrackSegment(position)?.Position2Coordinate(position);
        }

        // Return the switch attached to the specified end of a track segment.
        public Switch? GetAttachedSwitch(TrackSegment trackSegment, bool isStart)
        {
            foreach (Switch sw in Switches)
                if (sw.IsAttached(trackSegment, isStart))
                    return sw;
            return null;
        }

        // Return the track segment attached to the specified end of a track segment.
        public TrackSegment? GetAttachedTrackSegment(TrackSegment trackSegment, bool isStart)
        {
            if (isStart)
            {
                foreach (TrackSegment ts in TrackSegments)
                    if (trackSegment.StartPosition == ts.EndPosition)
                        return ts;
            }
            else
            {
                foreach (TrackSegment ts in TrackSegments)
                    if (trackSegment.EndPosition == ts.StartPosition)
                        return ts;
            }
            return null;
        }

        public void AddSegment(double? startPosition, Coordinate? startCoordinate,
            double? startAngle, double radius, TrackSegmentType type)
        {
            if (TrackSegments.Count == 0)
                TrackSegments.Add(new(
                    TrackSegmentID,
                    startPosition ?? 0,
                    startCoordinate ?? new(0, 0),
                    startAngle ?? 0, radius, type
                ));
            else
                TrackSegments.Add(new(
                    TrackSegmentID,
                    startPosition ?? TrackSegments[^1].EndPosition,
                    startCoordinate ?? TrackSegments[^1].EndCoordinate.Copy(),
                    startAngle ?? TrackSegments[^1].EndAngle,
                    radius,
                    type
                ));
            TrackSegmentID++;
        }

        public void AddSegment(double? startPosition, Coordinate? startCoordinate,
            Coordinate endCoordinate, TrackSegmentType type)
        {
            if (TrackSegments.Count == 0)
                TrackSegments.Add(new(
                    TrackSegmentID,
                    startPosition ?? 0,
                    startCoordinate ?? new(0, 0),
                    endCoordinate,
                    type
                ));
            else
                TrackSegments.Add(new(
                    TrackSegmentID,
                    startPosition ?? TrackSegments[^1].EndPosition,
                    startCoordinate ?? TrackSegments[^1].EndCoordinate.Copy(),
                    endCoordinate,
                    type
                ));
            TrackSegmentID++;
        }

        public void AddSwitch(int sourceTrackID, bool sourceIsStart, int rightTrackID, bool rightIsStart, int leftTrackID, bool leftIsStart, bool right, bool isRightDiverging)
        {
            Switches.Add(new(
                SwitchID,
                TrackSegments[sourceTrackID],
                sourceIsStart,
                TrackSegments[rightTrackID],
                rightIsStart,
                TrackSegments[leftTrackID],
                leftIsStart,
                right,
                isRightDiverging
            ));
            SwitchID++;
        }
    }
}
