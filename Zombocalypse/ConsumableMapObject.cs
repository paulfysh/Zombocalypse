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
    //this is really for consumable map objects
    class ConsumableMapObject
    {
        Texture2D objectTexture;
        Vector2 objectPosition;
        type objectType;

        bool destroyed = false;
        

        protected List<resource.resourceTypes> resourceProvided = new List<resource.resourceTypes>();
        protected List<resource.resourceTypes> resourceRequiredToHarvest = new List<resource.resourceTypes>();
        protected int quantityOfResourceProvided;

        public enum type
        {
            Tree=0
        };

        public ConsumableMapObject(Texture2D currMapObjectTex, Vector2 currObjectPosition, type currObjectType)
        {
            objectTexture = currMapObjectTex;
            objectPosition = currObjectPosition;
            objectType = currObjectType;
        }

        public Vector2 getLocation()
        {
            return objectPosition;
        }

        public Texture2D getTexture()
        {
            return objectTexture;
        }

        public void draw(SpriteBatch spriteBatch, Vector2 currentViewPort)
        {
            Vector2 renderPosition = new Vector2(objectPosition.X - currentViewPort.X, objectPosition.Y - currentViewPort.Y);
            spriteBatch.Draw(objectTexture, renderPosition, Color.White);
        }

        public type getType()
        {
            return objectType;
        }

        public List<resource.resourceTypes> getTypesOfResoucesProvided()
        {
            return resourceProvided;
        }

        public int getAmountOfResourceProvided()
        {
            return quantityOfResourceProvided;
        }

        public List<resource.resourceTypes> getToolsRequiredForHarvesting()
        {
            return resourceRequiredToHarvest;
        }

        public bool isDestroyed()
        {
            return destroyed;
        }

        public void Destroy()
        {
            destroyed = true;
        }


    }
}
