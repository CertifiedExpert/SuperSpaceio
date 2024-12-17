using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ConsoleEngine
{
    [DataContract]
    internal class ResourceManagerSaveData
    {
        [DataMember] public ResIDManagerSaveData ResIDManager;
        [DataMember] public BijectiveDictionary<string, ResID> namesAndResIDs;
    }
}