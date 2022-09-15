using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class ProgressBar : DynamicColorImage
    {
        public Image Frame;

        public override void SetValue(float value)
        {
            base.SetValue(value);

            if (base.Image.color != Frame.color)
            {
                Frame.color = base.Image.color;
            }
            Image.fillAmount = value;
        }

        protected override void OnAwake() { }
    }
}
