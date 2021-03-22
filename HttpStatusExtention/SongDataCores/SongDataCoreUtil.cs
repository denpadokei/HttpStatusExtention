using SongCore;
using SongDataCore.BeatStar;
using System.Collections.Generic;
using System.Linq;

namespace HttpStatusExtention.SongDataCores
{
    public class SongDataCoreUtil
    {
        /// <summary>
        /// HASHがキーになっています。
        /// </summary>
        public static Dictionary<string, BeatStarSong> SongDataCoreSongs => SongDataCore.Plugin.Songs.Data.Songs;


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

        public static BeatStarSongDifficultyStats GetBeatStarSongDiffculityStats(BeatStarSong song, BeatmapDifficulty difficulty) => song.diffs.FirstOrDefault(x => x.diff.Replace("+", "Plus").ToLower() == difficulty.ToString().ToLower());

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
    }
}
