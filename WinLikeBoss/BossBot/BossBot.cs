using System;
using System.Drawing;
using Robocode;

namespace BossBot
{
    public class BossBot : AdvancedRobot
    {
        private BotMode currentMode = BotMode.Scanning;
        private BotStatus currentFoe;
        private long lastFoeScanTime;
        private double movingModifier = 1;
        private double radarRotationModifier = 1;
        private bool isReachingRightWall = false;
        private const double VelocityCoefficient = 100;
        private const double FirePower = 1d;
        private const double AngleShootingError = 5;
        private const long TargetLostTime = 10;
        private const int FullScanDurationTurns = 10;
        private const double PreferedRadius = 200;

        public double RealHeading
        {
            get
            {
                var heading = Math.Abs(movingModifier - 1) < Double.Epsilon ? Heading : Heading + 180;
                return heading > 360 ? heading - 360 : heading;
            }
        }

        public override void Run()
        {
            Initialize();

            SetTurnRadarLeft(double.MaxValue);

            var turnsOnHold = 0;

            while (true)
            {
                turnsOnHold = currentMode == BotMode.Scanning ? turnsOnHold + 1 : 0;
                if (turnsOnHold >= FullScanDurationTurns)
                    ReachRightWall();                    

                AvoidWallHit();

                SetMoveDirection();

                PointGun();

                Execute();
            }

        }

        private void PointGun()
        {
            if (currentFoe != null)
            {
                if (Time - lastFoeScanTime > TargetLostTime)
                {
                    currentMode = BotMode.Scanning;
                }
                else
                {
                    var shotAngle = MathHelper.NormalizeAngleDegrees(Heading + currentFoe.Bearing - GunHeading);

                    var foeBearing = MathHelper.NormalizeAngleDegrees(currentFoe.Bearing + currentFoe.Heading - Heading);
                    if (foeBearing > 180)
                        foeBearing = foeBearing - 360;

                    var biasOwn =
                        Math.Cos(
                            MathHelper.DegreesToRadians(currentFoe.Bearing > 0
                                ? currentFoe.Bearing - 90
                                : currentFoe.Bearing + 90))
                        * Velocity;
                    var biasFoe =
                        Math.Cos(MathHelper.DegreesToRadians(foeBearing > 0 ? foeBearing - 90 : foeBearing + 90))
                        * currentFoe.Velocity;
                    var angleAdjustment = MathHelper.RadiansToDegrees(Math.Atan2(biasFoe + biasOwn, currentFoe.Distance)) /
                                          VelocityCoefficient * currentFoe.Distance;
                    shotAngle += angleAdjustment;

                    //Out.WriteLine("################");
                    //Out.WriteLine("BiasOwn {0}", biasOwn);
                    //Out.WriteLine("BiasFoe {0}", biasFoe);
                    //Out.WriteLine("angleAdjustment {0}", angleAdjustment);
                    //Out.WriteLine("################");

                    SetTurnGunRight(shotAngle);

                    //Out.WriteLine("-----------------------");
                    //Out.WriteLine("Heading    {0}", Heading);
                    //Out.WriteLine("Bearing    {0}", currentFoe.Bearing);
                    //Out.WriteLine("GunHeading {0}", GunHeading);
                    //Out.WriteLine("ShotAngle  {0}", shotAngle);
                    //Out.WriteLine("-----------------------");

                    if (Math.Abs(shotAngle) < AngleShootingError)
                        Fire(FirePower);
                }
            }
        }

        private void ReachRightWall()
        {
            if (Heading < 90)
                TurnRight(90 - Heading);
            else
                TurnLeft(Heading - 90);
            Ahead(BattleFieldWidth - X - Width / 2);
        }

        private void SetMoveDirection()
        {

            SetAhead(100 * movingModifier);
            SetTurnLeft(1000);
        }

        private void AvoidWallHit()
        {
            var realHeading = RealHeading;
            if ((X <= Width && realHeading > 180) ||
                (BattleFieldHeight - Y <= Height && (realHeading < 90 || realHeading > 270)) ||
                (BattleFieldWidth - X <= Width && realHeading < 180) ||
                (Y <= Height && (realHeading > 90 && realHeading < 270)))
            {
                SetAhead(0);
                ChangeDirection();
            }

        }

        public override void OnHitWall(HitWallEvent evnt)
        {
            ChangeDirection();
        }

        public override void OnHitRobot(HitRobotEvent evnt)
        {
            ChangeDirection();
        }

        public override void OnScannedRobot(ScannedRobotEvent evnt)
        {
            if (currentMode == BotMode.Scanning)
            {
                if (IsFoe(evnt.Name))
                {
                    currentMode = BotMode.Locked;
                    currentFoe = new BotStatus
                    {
                        Name = evnt.Name,
                        Heading = evnt.Heading,
                        Velocity = evnt.Velocity,
                        Distance = evnt.Distance,
                        Bearing = evnt.Bearing
                    };
                    lastFoeScanTime = Time;
                }
            }
            if (currentMode == BotMode.Locked)
            {
                if (evnt.Name == currentFoe.Name)
                {
                    currentFoe = new BotStatus
                    {
                        Name = evnt.Name,
                        Heading = evnt.Heading,
                        Velocity = evnt.Velocity,
                        Distance = evnt.Distance,
                        Bearing = evnt.Bearing
                    };
                    lastFoeScanTime = Time;
                }
            }
        }

        private void ChangeDirection()
        {
            movingModifier *= -1;
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

    public class BotStatus
    {
        public string Name { get; set; }
        public double Heading { get; set; }
        public double Velocity { get; set; }
        public double Distance { get; set; }
        public double Bearing { get; set; }
    }

    public enum BotMode
    {
        Scanning,
        Locked
    }
}
