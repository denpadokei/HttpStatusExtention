using System.Collections.Generic;
using System.Linq;

namespace HttpStatusExtention.PPCounters
{
    public class PPCounterUtil
    {
        static PPCounterUtil()
        {
            s_oldSlopes = new float[s_oldPPCurve.Length - 1];
            for (var i = 0; i < s_oldPPCurve.Length - 1; i++) {
                var x1 = s_oldPPCurve[i].Item1;
                var y1 = s_oldPPCurve[i].Item2;
                var x2 = s_oldPPCurve[i + 1].Item1;
                var y2 = s_oldPPCurve[i + 1].Item2;

                var m = (y2 - y1) / (x2 - x1);
                s_oldSlopes[i] = m;
            }

            s_slopes = new float[s_ppCurve.Length - 1];
            for (var i = 0; i < s_ppCurve.Length - 1; i++) {
                var x1 = s_ppCurve[i].Item1;
                var y1 = s_ppCurve[i].Item2;
                var x2 = s_ppCurve[i + 1].Item1;
                var y2 = s_ppCurve[i + 1].Item2;

                var m = (y2 - y1) / (x2 - x1);
                s_slopes[i] = m;
            }
        }
        private static readonly float[] s_oldSlopes;
        private static readonly float[] s_slopes;

        /// <summary>
        /// オーバーキル用
        /// </summary>
        private static readonly (float, float)[] s_oldPPCurve = new (float, float)[]
        {
            (0f, 0),
            (.45f, .015f),
            (.50f, .03f),
            (.55f, .06f),
            (.60f, .105f),
            (.65f, .16f),
            (.68f, .24f),
            (.70f, .285f),
            (.80f, .563f),
            (.84f, .695f),
            (.88f, .826f),
            (.945f, 1.015f),
            (.95f, 1.046f),
            (1.00f, 1.12f),
            (1.10f, 1.18f),
            (1.14f, 1.25f)
        };

        /// <summary>
        /// 左がスコアのパーセンテージ、右がPP補正値
        /// </summary>
        private static readonly (float, float)[] s_ppCurve = new (float, float)[]
        {
            (0f, 0),
            (.6f, .25f),
            (.65f, .29f),
            (.7f, .34f),
            (.75f, .40f),
            (.8f, .47f),
            (.825f, .51f),
            (.85f, .57f),
            (.875f, .655f),
            (.9f, .75f),
            (.91f, .79f),
            (.92f, .835f),
            (.93f, 0.885f),
            (.94f, 0.94f),
            (.95f, 1f),
            (.955f, 1.045f),
            (.96f, 1.11f),
            (.965f, 1.20f),
            (.97f, 1.31f),
            (.9725f, 1.37f),
            (.975f, 1.45f),
            (.9775f, 1.57f),
            (.98f, 1.71f),
            (.9825f, 1.88f),
            (.985f, 2.1f),
            (.9875f, 2.38f),
            (.99f, 2.73f),
            (.9925f, 3.17f),
            (.995f, 3.76f),
            (.9975f, 4.7f),
            (.999f, 5.8f),
            (1, 7f)
        };
        private static readonly HashSet<string> s_songsAllowingPositiveModifiers = new HashSet<string> {
            "2FDDB136BDA7F9E29B4CB6621D6D8E0F8A43B126", // Overkill Nuketime
            "27FCBAB3FB731B16EABA14A5D039EEFFD7BD44C9" // Overkill Kry
        };
        public static bool AllowedPositiveModifiers(string levelID)
        {
            var labels = levelID.Split('_');
            if (labels.Length != 3) {
                return true;
            }
            return s_songsAllowingPositiveModifiers.Contains(labels.ElementAt(2).Substring(0, 40).ToUpper());
        }

        public static float CalculatePP(float rawPP, float accuracy, bool oldCurve)
        {
            return rawPP * PPPercentage(accuracy, oldCurve);
        }

        private static float PPPercentage(float accuracy, bool oldCurve)
        {
            var max = oldCurve ? 1.14f : 1f;
            var maxReward = oldCurve ? 1.25f : 7f;

            if (accuracy >= max) {
                return maxReward;
            }

            if (accuracy <= 0) {
                return 0;
            }

            var i = -1;
            if (oldCurve) {
                foreach ((var score, var given) in s_oldPPCurve) {
                    if (score > accuracy) {
                        break;
                    }

                    i++;
                }
            }
            else {
                foreach ((var score, var given) in s_ppCurve) {
                    if (score > accuracy) {
                        break;
                    }

                    i++;
                }
            }
            if (!oldCurve) {
                var lowerScore = s_ppCurve[i].Item1;
                var higherScore = s_ppCurve[i + 1].Item1;
                var lowerGiven = s_ppCurve[i].Item2;
                var higherGiven = s_ppCurve[i + 1].Item2;
                return Lerp(lowerScore, lowerGiven, higherScore, higherGiven, accuracy, i, oldCurve);
            }
            else {
                var lowerScore = s_oldPPCurve[i].Item1;
                var higherScore = s_oldPPCurve[i + 1].Item1;
                var lowerGiven = s_oldPPCurve[i].Item2;
                var higherGiven = s_oldPPCurve[i + 1].Item2;
                return Lerp(lowerScore, lowerGiven, higherScore, higherGiven, accuracy, i, oldCurve);
            }
        }

        private static float Lerp(float x1, float y1, float x2, float y2, float x3, int i, bool oldCurve)
        {
            float m;
            if (!oldCurve && s_slopes != null) {
                m = s_slopes[i];
            }
            else if (!oldCurve && s_oldSlopes != null) {
                m = s_oldSlopes[i];
            }
            else {
                m = (y2 - y1) / (x2 - x1);
            }
            return m * (x3 - x1) + y1;
        }
    }
}
