using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ModifAmorphic.Outward.Internal
{
    public static class ManifestFiles
    {
        public static void RemoveOtherVersions(IModifLogger logger)
        {
            //This Shared Outward Assembly. May be different from mod directory if multiple
            //mods with the same version of this assembly are installed.
            var thisAssembly = Assembly.GetExecutingAssembly();
            var thisFilename = Path.GetFileName(thisAssembly.Location);

            //The mod assembly calling this method
            var callingAssembly = Assembly.GetCallingAssembly();
            var callingDir = Path.GetDirectoryName(callingAssembly.Location);

            //Get all of the potential Outward Shared assemblies in the calling mods folder.
            var filePaths = Directory.GetFiles(callingDir, "ModifAmorphic.Outward.v*_*_*.dll").ToList();

            //Check for original shared assembly name prior to versioning.
            if (File.Exists(Path.Combine(callingDir, "ModifAmorphic.Outward.dll")))
                filePaths.Add(Path.Combine(callingDir, "ModifAmorphic.Outward.dll"));
            logger.LogDebug($"{nameof(ManifestFiles)}::{nameof(RemoveOtherVersions)}: Mod is using Outward.Shared assembly loaded from '{thisAssembly.Location}'." +
                $" Calling mod directory is '{callingDir}'.");
            foreach (var fileName in filePaths)
            {
                if (thisFilename != Path.GetFileName(fileName))
                {
                    logger.LogDebug($"{nameof(ManifestFiles)}::{nameof(RemoveOtherVersions)}: Deleting Assembly file '{fileName}'");
                    File.Delete(fileName);
                }
            }
        }
    }
}
