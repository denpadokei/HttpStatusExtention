namespace HttpStatusExtention.Models
{
    public class BeatSongDataDifficultyStats
    {
        public float Star { get; set; }
        public float NJS { get; set; }
        public int Bombs { get; set; }
        public int Notes { get; set; }
        public int Obstacles { get; set; }
        public bool Ranked { get; set; }
        public BeatDataCharacteristics Characteristics { get; set; }
        public BeatMapDifficulty Difficulty { get; set; }
        public RecomendMod Mods { get; set; }
        public BeatSongData Song { get; set; }
        public float PP { get; set; }
    }
}
