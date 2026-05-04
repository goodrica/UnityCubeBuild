using System;

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
