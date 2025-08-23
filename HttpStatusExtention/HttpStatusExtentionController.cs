using HttpSiraStatus.Enums;
using HttpSiraStatus.Interfaces;
using HttpSiraStatus.Util;
using HttpStatusExtention.Interfaces;
using HttpStatusExtention.Models;
using HttpStatusExtention.PPCounters;
using SiraUtil.Zenject;
using SongCore;
using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace HttpStatusExtention
{
    public class HttpStatusExtentionController : IAsyncInitializable, IDisposable
    {
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // パブリックメソッド
        public Task InitializeAsync(CancellationToken token)
        {
            return this.Setup();
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プライベートメソッド
        private void OnGameResume()
        {
            _ = CoroutineManager.Instance.StartCoroutine(this.SongStartWait(false));
        }
        private void SendPP()
        {
            this.SendScoreSaberPP(this._relativeScoreAndImmediateRankCounter.relativeScore);
            this.SendBeatLeaderPP(this._relativeScoreAndImmediateRankCounter.relativeScore);
            this.SendAccSaberAP(this._relativeScoreAndImmediateRankCounter.relativeScore);
        }

        private void SendScoreSaberPP(float relativeScore)
        {
            if (this._scoreSaberRowPP == 0) {
                return;
            }
            if (this._statusManager.StatusJSON["performance"] == null) {
                this._statusManager.StatusJSON["performance"] = new JSONObject();
            }
            var jsonObject = this._statusManager.StatusJSON["performance"].AsObject;
            jsonObject["current_pp"].AsFloat = this._scoreSaberCalculator.CalculatePP(this._scoreSaberRowPP, relativeScore, this._failed);
            this._statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.ScoreChanged);
        }

        private void SendBeatLeaderPP(float accracy)
        {
            if (!this._isBeatLeaderRank) {
                return;
            }
            if (this._statusManager.StatusJSON["performance"] == null) {
                this._statusManager.StatusJSON["performance"] = new JSONObject();
            }
            var jsonObject = this._statusManager.StatusJSON["performance"].AsObject;
            jsonObject["current_bl_pp"].AsFloat = this._beatLeaderCalculator.CalculatePP(this._songID, accracy, this._failed);
            this._statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.ScoreChanged);
        }

        private void SendAccSaberAP(float accracy)
        {
            if (!this._isAccSaberRank) {
                return;
            }
            if (this._statusManager.StatusJSON["performance"] == null) {
                this._statusManager.StatusJSON["performance"] = new JSONObject();
            }
            var jsonObject = this._statusManager.StatusJSON["performance"].AsObject;
            jsonObject["current_acc_saber_ap"].AsFloat = this._accSaberCalculator.CalculateAP(this._scoreSaberRowPP, accracy);
            this._statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.ScoreChanged);
        }

        private async Task Setup()
        {
            Plugin.Log.Debug($"Setup start.");
            this._gamePause.didResumeEvent += this.OnGameResume;
            this._relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent += this.RelativeScoreAndImmediateRankCounter_relativeScoreOrImmediateRankDidChangeEvent;
            var beatmapLevel = this._gameplayCoreSceneSetupData.beatmapLevel;
            var beatmapKey = this._gameplayCoreSceneSetupData.beatmapKey;
            var key = beatmapKey.beatmapCharacteristic.serializedName;
            //this._gameplayCoreSceneSetupData.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            this._currentBeatmapCharacteristics = Enum.GetValues(typeof(BeatDataCharacteristics)).OfType<BeatDataCharacteristics>().FirstOrDefault(x => x.GetDescription() == key);
            this._currentBeatmapDifficulty = beatmapKey.difficulty;
            //this._gameplayCoreSceneSetupData.difficultyBeatmap.difficulty;
            this._levelID = beatmapLevel.levelID;

            var previewBeatmap = Loader.GetLevelById(beatmapLevel.levelID);
            this._currentCustomBeatmapLevel = previewBeatmap;
            if (this._currentCustomBeatmapLevel != null) {
                await this.SetStarInfo(this._levelID);
            }
            _ = CoroutineManager.Instance.StartCoroutine(this.SongStartWait());
        }

        private async Task SetStarInfo(string levelID)
        {
            var multiplier = this._gameStatus.modifierMultiplier;
            this._scoreSaberRowPP = this._ssData.GetPP(this._songID);
            this._isBeatLeaderRank = this._beatLeaderData.IsRanked(this._songID);
            this._isAccSaberRank = this._accSaberData.IsRanked(this._songID);
            this.SetCustomLabel(this._currentCustomBeatmapLevel, this._currentBeatmapDifficulty, this._currentBeatmapCharacteristics);
            this._currentStarSong = this._songDataUtil.GetBeatStarSong(this._currentCustomBeatmapLevel);
            this._currentStarSongDiff = this._songDataUtil.GetBeatStarSongDiffculityStats(this._currentCustomBeatmapLevel, this._currentBeatmapDifficulty, this._currentBeatmapCharacteristics);
            if (this._statusManager.StatusJSON["beatmap"] == null) {
                this._statusManager.StatusJSON["beatmap"] = new JSONObject();
            }
            var beatmapJson = this._statusManager.StatusJSON["beatmap"].AsObject;

            if (this._currentStarSong != null && this._currentStarSongDiff != null) {
                while (this._PPDownloader?.Init != true) {
                    await Task.Delay(1);
                }
                beatmapJson["pp"] = new JSONNumber(this._scoreSaberCalculator.CalculatePP(this._scoreSaberRowPP, 0.95f));
                beatmapJson["bl_pp"] = new JSONNumber(this._beatLeaderCalculator.CalculatePP(this._songID, 0.95f));
                beatmapJson["acc_saber_ap"] = new JSONNumber(this._accSaberCalculator.CalculateAP(this._songID, 0.95f));
                beatmapJson["star"] = new JSONNumber(this._currentStarSongDiff.Star);
                beatmapJson["downloadCount"] = new JSONNumber(this._currentStarSong.DownloadCount);
                beatmapJson["upVotes"] = new JSONNumber(this._currentStarSong.Upvotes);
                beatmapJson["downVotes"] = new JSONNumber(this._currentStarSong.Downvotes);
                beatmapJson["rating"] = new JSONNumber(this._currentStarSong.Rating);
            }
        }

        private void RelativeScoreAndImmediateRankCounter_relativeScoreOrImmediateRankDidChangeEvent()
        {
            this.SendPP();
        }

        private void SetCustomLabel(BeatmapLevel beatmap, BeatmapDifficulty diff, BeatDataCharacteristics beatDataCharacteristics)
        {
            if (beatmap == null) {
                return;
            }
            var songData = Collections.GetCustomLevelSongData(beatmap.levelID);
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

            if (songStart) {
                this._statusManager.EmitStatusUpdate(ChangedProperty.AllButNoteCut, BeatSaberEvent.SongStart);
            }
            else {
                this._statusManager.EmitStatusUpdate(ChangedProperty.Beatmap, BeatSaberEvent.Resume);
            }
        }

        private void OnGameEnergyCounter_gameEnergyDidReach0Event()
        {
            this._failed = true;
            if (this._gameEnergyCounter != null) {
                this._gameEnergyCounter.gameEnergyDidReach0Event -= this.OnGameEnergyCounter_gameEnergyDidReach0Event;
            }
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // メンバ変数
        private IGamePause _gamePause;
        private IGameStatus _gameStatus;
        private bool _disposedValue;
        private IStatusManager _statusManager;
        private RelativeScoreAndImmediateRankCounter _relativeScoreAndImmediateRankCounter;
        private IAudioTimeSource _audioTimeSource;
        private GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;
        private ISongDataUtil _songDataUtil;
        private BeatmapLevel _currentCustomBeatmapLevel;
        private BeatmapDifficulty _currentBeatmapDifficulty;
        private BeatDataCharacteristics _currentBeatmapCharacteristics;
        private BeatSongData _currentStarSong;
        private BeatSongDataDifficultyStats _currentStarSongDiff;
        private float _scoreSaberRowPP;
        private bool _isBeatLeaderRank;
        private bool _isAccSaberRank;
        private string _levelID;
        private bool _failed = false;
        private ScoreSaberCalculator _scoreSaberCalculator;
        private SSData _ssData;
        private BeatLeaderData _beatLeaderData;
        private AccSaberData _accSaberData;
        private BeatLeaderCalculator _beatLeaderCalculator;
        private AccSaberCalculator _accSaberCalculator;
        private PPDownloader _PPDownloader;
        private SongID _songID;
        private IGameEnergyCounter _gameEnergyCounter;
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // 構築・破棄
        [Inject]
        protected void Constractor(
            IStatusManager statusManager,
            IGameStatus gameStatus,
            RelativeScoreAndImmediateRankCounter relativeScoreAndImmediateRankCounter,
            IAudioTimeSource audioTimeSource,
            GameplayCoreSceneSetupData gameplayCoreSceneSetupData,
            ISongDataUtil songDataUtil,
            IGamePause gamePause,
            PPDownloader pPDownloader,
            ScoreSaberCalculator scoreSaberCalculator,
            BeatLeaderCalculator beatLeaderCalculator,
            AccSaberCalculator accSaberCalculator,
            SSData ssData,
            BeatLeaderData beatLeaderData,
            AccSaberData accSaberData,
            IGameEnergyCounter gameEnergyCounter)
        {
            this._statusManager = statusManager;
            this._relativeScoreAndImmediateRankCounter = relativeScoreAndImmediateRankCounter;
            this._audioTimeSource = audioTimeSource;
            this._gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
            this._songDataUtil = songDataUtil;
            this._gamePause = gamePause;
            this._gameStatus = gameStatus;
            this._scoreSaberCalculator = scoreSaberCalculator;
            this._beatLeaderCalculator = beatLeaderCalculator;
            this._accSaberCalculator = accSaberCalculator;
            this._PPDownloader = pPDownloader;
            this._ssData = ssData;
            this._beatLeaderData = beatLeaderData;
            this._accSaberData = accSaberData;
            this._gameEnergyCounter = gameEnergyCounter;
            this._gameEnergyCounter.gameEnergyDidReach0Event += this.OnGameEnergyCounter_gameEnergyDidReach0Event;
            var level = gameplayCoreSceneSetupData.beatmapLevel;
            var key = gameplayCoreSceneSetupData.beatmapKey;
            var id = SongDataUtils.GetHash(level.levelID);
            this._songID = new SongID(id, key.difficulty);
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
