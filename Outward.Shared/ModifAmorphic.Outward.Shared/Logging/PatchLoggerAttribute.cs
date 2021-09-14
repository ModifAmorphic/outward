using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Logging
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal class PatchLoggerAttribute : Attribute
    {
    }
}
