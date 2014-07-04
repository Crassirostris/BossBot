using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Robocode;
using Robocode.Util;

namespace BossBot
{
    public class BossBot : AdvancedRobot
    {
        private double direction = 1;
        private long lastTimeScannedCurrentFoe;
        private bool isTurning;
        private bool isMoving;
        private const long LostTargetTime = 10;
        private const double TolerableFiringErrorDegrees = 3;
        private bool targetScanned;
        private double scanDirection = 1;

        private int GetPriority(string name)
        {
            if (name.ToLower().Contains("track"))
                return 2;
            if (name.ToLower().Contains("spin"))
                return 1;
            return 0;
        }

        private BotInfo currentFoe;
        private bool nooneToShoot;
        private readonly Dictionary<string, BotInfo> foes = new Dictionary<string, BotInfo>();
        private const int MaxTimeOnHoldAllowed = 30;

        public override void Run()
        {
            Initialize();

            InitialMovements();

            nooneToShoot = false;

            while (true)
            {
                if (nooneToShoot)
                    ReachRightWall();

                Move();

                DoScan();

                DoFiring();

                Execute();
            }
        }

        private void InitialMovements()
        {
            TurnLeft(Heading % 90);
            Ahead(GetDistanceToWall());

            TurnGunRight(90);
            TurnRight(90);

            TurnRadarLeft(360);

        }

        private void DoFiring()
        {
            if (currentFoe == null)
                return;

            var angle = Utils.NormalRelativeAngleDegrees(Heading + currentFoe.Bearing - GunHeading);

            SetTurnGunRight(angle);
            if (angle < TolerableFiringErrorDegrees)
            {
                var power = 1 / (currentFoe.Distance / (Math.Max(BattleFieldWidth, BattleFieldHeight) / 2 - 42) * 2);
                if (power > 0.3)
                {
                    if (power > 3)
                        power = 3;
                    Out.WriteLine("Power: {0}", power);
                    Fire(power);
                }
            }

        }

        private void DoScan()
        {
            if (currentFoe == null)
            {
                if (Time - lastTimeScannedCurrentFoe > MaxTimeOnHoldAllowed)
                    nooneToShoot = true;
                SetTurnRadarLeft(double.MaxValue);
                return;
            }

            var foeHeading = Utils.NormalAbsoluteAngleDegrees(Heading + currentFoe.Bearing);
            var angle = Utils.NormalRelativeAngleDegrees(RadarHeading - foeHeading);
            SetTurnRadarLeft(double.MaxValue * (angle > 0 ? 1 : -1));

            Out.WriteLine("Scanning foe {0}", currentFoe.Name);
            Out.WriteLine("Foe heading  {0}", foeHeading);
            Out.WriteLine("Angle        {0}", angle);
        }

        private void Move()
        {
            ChangeDirection();

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

        private void ChangeDirection()
        {
            direction *= -1;
        }


        private double GetDistanceToWall()
        {
            var trackDeltas = foes.Keys
                .Where(foeName => foeName.ToLower().Contains("track"))
                .Select(name => new DeltaInfo(foes[name], this))
                .ToList();

            Out.WriteLine(trackDeltas.Count);

            if (Heading <= 2 || Heading >= 358)
            {
                if (trackDeltas.Any(delta => X >= delta.MinX && X <= delta.MaxX && Y <= delta.MinY))
                {
                    Out.WriteLine("ololo");
                    return Y - Height/2;
                }
                if (X >= Width + 2)
                    return BattleFieldHeight - Y - Height/2;
                return BattleFieldHeight - Y - 3*Height/2;
            }
            if (Heading >= 88 && Heading <= 92)
            {
                if (trackDeltas.Any(delta => Y >= delta.MinY && Y <= delta.MaxY && X <= delta.MinX))
                {
                    {
                        Out.WriteLine("ololo");
                    }
                    if (Y >= BattleFieldHeight - Height - 2)
                        return X - 3 * Width / 2;
                    return X - Width / 2;
                }
                return BattleFieldWidth - X - Width/2;
            }
            if (Heading >= 178 && Heading <= 182)
            {
                if (trackDeltas.Any(delta => X >= delta.MinX && X <= delta.MaxX && Y >= delta.MaxY))
                {
                    {
                        Out.WriteLine("ololo");
                    }
                    if (X >= Width + 2)
                        return BattleFieldHeight - Y - Height/2;
                    return BattleFieldHeight - Y - 3*Height/2;
                }
                return Y - Height/2;
            }
            if (trackDeltas.Any(delta => Y >= delta.MinY && Y <= delta.MaxY && X >= delta.MaxX))
            {
                Out.WriteLine("ololo");
                return BattleFieldWidth - X - Width/2;
            }
            if (Y >= BattleFieldHeight - Height - 2)
                return X - 3*Width/2;
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
            ChangeDirection();
        }

        public override void OnHitByBullet(HitByBulletEvent evnt)
        {
            isMoving = false;
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
                var info = new BotInfo {
                        Name = evnt.Name,
                        Bearing = evnt.Bearing,
                        Heading = evnt.Heading,
                        Distance = evnt.Distance,
                        Velocity = evnt.Velocity
                    };
                foes[evnt.Name] = info;
                if (currentFoe == null 
                    || currentFoe.Name == evnt.Name
                    || GetPriority(currentFoe.Name) < GetPriority(evnt.Name)
                    || Time - lastTimeScannedCurrentFoe > LostTargetTime
                    || (GetPriority(currentFoe.Name) == GetPriority(evnt.Name) && currentFoe.Distance > evnt.Distance))
                {
                    lastTimeScannedCurrentFoe = Time;
                    currentFoe = foes[evnt.Name];
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

    internal class DeltaInfo
    {
        public DeltaInfo(BotInfo botInfo, BossBot bossBot)
        {
            var X = Math.Sin(botInfo.Bearing + bossBot.Heading) * botInfo.Distance;
            MinX = X - bossBot.Width/2;
            MaxX = X + bossBot.Width/2;
            var Y = Math.Cos(botInfo.Bearing + bossBot.Heading) * botInfo.Distance;
            MinY = Y - bossBot.Height/2;
            MaxY = Y + bossBot.Height/2;
        }

        public double MaxY { get; set; }

        public double MinY { get; set; }

        public double MaxX { get; set; }

        public double MinX { get; set; }
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
