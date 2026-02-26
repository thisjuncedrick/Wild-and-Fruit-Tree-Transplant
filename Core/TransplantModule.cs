using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Wild_and_Fruit_Tree_Transplant.Core;
using Wild_and_Fruit_Tree_Transplant.Patches;

namespace Wild_and_Fruit_Tree_Transplant.Interaction
{
  internal sealed class TransplantModule
  {
    private readonly IModHelper helper = null!;

    private readonly FreeTransplantMode freeMode;
    private readonly RobinTransplantMode robinMode;
    private readonly RobinMenuPatches robinPatch;

    public TransplantModule(IModHelper helper, ModConfig config)
    {
      this.helper = helper;
      var service = new TransplantService(config);
      var renderer = new TransplantRenderer(service);

      freeMode = new FreeTransplantMode(helper, config, service, renderer);
      robinMode = new RobinTransplantMode(config, service, renderer);
      robinPatch = new RobinMenuPatches(config, robinMode);
    }

    public void RegisterEvents()
    {
      freeMode.RegisterEvents();
      helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
    }

    public void ApplyPatches(Harmony harmony)
    {
      robinPatch.Apply(harmony);
    }

    private void OnSaveLoaded(object? _sender, SaveLoadedEventArgs _e)
    {
      LocationRegistry.Clear();

      LocationRegistry.Register(new Location(
        Key: "Farm",
        DisplayName: Game1.getFarm().DisplayName,
        Tile: Game1.getFarm().GetStarterFarmhouseLocation()
      ));

      LocationRegistry.Register(new Location(
        Key: "IslandWest",
        DisplayName: I18n.CarpenterMenu_TransplantTree_Option_IslandWest(),
        Tile: new(77, 41),
        Condition: () => GameStateQuery.CheckConditions("PLAYER_VISITED_LOCATION Any IslandWest")
      ));
    }
  }
}
