namespace ModifAmorphic.Outward.StashPacks.Extensions
{
    public static class PlayerSystemExtensions
    {
        public static bool IsHostPlayer(this PlayerSystem playerSystem)
        {
            return playerSystem.IsMasterClient && playerSystem.PlayerID == 0;
        }
    }
}
