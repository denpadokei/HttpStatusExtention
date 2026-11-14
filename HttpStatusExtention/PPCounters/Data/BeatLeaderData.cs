using HttpStatusExtention.Models;
using Newtonsoft.Json;
using SiraUtil.Zenject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HttpStatusExtention.PPCounters
{
    public class BeatLeaderData : IAsyncInitializable
    {
        private static readonly string BL_CACHE_FILE = Path.Combine(Environment.CurrentDirectory, "UserData", "BeatLeader", "LeaderboardsCache");

        // BeatLeader のキャッシュは数 MB あり、曲ごとに読み直すと無視できない負荷になる。
        // ファイルが更新されたときだけ読み直し、それ以外は前回の解析結果を使い回す。
        private static Dictionary<SongID, BeatLeaderLeaderboardCacheEntry> s_cache = new Dictionary<SongID, BeatLeaderLeaderboardCacheEntry>();
        private static DateTime s_cacheLastWriteTimeUtc = DateTime.MinValue;
        private static long s_cacheLength = -1;
        private static bool s_cacheLoaded;

        public bool DataInit { get; private set; } = false;

        private readonly TaskCompletionSource<bool> _ready = new TaskCompletionSource<bool>();
        private Dictionary<SongID, BeatLeaderLeaderboardCacheEntry> _cache = s_cache;

        public async Task InitializeAsync(CancellationToken token)
        {
            try {
                var fileInfo = new FileInfo(BL_CACHE_FILE);
                if (!fileInfo.Exists) {
                    this._cache = new Dictionary<SongID, BeatLeaderLeaderboardCacheEntry>();
                    return;
                }

                if (s_cacheLoaded && s_cacheLastWriteTimeUtc == fileInfo.LastWriteTimeUtc && s_cacheLength == fileInfo.Length) {
                    this._cache = s_cache;
                    this.DataInit = true;
                    return;
                }

                // 数 MB の JSON 解析をメインスレッドで行わない。
                var cache = await Task.Run(() => LoadCacheFile(fileInfo)).ConfigureAwait(true);
                if (cache == null) {
                    return;
                }

                s_cache = cache;
                s_cacheLastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
                s_cacheLength = fileInfo.Length;
                s_cacheLoaded = true;

                this._cache = cache;
                this.DataInit = true;
            }
            catch (Exception e) {
                Plugin.Log.Warn($"Failed to load BeatLeader cache: {e.Message}");
            }
            finally {
                _ = this._ready.TrySetResult(true);
            }
        }

        public Task<bool> WaitForReadyAsync(CancellationToken token)
        {
            return TaskUtils.WaitAsync(this._ready.Task, token);
        }

        public bool IsRanked(SongID songID)
        {
            return this.DataInit && this._cache.ContainsKey(songID) && this._cache[songID].DifficultyInfo.stars > 0;
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

            return this._cache[songID].DifficultyInfo.modifierValues;
        }

        private static Dictionary<SongID, BeatLeaderLeaderboardCacheEntry> LoadCacheFile(FileInfo fileInfo)
        {
            try {
                var data = File.ReadAllText(fileInfo.FullName);
                var cacheFileData = JsonConvert.DeserializeObject<BeatLeaderCacheFileData>(data);
                if (cacheFileData?.Entries == null) {
                    return null;
                }

                var cache = new Dictionary<SongID, BeatLeaderLeaderboardCacheEntry>();
                foreach (var entry in cacheFileData.Entries) {
                    try {
                        var songID = new SongID(entry.SongInfo.hash.ToUpper(), SongDataUtils.GetDifficulty(entry.DifficultyInfo.difficultyName));
                        cache[songID] = entry;
                    }
                    catch (Exception) {
                        // 不明な難易度などは無視する。
                    }
                }
                return cache;
            }
            catch (Exception e) {
                Plugin.Log.Warn($"Failed to parse BeatLeader cache: {e.Message}");
                return null;
            }
        }
    }
}
