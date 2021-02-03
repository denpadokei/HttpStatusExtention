using BeatSaberHTTPStatus;
using BeatSaberHTTPStatus.Interfaces;
using SimpleJSON;
using SongCore;
using SongCore.HarmonyPatches;
using SongDataCore.BeatStar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using static SongCore.HarmonyPatches.StandardLevelDetailViewRefreshContent;

namespace HttpStatusExtention
{
    public class HttpStatusExtentionController : IInitializable, IDisposable
    {
        private bool disposedValue;
        [Inject]
        IStatusManager statusManager;
        GameplayCoreSceneSetupData CurrentData => BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData;
        Dictionary<string, BeatStarSong> SongDataCoreSongs => SongDataCore.Plugin.Songs.Data.Songs;
        BeatStarSong _currentStarSong;
        BeatStarSongDifficultyStats _currentStarSongDiff;
        public void Initialize()
        {
            HMMainThreadDispatcher.instance.Enqueue(this.Setup());
        }

        private IEnumerator Setup()
        {
            Plugin.Log.Debug($"Setup start.");
            var beatmapLevel = CurrentData.difficultyBeatmap.level;
            var diff = CurrentData.difficultyBeatmap.difficulty;
            var levelID = beatmapLevel.levelID;
            
            var beatmap = Loader.GetLevelByHash(levelID.Split('_').Last());
            
            if (beatmap != null) {
                
                var songData = Collections.RetrieveExtraSongData(SongCore.Utilities.Hashing.GetCustomLevelHash(beatmap));
                if (this.SongDataCoreSongs.TryGetValue(levelID.Split('_').Last(), out this._currentStarSong)) {
                    var diffData = songData._difficulties?.FirstOrDefault(x => x._difficulty == diff);
                    var currentDiffLabel = diffData._difficultyLabel;
                    if (!string.IsNullOrEmpty(currentDiffLabel)) {
                        yield return new WaitWhile(() => string.IsNullOrEmpty(this.statusManager.GameStatus.difficulty));
                        this.statusManager.GameStatus.difficulty += $"({currentDiffLabel})";
                    }
                    this._currentStarSongDiff = this._currentStarSong.diffs.FirstOrDefault(x => x.diff.Replace("+", "Plus").ToLower() == diff.ToString().ToLower());
                    if (this.statusManager.StatusJSON["beatmap"] == null) {
                        this.statusManager.StatusJSON["beatmap"] = new JSONObject();
                    }
                    var beatmapJson = this.statusManager.StatusJSON["beatmap"].AsObject;
                    beatmapJson["pp"] = new JSONNumber(this._currentStarSongDiff.pp);
                    beatmapJson["star"] = new JSONNumber(this._currentStarSongDiff.star);
                    beatmapJson["downloadCount"] = new JSONNumber(this._currentStarSong.downloadCount);
                    beatmapJson["upVotes"] = new JSONNumber(this._currentStarSong.upVotes);
                    beatmapJson["downVotes"] = new JSONNumber(this._currentStarSong.downVotes);
                    beatmapJson["rating"] = new JSONNumber(this._currentStarSong.rating);
                    Plugin.Log.Debug($"{this.statusManager.StatusJSON}");
                    this.statusManager.EmitStatusUpdate(ChangedProperty.AllButNoteCut, BeatSaberEvent.SongStart);
                }
            }
            yield break;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    Plugin.Log.Debug($"Dispose call");
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
