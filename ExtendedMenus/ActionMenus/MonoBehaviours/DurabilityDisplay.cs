using ModifAmorphic.Outward.Unity.ActionMenus.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    
    [UnityScriptComponent]
    public class DurabilityDisplay : MonoBehaviour
    {
        public PlayerActionMenus PlayerActionMenus;

        public DurabilitySlot Head;
        public DurabilitySlot Chest;
        public DurabilitySlot RightHand;
        public DurabilitySlot LeftHand;
        public DurabilitySlot Feet;

        private Canvas _canvas;
        private PositionableUI _positionable;

        private Dictionary<DurableEquipmentSlot, DurabilitySlot> _durabilitySlots = new Dictionary<DurableEquipmentSlot, DurabilitySlot>();
        public Dictionary<DurableEquipmentSlot, DurabilitySlot> DurabilitySlots => _durabilitySlots;

        private bool isAwake = false;
        public bool IsAwake => isAwake;

        public UnityEvent OnAwake { get; } = new UnityEvent();

        private Dictionary<DurableEquipmentSlot, float> _displayMinimums = new Dictionary<DurableEquipmentSlot, float>();

        private Dictionary<DurableEquipmentSlot, Coroutine> _coroutines = new Dictionary<DurableEquipmentSlot, Coroutine>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        void Awake()
        {
            //StartCoroutine(AddToPsp());

            _canvas = GetComponent<Canvas>();
            _positionable = GetComponent<PositionableUI>();

            if (_positionable != null)
                _positionable.OnIsPositionableChanged.AddListener((isPositionable) => RefreshDisplay());

            _durabilitySlots.Add(DurableEquipmentSlot.Head, Head);
            _durabilitySlots.Add(DurableEquipmentSlot.Chest, Chest);
            _durabilitySlots.Add(DurableEquipmentSlot.RightHand, RightHand);
            _durabilitySlots.Add(DurableEquipmentSlot.LeftHand, LeftHand);
            _durabilitySlots.Add(DurableEquipmentSlot.Feet, Feet);

            foreach (var slot in _durabilitySlots.Values)
            {
                var changedSlot = slot;
                slot.OnValueChanged += (v) => OnSlotValueChanged(changedSlot);
            }

            isAwake = true;
            OnAwake.TryInvoke();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        void Start()
        {
            var psp = Psp.Instance.GetServicesProvider(PlayerActionMenus.PlayerID);
            if (!psp.TryGetService<DurabilityDisplay>(out _))
                psp.AddSingleton(this);

            RefreshDisplay();
        }
        public void TrackDurability(IDurability durability)
        {
            StopTracking(durability.DurableEquipmentSlot);

            SetMinimumDisplayValue(durability.DurableEquipmentSlot, durability.MinimumDisplayValue);
            
            foreach (var range in durability.ColorRanges)
                _durabilitySlots[durability.DurableEquipmentSlot].AddColorRange(range);

            _durabilitySlots[durability.DurableEquipmentSlot].SetEquipmentType(durability.DurableEquipmentType);

            _coroutines.Add(
                durability.DurableEquipmentSlot,
                StartCoroutine(
                    SetDurabilityValue(_durabilitySlots[durability.DurableEquipmentSlot], durability.GetDurabilityRatio
            )));

            RefreshDisplay();
        }

        public void StopTracking(DurableEquipmentSlot slot)
        {
            if (_coroutines.ContainsKey(slot))
            {
                StopCoroutine(_coroutines[slot]);
                _coroutines.Remove(slot);
                _durabilitySlots[slot].ResetColorRanges();
                _durabilitySlots[slot].SetValue(0f);
                SetMinimumDisplayValue(slot, -1f);
                //RefreshDisplay();
            }
        }

        public void StopAllTracking()
        {
            foreach (var slot in Enum.GetValues(typeof(DurableEquipmentSlot)).Cast<DurableEquipmentSlot>())
                if (_coroutines.ContainsKey(slot))
                    StopCoroutine(_coroutines[slot]);
        }

        private void SetMinimumDisplayValue(DurableEquipmentSlot slot, float minimum)
        {
            if (_displayMinimums.ContainsKey(slot))
                _displayMinimums[slot] = minimum;
            else
                _displayMinimums.Add(slot, minimum);

            RefreshDisplay();
        }

        private void OnSlotValueChanged(DurabilitySlot equipSlot)
        {
            //Debug.Log($"OnSlotValueChanged({equipSlot.EquipmentSlot})");
            if (_displayMinimums.TryGetValue(equipSlot.EquipmentSlot, out var minimum))
            {
                //Debug.Log($"OnSlotValueChanged({equipSlot.EquipmentSlot}) Got minimum value of {minimum}. equipSlot.Value == {equipSlot.Value}");
                if ((equipSlot.Value <= minimum && !_canvas.enabled) || (equipSlot.Value > minimum && _canvas.enabled))
                    RefreshDisplay();
            }
        }

        private void RefreshDisplay()
        {
            bool canvasEnabled = _positionable?.IsPositionable??false;
            //Debug.Log($"RefreshDisplay()");
            foreach (var slotKvp in _durabilitySlots)
            {
                if (_displayMinimums.ContainsKey(slotKvp.Key))
                    if (slotKvp.Value.Value <= _displayMinimums[slotKvp.Key])
                    {
                        slotKvp.Value.Image.enabled = true;
                        canvasEnabled = true;
                    }
            }

            if (canvasEnabled)
            {
                LeftHand.Image.enabled = _coroutines.ContainsKey(DurableEquipmentSlot.LeftHand);
                RightHand.Image.enabled = _coroutines.ContainsKey(DurableEquipmentSlot.RightHand);
                Head.Image.enabled = true;
                Chest.Image.enabled = true;
                Feet.Image.enabled = true;
            }
            else
            {
                Head.Image.enabled = false;
                Chest.Image.enabled = false;
                Feet.Image.enabled = false;
            }
            _canvas.enabled = canvasEnabled;
        }

        private IEnumerator SetDurabilityValue(DurabilitySlot slot, Func<float> getDurability)
        {
            while (true)
            {
                slot.SetValue(getDurability());
                yield return new WaitForSeconds(Timings.DurabilityWait);
            }
        }

        private IEnumerator AddToPsp()
        {
            yield return new WaitUntil(() => Psp.Instance != null && PlayerActionMenus.PlayerID > -1);
            
            var psp = Psp.Instance.GetServicesProvider(PlayerActionMenus.PlayerID);
            if (!psp.TryGetService<DurabilityDisplay>(out _))
                psp.AddSingleton(this);
        }

    }
}