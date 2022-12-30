using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Common;

namespace GroundStorageCrafting {
  public class GroundStorageCraftingMod : ModSystem {
    public static readonly string HarmonyId = "japanhasrice.groundstoragecrafting";
    public override void Start(ICoreAPI api) {
      base.Start(api);

      RegisterModClasses(api);
      ApplyHarmonyPatches();
    }

    public void RegisterModClasses(ICoreAPI api) {
      api.RegisterCollectibleBehaviorClass("ContainedInteractable", typeof(CollectibleBehaviorContainedInteractable));
    }

    public void ApplyHarmonyPatches() {
      new Harmony(HarmonyId).PatchAll(Assembly.GetExecutingAssembly());
    }

    public override void Dispose() {
      base.Dispose();
      RemoveHarmonyPatches();
    }

    public void RemoveHarmonyPatches() {
      new Harmony(HarmonyId).UnpatchAll(HarmonyId);
    }
  }
}
