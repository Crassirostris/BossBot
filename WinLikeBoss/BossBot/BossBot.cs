using System;
using System.Drawing;
using Robocode;
using Robocode.Util;

namespace BossBot
{
    public class BossBot : AdvancedRobot
    {
        private double moveAmount;
        private double direction = 1;
        private bool peek;
        private bool directionChanged;

        public override void Run()
        {
            Initialize();

            //SetTurnRadarLeft(double.MaxValue);

            moveAmount = Math.Max(BattleFieldHeight, BattleFieldWidth);
            peek = false;

            TurnLeft(Heading % 90);
            Ahead(moveAmount);

            peek = true;
            TurnGunRight(90);
            TurnRight(90);

            while (true)
            {
                if (directionChanged)
                {
                    direction *= -1;
                    directionChanged = false;
                }
                peek = true;
                Ahead(moveAmount * direction);
                peek = false;
                TurnRight(90 * direction);
            }
        }

        public override void OnHitRobot(HitRobotEvent evnt)
        {
            directionChanged = true;
        }

        public override void OnScannedRobot(ScannedRobotEvent evnt)
        {
            if (IsFoe(evnt.Name))
                Fire(2);
        }

        private bool IsFoe(string name)
        {
            return !name.ToLower().Contains("observer");
        }

        private void Initialize()
        {
            SetColors(Color.Black, Color.DarkRed, Color.Red, Color.DeepPink, Color.Pink);

            IsAdjustGunForRobotTurn = false;
            IsAdjustRadarForGunTurn = false;
            IsAdjustRadarForRobotTurn = false;
        }
    }
}
