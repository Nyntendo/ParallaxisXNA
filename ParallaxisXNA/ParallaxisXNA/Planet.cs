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
    public class Planet
    {
        public Vector2 Position { get; set; }
        public float Radius { get; set; }
        public float ClickRadius { get; set; }
        public bool IsDead { get; set; }

        public Planet(float x, float y, float radius, float clickRadius)
        {
            Position = new Vector2(x, y);
            Radius = radius;
            ClickRadius = clickRadius;
            IsDead = false;
        }

        public bool IsInside(Vector2 position)
        {
            if (Vector2.Subtract(position, Position).Length() < ClickRadius)
                return true;
            return false;
        }
    }
}
