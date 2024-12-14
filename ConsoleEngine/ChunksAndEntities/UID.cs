﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleEngine
{
    [DataContract]
    public readonly struct UID : IEquatable<UID>
    {
        [DataMember] public readonly uint BaseID;
        [DataMember] public readonly uint Generation;
        internal UID(uint ID = uint.MaxValue, uint generation = uint.MaxValue)    
        {
            BaseID = ID;
            Generation = generation;
        }
        public bool Equals(UID other) => BaseID == other.BaseID && Generation == other.Generation;
        public override string ToString() => $"{BaseID}-{Generation}";
        public override bool Equals(object obj) => obj is UID other && Equals(other);
        public static bool operator ==(UID left, UID right) => left.Equals(right);
        public static bool operator !=(UID left, UID right) => !left.Equals(right); 
        public override int GetHashCode() 
        {
            unchecked
            {
                int hash = 17; // A prime number for initial hash
                hash = hash * 23 + BaseID.GetHashCode(); // Combine Part1
                hash = hash * 23 + Generation.GetHashCode(); // Combine Part2
                return hash;
            }
        }

        public static UID InvalidUID() => new UID(uint.MaxValue, uint.MaxValue);
    }
}
