using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public class ProgressBar : MonoBehaviour
    {
        public Image Fill;
        public Image Frame;

        private List<ColorRange> initColorRanges = new List<ColorRange>();

        private Dictionary<int, Color> colorMap = new Dictionary<int, Color>();

        private float _value;
        public float Value => _value;

        private static readonly Color gray = new Color(.5f, .5f, .5f, 1f);

        private bool _isAwake = false;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            ResetColorRanges();
            _isAwake = true;
            if (initColorRanges.Any())
            {
                Debug.Log("Init Colors ranges found. Adding ColorMaps for progress bar.");
                foreach(var colorRange in initColorRanges)
                    AddColorMaps(colorRange);

                initColorRanges.Clear();
            }
        }

        public void SetValue(float value)
        {
            if (value < 0 || value > 1f)
                throw new ArgumentOutOfRangeException("value");

            _value = value;
            var rounded = (int)(_value * 1000);

            if (colorMap.TryGetValue(rounded, out var color) && Fill.color != color)
            {
                Fill.color = color;
                Frame.color = color;
            }

            Fill.fillAmount = value;
        }

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
                colorMap.Add(i, gray);
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
