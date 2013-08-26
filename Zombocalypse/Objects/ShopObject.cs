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
    class ShopObject : EnterableMapObject
    {
        public ShopObject(Texture2D currMapObjectTex, Vector2 currObjectPosition, ContentManager Content, int height, int width) 
            : base(currMapObjectTex, currObjectPosition, type.Shop)
        {
            locationMap = new MapLoader();
            locationMap.loadContent(Content, height, width);
            locationMap.loadMap("content/shopMap.txt");
        }

    }
}
