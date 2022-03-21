using HttpStatusExtention.Models;

namespace HttpStatusExtention.Interfaces
{
    public interface ISongDataUtil
    {
        BeatSongData GetBeatStarSong(CustomPreviewBeatmapLevel beatmapLevel);
        BeatSongDataDifficultyStats GetBeatStarSongDiffculityStats(BeatSongData song, BeatmapDifficulty difficulty);
        BeatSongDataDifficultyStats GetBeatStarSongDiffculityStats(BeatSongData song, BeatmapDifficulty difficulty, BeatDataCharacteristics beatDataCharacteristics);
        BeatSongDataDifficultyStats GetBeatStarSongDiffculityStats(CustomPreviewBeatmapLevel beatmapLevel, BeatmapDifficulty difficulty);
        BeatSongDataDifficultyStats GetBeatStarSongDiffculityStats(CustomPreviewBeatmapLevel beatmapLevel, BeatmapDifficulty difficulty, BeatDataCharacteristics beatDataCharacteristics);
        double GetPP(CustomPreviewBeatmapLevel beatmapLevel, BeatmapDifficulty difficulty);
        void Initialize();
        bool IsRank(CustomPreviewBeatmapLevel beatmapLevel, BeatmapDifficulty beatmapDifficulty);
        bool IsRank(string levelID, BeatmapDifficulty beatmapDifficulty);
    }
}