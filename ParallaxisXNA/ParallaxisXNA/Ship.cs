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
    public class Ship
    {
        public enum ImpactBehaviour { KeepCurrentTarget, SwitchTarget, SwitchTargetIfCurrentIsOutOfSight }

        public ShipType ShipType { get; set; }

        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Acceleration { get; set; }

        public float MaxSpeed { get; set; }
        public float MaxForce { get; set; }

        public float NeighborSeparation { get; set; }
        public float NeighborDetection { get; set; }

        public float SeparationWeight { get; set; }
        public float AlignmentWeight { get; set; }
        public float CohesionWeight { get; set; }
        public float AvoidanceWeight { get; set; }
        public float TargetSeekWeight { get; set; }
        public float WaypointWeight { get; set; }
        public float BoundsWeight { get; set; }

        public ShotTypes ShotType { get; set; }
        public float WeaponReloadTime { get; set; }
        public float WeaponAimRange { get; set; }
        public float WeaponRange { get; set; }
        public float WeaponVelocity { get; set; }
        public float WeaponMaxForce { get; set; }
        public float ReloadTimer { get; set; }
        public float EnemySpotRange { get; set; }
        public int Hitpoints { get; set; }
        public float Mass { get; set; }
        public float AvoidRadius { get; set; }
        public float ClickRadius { get; set; }
        public float HitRadius { get; set; }
        public Vector2 BoundsCenter { get; set; }
        public float BoundsRadius { get; set; }
        public float WaypointHitRadius { get; set; }

        public Ship Target { get; set; }
        public bool TargetIsOutOfSight { get; set; }
        public int NumberShots { get; set; }
        public List<Shot> Shots { get; set; }
        public int NumberSparks { get; set; }
        public List<Spark> Sparks { get; set; }
        public bool IsDead { get; set; }
        public bool IsPushedByImpacts { get; set; }
        public ImpactBehaviour OnImpactBehaviour { get; set; }
        public Queue<Vector2> Waypoints { get; set; }

        public CreateGravityExplosionDelegate CreateGravityExplosion { get; set; }

        public Ship(ShipType shipType)
        {
            ShipType = shipType;

            Position = new Vector2(0);
            Velocity = new Vector2(0);
            Acceleration = new Vector2(0);

            MaxSpeed = 7.0f;
            MaxForce = 0.13f;

            NeighborSeparation = 50.0f;
            NeighborDetection = 100.0f;

            SeparationWeight = 1.5f;
            AlignmentWeight = 1.0f;
            CohesionWeight = 1.0f;
            AvoidanceWeight = 1.5f;
            TargetSeekWeight = 1.5f;
            WaypointWeight = 1.5f;
            BoundsWeight = 1.5f;

            WeaponReloadTime = 3.0f;
            WeaponAimRange = 400.0f;
            WeaponRange = 800.0f;
            WeaponVelocity = 10.0f;
            WeaponMaxForce = 0.13f;
            ReloadTimer = 0.0f;
            EnemySpotRange = 1600.0f;

            Target = null;
            TargetIsOutOfSight = false;

            ClickRadius = 20.0f;
            AvoidRadius = 50.0f;
            HitRadius = 20.0f;
            WaypointHitRadius = 50.0f;

            BoundsCenter = new Vector2(0);
            BoundsRadius = 8000.0f;
            
            Mass = 1.0f;

            ShotType = ShotTypes.Normal;

            NumberShots = 5;

            Shots = new List<Shot>();
            for (int i = 0; i < NumberShots; i++)
            {
                Shots.Add(new Shot(ShotType));
            }

            NumberSparks = 200;

            Sparks = new List<Spark>();
            for (int i = 0; i < NumberSparks; i++)
            {
                Sparks.Add(new Spark());
            }

            Hitpoints = 10;
            IsDead = false;
            IsPushedByImpacts = true;

            OnImpactBehaviour = ImpactBehaviour.SwitchTarget;

            Waypoints = new Queue<Vector2>();
        }

        public void Update(List<Ship> ships, List<Ship> enemies, float elapsed)
        {
            if (ReloadTimer > 0.0f)
                ReloadTimer -= elapsed;

            CalculateAcceleration(ships, enemies);

            Velocity += Acceleration;
            Velocity = Limit(Velocity, MaxSpeed);
            Position += Velocity;

            if (Target != null)
            {
                if (!Target.IsDead)
                {
                    Vector2 distanceToTarget = Vector2.Subtract(Target.Position, Position);
                    if (distanceToTarget.Length() < WeaponAimRange)
                    {
                        TargetIsOutOfSight = false;
                        if (distanceToTarget.Length() < WeaponAimRange && ReloadTimer <= 0.0f)
                        {
                            Shoot();
                            ReloadTimer = WeaponReloadTime;
                        }
                    }
                    else
                    {
                        TargetIsOutOfSight = true;
                    }
                }
                else
                {
                    Target = null;
                    FindNextTarget(enemies);
                }
            }
            else if(Waypoints.Count == 0)
            {
                FindNextTarget(enemies);
            }

            if (Waypoints.Count > 0)
            {
                Vector2 distanceToWaypoint = Vector2.Subtract(Waypoints.Peek(), Position);
                if (distanceToWaypoint.Length() < WaypointHitRadius)
                {
                    Waypoints.Dequeue();
                }
            }

            foreach (Shot shot in Shots)
            {
                if (shot.Visible)
                {
                    if (shot.ShotType == ShotTypes.Homing && Target != null)
                    {
                        Vector2 acceleration = Vector2.Subtract(Target.Position, shot.Position);
                        acceleration.Normalize();
                        acceleration *= WeaponVelocity;
                        acceleration -= shot.Velocity;
                        acceleration = Limit(acceleration, WeaponMaxForce);
                        shot.Velocity += acceleration;
                    }

                    shot.Position += shot.Velocity;

                    foreach (Ship enemy in enemies)
                    {
                        if (Vector2.Subtract(shot.Position, enemy.Position).Length() < enemy.HitRadius && !enemy.IsDead)
                        {
                            shot.Visible = false;
                            CreateExplosion(enemy.Position.X, enemy.Position.Y, 25, 1);
                            if (IsPushedByImpacts)
                                enemy.Acceleration += shot.Velocity * (shot.Mass / enemy.Mass);
                            if (enemy.OnImpactBehaviour == ImpactBehaviour.SwitchTarget || (enemy.OnImpactBehaviour == ImpactBehaviour.SwitchTargetIfCurrentIsOutOfSight && enemy.TargetIsOutOfSight))
                                enemy.Target = this;
                            enemy.Hitpoints--;
                            if (enemy.Hitpoints <= 0)
                            {
                                Target = null;
                                enemy.IsDead = true;
                                CreateExplosion(enemy.Position.X, enemy.Position.Y, 100, 2);
                                CreateGravityExplosion(enemy.Position, enemy.Mass);
                            }
                        }
                    }

                    shot.TravelDistance += shot.Velocity.Length();
                    if (shot.TravelDistance >= WeaponRange)
                        shot.Visible = false;
                }
            }

            foreach (Spark spark in Sparks)
            {
                if (spark.Visible)
                {
                    spark.Position += spark.Velocity;
                    spark.TTL -= elapsed;
                    spark.Opacity -= elapsed / spark.OriginalTTL / 1.0f;
                    spark.Scale += elapsed / spark.OriginalTTL * 1.0f;
                    if (spark.TTL <= 0)
                    {
                        spark.Visible = false;
                    }
                }
            }

            Acceleration *= 0.0f;
        }

        private void FindNextTarget(List<Ship> enemies)
        {
            Ship newTarget = Target;
            float distanceToTarget = EnemySpotRange;

            foreach(Ship enemy in enemies)
            {
                if (Vector2.Subtract(enemy.Position, Position).Length() < distanceToTarget && !enemy.IsDead)
                {
                    distanceToTarget = Vector2.Subtract(enemy.Position, Position).Length();
                    newTarget = enemy;
                }
            }

            Target = newTarget;
        }

        private void CalculateAcceleration(List<Ship> ships, List<Ship> enemies)
        {
            Vector2 Separation = new Vector2(0,0);
            Vector2 Alignment = new Vector2(0, 0);
            Vector2 Cohesion = new Vector2(0, 0);
            Vector2 TargetSeek = new Vector2(0, 0);
            Vector2 Avoidence = Avoid(enemies);
            Vector2 WaypointSeek = new Vector2(0, 0);
            Vector2 BoundsSeek = Seek(BoundsCenter);

            float modifiedTargetSeekWeight = (WeaponReloadTime - ReloadTimer) / WeaponReloadTime;
            modifiedTargetSeekWeight *= TargetSeekWeight;

            float modifiedBoundsWeight = (float)Math.Min(Math.Pow((Position - BoundsCenter).Length() / BoundsRadius, 2) * BoundsWeight, BoundsWeight);

            Flock(ref Separation, ref Alignment, ref Cohesion, ships);

            if (Target != null && Waypoints.Count == 0)
            {
                if (Target != this)
                    TargetSeek = Seek(Target.Position);
            }

            if (Waypoints.Count > 0)
            {
                WaypointSeek = Seek(Waypoints.Peek());
            }

            Separation *= SeparationWeight;
            Alignment *= AlignmentWeight;
            Cohesion *= CohesionWeight;
            TargetSeek *= modifiedTargetSeekWeight;
            Avoidence *= AvoidanceWeight;
            WaypointSeek *= WaypointWeight;
            BoundsSeek *= modifiedBoundsWeight;

            ApplyForce(Separation);
            ApplyForce(Alignment);
            ApplyForce(Cohesion);
            ApplyForce(TargetSeek);
            ApplyForce(Avoidence);
            ApplyForce(WaypointSeek);
            ApplyForce(BoundsSeek);
        }

        private void ApplyForce(Vector2 force)
        {
            Acceleration += force / Mass;
        }

        private Vector2 Avoid(List<Ship> enemies)  
        {
            Vector2 steer = new Vector2(0, 0);
            int count = 0;

            foreach (Ship enemy in enemies)
            {
                float distance = Vector2.Distance(Position, enemy.Position);
                if (distance < enemy.AvoidRadius + AvoidRadius)
                {
                    Vector2 diff = Vector2.Subtract(Position, enemy.Position);
                    diff.Normalize();
                    diff /= distance;
                    steer += diff;
                    count++;
                }
            }

            if (count > 0)
            {
                steer = Vector2.Divide(steer, (float)count);
            }

            if (steer.Length() > 0)
            {
                steer.Normalize();
                steer *= MaxSpeed;
                steer -= Velocity;
                steer = Limit(steer, MaxForce);
            }
            return steer;
        }

        private void Flock(ref Vector2 Separation, ref Vector2 Alignment, ref Vector2 Cohesion, List<Ship> ships)
        {
            int SeparationCount = 0;
            int AlignmentCount = 0;
            int CohesionCount = 0;

            foreach (Ship ship in ships)
            {
                if (ship != this)
                {
                    float distance = Vector2.Distance(Position, ship.Position);

                    //Separation
                    if (distance < NeighborSeparation)
                    {
                        Vector2 diff = Vector2.Subtract(Position, ship.Position);
                        diff.Normalize();
                        diff = Vector2.Divide(diff, distance);
                        Separation = Vector2.Add(Separation, diff);
                        SeparationCount++;
                    }

                    if (distance < NeighborDetection)
                    {
                        //Alignment
                        Alignment += ship.Velocity;
                        AlignmentCount++;

                        //Cohesion
                        Cohesion += ship.Position;
                        CohesionCount++;
                    }
                }
            }

            if (SeparationCount > 0)
            {
                Separation /= (float)SeparationCount;
            }

            if (Separation.Length() > 0)
            {
                Separation.Normalize();
                Separation *= MaxSpeed;
                Separation -= Velocity;
                Separation = Limit(Separation, MaxForce);
            }

            if (AlignmentCount > 0)
            {
                Alignment /= (float)AlignmentCount;
                Alignment.Normalize();
                Alignment *= MaxSpeed;
                Alignment -= Velocity;
                Alignment = Limit(Alignment, MaxForce);
            }

            if (CohesionCount > 0)
            {
                Cohesion /= (float)CohesionCount;
                Cohesion = Seek(Cohesion);
            }

        }

        private static Vector2 Limit(Vector2 vector, float limit)
        {
            if (vector.Length() > limit)
            {
                vector.Normalize();
                vector *= limit;
            }
            return vector;
        }

        private Vector2 Seek(Vector2 target)
        {
            Vector2 desired = Vector2.Subtract(target, Position);
            desired.Normalize();
            desired = Vector2.Multiply(desired, MaxSpeed);
            Vector2 steer = Vector2.Subtract(desired, Velocity);
            steer = Limit(steer, MaxForce);
            return steer;
        }

        public bool IsInside(Vector2 position)
        {
            if (Vector2.Subtract(position, Position).Length() < ClickRadius)
                return true;
            return false;
        }

        public void Shoot()
        {
            foreach (Shot shot in Shots)
            {
                if (!shot.Visible)
                {
                    shot.Position = new Vector2(Position.X, Position.Y);
                    Vector2 velocity = Vector2.Subtract(Target.Position, Position);
                    shot.TravelDistance = 0.0f;
                    velocity.Normalize();
                    velocity *= WeaponVelocity;
                    shot.Velocity = velocity;
                    shot.Visible = true;
                    break;
                }
            }
        }

        private void CreateExplosion(float x, float y, int numberSparks, float duration)
        {
            Random rand = new Random((int)(x * y));
            int sparksCreated = 0;
            foreach (Spark spark in Sparks)
            {
                if (!spark.Visible)
                {
                    spark.Position = new Vector2(x, y);
                    spark.Velocity = new Vector2((float)rand.NextDouble(), (float)rand.NextDouble()) * rand.Next(1, 3);
                    spark.TTL = ((float)rand.NextDouble() * 0.6f + 0.4f) * duration;
                    spark.OriginalTTL = spark.TTL;
                    spark.Scale = 1.0f;
                    spark.Opacity = 1.0f;
                    spark.Visible = true;
                    spark.Color = Spark.Colors[rand.Next(0, Spark.Colors.Length)];
                    sparksCreated++;
                    if (sparksCreated > numberSparks - 1)
                    {
                        break;
                    }
                }
            }
        }

        public void ResetShots()
        {
            Shots.Clear();
            for (int i = 0; i < NumberShots; i++)
            {
                Shots.Add(new Shot(ShotType));
            }
        }
    }
}
