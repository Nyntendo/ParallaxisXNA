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
    public class GravityExplosion
    {
        public GravityExplosion(Vector2 position, float ttl, float scale, float strength, float amplitude, float force)
        {
            Position = position;
            TTL = ttl;
            Scale = scale * amplitude;
            Strength = strength;
            Amplitude = amplitude;
            Force = force;
        }

        public Vector2 Position { get; set; }
        public float TTL { get; set; }
        public float Scale { get; set; }
        public float Strength { get; set; }
        public float Amplitude { get; set; }
        public float Force { get; set; }
    }
}
