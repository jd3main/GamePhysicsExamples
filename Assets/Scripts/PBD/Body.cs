using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace PBD
{
    public abstract class Body
    {
        public abstract void SyncStatus();

        public abstract Vector3 X { get; set; }
        public abstract Vector3 V { get; set; }
        public abstract Vector3 P { get; set; }
        public abstract float W { get; set; }
        public abstract Matrix4x4 I { get; set; }
        public abstract Quaternion Rotation { get; set; }
    }
}