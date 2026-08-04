using System;

public enum Direction
{
    North = 2,
    South = -2,
    West = -1,
    East = 1,
}

[Flags]
public enum FlagedDirection
{
    North = 1,
    South = 2,
    West = 4,
    East = 8,
}

