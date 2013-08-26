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
    class resource
    {
        public enum resourceTypes
        {
            Wood = 0,
            Hatchet,
            Acorn,
            Spike,
            SpikePit,
            Tree,
            None
        };

        /*static public resource.resourceTypes resourceProvided(MapObject obj)
        {
            switch (obj.getType())
            {
                case MapObject.type.Tree:
                    return resourceTypes.Wood;
                    break;

            }

            return resourceTypes.None;
        }*/
    }
}
