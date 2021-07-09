using BepInEx;
using HarmonyLib;

namespace ModifAmorphic.Outward.ExtraSlots
{
    [BepInDependency("com.bepis.bepinex.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("io.mefino.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("modifamorphic.outward.shared", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModInfo.ModId, ModInfo.ModName, ModInfo.ModVersion)]
    public class ExtraSlotsPlugin : BaseUnityPlugin
    {

        internal void Awake()
        {
            var harmony = new Harmony(ModInfo.ModId);
            try
            {
                UnityEngine.Debug.Log($"[{ModInfo.ModName}] - Starting...");
                harmony.PatchAll();

                var extraSlots = new ExtraSlots();
                extraSlots.Start(this);
            }
            catch
            {
                UnityEngine.Debug.LogError($"Failed to enable {ModInfo.ModId} {ModInfo.ModName}.");
                harmony.UnpatchSelf();
                throw;
            }
        }
    }
}
