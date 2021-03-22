using System.Linq;

namespace HttpStatusExtention.Extentions
{
    public static class BeatmapLevelExtention
    {
        public static string GetHash(this IBeatmapLevel level)
        {
            var strings = level.levelID.Split('_');
            if (strings.Length != 3) {
                return level.levelID;
            }
            return strings.ElementAt(2);
        }
    }
}
