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
        double GetPP(CustomPreviewBeatmapLevel beatmapLevel, BeatmapDifficulty difficulty, BeatDataCharacteristics beatDataCharacteristics);
        void Initialize();
        bool IsRank(CustomPreviewBeatmapLevel beatmapLevel, BeatmapDifficulty beatmapDifficulty, BeatDataCharacteristics beatDataCharacteristics);
        bool IsRank(string levelID, BeatmapDifficulty beatmapDifficulty, BeatDataCharacteristics beatDataCharacteristics);
    }
}