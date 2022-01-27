using System.Drawing;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Yasai;
using Yasai.Graphics;
using Yasai.Graphics.Containers;
using Yasai.Graphics.Text;
using Yasai.Resources.Stores;
using Yasai.Structures.DI;

namespace GameOfLife
{
    public class LifeGame : Game
    {
        private Grid grid;

        private const int TILE_SIZE = 16;

        private Vector2i size = new (110, 70);
        private bool running;
        private int tickRate = 5;
        
        private Container inst;

        public LifeGame()
        {
            grid = pregenerate(true);
        }

        public override void Load(DependencyContainer dependencies)
        {
            base.Load(dependencies);
            
            BackgroundColor = Color.Black;
            
            // create the cells
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    Root.Add(new DrawableCell
                    {
                        Position = new Vector2(x * (TILE_SIZE + 2), y * (TILE_SIZE + 2)),
                        Coord = new Vector2i(x, y),
                        Size = new Vector2(TILE_SIZE),
                        Visible = grid.Situation[y][x]
                    });
                }
            }

            // text
            var fonts = dependencies.Resolve<FontStore>();
            float TEXT_SCALE = 1;
            float SPACING_Y = 15;
            int WORD_SPACING = 8;
            
            inst = new Container()
            {
                Position = new Vector2(10),
                Items = new IDrawable[]
                {
                    // we need better containers NOW
                    new SpriteText("CONTROLS:", fonts.GetResource(SpriteFont.Segoe_Small))
                    {
                        Position = new Vector2(0, SPACING_Y*0),
                        Colour = Color.White,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                    new SpriteText("Play/Pause: Space", fonts.GetResource(SpriteFont.Segoe_Small))
                    {
                        Position = new Vector2(0, SPACING_Y*1),
                        Colour = Color.White,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                    new SpriteText("Faster: K/L/Up/Right", fonts.GetResource(SpriteFont.Segoe_Small))
                    {
                        Position = new Vector2(0, SPACING_Y*2),
                        Colour = Color.White,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                    new SpriteText("Slower: J/H/Down/Left", fonts.GetResource(SpriteFont.Segoe_Small))
                    {
                        Position = new Vector2(0, SPACING_Y*3),
                        Colour = Color.White,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                    new SpriteText("Step through: Enter (also pauses the simulation)", fonts.GetResource(SpriteFont.Segoe_Small))
                    {
                        Position = new Vector2(0, SPACING_Y*4),
                        Colour = Color.White,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                    new SpriteText("Generate random: R", fonts.GetResource(SpriteFont.Segoe_Small))
                    {
                        Position = new Vector2(0, SPACING_Y*5),
                        Colour = Color.White,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                    new SpriteText("Clear: E", fonts.GetResource(SpriteFont.Segoe_Small))
                    {
                        Position = new Vector2(0, SPACING_Y*6),
                        Colour = Color.White,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                    new SpriteText("Left click to place, Right click to remove", fonts.GetResource(SpriteFont.Segoe_Small))
                    {
                        Position = new Vector2(0, SPACING_Y*8),
                        Colour = Color.White,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                },
            };

            Root.Add(inst);
        }

        private int timer;
        public override void Update(FrameEventArgs args)
        {
            base.Update(args);
            timer++; // <- this sucks

            if (tickRate <= 0)
                return;
            
            if (timer % tickRate == 0 && running)
                tick();
        }

        /// <summary>
        /// Update the simulation
        /// </summary>
        private void tick()
        {
            grid.Tick();
            updateCells();
        }
        
        /// <summary>
        /// give a grid which is set to a preset situation
        /// </summary>
        /// <param name="empty"></param>
        /// <param name="probability"></param>
        /// <returns></returns>
        private Grid pregenerate(bool empty = false, double probability = 0.4)
        {
            var rand = new Random();
            bool[][] situation = new bool[size.Y][];
            for (int y = 0; y < size.Y; y++)
            {
                var row = new bool[size.X];
                if (!empty)
                {
                    for (int x = 0; x < size.X; x++)
                        row[x] = rand.Next(10) < probability * 10;
                }
                situation[y] = row;
            }

            return new Grid(situation);
        }

        /// <summary>
        /// Update drawable representation
        /// </summary>
        private void updateCells()
        {
            foreach (IDrawable d in Root)
            {
                if (d is DrawableCell cell)
                {
                    Vector2i pos = cell.Coord;
                    cell.Visible = grid.Situation[pos.Y][pos.X];
                }
            }
        }
        
        protected override void MouseDown(MouseButtonEventArgs args)
        {
            base.MouseDown(args);
            if (running)
                return;

            int s = TILE_SIZE + 2;
            int x = (int) Math.Floor(MousePosition.X / s);
            int y = (int) Math.Floor(MousePosition.Y / s);
            Vector2i mPos = new Vector2i(x, y);
            bool val = args.Button == MouseButton.Left;

            // keep in bounds
            if (mPos.X > grid.Width || mPos.Y > grid.Height)
                return;

            grid.Situation[mPos.Y][mPos.X] = val;
            updateCells();
        }

        protected override void KeyDown(KeyboardKeyEventArgs args)
        {
            base.KeyDown(args);
            switch (args.Key)
            {
                // pause
                case Keys.Space:
                    running = !running;
                    break;
                
                // faster
                case Keys.K:
                case Keys.L:
                case Keys.Up:
                case Keys.Right:
                    tickRate --;
                    break;
                
                // slower
                case Keys.J:
                case Keys.H:
                case Keys.Down:
                case Keys.Left:
                    tickRate ++;
                    break;
                
                // step through
                case Keys.Enter:
                    running = false;
                    tick();
                    break;
                
                // random
                case Keys.R:
                    if (running)
                        break;
                    grid = pregenerate();
                    updateCells();
                    break;
                
                // empty
                case Keys.E:
                    if (running)
                        break;
                    grid = pregenerate(true);
                    updateCells();
                    break;
            }
        }
    }
}