using System;
using System.ComponentModel;

namespace HttpStatusExtention.Models
{
    public enum BeatDataCharacteristics
    {
        Unkown,
        [Description("LEVEL_STANDARD")]
        Standard,
        [Description("LEVEL_ONE_SABER")]
        OneSaber,
        [Description("LEVEL_NO_ARROWS")]
        NoArrows,
        [Description("Lightshow")]
        Lightshow,
        [Description("LEVEL_90DEGREE")]
        Degree90,
        [Description("LEVEL_360DEGREE")]
        Degree360,
        [Description("Lawless")]
        Lawless
    }

    public enum BeatMapDifficulty
    {
        Easy = 1,
        Normal,
        Hard,
        Expert,
        ExpertPlus
    }
    [Flags]
    public enum RecomendMod
    {
        NoodleExtensions = 1 << 1,
        MappingExtensions = 1 << 2,
        Chroma = 1 << 3,
        Cinema = 1 << 4
    }

    public enum RankStatus
    {
        Unranked,
        Ranked,
        Qualified,
        Queued
    }
}
