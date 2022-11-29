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
            //if (TryGetComponent(OutwardAssembly.Types.BuildingVisual, out var buildingVisual))
            //{
            //    var phases = buildingVisual.GetFieldValue<Transform[]>(OutwardAssembly.Types.BuildingVisual, "Phases");
            //    var previousActivePhaseIndex = buildingVisual.GetFieldValue<int>(OutwardAssembly.Types.BuildingVisual, "m_previousActivePhaseIndex");

            //    if (previousActivePhaseIndex != -1 && phases.Length > 0 && phases.Length == previousActivePhaseIndex + 1)
            //    {
            //        ModifScriptsManager.Instance.Logger.LogTrace($"{nameof(HideBuildingBase)}::{nameof(Start)}(): {buildingVisual.name} previousActivePhaseIndex {previousActivePhaseIndex}, phases.Length {phases.Length}.");
            //        HideBase();
            //    }
            //    else
            //        ShowBase();
            //}
        }

        public void CheckHideBase(MonoBehaviour buildingVisual, int phaseIndex)
        {
            if (buildingVisual.TryGetComponent<HideBuildingBase>(out var hideBase))
            {
                var phases = buildingVisual.GetFieldValue<Transform[]>(OutwardAssembly.Types.BuildingVisual, "Phases");
                if (phases.Length > 0 && phaseIndex != -1 && phaseIndex + 1 == phases.Length)
                {
                    //ModifScriptsManager.Instance.Logger.LogTrace($"{nameof(HideBuildingBase)}::{nameof(CheckHideBase)}(): {buildingVisual.name} phaseIndex {phaseIndex}, phases.Length {phases.Length}.");
                    hideBase.HideBase();
                }
                else
                {
                    hideBase.ShowBase();
                }
            }
        }

        public void ShowBase()
        {
            BuildingBase.gameObject.SetActive(true);
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

            //Patch Awake
            var activateBuildingProcessVisuals = OutwardAssembly.Types.BuildingVisual.GetMethod("ActivateBuildingProcessVisuals", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(int) }, null);
            var activateBuildingProcessVisualsPostFix = typeof(HideBuildingBase).GetMethod(nameof(ActivateBuildingProcessVisualsPostFix), BindingFlags.NonPublic | BindingFlags.Static);
            HarmonyPatcher.Instance.Harmony.Patch(activatePhaseVisual, postfix: new HarmonyMethod(activatePhaseVisualPostFix));
        }

        public delegate void AfterActivatePhaseVisualDelegate(MonoBehaviour buildingVisual, int phaseIndex);
        public static AfterActivatePhaseVisualDelegate AfterActivatePhaseVisual;

        private static void ActivatePhaseVisualPostfix(MonoBehaviour __instance, int _phaseIndex)
        {
            try
            {
#if DEBUG
                //ModifScriptsManager.Instance.Logger.LogTrace($"{nameof(HideBuildingBase)}::{nameof(ActivatePhaseVisualPostfix)}(): Invoking {nameof(AfterActivatePhaseVisual)} for visual {__instance.name} phaseIndex {_phaseIndex}.");
#endif
                AfterActivatePhaseVisual?.Invoke(__instance, _phaseIndex);
            }
            catch (Exception ex)
            {
                ModifScriptsManager.Instance.Logger.LogException($"{nameof(HideBuildingBase)}::{nameof(ActivatePhaseVisualPostfix)}(): Exception invoking {nameof(AfterActivatePhaseVisual)}.", ex);
            }
        }

        private static void ActivateBuildingProcessVisualsPostFix(MonoBehaviour __instance, int _phaseIndex)
        {
            try
            {
#if DEBUG
                //ModifScriptsManager.Instance.Logger.LogTrace($"{nameof(HideBuildingBase)}::{nameof(ActivateBuildingProcessVisualsPostFix)}(): Invoking {nameof(AfterActivatePhaseVisual)} for visual {__instance.name} phaseIndex {_phaseIndex}.");
#endif
                AfterActivatePhaseVisual?.Invoke(__instance, _phaseIndex);
            }
            catch (Exception ex)
            {
                ModifScriptsManager.Instance.Logger.LogException($"{nameof(HideBuildingBase)}::{nameof(ActivateBuildingProcessVisualsPostFix)}(): Exception invoking {nameof(AfterActivatePhaseVisual)}.", ex);
            }
        }

        #endregion
    }
}
