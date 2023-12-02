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

        public async Task InitializeAsync(CancellationToken token)
        {
            this.LoadCurveFile();
            while (this._ppDownloader?.Init != true) {
                await Task.Delay(1);
            }
            lock (this.Curves) {
                this.Curves = this._ppDownloader.Curves;
                this.CurveInit = true;
                this.WriteCurveFile();
            }
        }

        private void LoadCurveFile()
        {
            if (File.Exists(CURVE_FILE_NAME)) {
                try {

                    lock (this.Curves) {
                        if (!this.CurveInit) {
                            var jsonString = File.ReadAllText(CURVE_FILE_NAME);
                            this.Curves = JsonConvert.DeserializeObject<Leaderboards>(jsonString);
                            this.CurveInit = true;
                        }
                    }
                }
                catch (Exception) {

                }
            }
        }

        private void WriteCurveFile()
        {
            OSUtils.WriteFile(this.Curves, CURVE_FILE_NAME);
        }
    }
}
