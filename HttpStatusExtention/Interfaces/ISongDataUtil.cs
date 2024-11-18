using HttpStatusExtention.Models;

namespace HttpStatusExtention.Interfaces
{
    public interface ISongDataUtil
    {
        BeatSongData GetBeatStarSong(BeatmapLevel beatmapLevel);
        BeatSongDataDifficultyStats GetBeatStarSongDiffculityStats(BeatSongData song, BeatmapDifficulty difficulty);
        BeatSongDataDifficultyStats GetBeatStarSongDiffculityStats(BeatSongData song, BeatmapDifficulty difficulty, BeatDataCharacteristics beatDataCharacteristics);
        BeatSongDataDifficultyStats GetBeatStarSongDiffculityStats(BeatmapLevel beatmapLevel, BeatmapDifficulty difficulty);
        BeatSongDataDifficultyStats GetBeatStarSongDiffculityStats(BeatmapLevel beatmapLevel, BeatmapDifficulty difficulty, BeatDataCharacteristics beatDataCharacteristics);
        double GetPP(BeatmapLevel beatmapLevel, BeatmapDifficulty difficulty, BeatDataCharacteristics beatDataCharacteristics);
        void Initialize();
        bool IsRank(BeatmapLevel beatmapLevel, BeatmapDifficulty beatmapDifficulty, BeatDataCharacteristics beatDataCharacteristics);
        bool IsRank(string levelID, BeatmapDifficulty beatmapDifficulty, BeatDataCharacteristics beatDataCharacteristics);
    }
}