using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TerrainFeatures;

namespace Wild_and_Fruit_Tree_Transplant.Core
{
  internal sealed class TransplantRenderer
  {

    private static readonly Rectangle GreenSquare = new(194, 388, 16, 16);
    private static readonly Rectangle RedSquare   = new(210, 388, 16, 16);

    private static readonly Color GhostColorValid   = Color.White * 0.5f;
    private static readonly Color GhostColorInvalid = Color.Red * 0.5f;

    private readonly TransplantService service  = null!;

    public TransplantRenderer(TransplantService service)
    {
      this.service = service;
    }

    public void Draw(SpriteBatch b, GameLocation location, Vector2 hoverTile)
    {
      var selectedTree = service.SelectedTree;

      if (selectedTree is null)
        return;

      bool isValidPlacement = service.IsValidPlacement(location, hoverTile);
      Color ghostColor      = isValidPlacement ? GhostColorValid : GhostColorInvalid;

      DrawFootprintTiles(b, location, hoverTile);
      DrawGhostTree(b, selectedTree, hoverTile, ghostColor);

    }

    /* ---- Footprint Overlay ---- */
    private void DrawFootprintTiles(SpriteBatch b, GameLocation location, Vector2 hoverTile)
    {
      foreach (var (tile, isValid) in service.GetFootprintTiles(location, hoverTile))
      {
        var screenPos = (tile * 64f) - new Vector2(Game1.viewport.X, Game1.viewport.Y);
        var dest      = new Rectangle((int)screenPos.X, (int)screenPos.Y, 64, 64);
        var src       = isValid ? GreenSquare : RedSquare;

        b.Draw(Game1.mouseCursors, dest, src, Color.White);
      }
    }

    /* ---- Ghost Tree Dispatch ---- */
    private static void DrawGhostTree(SpriteBatch b, object tree, Vector2 hoverTile, Color ghostColor) 
    { 
      switch (tree)
      {
        case Tree      wildTree:  DrawGhostWildTree (b, wildTree,  hoverTile, ghostColor); break;
        case FruitTree fruitTree: DrawGhostFruitTree(b, fruitTree, hoverTile, ghostColor); break;
      }
    }

   
    /* ---- Wild Tree Ghost Rendering ---- */
    private static void DrawGhostWildTree(SpriteBatch b, Tree tree, Vector2 hoverTile, Color ghostColor)
    {
      var     texture   = tree.texture.Value;
      var     effects   = tree.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
      int     stage     = tree.growthStage.Value;
      bool    hasMoss   = tree.hasMoss.Value;
      Vector2 pixelPos  = hoverTile * Game1.tileSize;

      // Young tree (Stage 0-4)
      if (stage < Tree.treeStage)
      {
        DrawWildTreeSapling(b, texture, stage, pixelPos, ghostColor, effects);
        return;
      }

      // Adult Tree (Stage 5)
      DrawWildTreeStump(b, texture, pixelPos, hasMoss, ghostColor, effects);

      if (!tree.stump.Value)
        DrawWildTreeCanopy(b, texture, pixelPos, hasMoss, ghostColor, effects);
    }

    private static void DrawWildTreeSapling(
      SpriteBatch   b,
      Texture2D     texture, 
      int           stage, 
      Vector2       pixelPos, 
      Color         ghostColor, 
      SpriteEffects effects
    )
    {
      Rectangle sourceRect = stage switch
      {
        Tree.seedStage    => new(32, 128, 16, 16),
        Tree.sproutStage  => new(0, 128, 16, 16),
        Tree.saplingStage => new(16, 128, 16, 16),
        _                 => new(0, 96, 16, 32)
      };

      b.Draw(
        texture:          texture,
        position:         Game1.GlobalToLocal(
                            Game1.viewport,
                            new Vector2(
                              x: pixelPos.X + 32,
                              y: pixelPos.Y - (sourceRect.Height * 4 - 64) + (stage >= Tree.bushStage ? 128f : 64f)
                            )
                          ),
        sourceRectangle:  sourceRect,
        color:            ghostColor,
        rotation:         0f,
        origin:           new(8f, stage >= Tree.bushStage ? 32f : 16f),
        scale:            4f,
        effects:          effects,
        layerDepth:       1f
      );
    }

    private static void DrawWildTreeStump(
      SpriteBatch   b, 
      Texture2D     texture, 
      Vector2       pixelPos, 
      bool          hasMoss, 
      Color         ghostColor, 
      SpriteEffects effects
    )
    {
      Rectangle stumpRect = Tree.stumpSourceRect;
      
      if (hasMoss)
        stumpRect.X += 96;

      b.Draw(
          texture:          texture,
          position:         Game1.GlobalToLocal(Game1.viewport, new Vector2(pixelPos.X, pixelPos.Y - 64f)),
          sourceRectangle:  stumpRect,
          color:            ghostColor,
          rotation:         0f,
          origin:           Vector2.Zero,
          scale:            4f,
          effects:          effects,
          layerDepth:       1f
      );
    }

    private static void DrawWildTreeCanopy(
      SpriteBatch   b, 
      Texture2D     texture, 
      Vector2       pixelPos, 
      bool          hasMoss, 
      Color         ghostColor, 
      SpriteEffects effects
    )
    {
      Rectangle topRect = Tree.treeTopSourceRect;
      
      if (hasMoss)
        topRect.X += 96;

      b.Draw(
        texture:          texture,
        position:         Game1.GlobalToLocal(Game1.viewport, new Vector2(pixelPos.X + 32f, pixelPos.Y + 64f)),
        sourceRectangle:  topRect,
        color:            ghostColor,
        rotation:         0f,
        origin:           new Vector2(24f, 96f),
        scale:            4f,
        effects:          effects,
        layerDepth:       1.1f
      );
    }


    /* ---- Fruit Tree Ghost Rendering ---- */
    private static void DrawGhostFruitTree(SpriteBatch b, FruitTree tree, Vector2 hoverTile, Color ghostColor)
    {
      var     texture   = tree.texture;
      var     effects   = tree.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
      int     stage     = tree.growthStage.Value;
      int     spriteRow = tree.GetSpriteRowNumber();
      Vector2 pixelPos  = hoverTile * Game1.tileSize;

      // Sapling (Stage0-3) ---
      if (stage < FruitTree.treeStage)
      {
        DrawFruitTreeSapling(b, texture, stage, spriteRow, pixelPos, ghostColor, effects);
        return;
      }

      Color treeColor = tree.struckByLightningCountdown.Value > 0
                         ? new Color((int)(ghostColor.R * 0.5f), (int)(ghostColor.G * 0.5f), (int)(ghostColor.B * 0.5f), ghostColor.A)
                         : ghostColor;

      // Adult Fruit Tree
      DrawFruitTreeStump(b, texture, spriteRow, pixelPos, treeColor, effects);
      
      if (!tree.stump.Value)
        DrawFruitTreeCanopy(b, texture, tree, spriteRow, pixelPos, treeColor, effects);

      if (tree.fruit.Count > 0)
        DrawFruits(b, tree, hoverTile, treeColor);
    }

    private static void DrawFruitTreeSapling(
      SpriteBatch   b,
      Texture2D     texture,
      int           stage,
      int           spriteRow,
      Vector2       pixelPos,
      Color         ghostColor,
      SpriteEffects effects
    )
    {
      Rectangle sourceRect = stage switch
      {
        FruitTree.seedStage     => new(0, spriteRow * 80, 48, 80),
        FruitTree.sproutStage   => new(48, spriteRow * 80, 48, 80),
        FruitTree.saplingStage  => new(96, spriteRow * 80, 48, 80),
        _                       => new(144, spriteRow * 80, 48, 80)
      };

      b.Draw(
        texture:          texture,
        position:         Game1.GlobalToLocal(Game1.viewport, new Vector2(pixelPos.X + 32f, pixelPos.Y + 64f)),
        sourceRectangle:  sourceRect,
        color:            ghostColor,
        rotation:         0f,
        origin:           new Vector2(24f, 80f),
        scale:            4f,
        effects:          effects,
        layerDepth:       1f
      );
    }

    private static void DrawFruits(SpriteBatch b, FruitTree tree, Vector2 hoverTile, Color ghostColor)
    {
      float layerDepth = 1.2f;
      float tx = hoverTile.X;
      float ty = hoverTile.Y;

      for (int i = 0; i < tree.fruit.Count; i++)
      {
        ParsedItemData obj = ((int)tree.struckByLightningCountdown.Value > 0) 
          ? ItemRegistry.GetDataOrErrorItem("(O)382") 
          : ItemRegistry.GetDataOrErrorItem(tree.fruit[i].QualifiedItemId); 
        Texture2D fruitTexture = obj.GetTexture();
        Rectangle sourceRect = obj.GetSourceRect();

        Vector2 fruitPos = i switch
        {
          0 => new Vector2(tx * 64f - 64f + tx * 200f % 64f / 2f, ty * 64f - 192f - tx % 64f / 3f),
          1 => new Vector2(tx * 64f + 32f, ty * 64f - 256f + tx * 232f % 64f / 3f),
          _ => new Vector2(tx * 64f + tx * 200f % 64f / 3f, ty * 64f - 160f + tx * 200f % 64f / 3f)
        };

        SpriteEffects effects = (i == 2) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        b.Draw(
            texture: fruitTexture,
            position: Game1.GlobalToLocal(Game1.viewport, fruitPos),
            sourceRectangle: sourceRect,
            color: ghostColor,
            rotation: 0f,
            origin: Vector2.Zero,
            scale: 4f,
            effects: effects,
            layerDepth: layerDepth
        );
      }
    }

    private static void DrawFruitTreeCanopy(
      SpriteBatch   b,
      Texture2D     texture,
      FruitTree     tree,
      int           spriteRow,
      Vector2       pixelPos,
      Color         ghostColor,
      SpriteEffects effects
    )
    {
      Season seasonForLocation = Game1.GetSeasonForLocation(tree.Location);
      int which = seasonForLocation switch
      {
        Season.Fall   => 2,
        Season.Winter => 3,
        Season.Summer => 1,
        _             => 0,
      };

      int seasonOffset  = (tree.IgnoresSeasonsHere() ? 1 : which) * 3;

      Rectangle canopyRect = new((12 + seasonOffset) * 16, spriteRow * 80, 48, 64);

      b.Draw(
        texture:          texture,
        position:         Game1.GlobalToLocal(Game1.viewport, new Vector2(pixelPos.X + 32f, pixelPos.Y + 64f)),
        sourceRectangle:  canopyRect,
        color:            ghostColor,
        rotation:         0f,
        origin:           new Vector2(24f, 80f),
        scale:            4f,
        effects:          effects,
        layerDepth:       1.1f
      );
    }

    private static void DrawFruitTreeStump(
      SpriteBatch   b,
      Texture2D     texture,
      int           spriteRow,
      Vector2       pixelPos,
      Color         ghostColor,
      SpriteEffects effects
    )
    {
      Rectangle trunkRect = new(384, spriteRow * 80 + 48, 48, 32);

      b.Draw(
        texture:          texture,
        position:         Game1.GlobalToLocal(Game1.viewport, new Vector2(pixelPos.X + 32f, pixelPos.Y + 64f)),
        sourceRectangle:  trunkRect,
        color:            ghostColor,
        rotation:         0f,
        origin:           new Vector2(24f, 32f),
        scale:            4f,
        effects:          effects,
        layerDepth:       1f
      );
    }
  }
}
