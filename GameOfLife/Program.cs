using GameOfLife;
using OpenTK.Windowing.Desktop;
using Yasai;

/*
using (GameBase b = new GameBase("wang", GameWindowSettings.Default, NativeWindowSettings.Default))
{
    b.Run();
}*/

using (Game game = new LifeGame())
    game.Run();