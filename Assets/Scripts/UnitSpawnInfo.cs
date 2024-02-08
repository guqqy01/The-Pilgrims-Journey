public class UnitSpawnInfo
{
    public UnitSO unit;
    public TileStats tileStats;

    public float spawnRate;
    public float nextSpawnTime;

    public UnitSpawnInfo(Tile tile, float nextSpawnTime, float spawnRateMultiplier)
    {
        unit = tile.unit;
        tileStats = tile.tileStats;
        spawnRate = unit.BaseSpawnRate * spawnRateMultiplier;
        this.nextSpawnTime = nextSpawnTime;
    }

    public void AddSpawnRate(float multiplier)
    {
        spawnRate += unit.BaseSpawnRate * multiplier;
    }
}
