using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace GroundStorageCrafting.Patch {
  [HarmonyPatch(typeof(BlockEntityGroundStorage))]
  public static class BlockEntityGroundStoragePatch {
    const bool runOriginalMethod = true;
    const bool skipOriginalMethod = false;

    [HarmonyPrefix()]
    [HarmonyPatch("OnPlayerInteractStart")]
    public static bool BeforeOnPlayerInteractStart(BlockEntityGroundStorage __instance, IPlayer player, BlockSelection bs, ref ItemSlot ___isUsingSlot, ref bool __result) {
      const bool interactionSuccessful = true;
      const bool interactionFailed = false;

      var isSneaking = player.Entity.Controls.Sneak;

      if (!isSneaking) {
        return runOriginalMethod;
      }

      var layout = __instance?.StorageProps?.Layout;
      if (layout == null || layout == EnumGroundStorageLayout.Stacking) {
        return runOriginalMethod;
      }

      var inventoryIndex = __instance.GetInventoryIndex(bs);
      var storedCollectible = __instance.Inventory[inventoryIndex].Itemstack?.Collectible;
      if (storedCollectible == null) {
        return runOriginalMethod;
      }


      var interactable = storedCollectible?.GetBehavior<CollectibleBehaviorContainedInteractable>();
      if (interactable == null) {
        __result = interactionFailed;
        return skipOriginalMethod;
      }

      ___isUsingSlot = __instance.Inventory[inventoryIndex];
      __result = interactable.OnContainedInteractStart(__instance, ___isUsingSlot, player, bs);
      return skipOriginalMethod;
    }

    [HarmonyPrefix()]
    [HarmonyPatch("OnPlayerInteractStep")]
    public static bool BeforeOnPlayerInteractStep(BlockEntityGroundStorage __instance, float secondsUsed, IPlayer byPlayer, BlockSelection blockSel, ref ItemSlot ___isUsingSlot, ref bool __result) {
      bool allowOriginalMethod = true;
      var interactable = ___isUsingSlot?.Itemstack?.Collectible.GetBehavior<CollectibleBehaviorContainedInteractable>();
      if (interactable?.IsInteracting ?? false) {
        __result = interactable.OnContainedInteractStep(secondsUsed, __instance, ___isUsingSlot, byPlayer, blockSel);
        allowOriginalMethod = false;
      }
      return allowOriginalMethod;
    }

    [HarmonyPrefix()]
    [HarmonyPatch("OnPlayerInteractStop")]
    public static bool BeforeOnPlayerInteractStop(BlockEntityGroundStorage __instance, float secondsUsed, IPlayer byPlayer, BlockSelection blockSel, ref ItemSlot ___isUsingSlot) {
      bool allowOriginalMethod = true;
      var interactable = ___isUsingSlot?.Itemstack?.Collectible.GetBehavior<CollectibleBehaviorContainedInteractable>();
      if (interactable?.IsInteracting ?? false) {
        interactable.OnContainedInteractStop(secondsUsed, __instance, ___isUsingSlot, byPlayer, blockSel);
        allowOriginalMethod = false;
      }
      return allowOriginalMethod;
    }
  }

  public static class BlockEntityGroundStorageExtension {
    public static int GetInventoryIndex(this BlockEntityGroundStorage blockEntityGroundStorage, BlockSelection blockSelection) {
      var layout = blockEntityGroundStorage.StorageProps?.Layout;
      if (layout == null || layout == EnumGroundStorageLayout.SingleCenter || layout == EnumGroundStorageLayout.Stacking) {
        return 0;
      }
      MethodInfo rotatedOffset = typeof(BlockEntityGroundStorage).GetMethod("rotatedOffset", BindingFlags.Instance | BindingFlags.NonPublic);
      Vec3f hitPos = rotatedOffset.Invoke(blockEntityGroundStorage, new object[] { blockSelection.HitPosition.ToVec3f(), blockEntityGroundStorage.MeshAngle }) as Vec3f;
      switch (blockEntityGroundStorage.StorageProps.Layout) {
        case EnumGroundStorageLayout.Halves:
        case EnumGroundStorageLayout.WallHalves:
          return hitPos.X <= 0.5 ? 0 : 1;
        case EnumGroundStorageLayout.Quadrants:
          return (hitPos.X > 0.5 ? 2 : 0) + (hitPos.Z > 0.5 ? 1 : 0);
        default:
          return 0;
      }
    }
  }
}
