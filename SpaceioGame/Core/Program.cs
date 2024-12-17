using ConsoleEngine;
using System;
using System.Runtime.CompilerServices;

namespace SpaceioGame
{
    //screen buffer size x: 250 y: 115 font: 10 
    //settings x: 125 y: 98
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var game = new Game();

            var settings = new Settings(10, 'O', "  " , 1000, 128, 98, 1000);
            var chunkManager = new ChunkManager(game);
            var uIDManager = new UIDManager();
            var gameObjectManager = new GameObjectManager(game, uIDManager);
            var camera = new Camera(new Vec2i(0, 0), new Vec2i(settings.CameraSizeX, settings.CameraSizeY));
            var resIDManager = new ResIDManager();
            var resourceManager = new ResourceManager(game, resIDManager);
            var saveFileManager = new SaveFileManager(game);
            var renderer = new Renderer(game);
            var inputManager = new InputManager();

            game.NewSave("test", settings, chunkManager, gameObjectManager, camera, resourceManager, saveFileManager, renderer, inputManager);
            //game.LoadFromSave("test");

            game.Init();

            game.Run();

            Console.ReadLine();
        }
    }
}