using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace ModifAmorphic.Outward.UnityScripts.Behaviours
{
    public enum FinalAdjustments
    {
        None,
        Hide,
        Resize,
        Destroy
    }

    public class BuildingVisualFinalizer : MonoBehaviour
    {
        public FinalAdjustments FinalAdjustmentType;
        public Transform BuildingTransform;
        public Transform FinishedBuilding;
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
            if (buildingVisual.TryGetComponent<BuildingVisualFinalizer>(out var visualFinalizer))
            {
                var phases = buildingVisual.GetFieldValue<Transform[]>(OutwardAssembly.Types.BuildingVisual, "Phases");
                ModifScriptsManager.Instance.Logger.LogDebug($"Processing {FinishedBuilding.name} {nameof(BuildingVisualFinalizer)}. PhaseIndex=={phaseIndex}, Phases.Length=={phases?.Length}.");
                if (phases.Length > 0 && phaseIndex != -1 && phaseIndex + 1 == phases.Length)
                {
                    visualFinalizer.Apply();
                }
                else
                {
                    visualFinalizer.Reset();
                }
            }
        }

        public void Apply()
        {
            if (FinalAdjustmentType == FinalAdjustments.Hide)
            {
                BuildingBase?.gameObject?.SetActive(false);
                DeploymentCollider?.gameObject?.SetActive(false);
                GroundTransform();
            }
            else if (FinalAdjustmentType == FinalAdjustments.Resize)
            {
                if (BuildingBase != null)
                    BuildingBase.localScale = new Vector3(FinalBuildingBaseScale.x, FinalBuildingBaseScale.y, FinalBuildingBaseScale.z);
                if (DeploymentCollider != null)
                    DeploymentCollider.localScale = new Vector3(FinalDeploymentColliderScale.x, FinalDeploymentColliderScale.y, FinalDeploymentColliderScale.z);
            }
            else if (FinalAdjustmentType == FinalAdjustments.Destroy)
            {

            }
        }

        public void GroundTransform()
        {
            if (FinishedBuilding == null)
                return;

            var envMask = ReflectionExtensions.GetStaticFieldValue<int>(OutwardAssembly.Types.Global, "LargeEnvironmentMask");
            var castPosition = FinishedBuilding.position + new Vector3(0, 5f, 0);
            //var fromPos = new Vector3(69.2342f, 150f, -58.9996f);
            //var toPos = new Vector3(69.2342f, -150f, -58.9996f);
            //Physics.Raycast(fromPos, Vector3.up, out RaycastHit hit, 200f, 67111425);
            //Debug.DrawRay(fromPos, toPos, Color.green, 120f);
            //Log($"Raycast hit collider {hit.collider?.name} at ({hit.point.x}, {hit.point.y}, {hit.point.z}). Started from " +
            //    $"({fromPos.x}, {fromPos.y}, {fromPos.z})");
            FinishedBuilding.GetChild(0).gameObject.layer = Physics.IgnoreRaycastLayer;
            RaycastHit hit;
            if (Physics.Raycast(castPosition, Vector3.down, out hit, 10f, envMask))
            {
                ModifScriptsManager.Instance.Logger.LogDebug($"Direction==Down. Raycast hit collider {hit.collider?.name}. Moving {FinishedBuilding.name} from position " +
                    $"({FinishedBuilding.position.x}, {FinishedBuilding.position.y}, {FinishedBuilding.position.z}) to ({hit.point.x}, {hit.point.y}, {hit.point.z})");
                //FinishedBuilding.position = hit.point;
            }

            var castPositionUp = FinishedBuilding.position - new Vector3(0, 20f, 0);
            RaycastHit upHit;
            if (Physics.Raycast(castPosition, Vector3.up, out upHit, 50f, envMask))
            {
                ModifScriptsManager.Instance.Logger.LogDebug($"Direction==Up. Raycast hit collider {hit.collider?.name}. Moving {FinishedBuilding.name} from position " +
                    $"({FinishedBuilding.position.x}, {FinishedBuilding.position.y}, {FinishedBuilding.position.z}) to ({hit.point.x}, {hit.point.y}, {hit.point.z})");
                //FinishedBuilding.position = hit.point;
            }
        }

        public void Reset()
        {
            if (FinalAdjustmentType == FinalAdjustments.Hide)
            {
                BuildingBase?.gameObject?.SetActive(true);
                DeploymentCollider?.gameObject?.SetActive(true);
            }
            else if (FinalAdjustmentType == FinalAdjustments.Resize)
            {
                if (BuildingBase != null)
                    BuildingBase.localScale = new Vector3(_originalScale.x, _originalScale.y, _originalScale.z);
                if (DeploymentCollider != null)
                    DeploymentCollider.localScale = new Vector3(_originalColliderScale.x, _originalColliderScale.y, _originalColliderScale.z);
            }
        }

        #region Patches

        private static bool _isPatched;

        private static void PatchBuildingVisual()
        {
            if (_isPatched)
                return;

            ModifScriptsManager.Instance.Logger.LogInfo("Patching BuildingVisual");

            //Patch Awake
            var activatePhaseVisual = OutwardAssembly.Types.BuildingVisual.GetMethod("ActivatePhaseVisual", BindingFlags.Instance | BindingFlags.Public, null, new Type[] {typeof(int)}, null);
            var activatePhaseVisualPostFix = typeof(BuildingVisualFinalizer).GetMethod(nameof(ActivatePhaseVisualPostfix), BindingFlags.NonPublic | BindingFlags.Static);
            HarmonyPatcher.Instance.Harmony.Patch(activatePhaseVisual, postfix: new HarmonyMethod(activatePhaseVisualPostFix));

            //Patch Awake
            var activateBuildingProcessVisuals = OutwardAssembly.Types.BuildingVisual.GetMethod("ActivateBuildingProcessVisuals", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(int) }, null);
            var activateBuildingProcessVisualsPostFix = typeof(BuildingVisualFinalizer).GetMethod(nameof(ActivateBuildingProcessVisualsPostFix), BindingFlags.NonPublic | BindingFlags.Static);
            HarmonyPatcher.Instance.Harmony.Patch(activatePhaseVisual, postfix: new HarmonyMethod(activatePhaseVisualPostFix));
            _isPatched = true;
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
                ModifScriptsManager.Instance.Logger.LogException($"{nameof(BuildingVisualFinalizer)}::{nameof(ActivatePhaseVisualPostfix)}(): Exception invoking {nameof(AfterActivatePhaseVisual)}.", ex);
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
                ModifScriptsManager.Instance.Logger.LogException($"{nameof(BuildingVisualFinalizer)}::{nameof(ActivateBuildingProcessVisualsPostFix)}(): Exception invoking {nameof(AfterActivatePhaseVisual)}.", ex);
            }
        }

        #endregion
    }
}
