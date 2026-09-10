using System;

[Flags]
public enum AddBuffParameters
{
    None = 0,
    Multiplier = 1,
    Base = 2,
    Additional = 4,
    All = Multiplier | Base | Additional
}
