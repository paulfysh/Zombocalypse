using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

using Zombocalypse.Objects;

namespace Zombocalypse
{
    class MapLoader
    {
        ContentManager Content;

        Texture2D grassTexture; 
        Texture2D houseTexture;
        Texture2D shopTexture;
        Texture2D treeTexture;
        Texture2D woodenTileTexture;
        Texture2D doorTileTexture;
        Texture2D spikePitTexture;
        Texture2D occupiedSpikePitTexture;

        Vector2 exitPos;

        int screenHeight = 0;
        int screenWidth = 0;

        int currentViewX = 0;
        int currentViewY = 0;

        bool firstDraw = true;

        int numColumns = 0;
        int numRows = 0;

        Vector2 maxMapWidthHeight;

        List<ConsumableMapObject> nonEnterableMapObjects = new List<ConsumableMapObject>();
        List<EnterableMapObject> enterableMapObjects = new List<EnterableMapObject>();
        
        //hack for time would usually do like the above.
        List<Vector2> traps = new List<Vector2>();
        List<Vector2> expiredTraps = new List<Vector2>();

        List<char[]> lines = new List<char[]>();

        struct intPair
        {
            public int first;
            public int second;

            public intPair(int x, int y)
            {
                first = x;
                second = y;
            }
        }

        ArrayList doNotRender = new ArrayList();

        public void loadContent(ContentManager Content, int height, int width)
        {
            this.Content = Content;
            grassTexture = Content.Load<Texture2D>("grass");
            houseTexture = Content.Load<Texture2D>("house");
            shopTexture = Content.Load<Texture2D>("shopExported");
            treeTexture = Content.Load <Texture2D>("tree2");
            woodenTileTexture = Content.Load<Texture2D>("woodenTile");
            doorTileTexture = Content.Load<Texture2D>("doorTile");
            spikePitTexture = Content.Load<Texture2D>("unoccupiedSpikePitTile");
            occupiedSpikePitTexture = Content.Load<Texture2D>("occupiedSpikePitTile");

            screenHeight = height;
            screenWidth = width;
        }

        public void loadMap(string mapName)
        {
            TextReader reader = new StreamReader(mapName);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                char[] currLine = line.ToCharArray();
                lines.Add(currLine);
            }

            firstMapParse();
        }


        //call this once when we load the map to find the locations of all the game objects that are stored and drawn seperately
        //but not draw anything. 
        private void firstMapParse()
        {
            int rowNo = 0;
            foreach (char[] line in lines)
            {
                int columnNo = 0;
                foreach (char currentItem in line)
                {
                    if (currentItem == 'S' || currentItem == 'H')
                    {
                        addBuilding(currentItem, columnNo, rowNo);
                    }
                    else if (currentItem == 'T')
                    {
                        addTree(columnNo, rowNo);
                    }
                    columnNo += 1;
                }
                numColumns = columnNo;

                rowNo += 1;
            }
            numRows = rowNo;

           maxMapWidthHeight = new Vector2((numColumns * grassTexture.Width), (numRows * grassTexture.Height));
        }

        public void drawMap(SpriteBatch spriteBatch)
        {
            //nonEnterableMapObjects.Clear();
            int rowNo = 0;
            foreach (char[] line in lines)
            {
                int columnNo = 0; 
                foreach (char currentItem in line)
                {
                    //grass tile, we dont need to create an object just render it. 
                    if (renderable(columnNo, rowNo))
                    {
                        Vector2 position = new Vector2(columnNo * grassTexture.Width - currentViewX, rowNo * grassTexture.Height - currentViewY);

                        if (currentItem == 'G')
                        {
                            spriteBatch.Draw(grassTexture, position, Color.White);
                        }
                        else if(currentItem == 'W')
                        {
                            spriteBatch.Draw(woodenTileTexture, position, Color.White);
                        }
                        else if (currentItem == 'E')
                        {
                            //do something to add the exit
                            exitPos = position;
                            spriteBatch.Draw(doorTileTexture, position, Color.White);
                        }
                        else if (currentItem == 'T')
                        {
                            spriteBatch.Draw(grassTexture, position, Color.White);
                        }
                    }
                    columnNo += 1;
                }
                rowNo += 1;
            }

            drawExpirableObjects(spriteBatch);
            drawEnterableObjects(spriteBatch);
            drawTraps(spriteBatch);
            drawExpiredTraps(spriteBatch);
            firstDraw = false;
            doNotRender.Clear();
        }

        public void removeOldestExpiredTrap()
        {
            if (expiredTraps.Count > 0)
            {
                expiredTraps.RemoveAt(0);
            }
        }

        public List<ConsumableMapObject> getNonEnterableObjects()
        {
            return nonEnterableMapObjects;
        }

        public List<EnterableMapObject> getEnterableObjects()
        {
            return enterableMapObjects;
        }

        //hack for time.
        public List<Vector2> getTraps()
        {
            return traps;
        }

        public Texture2D getTrapTexture()
        {
            return spikePitTexture;
        }

        private bool renderable(int columnNo, int rowNo)
        {
            foreach(intPair ignoreTiles in doNotRender)
            {
                if ((ignoreTiles.first == columnNo) && ignoreTiles.second == rowNo)
                {
                    return false;
                }
            }

            //check if we are going to render inside the screen, no point doing it if we are outside the viewport.
            if ((columnNo * grassTexture.Width) > (currentViewX + screenWidth) || (columnNo * grassTexture.Width) < (currentViewX - grassTexture.Width))
            {
                return false;
            }

            if ((rowNo * grassTexture.Height) > (currentViewY + screenHeight) || (rowNo * grassTexture.Height) < (currentViewY - grassTexture.Height))
            {
                return false;
            }

            return true;
        }

        public bool checkAndMoveViewport(Vector2 positionToMoveTo, Vector2 lastPosition)
        {
            if (positionToMoveTo.X > (screenWidth / 2) && lastPosition.X < positionToMoveTo.X)
            {
                //move screen
                float difference = positionToMoveTo.X - lastPosition.X;
                currentViewX += (int)difference;

                return insideMapBounds();
            }

            if (positionToMoveTo.X < (screenWidth / 2) && lastPosition.X > positionToMoveTo.X)
            {
                float difference = positionToMoveTo.X - lastPosition.X;
                currentViewX += (int)difference;

                return insideMapBounds();
            }

            if (positionToMoveTo.Y > (screenHeight / 2) && lastPosition.Y < positionToMoveTo.Y)
            {
                // move screen
                float difference = positionToMoveTo.Y - lastPosition.Y;
                currentViewY += (int)difference;

                return insideMapBounds();
            }

            if (positionToMoveTo.Y < (screenHeight / 2) && lastPosition.Y > positionToMoveTo.Y)
            {
                // move screen
                float difference = positionToMoveTo.Y - lastPosition.Y;
                currentViewY += (int)difference;

                return insideMapBounds();
            }

            return false;
        }

        private bool insideMapBounds()
        {
            int maxX = ((int)maxMapWidthHeight.X - screenWidth);
            int maxY = ((int)maxMapWidthHeight.Y - screenHeight);

            if (maxX < 0)
            {
                maxX = 0;
            }

            if (maxY < 0)
            {
                maxY = 0;
            }

            if (currentViewX < 0)
            {
                currentViewX = 0;
                return false;
            }
            else if (currentViewX > maxX)
            {
                currentViewX = maxX;
                return false;
            }

            if (currentViewY < 0)
            {
                currentViewY = 0;
                return false;
            }
            else if (currentViewY > maxY)
            {
                currentViewY = maxY;
                return false;
            }

            return true;
        }

        public bool moveWillExitMap(Vector2 position, Texture2D texture)
        {
            if (position.X + texture.Width > maxMapWidthHeight.X ||
                position.Y + texture.Height > maxMapWidthHeight.Y)
            {
                return true;
            }

            return false;
        }

        public Vector2 getMaxMapWidthHeight()
        {
            return maxMapWidthHeight;
        }


        private void addBuilding(char buildingType, int columnNo, int rowNo)
        {
            switch (buildingType)
            {
                case 'S':
                    if (lines[rowNo][columnNo + 1] == 'S' && lines[rowNo + 1][columnNo] == 'S' && lines[rowNo + 1][columnNo + 1] == 'S')
                    {
                        //blank out the other 4 tiles, the map assume 25x25 but the image in this case is 100x100 so we want to make sure we dont
                        //render anything over the top.

                        
                        Vector2 position = new Vector2((columnNo * grassTexture.Width) - currentViewX, (rowNo * grassTexture.Height) - currentViewY);

                        enterableMapObjects.Add(new ShopObject(shopTexture, position, Content, screenHeight, screenWidth));

                        //spriteBatch.Draw(shopTexture, position, Color.White);

                        doNotRender.Add(new intPair(columnNo + 1, rowNo));
                        doNotRender.Add(new intPair(columnNo, rowNo + 1));
                        doNotRender.Add(new intPair(columnNo + 1, rowNo + 1));
                    }
                    break;
                case 'H':
                    if (lines[rowNo][columnNo + 1] == 'H' && lines[rowNo + 1][columnNo] == 'H' && lines[rowNo + 1][columnNo + 1] == 'H')
                    {
                        //blank out the other 4 tiles, the map assume 25x25 but the image in this case is 100x100 so we want to make sure we dont
                        //render anything over the top.
                        Vector2 position = new Vector2((columnNo * grassTexture.Width) - currentViewX, (rowNo * grassTexture.Height) - currentViewY);

                        //spriteBatch.Draw(houseTexture, position, Color.White);
                        enterableMapObjects.Add(new HouseObject(houseTexture, position, Content, screenHeight, screenWidth));

                        doNotRender.Add(new intPair(columnNo + 1, rowNo));
                        doNotRender.Add(new intPair(columnNo, rowNo + 1));
                        doNotRender.Add(new intPair(columnNo + 1, rowNo + 1));
                    }
                    break;
            }
        }

        private void addTree(int columnNo, int rowNo)
        {
            Vector2 position = new Vector2(columnNo * grassTexture.Width, rowNo * grassTexture.Height);

            nonEnterableMapObjects.Add(new TreeObject(treeTexture, position, ConsumableMapObject.type.Tree));

            //we want our tree rendered on grass so render grass then a tree on top, transparancy will do the rest.
            //spriteBatch.Draw(grassTexture, position, Color.White);
            //spriteBatch.Draw(treeTexture, position, Color.White);
        }

        private void drawExpirableObjects(SpriteBatch spriteBatch)
        {
            List<ConsumableMapObject> deleteObjs = new List<ConsumableMapObject>();

            foreach (ConsumableMapObject obj in nonEnterableMapObjects)
            {
                //draw some grass under the object
                if (obj.isDestroyed())
                {
                    deleteObjs.Add(obj);
                }

                //spriteBatch.Draw(grassTexture, obj.getLocation(), Color.White);
                obj.draw(spriteBatch, new Vector2(currentViewX, currentViewY));
            }

            //destroy the items you have harevested. 
            foreach (ConsumableMapObject obj in deleteObjs)
            {
                nonEnterableMapObjects.Remove(obj);
            }
        }

        private void drawEnterableObjects(SpriteBatch spriteBatch)
        {
            foreach (EnterableMapObject obj in enterableMapObjects)
            {
                obj.draw(spriteBatch, new Vector2(currentViewX, currentViewY));
            }
        }

        private void drawTraps(SpriteBatch spriteBatch)
        {
            foreach (Vector2 pos in traps)
            {
                Vector2 actualPos = new Vector2(pos.X - currentViewX, pos.Y - currentViewY);
                spriteBatch.Draw(spikePitTexture, actualPos, Color.White);
            }
        }

        private void drawExpiredTraps(SpriteBatch spriteBatch)
        {
            foreach (Vector2 pos in expiredTraps)
            {
                Vector2 actualPos = new Vector2(pos.X - currentViewX, pos.Y - currentViewY);
                spriteBatch.Draw(occupiedSpikePitTexture, actualPos, Color.White);
            }
        }

        public Vector2 getViewport()
        {
            return new Vector2(currentViewX, currentViewY);
        }

        public Vector2 getExitPos()
        {
            return exitPos;
        }

        public Texture2D getExitTexture()
        {
            return doorTileTexture;
        }

        public void addTrapToMap(Vector2 position)
        {
            int rowNo = (int)(position.X / grassTexture.Width);
            int colNo = (int)(position.Y / grassTexture.Height);
            
            traps.Add(position);
        }

        public void removeTrap(Vector2 trap)
        {
            traps.Remove(trap);
            expiredTraps.Add(trap);
        }

        public void addTree(Vector2 position)
        {
            int rowNo = (int)(position.Y / grassTexture.Height);
            int colNo = (int)(position.X / grassTexture.Width);
            addTree(colNo, rowNo);
        }
    }
}
