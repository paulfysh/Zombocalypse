using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace Zombocalypse.Objects
{
    class TreeObject : ConsumableMapObject
    {
        public TreeObject(Texture2D currMapObjectTex, Vector2 currObjectPosition, type currObjectType)
            : base(currMapObjectTex, currObjectPosition, currObjectType)
        {
            resourceProvided.Add(resource.resourceTypes.Wood);
            resourceProvided.Add(resource.resourceTypes.Acorn);
            //might want an acorn as well. That'll allow replanting trees. 
            
            quantityOfResourceProvided = 10;

            resourceRequiredToHarvest.Add(resource.resourceTypes.Hatchet);
        }
    }
}
