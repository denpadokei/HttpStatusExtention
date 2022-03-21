using HttpSiraStatus;
using HttpSiraStatus.Interfaces;
using HttpSiraStatus.Util;
using HttpStatusExtention.Interfaces;
using HttpStatusExtention.Models;
using HttpStatusExtention.PPCounters;
using SongCore;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Zenject;

namespace HttpStatusExtention
{
    public class HttpStatusExtentionController : IInitializable, IDisposable
    {
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // パブリックメソッド
        public void Initialize()
        {
            this.Setup();
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プライベートメソッド
        private void OnGameResume()
        {
            HMMainThreadDispatcher.instance.Enqueue(this.SongStartWait(false));
        }
        private void SendPP()
        {
            this.SendPP(this._relativeScoreAndImmediateRankCounter.relativeScore);
        }

        private void SendPP(float relativeScore)
        {
            if (this._statusManager.StatusJSON["performance"] == null) {
                this._statusManager.StatusJSON["performance"] = new JSONObject();
            }
            var jsonObject = this._statusManager.StatusJSON["performance"].AsObject;
            jsonObject["current_pp"].AsFloat = PPCounterUtil.CalculatePP(this.songRawPP, relativeScore, PPCounterUtil.AllowedPositiveModifiers(this._levelID));
            this._statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.ScoreChanged);
        }

        private void Setup()
        {
            Plugin.Log.Debug($"Setup start.");
            this._gamePause.didResumeEvent += this.OnGameResume;
            this._relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent += this.RelativeScoreAndImmediateRankCounter_relativeScoreOrImmediateRankDidChangeEvent;
            var beatmapLevel = this._gameplayCoreSceneSetupData.difficultyBeatmap.level;
            var key = this._gameplayCoreSceneSetupData.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            this._currentBeatmapCharacteristics = Enum.GetValues(typeof(BeatDataCharacteristics)).OfType<BeatDataCharacteristics>().FirstOrDefault(x => x.GetDescription() == key);
            this._currentBeatmapDifficulty = this._gameplayCoreSceneSetupData.difficultyBeatmap.difficulty;
            this._levelID = beatmapLevel.levelID;

            var previewBeatmap = Loader.GetLevelById(beatmapLevel.levelID);
            this._currentCustomBeatmapLevel = previewBeatmap as CustomPreviewBeatmapLevel;
            if (this._currentCustomBeatmapLevel != null) {
                this.SetStarInfo(this._levelID);
            }
            HMMainThreadDispatcher.instance.Enqueue(this.SongStartWait());
        }

        private void SetStarInfo(string levelID)
        {
            this.songRawPP = (float)this._songDataUtil.GetPP(this._currentCustomBeatmapLevel, this._currentBeatmapDifficulty, this._currentBeatmapCharacteristics);
            this.SetCustomLabel(this._currentCustomBeatmapLevel, this._currentBeatmapDifficulty, this._currentBeatmapCharacteristics);
            this._currentStarSong = this._songDataUtil.GetBeatStarSong(this._currentCustomBeatmapLevel);
            this._currentStarSongDiff = this._songDataUtil.GetBeatStarSongDiffculityStats(this._currentCustomBeatmapLevel, this._currentBeatmapDifficulty, this._currentBeatmapCharacteristics);
            if (this._statusManager.StatusJSON["beatmap"] == null) {
                this._statusManager.StatusJSON["beatmap"] = new JSONObject();
            }
            var beatmapJson = this._statusManager.StatusJSON["beatmap"].AsObject;

            if (this._currentStarSong != null && this._currentStarSongDiff != null) {
                var multiplier = this._statusManager.GameStatus.modifierMultiplier;
                if (multiplier != 1 && !PPCounterUtil.AllowedPositiveModifiers(levelID)) {
                    beatmapJson["pp"] = 0;
                }
                else {
                    beatmapJson["pp"] = new JSONNumber(PPCounterUtil.CalculatePP(this.songRawPP, multiplier, PPCounterUtil.AllowedPositiveModifiers(levelID)));
                }
                beatmapJson["star"] = new JSONNumber(this._currentStarSongDiff.star);
                beatmapJson["downloadCount"] = new JSONNumber(this._currentStarSong.downloadCount);
                beatmapJson["upVotes"] = new JSONNumber(this._currentStarSong.upvotes);
                beatmapJson["downVotes"] = new JSONNumber(this._currentStarSong.downvotes);
                beatmapJson["rating"] = new JSONNumber(this._currentStarSong.rating);
            }
        }

        private void RelativeScoreAndImmediateRankCounter_relativeScoreOrImmediateRankDidChangeEvent()
        {
            this.SendPP();
        }

        private void SetCustomLabel(CustomPreviewBeatmapLevel beatmap, BeatmapDifficulty diff, BeatDataCharacteristics beatDataCharacteristics)
        {
            if (beatmap == null) {
                return;
            }
            var songData = Collections.RetrieveExtraSongData(SongCore.Utilities.Hashing.GetCustomLevelHash(beatmap));
            var diffData = songData._difficulties?.FirstOrDefault(x => x._beatmapCharacteristicName == beatDataCharacteristics.GetDescription() && x._difficulty == diff);
            var currentDiffLabel = diffData?._difficultyLabel;
            if (string.IsNullOrEmpty(currentDiffLabel)) {
                return;
            }
            if (this._statusManager.StatusJSON["beatmap"] == null) {
                this._statusManager.StatusJSON["beatmap"] = new JSONObject();
            }
            var beatmapJson = this._statusManager.StatusJSON["beatmap"].AsObject;
            beatmapJson["customLabel"] = currentDiffLabel;
        }

        private IEnumerator SongStartWait(bool songStart = true)
        {
            if (this._audioTimeSource == null) {
                yield break;
            }
            var songTime = this._audioTimeSource.songTime;
            yield return new WaitWhile(() => this._audioTimeSource.songTime <= songTime);
            var practiceSettings = this._gameplayCoreSceneSetupData.practiceSettings;
            var songSpeedMul = this._gameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul;
            if (practiceSettings != null) {
                songSpeedMul = practiceSettings.songSpeedMul;
            }

            this._statusManager.GameStatus.start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - (long)(this._audioTimeSource.songTime * 1000f / songSpeedMul);
            //resumeの時はstartSongTime分がsongTimeに含まれているので処理不要
            if (songStart && practiceSettings != null) {
                this._statusManager.GameStatus.start -= (long)(practiceSettings.startSongTime * 1000f / songSpeedMul);
            }

            if (songStart) {
                this._statusManager.EmitStatusUpdate(ChangedProperty.AllButNoteCut, BeatSaberEvent.SongStart);
            }
            else {
                this._statusManager.EmitStatusUpdate(ChangedProperty.Beatmap, BeatSaberEvent.Resume);
            }
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // メンバ変数
        private IGamePause _gamePause;
        private bool _disposedValue;
        private IStatusManager _statusManager;
        private RelativeScoreAndImmediateRankCounter _relativeScoreAndImmediateRankCounter;
        private IAudioTimeSource _audioTimeSource;
        private GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;
        private ISongDataUtil _songDataUtil;
        private CustomPreviewBeatmapLevel _currentCustomBeatmapLevel;
        private BeatmapDifficulty _currentBeatmapDifficulty;
        private BeatDataCharacteristics _currentBeatmapCharacteristics;
        private BeatSongData _currentStarSong;
        private BeatSongDataDifficultyStats _currentStarSongDiff;
        private float songRawPP;
        private string _levelID;
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // 構築・破棄
        [Inject]
        protected void Constractor(
            IStatusManager statusManager,
            RelativeScoreAndImmediateRankCounter relativeScoreAndImmediateRankCounter,
            IAudioTimeSource audioTimeSource,
            GameplayCoreSceneSetupData gameplayCoreSceneSetupData,
            ISongDataUtil songDataUtil,
            IGamePause gamePause)
        {
            this._statusManager = statusManager;
            this._relativeScoreAndImmediateRankCounter = relativeScoreAndImmediateRankCounter;
            this._audioTimeSource = audioTimeSource;
            this._gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
            this._songDataUtil = songDataUtil;
            this._gamePause = gamePause;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue) {
                if (disposing) {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    Plugin.Log.Debug($"Dispose call");
                    this._gamePause.didResumeEvent -= this.OnGameResume;
                    this._relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent -= this.RelativeScoreAndImmediateRankCounter_relativeScoreOrImmediateRankDidChangeEvent;
                }
                this._disposedValue = true;
            }
        }
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
