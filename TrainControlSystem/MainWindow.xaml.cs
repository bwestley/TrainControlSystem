using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TrainControlSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Simulation Simulation_;
        public MainWindow()
        {
            InitializeComponent();
            Simulation_ = InitializeSimulation();
        }

        private Simulation InitializeSimulation()
        {
            double sqrt2 = Math.Sqrt(2);
            double sin45 = sqrt2 / 2;

            Track track = new();
            
            // startPosition, startCoordinate, startAngle, radius, type
            // startPosition, startCoordinate, endCoordinate, type
            track.AddSegment(0, new Coordinate(0, 0), 0, 0.5, TrackSegmentType.Straight); // 0
            track.AddSegment(null, null, null, 0.5, TrackSegmentType.CCWHalfCurve); // 1
            track.AddSegment(null, null, null, 0.5, TrackSegmentType.Straight); // 2
            track.AddSegment(null, null, null, 0.5, TrackSegmentType.CCWHalfCurve); // 3
            track.AddSegment(30, new Coordinate(0, 0), new Coordinate(1, -1), TrackSegmentType.Straight); // 4
            track.AddSegment(10, null, 0, 1.5, TrackSegmentType.CCWHalfCurve); // 5
            track.AddSegment(null, null, null, 0.5, TrackSegmentType.Straight); // 6
            track.AddSegment(null, null, null, 1.5, TrackSegmentType.CCWHalfCurve); // 7
            track.AddSegment(null, null, null, 0.5, TrackSegmentType.Straight); // 8
            track.AddSegment(40, new Coordinate(0, 2), new Coordinate(0, 1), TrackSegmentType.CWHalfCurve); // 9
            // sourceTrackID, sourceIsStart, rightTrackID, rightIsStart, leftTrackID, leftIsStart, right, isRightDiverging
            track.AddSwitch(7, true, 9, true, 6, false, false, true); // 0
            track.AddSwitch(3, true, 2, false, 9, false, false, false); // 1
            track.AddSwitch(3, false, 4, true, 0, true, false, true); // 2
            track.AddSwitch(5, true, 4, false, 8, false, false, true); // 3
            
            List<Train> trains = new() { new(track, 0, true, 5, 10, 10) };

            Simulation_ = new(trains, track);
            Simulation_.Render(MainCanvas, 190, new(500, 400), DebugFlags.None);

            return Simulation_;
        }
    }
}
