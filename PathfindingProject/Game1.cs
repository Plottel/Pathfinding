using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PathfindingProject
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private static Game1 _instance;
        public SpriteFont smallFont;

        public static Game1 Instance
        {
            get { return _instance; }
        }

        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;

        public IWorld world;

        public Game1()
        {
            //string mode = "";

            //while (mode != "1")
            //{
            //    Console.WriteLine("Select pathfinding mode");
            //    Console.WriteLine("1 - Hierarchical");
            //    mode = Console.ReadLine();
            //}

            //switch (mode)
            //{
            //    case "1":
            //        world = new HierarchicalWorld();
            //        break;

            //    default:
            //        world = new HierarchicalWorld();
            //        break;
            //}

            //world = new HierarchicalWorld();
            //world = new NavMeshWorld();
            world = new FlowFieldWorld();


            if (_instance == null)
                _instance = this;
            else
                throw new InvalidOperationException("Cannot have more than one instance of World");

            graphics = new GraphicsDeviceManager(this);
            Camera.WIDTH = 1366;
            Camera.HEIGHT = 768;
            graphics.PreferredBackBufferWidth = 1366;
            graphics.PreferredBackBufferHeight = 768;

            Input.SetMaxMouseX(graphics.PreferredBackBufferWidth);
            Input.SetMaxMouseY(graphics.PreferredBackBufferHeight);


            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
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

            // TODO: use this.Content to load your game content here
            smallFont = Content.Load<SpriteFont>("SmallFont");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Input.UpdateStates();
            world.HandleInput();
            world.Update();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            // TODO: Add your drawing code here
            world.Render(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
