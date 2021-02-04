using BeatSaberHTTPStatus;
using BeatSaberHTTPStatus.Interfaces;
using HttpStatusExtention.DataBases;
using HttpStatusExtention.PPCounters;
using HttpStatusExtention.SongDataCores;
using SimpleJSON;
using SongCore;
using SongDataCore.BeatStar;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace HttpStatusExtention
{
    public class HttpStatusExtentionController : IInitializable, IDisposable
    {
        private bool disposedValue;
        [Inject]
        IStatusManager statusManager;
        [Inject]
        RelativeScoreAndImmediateRankCounter relativeScoreAndImmediateRankCounter;
        GameplayCoreSceneSetupData CurrentData => BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData;
        CustomPreviewBeatmapLevel _currentCustomBeatmapLevel;
        BeatmapDifficulty _currentBeatmapDifficulty;
        BeatStarSong _currentStarSong;
        BeatStarSongDifficultyStats _currentStarSongDiff;

        double songRawPP;
        
        public void Initialize()
        {
            this.Setup();
        }

        private void SendPP()
        {
            this.SendPP(this.relativeScoreAndImmediateRankCounter.relativeScore);
        }

        private void SendPP(float relativeScore)
        {
            if (this.statusManager.StatusJSON["performance"] == null) {
                this.statusManager.StatusJSON["performance"] = new JSONObject();
            }
            var jsonObject = this.statusManager.StatusJSON["performance"].AsObject;
            jsonObject["current_pp"] = new JSONNumber(PPCounterUtil.CalculatePP(this.songRawPP, relativeScore));
            this.statusManager.EmitStatusUpdate(ChangedProperty.Performance, BeatSaberEvent.ScoreChanged);
        }

        private void Setup()
        {
            Plugin.Log.Debug($"Setup start.");
            if (ScoreDataBase.Instance.Init) {
                this.relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent += this.RelativeScoreAndImmediateRankCounter_relativeScoreOrImmediateRankDidChangeEvent;
            }
            var beatmapLevel = CurrentData.difficultyBeatmap.level;
            this._currentBeatmapDifficulty = CurrentData.difficultyBeatmap.difficulty;
            var levelID = beatmapLevel.levelID;

            this._currentCustomBeatmapLevel = Loader.GetLevelByHash(levelID.Split('_').Last());
            this.songRawPP = ScoreDataBase.Instance.Init ? PPCounterUtil.GetPP(_currentCustomBeatmapLevel, _currentBeatmapDifficulty) : 0;
            if (_currentCustomBeatmapLevel != null) {
                
                HMMainThreadDispatcher.instance.Enqueue(this.SetCustomLabel(_currentCustomBeatmapLevel, _currentBeatmapDifficulty));
            }
            this._currentStarSong = SongDataCoreUtil.GetBeatStarSong(_currentCustomBeatmapLevel);
            this._currentStarSongDiff = SongDataCoreUtil.GetBeatStarSongDiffculityStats(_currentCustomBeatmapLevel, _currentBeatmapDifficulty);
            
            if (this._currentStarSong != null && this._currentStarSongDiff != null) {
                if (this.statusManager.StatusJSON["beatmap"] == null) {
                    this.statusManager.StatusJSON["beatmap"] = new JSONObject();
                }
                var beatmapJson = this.statusManager.StatusJSON["beatmap"].AsObject;
                var multiplier = this.statusManager.GameStatus.songSpeedMultiplier;
                if (ScoreDataBase.Instance.Init) {
                    if (multiplier == 1 || !PPCounterUtil.AllowedPositiveModifiers(levelID)) {
                        beatmapJson["pp"] = new JSONNumber(this.songRawPP * 1.12);
                    }
                    else {
                        beatmapJson["pp"] = new JSONNumber(PPCounterUtil.CalculatePP(this.songRawPP, multiplier));
                    }
                }
                beatmapJson["star"] = new JSONNumber(this._currentStarSongDiff.star);
                beatmapJson["downloadCount"] = new JSONNumber(this._currentStarSong.downloadCount);
                beatmapJson["upVotes"] = new JSONNumber(this._currentStarSong.upVotes);
                beatmapJson["downVotes"] = new JSONNumber(this._currentStarSong.downVotes);
                beatmapJson["rating"] = new JSONNumber(this._currentStarSong.rating);

                this.statusManager.EmitStatusUpdate(ChangedProperty.AllButNoteCut, BeatSaberEvent.SongStart);
            }
        }

        private void RelativeScoreAndImmediateRankCounter_relativeScoreOrImmediateRankDidChangeEvent()
        {
            this.SendPP();
        }

        private IEnumerator SetCustomLabel(CustomPreviewBeatmapLevel beatmap, BeatmapDifficulty diff)
        {
            if (beatmap == null) {
                yield break;
            }
            var songData = Collections.RetrieveExtraSongData(SongCore.Utilities.Hashing.GetCustomLevelHash(beatmap));
            var diffData = songData._difficulties?.FirstOrDefault(x => x._difficulty == diff);
            var currentDiffLabel = diffData._difficultyLabel;
            if (string.IsNullOrEmpty(currentDiffLabel)) {
                yield break;
            }
            yield return new WaitWhile(() => string.IsNullOrEmpty(this.statusManager.GameStatus.difficulty));
            this.statusManager.GameStatus.difficulty += $"({currentDiffLabel})";
            this.statusManager.EmitStatusUpdate(ChangedProperty.AllButNoteCut, BeatSaberEvent.SongStart);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    Plugin.Log.Debug($"Dispose call");
                    this.relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent -= this.RelativeScoreAndImmediateRankCounter_relativeScoreOrImmediateRankDidChangeEvent;
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~HttpStatusExtentionController()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
