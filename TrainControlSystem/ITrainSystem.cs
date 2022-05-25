using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainControlSystem
{
    internal interface ITrainSystem
    {
        void SetTrainVelocity(int trainID, float velocity);
        void SetSwitchPosition(int switchID, bool open);
        int GetDetectorState(int detectorID);
    }
}
