using StardewModdingAPI;
using StardewModdingAPI.Utilities;

using Wild_and_Fruit_Tree_Transplant.Integrations.GenericModConfigMenu;

namespace Wild_and_Fruit_Tree_Transplant
{
  internal sealed class ModConfig
  {
    public bool UseRobinsService          { get; set; } = true;
    public int WildTreeTransplantPrice    { get; set; } = 50;
    public int FruitTreeTransplantPrice   { get; set; } = 150;
    public KeybindList TransplantModeKey  { get; set; } = KeybindList.Parse("B");
    public bool ForceTransplant            { get; set; } = false;
    public bool AllowTapped               { get; set; } = false;
    public bool AllowStumps               { get; set; } = false;
    public bool AllowLightningStruck      { get; set; } = false;
    public bool OnlyMatureTrees           { get; set; } = true;


    public void RegisterOptions(IModHelper helper, IManifest manifest)
    {
      var gmcm = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

      if (gmcm is null)
        return;

      gmcm.Register(
        mod:      manifest,
        reset:    () => ResetConfig(helper),
        save:     () => helper.WriteConfig(this)
      );

      gmcm.AddBoolOption(
        mod:      manifest,
        name:     () => I18n.Config_UseRobinsService_Name(),
        tooltip:  () => I18n.Config_UseRobinsService_Tooltip(),
        getValue: () => UseRobinsService,
        setValue: (value) => UseRobinsService = value
      );

      gmcm.AddNumberOption(
        mod:      manifest,
        name:     () => I18n.Config_WildTreeTransplantPrice_Name(),
        tooltip:  () => I18n.Config_WildTreeTransplantPrice_Tooltip(),
        getValue: () => WildTreeTransplantPrice,
        setValue: (value) => WildTreeTransplantPrice = value,
        min:      0,
        max:      3000,
        interval: 50
      );

      gmcm.AddNumberOption(
        mod:      manifest,
        name:     () => I18n.Config_FruitTreeTransplantPrice_Name(),
        tooltip:  () => I18n.Config_FruitTreeTransplantPrice_Tooltip(),
        getValue: () => FruitTreeTransplantPrice,
        setValue: (value) => FruitTreeTransplantPrice = value,
        min:      0,
        max:      3000,
        interval: 50
      );

      gmcm.AddKeybindList(
        mod:      manifest,
        name:     () => I18n.Config_TransplantModeKeybind_Name(),
        tooltip:  () => I18n.Config_TransplantModeKeybind_Tooltip(),
        getValue: () => TransplantModeKey,
        setValue: (value) => TransplantModeKey = value
      );

      gmcm.AddBoolOption(
        mod:      manifest,
        name:     () => I18n.Config_ForceTransplant_Name(),
        tooltip:  () => I18n.Config_ForceTransplant_Tooltip(),
        getValue: () => ForceTransplant,
        setValue: (value) => ForceTransplant = value
      );

      gmcm.AddBoolOption(
        mod:      manifest,
        name:     () => I18n.Config_AllowTapped_Name(),
        tooltip:  () => I18n.Config_AllowTapped_Tooltip(),
        getValue: () => AllowTapped,
        setValue: (value) => AllowTapped = value
      );

      gmcm.AddBoolOption(
        mod:      manifest,
        name:     () => I18n.Config_AllowStumps_Name(),
        tooltip:  () => I18n.Config_AllowStumps_Tooltip(),
        getValue: () => AllowStumps,
        setValue: (value) => AllowStumps = value
      );

      gmcm.AddBoolOption(
        mod:      manifest,
        name:     () => I18n.Config_AllowLightningStruck_Name(),
        tooltip:  () => I18n.Config_AllowLightningStruck_Tooltip(),
        getValue: () => AllowLightningStruck,
        setValue: (value) => AllowLightningStruck = value
      );

      gmcm.AddBoolOption(
        mod:      manifest,
        name:     () => I18n.Config_OnlyMatureTrees_Name(),
        tooltip:  () => I18n.Config_OnlyMatureTrees_Tooltip(),
        getValue: () => OnlyMatureTrees,
        setValue: (value) => OnlyMatureTrees = value
      );
    }

    private void ResetConfig(IModHelper helper)
    {
      UseRobinsService          = true;
      WildTreeTransplantPrice   = 50;
      FruitTreeTransplantPrice  = 150;
      TransplantModeKey         = KeybindList.Parse("B");
      ForceTransplant            = false;
      AllowTapped               = false;
      AllowStumps               = false;
      AllowLightningStruck      = false;
      OnlyMatureTrees           = true;

      helper.WriteConfig(this);
    }
  }
}
