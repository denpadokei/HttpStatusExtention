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
        /// 計算に必要なカーブが1種類でも揃っているか。
        /// 欠けている種別は各 Calculator 側で個別にスキップする。
        /// </summary>
        public static bool IsValidCurves(Leaderboards curves)
        {
            return curves != null
                && (HasValidScoreSaber(curves.ScoreSaber)
                    || HasValidBeatLeader(curves.BeatLeader)
                    || HasValidAccSaber(curves.AccSaber));
        }

        /// <summary>
        /// キャッシュファイルを部分データで上書きしないための判定。
        /// </summary>
        public static bool IsCompleteCurves(Leaderboards curves)
        {
            return curves != null
                && HasValidScoreSaber(curves.ScoreSaber)
                && HasValidBeatLeader(curves.BeatLeader)
                && HasValidAccSaber(curves.AccSaber);
        }

        private static bool HasValidScoreSaber(ScoreSaber scoreSaber)
        {
            return scoreSaber?.standardCurve?.Count > 1
                && scoreSaber.modifierCurve?.Count > 1
                && scoreSaber.modifiers != null;
        }

        private static bool HasValidBeatLeader(BeatLeader beatLeader)
        {
            return beatLeader?.accCurve?.Count > 1;
        }

        private static bool HasValidAccSaber(AccSaber accSaber)
        {
            return accSaber?.curve?.Count > 1;
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プライベートメソッド
        private async Task GetRawSSPP(CancellationToken token)
        {
            try {
                var uri = URI_PREFIX + PP_FILE_NAME;
                var result = await this.MakeWebRequest<Dictionary<string, RawPPData>>(uri, token);
                if (result == null || result.Count == 0) {
                    Plugin.Log.Warn($"Failed to get ScoreSaber PP data. ({uri})");
                    return;
                }
                this.RowPPs = new ReadOnlyDictionary<string, RawPPData>(result);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) {
                throw;
            }
            catch (Exception e) {
                // 1本の失敗で他のダウンロード結果を捨てない。
                Plugin.Log.Warn($"Failed to get ScoreSaber PP data: {e.Message}");
            }
        }

        private async Task GetAccSaberRankedMaps(CancellationToken token)
        {
            try {
                var uri = ACCSABER_URL + ACCSABER_RANKED_MAPS;
                var result = await this.MakeWebRequest<List<AccSaberRankedMap>>(uri, token);
                if (result == null) {
                    Plugin.Log.Warn($"Failed to get AccSaber ranked maps. ({uri})");
                    return;
                }
                this.AccSaberData = result;
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) {
                throw;
            }
            catch (Exception e) {
                Plugin.Log.Warn($"Failed to get AccSaber ranked maps: {e.Message}");
            }
        }

        private async Task GetCurves(CancellationToken token)
        {
            try {
                var uri = URI_PREFIX + CURVE_FILE_NAME;
                var result = await this.MakeWebRequest<Leaderboards>(uri, token);
                if (!IsValidCurves(result)) {
                    Plugin.Log.Warn($"Failed to get PP curve data. ({uri})");
                    return;
                }
                this.Curves = result;
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) {
                throw;
            }
            catch (Exception e) {
                Plugin.Log.Warn($"Failed to get PP curve data: {e.Message}");
            }
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
