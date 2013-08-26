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
    class ConstructionMenu
    {
        inventory playerInv;
        MapLoader currMap;
        Player currPlayer;

        Vector2 location;

        Texture2D background;
        Texture2D itemSlot;
        Texture2D selectedItemSlot;
        Texture2D inventoryDialog;

        Texture2D Spike;
        Texture2D SpikePit;
        Texture2D Tree;

        SpriteFont font;

        List<constructionItem> items = new List<constructionItem>();

        struct constructionItem
        {
            public resource.resourceTypes type;
            String name;
            public List<Tuple<resource.resourceTypes, int>> cost;
            public bool appearsInInventory;
            public bool appearsOnMap;
            public bool selected;


            public constructionItem(resource.resourceTypes itemType, List<Tuple<resource.resourceTypes, int>> itemCost, bool itemAppearsInInventory, bool itemAppearsOnMap, bool itemIsSelected = false)
            {
                type = itemType;
                cost = itemCost;
                appearsInInventory = itemAppearsInInventory;
                appearsOnMap = itemAppearsOnMap;
                name = "";
                selected = itemIsSelected;

                name = lookupNameForType(itemType);
            }

            public void setSelected(bool sel)
            {
                selected = sel;
            }


            public string lookupNameForType(resource.resourceTypes typeToLookup)
            {
                switch (typeToLookup)
                {
                    case resource.resourceTypes.Spike:
                        return "Wooden Spike";
                    case resource.resourceTypes.SpikePit:
                        return "Wooden Spike Pit";
                    case resource.resourceTypes.Tree:
                        return "Tree";
                }
                return "";
            }
        }


        public ConstructionMenu(Vector2 renderPosition, ContentManager Content, inventory playerInvIn, MapLoader currMapIn, Player currPlayerIn)
        {
            location = renderPosition;

            List<Tuple<resource.resourceTypes, int>> spikeResources = new List<Tuple<resource.resourceTypes, int>>();
            spikeResources.Add(new Tuple<resource.resourceTypes, int>(resource.resourceTypes.Wood, 1));
            items.Add(new constructionItem(resource.resourceTypes.Spike, spikeResources, true, false, true));

            List<Tuple<resource.resourceTypes, int>> spikePitResources = new List<Tuple<resource.resourceTypes, int>>();
            spikePitResources.Add(new Tuple<resource.resourceTypes, int>(resource.resourceTypes.Spike, 5));
            items.Add(new constructionItem(resource.resourceTypes.SpikePit, spikePitResources, false, true));

            List<Tuple<resource.resourceTypes, int>> treeResources = new List<Tuple<resource.resourceTypes, int>>();
            treeResources.Add(new Tuple<resource.resourceTypes, int>(resource.resourceTypes.Acorn, 1));
            items.Add(new constructionItem(resource.resourceTypes.Tree, treeResources, false, true));

            background = Content.Load<Texture2D>("InventoryBackground");
            itemSlot = Content.Load<Texture2D>("inventorySquare");
            selectedItemSlot = Content.Load<Texture2D>("selectedInventorySquare");
            inventoryDialog = Content.Load<Texture2D>("inventoryDialogBox");

            Spike = Content.Load<Texture2D>("woodenStake");
            SpikePit = Content.Load<Texture2D>("spikepitTile");
            Tree = Content.Load<Texture2D>("treeIcon");

            font = Content.Load<SpriteFont>("SpriteFont1");

            playerInv = playerInvIn;
            currMap = currMapIn;
            currPlayer = currPlayerIn;
        }

        public void draw(SpriteBatch spriteBatch)
        {
            Vector2 menuLocation = new Vector2(location.X, location.Y + inventoryDialog.Height);

            spriteBatch.Draw(inventoryDialog, location, Color.White);
            spriteBatch.Draw(background, menuLocation, Color.White);

            drawSelectedItemInfo(spriteBatch);

            drawItems(spriteBatch, menuLocation);

            for (int i = 0; i < 10; i++)
            {
                for (int y = 0; y < 10; y++)
                {
                    Texture2D spriteToUse = itemSlot;
                    int rowNo = 0;
                    foreach (constructionItem conItem in items)
                    {
                        if (conItem.selected)
                        {
                            //another hack to get it working in the time limit, works because we only use the first column. 
                            if (rowNo == y && i == 0)
                            {
                                spriteToUse = selectedItemSlot;
                                break;
                            }
                            
                        }
                        rowNo += 1;
                    }
                    spriteBatch.Draw(spriteToUse, new Vector2(menuLocation.X + (i * itemSlot.Width), menuLocation.Y + (y * itemSlot.Height)), Color.White);
                }
            }
        }

        private string getTextForItemType(resource.resourceTypes itemType)
        {
            switch (itemType)
            {
                case resource.resourceTypes.Hatchet:
                    return "Hatchet";
                case resource.resourceTypes.Wood:
                    return "Wood";
                case resource.resourceTypes.Acorn:
                    return "Acorn";
                case resource.resourceTypes.Spike:
                    return "Spike";
                case resource.resourceTypes.SpikePit:
                    return "Spike Pit";
                case resource.resourceTypes.Tree:
                    return "Tree";
            };

            return "";
        }

        private void drawSelectedItemInfo(SpriteBatch spriteBatch)
        {
            string selectedItem = "";
            List<Tuple<resource.resourceTypes, int>> costs = new List<Tuple<resource.resourceTypes, int>>();
            foreach (constructionItem item in items)
            {
                if (item.selected)
                {
                    selectedItem = item.lookupNameForType(item.type);
                    costs = item.cost;
                    break;
                }
            }

            string itemText = "Construct: " + selectedItem;
            string quantityText = "Costs: ";

            foreach (Tuple<resource.resourceTypes, int> cost in costs)
            {
                quantityText += getTextForItemType(cost.Item1) + " " + cost.Item2.ToString() + " : ";
            }
         

            Vector2 itemStringSize = font.MeasureString(itemText);
            Vector2 quantityTextPos = new Vector2(location.X, location.Y + itemStringSize.Y);

            spriteBatch.DrawString(font, itemText, location, Color.Red);
            spriteBatch.DrawString(font, quantityText, quantityTextPos, Color.Red);
        }

        private void drawItems(SpriteBatch spriteBatch, Vector2 menuLocation)
        {
            //this method incomplete only works because I wont have time to implement more construction options.
            int i = 0;
            foreach(constructionItem conItem in items)
            {
                Vector2 itemLocation = new Vector2(menuLocation.X, menuLocation.Y + i * itemSlot.Height);
                spriteBatch.Draw(getSpriteForType(conItem.type), itemLocation, Color.White);
                i += 1;
            }
        }

        private Texture2D getSpriteForType(resource.resourceTypes type)
        {
            switch (type)
            {
                case resource.resourceTypes.Spike:
                    return Spike;
                case resource.resourceTypes.SpikePit:
                    return SpikePit;
                case resource.resourceTypes.Tree:
                    return Tree;
            }
            return null;
        }

        public void handleKeyPress(KeyboardState keybState)
        {
            int selectedItemPos = 0;

            for (int i = 0; i <= items.Count() - 1; i++)
            {
                if (items[i].selected)
                {
                    selectedItemPos = i;
                }
            }

            if (keybState.IsKeyDown(Keys.Up))
            {
                if (selectedItemPos > 0)
                {
                    constructionItem tmp = items[selectedItemPos];
                    tmp.setSelected(false);
                    items[selectedItemPos] = tmp;

                    tmp = items[selectedItemPos - 1];
                    tmp.setSelected(true);
                    items[selectedItemPos - 1] = tmp;
                }
            }
            if (keybState.IsKeyDown(Keys.Down))
            {
                if (selectedItemPos < items.Count() - 1)
                {
                    constructionItem tmp = items[selectedItemPos];
                    tmp.setSelected(false);
                    items[selectedItemPos] = tmp;

                    tmp = items[selectedItemPos + 1];
                    tmp.setSelected(true);
                    items[selectedItemPos + 1] = tmp;
                }
            }

            if (keybState.IsKeyDown(Keys.Enter))
            {
                constructItem();
            }
        }

        private void constructItem()
        {
            List<Tuple<resource.resourceTypes, int>> costs = new List<Tuple<resource.resourceTypes, int>>();
            foreach (constructionItem item in items)
            {
                if (item.selected)
                {
                    costs = item.cost;

                    foreach (Tuple<resource.resourceTypes, int> cost in costs)
                    {
                        if(!playerInv.contains(cost.Item1, cost.Item2))
                        {
                            //do nothing
                            return;
                        }
                    }

                    foreach (Tuple<resource.resourceTypes, int> cost in costs)
                    {
                        playerInv.decreaseResource(cost.Item1, cost.Item2);
                    }

                    if (item.appearsInInventory)
                    {
                        playerInv.addToInventory(item.type, 1);
                    }
                    else if (item.appearsOnMap)
                    {
                        Vector2 location = new Vector2();
                        location.X = currPlayer.getCurrentPos().X + currPlayer.getTexture().Width + currMap.getViewport().X;
                        location.Y = currPlayer.getCurrentPos().Y + currPlayer.getTexture().Height + currMap.getViewport().Y;
                        //add spike pits and trees to the world.
                        if (item.type == resource.resourceTypes.Tree)
                        {
                            currMap.addTree(location);
                        }
                        else if (item.type == resource.resourceTypes.SpikePit)
                        {
                            currMap.addTrapToMap(location);
                        }
                    }

                    break;
                }
            }
        }

    }
}

