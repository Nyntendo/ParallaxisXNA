using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ParallaxisXNA
{
    public enum ShipType { Fighter1, Fighter2, OrbitalCommand1, OrbitalCommand2, Dreadnaught }

    public static class ShipFactory
    {
        public static Ship CreateShip(ShipType shipType, Vector2 position, Vector2 velocity)
        {
            Ship ship = null;

            switch (shipType)
            {
                case ShipType.Fighter1:
                case ShipType.Fighter2:
                    return new Ship(shipType) { Position = position, Velocity = velocity };

                case ShipType.OrbitalCommand1:
                case ShipType.OrbitalCommand2:
                    ship = new Ship(shipType);
                    ship.Position = position;
                    ship.Velocity = velocity;
                    ship.WeaponReloadTime = 0.3f;
                    ship.NumberShots = 5;
                    ship.MaxSpeed = 1.0f;
                    ship.MaxForce = 0.01f;
                    ship.WeaponRange = 2000.0f;
                    ship.WeaponAimRange = 500.0f;
                    ship.WeaponVelocity = 5.0f;
                    ship.Hitpoints = 50;
                    ship.Mass = 5.0f;
                    ship.ClickRadius = 40.0f;
                    ship.AvoidRadius = 100.0f;
                    ship.IsPushedByImpacts = false;
                    ship.OnImpactBehaviour = Ship.ImpactBehaviour.SwitchTargetIfCurrentIsOutOfSight;
                    ship.ShotType = ShotTypes.Homing;
                    ship.ResetShots();
                    return ship;
                case ShipType.Dreadnaught:
                    ship = new Ship(shipType);
                    ship.Position = position;
                    ship.Velocity = velocity;
                    ship.WeaponReloadTime = 0.1f;
                    ship.NumberShots = 10;
                    ship.MaxSpeed = 3.0f;
                    ship.MaxForce = 0.05f;
                    ship.WeaponRange = 2000.0f;
                    ship.WeaponAimRange = 500.0f;
                    ship.WeaponVelocity = 5.0f;
                    ship.Hitpoints = 50;
                    ship.Mass = 20.0f;
                    ship.ClickRadius = 40.0f;
                    ship.AvoidRadius = 100.0f;
                    ship.IsPushedByImpacts = false;
                    ship.OnImpactBehaviour = Ship.ImpactBehaviour.SwitchTargetIfCurrentIsOutOfSight;
                    ship.ShotType = ShotTypes.Homing;
                    ship.ResetShots();
                    return ship;

                default:
                    return new Ship(shipType) { Position = position, Velocity = velocity };
            }
        }
    }
}
