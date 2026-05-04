using System;
using ChromaCube.Core;

namespace ChromaCube.Data
{
    [Serializable]
    public struct FaceColorEntry
    {
        public FaceKey face;
        public string colorId;
    }
}
