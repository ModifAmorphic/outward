using ModifAmorphic.Outward.Transmorphic.Transmog.Models;
using ModifAmorphic.Outward.Transmorphic.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Extensions
{
    public static class UIDExtensions
    {
        public static bool TryDecode(this UID uid, out Guid guid)
        {
            var rawValue = uid.Value;
            guid = default;
            if (rawValue.Length < 22)
                return false;
            
            try
            {
                guid = UID.Decode(rawValue.Substring(0, 22));
                return true;
            }
            catch { }

            return false;
        }
        public static bool IsTransmogrified(this UID uid)
        {
            if (!uid.TryDecode(out var guid))
                return false;

            return guid.ToByteArray().Take(4)
                       .SequenceEqual(TransmogSettings.BytePrefixUID);
        }
        public static int ToVisualItemID(this UID uid)
        {
            if (!TryGetVisualItemID(uid, out var visualItemID))
                throw new ArgumentException("UID is not an encoded Transmog! UID must be encoded with a transmog prefix.", nameof(uid));

            return visualItemID;
        }

        /// <summary>
        /// If the <paramref name="uid"/> contains the transmog prefix, returns the decoded ItemID from it..
        /// </summary>
        /// <param name="uid">The <see cref="UID"/> potentially encoded with the transmog prefix and visual ItemID.</param>
        /// <param name="visualItemID">The Visual ItemID, of found.</param>
        /// <returns></returns>
        public static bool TryGetVisualItemID(this UID uid, out int visualItemID)
        {
            if (!uid.TryDecode(out var guid))
            {
                visualItemID = default;
                return false;
            }
            var uidBytes = guid.ToByteArray();

            if (!uidBytes.Take(4)
                         .SequenceEqual(TransmogSettings.BytePrefixUID))
            {
                visualItemID = default;
                return false;
            }

            var itemBytes = uidBytes.Skip(8);
            visualItemID = BitConverter.ToInt32(itemBytes, 4);

            return true;
        }
        
        static Random random = new Random();

        /// <summary>
        /// Creates a UID with the <see cref="ItemVisualMap.VisualItemID" /> and a marker for a transmorgrify encoded into it.
        /// </summary>
        /// <param name="visualMap">The visual map containing the VisualItemID</param>
        /// <returns>A new <see cref="UID"/> with the <see cref="ItemVisualMap.VisualItemID" /> encoded in to it.</returns>
        public static UID ToUID(this ItemVisualMap visualMap)
        {
            var randomBytes = new byte[8];
            random.NextBytes(randomBytes);

            var visualBytes = BitConverter.GetBytes(visualMap.VisualItemID);
            //var itemBytes = BitConverter.GetBytes(visualMap.ItemID);


            var guidBytes = new byte[16];
            Array.Copy(TransmogSettings.BytePrefixUID, 0, guidBytes, 0, 4);
            Array.Copy(randomBytes, 0, guidBytes, 4, 8);
            //Array.Copy(itemBytes, 0, guidBytes, 8, 4);
            Array.Copy(visualBytes, 0, guidBytes, 12, 4);

            return new UID(new Guid(guidBytes));
        }
    }
}
