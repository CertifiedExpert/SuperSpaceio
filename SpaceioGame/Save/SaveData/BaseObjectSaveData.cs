using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ConsoleEngine;

namespace SpaceioGame
{
    [DataContract]
    public class BaseObjectSaveData : GameObjectSaveData
    {
        public BaseObjectSaveData(GameObjectSaveData gameObjectSaveData) : base (gameObjectSaveData)
        {

        }
    }
}
