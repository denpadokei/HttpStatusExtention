using SiraUtil.Zenject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Zenject;

namespace HttpStatusExtention.PPCounters
{
    public class AccSaberData : IAsyncInitializable
    {
        private readonly bool _dataInitStart = false;
        public bool DataInit { get; private set; } = false;
        [Inject]
        private readonly PPDownloader _ppDownloader;
        private readonly Dictionary<SongID, float> _rankedMaps = new Dictionary<SongID, float>();

        private static readonly string ACCSABER_FILE_NAME = Path.Combine(Environment.CurrentDirectory, "UserData", "HttpStatusExtention", "accsaber.json");

        public async Task InitializeAsync(CancellationToken token)
        {
            while (this._ppDownloader?.Init != true) {
                await Task.Delay(1);
            }
            this.CreateRankedMapsDict(this._ppDownloader.AccSaberData);
            this.DataInit = true;
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
            foreach (var rankedMap in rankedMaps) {
                var id = rankedMap.songHash.ToUpper();
                var beatmapDifficulty = SongDataUtils.GetDifficulty(rankedMap.difficulty);
                var songID = new SongID(id, beatmapDifficulty);
                this._rankedMaps[songID] = rankedMap.complexity;
            }
        }
    }
}
