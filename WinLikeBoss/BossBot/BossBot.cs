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
        private bool isTurning;
        private bool isMoving;
        private double radarTurnRate = 100500;
        private const long MaximumAllowedTimeOnHold = 73;
        private const long LostTargetTimeout = 42;
        private const double TolerableFiringErrorDegrees = 3;

        private BotInfo currentFoe;

        public override void Run()
        {
            Initialize();

            SetTurnRadarLeft(radarTurnRate);

            moveAmount = Math.Max(BattleFieldHeight, BattleFieldWidth);

            TurnLeft(Heading % 90);
            Ahead(GetDistanceToWall());

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

                Move();

                DoScan();

                DoFiring();

                Execute();
            }
        }

        private void DoFiring()
        {
            if (currentFoe == null)
                return;

            var angle = Utils.NormalRelativeAngleDegrees(Heading + currentFoe.Bearing - GunHeading);

            SetTurnGunRight(angle);
            if (angle < TolerableFiringErrorDegrees)
            {
                var power = 1 / (currentFoe.Distance / (Math.Max(BattleFieldWidth, BattleFieldHeight) - 42) * 2.7);
                if (power > 3)
                    power = 3;
                if (power < 0.3)
                    power = 0.3;
                Out.WriteLine("Power: {0}", power);
                Fire(power);
            }

        }

        private void DoScan()
        {
            if (currentFoe == null)
            {
                SetTurnRadarLeft(radarTurnRate);
                return;
            }

            Out.WriteLine("Scanning foe {0}", currentFoe.Name);

            var foeHeading = Utils.NormalAbsoluteAngleDegrees(Heading + currentFoe.Bearing);
            var angle = Utils.NormalRelativeAngleDegrees(RadarHeading - foeHeading);
            SetTurnRadarLeft(radarTurnRate * (angle > 0 ? 1 : -1));
        }

        private void Move()
        {
            if (!isTurning && !isMoving)
            {
                isMoving = true;

                SetAhead(GetDistanceToWall());
            }
            else if (!isTurning)
            {
                if (Math.Abs(DistanceRemaining) < 3)
                {
                    isMoving = false;
                    isTurning = true;
                    SetTurnRight(90 * direction);
                }
            }
            else if (Math.Abs(TurnRemaining) < 3)
            {
                isTurning = false;
            }
        }

        private double GetDistanceToWall()
        {
            if (Heading <= 2 || Heading >= 358)
                return BattleFieldHeight - Y - Height / 2;
            if (Heading >= 88 && Heading <= 92)
                return BattleFieldWidth - X - Width / 2;
            if (Heading >= 178 && Heading <= 182)
                return Y - Height / 2;
            return X - Width / 2;
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

        public override void OnBulletHit(BulletHitEvent evnt)
        {
            if (currentFoe != null && evnt.VictimEnergy <= 0 && evnt.VictimName == currentFoe.Name)
                currentFoe = null;
        }

        public override void OnScannedRobot(ScannedRobotEvent evnt)
        {
            if (IsFoe(evnt.Name))
            {
                lastTimeScannedFoe = Time;
                if (currentFoe == null || currentFoe.Name == evnt.Name || Time - lastTimeScannedFoe > LostTargetTimeout)
                {
                    currentFoe = new BotInfo
                    {
                        Name = evnt.Name,
                        Bearing = evnt.Bearing,
                        Heading = evnt.Heading,
                        Distance = evnt.Distance,
                        Velocity = evnt.Velocity
                    };
                }
            }
        }

        private bool IsFoe(string name)
        {
            return !name.ToLower().Contains("observer");
        }

        private void Initialize()
        {
            SetColors(Color.Black, Color.DarkRed, Color.Red, Color.DeepPink, Color.Pink);

            IsAdjustGunForRobotTurn = true;
            IsAdjustRadarForGunTurn = true;
            IsAdjustRadarForRobotTurn = true;
        }
    }

    internal class BotInfo
    {
        public string Name { get; set; }
        public double Heading { get; set; }
        public double Bearing { get; set; }
        public double Distance { get; set; }
        public double Velocity { get; set; }
    }
}
