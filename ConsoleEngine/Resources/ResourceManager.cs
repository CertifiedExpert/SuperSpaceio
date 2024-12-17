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
            namesAndResIDs = saveData.namesAndResIDs;
            foreach (var resID in namesAndResIDs.Values) bitmaps.Add(resID, null);
        }

        private BijectiveDictionary<string, ResID> namesAndResIDs = new BijectiveDictionary<string, ResID>();
        private Dictionary<ResID, Bitmap> bitmaps = new Dictionary<ResID, Bitmap>();
        public ReadOnlyDictionary<ResID, Bitmap> Bitmaps { get; }

        internal void Init()
        {
            resFolderPath = $"{Engine.pathRootFolder}\\Resources";
        }

        public void LoadAllBitmapsFromFolder(string path)
        {
            try
            {
                var filesPaths = Directory.GetFiles(path);

                foreach (var p in filesPaths)
                {
                    var tuple = LoadBitmapFromPath(p);

                    bitmaps[tuple.Item1] = tuple.Item2;
                }
            }
            catch { }
        }

        // Returns ResIDManager.InvalidResID if no resource with this name exists
        public ResID GetResID(string name) //TODO: is this even needed?
        {
            if (namesAndResIDs.ContainsKey(name)) return namesAndResIDs.GetValue(name);
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
            var result = new Tuple<ResID, Bitmap>(new ResID(hTpl.Item1, hTpl.Item2), new Bitmap(chars, new Vec2i(hTpl.Item3, hTpl.Item4))); 

            var name = Path.GetFileNameWithoutExtension(p);
            if (!namesAndResIDs.ContainsKey(name)) namesAndResIDs.Add(name, result.Item1);

            return result;
        }

        public void SaveBitmapToResourceFolder(string name, ResID resID) // TODO: instead of using dataserializer save as pure bytes to reduce space
        {
            //TODO: add checks if already exists (then don't add), don't allow overwrite of existing resources
            if (!namesAndResIDs.ContainsKey(name)) namesAndResIDs.Add(name, resID);

            var size = bitmaps[resID].Size;
            var headerBytes = EncodeBitmapHeaderBytes(resID, bitmaps[resID].Size);
            //var bitmapDataBytes = Serializer.ToXmlBytes(bitmaps[resID].Data);
            var bitmapDataBytes = Encoding.UTF8.GetBytes(bitmaps[resID].Data);
            var data = headerBytes.Concat(bitmapDataBytes).ToArray();

            using (var fs = new FileStream($"{resFolderPath}\\{name}.txt", FileMode.CreateNew, FileAccess.Write))
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
            for (var i = 0; i < generationb.Length; i++) bytes[sizeof(uint) + i] = generationb[i];
            for (var i = 0; i < x.Length; i++) bytes[sizeof(uint) * 2+ i] = x[i];
            for (var i = 0; i < y.Length; i++) bytes[sizeof(uint) * 2 + sizeof(int) + i] = y[i];

            return bytes;   
        }
        internal void OnSave()
        {
            foreach (var resID in bitmaps.Keys)
            {
                // Checks if this bitmap was dynamically created and does not have a save file. Remove it before saving.
                if (!namesAndResIDs.ContainsValue(resID)) RemoveBitmap(resID);
            }
        }

        internal ResourceManagerSaveData GetSaveData()
        {
            var sd = new ResourceManagerSaveData();
            sd.ResIDManager = ResIDManager.GetSaveData();
            sd.namesAndResIDs = namesAndResIDs;
            return sd;
        }

        public static char[,] test = new char[9, 9]
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


        public static char[,] fighter1Up = new char[9, 9]
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
        public static char[,] fighter1Down = new char[9, 9]
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
        public static char[,] fighter1Right = new char[9, 9]
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
        public static char[,] fighter1Left = new char[9, 9]
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

        public static char[,] fighter1UpLeft = new char[9, 9]
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
        public static char[,] fighter1UpRight = new char[9, 9]
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
        public static char[,] fighter1DownLeft = new char[9, 9]
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
        public static char[,] fighter1DownRight = new char[9, 9]
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

        public static char[,] enemyDefault1 = new char[4, 3]
            {
                {' ', '#', ' '},
                {'#', '+', '#'},
                {'#', '+', '#'},
                {'#', '+', '#'},
            };
        public static char[,] enemyDefault2 = new char[4, 3]
            {
                {' ', '#', ' '},
                {'#', ' ', '#'},
                {'#', ' ', '#'},
                {'#', ' ', '#'},
            };

        public static char[,] asteroid = new char[18, 18]
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
        public static void Initialize()
        {
            
            
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
