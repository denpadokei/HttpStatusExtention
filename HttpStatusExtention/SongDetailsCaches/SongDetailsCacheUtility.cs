using HttpStatusExtention.Converters;
using HttpStatusExtention.Interfaces;
using HttpStatusExtention.Models;
using SongDetailsCache;
using System.Collections.Concurrent;
using System.Linq;
using Zenject;

namespace HttpStatusExtention.SongDetailsCaches
{
    /// <summary>
    /// 書き直すのめんどくさすぎる
    /// </summary>
    public class SongDetailsCacheUtility : ISongDataUtil, IInitializable
    {
        private SongDetails _songDetails = null;
        private bool _init = false;

        public BeatSongData GetBeatStarSong(CustomPreviewBeatmapLevel beatmapLevel)
        {
            if (!this._init) {
                return null;
            }
            var levelID = beatmapLevel.levelID.Split('_');
            if (levelID.Length != 3 || levelID[2].Length < 40) {
                return null;
            }
            var hash = levelID[2].Substring(0, 40);
            this._songDetails.songs.FindByHash(hash, out var song);
            var result = new BeatSongData
            {
                characteristics = new ConcurrentDictionary<BeatDataCharacteristics, ConcurrentDictionary<BeatMapDifficulty, BeatSongDataDifficultyStats>>()
            };
            foreach (var chara in song.difficulties.GroupBy(x => x.characteristic)) {
                var diffData = new BeatSongDataDifficultyStats();
                var dic = new ConcurrentDictionary<BeatMapDifficulty, BeatSongDataDifficultyStats>();
                foreach (var diff in chara) {
                    diffData.Difficulty = SongDetailsConveter.ConvertToBeatMapDifficulty(diff.difficulty);
                    diffData.star = diff.stars;
                    diffData.njs = diff.njs;
                    diffData.bombs = (int)diff.bombs;
                    diffData.notes = (int)diff.notes;
                    diffData.obstacles = (int)diff.obstacles;
                    diffData.pp = diff.approximatePpValue;
                    diffData.Mods = SongDetailsConveter.ConvertToRecomendMod(diff.mods);
                    diffData.ranked = diff.ranked;
                    dic.TryAdd(diffData.Difficulty, diffData);
                }
                diffData.Characteristics = SongDetailsConveter.ConvertToBeatDataCharacteristics(chara.Key);
                result.characteristics.TryAdd(diffData.Characteristics, dic);
            }
            result.key = song.key;
            result.bpm = song.bpm;
            result.rating = song.rating;
            result.downloadCount = (int)song.downloadCount;
            result.upvotes = (int)song.upvotes;
            result.downvotes = (int)song.downvotes;
            result.songDuration = (int)song.songDuration.TotalSeconds;
            result.diffOffset = (int)song.diffOffset;
            result.diffCount = song.diffCount;
            result.rankedStatus = SongDetailsConveter.ConvertToTRankStatus(song.rankedStatus);
            result.uploadTime = song.uploadTime;
            result.hash = song.hash;
            result.songName = song.songName;
            result.uploaderName = song.uploaderName;
            return result;
        }

        public BeatSongDataDifficultyStats GetBeatStarSongDiffculityStats(BeatSongData song, BeatmapDifficulty difficulty)
        {
            return this.GetBeatStarSongDiffculityStats(song, difficulty, BeatDataCharacteristics.Standard);
        }

        public BeatSongDataDifficultyStats GetBeatStarSongDiffculityStats(CustomPreviewBeatmapLevel beatmapLevel, BeatmapDifficulty difficulty)
        {
            return this.GetBeatStarSongDiffculityStats(beatmapLevel, difficulty, BeatDataCharacteristics.Standard);
        }

        public BeatSongDataDifficultyStats GetBeatStarSongDiffculityStats(BeatSongData song, BeatmapDifficulty difficulty, BeatDataCharacteristics beatDataCharacteristics)
        {
            if (!this._init) {
                return null;
            }
            if (!song.characteristics.TryGetValue(beatDataCharacteristics, out var dic)) {
                return null;
            }
            if (!dic.TryGetValue(BeatMapCoreConverter.ConvertToBeatMapDifficulity(difficulty), out var result)) {
                return null;
            }
            return result;
        }

        public BeatSongDataDifficultyStats GetBeatStarSongDiffculityStats(CustomPreviewBeatmapLevel beatmapLevel, BeatmapDifficulty difficulty, BeatDataCharacteristics beatDataCharacteristics)
        {
            if (!this._init) {
                return null;
            }
            var song = this.GetBeatStarSong(beatmapLevel);
            if (song == null) {
                return null;
            }
            return this.GetBeatStarSongDiffculityStats(song, difficulty, beatDataCharacteristics);
        }

        public double GetPP(CustomPreviewBeatmapLevel beatmapLevel, BeatmapDifficulty difficulty, BeatDataCharacteristics beatDataCharacteristics)
        {
            if (!this._init) {
                return 0;
            }
            var song = this.GetBeatStarSongDiffculityStats(beatmapLevel, difficulty, beatDataCharacteristics);
            return song.pp;
        }

        public void Initialize()
        {
            _ = SongDetails.Init().ContinueWith(async x =>
            {
                this._songDetails = await x;
                this._init = true;
            });
        }

        public bool IsRank(CustomPreviewBeatmapLevel beatmapLevel, BeatmapDifficulty beatmapDifficulty, BeatDataCharacteristics beatDataCharacteristics)
        {
            if (!this._init) {
                return false;
            }
            var song = GetBeatStarSong(beatmapLevel);
            return this.GetBeatStarSongDiffculityStats(song, beatmapDifficulty, beatDataCharacteristics).ranked;
        }

        public bool IsRank(string levelID, BeatmapDifficulty beatmapDifficulty, BeatDataCharacteristics beatDataCharacteristics)
        {
            var prevMap = SongCore.Loader.GetLevelById(levelID);
            if (prevMap == null) {
                return false;
            }
            if (prevMap is CustomPreviewBeatmapLevel custom) {
                return this.IsRank(custom, beatmapDifficulty, beatDataCharacteristics);
            }
            else {
                return false;
            }
        }
    }
}
