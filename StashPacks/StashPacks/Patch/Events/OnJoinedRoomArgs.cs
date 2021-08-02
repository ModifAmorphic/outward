
namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal class OnJoinedRoomArgs
    {
        public NetworkLevelLoader NetworkLevelLoader { get;  }
        public bool FailedJoin { get; }
        internal OnJoinedRoomArgs(NetworkLevelLoader networkLevelLoader, bool failedJoin) => (NetworkLevelLoader, FailedJoin) = (networkLevelLoader, failedJoin);
    }
}
