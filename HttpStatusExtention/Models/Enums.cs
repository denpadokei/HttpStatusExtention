using System;
using System.ComponentModel;

namespace HttpStatusExtention.Models
{
    public enum BeatDataCharacteristics
    {
        [Description("Custom")]
        Unkown,
        [Description("Standard")]
        Standard,
        [Description("OneSaber")]
        OneSaber,
        [Description("NoArrows")]
        NoArrows,
        [Description("Lightshow")]
        Lightshow,
        [Description("90Degree")]
        Degree90,
        [Description("360Degree")]
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
