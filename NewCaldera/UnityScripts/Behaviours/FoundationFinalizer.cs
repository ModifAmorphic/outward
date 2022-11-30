using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts.Behaviours
{
    public enum FoundationAdjustments
    {
        None,
        Hide,
        Resize
    }
    public class FoundationFinalizer : MonoBehaviour
    {
        public FoundationAdjustments FinalAdjustmentType;
        public Transform BuildingBase;
        public Transform DeploymentCollider;
        public Vector3 FinalBuildingBaseScale;
        public Vector3 FinalDeploymentColliderScale;

        private Vector3 _originalScale;
        private Vector3 _originalColliderScale;

        private void Awake()
        {
            PatchBuildingVisual();
            AfterActivatePhaseVisual = CheckApply;
        }

        private void Start()
        {
            if (BuildingBase != null)
                _originalScale = BuildingBase.localScale;
            if (DeploymentCollider != null)
                _originalColliderScale = DeploymentCollider.localScale;
        }

        public void CheckApply(MonoBehaviour buildingVisual, int phaseIndex)
        {
            if (buildingVisual.TryGetComponent<FoundationFinalizer>(out var hideBase))
            {
                var phases = buildingVisual.GetFieldValue<Transform[]>(OutwardAssembly.Types.BuildingVisual, "Phases");
                if (phases.Length > 0 && phaseIndex != -1 && phaseIndex + 1 == phases.Length)
                {
                    hideBase.Apply();
                }
                else
                {
                    hideBase.Reset();
                }
            }
        }

        public void Apply()
        {
            if (FinalAdjustmentType == FoundationAdjustments.Hide)
            {
                BuildingBase?.gameObject?.SetActive(false);
                DeploymentCollider?.gameObject?.SetActive(false);
            }
            else if (FinalAdjustmentType == FoundationAdjustments.Resize)
            {
                if (BuildingBase != null)
                    BuildingBase.localScale = new Vector3(FinalBuildingBaseScale.x, FinalBuildingBaseScale.y, FinalBuildingBaseScale.z);
                if (DeploymentCollider != null)
                    DeploymentCollider.localScale = new Vector3(FinalDeploymentColliderScale.x, FinalDeploymentColliderScale.y, FinalDeploymentColliderScale.z);
            }
        }

        public void Reset()
        {
            if (FinalAdjustmentType == FoundationAdjustments.Hide)
            {
                BuildingBase?.gameObject?.SetActive(true);
                DeploymentCollider?.gameObject?.SetActive(true);
            }
            else if (FinalAdjustmentType == FoundationAdjustments.Resize)
            {
                if (BuildingBase != null)
                    BuildingBase.localScale = new Vector3(_originalScale.x, _originalScale.y, _originalScale.z);
                if (DeploymentCollider != null)
                    DeploymentCollider.localScale = new Vector3(_originalColliderScale.x, _originalColliderScale.y, _originalColliderScale.z);
            }
        }

        #region Patches

        private void PatchBuildingVisual()
        {
            ModifScriptsManager.Instance.Logger.LogInfo("Patching BuildingVisual");

            //Patch Awake
            var activatePhaseVisual = OutwardAssembly.Types.BuildingVisual.GetMethod("ActivatePhaseVisual", BindingFlags.Instance | BindingFlags.Public, null, new Type[] {typeof(int)}, null);
            var activatePhaseVisualPostFix = typeof(FoundationFinalizer).GetMethod(nameof(ActivatePhaseVisualPostfix), BindingFlags.NonPublic | BindingFlags.Static);
            HarmonyPatcher.Instance.Harmony.Patch(activatePhaseVisual, postfix: new HarmonyMethod(activatePhaseVisualPostFix));

            //Patch Awake
            var activateBuildingProcessVisuals = OutwardAssembly.Types.BuildingVisual.GetMethod("ActivateBuildingProcessVisuals", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(int) }, null);
            var activateBuildingProcessVisualsPostFix = typeof(FoundationFinalizer).GetMethod(nameof(ActivateBuildingProcessVisualsPostFix), BindingFlags.NonPublic | BindingFlags.Static);
            HarmonyPatcher.Instance.Harmony.Patch(activatePhaseVisual, postfix: new HarmonyMethod(activatePhaseVisualPostFix));
        }

        public delegate void AfterActivatePhaseVisualDelegate(MonoBehaviour buildingVisual, int phaseIndex);
        public static AfterActivatePhaseVisualDelegate AfterActivatePhaseVisual;

        private static void ActivatePhaseVisualPostfix(MonoBehaviour __instance, int _phaseIndex)
        {
            try
            {
                AfterActivatePhaseVisual?.Invoke(__instance, _phaseIndex);
            }
            catch (Exception ex)
            {
                ModifScriptsManager.Instance.Logger.LogException($"{nameof(FoundationFinalizer)}::{nameof(ActivatePhaseVisualPostfix)}(): Exception invoking {nameof(AfterActivatePhaseVisual)}.", ex);
            }
        }

        private static void ActivateBuildingProcessVisualsPostFix(MonoBehaviour __instance, int _phaseIndex)
        {
            try
            {
                AfterActivatePhaseVisual?.Invoke(__instance, _phaseIndex);
            }
            catch (Exception ex)
            {
                ModifScriptsManager.Instance.Logger.LogException($"{nameof(FoundationFinalizer)}::{nameof(ActivateBuildingProcessVisualsPostFix)}(): Exception invoking {nameof(AfterActivatePhaseVisual)}.", ex);
            }
        }

        #endregion
    }
}
