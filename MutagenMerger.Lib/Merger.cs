using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;

namespace MutagenMerger.Lib
{
    [PublicAPI]
    public sealed class Merger : IDisposable
    {
        private readonly LoadOrder<IModListing<ISkyrimModGetter>> _loadOrder;
        private readonly SkyrimMod _outputMod;
        private readonly string _outputPath;
        private readonly IEnumerable<ModKey> _plugins;
        private readonly List<ModKey> _modsToMerge;

        public Merger(string dataFolderPath, List<ModKey> plugins, List<ModKey> modsToMerge, ModKey outputKey)
        {
            _loadOrder = LoadOrder.Import(
                dataFolderPath,
                plugins, 
                path => ModInstantiator<ISkyrimModGetter>.Importer(path, GameRelease.SkyrimSE));

            _outputMod = new SkyrimMod(outputKey, SkyrimRelease.SkyrimSE);
            _outputPath = Path.Combine(dataFolderPath, outputKey.FileName);
            _plugins = plugins;
            _modsToMerge = modsToMerge;
        }
        
        public void Merge()
        {
            var mods = _loadOrder.PriorityOrder.Resolve();
            var mergingMods = mods.Where(x => _modsToMerge.Contains(x.ModKey));
            mergingMods.MergeMods<ISkyrimModGetter, ISkyrimMod, ISkyrimMajorRecord, ISkyrimMajorRecordGetter>(
                _outputMod,
                out var mapping);
        }

        public void Dispose()
        {
            _loadOrder.Dispose();
            _outputMod.WriteToBinary(_outputPath, Utils.SafeBinaryWriteParameters);
        }
    }
}
