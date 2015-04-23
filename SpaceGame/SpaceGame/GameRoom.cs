using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace SpaceGame
{
    public class GameRoom : Game
    {
        enum GameState
        {
            StartMenu,
            Help,
            Loading,
            Playing,
            Paused
        }

        public GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Player player;
        List<Enemies> enemies = new List<Enemies>();
        public List<Bullet> bullets = new List<Bullet>();
        Random random = new Random();

        private Texture2D startButton;
        private Texture2D helpButton;
        private Texture2D exitButton;
        private Texture2D pauseButton;
        private Texture2D resumeButton;
        private Texture2D loadingScreen;
        private Texture2D loadingScreen1;
        private Texture2D loadingScreen2;
        private Texture2D loadingScreen3;
        public Texture2D earth;
        public Texture2D background;
        public Texture2D menuBackground;
        public Texture2D helpBackground;
        public Texture2D gameInterface;
        public Texture2D pausedScreen;

        private Vector2 startButtonPosition;
        private Vector2 helpButtonPosition;
        private Vector2 exitButtonPosition;
        private Vector2 resumeButtonPosition;

        private float _remainingDelay = 1.0f;
        private int loadingCount = 0;
        float spawn = 0;
        private GameState gameState;
        private Thread backgroundThread;
        private bool isLoading = false;
        MouseState mouseState;
        MouseState previousMouseState;

        public double score { get; set; }

        SpriteFont font;
        public GameRoom() : base()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Enable the mousepointer
            IsMouseVisible = true;

            // Set the position of the buttons
            startButtonPosition = new Vector2((GraphicsDevice.Viewport.Width / 2) - 50, 200);
            helpButtonPosition = new Vector2((GraphicsDevice.Viewport.Width / 2) - 50, 250);
            exitButtonPosition = new Vector2((GraphicsDevice.Viewport.Width / 2) - 50, 300);

            // Set the gamestate
            gameState = GameState.StartMenu;

            // Get the mousestate
            mouseState = Mouse.GetState();
            previousMouseState = mouseState;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Load up our Content
            startButton = Content.Load<Texture2D>("start");
            helpButton = Content.Load<Texture2D>("help");
            exitButton = Content.Load<Texture2D>("exit");
            menuBackground = Content.Load<Texture2D>("menuBackground");
            helpBackground = Content.Load<Texture2D>("helpBackground");
            loadingScreen = Content.Load<Texture2D>("loading1");
            loadingScreen1 = Content.Load<Texture2D>("loading2");
            loadingScreen2 = Content.Load<Texture2D>("loading3");
            loadingScreen3 = Content.Load<Texture2D>("loading4");
            font = Content.Load<SpriteFont>("SpriteFont1");
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (gameState == GameState.StartMenu)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed)
                {
                   gameState = GameState.Loading;

                   // Set backgroundThreadd
                   backgroundThread = new Thread(LoadGame);

                   // Start backgroundThread
                   isLoading = true;
                   backgroundThread.Start();
                }

                if (GamePad.GetState(PlayerIndex.One).Buttons.X == ButtonState.Pressed)
                {
                    gameState = GameState.Help;
                }
            }

            if (gameState == GameState.Help)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.B == ButtonState.Pressed)
                {
                    gameState = GameState.StartMenu;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Back))
                {
                    gameState = GameState.StartMenu;
                }
            }

            if (gameState == GameState.Playing)
            {
                spawn += (float)gameTime.ElapsedGameTime.TotalSeconds;

                foreach(Enemies enemy in enemies)
                {
                    enemy.Update(graphics.GraphicsDevice);
                    for (int i = 0; i < bullets.Count; i++)
                    {
                        if (enemy.enemyBox.Intersects(bullets[i].bulletBox))
                        {
                            bullets[i].isVisible = false;
                            enemy.isVisible = false;
                            score += 10;
                        }
                    }
                    
                    
                }

                if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    gameState = GameState.Paused;
                }
            }

            if (gameState == GameState.Paused)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.B == ButtonState.Pressed)
                {
                    gameState = GameState.Playing;
                }
            }

            // The time since Update was called last.
            //float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Load the game when needed
            if (gameState == GameState.Loading)
            {
                // Set backgroundThreadd
                backgroundThread = new Thread(LoadGame);

                isLoading = true;

                // Start backgroundThread
                backgroundThread.Start();
            }

            // Wait for mouseClick
            mouseState = Mouse.GetState();

            if (previousMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released)
            {
                MouseClicked(mouseState.X, mouseState.Y);
            }

            previousMouseState = mouseState;

            if (gameState == GameState.Playing && isLoading)
            {
                LoadGame();

                isLoading = false;
            }

            if (player != null)
            {
                player.Update();
            }

            UpdateBullets();
            LoadEnemies();
            base.Update(gameTime);
        }

        public void UpdateBullets()
        {
            foreach(Bullet bullet in bullets)
            {
                bullet.position += bullet.velocity;

                if(Vector2.Distance(bullet.position, player.playerPosition) > 740)
                {
                    bullet.isVisible = false;
                }

                bullet.UpdateBulletBox();
            }

            for(int i = 0; i < bullets.Count; i++)
            {
                if(!bullets[i].isVisible)
                {
                    bullets.RemoveAt(i);
                    i--;
                }
            }
        }

        public void LoadEnemies()
        {
            int randY = random.Next(200, 600);

            if(spawn >= 1)
            {
                spawn = 0;
                if(enemies.Count() <15)
                {
                    enemies.Add(new Enemies(Content.Load<Texture2D>("enemyShip"), new Vector2(1300, randY)));
                }
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                if (!enemies[i].isVisible)
                {
                    enemies.RemoveAt(i);
                    i--;
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            var timer = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (isLoading)
            {
                Console.WriteLine(_remainingDelay);
                Console.WriteLine(loadingCount);

                _remainingDelay -= timer;

                if (_remainingDelay <= 0)
                {
                    loadingCount++;
                    if (loadingCount > 3) loadingCount = 0;

                    _remainingDelay = 0.3f;
                }
            }

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            // Draw the start menu
            if (gameState == GameState.StartMenu)
            {
                spriteBatch.Draw(menuBackground, new Rectangle(0, 0, 1280, 720), Color.White);
                spriteBatch.Draw(startButton, startButtonPosition, Color.White);
                spriteBatch.Draw(helpButton, helpButtonPosition, Color.White);
                spriteBatch.Draw(exitButton, exitButtonPosition, Color.White);
            }

            if (gameState == GameState.Help)
            {
                spriteBatch.Draw(helpBackground, new Rectangle(0, 0, 1280, 720), Color.White);
            }

            // Show loading screens
            if (gameState == GameState.Loading)
            {
                switch(loadingCount)
                {
                    case 0:
                    {
                        spriteBatch.Draw(loadingScreen, new Rectangle(0, 0, 1280, 720), Color.White);
                        break;
                    }
                    case 1:
                    {
                        spriteBatch.Draw(loadingScreen1, new Rectangle(0, 0, 1280, 720), Color.White);
                        break;
                    }
                    case 2:
                    {
                        spriteBatch.Draw(loadingScreen2, new Rectangle(0, 0, 1280, 720), Color.White);
                        break;
                    }
                    case 3:
                    {
                        spriteBatch.Draw(loadingScreen3, new Rectangle(0, 0, 1280, 720), Color.White);
                        break;
                    }
                }
            }

            // Draw the game objects
            if (gameState == GameState.Playing)
            {
                // Draw our background
                spriteBatch.Draw(background, new Rectangle(0, 0, 1280, 720), Color.White);
                spriteBatch.Draw(gameInterface, new Rectangle(0, 0, 1280, 720), Color.White);
                spriteBatch.Draw(earth, new Rectangle(50, 50, 300, 300), Color.White);
                
                foreach (Enemies enemy in enemies)
                {
                    enemy.Draw(spriteBatch);
                }

                // Pause button
                spriteBatch.Draw(pauseButton, new Vector2(0, 0), Color.White);

                player.Draw(spriteBatch);

                spriteBatch.DrawString(font, score.ToString(), new Vector2(550, -1),Color.White);
               
            }

            // Draw the pause button
            if (gameState == GameState.Paused)
            {
                spriteBatch.Draw(pausedScreen, new Rectangle(0, 0, 1280, 720), Color.White);
                spriteBatch.Draw(resumeButton, resumeButtonPosition, Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        void MouseClicked (int x, int y)
        {
           // Creates a rectangle 10x10 around mouseClickPosition
           Rectangle mouseClickRect = new Rectangle(x, y, 10, 10);

           // Check startmenu
           if (gameState == GameState.StartMenu)
           {
               Rectangle startButtonRect = new Rectangle((int)startButtonPosition.X, (int) startButtonPosition.Y, 100, 20);

               Rectangle helpButtonRect = new Rectangle((int)helpButtonPosition.X, (int)helpButtonPosition.Y, 100, 20);

               Rectangle exitButtonRect = new Rectangle((int)exitButtonPosition.X, (int) exitButtonPosition.Y, 100, 20);

               if (mouseClickRect.Intersects(startButtonRect))
               {
                   gameState = GameState.Loading;

                   // Set backgroundThreadd
                   backgroundThread = new Thread(LoadGame);

                   // Start backgroundThread
                   isLoading = true;
                   backgroundThread.Start();
               }

               if (mouseClickRect.Intersects(helpButtonRect))
               {
                   gameState = GameState.Help;
               }

               else if (mouseClickRect.Intersects(exitButtonRect))
               {
                   Exit();
               }
           }

            // Check pauseButton
            if (gameState == GameState.Playing)
            {
                Rectangle pauseButtonRect = new Rectangle(0, 0, 70, 70);

                if (mouseClickRect.Intersects(pauseButtonRect))
                {
                    gameState = GameState.Paused;
                }
            }

            // Check resumeButton
            if (gameState == GameState.Paused)
            {
                Rectangle resumeButtonRect = new Rectangle((int)resumeButtonPosition.X, (int)resumeButtonPosition.Y, 100, 20);

                if (mouseClickRect.Intersects(resumeButtonRect))
                {
                    gameState = GameState.Playing;
                }
            }
        }

        void LoadGame()
        {
            // Load the game images into the content pipeline
            pauseButton = Content.Load<Texture2D>("pause");
            resumeButton = Content.Load<Texture2D>("resume");
            pausedScreen = Content.Load<Texture2D>("pauseScreen");
            background = Content.Load<Texture2D>("space");
            gameInterface = Content.Load<Texture2D>("gameInterface");
            earth = Content.Load<Texture2D>("pixelEarth");

            // Load our content
            //font = Content.Load<SpriteFont>("monoGameFont");
            player = new Player(this, Content.Load<Texture2D>("spaceship"));

            resumeButtonPosition = new Vector2((GraphicsDevice.Viewport.Width / 2) - (resumeButton.Width / 2), (GraphicsDevice.Viewport.Height / 2) - (resumeButton.Height / 2));

            // Wait for 3 seconds
            Thread.Sleep(4000);

            // Start playing
            gameState = GameState.Playing;

            isLoading = false;
        }
    }
}
