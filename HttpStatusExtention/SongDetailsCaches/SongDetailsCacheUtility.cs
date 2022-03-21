﻿using HttpStatusExtention.Converters;
using HttpStatusExtention.Extentions;
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
            this._songDetails.songs.FindByHash(hash, out var song);
            var result = new BeatSongData
            {
                Characteristics = new ConcurrentDictionary<BeatDataCharacteristics, ConcurrentDictionary<BeatMapDifficulty, BeatSongDataDifficultyStats>>()
            };
            foreach (var chara in song.difficulties.GroupBy(x => x.characteristic)) {
                var diffData = new BeatSongDataDifficultyStats();
                var dic = new ConcurrentDictionary<BeatMapDifficulty, BeatSongDataDifficultyStats>();
                foreach (var diff in chara) {
                    diffData.Difficulty = SongDetailsConveter.ConvertToBeatMapDifficulty(diff.difficulty);
                    diffData.Star = diff.stars;
                    diffData.NJS = diff.njs;
                    diffData.Bombs = (int)diff.bombs;
                    diffData.Notes = (int)diff.notes;
                    diffData.Obstacles = (int)diff.obstacles;
                    diffData.PP = diff.approximatePpValue;
                    diffData.Mods = SongDetailsConveter.ConvertToRecomendMod(diff.mods);
                    diffData.Ranked = diff.ranked;
                    dic.TryAdd(diffData.Difficulty, diffData);
                }
                diffData.Characteristics = SongDetailsConveter.ConvertToBeatDataCharacteristics(chara.Key);
                result.Characteristics.TryAdd(diffData.Characteristics, dic);
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
            result.RankedStatus = SongDetailsConveter.ConvertToTRankStatus(song.rankedStatus);
            result.UploadTime = song.uploadTime;
            result.Hash = song.hash;
            result.SongName = song.songName;
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
            if (!song.Characteristics.TryGetValue(beatDataCharacteristics, out var dic)) {
                return null;
            }
            if (!dic.TryGetValue(BeatMapCoreConverter.ConvertToBeatMapDifficulity(difficulty), out var result)) {
                return null;
            }
            return result;
        }

        public BeatSongDataDifficultyStats GetBeatStarSongDiffculityStats(CustomPreviewBeatmapLevel beatmapLevel, BeatmapDifficulty difficulty, BeatDataCharacteristics beatDataCharacteristics)
        {
            var song = this.GetBeatStarSong(beatmapLevel);
            if (song == null) {
                return null;
            }
            return this.GetBeatStarSongDiffculityStats(song, difficulty, beatDataCharacteristics);
        }

        public double GetPP(CustomPreviewBeatmapLevel beatmapLevel, BeatmapDifficulty difficulty, BeatDataCharacteristics beatDataCharacteristics)
        {
            var song = this.GetBeatStarSongDiffculityStats(beatmapLevel, difficulty, beatDataCharacteristics);
            return song.PP;
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