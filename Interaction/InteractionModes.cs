using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

using Wild_and_Fruit_Tree_Transplant.Core;

namespace Wild_and_Fruit_Tree_Transplant.Interaction
{
  internal sealed class FreeTransplantMode
  {
    private readonly IModHelper helper  = null!;
    private readonly ModConfig config   = null!;
    
    private readonly TransplantService service   = null!;
    private readonly TransplantRenderer renderer = null!;

    public FreeTransplantMode(
      IModHelper          helper, 
      ModConfig           config, 
      TransplantService   service, 
      TransplantRenderer  renderer
    )
    {
      this.helper   = helper;
      this.config   = config;
      this.service  = service;
      this.renderer = renderer;
    }

    public void RegisterEvents()
    {
      helper.Events.Input.ButtonPressed   += OnButtonPressed;
      helper.Events.Display.RenderedWorld += OnRenderedWorld;
    }

    private void OnButtonPressed(object? _sender, ButtonPressedEventArgs e)
    {
      // Do nothing if Robin's service is active
      // or the player is not free.
      if (config.UseRobinsService || !Context.IsPlayerFree)
        return;

      // No tree seleced
      if (service.SelectedTree is null)
      {
        // Allow selection only when transplant mode is held
        // and the tool button is pressed.
        if (config.TransplantModeKey.IsDown() && e.Button.IsUseToolButton())
          TransplantSelection.TrySelectTree(
              service:  service,
              location: Game1.currentLocation,
              tile:     e.Cursor.Tile,
              suppress: () => helper.Input.Suppress(e.Button)
          );

        // Nothing else to do when no tree is selected.
        return;
      }

      // A tree is currently selected.

      // Escape cancels the operation and clears the selection.
      if (e.Button == SButton.Escape)
      {
        helper.Input.Suppress(e.Button);
        TransplantSelection.CancelSelection(service);
        return;
      }

      // Tool button attempts to place the selected tree.
      if (e.Button.IsUseToolButton())
        TransplantSelection.TryPlaceTree(
            service: service,
            location: Game1.currentLocation,
            tile: e.Cursor.Tile,
            suppress: () => helper.Input.Suppress(e.Button)
        );
    }
    private void OnRenderedWorld(object? _sender, RenderedWorldEventArgs e)
    {
      // Render preview only when transplant mode is active and usable.
      // Skip if Robin's service is active, no tree is selected,
      // or the player is not in a free.
      if (config.UseRobinsService || service.SelectedTree is null || !Context.IsPlayerFree)
        return;

      renderer.Draw(e.SpriteBatch, Game1.currentLocation, Game1.currentCursorTile);
    }

  }

  internal sealed class RobinTransplantMode
  {
    private readonly ModConfig          config;
    private readonly TransplantService  service;
    private readonly TransplantRenderer renderer;

    public RobinTransplantMode(ModConfig config, TransplantService service, TransplantRenderer renderer)
    {
      this.config = config;
      this.service = service;
      this.renderer = renderer;
    }

    public void Open(string location = "Farm")
    {
      // Do nothing if Robin's service is disabled
      if (!config.UseRobinsService)
        return;

      Game1.activeClickableMenu = new TransplantMenu(config, service, renderer, location);
    }
  }
}
