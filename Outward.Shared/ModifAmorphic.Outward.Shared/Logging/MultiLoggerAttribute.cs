using System;

namespace ModifAmorphic.Outward.Logging
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal class MultiLoggerAttribute : Attribute
    {
    }
}
