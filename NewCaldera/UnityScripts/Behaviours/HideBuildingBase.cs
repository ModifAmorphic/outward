using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts.Behaviours
{
    public class HideBuildingBase : MonoBehaviour
    {
        //public Transform FinishedModel;
        public Transform BuildingBase;
        
        private void Awake()
        {
            PatchBuildingVisual();
            AfterActivatePhaseVisual = CheckHideBase;
        }

        private void Start()
        {
            if (TryGetComponent(OutwardAssembly.Types.BuildingVisual, out var buildingVisual))
            {
                var phases = buildingVisual.GetFieldValue<Transform[]>(OutwardAssembly.Types.BuildingVisual, "Phases");
                var previousActivePhaseIndex = buildingVisual.GetFieldValue<int>(OutwardAssembly.Types.BuildingVisual, "m_previousActivePhaseIndex");
                if (phases.Length == previousActivePhaseIndex + 1)
                    HideBase();
            }
        }

        public void CheckHideBase(MonoBehaviour buildingVisual, int phaseIndex)
        {
            if (buildingVisual.TryGetComponent<HideBuildingBase>(out var hideBase))
            {
                var phases = buildingVisual.GetFieldValue<Transform[]>(OutwardAssembly.Types.BuildingVisual, "Phases");
                if (phaseIndex + 1 == phases.Length)
                {
                    hideBase.HideBase();
                }
            }
        }

        public void HideBase()
        {
            BuildingBase.gameObject.SetActive(false);
        }

        #region Patches

        private void PatchBuildingVisual()
        {
            ModifScriptsManager.Instance.Logger.LogInfo("Patching BuildingVisual");

            //Patch Awake
            var activatePhaseVisual = OutwardAssembly.Types.BuildingVisual.GetMethod("ActivatePhaseVisual", BindingFlags.Instance | BindingFlags.Public, null, new Type[] {typeof(int)}, null);
            var activatePhaseVisualPostFix = typeof(HideBuildingBase).GetMethod(nameof(ActivatePhaseVisualPostfix), BindingFlags.NonPublic | BindingFlags.Static);
            HarmonyPatcher.Instance.Harmony.Patch(activatePhaseVisual, postfix: new HarmonyMethod(activatePhaseVisualPostFix));
        }

        public delegate void AfterActivatePhaseVisualDelegate(MonoBehaviour buildingVisual, int phaseIndex);
        public static AfterActivatePhaseVisualDelegate AfterActivatePhaseVisual;

        private static void ActivatePhaseVisualPostfix(MonoBehaviour __instance, int _phaseIndex)
        {
            try
            {
#if DEBUG
                ModifScriptsManager.Instance.Logger.LogTrace($"{nameof(HideBuildingBase)}::{nameof(ActivatePhaseVisualPostfix)}(): Invoking {nameof(AfterActivatePhaseVisual)}.");
#endif
                AfterActivatePhaseVisual?.Invoke(__instance, _phaseIndex);
            }
            catch (Exception ex)
            {
                ModifScriptsManager.Instance.Logger.LogException($"{nameof(HideBuildingBase)}::{nameof(ActivatePhaseVisualPostfix)}(): Exception invoking {nameof(AfterActivatePhaseVisual)}.", ex);
            }
        }

        #endregion
    }
}
