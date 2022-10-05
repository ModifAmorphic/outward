using ModifAmorphic.Outward.Unity.ActionUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public abstract class DynamicColorImage : MonoBehaviour
    {
        public Image Image;

        protected List<ColorRange> initColorRanges = new List<ColorRange>();

        protected Dictionary<int, Color> colorMap = new Dictionary<int, Color>();

        protected bool _isAwake = false;

        protected float _value;
        public float Value => _value;

        public event Action<Color> OnColorChanged;
        public event Action<float> OnValueChanged;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            ResetColorRanges();
            _isAwake = true;
            if (initColorRanges.Any())
            {
                DebugLogger.Log("Init Colors ranges found. Adding ColorMaps for image.");
                foreach (var colorRange in initColorRanges)
                    AddColorMaps(colorRange);

                initColorRanges.Clear();
            }
            OnAwake();
        }

        protected abstract void OnAwake();

        public void AddColorRange(ColorRange colorRange)
        {
            if (colorRange.Min < 0f)
                throw new ArgumentOutOfRangeException(nameof(ColorRange.Min));
            if (colorRange.Max > 1f)
                throw new ArgumentOutOfRangeException(nameof(ColorRange.Max));

            if (!_isAwake)
                initColorRanges.Add(colorRange);
            else
                AddColorMaps(colorRange);

        }

        public void ResetColorRanges()
        {
            colorMap.Clear();
            for (int i = 0; i <= 1000; i++)
            {
                colorMap.Add(i, Color.gray);
            }
        }

        public virtual void SetValue(float value)
        {
            if (value < 0 || value > 1f)
                return;
            //DebugLogger.Log($"SetValue({value}).  Current _value == {_value}");
            var rounded = (int)(value * 1000);

            if (colorMap.TryGetValue(rounded, out var color) && Image.color != color)
            {
                Image.color = color;
                OnColorChanged?.Invoke(color);
            }
            if (_value != value)
            {
                _value = value;
                //DebugLogger.Log($"Raising OnValueChanged({_value})");
                OnValueChanged?.Invoke(_value);
            }
        }

        private void AddColorMaps(ColorRange colorRange)
        {
            int min = (int)(1000 * colorRange.Min);
            int max = (int)(1000 * colorRange.Max);

            for (int i = min; i <= max; i++)
            {
                if (!colorMap.ContainsKey(i))
                    colorMap.Add(i, colorRange.Color);
                else
                    colorMap[i] = colorRange.Color;
            }
        }
    }
}