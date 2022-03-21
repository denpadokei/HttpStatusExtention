namespace HttpStatusExtention.Models
{
    public class BeatSongDataDifficultyStats
    {
        public float star { get; set; }
        public float njs { get; set; }
        public int bombs { get; set; }
        public int notes { get; set; }
        public int obstacles { get; set; }
        public BeatDataCharacteristics Characteristics { get; set; }
        public BeatMapDifficulty Difficulty { get; set; }
        public RecomendMod Mods { get; set; }
        public BeatSongData Song { get; set; }
        public float pp { get; set; }
    }
}
