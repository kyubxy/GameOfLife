using System.Drawing;
using System.Globalization;
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

        private int t;
        private int ticks
        {
            get => t;
            set
            {
                t = value;
                if (tickText != null)
                    tickText.Text = "Ticks: " + t;
            }
        }
        
        private bool r;
        private bool running
        {
            get => r;
            set
            {
                // TODO: this is broken
                runningText.Text = "Running: " + (r ? "True" : "False");
                runningText.Colour = r ? Color.Green : Color.Red;
                r = value;
            }
        }

        private float tr = 0.1f;
        private float tickRate
        {
            get => tr;
            set
            {
                tr = value;
                rateText.Text = "tickRate: " + Math.Round(tr, 2);
            }
        }
        
        private Container inst;
        private Container info;

        private SpriteText renderText;
        private SpriteText rateText;
        private SpriteText tickText;
        private SpriteText deltaText;
        private SpriteText runningText;
        
        private SpriteText editPlayText;

        public LifeGame()
        {
            grid = generate(true);
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

            var font = fonts.GetResource(SpriteFont.SegoeSmall);

            info = new Container()
            {
                Position = new Vector2(-20, -130),
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Items = new IDrawable[]
                {
                    new SpriteText("STATS", font)
                    {
                        Position = new Vector2(0, SPACING_Y * 0),
                        TextAlign = Align.Right,
                        Colour = Color.Red,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                    renderText = new SpriteText("RenderFrequency: idk", font)
                    {
                        Position = new Vector2(0, SPACING_Y * 1),
                        TextAlign = Align.Right,
                        Colour = Color.CornflowerBlue,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                    rateText = new SpriteText("tickRate: " + tickRate.ToString(CultureInfo.CurrentCulture), font)
                    {
                        Position = new Vector2(0, SPACING_Y * 2),
                        TextAlign = Align.Right,
                        Colour = Color.CornflowerBlue,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                    deltaText = new SpriteText("deltaTime", font)
                    {
                        Position = new Vector2(0, SPACING_Y * 3),
                        TextAlign = Align.Right,
                        Colour = Color.CornflowerBlue,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                    runningText = new SpriteText("Running: False", font)
                    {
                        Position = new Vector2(0, SPACING_Y * 4),
                        TextAlign = Align.Right,
                        Colour = Color.Red,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                    tickText = new SpriteText("Ticks: 0", font)
                    {
                        Position = new Vector2(0, SPACING_Y * 5),
                        TextAlign = Align.Right,
                        Colour = Color.CornflowerBlue,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                    new SpriteText("Powered by Yasai", font)
                    {
                        Position = new Vector2(0, SPACING_Y * 6),
                        TextAlign = Align.Right,
                        Colour = Color.SpringGreen,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                }
            };
            
            inst = new Container
            {
                Position = new Vector2(10),
                Items = new IDrawable[]
                {
                    // we need better containers NOW
                    new SpriteText("CONTROLS:", font)
                    {
                        Position = new Vector2(0, SPACING_Y*0),
                        Colour = Color.White,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                    new SpriteText("Play/Pause: Space", font)
                    {
                        Position = new Vector2(0, SPACING_Y*1),
                        Colour = Color.White,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                    new SpriteText("Slower: K/L/Up/Right", font)
                    {
                        Position = new Vector2(0, SPACING_Y*2),
                        Colour = Color.White,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                    new SpriteText("Faster: J/H/Down/Left", font)
                    {
                        Position = new Vector2(0, SPACING_Y*3),
                        Colour = Color.White,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                    new SpriteText("Step through: Enter (also pauses the simulation)", font)
                    {
                        Position = new Vector2(0, SPACING_Y*4),
                        Colour = Color.White,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                    new SpriteText("Generate random: R (this clears the simulation first)", font)
                    {
                        Position = new Vector2(0, SPACING_Y*5),
                        Colour = Color.White,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                    new SpriteText("Clear: E", font)
                    {
                        Position = new Vector2(0, SPACING_Y*6),
                        Colour = Color.White,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                    new SpriteText("Left click to place, Right click to remove", font)
                    {
                        Position = new Vector2(0, SPACING_Y*8),
                        Colour = Color.White,
                        CharScale = TEXT_SCALE,
                        WordSpacing = WORD_SPACING,
                    },
                },
            };

            Root.Add(inst);
            Root.Add(info);
            Root.Add(editPlayText = new SpriteText("Press space to pause the simulation first", fonts.GetResource(SpriteFont.Segoe))
            {
                Anchor = Anchor.Center,
                Origin = Anchor.Center,
                Colour = Color.Red,
                TextAlign = Align.Center,
            });
            // TODO: setting this before load doesnt do anything
            editPlayText.Visible = false;
        }

        private double deltaTime;
        public override void Update(FrameEventArgs args)
        {
            base.Update(args);
            
            deltaText.Text = "deltaTime: " + deltaTime;

            if (tickRate <= 0 || !running)
                return;

            deltaTime += args.Time;

            while (deltaTime > tickRate)
            {
                tick();
                deltaTime -= tickRate;
            }
        }

        /// <summary>
        /// Update the simulation
        /// </summary>
        private void tick()
        {
            grid.Tick();
            updateCells();
            ticks++;
        }
        
        /// <summary>
        /// give a grid which is set to a preset situation
        /// </summary>
        /// <param name="empty"></param>
        /// <param name="probability"></param>
        /// <returns></returns>
        private Grid generate(bool empty = false, double probability = 0.4)
        {
            ticks = 0;
            
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

        protected override void MouseUp(MouseButtonEventArgs args)
        {
            base.MouseUp(args);
            editPlayText.Visible = false;
        }

        protected override void MouseDown(MouseButtonEventArgs args)
        {
            base.MouseDown(args);
            if (running)
            {
                editPlayText.Visible = true;
                return;
            }

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
                    inst.Visible = !inst.Visible;
                    break;
                
                // faster
                case Keys.K:
                case Keys.L:
                case Keys.Up:
                case Keys.Right:
                    tickRate += 0.01f;
                    break;
                
                // slower
                case Keys.J:
                case Keys.H:
                case Keys.Down:
                case Keys.Left:
                    if (tickRate > 0.02)
                        tickRate -= 0.01f;
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
                    grid = generate();
                    updateCells();
                    break;
                
                // empty
                case Keys.E:
                    if (running)
                        break;
                    grid = generate(true);
                    updateCells();
                    break;
            }
        }
    }
}