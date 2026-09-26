namespace TPJ.Gameplay
{
    // Passive — resolved from the victim's side above, but still need entries
    // in the registry so CollapseFlags() dispatch doesn't hit a missing key.
    public class GuardBehavior : TileCardTypeBehavior { public override TileCardType HandledType => TileCardType.Guard; }
}