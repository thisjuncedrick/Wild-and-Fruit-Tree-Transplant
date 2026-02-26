using HarmonyLib;
using StardewModdingAPI;

using StardewValley;
using Wild_and_Fruit_Tree_Transplant.Core;
using Wild_and_Fruit_Tree_Transplant.Interaction;

namespace Wild_and_Fruit_Tree_Transplant.Patches
{
  internal class RobinMenuPatches
  {
    private static ModConfig config = null!;
    private static RobinTransplantMode mode = null!;

    private const string TransplantOptKey = "Transplant";

    public RobinMenuPatches(ModConfig config, RobinTransplantMode mode)
    {
      RobinMenuPatches.config = config;
      RobinMenuPatches.mode = mode;
    }

    internal void Apply(Harmony harmony)
    {
      harmony.Patch(
        original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.createQuestionDialogue),
          new[] { typeof(string), typeof(Response[]), typeof(string) }),
        prefix: new HarmonyMethod(typeof(RobinMenuPatches), nameof(InjectTransplantOption))
      );

      harmony.Patch(
        original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
        postfix: new HarmonyMethod(typeof(RobinMenuPatches), nameof(HandleTransplantAnswer))
      );
    }

    private static void InjectTransplantOption(ref Response[] answerChoices, string dialogKey)
    {
      // Only act on Robin's carpenter menu, and only if the service is enabled in config
      if (dialogKey != "carpenter" || !config.UseRobinsService)
        return;

      // Setup custom "Transplant Tree" option
      var option = new Response(TransplantOptKey, I18n.CarpenterMenu_TransplantTree_Option());

      // Append "Transplant Tree" option just before "Leave" option (Last option)
      var list = answerChoices.ToList();
      list.Insert(list.Count - 1, option);
      answerChoices = list.ToArray();
    }

    private static bool HandleTransplantAnswer(bool result, string questionAndAnswer)
    {
      // Use default action if the "Transplant" option is not selected
      if (questionAndAnswer != "carpenter_" + TransplantOptKey)
        return result;

      var answerChoices = LocationRegistry.ToResponse();

      //// Default to opening "Farm" when TransplantLocations.ToResponse only has 2 (Farm and Leave option)
      if (answerChoices.Length > 2)
        AskLocationDialog(answerChoices);
      else
        mode.Open();
        

      return true;
    }

    private static void AskLocationDialog(Response[] answerChoices)
    {
      // Create another question dialog, passing the RegisteredLocations as options
      Game1.currentLocation.createQuestionDialogue(
        question: Game1.content.LoadString("Strings\\UI:Carpenter_ChooseLocation"),
        answerChoices: answerChoices,
        afterDialogueBehavior: (_, answer) =>
        {
          if (answer != "Leave")
            mode.Open(answer);
        }
      );
    }
  }
}