using SiraUtil.Zenject;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zenject;

namespace HttpStatusExtention.PPCounters
{
    public class AccSaberData : IAsyncInitializable
    {
        public bool DataInit { get; private set; } = false;
        [Inject]
        private readonly PPDownloader _ppDownloader;
        private readonly Dictionary<SongID, float> _rankedMaps = new Dictionary<SongID, float>();

        public async Task InitializeAsync(CancellationToken token)
        {
            try {
                if (await this._ppDownloader.WaitForInitializationAsync(token)) {
                    this.CreateRankedMapsDict(this._ppDownloader.AccSaberData);
                }
            }
            finally {
                this.DataInit = true;
            }
        }

        public float GetComplexity(SongID songID)
        {
            return !this.DataInit ? 0 : !this._rankedMaps.ContainsKey(songID) ? 0 : this._rankedMaps[songID];
        }

        public bool IsRanked(SongID songID)
        {
            return this._rankedMaps.ContainsKey(songID);
        }

        private void CreateRankedMapsDict(List<AccSaberRankedMap> rankedMaps)
        {
            if (rankedMaps == null) {
                Plugin.Log.Warn("AccSaber ranked maps are not available.");
                return;
            }
            foreach (var rankedMap in rankedMaps) {
                if (string.IsNullOrEmpty(rankedMap.songHash) || string.IsNullOrEmpty(rankedMap.difficulty)) {
                    continue;
                }
                try {
                    var id = rankedMap.songHash.ToUpper();
                    var beatmapDifficulty = SongDataUtils.GetDifficulty(rankedMap.difficulty);
                    var songID = new SongID(id, beatmapDifficulty);
                    this._rankedMaps[songID] = rankedMap.complexity;
                }
                catch (Exception) {
                    // 不明な難易度などは無視する。
                }
            }
        }
    }
}
