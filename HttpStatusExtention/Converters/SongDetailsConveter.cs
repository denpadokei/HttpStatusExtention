using HttpStatusExtention.Models;
using SongDetailsCache.Structs;

namespace HttpStatusExtention.Converters
{
    public static class SongDetailsConveter
    {
        public static BeatDataCharacteristics ConvertToBeatDataCharacteristics(MapCharacteristic mapCharacteristic)
        {
            var result = BeatDataCharacteristics.Unkown;
            switch (mapCharacteristic) {
                case MapCharacteristic.Custom:
                    result = BeatDataCharacteristics.Unkown;
                    break;
                case MapCharacteristic.Standard:
                    result = BeatDataCharacteristics.Standard;
                    break;
                case MapCharacteristic.OneSaber:
                    result = BeatDataCharacteristics.OneSaber;
                    break;
                case MapCharacteristic.NoArrows:
                    result = BeatDataCharacteristics.NoArrows;
                    break;
                case MapCharacteristic.NinetyDegree:
                    result = BeatDataCharacteristics.Degree90;
                    break;
                case MapCharacteristic.ThreeSixtyDegree:
                    result = BeatDataCharacteristics.Degree360;
                    break;
                case MapCharacteristic.Lightshow:
                    result = BeatDataCharacteristics.Lightshow;
                    break;
                case MapCharacteristic.Lawless:
                    result = BeatDataCharacteristics.Lawless;
                    break;
                default:
                    break;
            }
            return result;
        }

        public static BeatMapDifficulty ConvertToBeatMapDifficulty(MapDifficulty mapDifficulty)
        {
            var result = BeatMapDifficulty.Easy;
            switch (mapDifficulty) {
                case MapDifficulty.Easy:
                    result = BeatMapDifficulty.Easy;
                    break;
                case MapDifficulty.Normal:
                    result = BeatMapDifficulty.Normal;
                    break;
                case MapDifficulty.Hard:
                    result = BeatMapDifficulty.Hard;
                    break;
                case MapDifficulty.Expert:
                    result = BeatMapDifficulty.Expert;
                    break;
                case MapDifficulty.ExpertPlus:
                    result = BeatMapDifficulty.ExpertPlus;
                    break;
                default:
                    break;
            }
            return result;
        }

        public static RecomendMod ConvertToRecomendMod(MapMods mapMods)
        {
            RecomendMod result = 0;
            if ((mapMods & MapMods.NoodleExtensions) != 0) {
                result |= RecomendMod.NoodleExtensions;
            }
            if ((mapMods & MapMods.MappingExtensions) != 0) {
                result |= RecomendMod.MappingExtensions;
            }
            if ((mapMods & MapMods.Chroma) != 0) {
                result |= RecomendMod.Chroma;
            }
            if ((mapMods & MapMods.Cinema) != 0) {
                result |= RecomendMod.Cinema;
            }
            return result;
        }

        public static RankStatus ConvertToTRankStatus(RankedStatus rankedStatus)
        {
            var result = RankStatus.Unranked;
            switch (rankedStatus) {
                case RankedStatus.Unranked:
                    result = RankStatus.Unranked;
                    break;
                case RankedStatus.Ranked:
                    result = RankStatus.Ranked;
                    break;
                case RankedStatus.Qualified:
                    result = RankStatus.Qualified;
                    break;
                case RankedStatus.Queued:
                    result = RankStatus.Queued;
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}
