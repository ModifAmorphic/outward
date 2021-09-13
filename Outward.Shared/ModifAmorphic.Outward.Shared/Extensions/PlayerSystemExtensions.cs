namespace ModifAmorphic.Outward.Extensions
{
    public static class PlayerSystemExtensions
    {
        public static bool IsHostPlayer(this PlayerSystem playerSystem)
        {
            return playerSystem.IsMasterClient && playerSystem.PlayerID == 0;
        }
    }
}
