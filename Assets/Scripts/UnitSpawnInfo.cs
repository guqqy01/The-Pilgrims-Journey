public class UnitSpawnInfo
{
    public UnitSO unit;
    public float spawnRate;
    public float nextSpawnTime;

    public UnitSpawnInfo(Tile tile, float nextSpawnTime, int spawnRateMultiplier)
    {
        unit = tile.unit;
        spawnRate = unit.BaseSpawnRate * spawnRateMultiplier;
        this.nextSpawnTime = nextSpawnTime;
    }

    public void AddSpawnRate(int multiplier)
    {
        spawnRate += unit.BaseSpawnRate * multiplier;
    }
}
