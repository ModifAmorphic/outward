using System;

namespace ModifAmorphic.Outward.Unity.ActionUI
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class UnityScriptComponentAttribute : Attribute
    {
        public string ComponentPath { get; set; }
    }
}
