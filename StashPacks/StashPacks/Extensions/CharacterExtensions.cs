namespace ModifAmorphic.Outward.StashPacks.Extensions
{
    public static class CharacterExtensions
    {
        public static bool IsHostCharacter(this Character character)
        {
            return character.OwnerPlayerSys.IsMasterClient && character.OwnerPlayerSys.PlayerID == 0;
        }
    }
}
