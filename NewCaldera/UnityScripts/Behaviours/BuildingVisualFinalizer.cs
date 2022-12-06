using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace ModifAmorphic.Outward.UnityScripts.Behaviours
{
    public enum FinalAdjustments
    {
        None,
        RemoveBase,
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
            if (FinalAdjustmentType == FinalAdjustments.RemoveBase)
            {
                BuildingBase?.gameObject?.SetActive(false);
                //DeploymentCollider?.gameObject?.SetActive(false);
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

            var colliderLayers = SetCollidersIgnoreRaycasts(gameObject);
            var hits = Physics.RaycastAll(castPosition, Vector3.down, 10.0F, envMask);

            bool hitGround = false;
            RaycastHit hit = default;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider != null && hits[i].collider.name.Equals("mdl_env_calderaBaseBuildingTerrain_c (NonExplodedTerrain)", StringComparison.InvariantCultureIgnoreCase))
                {
                    hit = hits[i];
                    hitGround = true;
                    break;
                }
            }

            if (hitGround)
            {
                //ModifScriptsManager.Instance.Logger.LogDebug($"Direction==Down. Raycast hit collider {hit.collider?.name}. Moving {FinishedBuilding.name} from position " +
                //    $"({FinishedBuilding.position.x}, {FinishedBuilding.position.y}, {FinishedBuilding.position.z}) to ({hit.point.x}, {hit.point.y}, {hit.point.z})");
                FinishedBuilding.localPosition = Vector3.zero;
                FinishedBuilding.position = hit.point;
            }

            ResetColliderLayers(colliderLayers);
        }

        
        private List<(Collider Collider, int Layer)> SetCollidersIgnoreRaycasts(GameObject parent)
        {
            var colliderLayers = new List<(Collider, int)>();
            if (parent.TryGetComponent<Collider>(out var parentCollider))
            {
                colliderLayers.Add((parentCollider, parentCollider.gameObject.layer));
                parentCollider.gameObject.layer = OutwardAssembly.IgnoreRaycastsLayer;
            }
            var colliders = parent.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                colliderLayers.Add((colliders[i], colliders[i].gameObject.layer));
                colliders[i].gameObject.layer = OutwardAssembly.IgnoreRaycastsLayer;
            }
            return colliderLayers;
        }

        private void ResetColliderLayers(List<(Collider Collider, int Layer)> colliderLayers)
        {
            for (int i = 0; i < colliderLayers.Count; i++)
                colliderLayers[i].Collider.gameObject.layer = colliderLayers[i].Layer;
        }

        public void Reset()
        {
            if (FinalAdjustmentType == FinalAdjustments.RemoveBase)
            {
                BuildingBase?.gameObject?.SetActive(true);
                //DeploymentCollider?.gameObject?.SetActive(true);
            }
            else if (FinalAdjustmentType == FinalAdjustments.Resize)
            {
                if (BuildingBase != null && _originalScale != Vector3.zero)
                    BuildingBase.localScale = new Vector3(_originalScale.x, _originalScale.y, _originalScale.z);
                if (DeploymentCollider != null && _originalColliderScale != Vector3.zero)
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
