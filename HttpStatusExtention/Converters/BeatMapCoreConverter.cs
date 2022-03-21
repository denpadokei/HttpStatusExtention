using HttpStatusExtention.Models;

namespace HttpStatusExtention.Converters
{
    public static class BeatMapCoreConverter
    {
        public static BeatMapDifficulty ConvertToBeatMapDifficulity(BeatmapDifficulty difficulty)
        {
            var result = BeatMapDifficulty.Easy;
            switch (difficulty) {
                case BeatmapDifficulty.Easy:
                    result = BeatMapDifficulty.Easy;
                    break;
                case BeatmapDifficulty.Normal:
                    result = BeatMapDifficulty.Normal;
                    break;
                case BeatmapDifficulty.Hard:
                    result = BeatMapDifficulty.Hard;
                    break;
                case BeatmapDifficulty.Expert:
                    result = BeatMapDifficulty.Expert;
                    break;
                case BeatmapDifficulty.ExpertPlus:
                    result = BeatMapDifficulty.ExpertPlus;
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}
