using Microsoft.Xna.Framework;

using StardewValley;
using StardewValley.TerrainFeatures;

namespace Wild_and_Fruit_Tree_Transplant.Core
{
  public enum SelectionResult { 
    Success, 
    NoTreeFound, 
    TreeIsTapped, 
    TreeIsStump, 
    TreeIsStruckByLightning, 
    TreeIsYoung 
  }

  internal sealed class TransplantService
  {
    public TerrainFeature?  SelectedTree { get; private set; }
    private GameLocation?   originLocation;
    private Vector2         originTile;

    private List<Vector2>?              cachedFootprint;
    private Dictionary<Vector2, bool>?  cachedFootprintTiles;
    private Vector2?                    cachedTargetTile;

    private readonly ModConfig config;

    public TransplantService(ModConfig config) 
      => this.config = config;

    public SelectionResult TrySelectTree(GameLocation location, Vector2 currentTile)
    {
      var snappedTile = new Vector2((int)currentTile.X, (int)currentTile.Y);

      if (!location.terrainFeatures.TryGetValue(snappedTile, out var tree) ||
          tree is not (Tree or FruitTree))
        return SelectionResult.NoTreeFound;

      if (tree is Tree wildTree)
      {
        if (wildTree.growthStage.Value < Tree.treeStage && config.OnlyMatureTrees)
          return SelectionResult.TreeIsYoung;

        if (wildTree.tapped.Value && !config.AllowTapped)
          return SelectionResult.TreeIsTapped;

        if (wildTree.stump.Value && !config.AllowStumps)
          return SelectionResult.TreeIsStump;
      }
      else if (tree is FruitTree fruitTree)
      {
        if (fruitTree.growthStage.Value < FruitTree.treeStage && config.OnlyMatureTrees)
          return SelectionResult.TreeIsYoung;

        if (fruitTree.struckByLightningCountdown.Value > 0 && !config.AllowLightningStruck)
          return SelectionResult.TreeIsStruckByLightning;

        if (fruitTree.stump.Value && !config.AllowStumps)
          return SelectionResult.TreeIsStump;
      }

      SelectedTree    = tree;
      originTile      = snappedTile;
      originLocation  = location;

      location.terrainFeatures.Remove(snappedTile);
      return SelectionResult.Success;
    }

    public void ClearSelection()
    {
      SelectedTree         = null;
      originLocation       = null;
      cachedTargetTile     = null;
      cachedFootprint      = null;
      cachedFootprintTiles = null;
    }

    public void ReturnTreeToOrigin()
    {
      if (SelectedTree is not null && originLocation is not null)
        originLocation.terrainFeatures.TryAdd(originTile, SelectedTree);
    }

    /* ---- Transplant ---- */
    public void ExecuteTransplant(GameLocation location, Vector2 targetTile)
    {
      if (SelectedTree is Tree wt)
        wt.Tile = targetTile;
      else if (SelectedTree is FruitTree ft)
        ft.Tile = targetTile;

      if (location.terrainFeatures.TryAdd(targetTile, SelectedTree))
        ClearSelection();
    }

    /* ---- Placement Validation ---- */
    public Dictionary<Vector2, bool> GetFootprintTiles(GameLocation location, Vector2 targetTile)
    {
      if (cachedFootprintTiles is not null && targetTile == cachedTargetTile)
        return cachedFootprintTiles;

      var footprint = GetFootprint(targetTile);
      var result    = new Dictionary<Vector2, bool>(footprint.Count);
      
      foreach (var tile in footprint)
        result[tile] = TileIsFree(location, tile);

      cachedFootprintTiles = result;
      return result;
    }




    public bool IsValidPlacement(GameLocation location, Vector2 targetTile)
      => GetFootprintTiles(location, targetTile).All(kv => kv.Value);

    /* ---- Helper ---- */
    private List<Vector2> GetFootprint(Vector2 targetTile)
    {
      if (cachedFootprint is not null && targetTile == cachedTargetTile)
        return cachedFootprint;
      
      List<Vector2> result = new();

      if (SelectedTree is null)
        return result;
     
      if (SelectedTree is FruitTree && !config.ForceTransplant)
        result = Utility
          .getSurroundingTileLocationsArray(targetTile)
          .Append(targetTile)
          .ToList();
      else
        result = new List<Vector2> { targetTile };

      cachedFootprint = result;
      cachedTargetTile = targetTile;
      return result;
    }

    private bool TileIsFree(GameLocation location, Vector2 tile)
    {
      int x = (int)tile.X;
      int y = (int)tile.Y;

      // Basic checks (Objects/Features)
      if (location.objects.ContainsKey(tile) || location.terrainFeatures.ContainsKey(tile))
        return false;

      // 2. Map-Level Build Check, mimics Robin's carpentermenu
      if (!location.isBuildable(tile, true))
        return false;

      // 3. Tile Property Check
      if (location.doesTileHaveProperty(x, y, "Passable", "Back") == "F" ||
          location.doesTileHaveProperty(x, y, "Water", "Back") != null)
      {
        return false;
      }

      // 4. Force Transplant bypass
      if (config.ForceTransplant) 
        return true;

      // 5. Tree-Specific Proximity Logic
      return !(SelectedTree is FruitTree ? IsTooCloseToFruitTree(location, tile) : IsTooCloseToWildTree(location, tile));
    }


    private static bool IsTooCloseToFruitTree(GameLocation location, Vector2 targetTile)
    {
      for (int x = (int)targetTile.X - 1; x <= (int)targetTile.X + 1; x++)
        for (int y = (int)targetTile.Y - 1; y <= (int)targetTile.Y + 1; y++)
        {
          if (x == (int)targetTile.X && y == (int)targetTile.Y) continue;
          if (location.terrainFeatures.TryGetValue(new Vector2(x, y), out var f) && f is FruitTree)
            return true;
        }
      return false;
    }

    private static bool IsTooCloseToWildTree(GameLocation location, Vector2 targetTile)
    {
      Vector2[] adjacent = {
        new(targetTile.X - 1, targetTile.Y),
        new(targetTile.X + 1, targetTile.Y),
        new(targetTile.X, targetTile.Y - 1),
        new(targetTile.X, targetTile.Y + 1)
    };

      foreach (var v in adjacent)
        if (location.terrainFeatures.TryGetValue(v, out var f) && f is Tree)
          return true;

      return false;
    }
  }
}
