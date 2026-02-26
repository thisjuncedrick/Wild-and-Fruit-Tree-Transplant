using Microsoft.Xna.Framework;

using StardewValley;

namespace Wild_and_Fruit_Tree_Transplant.Core
{
  internal record Location(
    string      Key,
    string      DisplayName,
    Vector2     Tile,
    Func<bool>? Condition = null
  );

  internal static class LocationRegistry
  {
    private static readonly xTile.Dimensions.Location FallbackViewport =
            new(49 * Game1.tileSize, 5 * Game1.tileSize);

    private static readonly List<Location> RegisteredLocations = new();

    public static void Register(Location location) => RegisteredLocations.Add(location);

    public static void Clear() => RegisteredLocations.Clear();

    public static Response[] ToResponse()
    {
      var responses = RegisteredLocations
        .Where(l=> l.Condition is null || l.Condition())
        .Select(l => new Response(l.Key, l.DisplayName))
        .ToList();

      responses.Add(new Response(
        "Leave",
        Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Leave")
      ));

      return responses.ToArray();
    }

    public static xTile.Dimensions.Location GetViewport(string key)
    {
      var entry = RegisteredLocations.First(l => l.Key == key);
      return entry is not null ? CenterViewportOn(entry.Tile) : FallbackViewport;
    }

    private static xTile.Dimensions.Location CenterViewportOn(Vector2 tile)
    {
      return new xTile.Dimensions.Location(
        (int)((tile.X - Game1.viewport.Width / Game1.tileSize / 2) * Game1.tileSize),
        (int)((tile.Y - Game1.viewport.Height / Game1.tileSize / 2) * Game1.tileSize)
      );
    }
  }
}
