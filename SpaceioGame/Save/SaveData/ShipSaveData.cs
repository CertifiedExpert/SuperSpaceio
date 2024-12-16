using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SpaceioGame
{
    [DataContract]
    public class ShipSaveData : BaseObjectSaveData
    {
        public ShipSaveData(BaseObjectSaveData baseObjectSaveData) : base(baseObjectSaveData)
        {

        }
    }
}
