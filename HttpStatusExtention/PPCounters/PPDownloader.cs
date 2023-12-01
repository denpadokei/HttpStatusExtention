using SiraUtil.Zenject;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace HttpStatusExtention.PPCounters
{
    public class PPDownloader : IAsyncInitializable
    {
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プロパティ
        public ReadOnlyDictionary<string, RawPPData> RowPPs { get; private set; }
        public List<AccSaberRankedMap> AccSaberData { get; private set; }
        public Leaderboards Curves { get; private set; }
        public bool Init { get; private set; }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // メンバ変数
        private const string URI_PREFIX = "https://cdn.pulselane.dev/";
        private const string ACCSABER_URL = "https://api.accsaber.com/";
        private const string PP_FILE_NAME = "raw_pp.json";
        private const string CURVE_FILE_NAME = "curves.json";
        private const string ACCSABER_RANKED_MAPS = "ranked-maps";
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // 構築・破棄
        public async Task InitializeAsync(CancellationToken token)
        {
            try {
                this.Init = false;
                var tasks = new List<Task>
                {
                    this.StartDownloadingCurves(token),
                    this.StartDownloadingSS(token),
                    this.StartDownloadingAccSaber(token)
                };
                await Task.WhenAll(tasks.ToArray());
                this.Init = true;
            }
            catch (Exception) {
            }
        }

        public Task StartDownloadingCurves(CancellationToken token)
        {
            return this.GetCurves(token);
        }

        public Task StartDownloadingSS(CancellationToken token)
        {
            return this.GetRawSSPP(token);
        }

        public Task StartDownloadingAccSaber(CancellationToken token)
        {
            return this.GetAccSaberRankedMaps(token);
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プライベートメソッド
        private async Task GetRawSSPP(CancellationToken token)
        {
            var uri = URI_PREFIX + PP_FILE_NAME;
            var result = await this.MakeWebRequest<Dictionary<string, RawPPData>>(uri, token);
            this.RowPPs = new ReadOnlyDictionary<string, RawPPData>(result);
        }

        private async Task GetAccSaberRankedMaps(CancellationToken token)
        {
            var uri = ACCSABER_URL + ACCSABER_RANKED_MAPS;
            var result = await this.MakeWebRequest<List<AccSaberRankedMap>>(uri, token);
            this.AccSaberData = result;
        }

        private async Task GetCurves(CancellationToken token)
        {
            var uri = URI_PREFIX + CURVE_FILE_NAME;
            var result = await this.MakeWebRequest<Leaderboards>(uri, token);
            this.Curves = result;
        }

        private async Task<T> MakeWebRequest<T>(string uri, CancellationToken token)
        {
            var result = await WebClient.GetAsync(uri, token);
            if (result == null || !result.IsSuccessStatusCode) {
                return default;
            }
            var jsonToken = result.ConvertToJToken();
            return jsonToken.ToObject<T>();
        }
        #endregion
    }
}
