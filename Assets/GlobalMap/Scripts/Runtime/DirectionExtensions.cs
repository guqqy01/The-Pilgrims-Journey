using System.Collections.Generic;

public static class DirectionExtensions
{
    public static Direction OppositeDirection(this Direction direction)
    {
        return (Direction)(-(int)direction);
    }

    public static List<Direction> TangentDirections(this Direction direction)
    {
        List<Direction> returnList = new List<Direction>();
        List<Direction> allDirections = new List<Direction>() { Direction.South, Direction.West, Direction.East, Direction.North };

        foreach (Direction dir in allDirections)
        {
            if (dir != direction && dir != direction.OppositeDirection())
                returnList.Add(dir);
        }

        return returnList;
    }

    public static List<Direction> ConvertToListOfDirections(this FlagedDirection flagedDirection)
    {
        List<Direction> returnList = new List<Direction>();

        if (flagedDirection.HasFlag(FlagedDirection.North))
        {
            returnList.Add(Direction.North);
        }

        if (flagedDirection.HasFlag(FlagedDirection.South))
        {
            returnList.Add(Direction.South);
        }

        if (flagedDirection.HasFlag(FlagedDirection.East))
        {
            returnList.Add(Direction.East);
        }

        if (flagedDirection.HasFlag(FlagedDirection.West))
        {
            returnList.Add(Direction.West);
        }

        return returnList;
    }

    public static FlagedDirection ConvertToFlagedDirection(this Direction flagedDirection)
    {
        switch (flagedDirection)
        {
            case Direction.North:
                return FlagedDirection.North;
            case Direction.South:
                return FlagedDirection.South;
            case Direction.West:
                return FlagedDirection.West;
            case Direction.East:
                return FlagedDirection.East;
            default:
                return default;
        }
    }
}
