using Newtonsoft.Json;

namespace HttpStatusExtention.PPCounters
{
    public class RawPPData
    {
        [JsonProperty("_Easy_SoloStandard", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public float EasySoloStandard { get; set; }
        [JsonProperty("_Normal_SoloStandard", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public float NormalSoloStandard { get; set; }
        [JsonProperty("_Hard_SoloStandard", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public float HardSoloStandard { get; set; }
        [JsonProperty("_Expert_SoloStandard", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public float ExpertSoloStandard { get; set; }
        [JsonProperty("_ExpertPlus_SoloStandard", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public float ExpertPlusSoloStandard { get; set; }
    }
}
