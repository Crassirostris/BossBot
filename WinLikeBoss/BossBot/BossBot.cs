using System;
using System.Drawing;
using System.Threading;
using Robocode;
using Robocode.Util;

namespace BossBot
{
    public class BossBot : AdvancedRobot
    {
        private double moveAmount;
        private double direction = 1;
        private bool directionChanged;
        private long lastTimeScannedFoe;
        private const long MaximumAllowedTimeOnHold = 300;

        public override void Run()
        {
            Initialize();

            moveAmount = Math.Max(BattleFieldHeight, BattleFieldWidth);

            TurnLeft(Heading % 90);
            Ahead(moveAmount);

            TurnGunRight(90);
            TurnRight(90);

            while (true)
            {
                if (Time - lastTimeScannedFoe > MaximumAllowedTimeOnHold)
                    ReachRightWall();

                if (directionChanged)
                {
                    direction *= -1;
                    directionChanged = false;
                }
                Out.WriteLine("Moving by {0}", moveAmount * direction);
                Ahead(moveAmount * direction);
                Out.WriteLine("Turning by {0} to the right", 90 * direction);
                TurnRight(90 * direction);
            }
        }

        private void ReachRightWall()
        {
            Out.WriteLine("My work is done here, going to visit right wall");
            if (Heading < 90)
                TurnRight(90 - Heading);
            else
                TurnLeft(Heading - 90);
            Ahead(BattleFieldWidth - X - Width / 2);
        }

        public override void OnHitRobot(HitRobotEvent evnt)
        {
            directionChanged = true;
        }

        public override void OnScannedRobot(ScannedRobotEvent evnt)
        {
            if (IsFoe(evnt.Name))
            {
                Fire(2);
                lastTimeScannedFoe = Time;
            }
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
