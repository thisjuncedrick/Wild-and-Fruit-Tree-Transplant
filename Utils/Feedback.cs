using StardewValley;

namespace Wild_and_Fruit_Tree_Transplant.Utils
{
  internal sealed class Feedback
  {
    public const string Cancel = "cancel";
    public const string Pickup = "axchop";
    public const string Drop = "dirtyHit";
    public const string Deduct = "purchase";
    public const string Clear = "shwip";

    public static void Message(string message, int type = 3)
    {
      Game1.addHUDMessage(new HUDMessage(message, type));
    }
  }
}
