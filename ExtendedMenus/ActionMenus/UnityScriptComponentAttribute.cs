﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class UnityScriptComponentAttribute : Attribute
    {
        public string ComponentPath { get; set; }
    }
}
