using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleEngine
{
    public class ResourceManager
    {
        private Engine Engine { get; set; }
        private ResIDManager ResIDManager { get; set; }

        public string resFolderPath;

        public ResourceManager(Engine engine, ResIDManager resIDManager)
        {
            Engine = engine;
            ResIDManager = resIDManager;
            Bitmaps = new ReadOnlyDictionary<ResID, Bitmap>(bitmaps);
        }

        internal ResourceManager(Engine engine, ResourceManagerSaveData saveData)
        {
            Engine = engine;
            Bitmaps = new ReadOnlyDictionary<ResID, Bitmap>(bitmaps);
            resFolderPath = $"{engine.pathRootFolder}\\Resources";

            ResIDManager = new ResIDManager(saveData.ResIDManager);
        }

        private Dictionary<string, ResID> namesAndResIDs = new Dictionary<string, ResID>(); 
        private Dictionary<ResID, Bitmap> bitmaps = new Dictionary<ResID, Bitmap>();
        public ReadOnlyDictionary<ResID, Bitmap> Bitmaps { get; }

        internal void Init()
        {
            resFolderPath = $"{Engine.pathRootFolder}\\Resources";
        }

        internal void LoadAllBitmapsFromFolder(string path)
        {
            try
            {
                var filesPaths = Directory.GetFiles(path);

                foreach (var p in filesPaths)
                {
                    var tuple = LoadBitmapFromPath(p);

                    if (bitmaps.ContainsKey(tuple.Item1)) bitmaps[tuple.Item1] = tuple.Item2;
                    else
                    {
                        var resID = AddBitmap(tuple.Item2);
                        namesAndResIDs.Add(Path.GetFileNameWithoutExtension(p), resID);
                    }
                }
            }
            catch { }
        }

        // Returns ResIDManager.InvalidResID if no resource with this name exists
        public ResID GetResID(string name)
        {
            if (namesAndResIDs.ContainsKey(name)) return namesAndResIDs[name];
            else return ResIDManager.InvalidResID;
        }

        public ResID AddBitmap(Bitmap bitmap)
        {
            var resID = ResIDManager.GenerateResID();
            bitmaps.Add(resID, bitmap);
            return resID;
        }

        public void RemoveBitmap(ResID resId)
        {
            bitmaps.Remove(resId);
            ResIDManager.RetireResID(resId);
        }

        private Tuple<ResID, Bitmap> LoadBitmapFromPath(string p)
        {
            var data = File.ReadAllBytes(p);

            var headerSize = sizeof(uint) * 2 + sizeof(int) * 2;
            var header = data.Take(headerSize).ToArray();
            var hTpl = DecodeBitmapHeaderBytes(header);
            var chars = Encoding.UTF8.GetChars(data, headerSize, data.Length - headerSize);
            return new Tuple<ResID, Bitmap>(new ResID(hTpl.Item1, hTpl.Item2), new Bitmap(chars, new Vec2i(hTpl.Item3, hTpl.Item4))); 
        }

        private void SaveBitmapToResourceFolder(string name, ResID resID) 
        {
            //TODO: add checks if already exists (then don't add), don't allow overwrite of existing resources
            namesAndResIDs.Add(name, resID);
            var size = bitmaps[resID].Size;
            var headerBytes = EncodeBitmapHeaderBytes(resID, bitmaps[resID].Size);
            var bitmapDataBytes = Serializer.ToXmlBytes(bitmaps[resID].Data);
            var data = headerBytes.Concat(bitmapDataBytes).ToArray();

            using (var fs = new FileStream($"{resFolderPath}\\{name}\\.txt", FileMode.CreateNew, FileAccess.Write))
            {
                fs.Write(data, 0, data.Length);
            }
        }

        // UID.BaseID, UID.Generation, Size.X, Size.Y
        private Tuple<uint, uint, int, int> DecodeBitmapHeaderBytes(byte[] bytes)
        {
            var baseID = BitConverter.ToUInt32(bytes, 0);
            var generation = BitConverter.ToUInt32(bytes, sizeof(uint));
            var x = BitConverter.ToInt32(bytes, sizeof(uint) * 2);
            var y = BitConverter.ToInt32(bytes, sizeof(uint) * 2 + sizeof(int));
            return new Tuple<uint, uint, int, int>(baseID, generation, x, y);
        }
        private byte[] EncodeBitmapHeaderBytes(ResID resID, Vec2i size)
        {
            var bytes = new byte[sizeof(uint) * 2 + sizeof(int) * 2];
            var baseIDb = BitConverter.GetBytes(resID.BaseID);
            var generationb = BitConverter.GetBytes(resID.Generation);
            var x = BitConverter.GetBytes(size.X);
            var y = BitConverter.GetBytes(size.Y);

            for (var i = 0; i < baseIDb.Length; i++) bytes[i] = baseIDb[i];
            for (var i = baseIDb.Length; i < baseIDb.Length + generationb.Length; i++) bytes[i] = generationb[i];
            for (var i = baseIDb.Length + generationb.Length; i < baseIDb.Length + generationb.Length + x.Length; i++) bytes[i] = x[i];
            for (var i = baseIDb.Length + generationb.Length + x.Length; i < bytes.Length; i++) bytes[i] = y[i];

            return bytes;   
        }

        internal ResourceManagerSaveData GetSaveData()
        {
            var sd = new ResourceManagerSaveData();
            sd.ResIDManager = ResIDManager.GetSaveData();
            return sd;
        }

        public static char[,] test2D = new char[9, 9]
        {
                {'L',' ', '-', '-', 'T','-', '-', ' ', 'R'},
                {' ',' ', ' ', ' ', ' ',' ', ' ', ' ', ' '},
                {'I',' ', ' ', ' ', ' ',' ', ' ', ' ', 'I'},
                {'I',' ', ' ', ' ', ' ',' ', ' ', ' ', 'I'},
                {'I',' ', ' ', ' ', 'M',' ', ' ', ' ', 'I'},
                {'I',' ', ' ', ' ', ' ',' ', ' ', ' ', 'I'},
                {'I',' ', ' ', ' ', ' ',' ', ' ', ' ', 'I'},
                {' ',' ', ' ', ' ', ' ',' ', ' ', ' ', ' '},
                {'L',' ', '-', '-', 'B','-', '-', ' ', 'R'},
        };
        public static char[] test = Util.Flatten2dArray(test2D);

        public static Bitmap fighter1Up;
        public static Bitmap fighter1Down;
        public static Bitmap fighter1Right;
        public static Bitmap fighter1Left;

        public static Bitmap fighter1UpLeft;
        public static Bitmap fighter1UpRight;
        public static Bitmap fighter1DownLeft;
        public static Bitmap fighter1DownRight;

        public static Bitmap enemyDefault;
        public static Bitmap enemyDefault2;

        public static Bitmap asteroid;
        public static void Initialize()
        {
            /*
            #region FighterSprites

            var fighter1UpBitmap = new char[9, 9]
            {
                {' ',' ', ' ', ' ', '%',' ', ' ', ' ', ' '},
                {' ',' ', ' ', ' ', '%',' ', ' ', ' ', ' '},
                {' ',' ', ' ', '%', '.','%', ' ', ' ', ' '},
                {' ',' ', ' ', '%', '.','%', ' ', ' ', ' '},
                {' ',' ', '%', '.', 'O','.', '%', ' ', ' '},
                {'!',' ', '%', '.', 'O','.', '%', ' ', '!'},
                {'|','%', '.', '.', '.','.', '.', '%', '|'},
                {'@','%', '%', '%', '%','%', '%', '%', '@'},
                {' ',' ', '\\', '#', ' ','#', '/', ' ', ' '},
            };
            fighter1Up = new Bitmap(PrepareData(fighter1UpBitmap));

            var fighter1DownBitmap = new char[9, 9]
            {
                {' ',' ', '/', '#', ' ','#', '\\', ' ', ' '},
                {'@','%', '%', '%', '%','%', '%', '%', '@'},
                {'|','%', '.', '.', '.','.', '.', '%', '|'},
                {'!',' ', '%', '.', 'O','.', '%', ' ', '!'},
                {' ',' ', '%', '.', 'O','.', '%', ' ', ' '},
                {' ',' ', ' ', '%', '.','%', ' ', ' ', ' '},
                {' ',' ', ' ', '%', '.','%', ' ', ' ', ' '},
                {' ',' ', ' ', ' ', '%',' ', ' ', ' ', ' '},
                {' ',' ', ' ', ' ', '%',' ', ' ', ' ', ' '},
            };
            fighter1Down = new Bitmap(PrepareData(fighter1DownBitmap));

            var fighter1RightBitmap = new char[9, 9]
            {
                {' ','@', '―', '-', ' ',' ', ' ', ' ', ' '},
                {' ','%', '%', ' ', ' ',' ', ' ', ' ', ' '},
                {'/','%', '.', '%', '%',' ', ' ', ' ', ' '},
                {'#','%', '.', '.', '.','%', '%', ' ', ' '},
                {' ','%', '.', 'O', 'O','.', '.', '%', '%'},
                {'#','%', '.', '.', '.','%', '%', ' ', ' '},
                {'\\','%', '.', '%', '%',' ', ' ', ' ', ' '},
                {' ','%', '%', ' ', ' ',' ', ' ', ' ', ' '},
                {' ','@', '―', '-', ' ',' ', ' ', ' ', ' '},
            };
            fighter1Right = new Bitmap(PrepareData(fighter1RightBitmap));


            var fighter1LeftBitmap = new char[9, 9]
            {
                {' ',' ', ' ', ' ', ' ','-', '―', '@', ' '},
                {' ',' ', ' ', ' ', ' ',' ', '%', '%', ' '},
                {' ',' ', ' ', ' ', '%','%', '.', '%', '\\'},
                {' ',' ', '%', '%', '.','.', '.', '%', '#'},
                {'%','%', '.', '.', 'O','O', '.', '%', ' '},
                {' ',' ', '%', '%', '.','.', '.', '%', '#'},
                {' ',' ', ' ', ' ', '%','%', '.', '%', '/'},
                {' ',' ', ' ', ' ', ' ',' ', '%', '%', ' '},
                {' ',' ', ' ', ' ', ' ','-', '―', '@', ' '},
            };
            fighter1Left = new Bitmap(PrepareData(fighter1LeftBitmap));


            var fighter1UpLeftBitmap = new char[9, 9]
            {
                {'%','%', '%', ' ', ' ',' ', '\\', ' ', ' '},
                {'%','.', '.', '%', '%','%', ' ', '\\', ' '},
                {'%','.', '.', '.', '.','.', '%', '%', '@'},
                {' ','%', '.', 'O', '.','.', '.', '%', ' '},
                {' ','%', '.', '.', 'O','.', '%', '|', ' '},
                {' ','%', '.', '.', '.','%', ' ', '#', ' '},
                {'\\',' ', '%', '.', '%','.', ' ', ' ', ' '},
                {' ','\\', '%', '%', '―','#', ' ', ' ', ' '},
                {' ',' ', '@', ' ', ' ',' ', ' ', ' ', ' '}
            };
            fighter1UpLeft = new Bitmap(PrepareData(fighter1UpLeftBitmap));

            var fighter1UpRightBitmap = new char[9, 9]
            {
                {' ',' ', '/', ' ', ' ',' ', '%', '%', '%'},
                {' ','/', '.', '%', '%','%', '.', '.', '%'},
                {'@','%', '%', '.', '.','.', '.', '.', '%'},
                {' ','%', '.', '.', '.','O', '.', '%', ' '},
                {' ','|', '%', '.', 'O','.', '.', '%', ' '},
                {' ','#', ' ', '%', '.','.', '.', '%', ' '},
                {' ',' ', ' ', ' ', '%','.', '%', ' ', '/'},
                {' ',' ', ' ', '#', '―','%', '%', '/', ' '},
                {' ',' ', ' ', ' ', ' ',' ', '@', ' ', ' '}
            };
            fighter1UpRight = new Bitmap(PrepareData(fighter1UpRightBitmap));

            var fighter1DownLeftBitmap = new char[9, 9]
            {
                {' ',' ', '@', ' ', ' ',' ', ' ', ' ', ' '},
                {' ','/', '%', '%', '―','#', ' ', ' ', ' '},
                {'/',' ', '%', '.', '%',' ', ' ', ' ', ' '},
                {' ','%', '.', '.', '.','%', ' ', '#', ' '},
                {' ','%', '.', '.', 'O','.', '%', '|', ' '},
                {' ','%', '.', 'O', '.','.', '.', '%', ' '},
                {'%','.', '.', '.', '.','.', '%', '%', '@'},
                {'%','.', '.', '%', '%','%', ' ', '/', ' '},
                {'%','%', '%', ' ', ' ',' ', '/', ' ', ' '},
            };
            fighter1DownLeft = new Bitmap(PrepareData(fighter1DownLeftBitmap));

            var fighter1DownRightBitmap = new char[9, 9]
            {
                {' ',' ', ' ', ' ', ' ',' ', '@', ' ', ' '},
                {' ',' ', ' ', '#', '―','%', '%', '\\', ' '},
                {' ',' ', ' ', ' ', '%','.', '%', ' ', '\\'},
                {' ','#', ' ', '%', '.','.', '.', '%', ' '},
                {' ','|', '%', '.', 'O','.', '.', '%', ' '},
                {' ','%', '.', '.', '.','O', '.', '%', ' '},
                {'@','%', '%', '.', '.','.', '.', '.', '%'},
                {' ','\\', ' ', '%', '%','%', '.', '.', '%'},
                {' ',' ', '\\', ' ', ' ',' ', '%', '%', '%'},
            };
            fighter1DownRight = new Bitmap(PrepareData(fighter1DownRightBitmap));
            #endregion

            #region EnemyDefaults

            var enemyDefaultBitmap = new char[4, 3]
            {
                {' ', '#', ' '},
                {'#', '+', '#'},
                {'#', '+', '#'},
                {'#', '+', '#'},
            };
            enemyDefault = new Bitmap(PrepareData(enemyDefaultBitmap));

            var enemyDefault2Bitmap = new char[4, 3]
            {
                {' ', '#', ' '},
                {'#', ' ', '#'},
                {'#', ' ', '#'},
                {'#', ' ', '#'},
            };
            enemyDefault2 = new Bitmap(PrepareData(enemyDefault2Bitmap));
            #endregion

            var asteroidBitmap = new char[18, 18]
            {
                {' ', ' ', ' ', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', ' ', ' ', ' ', ' ', ' ', ' ', ' '},
                {' ', ' ', ' ', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', ' ', ' ', ' ', ' ', ' ', ' ', ' '},
                {' ', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', ' ', ' ', ' '},
                {' ', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', ' ', ' ', ' '},
                {' ', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', ' ', ' ', ' '},
                {' ', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', ' ', ' ', ' '},
                {' ', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', ' ', ' ', ' '},
                {' ', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛'},
                {' ', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛'},
                {'⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛'},
                {'⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛'},
                {'⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛'},
                {'⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛'},
                {'⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛'},
                {' ', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛'},
                {' ', ' ', ' ', ' ', ' ', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛'},
                {' ', ' ', ' ', ' ', ' ', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛'},
                {' ', ' ', ' ', ' ', ' ', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛', '⬛'},
            };

            asteroid = new Bitmap(PrepareData(asteroidBitmap));
            */
        }


        // Rotates the data 90 degrees to the left to a allow a cleaner representation of sprites in char[]
        public static char[,] PrepareData(char[,] prior)
        {
            var priorX = prior.GetLength(0);
            var priorY = prior.GetLength(1);

            var data = new char[priorY, priorX];

            for (var x = 0; x < data.GetLength(0); x++)
            {
                for (var y = 0; y < data.GetLength(1); y++)
                {
                    data[x, y] = prior[priorX - y - 1, x];
                }
            }

            return data;
        }
    }
}
