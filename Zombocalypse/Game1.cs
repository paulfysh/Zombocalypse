using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Zombocalypse
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        SpriteFont font;

        MapLoader worldMap = new MapLoader();
        MapLoader currentMap;
        Vector2 worldMapPos = new Vector2(0, 0);

        Player currentPlayer;

        bool attacking = false;

        ConsumableMapObject collidingObject;

        bool keyReleased = true;

        Texture2D attackingWeaponTexture;
        Vector2 attackingWeaponPosition;
        Vector2 attackingWeaponOrigin;
        private float RotationAngle;
        int AttackTimeElapsed;
        int maxAttackTime;

        SoundEffect soundOhNo;
        SoundEffectInstance soundOhNoInstance;
        bool gameOverSoundPlayed = false;

        bool constructionMenuOpen = false;
        ConstructionMenu conMenu;

        List<Zombie> zombies = new List<Zombie>();

        double nextZombieSpawnTime = 10.0;
        double intervalBetweenSpawns = 10.0;
         
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Window.Title = "Zombocalypose";
            //this.graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            worldMap.loadContent(Content, graphics.PreferredBackBufferHeight, graphics.PreferredBackBufferWidth);
            worldMap.loadMap("content/worldMap.txt");

            currentMap = worldMap;

            currentPlayer = new Player(Content);

            zombies.Add(new Zombie(Content, new Vector2(450, 450)));

            font = Content.Load<SpriteFont>("SpriteFont1");

            soundOhNo = Content.Load<SoundEffect>("ohNo");
            soundOhNoInstance = soundOhNo.CreateInstance();

            conMenu = new ConstructionMenu(new Vector2(0, 0), Content, currentPlayer.getInventory(), currentMap, currentPlayer);


            //inv.addToInventory(resource.resourceTypes.Hatchet, 1);
           

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            if (attacking)
            {
                //RotationAngle += elapsed;
                //float circle = MathHelper.Pi * 300;
                //RotationAngle = RotationAngle % circle;

                RotationAngle += 0.1f;

                AttackTimeElapsed += 1;

                if (AttackTimeElapsed > maxAttackTime)
                {
                    attacking = false;
                }

                checkZombieWeaponCollision();
            }

            double elapsed = gameTime.TotalGameTime.TotalSeconds;

            if (elapsed >= nextZombieSpawnTime)
            {
                nextZombieSpawnTime += intervalBetweenSpawns;
                spawnZombies();
            }

            if (!constructionMenuOpen && !currentPlayer.isInventoryOpen())
            {
                List<Zombie> deleteZeds = new List<Zombie>();
                List<Vector2> deleteTraps = new List<Vector2>();
                foreach (Zombie zed in zombies)
                {
                    Vector2 viewport = currentMap.getViewport();
                    Vector2 PlayerRealPos = currentPlayer.getCurrentPos();
                    PlayerRealPos.X += viewport.X;
                    PlayerRealPos.Y += viewport.Y;

                    zed.moveTowardsPlayer(currentPlayer, viewport);

                    Matrix zombieMatrix = Matrix.CreateTranslation(zed.getLocation().X, zed.getLocation().Y, 0);
                    Color[,] zombieColours = Helpers.TextureTo2DArray(zed.getTexture());

                    foreach (Vector2 trap in currentMap.getTraps())
                    {
                        //Vector2 trapPos = new Vector2(trap.X, trap.Y);

                        Matrix trapMatrix = Matrix.CreateTranslation(trap.X, trap.Y, 0);
                        Color[,] trapColours = Helpers.TextureTo2DArray(currentMap.getTrapTexture());

                        Vector2 collisionPoint = Helpers.TexturesCollide(zombieColours, zombieMatrix, trapColours, trapMatrix);

                        if (collisionPoint.X != -1)
                        {
                            zed.setDead();
                            deleteZeds.Add(zed);
                            deleteTraps.Add(trap);
                            
                        }

                    }
                }

                foreach (Zombie deadZed in deleteZeds)
                {
                    zombies.Remove(deadZed);
                }

                foreach (Vector2 trap in deleteTraps)
                {
                    currentMap.removeTrap(trap);
                }

                checkZombiePlayerCollision();
            }
            else
            {
                //delay the spawn timer
                nextZombieSpawnTime += gameTime.ElapsedGameTime.TotalSeconds;
            }

            checkKeyboard();

            base.Update(gameTime);
        }

        private void spawnZombies()
        {
            //summon a zombie outside every structure. 
            foreach (EnterableMapObject obj in currentMap.getEnterableObjects())
            {
                zombies.Add(new Zombie(Content, new Vector2(obj.getLocation().X + (obj.getTexture().Width / 2), obj.getLocation().Y + (obj.getTexture().Height))));
            }

            currentMap.removeOldestExpiredTrap();
            
        }

        private void performAttack()
        {
            //start the attack, the update and draw methods will do the rest. 
            attacking = true;
            attackingWeaponTexture = currentPlayer.getTextureForCurrentWeapon();
            attackingWeaponPosition = new Vector2(currentPlayer.getCurrentPos().X + currentPlayer.getTexture().Width -20, currentPlayer.getCurrentPos().Y + 72);
            attackingWeaponOrigin = new Vector2(0, attackingWeaponTexture.Height);
            RotationAngle = 0.0f;
            AttackTimeElapsed = 0;
            maxAttackTime = 10;
        }

        private void checkKeyboard()
        {
            KeyboardState keybState = Keyboard.GetState();

            if (currentPlayer.isInventoryOpen())
            {
                if (keybState.IsKeyDown(Keys.I))
                {
                    if (keyReleased)
                    {
                        currentPlayer.openCloseInventory();
                    }
                }
                else
                {
                    if (keyReleased)
                    {
                        currentPlayer.handleInventoryInput(keybState);
                    }
                }
            }
            else if (constructionMenuOpen)
            {
                if (keyReleased)
                {
                    if (keybState.IsKeyDown(Keys.C))
                    {
                        constructionMenuOpen = false;
                    }
                    else
                    {
                        conMenu.handleKeyPress(keybState);
                    }
                }

            }
            else
            {

                if (keybState.IsKeyDown(Keys.Escape))
                {
                    Exit();
                }

                if (keybState.IsKeyDown(Keys.RightControl))
                {
                    //attack.
                    if (!attacking)
                    {
                        performAttack();
                    }
                }

                if (keybState.IsKeyDown(Keys.Left))
                {
                    if (!willCollide(currentPlayer.getNextPos(Player.direction.Left), currentPlayer.getTexture()))
                    {
                        if (!currentMap.checkAndMoveViewport(currentPlayer.getNextPos(Player.direction.Left), currentPlayer.getCurrentPos()))
                        {
                            currentPlayer.move(Player.direction.Left);
                            if (attacking)
                            {
                                attackingWeaponPosition = new Vector2(currentPlayer.getCurrentPos().X + currentPlayer.getTexture().Width - 20, currentPlayer.getCurrentPos().Y + 72);
                            }
                        }
                    }
                }

                if (keybState.IsKeyDown(Keys.Right))
                {
                    if (!willCollide(currentPlayer.getNextPos(Player.direction.Right), currentPlayer.getTexture()))
                    {
                        if (!currentMap.checkAndMoveViewport(currentPlayer.getNextPos(Player.direction.Right), currentPlayer.getCurrentPos()))
                        {
                            currentPlayer.move(Player.direction.Right);
                            if (attacking)
                            {
                                attackingWeaponPosition = new Vector2(currentPlayer.getCurrentPos().X + currentPlayer.getTexture().Width - 20, currentPlayer.getCurrentPos().Y + 72);
                            }
                        }
                    }
                }

                if (keybState.IsKeyDown(Keys.Up))
                {
                    if (!willCollide(currentPlayer.getNextPos(Player.direction.Up), currentPlayer.getTexture()))
                    {
                        if (!currentMap.checkAndMoveViewport(currentPlayer.getNextPos(Player.direction.Up), currentPlayer.getCurrentPos()))
                        {
                            currentPlayer.move(Player.direction.Up);
                            if (attacking)
                            {
                                attackingWeaponPosition = new Vector2(currentPlayer.getCurrentPos().X + currentPlayer.getTexture().Width - 20, currentPlayer.getCurrentPos().Y + 72);
                            }
                        }
                    }
                }

                if (keybState.IsKeyDown(Keys.Down))
                {
                    if (!willCollide(currentPlayer.getNextPos(Player.direction.Down), currentPlayer.getTexture()))
                    {
                        if (!currentMap.checkAndMoveViewport(currentPlayer.getNextPos(Player.direction.Down), currentPlayer.getCurrentPos()))
                        {
                            currentPlayer.move(Player.direction.Down);
                            if (attacking)
                            {
                                attackingWeaponPosition = new Vector2(currentPlayer.getCurrentPos().X + currentPlayer.getTexture().Width - 20, currentPlayer.getCurrentPos().Y + 72);
                            }
                        }
                    }
                }

                if (keybState.IsKeyDown(Keys.I))
                {
                    if (keyReleased)
                    {
                        currentPlayer.openCloseInventory();
                    }
                }

                if (keybState.IsKeyDown(Keys.Space))
                {
                    currentPlayer.interactWithAdjacentObj();
                }

                if (keybState.IsKeyDown(Keys.C))
                {
                    if (keyReleased)
                    {
                        constructionMenuOpen = true;
                    }
                }
            }

            if (keybState.GetPressedKeys().Count() == 0)
            {
                keyReleased = true;
            }
            else
            {
                keyReleased = false;
            }
        }

        bool willCollide(Vector2 positionToMoveTo, Texture2D movingObject)
        {
            Vector2 currentViewport = currentMap.getViewport();
            //Vector2 moveToRelativeToScreen = new Vector2(positionToMoveTo.X - currentViewport.X, positionToMoveTo.Y - currentViewport.Y);

            Matrix movingObjectMatrix = Matrix.CreateTranslation(positionToMoveTo.X, positionToMoveTo.Y, 0);
            Color[,] movingObjectColours = Helpers.TextureTo2DArray(movingObject);

            //check for consumable objects
            foreach (ConsumableMapObject obj in currentMap.getNonEnterableObjects())
            {
                Vector2 objPositionRelativeToScreen = new Vector2(obj.getLocation().X - currentViewport.X, obj.getLocation().Y - currentViewport.Y);
                Matrix objectMatrix = Matrix.CreateTranslation(objPositionRelativeToScreen.X, objPositionRelativeToScreen.Y, 0);

                Color[,] nonEnterableObjectColours = Helpers.TextureTo2DArray(obj.getTexture());

                Vector2 collisionPoint = Helpers.TexturesCollide(nonEnterableObjectColours, objectMatrix, movingObjectColours, movingObjectMatrix);

                if (collisionPoint.X != -1)
                {
                    collidingObject = obj;
                    currentPlayer.setAdjacantObject(obj);
                    return true;
                }
            }

            //check for enterable objects. 
            foreach (EnterableMapObject obj in currentMap.getEnterableObjects())
            {
                Vector2 objPositionRelativeToScreen = new Vector2(obj.getLocation().X - currentViewport.X, obj.getLocation().Y - currentViewport.Y);
                Matrix objectMatrix = Matrix.CreateTranslation(objPositionRelativeToScreen.X, objPositionRelativeToScreen.Y, 0);

                Color[,] nonEnterableObjectColours = Helpers.TextureTo2DArray(obj.getTexture());

                Vector2 collisionPoint = Helpers.TexturesCollide(nonEnterableObjectColours, objectMatrix, movingObjectColours, movingObjectMatrix);

                if (collisionPoint.X != -1)
                {
                    Vector2 actualObjectPosition = new Vector2((positionToMoveTo.X + currentViewport.X), positionToMoveTo.Y + currentViewport.Y);
                    if (obj.canEnterFromPosition(actualObjectPosition))
                    {
                        worldMapPos = new Vector2((obj.getLocation().X + obj.getTexture().Width / 2) - currentViewport.X, obj.getLocation().Y - currentViewport.Y + obj.getTexture().Height);
                        currentPlayer.setPosition(new Vector2(0, 0));
                        currentMap = obj.getMap();
                    }
                    return true;
                }
            }

            if (currentMap.moveWillExitMap(positionToMoveTo, movingObject))
            {
                return true;
            }

            Color[,] exitColours = Helpers.TextureTo2DArray(currentMap.getExitTexture());
            Matrix exitMatrix = Matrix.CreateTranslation(currentMap.getExitPos().X, currentMap.getExitPos().Y, 0);

            Vector2 exitCollisionPoint = Helpers.TexturesCollide(exitColours, exitMatrix, movingObjectColours, movingObjectMatrix);
            if (exitCollisionPoint.X != -1)
            {
                currentPlayer.setPosition(worldMapPos);
                currentMap = worldMap;
                return true;
            }

            //check for leaving the map
            if (positionToMoveTo.X < 0 || positionToMoveTo.Y < 0 || positionToMoveTo.X > (graphics.PreferredBackBufferWidth - currentPlayer.getTexture().Width) || 
                positionToMoveTo.Y > (graphics.PreferredBackBufferHeight - currentPlayer.getTexture().Height))
            {
                return true;
            }

            collidingObject = null;
            currentPlayer.setAdjacantObject(null);

            return false;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            if (currentPlayer.getIsDead())
            {
                string gameOver = "A Zombie got your brains -  Game Over! ";
                Vector2 position = new Vector2((graphics.PreferredBackBufferWidth / 2) - (font.MeasureString(gameOver).X / 2), graphics.PreferredBackBufferHeight / 2);
                spriteBatch.DrawString(font, gameOver, position, Color.Red);
            }
            else
            {
                currentMap.drawMap(spriteBatch);

                currentPlayer.drawPlayer(spriteBatch);

                foreach (Zombie zed in zombies)
                {
                    zed.drawZombie(spriteBatch, currentMap.getViewport());
                }

                if (attacking)
                {
                    //spriteBatch.Draw(attackingWeaponTexture, attackingWeaponPosition, Color.White);
                    spriteBatch.Draw(attackingWeaponTexture, attackingWeaponPosition, null, Color.White, RotationAngle,
                                     attackingWeaponOrigin, 1.0f, SpriteEffects.None, 0f);
                }

                if (constructionMenuOpen)
                {
                    conMenu.draw(spriteBatch);
                }

            }
            spriteBatch.End();

            base.Draw(gameTime);
        }


        //this isnt perfect but itll do for now, return later if time (there wont be...)
        private void checkZombieWeaponCollision()
        {
            Vector2 viewport = currentMap.getViewport();
            Matrix weaponMatrix = Matrix.CreateTranslation(attackingWeaponOrigin.X * -1, attackingWeaponOrigin.Y * -1, 0) * Matrix.CreateRotationZ(RotationAngle) * Matrix.CreateTranslation(attackingWeaponPosition.X + viewport.X, attackingWeaponPosition.Y + viewport.Y, 0);
            Color[,] weaponColours = Helpers.TextureTo2DArray(attackingWeaponTexture);

            List<Zombie> deleteZeds = new List<Zombie>();
            
            foreach (Zombie zed in zombies)
            {
                Matrix zombieMatrix = Matrix.CreateTranslation(zed.getLocation().X, zed.getLocation().Y, 0);
                Color[,] zombieColours = Helpers.TextureTo2DArray(zed.getTexture());

                Vector2 collisionPoint = Helpers.TexturesCollide(zombieColours, zombieMatrix, weaponColours, weaponMatrix);

                if (collisionPoint.X != -1)
                {
                    zed.setDead();
                    deleteZeds.Add(zed);
                }
            }

            foreach (Zombie zed in deleteZeds)
            {
                zombies.Remove(zed);
            }
        }

        private void checkZombiePlayerCollision()
        {
            Vector2 viewport = currentMap.getViewport();
            Matrix playerMatrix = Matrix.CreateTranslation(currentPlayer.getCurrentPos().X + viewport.X, currentPlayer.getCurrentPos().Y + viewport.Y, 0);
            Color[,] playerColours = Helpers.TextureTo2DArray(currentPlayer.getTexture());

            foreach (Zombie zed in zombies)
            {
                Matrix zombieMatrix = Matrix.CreateTranslation(zed.getLocation().X, zed.getLocation().Y, 0);
                Color[,] zombieColours = Helpers.TextureTo2DArray(zed.getTexture());

                Vector2 collisionPoint = Helpers.TexturesCollide(zombieColours, zombieMatrix, playerColours, playerMatrix);

                if (collisionPoint.X != -1)
                {
                    currentPlayer.setDead();
                    if (soundOhNoInstance.State == SoundState.Stopped && !gameOverSoundPlayed)
                    {
                        soundOhNoInstance.Volume = 0.50f;
                        //groanInstance.IsLooped = false;
                        soundOhNoInstance.Play();
                        gameOverSoundPlayed = true;
                    }
                }
            }
            
        }
    }
}
