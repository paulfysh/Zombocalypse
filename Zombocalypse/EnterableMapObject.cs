using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace Zombocalypse
{
    class EnterableMapObject
    {
        public enum type
        {
            House,
            Shop
        };

        Vector2 objectPosition;
        EnterableMapObject.type objectType;
        Texture2D objectTexture;

        protected MapLoader locationMap;

        bool inside;

        public EnterableMapObject(Texture2D currMapObjectTex, Vector2 currObjectPosition, EnterableMapObject.type currObjectType)
        {
            objectPosition = currObjectPosition;
            objectType = currObjectType;
            objectTexture = currMapObjectTex;
            inside = false;
        }

        public void draw(SpriteBatch spriteBatch, Vector2 currentViewPort)
        {
            if (inside)
            {
                locationMap.drawMap(spriteBatch);
            }
            else
            {
                Vector2 renderPosition = new Vector2(objectPosition.X - currentViewPort.X, objectPosition.Y - currentViewPort.Y);
                spriteBatch.Draw(objectTexture, renderPosition, Color.White);
            }
        }

        public bool canEnterFromPosition(Vector2 positionToEnterFrom)
        {
            float bottomLeftPos = (objectPosition.X);
            float bottomRightPos = (objectPosition.X + objectTexture.Width);
            float bottomPos = objectPosition.Y + objectTexture.Height;

            if (positionToEnterFrom.X >= bottomLeftPos &&
                positionToEnterFrom.X <= bottomRightPos &&
                positionToEnterFrom.Y >= bottomPos - 10 &&
                positionToEnterFrom.Y <= bottomPos + 10)
            {
                return true;
            }
            return false;
        }

        public void enter()
        {
            inside = true;
        }

        public void exit()
        {
            inside = false;
        }

        public Vector2 getLocation()
        {
            return objectPosition;
        }

        public Texture2D getTexture()
        {
            return objectTexture;
        }

        public MapLoader getMap()
        {
            return locationMap;
        }
    }
}
