using System;
using UnityEngine;

namespace ChromaCube.Core
{
    [Serializable]
    public struct CubeOrientation
    {
        public FaceKey top;
        public FaceKey bottom;
        public FaceKey north;
        public FaceKey south;
        public FaceKey east;
        public FaceKey west;

        /// <summary>
        /// Derive which logical face is pointing DOWN from the cube's actual world rotation.
        /// This is the ground-truth source for capture logic — it accounts for edge transitions
        /// and frame rotations that the purely logical Roll() method cannot track.
        /// </summary>
        public static FaceKey GetBottomFaceFromRotation(Quaternion worldRotation)
        {
            // The six local face normals in identity orientation
            var directions = new[]
            {
                (FaceKey.Top,    Vector3.up),
                (FaceKey.Bottom, Vector3.down),
                (FaceKey.North,  Vector3.forward),
                (FaceKey.South,  Vector3.back),
                (FaceKey.East,   Vector3.right),
                (FaceKey.West,   Vector3.left)
            };

            var worldDown = Vector3.down;
            var bestFace = FaceKey.Bottom;
            var bestDot = float.NegativeInfinity;

            foreach (var (face, localDir) in directions)
            {
                var worldDir = worldRotation * localDir;
                var dot = Vector3.Dot(worldDir, worldDown);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestFace = face;
                }
            }

            return bestFace;
        }

        public static CubeOrientation Identity()
        {
            return new CubeOrientation
            {
                top = FaceKey.Top,
                bottom = FaceKey.Bottom,
                north = FaceKey.North,
                south = FaceKey.South,
                east = FaceKey.East,
                west = FaceKey.West
            };
        }

        public CubeOrientation(FaceKey top, FaceKey bottom, FaceKey north, FaceKey south, FaceKey east, FaceKey west)
        {
            this.top = top;
            this.bottom = bottom;
            this.north = north;
            this.south = south;
            this.east = east;
            this.west = west;
        }

        public FaceKey[] SerializeFaceStates()
        {
            return new[] { top, bottom, north, south, east, west };
        }

        public FaceKey GetBottomFace()
        {
            return bottom;
        }

        public CubeOrientation Clone()
        {
            return this;
        }

        public void Roll(Direction direction)
        {
            var previous = this;

            switch (direction)
            {
                case Direction.North:
                    top = previous.south;
                    bottom = previous.north;
                    north = previous.top;
                    south = previous.bottom;
                    east = previous.east;
                    west = previous.west;
                    break;
                case Direction.South:
                    top = previous.north;
                    bottom = previous.south;
                    north = previous.bottom;
                    south = previous.top;
                    east = previous.east;
                    west = previous.west;
                    break;
                case Direction.East:
                    top = previous.west;
                    bottom = previous.east;
                    north = previous.north;
                    south = previous.south;
                    east = previous.top;
                    west = previous.bottom;
                    break;
                case Direction.West:
                    top = previous.east;
                    bottom = previous.west;
                    north = previous.north;
                    south = previous.south;
                    east = previous.bottom;
                    west = previous.top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public override string ToString()
        {
            return $"Top={top}, Bottom={bottom}, North={north}, South={south}, East={east}, West={west}";
        }
    }
}
