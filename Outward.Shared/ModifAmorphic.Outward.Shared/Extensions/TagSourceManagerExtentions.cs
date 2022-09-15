namespace ModifAmorphic.Outward.Extensions
{
    public static class TagSourceManagerExtentions
    {
        public static void RegisterTag(this TagSourceManager tagSourceManager, Tag tag, bool forceRefresh = false)
        {
            tagSourceManager.DbTags.Add(tag);
            if (forceRefresh)
                tagSourceManager.RefreshTags(true);
        }
        public static bool TryAddTag(this TagSourceManager tagSourceManager, Tag tag, bool forceRefresh = false)
        {
            var existing = tagSourceManager.DbTags.IndexOf(tag);
            if (existing > -1)
            {
                return false;
            }

            tagSourceManager.DbTags.Add(tag);
            if (forceRefresh)
                tagSourceManager.RefreshTags(true);

            return true;
        }
        public static void AddOrUpdateTag(this TagSourceManager tagSourceManager, Tag tag)
        {

            var existing = tagSourceManager.DbTags.IndexOf(tag);
            if (existing > -1)
            {
                tagSourceManager.DbTags[existing] = tag;
                return;
            }

            tagSourceManager.DbTags.Add(tag);
            tagSourceManager.RefreshTags(true);
        }
    }
}
