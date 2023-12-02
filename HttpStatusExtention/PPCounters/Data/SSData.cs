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

        public async Task InitializeAsync(CancellationToken token)
        {
            this.LoadPPFile();
            while (this._ppDownloader?.Init != true) {
                await Task.Delay(1);
            }
            lock (this._songData) {
                this._songData = this._ppDownloader.RowPPs;
                this.DataInit = true;
                this.WritePPFile();
            }
        }

        public float GetPP(SongID songID)
        {
            if (!this.DataInit) {
                return 0f;
            }

            switch (songID.difficulty) {
                case BeatmapDifficulty.Easy:
                    return this._songData[songID.id]._Easy_SoloStandard;
                case BeatmapDifficulty.Normal:
                    return this._songData[songID.id]._Normal_SoloStandard;
                case BeatmapDifficulty.Hard:
                    return this._songData[songID.id]._Hard_SoloStandard;
                case BeatmapDifficulty.Expert:
                    return this._songData[songID.id]._Expert_SoloStandard;
                case BeatmapDifficulty.ExpertPlus:
                    return this._songData[songID.id]._ExpertPlus_SoloStandard;
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
                    lock (this._songData) {
                        if (!this.DataInit) {
                            var json = JsonConvert.DeserializeObject<Dictionary<string, RawPPData>>(File.ReadAllText(SS_PP_FILE_NAME));
                            this._songData = json;
                            this.DataInit = true;
                        }
                    }
                }

                catch (Exception) {
                }
            }
        }

        private void WritePPFile()
        {
            OSUtils.WriteFile(this._songData, SS_PP_FILE_NAME);
        }
    }
}
