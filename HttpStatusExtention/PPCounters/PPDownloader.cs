using HttpStatusExtention.Models;
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
        public ReadOnlyDictionary<string, RawPPData> RowPPs { get; private set; } = EmptyRowPPs;
        public List<AccSaberRankedMap> AccSaberData { get; private set; } = new List<AccSaberRankedMap>();
        public Leaderboards Curves { get; private set; } = new Leaderboards();
        public bool Init { get; private set; }
        public bool HasValidCurves => IsValidCurves(this.Curves);
        public bool HasRawPPData => this.RowPPs != null && this.RowPPs.Count > 0;
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // メンバ変数
        private const string URI_PREFIX = "https://cdn.pulselane.dev/";
        private const string ACCSABER_URL = "https://api.accsaber.com/";
        private const string PP_FILE_NAME = "raw_pp.json";
        private const string CURVE_FILE_NAME = "curves.json";
        private const string ACCSABER_RANKED_MAPS = "ranked-maps";

        private static readonly ReadOnlyDictionary<string, RawPPData> EmptyRowPPs = new ReadOnlyDictionary<string, RawPPData>(new Dictionary<string, RawPPData>());
        private readonly TaskCompletionSource<bool> _initialization = new TaskCompletionSource<bool>();
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
            }
            catch (OperationCanceledException) {
                Plugin.Log.Warn("PP data download was cancelled.");
            }
            catch (Exception e) {
                // ダウンロードに失敗しても Init は必ず true にする。
                // 失敗したまま Init が false だと、シーンごとに生成される各初期化処理が
                // 1ms 間隔のポーリングを永久に続けて CPU を食いつぶす。
                Plugin.Log.Warn($"PP data download failed: {e.Message}");
            }
            finally {
                this.Init = true;
                _ = this._initialization.TrySetResult(true);
            }
        }

        /// <summary>
        /// 初期化 (ダウンロードまたは失敗確定) を待つ。
        /// キャンセルされた場合は false を返す。
        /// </summary>
        public Task<bool> WaitForInitializationAsync(CancellationToken token)
        {
            return TaskUtils.WaitAsync(this._initialization.Task, token);
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

        /// <summary>
        /// 計算に必要なカーブがすべて揃っているか。
        /// </summary>
        public static bool IsValidCurves(Leaderboards curves)
        {
            return curves?.ScoreSaber?.standardCurve?.Count > 1
                && curves.ScoreSaber.modifierCurve?.Count > 1
                && curves.ScoreSaber.modifiers != null
                && curves.BeatLeader?.accCurve?.Count > 1
                && curves.AccSaber?.curve?.Count > 1;
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プライベートメソッド
        private async Task GetRawSSPP(CancellationToken token)
        {
            var uri = URI_PREFIX + PP_FILE_NAME;
            var result = await this.MakeWebRequest<Dictionary<string, RawPPData>>(uri, token);
            if (result == null || result.Count == 0) {
                Plugin.Log.Warn($"Failed to get ScoreSaber PP data. ({uri})");
                return;
            }
            this.RowPPs = new ReadOnlyDictionary<string, RawPPData>(result);
        }

        private async Task GetAccSaberRankedMaps(CancellationToken token)
        {
            var uri = ACCSABER_URL + ACCSABER_RANKED_MAPS;
            var result = await this.MakeWebRequest<List<AccSaberRankedMap>>(uri, token);
            if (result == null) {
                Plugin.Log.Warn($"Failed to get AccSaber ranked maps. ({uri})");
                return;
            }
            this.AccSaberData = result;
        }

        private async Task GetCurves(CancellationToken token)
        {
            var uri = URI_PREFIX + CURVE_FILE_NAME;
            var result = await this.MakeWebRequest<Leaderboards>(uri, token);
            if (!IsValidCurves(result)) {
                Plugin.Log.Warn($"Failed to get PP curve data. ({uri})");
                return;
            }
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
