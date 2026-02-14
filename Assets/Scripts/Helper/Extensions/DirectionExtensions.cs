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
}
