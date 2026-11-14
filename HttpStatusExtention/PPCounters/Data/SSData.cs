using Newtonsoft.Json;
using SiraUtil.Zenject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Zenject;

namespace HttpStatusExtention.PPCounters
{
    public class SSData : IAsyncInitializable
    {
        public bool DataInit { get; private set; } = false;
        [Inject] private readonly PPDownloader _ppDownloader;
        private IReadOnlyDictionary<string, RawPPData> _songData = new Dictionary<string, RawPPData>();

        private static readonly string SS_PP_FILE_NAME = Path.Combine(Environment.CurrentDirectory, "UserData", "HttpStatusExtention", "pp.json");
        private readonly object _sync = new object();

        public async Task InitializeAsync(CancellationToken token)
        {
            this.LoadPPFile();

            if (!await this._ppDownloader.WaitForInitializationAsync(token)) {
                return;
            }

            if (this._ppDownloader.HasRawPPData) {
                lock (this._sync) {
                    this._songData = this._ppDownloader.RowPPs;
                    this.DataInit = true;
                }
                this.WritePPFile();
            }
        }

        public float GetPP(SongID songID)
        {
            if (!this.DataInit || !this._songData.TryGetValue(songID.id, out var data)) {
                return 0f;
            }

            switch (songID.difficulty) {
                case BeatmapDifficulty.Easy:
                    return data._Easy_SoloStandard;
                case BeatmapDifficulty.Normal:
                    return data._Normal_SoloStandard;
                case BeatmapDifficulty.Hard:
                    return data._Hard_SoloStandard;
                case BeatmapDifficulty.Expert:
                    return data._Expert_SoloStandard;
                case BeatmapDifficulty.ExpertPlus:
                    return data._ExpertPlus_SoloStandard;
                default:
                    return 0;
            }
        }

        public bool IsRanked(SongID songID)
        {
            return this._songData.ContainsKey(songID.id) && this.GetPP(songID) > 0;
        }

        private void LoadPPFile()
        {
            if (File.Exists(SS_PP_FILE_NAME)) {
                try {
                    var json = JsonConvert.DeserializeObject<Dictionary<string, RawPPData>>(File.ReadAllText(SS_PP_FILE_NAME));
                    if (json != null && json.Count > 0) {
                        lock (this._sync) {
                            if (!this.DataInit) {
                                this._songData = json;
                                this.DataInit = true;
                            }
                        }
                    }
                }
                catch (Exception e) {
                    Plugin.Log.Warn($"Failed to load ScoreSaber PP cache: {e.Message}");
                }
            }
        }

        private void WritePPFile()
        {
            var songData = this._songData;
            _ = Task.Run(() =>
            {
                try {
                    OSUtils.WriteFile(songData, SS_PP_FILE_NAME);
                }
                catch (Exception e) {
                    Plugin.Log.Warn($"Failed to write ScoreSaber PP cache: {e.Message}");
                }
            });
        }
    }
}
