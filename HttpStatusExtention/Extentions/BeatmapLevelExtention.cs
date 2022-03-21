using System.Linq;

namespace HttpStatusExtention.Extentions
{
    public static class BeatmapLevelExtention
    {
        public static string GetHashOrLevelID(this IPreviewBeatmapLevel level)
        {
            var strings = level.levelID.Split('_');
            if (strings.Length != 3 || strings.ElementAt(2).Length < 40) {
                return level.levelID;
            }
            return strings.ElementAt(2).Substring(0, 40);
        }
    }
}
