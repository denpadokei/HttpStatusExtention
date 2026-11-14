using SiraUtil.Zenject;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zenject;

namespace HttpStatusExtention.PPCounters
{
    public class AccSaberCalculator : IAsyncInitializable
    {
        [Inject] private readonly AccSaberData accSaberData;

        private List<Point> _curve;
        private float[] _slopes;

        private float _scale;
        private float _shift;
        private bool _ready;
        private readonly PPData _pPData;

        public AccSaberCalculator(PPData pPData)
        {
            this._pPData = pPData;
        }

        public void SetCurve(AccSaber accSaber)
        {
            if (accSaber?.curve == null || accSaber.curve.Count < 2) {
                return;
            }

            this._curve = accSaber.curve;
            this._scale = accSaber.scale;
            this._shift = accSaber.shift;

            this._slopes = CurveUtils.GetSlopes(this._curve);
            this._ready = this._slopes.Length > 0;
        }

        public bool IsRanked(SongID songID)
        {
            return this.accSaberData.IsRanked(songID);
        }

        public bool IsReady => this._ready;

        public float CalculateAP(SongID songID, float accuracy)
        {
            var complexity = this.accSaberData.GetComplexity(songID);
            return this.CalculateAP(complexity, accuracy);
        }

        public float CalculateAP(float complexity, float accuracy)
        {
            if (!this._ready) {
                return 0f;
            }
            return CurveUtils.GetCurveMultiplier(this._curve, this._slopes, accuracy) * (complexity - this._shift) * this._scale;
        }

        public async Task InitializeAsync(CancellationToken token)
        {
            if (!await this._pPData.WaitForReadyAsync(token)) {
                return;
            }
            if (!this._pPData.CurveInit || this._pPData.Curves?.AccSaber == null) {
                return;
            }
            this.SetCurve(this._pPData.Curves.AccSaber);
        }
    }
}
