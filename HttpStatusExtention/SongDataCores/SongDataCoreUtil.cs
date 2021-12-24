using SongCore;
using SongDataCore.BeatStar;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace HttpStatusExtention.SongDataCores
{
    public class SongDataCoreUtil
    {
        private static ReadOnlyDictionary<string, BeatStarSong> _songCache;
        /// <summary>
        /// HASHがキーになっています。
        /// </summary>
        public static ReadOnlyDictionary<string, BeatStarSong> SongDataCoreSongs => _songCache;


        public static bool IsRank(CustomPreviewBeatmapLevel beatmapLevel, BeatmapDifficulty beatmapDifficulty)
        {
            if (GetBeatStarSongDiffculityStats(beatmapLevel, beatmapDifficulty) is BeatStarSongDifficultyStats beatStarSongDifficultyStats) {
                return beatStarSongDifficultyStats.pp > 0;
            }
            else {
                return false;
            }
        }

        public static bool IsRank(string levelID, BeatmapDifficulty beatmapDifficulty)
        {
            var beatmapLevel = Loader.GetLevelByHash(levelID.Split('_').Last());
            return IsRank(beatmapLevel, beatmapDifficulty);
        }

        public static BeatStarSong GetBeatStarSong(CustomPreviewBeatmapLevel beatmapLevel)
        {
            if (SongDataCoreSongs.TryGetValue(beatmapLevel.levelID.Split('_').Last(), out var beatStarSong)) {
                return beatStarSong;
            }
            else {
                return null;
            }
        }

        public static BeatStarSongDifficultyStats GetBeatStarSongDiffculityStats(BeatStarSong song, BeatmapDifficulty difficulty)
        {

            return song.diffs.FirstOrDefault(x => x.diff.Replace("+", "Plus").ToLower() == difficulty.ToString().ToLower());
        }

        public static BeatStarSongDifficultyStats GetBeatStarSongDiffculityStats(CustomPreviewBeatmapLevel beatmapLevel, BeatmapDifficulty difficulty)
        {
            if (SongDataCoreSongs.TryGetValue(beatmapLevel.levelID.Split('_').Last(), out var beatStarSong)) {
                return GetBeatStarSongDiffculityStats(beatStarSong, difficulty);
            }
            else {
                return null;
            }
        }

        public static double GetPP(CustomPreviewBeatmapLevel beatmapLevel, BeatmapDifficulty difficulty) => GetBeatStarSongDiffculityStats(beatmapLevel, difficulty)?.pp ?? 0;

        public static IEnumerator Initialize()
        {
            yield return new WaitWhile(() => SongDataCore.Plugin.Songs == null || !SongDataCore.Plugin.Songs.IsDataAvailable());
            yield return new WaitWhile(() => SongDataCore.Plugin.Songs.IsDownloading);
            var dictionary = new Dictionary<string, BeatStarSong>();
            foreach (var item in SongDataCore.Plugin.Songs.Data.Songs) {
                dictionary.Add(item.Key, item.Value);
            }
            _songCache = new ReadOnlyDictionary<string, BeatStarSong>(dictionary);
        }
    }
}
