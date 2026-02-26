using Microsoft.Xna.Framework;
using StardewValley;
using Wild_and_Fruit_Tree_Transplant.Core;
using Wild_and_Fruit_Tree_Transplant.Utils;

namespace Wild_and_Fruit_Tree_Transplant.Interaction
{
  internal static class TransplantSelection
  {
    public static bool TrySelectTree(
      TransplantService service,
      GameLocation      location,
      Vector2           tile,
      Action            suppress,
      int               fee = 0
    )
    {
      var result = service.TrySelectTree(location, tile);

      if (result == SelectionResult.NoTreeFound)
        return false;

      suppress();

      if (result == SelectionResult.Success)
      {
        // Fee check: reject the pickup if the player cannot afford it.
        // Skipped entirely in free mode (fee == 0).
        if (fee > 0 && Game1.player.Money < fee)
        {
          service.ReturnTreeToOrigin();
          service.ClearSelection();

          Game1.dayTimeMoneyBox.moneyShakeTimer = 800;

          Feedback.Message(I18n.HUD_NoFunds());
          Game1.playSound(Feedback.Cancel);
          
          return false;
        }

        Game1.playSound(Feedback.Pickup);
        return true;
      }

      Feedback.Message(result switch
      {
        SelectionResult.TreeIsYoung             => I18n.HUD_TreeIsYoung(),
        SelectionResult.TreeIsTapped            => I18n.HUD_TreeIsTapped(),
        SelectionResult.TreeIsStruckByLightning => I18n.HUD_TreeIsStruckByLightning(),
        SelectionResult.TreeIsStump             => I18n.HUD_TreeIsStump(),
        _                                       => string.Empty
      });
      Game1.playSound(Feedback.Cancel);

      return false;
    }


    public static bool TryPlaceTree(
      TransplantService service,
      GameLocation      location,
      Vector2           tile,
      Action            suppress,
      int               fee = 0
    )
    {
      suppress();

      if (!service.IsValidPlacement(location, tile))
      {
        Feedback.Message(I18n.HUD_InvalidTile());
        Game1.playSound(Feedback.Cancel);
        return false;
      }

      if (fee > 0)
      {
        Game1.player.Money -= fee;
        Game1.playSound(Feedback.Deduct);
      }

      service.ExecuteTransplant(location, tile);
      Game1.playSound(Feedback.Drop);
      return true;
    }

    public static void CancelSelection(TransplantService service)
    {
      service.ReturnTreeToOrigin();
      service.ClearSelection();
      Game1.playSound(Feedback.Clear);
    }
  }
}
