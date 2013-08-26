using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace Zombocalypse
{
    class Player
    {
        Vector2 playerLoc = new Vector2(100, 100);
        Texture2D playerTexture;
        

        ConsumableMapObject adjacentObject;

        inventory inv;
        bool drawInventory = false;

        bool isDead;

        int movementVal = 3;

        public enum direction
        {
            Up = 0,
            Down,
            Left,
            Right
        };

        public Player(ContentManager Content)
        {
            playerTexture = Content.Load<Texture2D>("player");
            
            inv = new inventory(Content);

            //inv.addToInventory(resource.resourceTypes.Wood, 1);
            inv.addToInventory(resource.resourceTypes.Hatchet, 1);

            isDead = false;
        }

        public void drawPlayer(SpriteBatch spriteBatch)
        {
            if (!isDead)
            {
                spriteBatch.Draw(playerTexture, new Vector2(playerLoc.X, playerLoc.Y), Color.White);

                if (drawInventory)
                {
                    inv.draw(spriteBatch, playerLoc);
                }
            }
        }

        public void setAdjacantObject(ConsumableMapObject obj)
        {
            adjacentObject = obj;
        }

        public Vector2 getNextPos(direction dir)
        {
            Vector2 nextPos = new Vector2(playerLoc.X, playerLoc.Y);

            switch (dir)
            {
                case direction.Up:
                    nextPos.Y -= movementVal;
                    break;
                case direction.Down:
                    nextPos.Y += movementVal;
                    break;
                case direction.Left:
                    nextPos.X -= movementVal;
                    break;
                case direction.Right:
                    nextPos.X += movementVal;
                    break;
            }

            return nextPos;
        }

        public Vector2 getCurrentPos()
        {
            return playerLoc;
        }

        public void move(direction dir)
        {
            switch (dir)
            {
                case direction.Up:
                    playerLoc.Y -= movementVal;
                    break;
                case direction.Down:
                    playerLoc.Y += movementVal;
                    break;
                case direction.Left:
                    playerLoc.X -= movementVal;
                    break;
                case direction.Right:
                    playerLoc.X += movementVal;
                    break;
            }
        }

        public resource.resourceTypes getCurrentWeapon()
        {
            return inv.getCurrentWeapon();
        }

        public Texture2D getTextureForCurrentWeapon()
        {
            return inv.getCurrentWeaponTexture();
        }

        public void setDead()
        {
            isDead = true;
        }

        public bool getIsDead()
        {
            return isDead;
        }

        public void interactWithAdjacentObj()
        {
            if (adjacentObject != null)
            {
                //do stuff
                foreach (resource.resourceTypes res in adjacentObject.getToolsRequiredForHarvesting())
                {
                    if (inv.contains(res, 1))
                    {
                        foreach (resource.resourceTypes addRes in adjacentObject.getTypesOfResoucesProvided())
                        {
                            inv.addToInventory(addRes, adjacentObject.getAmountOfResourceProvided());
                        }
                    }
                }

                adjacentObject.Destroy();
                adjacentObject = null;
            }
        }

        public Texture2D getTexture()
        {
            return playerTexture;
        }

        public void openCloseInventory()
        {
            drawInventory = !drawInventory;
        }

        public bool isInventoryOpen()
        {
            return drawInventory;
        }

        public inventory getInventory()
        {
            return inv;
        }

        //this is used when we exit somewhere so we can put ourselves in the correct position. 
        public void setPosition(Vector2 newPosition)
        {
            playerLoc = newPosition;
        }

        public void handleInventoryInput(KeyboardState keybstate)
        {
            inv.handleKeyPress(keybstate);
        }
    }
}
