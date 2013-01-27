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
    public class Spark
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float TTL { get; set; }
        public bool Visible { get; set; }
        public Color Color { get; set; }
        public float Scale { get; set; }
        public float Opacity { get; set; }
        public float OriginalTTL { get; set; }

        public Spark()
        {
            Position = new Vector2(0);
            Velocity = new Vector2(0);
            TTL = 0;
            Visible = false;
            Color = Color.White;
            Scale = 1.0f;
            Opacity = 1.0f;
        }

        public static Color[] Colors = {Color.Yellow, Color.LightGoldenrodYellow, Color.LightYellow, Color.White };
    }
}
