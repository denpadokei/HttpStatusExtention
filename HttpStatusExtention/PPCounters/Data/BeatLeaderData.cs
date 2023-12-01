using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Zenject;

namespace HttpStatusExtention.PPCounters
{
    public class BeatLeaderData : IInitializable
    {
        private static readonly string BL_CACHE_FILE = Path.Combine(Environment.CurrentDirectory, "UserData", "BeatLeader", "LeaderboardsCache");
        public bool DataInit { get; private set; } = false;

        private readonly Dictionary<SongID, BeatLeaderLeaderboardCacheEntry> _cache = new Dictionary<SongID, BeatLeaderLeaderboardCacheEntry>();

        public void Initialize()
        {
            // TODO: support this better - won't work on first cache creation, or respect in-game cache updates.
            // Could use reflection to access BL cache, but may also want to load data myself so it doesn't rely on bl mod
            this.TryLoadCache();
        }

        public bool IsRanked(SongID songID)
        {
            return this._cache.ContainsKey(songID) && this._cache[songID].DifficultyInfo.stars > 0;
        }

        public BeatLeaderRating GetStars(SongID songID)
        {
            if (!this.DataInit) {
                return default;
            }

            if (!this._cache.ContainsKey(songID)) {
                return default;
            }

            var diffInfo = this._cache[songID].DifficultyInfo;
            return new BeatLeaderRating(diffInfo.accRating, diffInfo.passRating, diffInfo.techRating);
        }

        public ModifiersMap GetModifiersMap(SongID songID)
        {
            if (!this.DataInit) {
                return default;
            }

            if (!this._cache.ContainsKey(songID)) {
                return default;
            }

            var diffInfo = this._cache[songID].DifficultyInfo;
            return this._cache[songID].DifficultyInfo.modifierValues;
        }

        private void TryLoadCache()
        {
            if (File.Exists(BL_CACHE_FILE)) {
                try {
                    var data = File.ReadAllText(BL_CACHE_FILE);
                    var cacheFileData = JsonConvert.DeserializeObject<BeatLeaderCacheFileData>(data);
                    this.CreateCache(cacheFileData);
                    this.DataInit = true;
                }
                catch (Exception) {
                }
            }
        }

        private void CreateCache(BeatLeaderCacheFileData cacheFileData)
        {
            foreach (var entry in cacheFileData.Entries) {
                var songID = new SongID(entry.SongInfo.hash.ToUpper(), SongDataUtils.GetDifficulty(entry.DifficultyInfo.difficultyName));
                this._cache[songID] = entry;
            }
        }
    }
}
