using System;
using System.Drawing;
using Robocode;
using Robocode.Util;

namespace BossBot
{
    public class BossBot : AdvancedRobot
    {
        private double direction = 1;
        private bool peek;
        private bool directionChanged;

        public override void Run()
        {
            Initialize();

            //SetTurnRadarLeft(double.MaxValue);

            peek = false;

            TurnLeft(Heading % 90);
            MoveToWall();
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
                MoveToWall();
                peek = false;
                TurnRight(90 * direction);
            }
        }

        private void MoveToWall()
        {
            if (Heading <= 2 || Heading >= 358)
                Ahead(BattleFieldHeight - Y - Height/2);
            else if (Heading >= 88 && Heading <= 92)
                Ahead(BattleFieldWidth - X - Width/2);
            else if (Heading >= 178 && Heading <= 182)
                Ahead(Y - Height/2);
            else
                Ahead(X - Width/2);
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
