namespace TrainControlSystem
{
    public class Switch
    {
        public readonly int ID;
        readonly TrackSegment SourceTrack; // TrackSegment on the narrow end of the switch
        readonly bool SourceIsStart; // true if the start and false if the end of SourceTrack connects to the switch
        readonly TrackSegment RightTrack; // TrackSegment encountered when turning right from SourceTrack
        readonly bool RightIsStart; // true if the start and false if the end of RightTrack connects to the switch
        readonly TrackSegment LeftTrack; // TrackSegment encountered when turning left from SourceTrack
        readonly bool LeftIsStart; // true if the start and false if the end of LeftTrack connects to the switch
        public bool Right; // true if the train will turn right when traveling from the narrow to the wide end of the switch
        public readonly bool IsRightDiverging; // true if the right track is curved, false otherwise
        public readonly bool IsLeftDiverging; // true if the left track is curved, false otherwise
        public readonly Coordinate Coordinate_;

        public Switch(int id, TrackSegment sourceTrack, bool sourceIsStart, TrackSegment rightTrack, bool rightIsStart, TrackSegment leftTrack, bool leftIsStart, bool right, bool isRightDiverging)
        {
            ID = id;
            SourceTrack = sourceTrack;
            SourceIsStart = sourceIsStart;
            RightTrack = rightTrack;
            RightIsStart = rightIsStart;
            LeftTrack = leftTrack;
            LeftIsStart = leftIsStart;
            Right = right;

            IsRightDiverging = isRightDiverging;
            IsLeftDiverging = !isRightDiverging;
            Coordinate_ = SourceIsStart ? SourceTrack.StartCoordinate.Copy() : SourceTrack.EndCoordinate.Copy();
        }

        // Return the TrackSegment that a train will end up on when traveling from source.
        public TrackSegment DirectTrackSegment(TrackSegment source)
        {
            return source == SourceTrack ? (Right ? RightTrack : LeftTrack) : SourceTrack;
        }

        // Return the position that a train will end up at when traveling from source.
        public double DirectPosition(TrackSegment source)
        {
            if (source == RightTrack || source == LeftTrack)
                return SourceIsStart ? SourceTrack.StartPosition : SourceTrack.EndPosition - 0.0000001;
            if (Right)
                return RightIsStart ? RightTrack.StartPosition : RightTrack.EndPosition - 0.0000001;
            return LeftIsStart ? LeftTrack.StartPosition : LeftTrack.EndPosition - 0.0000001;
        }

        // Check if the direction changes when traveling from source to the track segment returned from DirectPosition
        public bool DirectionChanges(TrackSegment source)
        {
            // Direction changes if the source and destination tracks attach to the switch from the same end.
            if (source == RightTrack) // source: RightTrack, destination: SourceTrack
                return RightIsStart == SourceIsStart;
            if (source == LeftTrack) // source: LeftTrack, destination: SourceTrack
                return LeftIsStart == SourceIsStart;
            if (Right) // source: SourceTrack, destination: RightTrack
                return SourceIsStart == RightIsStart;
            // source: SourceTrack, destination: LeftTrack
            return SourceIsStart == LeftIsStart;
        }

        // Check if the provided track segment is attached to this switch at the track segment's end or start.
        public bool IsAttached(TrackSegment trackSegment, bool isStart)
        {
            if (trackSegment == SourceTrack)
                return isStart == SourceIsStart;
            if (trackSegment == RightTrack)
                return isStart == RightIsStart;
            if (trackSegment == LeftTrack)
                return isStart == LeftIsStart;
            return false;
        }
    }
}
