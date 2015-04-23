using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace SpaceGame
{
    class Enemies
    {
        public Texture2D enemyShips;
        public Vector2 position;
        public Vector2 velocity;
        public Rectangle enemyBox;

        public bool isVisible = true;

        Random random = new Random();
        int randX, randY;

        public Enemies(Texture2D newTexture, Vector2 newPosition)
        {
            enemyShips = newTexture;
            position = newPosition;

            randY = random.Next(-4, 4);
            randX = random.Next(-6, -2);

            velocity = new Vector2(randX, randY);
        }

        public void Update(GraphicsDevice graphics)
        {
            position += velocity;

            if(position.Y <= 110.0f || position.Y >= 700.0f - enemyShips.Height)
            {
                velocity.Y = -velocity.Y;
            }

            if(position.X < 0 - enemyShips.Width)
            {
                isVisible = false;
            }

            UpdateEnemyBox();
        }

        public void Draw (SpriteBatch spriteBatch)
        {
            float scale = 1.0f;
            float rotation = -0;
            Vector2 origin = new Vector2(enemyShips.Width / 2, enemyShips.Height / 2);

            spriteBatch.Draw(enemyShips, position, new Rectangle(0, 0, 90, 54), Color.White, rotation, origin, scale, SpriteEffects.None, 0f);
        }

        public void UpdateEnemyBox()
        {
            enemyBox = new Rectangle((int)position.X, (int)position.Y, enemyShips.Width, enemyShips.Height);
        }
    }
}
