using System.Runtime.Serialization;
using ConsoleEngine;

namespace SpaceioGame
{
    abstract class BaseObject : GameObject
    {
        public Game Game { get; set; }

        public BaseObject(Vec2i position, Game game) : base(position, game)
        {
            Game = game;
        }

        public BaseObject(Game game, Vec2i chunkIndex, BaseObjectSaveData saveData) : base (game, chunkIndex, saveData)
        {
            Game = game;
        }

        protected override void OnCollision(GameObject collidingObject)
        {

        }

        public override GameObjectSaveData GetSaveData()
        {
            var gameObjectSaveData = base.GetSaveData();
            var baseObjectSaveData = new BaseObjectSaveData(gameObjectSaveData);

            // things specific for baseObjectSaveData

            return baseObjectSaveData;
        }
    }
}
