using System.Linq;

namespace HttpStatusExtention.Extentions
{
    public static class BeatmapLevelExtention
    {
        public static string GetHashOrLevelID(this BeatmapLevel level)
        {
            var strings = level.levelID.Split('_');
            return strings.Length != 3 || strings.ElementAt(2).Length < 40 ? level.levelID : strings.ElementAt(2).Substring(0, 40);
        }
    }
}
