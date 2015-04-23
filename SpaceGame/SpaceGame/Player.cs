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
    public class Player
    {
        private GamePadState oldState;
        public Vector2 playerPosition { get; set; }
        public float rotation;
        public Vector2 playerOrigin;

        public GameRoom gameRoom;
        public Texture2D playerShip;
        public Rectangle playerRectangle;

        public Player(GameRoom G, Texture2D S) 
        {
            gameRoom = G;
            playerShip = S;
        }

        public void Update()
        {
            var currGamePadState = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);
            GamePadState newState = GamePad.GetState(PlayerIndex.One);

            if (currGamePadState.IsConnected)
            {   
                playerPosition += new Vector2(currGamePadState.ThumbSticks.Left.X * 5f, -currGamePadState.ThumbSticks.Left.Y * 5f);

                if(currGamePadState.Triggers.Left == 1.0f)
                {
                    playerPosition += new Vector2(currGamePadState.ThumbSticks.Left.X * 8f, -currGamePadState.ThumbSticks.Left.Y * 8f);
                }

                if (newState.Triggers.Right == 1.0f && oldState.Triggers.Right <= 0.9f)
                {
                    Shoot();
                }

                oldState = newState;
            }

            else
            {
                KeyboardState state = Keyboard.GetState();

                if (state.IsKeyDown(Keys.A))
                {
                    playerPosition -= new Vector2(5f, 0f);

                    if (state.IsKeyDown(Keys.LeftShift))
                    {
                        playerPosition -= new Vector2(8f, 0f);
                    }
                }
                
                if (state.IsKeyDown(Keys.D))
                {
                    playerPosition += new Vector2(5f, 0f);

                    if (state.IsKeyDown(Keys.LeftShift))
                    {
                        playerPosition += new Vector2(8f, 0f);
                    }
                }
                
                if (state.IsKeyDown(Keys.W))
                {
                    playerPosition -= new Vector2(0f, 5f);

                    if (state.IsKeyDown(Keys.LeftShift))
                    {
                        playerPosition -= new Vector2(0f, 8f);
                    }
                }
                
                if (state.IsKeyDown(Keys.S))
                {
                    playerPosition += new Vector2(0f, 5f);

                    if (state.IsKeyDown(Keys.LeftShift))
                    {
                        playerPosition += new Vector2(0f, 8f);
                    }
                }
            }

            if (playerPosition.Y <= 100f)
            {
                playerPosition += new Vector2(0f, 15f);
            }

            if (playerPosition.Y >= 700f)
            {
                playerPosition -= new Vector2(0f, 15f);
            }

            if (playerPosition.X <= 20f)
            {
                playerPosition += new Vector2(15f, 0f);
            }

            if (playerPosition.X >= 1200f)
            {
                playerPosition -= new Vector2(15f, 0f);
            }
        }

        public void Shoot()
        {

            Bullet newBullet = new Bullet(gameRoom.Content.Load<Texture2D>("playerLasers2"));
            newBullet.velocity += new Vector2 (4.0f, 0.0f);
            newBullet.position = playerPosition + newBullet.velocity * 5.0f;
            newBullet.isVisible = true;
            newBullet.UpdateBulletBox();

            gameRoom.bullets.Add(newBullet);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            float scale = 0.5f;
            float rotation = 1.57f;
            Vector2 origin = new Vector2(playerShip.Width / 2, playerShip.Height / 2);

            foreach(Bullet bullet in gameRoom.bullets)
            {
                bullet.Draw(spriteBatch);
            }

            spriteBatch.Draw(playerShip, playerPosition, new Rectangle(0, 0, 173, 291), Color.White, rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
