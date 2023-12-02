using HttpStatusExtention.Converters;
using HttpStatusExtention.Extentions;
using HttpStatusExtention.Interfaces;
using HttpStatusExtention.Models;
using HttpStatusExtention.PPCounters;
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
        private PPDownloader _downloader = null;
        private volatile bool _init = false;

        public BeatSongData GetBeatStarSong(CustomPreviewBeatmapLevel beatmapLevel)
        {
            if (!this._init) {
                return null;
            }
            var hash = beatmapLevel.GetHashOrLevelID();
            if (hash.Length != 40) {
                return null;
            }
            _ = this._songDetails.songs.FindByHash(hash, out var song);
            var result = new BeatSongData
            {
                Characteristics = new ConcurrentDictionary<BeatDataCharacteristics, ConcurrentDictionary<BeatMapDifficulty, BeatSongDataDifficultyStats>>()
            };
            foreach (var chara in song.difficulties.GroupBy(x => x.characteristic)) {
                var characteristics = SongDetailsConveter.ConvertToBeatDataCharacteristics(chara.Key);
                var dic = new ConcurrentDictionary<BeatMapDifficulty, BeatSongDataDifficultyStats>();
                foreach (var diff in chara) {
                    float calcPP()
                    {
                        return diff.stars <= 0.05f || (diff.song.rankedStates & SongDetailsCache.Structs.RankedStates.ScoresaberRanked) == 0
                            ? 0f
                            : diff.stars * 43.146f;
                    }
                    var pp = calcPP();
                    var diffData = new BeatSongDataDifficultyStats
                    {
                        Difficulty = SongDetailsConveter.ConvertToBeatMapDifficulty(diff.difficulty),
                        Star = diff.stars,
                        NJS = diff.njs,
                        Bombs = (int)diff.bombs,
                        Notes = (int)diff.notes,
                        Obstacles = (int)diff.obstacles,
                        PP = pp,
                        Mods = SongDetailsConveter.ConvertToRecomendMod(diff.mods),
                        Ranked = (diff.song.rankedStates & SongDetailsCache.Structs.RankedStates.ScoresaberRanked) != 0,
                        Song = result,
                        Characteristics = characteristics
                    };
                    _ = dic.TryAdd(diffData.Difficulty, diffData);
                }
                _ = result.Characteristics.TryAdd(characteristics, dic);
            }
            result.Key = song.key;
            result.BPM = song.bpm;
            result.Rating = song.rating;
            result.DownloadCount = (int)song.downloadCount;
            result.Upvotes = (int)song.upvotes;
            result.Downvotes = (int)song.downvotes;
            result.SongDuration = (int)song.songDuration.TotalSeconds;
            result.DiffOffset = (int)song.diffOffset;
            result.DiffCount = song.diffCount;
            result.RankedStatus = SongDetailsConveter.ConvertToTRankStatus(song.rankedStates);
            result.UploadTime = song.uploadTime;
            result.Hash = song.hash;
            result.SongName = song.songName;
            result.SongAuthorName = song.songAuthorName;
            result.LevelAuthorName = song.levelAuthorName;
            result.CoverURL = song.coverURL;
            result.UploaderName = song.uploaderName;
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
            return !song.Characteristics.TryGetValue(beatDataCharacteristics, out var dic)
                ? null
                : !dic.TryGetValue(BeatMapCoreConverter.ConvertToBeatMapDifficulity(difficulty), out var result) ? null : result;
        }

        public BeatSongDataDifficultyStats GetBeatStarSongDiffculityStats(CustomPreviewBeatmapLevel beatmapLevel, BeatmapDifficulty difficulty, BeatDataCharacteristics beatDataCharacteristics)
        {
            var song = this.GetBeatStarSong(beatmapLevel);
            return song == null ? null : this.GetBeatStarSongDiffculityStats(song, difficulty, beatDataCharacteristics);
        }

        public double GetPP(CustomPreviewBeatmapLevel beatmapLevel, BeatmapDifficulty difficulty, BeatDataCharacteristics beatDataCharacteristics)
        {
            if (beatDataCharacteristics == BeatDataCharacteristics.Standard && this._downloader.Init && this._downloader.RowPPs.TryGetValue(beatmapLevel.GetHashOrLevelID().ToUpper(), out var pp)) {
                // PP counterと同じ処理
                switch (difficulty) {
                    case BeatmapDifficulty.Easy:
                        return pp._Easy_SoloStandard;
                    case BeatmapDifficulty.Normal:
                        return pp._Normal_SoloStandard;
                    case BeatmapDifficulty.Hard:
                        return pp._Hard_SoloStandard;
                    case BeatmapDifficulty.Expert:
                        return pp._Expert_SoloStandard;
                    case BeatmapDifficulty.ExpertPlus:
                        return pp._ExpertPlus_SoloStandard;
                    default:
                        break;
                }
            }
            var song = this.GetBeatStarSongDiffculityStats(beatmapLevel, difficulty, beatDataCharacteristics);
            return song != null ? (double)song.PP : 0;
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
            var song = this.GetBeatStarSong(beatmapLevel);
            return this.GetBeatStarSongDiffculityStats(song, beatmapDifficulty, beatDataCharacteristics).Ranked;
        }

        public bool IsRank(string levelID, BeatmapDifficulty beatmapDifficulty, BeatDataCharacteristics beatDataCharacteristics)
        {
            var prevMap = SongCore.Loader.GetLevelById(levelID);
            return prevMap != null && prevMap is CustomPreviewBeatmapLevel custom && this.IsRank(custom, beatmapDifficulty, beatDataCharacteristics);
        }
        [Inject]
        private void Constractor(PPDownloader downloader)
        {
            this._downloader = downloader;
        }
    }
}
