using System.Collections.Concurrent;

namespace ModifAmorphic.Outward.Models
{
    internal class CustomDropTables
    {
        public ConcurrentBag<GuaranteedDropTable> GuaranteedDrops { get; internal set; } = new ConcurrentBag<GuaranteedDropTable>();
        public ConcurrentBag<ConditionalGuaranteedDrop> ConditionalGuaranteedDrops { get; internal set; } = new ConcurrentBag<ConditionalGuaranteedDrop>();
        public ConcurrentBag<ConditionalDropTable> ConditionalDropTables { get; internal set; } = new ConcurrentBag<ConditionalDropTable>();

    }
}
