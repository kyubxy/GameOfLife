namespace GameOfLife
{
    public class Grid
    {
        public bool[][] Situation;

        public int Width => GetDimensions(Situation).Item1;
        public int Height => GetDimensions(Situation).Item2;

        public Grid(bool[][] situation)
        {
            Situation = situation;
        }

        public void Tick()
        {
            var (w, h) = GetDimensions(Situation);
            bool[][] _situation = new bool[h][];
            for (int i = 0; i < w; i++)
                _situation[i] = new bool[w];
            
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    _situation[y][x] = IsAlive(Situation, x, y);
                }
            }

            Situation = _situation;
        }

        public void Print()
        {
            var (w, h) = GetDimensions(Situation);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                    Console.Write(Situation[y][x] ? "O " : ". ");
                Console.WriteLine();
            }

            Console.WriteLine();
        }

        #region helper functions
        public static bool IsAlive(bool[][] situation, int x, int y)
        {
            int adj = CheckAdjCells(situation, x, y);
            bool ret = situation[y][x] ? adj == 2 || adj == 3 : adj == 3;

            return ret;
        }

        public static int CheckAdjCells(bool[][] situation, int x, int y)
        {
            int aliveCells = 0;
            var (w, h) = GetDimensions(situation);
            
            for (int _y = -1; _y <= 1; _y++)
            {
                int sy = _y + y;
                if (sy < 0 || sy > h - 1)
                    continue;
                
                for (int _x = -1; _x <= 1; _x++)
                {
                    int sx = _x + x;
                    if (sx < 0 || sx > w - 1)
                        continue;

                    if (sy == y && sx == x)
                        continue;
                    
                    if (situation[sy][sx])
                        aliveCells++;
                }
            }

            return aliveCells;
        }

        public static (int, int) GetDimensions(bool[][] situation) => (situation[0].Length, situation.Length);
        
        #endregion
    }
}