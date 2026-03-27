using UnityEngine;

public class Grid3x3<T>
{
    private readonly T[,] grid = new T[3, 3];

    public T this[int i, int j]
    {
        get
        {
            return grid[i, j];
        }
        set
        {
            grid[i, j] = value;
        }
    }

    public Grid3x3(){}

    public Grid3x3(T[,] grid3x3)
    {
        if (grid3x3 == null)
            return;

        if (grid3x3.GetLength(0) != 3 || grid3x3.GetLength(1) != 3)
        {
            for (int i = 0; i < Mathf.Min(grid3x3.GetLength(0), 3); i++)
            {
                for (int j = 0; j < Mathf.Min(grid3x3.GetLength(1), 3); j++)
                {
                    grid[i, j] = grid3x3[i, j];
                }
            }

            return;
        }

        grid = grid3x3;
    }

    #region Properties

    public T TopLeft
    {
        get
        {
            return this[0, 0];
        }
        set
        {
            grid[0, 0] = value;
        }
    }

    public T TopCenter
    {
        get
        {
            return this[1, 0];
        }
        set
        {
            grid[1, 0] = value;
        }
    }

    public T TopRight
    {
        get
        {
            return this[2, 0];
        }
        set
        {
            grid[2, 0] = value;
        }
    }

    public T CenterLeft
    {
        get
        {
            return this[0, 1];
        }
        set
        {
            grid[0, 1] = value;
        }
    }

    public T CenterCenter
    {
        get
        {
            return this[1, 1];
        }
        set
        {
            grid[1, 1] = value;
        }
    }

    public T CenterRight
    {
        get
        {
            return this[2, 1];
        }
        set
        {
            grid[2, 1] = value;
        }
    }

    public T BottomLeft
    {
        get
        {
            return this[0, 2];
        }
        set
        {
            grid[0, 2] = value;
        }
    }

    public T BottomCenter
    {
        get
        {
            return this[1, 2];
        }
        set
        {
            grid[1, 2] = value;
        }
    }

    public T BottomRight
    {
        get
        {
            return this[2, 2];
        }
        set
        {
            grid[2, 2] = value;
        }
    }

    #endregion

    public static implicit operator Grid3x3<T>(T[,] value) => new Grid3x3<T>(value);
    public static implicit operator T[,](Grid3x3<T> value) => value.grid;
}