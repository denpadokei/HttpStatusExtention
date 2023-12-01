using System.Collections.Generic;
using Zenject;

namespace HttpStatusExtention.PPCounters
{
    public class AccSaberCalculator
    {
        [Inject] private readonly AccSaberData accSaberData;

        private List<Point> _curve;
        private float[] _slopes;

        private float _scale;
        private float _shift;

        public void SetCurve(AccSaber accSaber)
        {
            this._curve = accSaber.curve;
            this._scale = accSaber.scale;
            this._shift = accSaber.shift;

            this._slopes = CurveUtils.GetSlopes(this._curve);
        }

        public bool IsRanked(SongID songID)
        {
            return this.accSaberData.IsRanked(songID);
        }

        public float CalculateAP(SongID songID, float accuracy)
        {
            var complexity = this.accSaberData.GetComplexity(songID);
            return this.CalculateAP(complexity, accuracy);
        }

        public float CalculateAP(float complexity, float accuracy)
        {
            return CurveUtils.GetCurveMultiplier(this._curve, this._slopes, accuracy) * (complexity - this._shift) * this._scale;
        }
    }
}
