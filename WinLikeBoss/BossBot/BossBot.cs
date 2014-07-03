using System.Drawing;
using Robocode;

namespace BossBot
{
    public class BossBot : AdvancedRobot
    {
        public override void Run()
        {
            Initialize();

            while (true)
            {
                //Logic here
            }
        }

        private void Initialize()
        {
            SetColors(Color.Black, Color.DarkRed, Color.Red, Color.DeepPink, Color.Pink);
            IsAdjustGunForRobotTurn = true;
            IsAdjustRadarForGunTurn = true;
            IsAdjustRadarForRobotTurn = true;
        }
    }
}
