using System;
using System.Collections.Concurrent;

namespace HttpStatusExtention.Models
{
    public class BeatSongData
    {
        public float bpm { get; set; }
        public int downloadCount { get; set; }
        public int upvotes { get; set; }
        public int downvotes { get; set; }
        public int songDuration { get; set; }
        public int diffOffset { get; set; }
        public int diffCount { get; set; }
        public RankStatus rankedStatus { get; set; }
        public float rating { get; set; }
        public DateTime uploadTime { get; set; }
        public string key { get; set; }
        public string hash { get; set; }
        public string songName { get; set; }
        public string songAuthorName { get; set; }
        public string levelAuthorName { get; set; }
        public string coverURL { get; set; }
        public string uploaderName { get; set; }
        public ConcurrentDictionary<BeatDataCharacteristics, ConcurrentDictionary<BeatMapDifficulty, BeatSongDataDifficultyStats>> characteristics { get; set; }
    }
}