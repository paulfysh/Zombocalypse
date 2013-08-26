using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using Microsoft.Xna.Framework.Audio;

namespace Zombocalypse
{
    class Zombie
    {
        Vector2 zombieLoc;
        Texture2D zombieTexture;
        bool isDead;

        int moveSpeed;

        SoundEffect soundGroan;
        SoundEffectInstance groanInstance;

        public Zombie(ContentManager Content, Vector2 location)
        {
            zombieTexture = Content.Load<Texture2D>("Zombie");

            soundGroan = Content.Load<SoundEffect>("ZombieGroan");
            groanInstance = soundGroan.CreateInstance();

            isDead = false;
            zombieLoc = location;
            moveSpeed = 1;
        }

        public void drawZombie(SpriteBatch spriteBatch, Vector2 currentViewPort)
        {
            if (!isDead)
            {
                Vector2 renderPosition = new Vector2(zombieLoc.X - currentViewPort.X, zombieLoc.Y - currentViewPort.Y);
                spriteBatch.Draw(zombieTexture, renderPosition, Color.White);
            }
        }

        public Vector2 getLocation()
        {
            return zombieLoc;
        }

        public Texture2D getTexture()
        {
            return zombieTexture;
        }

        public void setDead()
        {
            isDead = true;
            groanInstance.Stop();
        }

        public void moveTowardsPlayer(Player thePlayer, Vector2 Viewport)
        {
            Vector2 preModifiedPlayerLoc = thePlayer.getCurrentPos();
            Vector2 playerLoc = new Vector2(preModifiedPlayerLoc.X + Viewport.X, preModifiedPlayerLoc.Y + Viewport.Y);


            if (playerLoc.X < zombieLoc.X)
            {
                zombieLoc.X -= moveSpeed;
            }
            else if (playerLoc.X > zombieLoc.X)
            {
                zombieLoc.X += moveSpeed;
            }

            if (playerLoc.Y < zombieLoc.Y)
            {
                zombieLoc.Y -= moveSpeed;
            }
            else if (playerLoc.Y > zombieLoc.Y)
            {
                zombieLoc.Y += moveSpeed;
            }

            //zombies get excited by food and groan.
            if ((playerLoc.X >= zombieLoc.X - 100) && (playerLoc.X <= zombieLoc.X + 100) &&
                (playerLoc.Y >= zombieLoc.Y - 100) && (playerLoc.Y <= zombieLoc.Y + 100))
            {
                if (thePlayer.getIsDead())
                {
                    groanInstance.Stop();
                }
                else if (groanInstance.State == SoundState.Stopped)
                {
                    groanInstance.Volume = 0.50f;
                    //groanInstance.IsLooped = false;
                    groanInstance.Play();
                }
            }
        }
    }
}
