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
    class HouseObject : EnterableMapObject
    {
        public HouseObject(Texture2D currMapObjectTex, Vector2 currObjectPosition, ContentManager Content, int height, int width) 
            : base(currMapObjectTex, currObjectPosition, type.House)
        {
            locationMap = new MapLoader();
            locationMap.loadContent(Content, height, width);
            locationMap.loadMap("content/houseMap.txt");
        }
    }
}
