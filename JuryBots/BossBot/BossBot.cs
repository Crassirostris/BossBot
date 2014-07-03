using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Robocode;

namespace BossBot
{
    class BotInfo
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Velocity { get; set; }
        public double Heading { get; set; }
        public double Distance { get; set; }
        public double Bearing { get; set; }
    }

    public class BossBot : AdvancedRobot
    {
        private const double RadarTurningRateDegrees = 45;
        private const double MaxStepLength = 10;
        private const double GunTurnRate = 13.67;
        private const double AngleToFire = 10;

        private readonly string[] foes = { "JuryBots.Evil" };
        private readonly Dictionary<string, BotInfo> botsInfo = new Dictionary<string, BotInfo>();
        private bool isRightWallReached = false;
        private bool isReadyToChangeAim = true;
        private string currentFoeName = null;
        private double firePower = 1d;

        public override void Run()
        {
            SetColors(Color.Black, Color.DarkRed, Color.Red, Color.DeepPink, Color.Pink);
            IsAdjustGunForRobotTurn = true;
            IsAdjustRadarForGunTurn = true;
            IsAdjustRadarForRobotTurn = true;

            while (true)
            {
                ControlRadar();
                ControlMovements();
                ControlGun();
            }
        }

        private double ClampAngleDegrees(double angle)
        {
            angle = angle % 360;
            if (angle > 180)
                angle = angle - 360;
            if (angle < -180)
                angle = angle + 360;
            return angle;
        }

        private double Clamp(double value, double min, double max)
        {
            if (value > max)
                value = max;
            if (value < min)
                value = min;
            return value;
        }

        private double RelativeAngle(double angleFrom, double angleTo)
        {
            return angleTo - angleFrom;
        }

        private void ControlGun()
        {
            if (isReadyToChangeAim && botsInfo.Count > 0)
            {
                currentFoeName = botsInfo
                    .Where(e => foes.Contains(e.Key))
                    .Select(e => new { Name = e.Key, e.Value.Distance })
                    .OrderBy(e => e.Distance)
                    .First()
                    .Name;
                isReadyToChangeAim = false;
            }
            if (currentFoeName != null)
            {
                var info = botsInfo[currentFoeName];
                var angle = Clamp(ClampAngleDegrees(info.Bearing + Heading - GunHeading), -GunTurnRate, GunTurnRate);
                Out.WriteLine("Bearing: {0} Heading: {1} GunHeading: {2} Angle: {3}", info.Bearing, Heading, GunHeading, angle);
                TurnGunRight(angle);
                if (Math.Abs(angle) < GunTurnRate)
                    Fire(firePower);
            }
        }

        private void ControlMovements()
        {
            if (!isRightWallReached)
            {
                Out.WriteLine("Turning {0} degrees", Heading);
                var angle = RelativeAngle(Heading, 90);
                TurnRight(angle);
                Out.WriteLine("Heading {0} pixels right", BattleFieldWidth - X - Width / 2);
                var moveDistance = BattleFieldWidth - X - Width / 2;
                Ahead(moveDistance);
                if (X >= BattleFieldWidth - Width / 2)
                    isRightWallReached = true;
            }
            else
            {
                //TODO: Do
            }
        }

        private void ControlRadar()
        {
            TurnRadarLeft(RadarTurningRateDegrees);
        }

        public override void OnScannedRobot(ScannedRobotEvent evnt)
        {
            var angle = Heading - evnt.Bearing;
            var x = X + evnt.Distance * Math.Cos(angle);
            var y = Y + evnt.Distance * Math.Sin(angle);
            botsInfo[evnt.Name] = new BotInfo
            {
                Heading = evnt.Heading,
                Velocity = evnt.Velocity,
                X = x,
                Y = y,
                Distance = evnt.Distance,
                Bearing = evnt.Bearing
            };
        }
    }
}
