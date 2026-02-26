using System.Diagnostics;

using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

using Wild_and_Fruit_Tree_Transplant.Interaction;

namespace Wild_and_Fruit_Tree_Transplant
{
  public class ModEntry : Mod
  {
    private ModConfig config =  null!;

    public override void Entry(IModHelper helper)
    {
      I18n.Init(helper.Translation);
      config = helper.ReadConfig<ModConfig>();

      var harmony = new Harmony(this.ModManifest.UniqueID);

      var module = new TransplantModule(helper, config);
      module.ApplyPatches(harmony);
      module.RegisterEvents();

      helper.Events.GameLoop.GameLaunched += OnGameLaunched;
      helper.Events.Input.ButtonPressed += OnButtonPressed;
    }

    private void OnGameLaunched(object? _sender, GameLaunchedEventArgs _e)
      => config.RegisterOptions(this.Helper, this.ModManifest);

    
    private void OnButtonPressed(object? _sender, ButtonPressedEventArgs e)
    {
      if (!Context.IsPlayerFree)
        return;

      if (e.Button == SButton.F2)
        WarpToRobin();
    }

    [Conditional("DEBUG")]
    private static void WarpToRobin()
    {
      while (Game1.timeOfDay < 1000)
        Game1.performTenMinuteClockUpdate();

      NPC robin = Game1.getCharacterFromName("Robin");
      robin?.warpToPathControllerDestination();

      Game1.warpFarmer("ScienceHouse", 8, 20, 0);
    }
  }
}
