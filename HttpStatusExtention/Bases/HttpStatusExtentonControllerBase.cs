using HttpSiraStatus;
using HttpSiraStatus.Interfaces;
using HttpSiraStatus.Util;
using HttpStatusExtention.DataBases;
using HttpStatusExtention.Extentions;
using HttpStatusExtention.PPCounters;
using HttpStatusExtention.SongDataCores;
using SongCore;
using SongDataCore.BeatStar;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Zenject;

namespace HttpStatusExtention.Bases
{
    public abstract class HttpStatusExtentonControllerBase : IInitializable, IDisposable
    {
        private bool disposedValue;
        [Inject]
        private readonly IStatusManager statusManager;
        [Inject]
        private readonly RelativeScoreAndImmediateRankCounter relativeScoreAndImmediateRankCounter;
        [Inject]
        private readonly IAudioTimeSource _audioTimeSource;
        [Inject]
        private readonly GameplayCoreSceneSetupData _currentData;

        private CustomPreviewBeatmapLevel _currentCustomBeatmapLevel;
        private BeatmapDifficulty _currentBeatmapDifficulty;
        private BeatStarSong _currentStarSong;
        private BeatStarSongDifficultyStats _currentStarSongDiff;
        private float songRawPP;

        public void Initialize() => this.Setup();

        private void SendPP() => this.SendPP(this.relativeScoreAndImmediateRankCounter.relativeScore);

        private void SendPP(float relativeScore)
        {
            if (this.statusManager.StatusJSON["performance"] == null) {
                this.statusManager.StatusJSON["performance"] = new JSONObject();
            }
            var jsonObject = this.statusManager.StatusJSON["performance"].AsObject;
            jsonObject["current_pp"].AsFloat = PPCounterUtil.CalculatePP(this.songRawPP, relativeScore, PPCounterUtil.AllowedPositiveModifiers(this._currentData.difficultyBeatmap.level.levelID));
            this.statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.ScoreChanged);
        }

        protected virtual void Setup()
        {
            Plugin.Log.Debug($"Setup start.");

            if (ScoreDataBase.Instance.Init) {
                this.relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent += this.RelativeScoreAndImmediateRankCounter_relativeScoreOrImmediateRankDidChangeEvent;
            }
            var beatmapLevel = this._currentData.difficultyBeatmap.level;
            this._currentBeatmapDifficulty = this._currentData.difficultyBeatmap.difficulty;
            var levelID = beatmapLevel.levelID;

            var previewBeatmap = Loader.GetLevelById(beatmapLevel.levelID);
            this._currentCustomBeatmapLevel = previewBeatmap as CustomPreviewBeatmapLevel;
            if (this._currentCustomBeatmapLevel != null) {
                this.SetStarInfo(levelID);
            }
            HMMainThreadDispatcher.instance.Enqueue(this.SongStartWait());
        }

        private void SetStarInfo(string levelID)
        {
            this.songRawPP = ScoreDataBase.Instance.Init ? PPCounterUtil.GetPP(this._currentCustomBeatmapLevel, this._currentBeatmapDifficulty) : 0;
            this.SetCustomLabel(this._currentCustomBeatmapLevel, this._currentBeatmapDifficulty);
            this._currentStarSong = SongDataCoreUtil.GetBeatStarSong(this._currentCustomBeatmapLevel);
            this._currentStarSongDiff = SongDataCoreUtil.GetBeatStarSongDiffculityStats(this._currentCustomBeatmapLevel, this._currentBeatmapDifficulty);
            if (this.statusManager.StatusJSON["beatmap"] == null) {
                this.statusManager.StatusJSON["beatmap"] = new JSONObject();
            }
            var beatmapJson = this.statusManager.StatusJSON["beatmap"].AsObject;

            if (this._currentStarSong != null && this._currentStarSongDiff != null) {
                var multiplier = this.statusManager.GameStatus.songSpeedMultiplier;
                if (ScoreDataBase.Instance.Init) {
                    if (multiplier == 1 || !PPCounterUtil.AllowedPositiveModifiers(levelID)) {
                        beatmapJson["pp"] = new JSONNumber(this.songRawPP * 1.12);
                    }
                    else {
                        beatmapJson["pp"] = new JSONNumber(PPCounterUtil.CalculatePP(this.songRawPP, multiplier, PPCounterUtil.AllowedPositiveModifiers(levelID)));
                    }
                }
                beatmapJson["star"] = new JSONNumber(this._currentStarSongDiff.star);
                beatmapJson["downloadCount"] = new JSONNumber(this._currentStarSong.downloadCount);
                beatmapJson["upVotes"] = new JSONNumber(this._currentStarSong.upVotes);
                beatmapJson["downVotes"] = new JSONNumber(this._currentStarSong.downVotes);
                beatmapJson["rating"] = new JSONNumber(this._currentStarSong.rating);
            }
        }

        private void RelativeScoreAndImmediateRankCounter_relativeScoreOrImmediateRankDidChangeEvent() => this.SendPP();

        private void SetCustomLabel(CustomPreviewBeatmapLevel beatmap, BeatmapDifficulty diff)
        {
            if (beatmap == null) {
                return;
            }
            var songData = Collections.RetrieveExtraSongData(SongCore.Utilities.Hashing.GetCustomLevelHash(beatmap));
            var diffData = songData._difficulties?.FirstOrDefault(x => x._difficulty == diff);
            var currentDiffLabel = diffData._difficultyLabel;
            if (string.IsNullOrEmpty(currentDiffLabel)) {
                return;
            }
            if (this.statusManager.StatusJSON["beatmap"] == null) {
                this.statusManager.StatusJSON["beatmap"] = new JSONObject();
            }
            var beatmapJson = this.statusManager.StatusJSON["beatmap"].AsObject;
            beatmapJson["customLabel"] = currentDiffLabel;
        }

        protected virtual IEnumerator SongStartWait(bool songStart = true)
        {
            if (this._audioTimeSource == null) {
                yield break;
            }
            var songTime = this._audioTimeSource.songTime;
            yield return new WaitWhile(() => this._audioTimeSource.songTime <= songTime);
            var practiceSettings = this._currentData.practiceSettings;
            var songSpeedMul = this._currentData.gameplayModifiers.songSpeedMul;
            if (practiceSettings != null) songSpeedMul = practiceSettings.songSpeedMul;
            this.statusManager.GameStatus.start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - (long)(this._audioTimeSource.songTime * 1000f / songSpeedMul);
            //resumeの時はstartSongTime分がsongTimeに含まれているので処理不要
            if (songStart && practiceSettings != null) this.statusManager.GameStatus.start -= (long)(practiceSettings.startSongTime * 1000f / songSpeedMul);
            if (songStart) {
                this.statusManager.EmitStatusUpdate(ChangedProperty.AllButNoteCut, BeatSaberEvent.SongStart);
            }
            else {
                this.statusManager.EmitStatusUpdate(ChangedProperty.Beatmap, BeatSaberEvent.Resume);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue) {
                if (disposing) {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    Plugin.Log.Debug($"Dispose call");
                    this.relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent -= this.RelativeScoreAndImmediateRankCounter_relativeScoreOrImmediateRankDidChangeEvent;
                }
                this.disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
