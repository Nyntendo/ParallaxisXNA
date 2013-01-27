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

    public delegate void CreateGravityExplosionDelegate(Vector2 position, float amplitude);

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        BloomComponent bloom;

        int currentBloomSetting = 0;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Rectangle viewportRect;

        List<Ship> ships;
        List<Ship> ships2;
        List<Planet> planets;
        List<Planet> moons;
        List<GravityExplosion> gravityExplosions;

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
        Texture2D backgroundTexture;
        Texture2D gravityExplosionTexture;

        Effect rippleEffect;

        Vector2 planetOrigin;
        Vector2 moonOrigin;
        Vector2 selectedShipOrigin;
        Vector2 sparkOrigin;

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

        RenderTarget2D mainRenderTarget;
        RenderTarget2D explosionRenderTarget;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1680;
            graphics.PreferredBackBufferHeight = 1050;
            graphics.IsFullScreen = true;
            IsMouseVisible = true;
            Content.RootDirectory = "Content";

            bloom = new BloomComponent(this);

            Components.Add(bloom);
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
            gravityExplosions = new List<GravityExplosion>();

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
            sparkTexture = Content.Load<Texture2D>("biggerspark");
            selectedShipTexture = Content.Load<Texture2D>("selected_ship");
            gravityExplosionTexture = Content.Load<Texture2D>("gravity_explosion");

            rippleEffect = Content.Load<Effect>("Ripple");

            planetOrigin = new Vector2(planetTexture.Width / 2, planetTexture.Height / 2);
            moonOrigin = new Vector2(moonTexture.Width / 2, moonTexture.Height / 2);
            selectedShipOrigin = new Vector2(selectedShipTexture.Width / 2, selectedShipTexture.Height / 2);
            sparkOrigin = new Vector2(sparkTexture.Width / 2, sparkTexture.Height / 2);

            selectionTexture = Content.Load<Texture2D>("selection");
            backgroundTexture = Content.Load<Texture2D>("space");

            debugFont = Content.Load<SpriteFont>("debugFont");

            shipParallax = new Vector2(1.0f);
            planetParallax = new Vector2(0.4f);
            moonParallax = new Vector2(0.5f);

            CreateShips();
            CreateShips2();
            CreatePlanets();
            CreateMoons();

            mainRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            explosionRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);

            previousMouseState = Mouse.GetState();
            previousKeyboardState = Keyboard.GetState();

            bloom.Settings = BloomSettings.PresetSettings[currentBloomSetting];
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            HandleInput();

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
                    debugText += prop.Name + " : " + prop.GetValue(selectedShips.First(), null) + "\n";
                }
            }
            else
            {
                debugText = "";
            }

            UpdateGravityExplosions(elapsed);

            base.Update(gameTime);
        }

        protected void HandleInput()
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();

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
                if (keyboardState.IsKeyUp(Keys.LeftShift) && keyboardState.IsKeyUp(Keys.RightShift))
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

                foreach (Ship ship in (activePlayer == Player.Player1) ? ships2 : ships)
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

            if (keyboardState.IsKeyDown(Keys.PageUp) && previousKeyboardState.IsKeyUp(Keys.PageUp))
            {
                camera.Zoom /= 0.9f;
            }

            if (keyboardState.IsKeyDown(Keys.PageDown) && previousKeyboardState.IsKeyUp(Keys.PageDown))
            {
                camera.Zoom *= 0.9f;
            }

            if (keyboardState.IsKeyDown(Keys.Add) && previousKeyboardState.IsKeyUp(Keys.Add))
            {
                if (currentBloomSetting < BloomSettings.PresetSettings.Length -1)
                    currentBloomSetting++;
                bloom.Settings = BloomSettings.PresetSettings[currentBloomSetting];
            }

            if (keyboardState.IsKeyDown(Keys.Subtract) && previousKeyboardState.IsKeyUp(Keys.Subtract))
            {
                if (currentBloomSetting > 0)
                    currentBloomSetting--;
                bloom.Settings = BloomSettings.PresetSettings[currentBloomSetting];
            }

            if (keyboardState.IsKeyDown(Keys.Multiply) && previousKeyboardState.IsKeyUp(Keys.Multiply))
            {
                bloom.Visible = !bloom.Visible;
            }

            previousMouseState = mouseState;
            previousKeyboardState = keyboardState;
        }

        protected override void Draw(GameTime gameTime)
        {
            

            GraphicsDevice.SetRenderTarget(mainRenderTarget);

            #region mainRenderTarget Draw

            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            spriteBatch.Draw(backgroundTexture, Vector2.Zero, Color.White);

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

            spriteBatch.Begin(SpriteSortMode.FrontToBack, null, null, null, null, null, camera.GetViewMatrix(shipParallax));

            foreach (Ship ship in selectedShips)
            {
                spriteBatch.Draw(selectedShipTexture, ship.Position, null, Color.White * 0.5f, 0.0f, selectedShipOrigin, 1.0f, SpriteEffects.None, 0.55f);

                if (ship.Target != null)
                {
                    spriteBatch.Draw(selectedShipTexture, ship.Target.Position, null, Color.Red * 0.5f, 0.0f, selectedShipOrigin, 1.0f, SpriteEffects.None, 0.55f);
                }
            }

            DrawShips(spriteBatch, ships);
            DrawShips(spriteBatch, ships2);

            

            spriteBatch.End();

            #endregion

            GraphicsDevice.SetRenderTarget(explosionRenderTarget);

            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.GetViewMatrix(shipParallax));

            foreach (GravityExplosion explosion in gravityExplosions)
            {
                spriteBatch.Draw(gravityExplosionTexture, explosion.Position, null, Color.White * explosion.Strength, 0.0f, new Vector2(gravityExplosionTexture.Width / 2, gravityExplosionTexture.Height / 2), explosion.Scale, SpriteEffects.None, 1.0f);
            }

            spriteBatch.End();

            if (bloom.Visible)
                bloom.BeginDraw();
            else
                GraphicsDevice.SetRenderTarget(null);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            rippleEffect.Parameters["RippleTexture"].SetValue(explosionRenderTarget);

            rippleEffect.CurrentTechnique.Passes[0].Apply();

            spriteBatch.Draw(mainRenderTarget, new Vector2(0, 0), Color.White);

            spriteBatch.DrawString(debugFont, debugText, new Vector2(10, 10), Color.White);

            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.GetViewMatrix(shipParallax));

            if (isSelecting)
            {
                spriteBatch.Draw(selectionTexture, selectionRect, null, Color.White * 0.3f, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            }

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
                        spriteBatch.Draw(sparkTexture, spark.Position, null, spark.Color * spark.Opacity, 0.0f, sparkOrigin, spark.Scale, SpriteEffects.None, 0.9f);
                }
            }
        }

        private void CreateShips()
        {
            for (int i = 0; i < numberShips; i++)
            {
                ships.Add(ShipFactory.CreateShip(ShipType.Fighter1, new Vector2(rand.Next(0, viewportRect.Width) - 2000, rand.Next(0, viewportRect.Height)), new Vector2(rand.Next(-1, 2), rand.Next(-1, 2))));
            }

            ships.Add(ShipFactory.CreateShip(ShipType.OrbitalCommand1, new Vector2(rand.Next(0, viewportRect.Width) - 2000, rand.Next(0, viewportRect.Height)), new Vector2(rand.Next(-1, 2), rand.Next(-1, 2))));
            ships.Add(ShipFactory.CreateShip(ShipType.OrbitalCommand1, new Vector2(rand.Next(0, viewportRect.Width) - 2000, rand.Next(0, viewportRect.Height)), new Vector2(rand.Next(-1, 2), rand.Next(-1, 2))));
            ships.Add(ShipFactory.CreateShip(ShipType.Dreadnaught, new Vector2(rand.Next(0, viewportRect.Width) - 2000, rand.Next(0, viewportRect.Height)), new Vector2(rand.Next(-1, 2), rand.Next(-1, 2))));

            foreach (Ship ship in ships)
            {
                ship.CreateGravityExplosion = CreateGravityExplosion;
            }
        }

        private void CreateShips2()
        {
            for (int i = 0; i < numberShips; i++)
            {
                ships2.Add(ShipFactory.CreateShip(ShipType.Fighter2, new Vector2(rand.Next(0, viewportRect.Width) + 2000, rand.Next(0, viewportRect.Height)), new Vector2(rand.Next(-1, 2), rand.Next(-1, 2))));
            }

            ships2.Add(ShipFactory.CreateShip(ShipType.OrbitalCommand2, new Vector2(rand.Next(0, viewportRect.Width) + 2000, rand.Next(0, viewportRect.Height)), new Vector2(rand.Next(-1, 2), rand.Next(-1, 2))));
            ships2.Add(ShipFactory.CreateShip(ShipType.OrbitalCommand2, new Vector2(rand.Next(0, viewportRect.Width) + 2000, rand.Next(0, viewportRect.Height)), new Vector2(rand.Next(-1, 2), rand.Next(-1, 2))));
            ships2.Add(ShipFactory.CreateShip(ShipType.Dreadnaught, new Vector2(rand.Next(0, viewportRect.Width) + 2000, rand.Next(0, viewportRect.Height)), new Vector2(rand.Next(-1, 2), rand.Next(-1, 2))));

            foreach (Ship ship in ships2)
            {
                ship.CreateGravityExplosion = CreateGravityExplosion;
            }
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

        private void UpdateGravityExplosions(float elapsed)
        {
            foreach (GravityExplosion explosion in gravityExplosions)
            {
                explosion.TTL -= elapsed;
                explosion.Scale += 2.0f * elapsed * explosion.Amplitude;
                explosion.Strength -= 0.05f * elapsed;
                if (explosion.Force > 0.0f)
                    explosion.Force -= 0.5f * elapsed;
                else
                    explosion.Force = 0.0f;

                List<Ship> allShips = new List<Ship>();
                allShips.AddRange(ships);
                allShips.AddRange(ships2);

                foreach (Ship ship in allShips)
                {
                    Vector2 distance = ship.Position - explosion.Position;
                    if (distance.Length() < gravityExplosionTexture.Width / 2 * explosion.Scale)
                    {
                        distance.Normalize();
                        ship.Velocity += distance / ship.Mass * explosion.Force;
                    }
                }
            }

            gravityExplosions.RemoveAll(explosion => explosion.TTL < 0.0f);
        }

        public void CreateGravityExplosion(Vector2 position, float amplitude)
        {
            gravityExplosions.Add(new GravityExplosion(new Vector2(position.X, position.Y), 4.0f, 0.01f, 0.05f, amplitude, 1.0f));
        }
    }
}
