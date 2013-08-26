using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

using Microsoft.Xna.Framework.Input;

namespace Zombocalypse
{
    class inventory
    {
        Texture2D background;
        Texture2D itemSlot;
        Texture2D inventoryDialog;

        #region inventory item textures
        Texture2D wood;
        Texture2D hatchet;
        Texture2D selectedItemSlot;
        Texture2D acorn;
        Texture2D spike;
        #endregion

        SpriteFont font;

        Vector2 invDialogLocation = new Vector2();

        char[,] occupiedSlots = new char[10,10];

        List<inventoryItem> items = new List<inventoryItem>();

        resource.resourceTypes currentWeapon;

        struct inventoryItem
        {
            public int quantity;
            public resource.resourceTypes type;
            public Tuple<int, int> topLeftInventoryPos;
            public Tuple<int, int> bottomRightInventoryPos;
            public bool itemSelected;

            public inventoryItem(resource.resourceTypes itemType, int itemQuantity, Tuple<int,int> pos, Tuple<int, int> lowerPos, bool selected=false)
            {
                type = itemType;
                quantity = itemQuantity;
                topLeftInventoryPos = pos;
                bottomRightInventoryPos = lowerPos;
                itemSelected = selected;
            }

            public void increaseQuantity(int value)
            {
                quantity += value;
            }

            public void decreaseQuantity(int value)
            {
                if (quantity - value > 0)
                {
                    quantity -= value;
                }
                else
                {
                    quantity = 0;
                }
            }

            public bool isSelectedItemSlot(int x, int y)
            {
                if (x >= topLeftInventoryPos.Item1 && x <= bottomRightInventoryPos.Item1)
                {
                    if (y >= topLeftInventoryPos.Item2 && y <= bottomRightInventoryPos.Item2)
                    {
                        return true;
                    }
                }
                return false;
            }

            public void setSelected(bool value)
            {
                this.itemSelected = value;
            }
        }

        public inventory(ContentManager Content)
        {
            background = Content.Load<Texture2D>("InventoryBackground");
            itemSlot = Content.Load<Texture2D>("inventorySquare");
            selectedItemSlot = Content.Load<Texture2D>("selectedInventorySquare");
            wood = Content.Load<Texture2D>("wood");
            hatchet = Content.Load<Texture2D>("hatchet");
            inventoryDialog = Content.Load<Texture2D>("InventoryDialogBox");
            acorn = Content.Load<Texture2D>("acornIcon");
            font = Content.Load<SpriteFont>("SpriteFont1");
            spike = Content.Load<Texture2D>("woodenStake");


            currentWeapon = resource.resourceTypes.Hatchet;


            //set all inventory slots to unoccupied. 
            for (int i = 0; i < 10; i++)
            {
                for (int y = 0; y < 10; y++)
                {
                    occupiedSlots[i, y] = 'F'; 
                }
            }

            //load item textures
        }

        public resource.resourceTypes getCurrentWeapon()
        {
            return currentWeapon;
        }

        public Texture2D getCurrentWeaponTexture()
        {
            return getTextureForType(currentWeapon);
        }

        private void drawItems(SpriteBatch spriteBatch, Vector2 invLocation)
        {
            //draw stuff in the inventory
            foreach (inventoryItem item in items)
            {
                spriteBatch.Draw(getTextureForType(item.type), new Vector2(invLocation.X + (item.topLeftInventoryPos.Item1 * itemSlot.Width), invLocation.Y + (item.topLeftInventoryPos.Item2 * itemSlot.Height)), Color.White);
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
            };

            return "";
        }

        private void drawSelectedItemInfo(SpriteBatch spriteBatch)
        {
            string selectedItem = "";
            int selectedQuantity = 0;
            foreach (inventoryItem item in items)
            {
                if (item.itemSelected)
                {
                    selectedItem = getTextForItemType(item.type);
                    selectedQuantity = item.quantity;
                    break;
                }
            }

            string itemText = "Item: " + selectedItem;
            string quantityText = "Quantity: " + selectedQuantity;

            Vector2 itemStringSize = font.MeasureString(itemText);
            Vector2 quantityTextPos = new Vector2(invDialogLocation.X, invDialogLocation.Y + itemStringSize.Y);

            spriteBatch.DrawString(font, itemText, invDialogLocation, Color.Red);
            spriteBatch.DrawString(font, quantityText, quantityTextPos, Color.Red);
        }

        public void draw(SpriteBatch spriteBatch, Vector2 location)
        {
            invDialogLocation = new Vector2(location.X, location.Y - inventoryDialog.Height);
            spriteBatch.Draw(background, location, Color.White);
            spriteBatch.Draw(inventoryDialog, invDialogLocation, Color.White);

            drawSelectedItemInfo(spriteBatch);

            drawItems(spriteBatch, location);

            for (int i = 0; i < 10; i++)
            {
                for(int y=0; y < 10; y++)
                {
                    Texture2D spriteToUse = itemSlot;
                    foreach (inventoryItem item in items)
                    {
                        if (item.itemSelected)
                        {
                            if (item.isSelectedItemSlot(i, y))
                            {
                                spriteToUse = selectedItemSlot;
                                break;
                            }
                        }
                    }
                    spriteBatch.Draw(spriteToUse, new Vector2(location.X + (i * itemSlot.Width), location.Y + (y * itemSlot.Height)), Color.White);
                }
            }
        }

        public void handleKeyPress(KeyboardState keybState)
        {
            int selectedItemPos = 0;

            for (int i = 0; i <= items.Count() - 1; i++ )
            {
                if (items[i].itemSelected)
                {
                    selectedItemPos = i;
                }
            }

            if (keybState.IsKeyDown(Keys.Up))
            {
                if (selectedItemPos > 0)
                {
                    inventoryItem tmp = items[selectedItemPos];
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
                    inventoryItem tmp = items[selectedItemPos];
                    tmp.setSelected(false);
                    items[selectedItemPos] = tmp;

                    tmp = items[selectedItemPos + 1];
                    tmp.setSelected(true);
                    items[selectedItemPos + 1] = tmp;
                }
            }
        }

        //this method works but its putting stuff in the wrong way (as in it adds in verticle columns rather than horizontal which was unintended).
        //TODO: fixme
        public void addToInventory(resource.resourceTypes type, int quantity)
        {
            if(contains(type, 0))
            {
                increaseResource(type, quantity);
            }
            else
            {
                Texture2D itemTexture = getTextureForType(type);

                int colSlotsNeeded = itemTexture.Width / 20;
                int rowSlotsNeeded = itemTexture.Height / 20;

                for (int col = 0; col < 10; col++)
                {
                    for (int row = 0; row < 10; row++)
                    {
                        if (occupiedSlots[col, row] == 'F')
                        {
                            //make sure theres enough slots to do the checks
                            if (col + colSlotsNeeded <= 10 && row + rowSlotsNeeded <= 10)
                            {
                                bool columnsMatch = true;
                                bool rowsMatch = true;

                                for (int colSizeCheck = 0; colSizeCheck < colSlotsNeeded; colSizeCheck += 1)
                                {
                                    if (occupiedSlots[col + colSizeCheck, row] != 'F')
                                    {
                                        columnsMatch = false;
                                    }
                                }

                                if (columnsMatch)
                                {
                                    for (int rowSizeCheck = 0; rowSizeCheck < rowSlotsNeeded; rowSizeCheck += 1)
                                    {
                                        if (occupiedSlots[col, row + rowSizeCheck] != 'F')
                                        {
                                            rowsMatch = false;
                                        }
                                    }
                                }

                                if (rowsMatch && columnsMatch)
                                {
                                    //set the rows to used
                                    for (int colSizeCheck = 0; colSizeCheck < colSlotsNeeded; colSizeCheck += 1)
                                    {
                                        occupiedSlots[col + colSizeCheck, row] = 'T';
                                    }

                                    for (int rowSizeCheck = 0; rowSizeCheck < rowSlotsNeeded; rowSizeCheck += 1)
                                    {
                                        occupiedSlots[col, row + rowSizeCheck] = 'T';
                                    }
                                    if (items.Count == 0)
                                    {
                                        items.Add(new inventoryItem(type, quantity, new Tuple<int, int>(col, row), new Tuple<int, int>(col + (colSlotsNeeded -1), row + (rowSlotsNeeded - 1)), true));
                                    }
                                    else
                                    {
                                        items.Add(new inventoryItem(type, quantity, new Tuple<int, int>(col, row), new Tuple<int, int>(col + (colSlotsNeeded - 1), row + (rowSlotsNeeded - 1))));
                                    }
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void increaseResource(resource.resourceTypes res, int quantity)
        {
            int selectedItemPos = 0;

            for (int i = 0; i <= items.Count() - 1; i++)
            {
                if (items[i].type == res)
                {
                    selectedItemPos = i;
                    break;
                }
            }

            inventoryItem tmp = items[selectedItemPos];
            tmp.increaseQuantity(quantity);
            items[selectedItemPos] = tmp;
        }

        public void decreaseResource(resource.resourceTypes res, int quantity)
        {
            int selectedItemPos = 0;

            for (int i = 0; i <= items.Count() - 1; i++)
            {
                if (items[i].type == res)
                {
                    selectedItemPos = i;
                    break;
                }
            }

            inventoryItem tmp = items[selectedItemPos];
            tmp.decreaseQuantity(quantity);
            items[selectedItemPos] = tmp;
        }

        public bool contains(resource.resourceTypes res, int quantity)
        {
            foreach (inventoryItem item in items)
            {
                if (item.type == res && item.quantity >= quantity)
                {
                    return true;
                }
            }

            return false;
        }

        private Texture2D getTextureForType(resource.resourceTypes type)
        {
            switch (type)
            {
                case resource.resourceTypes.Wood:
                    return wood;
                case resource.resourceTypes.Hatchet:
                    return hatchet;
                case resource.resourceTypes.Acorn:
                    return acorn;
                case resource.resourceTypes.Spike:
                    return spike;
            }

            return null;
        }
    }
}
