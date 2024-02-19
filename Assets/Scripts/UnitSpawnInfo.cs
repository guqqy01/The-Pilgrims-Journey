using Units;

public class UnitSpawnInfo
{
    public Tile tile;
    public TileStats tileStats;

    public float spawnRate;
    public float nextSpawnTime;

    public UnitSpawnInfo(Tile tile, float nextSpawnTime, float spawnRateMultiplier)
    {
        this.tile = tile;
        tileStats = tile.tileStats;
        spawnRate = tile.unit.BaseSpawnRate * spawnRateMultiplier;
        this.nextSpawnTime = nextSpawnTime;
    }

    public void AddSpawnRate(float multiplier)
    {
        spawnRate += tile.unit.BaseSpawnRate * multiplier;
    }
}
