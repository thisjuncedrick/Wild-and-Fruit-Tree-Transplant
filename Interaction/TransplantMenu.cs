using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using Wild_and_Fruit_Tree_Transplant.Core;
using Wild_and_Fruit_Tree_Transplant.Utils;

namespace Wild_and_Fruit_Tree_Transplant.Interaction
{
  internal class TransplantMenu : IClickableMenu
  {
    private readonly ModConfig          config;
    private readonly TransplantService  service;
    private readonly TransplantRenderer renderer;

    private readonly GameLocation?  savedLocation;
    private readonly string         targetLocation;

    private readonly  bool     hasFee;
    private           Vector2  cursorTile;
    private           string   bannerText = string.Empty;
    private           int      bannerScrollX;

    private readonly ClickableTextureComponent cancelButton;

    public TransplantMenu(ModConfig config, TransplantService service, TransplantRenderer rendrer, string targetLocation)
    {
      this.config = config;
      this.service = service;
      this.renderer = rendrer;
      this.targetLocation = targetLocation;

      savedLocation = Game1.currentLocation;
      hasFee = config.FruitTreeTransplantPrice > 0 || config.WildTreeTransplantPrice > 0;

      cancelButton = new(
        bounds: Rectangle.Empty,
        texture: Game1.mouseCursors,
        sourceRect: Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47, -1, -1),
        scale: 1.0f
      );

      ResetBounds();
      Game1.globalFadeToBlack(Open);
      Game1.player.forceCanMove();
    }

    /* ---- Open / Close ---- */
    private void Open()
    {
      Game1.currentLocation.cleanupBeforePlayerExit();
      Game1.currentLocation = Game1.getLocationFromName(targetLocation);
      Game1.player.viewingLocation.Value = Game1.currentLocation.NameOrUniqueName;
      Game1.currentLocation.resetForPlayerEntry();

      Game1.displayHUD      = false;
      Game1.viewportFreeze  = true;
      Game1.displayFarmer   = false;

      Game1.viewport.Location = LocationRegistry.GetViewport(targetLocation);

      Game1.panScreen(0, 0);
      Game1.globalFadeToClear();
    }

    private void Close()
    {
      if (!readyToClose())
        return;

      // Defensive measure - SelectedTree at this point should already be cleared
      if (service.SelectedTree is not null)
      {
        service.ReturnTreeToOrigin();
        service.ClearSelection();
      }

      Game1.currentLocation.cleanupBeforePlayerExit();
      Game1.currentLocation = savedLocation;
      Game1.player.viewingLocation.Value = null;
      Game1.currentLocation!.resetForPlayerEntry();

      Game1.displayHUD      = true;
      Game1.viewportFreeze  = false;
      Game1.displayFarmer   = true;

      Game1.globalFadeToClear();
      exitThisMenu();
    }

    /* ---- Input Handler ---- */
    private void OnTreeDeselect()
    {
      if (service.SelectedTree is not null)
      {
        // Tree is held — put it back where it came from, stay in the menu.
        TransplantSelection.CancelSelection(service);
        RefreshBanner();
      }
    }

    private void OnCancel()
    {
      OnTreeDeselect();

      if (readyToClose())
        Game1.globalFadeToBlack(Close);
    }


    // Adapted from CarpenterMenu.cs — pans the viewport when the cursor
    // reaches the screen edges and forwards held keys to receiveKeyPress.
    // https://github.com/Dannode36/StardewValleyDecompiled/blob/main/Stardew%20Valley/StardewValley.Menus/CarpenterMenu.cs#L620
    public override void update(GameTime time)
    {
      base.update(time);

      if (Game1.IsFading())
        return;

      int mouseX = Game1.getOldMouseX(ui_scale: false) + Game1.viewport.X;
      int mouseY = Game1.getOldMouseY(ui_scale: false) + Game1.viewport.Y;
      cursorTile = new Vector2(mouseX / Game1.tileSize, mouseY / Game1.tileSize);

      // Edge-scroll when the cursor is near the screen border.
      if      (mouseX - Game1.viewport.X < 64)                             Game1.panScreen(-8, 0);
      else if (mouseX - (Game1.viewport.X + Game1.viewport.Width) >= -128) Game1.panScreen( 8, 0);

      if      (mouseY - Game1.viewport.Y < 64)                             Game1.panScreen(0, -8);
      else if (mouseY - (Game1.viewport.Y + Game1.viewport.Height) >= -64) Game1.panScreen(0,  8);

      // Forward held keys (excluding E/Escape key).
      Keys[] pressedKeys = Game1.oldKBState.GetPressedKeys();
      foreach (Keys key in pressedKeys)
        if (!Game1.options.doesInputListContain(Game1.options.menuButton, key))
          receiveKeyPress(key);
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
      base.receiveLeftClick(x, y, playSound);

      if (Game1.IsFading())
        return;

      if (cancelButton.containsPoint(x, y))
      {
        OnCancel();
        return;
      }

      // No tree held — try to pick one up.
      if (service.SelectedTree is null)
      {
        bool selected = TransplantSelection.TrySelectTree(
            service:  service,
            location: Game1.currentLocation,
            tile:     cursorTile,
            suppress: static () => { },
            fee: GetTransplantFee()
          );

        if (selected)
          RefreshBanner();
        return;
      }

      // Tree is held — attempt to place.
      bool placed = TransplantSelection.TryPlaceTree(
          service:  service,
          location: Game1.currentLocation,
          tile:     cursorTile,
          suppress: static () => { },
          fee:      GetTransplantFee()
      );
      if (placed)
        RefreshBanner();
    }

    // Adapted from CarpenterMenu.cs - pans the viewport when the move keys are pressed
    // https://github.com/Dannode36/StardewValleyDecompiled/blob/main/Stardew%20Valley/StardewValley.Menus/CarpenterMenu.cs#L581
    public override void receiveKeyPress(Keys key)
    {
      if (Game1.IsFading())
        return;

      if (Game1.options.doesInputListContain(Game1.options.menuButton, key))
      {
        if (service.SelectedTree is not null)
          OnTreeDeselect();
        else
          OnCancel();
        
        return;
      }

      if (Game1.options.SnappyMenus)
        return;

      if      (Game1.options.doesInputListContain(Game1.options.moveDownButton,  key)) Game1.panScreen( 0,  4);
      else if (Game1.options.doesInputListContain(Game1.options.moveUpButton,    key)) Game1.panScreen( 0, -4);
      else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key)) Game1.panScreen( 4,  0);
      else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton,  key)) Game1.panScreen(-4,  0);
    }

    /* ---- Rendering ---- */
    public override void draw(SpriteBatch b)
    {
      base.draw(b);

      if (Game1.globalFade)
        return;

      Game1.StartWorldDrawInUI(b);
      renderer.Draw(b, Game1.currentLocation, cursorTile);
      Game1.EndWorldDrawInUI(b);

      cancelButton.draw(b);

      SpriteText.drawStringWithScrollBackground(b, bannerText, bannerScrollX, 16);
      
      if (hasFee)
        Game1.dayTimeMoneyBox.draw(b);

      drawMouse(b);
    }

    /* ---- Layout / Resize ---- */
    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
      => ResetBounds();

    public override void performHoverAction(int x, int y)
    {
      cancelButton.tryHover(x, y);
      base.performHoverAction(x, y);
    }

    public override bool readyToClose() =>
      base.readyToClose() && service.SelectedTree is null;

    /* ---- Helpers ---- */
    private void ResetBounds()
    {
      cancelButton.bounds = new Rectangle(
        x:      xPositionOnScreen + Game1.uiViewport.Width  - borderWidth - spaceToClearSideBorder - 64,
        y:      yPositionOnScreen + Game1.uiViewport.Height - 128,
        width:  64,
        height: 64
      );

      RefreshBanner();
    }

    private void RefreshBanner()
    {
      bannerText = service.SelectedTree is null
          ? I18n.CarpenterMenu_Banner_SelectTree()
          : I18n.CarpenterMenu_Banner_PickLocation();

      bannerScrollX = (Game1.uiViewport.Width / 2) - (SpriteText.getWidthOfString(bannerText) / 2);
    }

    private int GetTransplantFee()
    {
      return service.SelectedTree is FruitTree
          ? config.FruitTreeTransplantPrice
          : config.WildTreeTransplantPrice;
    }
  }
}
