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
            var thisAssembly = Assembly.GetExecutingAssembly();
            var assemblyDir = Path.GetDirectoryName(thisAssembly.Location);

            var fileNames = Directory.GetFiles(assemblyDir, "ModifAmorphic.Outward.v*_*_*.dll").ToList();
            if (File.Exists(Path.Combine(assemblyDir, "ModifAmorphic.Outward.dll")))
                fileNames.Add(Path.Combine(assemblyDir, "ModifAmorphic.Outward.dll"));

            foreach (var fileName in fileNames)
            {
                if (thisAssembly.Location != fileName)
                {
                    logger.LogDebug($"{nameof(ManifestFiles)}::{nameof(RemoveOtherVersions)}: Deleting Assembly file '{fileName}'");
                    File.Delete(fileName);
                }
            }
        }
    }
}
