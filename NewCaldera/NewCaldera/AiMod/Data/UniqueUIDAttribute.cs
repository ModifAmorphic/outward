using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Data
{
    internal enum UniquePropertyTypes
    {
        UID
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal class UniquePropertyAttribute : Attribute
    {
        public UniquePropertyTypes UniquePropertyType { get; set; }
    }
}
