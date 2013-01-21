using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ParallaxisXNA
{
    public enum Player { Player1, Player2 };

    public struct TextureOrigin
    {
        public TextureOrigin(Texture2D texture) : this()
        {
            Texture = texture;
            Origin = new Vector2(Texture.Width / 2, Texture.Height / 2);
        }

        public Texture2D Texture { get; set; }
        public Vector2 Origin { get; set; }
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Rectangle viewportRect;

        List<Ship> ships;
        List<Ship> ships2;
        List<Planet> planets;
        List<Planet> moons;

        List<Ship> selectedShips;

        SpriteFont debugFont;
        string debugText;

        Dictionary<ShipType, TextureOrigin> shipTextures;
        Dictionary<ShotTypes, TextureOrigin> shotTextures;
        Texture2D planetTexture;
        Texture2D moonTexture;
        Texture2D sparkTexture;
        Texture2D selectedShipTexture;
        Texture2D selectionTexture;
        Texture2D background;

        Vector2 planetOrigin;
        Vector2 moonOrigin;
        Vector2 selectedShipOrigin;

        int numberShips = 100;
        int numberPlanets = 1;

        float scrollSpeed = 20.0f;

        MouseState previousMouseState;
        KeyboardState previousKeyboardState;

        Random rand;

        Player activePlayer;

        Vector2 selectionStart;
        Rectangle selectionRect;
        bool isSelecting;

        Camera camera;
        Vector2 shipParallax;
        Vector2 planetParallax;
        Vector2 moonParallax;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1680;
            graphics.PreferredBackBufferHeight = 1050;
            graphics.IsFullScreen = true;
            IsMouseVisible = true;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            shipTextures = new Dictionary<ShipType, TextureOrigin>();
            shotTextures = new Dictionary<ShotTypes, TextureOrigin>();
            ships = new List<Ship>();
            ships2 = new List<Ship>();
            planets = new List<Planet>();
            moons = new List<Planet>();
            selectedShips = new List<Ship>();

            rand = new Random();

            debugText = "";

            activePlayer = Player.Player1;
            isSelecting = false;

            camera = new Camera(GraphicsDevice.Viewport);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            viewportRect = new Rectangle(0, 0,
                graphics.GraphicsDevice.Viewport.Width,
                graphics.GraphicsDevice.Viewport.Height);

            shipTextures.Add(ShipType.Fighter1, new TextureOrigin(Content.Load<Texture2D>("ship")));
            shipTextures.Add(ShipType.Fighter2, new TextureOrigin(Content.Load<Texture2D>("ship2")));
            shipTextures.Add(ShipType.OrbitalCommand1, new TextureOrigin(Content.Load<Texture2D>("orbital_command")));
            shipTextures.Add(ShipType.OrbitalCommand2, new TextureOrigin(Content.Load<Texture2D>("orbital_command2")));
            shipTextures.Add(ShipType.Dreadnaught, new TextureOrigin(Content.Load<Texture2D>("dreadnaught")));

            shotTextures.Add(ShotTypes.Normal, new TextureOrigin(Content.Load<Texture2D>("shot")));
            shotTextures.Add(ShotTypes.Homing, new TextureOrigin(Content.Load<Texture2D>("shot2")));

            planetTexture = Content.Load<Texture2D>("venus");
            moonTexture = Content.Load<Texture2D>("moon");
            sparkTexture = Content.Load<Texture2D>("spark");
            selectedShipTexture = Content.Load<Texture2D>("selected_ship");

            planetOrigin = new Vector2(planetTexture.Width / 2, planetTexture.Height / 2);
            moonOrigin = new Vector2(moonTexture.Width / 2, moonTexture.Height / 2);
            selectedShipOrigin = new Vector2(selectedShipTexture.Width / 2, selectedShipTexture.Height / 2);

            selectionTexture = Content.Load<Texture2D>("selection");
            background = Content.Load<Texture2D>("space");

            debugFont = Content.Load<SpriteFont>("debugFont");

            shipParallax = new Vector2(1.0f);
            planetParallax = new Vector2(0.4f);
            moonParallax = new Vector2(0.5f);

            CreateShips();
            CreateShips2();
            CreatePlanets();
            CreateMoons();

            previousMouseState = Mouse.GetState();
            previousKeyboardState = Keyboard.GetState();
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keyboardState.IsKeyDown(Keys.Escape))
                this.Exit();

            if (keyboardState.IsKeyDown(Keys.F5) && previousKeyboardState.IsKeyUp(Keys.F5))
            {
                Reset();
            }

            if (keyboardState.IsKeyDown(Keys.Insert) && previousKeyboardState.IsKeyUp(Keys.Insert))
            {
                camera.Rotation += 0.1f;
            }

            if (keyboardState.IsKeyDown(Keys.Delete) && previousKeyboardState.IsKeyUp(Keys.Delete))
            {
                camera.Rotation -= 0.1f;
            }

            if (keyboardState.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space))
            {
                selectedShips.Clear();
                selectedShips.AddRange((activePlayer == Player.Player1) ? ships : ships2);
            }

            if (keyboardState.IsKeyDown(Keys.Tab) && previousKeyboardState.IsKeyUp(Keys.Tab))
            {
                selectedShips.Clear();
                activePlayer = (activePlayer == Player.Player1) ? Player.Player2 : Player.Player1;
            }

            if (mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
            {
                isSelecting = true;
                selectionStart = ScreenToWorld(new Vector2(mouseState.X, mouseState.Y), new Vector2(1));
            }

            if (isSelecting)
            {
                Vector2 selectionEnd = ScreenToWorld(new Vector2(mouseState.X, mouseState.Y), new Vector2(1));

                int x = (int)((selectionStart.X < selectionEnd.X) ? selectionStart.X : selectionEnd.X);
                int y = (int)((selectionStart.Y < selectionEnd.Y) ? selectionStart.Y : selectionEnd.Y);
                int width = (int)Math.Abs(selectionEnd.X - selectionStart.X);
                int height = (int)Math.Abs(selectionEnd.Y - selectionStart.Y);

                selectionRect = new Rectangle(x, y, width, height);
            }

            if (mouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed && isSelecting)
            {
                if(keyboardState.IsKeyUp(Keys.LeftShift) && keyboardState.IsKeyUp(Keys.RightShift))
                    selectedShips.Clear();

                List<Ship> shipsToSelect = (activePlayer == Player.Player1) ? ships : ships2;

                foreach (Ship ship in shipsToSelect)
                {
                    if (selectionRect.Contains((int)ship.Position.X, (int)ship.Position.Y) && !ship.IsDead)
                        selectedShips.Add(ship);
                }

                isSelecting = false;
            }

            if (mouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released)
            {
                Vector2 mousePos = ScreenToWorld(new Vector2(mouseState.X, mouseState.Y), new Vector2(1));

                debugText = string.Format("X: {0}, Y: {1}", mousePos.X, mousePos.Y);

                bool foundTarget = false;

                foreach (Ship ship in (activePlayer == Player.Player1)?ships2:ships)
                {
                    if (ship.IsInside(mousePos))
                    {
                        SetTarget(ship, keyboardState.IsKeyUp(Keys.LeftShift) && keyboardState.IsKeyUp(Keys.RightShift));
                        foundTarget = true;
                        break;
                    }
                }

                if (!foundTarget)
                {
                        SetWaypoint(mousePos, keyboardState.IsKeyUp(Keys.LeftShift) && keyboardState.IsKeyUp(Keys.RightShift));
                }
            }

            if (mouseState.X <= 0 && previousMouseState.X <= 0)
                camera.Position -= new Vector2(scrollSpeed, 0);
            if (mouseState.X >= viewportRect.Width - 1 && previousMouseState.X >= viewportRect.Width - 1)
                camera.Position += new Vector2(scrollSpeed, 0);
            if (mouseState.Y <= 0 && previousMouseState.Y <= 0)
                camera.Position -= new Vector2(0, scrollSpeed);
            if (mouseState.Y >= viewportRect.Height - 1 && previousMouseState.Y >= viewportRect.Height - 1)
                camera.Position += new Vector2(0, scrollSpeed);

            if (mouseState.ScrollWheelValue < previousMouseState.ScrollWheelValue)
            {
                camera.Zoom *= 0.9f;
            }
            else if (mouseState.ScrollWheelValue > previousMouseState.ScrollWheelValue)
            {
                camera.Zoom /= 0.9f;
            }

            if(keyboardState.IsKeyDown(Keys.PageUp) && previousKeyboardState.IsKeyUp(Keys.PageUp))
            {
                camera.Zoom /= 0.9f;
            }

            if (keyboardState.IsKeyDown(Keys.PageDown) && previousKeyboardState.IsKeyUp(Keys.PageDown))
            {
                camera.Zoom *= 0.9f;
            }

            ships.RemoveAll(x => x.Hitpoints <= 0);
            ships2.RemoveAll(x => x.Hitpoints <= 0);
            selectedShips.RemoveAll(x => x.Hitpoints <= 0);

            foreach (Ship ship in ships)
            {
                ship.Update(ships, ships2, elapsed);
            }

            foreach (Ship ship in ships2)
            {
                ship.Update(ships2, ships, elapsed);
            }

            if (selectedShips.Count == 1)
            {
                debugText = "";

                foreach (var prop in selectedShips.First().GetType().GetProperties())
                {
                    debugText += prop.Name + " : " + prop.GetValue(selectedShips.First(),null) + "\n";
                }
            }

            previousMouseState = mouseState;
            previousKeyboardState = keyboardState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            spriteBatch.Draw(background, Vector2.Zero, Color.White);

            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.GetViewMatrix(planetParallax));

            foreach (Planet planet in planets)
            {
                spriteBatch.Draw(planetTexture, planet.Position, null, Color.White, 0.0f, planetOrigin, 1.0f, SpriteEffects.None, 0.2f);
            }

            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.GetViewMatrix(moonParallax));

            foreach (Planet moon in moons)
            {
                spriteBatch.Draw(moonTexture, moon.Position, null, Color.White, 0.0f, moonOrigin, 0.8f, SpriteEffects.None, 0.3f);
            }

            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.GetViewMatrix(shipParallax));

            foreach (Ship ship in selectedShips)
            {
                spriteBatch.Draw(selectedShipTexture, ship.Position, null, Color.White * 0.5f, 0.0f, selectedShipOrigin, 1.0f, SpriteEffects.None, 0.4f);

                if (ship.Target != null)
                {
                    spriteBatch.Draw(selectedShipTexture, ship.Target.Position, null, Color.Red * 0.5f, 0.0f, selectedShipOrigin, 1.0f, SpriteEffects.None, 0.4f);
                }
            }

            DrawShips(spriteBatch, ships);
            DrawShips(spriteBatch, ships2);

            if (isSelecting)
            {
                spriteBatch.Draw(selectionTexture, selectionRect, null, Color.White * 0.3f, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            }

            spriteBatch.End();

            spriteBatch.Begin();

            spriteBatch.DrawString(debugFont, debugText, new Vector2(10, 10), Color.White);

            //spriteBatch.DrawString(debugFont, "+", new Vector2(viewportRect.Width/2, viewportRect.Height/2) * scale + scroll, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawShips(SpriteBatch spriteBatch, List<Ship> ships)
        {
            foreach (Ship ship in ships)
            {
                float rotation = 0.0f;
                rotation += (float)Math.Atan2(ship.Velocity.X, -ship.Velocity.Y);

                spriteBatch.Draw(shipTextures[ship.ShipType].Texture, ship.Position, null, Color.White, rotation, shipTextures[ship.ShipType].Origin, 1.0f, SpriteEffects.None, 0.5f);

                foreach (Shot shot in ship.Shots)
                {
                    if (shot.Visible)
                    {
                        float shotRotation = (shot.ShotType == ShotTypes.Normal) ? (float)Math.Atan2(shot.Velocity.X, -shot.Velocity.Y) : 0;
                        spriteBatch.Draw(shotTextures[shot.ShotType].Texture, shot.Position, null, Color.White, shotRotation, shotTextures[shot.ShotType].Origin, 1.0f, SpriteEffects.None, 0.8f);
                    }
                }

                foreach (Spark spark in ship.Sparks)
                {
                    if (spark.Visible)
                        spriteBatch.Draw(sparkTexture, spark.Position, null, spark.Color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.9f);
                }
            }
        }

        private void CreateShips()
        {
            for (int i = 0; i < numberShips; i++)
            {
                ships.Add(ShipFactory.CreateShip(ShipType.Fighter1, new Vector2(rand.Next(0, viewportRect.Width), rand.Next(0, viewportRect.Height)), new Vector2(rand.Next(-1, 2), rand.Next(-1, 2))));
            }

            ships.Add(ShipFactory.CreateShip(ShipType.OrbitalCommand1, new Vector2(rand.Next(0, viewportRect.Width), rand.Next(0, viewportRect.Height)), new Vector2(rand.Next(-1, 2), rand.Next(-1, 2))));
            ships.Add(ShipFactory.CreateShip(ShipType.OrbitalCommand1, new Vector2(rand.Next(0, viewportRect.Width), rand.Next(0, viewportRect.Height)), new Vector2(rand.Next(-1, 2), rand.Next(-1, 2))));
            ships.Add(ShipFactory.CreateShip(ShipType.Dreadnaught, new Vector2(rand.Next(0, viewportRect.Width), rand.Next(0, viewportRect.Height)), new Vector2(rand.Next(-1, 2), rand.Next(-1, 2))));
        }

        private void CreateShips2()
        {
            for (int i = 0; i < numberShips; i++)
            {
                ships2.Add(ShipFactory.CreateShip(ShipType.Fighter2, new Vector2(rand.Next(0, viewportRect.Width), rand.Next(0, viewportRect.Height)), new Vector2(rand.Next(-1, 2), rand.Next(-1, 2))));
            }

            ships2.Add(ShipFactory.CreateShip(ShipType.OrbitalCommand2, new Vector2(rand.Next(0, viewportRect.Width), rand.Next(0, viewportRect.Height)), new Vector2(rand.Next(-1, 2), rand.Next(-1, 2))));
            ships2.Add(ShipFactory.CreateShip(ShipType.OrbitalCommand2, new Vector2(rand.Next(0, viewportRect.Width), rand.Next(0, viewportRect.Height)), new Vector2(rand.Next(-1, 2), rand.Next(-1, 2))));
            ships2.Add(ShipFactory.CreateShip(ShipType.Dreadnaught, new Vector2(rand.Next(0, viewportRect.Width), rand.Next(0, viewportRect.Height)), new Vector2(rand.Next(-1, 2), rand.Next(-1, 2))));
        }

        private void Reset()
        {
            selectedShips.Clear();
            ships.Clear();
            CreateShips();
            ships2.Clear();
            CreateShips2();
        }

        private void CreatePlanets()
        {
            planets.Add(new Planet(viewportRect.Width / 2, viewportRect.Height / 2, 2000, planetTexture.Width / 2));

            /*
            for (int i = 0; i < numberPlanets; i++)
            {
                planets.Add(new Planet((float)rand.Next(0, viewportRect.Width), (float)rand.Next(0, viewportRect.Height), 2000, planetTexture.Width / 2));
            }
            */
        }

        private void CreateMoons()
        {
            moons.Add(new Planet(viewportRect.Width / 2 + 300, viewportRect.Height / 2 + 300, 2000, moonTexture.Width / 2));

            /*
            for (int i = 0; i < numberPlanets; i++)
            {
                planets.Add(new Planet((float)rand.Next(0, viewportRect.Width), (float)rand.Next(0, viewportRect.Height), 2000, planetTexture.Width / 2));
            }
            */
        }

        private Vector2 ScreenToWorld(Vector2 screenPosition, Vector2 parallax)
        {
            return Vector2.Transform(screenPosition, Matrix.Invert(camera.GetViewMatrix(parallax)));
        }

        private Vector2 WorldToScreen(Vector2 worldPosition, Vector2 parallax)
        {
            return Vector2.Transform(worldPosition, camera.GetViewMatrix(parallax));
        }

        private void SetTarget(Ship target, bool clearWaypoints)
        {
            foreach (Ship ship in selectedShips)
            {
                if (clearWaypoints)
                    ship.Waypoints.Clear();
                ship.Target = target;
            }
        }

        private void SetWaypoint(Vector2 waypoint, bool clearWaypoints)
        {
            foreach (Ship ship in selectedShips)
            {
                if (clearWaypoints)
                    ship.Waypoints.Clear();
                ship.Waypoints.Enqueue(waypoint);
                ship.Target = null;
            }
        }
    }
}
