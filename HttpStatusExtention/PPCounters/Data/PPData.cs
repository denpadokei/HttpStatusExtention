using HttpStatusExtention.Models;
using Newtonsoft.Json;
using SiraUtil.Zenject;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Zenject;

namespace HttpStatusExtention.PPCounters
{
    public class PPData : IAsyncInitializable
    {
        public bool DataInit { get; private set; } = false;
        public bool CurveInit { get; private set; } = false;
        [Inject] private readonly PPDownloader _ppDownloader;

        public Leaderboards Curves { get; private set; } = new Leaderboards();

        private static readonly string CURVE_FILE_NAME = Path.Combine(Environment.CurrentDirectory, "UserData", "HttpStatusExtention", "curves.json");
        private readonly TaskCompletionSource<bool> _ready = new TaskCompletionSource<bool>();

        public async Task InitializeAsync(CancellationToken token)
        {
            try {
                this.LoadCurveFile();

                // ダウンロードが終わるまで (または失敗が確定するまで) 待つ。
                // 失敗しても PPDownloader.Init は true になるので、ここで止まり続けることはない。
                if (!await this._ppDownloader.WaitForInitializationAsync(token)) {
                    return;
                }

                if (this._ppDownloader.HasValidCurves) {
                    lock (this.Curves) {
                        this.Curves = this._ppDownloader.Curves;
                        this.CurveInit = true;
                        this.WriteCurveFile();
                    }
                }
            }
            finally {
                _ = this._ready.TrySetResult(true);
            }
        }

        public Task<bool> WaitForReadyAsync(CancellationToken token)
        {
            return TaskUtils.WaitAsync(this._ready.Task, token);
        }

        private void LoadCurveFile()
        {
            if (File.Exists(CURVE_FILE_NAME)) {
                try {
                    var jsonString = File.ReadAllText(CURVE_FILE_NAME);
                    var curves = JsonConvert.DeserializeObject<Leaderboards>(jsonString);
                    if (PPDownloader.IsValidCurves(curves)) {
                        lock (this.Curves) {
                            if (!this.CurveInit) {
                                this.Curves = curves;
                                this.CurveInit = true;
                            }
                        }
                    }
                }
                catch (Exception e) {
                    Plugin.Log.Warn($"Failed to load curve cache: {e.Message}");
                }
            }
        }

        private void WriteCurveFile()
        {
            OSUtils.WriteFile(this.Curves, CURVE_FILE_NAME);
        }
    }
}
