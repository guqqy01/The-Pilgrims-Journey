public class UnitSpawnInfo
{
    public Unit unit;
    public float spawnRate;
    public float nextSpawnTime;

    public UnitSpawnInfo(Tile tile, float nextSpawnTime, int spawnRateMultiplier)
    {
        unit = tile.unit;
        spawnRate = unit.baseSpawnRate * spawnRateMultiplier;
        this.nextSpawnTime = nextSpawnTime;
    }

    public void AddSpawnRate(int multiplier)
    {
        spawnRate += unit.baseSpawnRate * multiplier;
    }
}
