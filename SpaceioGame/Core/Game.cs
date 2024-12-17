using System.Runtime.Serialization;
using ConsoleEngine;

namespace SpaceioGame
{
    sealed class Game : Engine
    {
        int i = 0;
        // Settings
        public int millisecondsPerPlayerMove = 50;
        public const int chunkLoadRadius = 3;

        public int millisecondsSinceLastPlayerMove = 0;
        public bool playerMovedInThisFrame = false;

        public UID shipUID;
        protected override void OnLoad()
        {
            Serializer.knownTypes.Add(typeof(BaseObjectSaveData));
            Serializer.knownTypes.Add(typeof(ShipSaveData));

            ResourceManager.LoadAllBitmapsFromFolder(ResourceManager.resFolderPath);

            ChunkManager.GenerateEmptyChunk(0, 0);
            var sprite = new Sprite(ResourceManager.GetResID("test"));
            var ship = new Ship(new Vec2i(0, 0), 10, this);
            ship.Sprites.Add(sprite);
            shipUID = AddBaseObject(ship);
        }

        protected override void Update()
        {
            if (InputManager.Escape.IsPressed) CloseEngine();

            UpdateCamera();

            var s = (Ship)Find(shipUID);
            s?.MoveGameObject(1, 0);

            //if (i == 3) CloseEngine();
            //else i++;
        }

        public override void Init()
        {
            ChunkManager.GameObjectFromSaveDataInterpreter += GameObjectFromSaveDataInterpreter;
        }

        private GameObject GameObjectFromSaveDataInterpreter(Vec2i chunkIndex, GameObjectSaveData gameObjectSaveData)
        {
            switch (gameObjectSaveData)
            {
                case ShipSaveData shipSaveData:
                    return new Ship(this, chunkIndex, shipSaveData);

                default:
                    return null;
            }
        }
        private void UpdateCamera()
        {
            if (InputManager.ArrowUp.IsPressed) Camera.Move(0, Camera.Position.Y + 3);
            if (InputManager.ArrowDown.IsPressed) Camera.Move(0, Camera.Position.Y - 3);
            if (InputManager.ArrowLeft.IsPressed) Camera.Move(Camera.Position.X - 3, 0);
            if (InputManager.ArrowRight.IsPressed) Camera.Move(Camera.Position.X + 3, 0);
        }
        public UID AddBaseObject(BaseObject baseObject)
        {
            return AddGameObject(baseObject);
        }
        public void RemoveBaseObject(UID baseObjectUID)
        {
            RemoveGameObject(baseObjectUID);
        }

        protected override void OnSave()
        {
            
        }
    }
}

// STORAGE //

//ResourceManager.SaveBitmapToResourceFolder("test", ResourceManager.AddBitmap(new Bitmap(Util.Flatten2dArray(ResourceManager.PrepareData(ResourceManager.test)), new Vec2i(9, 9))));