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
    public enum ShotTypes { Normal, Homing }

    public class Shot
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public bool Visible { get; set; }
        public float TravelDistance { get; set; }
        public float Mass { get; set; }
        public ShotTypes ShotType { get; set; }

        public Shot(ShotTypes shotType)
        {
            Position = Vector2.Zero;
            Velocity = Vector2.Zero;
            Visible = false;
            TravelDistance = 0.0f;
            Mass = 0.5f;
            ShotType = shotType;
        }
    }
}
