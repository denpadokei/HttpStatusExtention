using System;
using System.Collections.Concurrent;

namespace HttpStatusExtention.Models
{
    public class BeatSongData
    {
        public float BPM { get; set; }
        public int DownloadCount { get; set; }
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
        public int SongDuration { get; set; }
        public int DiffOffset { get; set; }
        public int DiffCount { get; set; }
        public RankStatus RankedStatus { get; set; }
        public float Rating { get; set; }
        public DateTime UploadTime { get; set; }
        public string Key { get; set; }
        public string Hash { get; set; }
        public string SongName { get; set; }
        public string SongAuthorName { get; set; }
        public string LevelAuthorName { get; set; }
        public string CoverURL { get; set; }
        public string UploaderName { get; set; }
        public ConcurrentDictionary<BeatDataCharacteristics, ConcurrentDictionary<BeatMapDifficulty, BeatSongDataDifficultyStats>> Characteristics { get; set; }
    }
}