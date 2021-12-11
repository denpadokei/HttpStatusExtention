using HttpStatusExtention.DataBases;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HttpStatusExtention.PPCounters
{
    public class PPCounterUtil
    {
        static PPCounterUtil()
        {
            slopes = new double[ppCurve.Length - 1];
            for (var i = 0; i < ppCurve.Length - 1; i++) {
                var x1 = ppCurve[i].Item1;
                var y1 = ppCurve[i].Item2;
                var x2 = ppCurve[i + 1].Item1;
                var y2 = ppCurve[i + 1].Item2;

                var m = (y2 - y1) / (x2 - x1);
                slopes[i] = m;
            }
        }
        private static readonly double[] slopes;

        /// <summary>
        /// 左がスコアのパーセンテージ、右がPP補正値
        /// </summary>
        private static readonly (double, double)[] ppCurve = new (double, double)[]
        {
            (0f, 0),
            (.45f, .015f),
            (.50f, .03f),
            (.55f, .06f),
            (.60f, .105f),
            (.65f, .15f),
            (.70f, .22f),
            (.75f, .35f),
            (.80f, .42f),
            (.86f, .6f),
            (.9f, .78f),
            (.925f, .905f),
            (.945f, 1.015f),
            (.95f, 1.046f),
            (.96f, 1.115f),
            (.97f, 1.2f),
            (.98f, 1.29f),
            (.99f, 1.39f),
            (1, 1.5f),
        };
        private static readonly HashSet<string> songsAllowingPositiveModifiers = new HashSet<string> {
            "2FDDB136BDA7F9E29B4CB6621D6D8E0F8A43B126", // Overkill Nuketime
            "27FCBAB3FB731B16EABA14A5D039EEFFD7BD44C9" // Overkill Kry
        };
        public static bool AllowedPositiveModifiers(string levelID)
        {
            var labels = levelID.Split('_');
            if (labels.Length != 3) {
                return true;
            }
            return songsAllowingPositiveModifiers.Contains(labels.ElementAt(2).ToUpper());
        }

        public static double GetPP(CustomPreviewBeatmapLevel beatmapLevel, BeatmapDifficulty difficulty)
        {
            if (beatmapLevel == null) {
                return 0;
            }
            return GetPP(beatmapLevel.levelID.Split('_').ElementAt(2), difficulty);
        }

        public static double GetPP(string hash, BeatmapDifficulty difficulty)
        {
            try {
                var song = ScoreDataBase.Songs[hash].AsObject;
                if (song == null) {
                    return 0;
                }
                switch (difficulty) {
                    case BeatmapDifficulty.Easy:
                        return song["_Easy_SoloStandard"].AsDouble;
                    case BeatmapDifficulty.Normal:
                        return song["_Normal_SoloStandard"].AsDouble;
                    case BeatmapDifficulty.Hard:
                        return song["_Hard_SoloStandard"].AsDouble;
                    case BeatmapDifficulty.Expert:
                        return song["_Expert_SoloStandard"].AsDouble;
                    case BeatmapDifficulty.ExpertPlus:
                        return song["_ExpertPlus_SoloStandard"].AsDouble;
                    default:
                        return 0;
                }
            }
            catch (Exception e) {
                Plugin.Log.Error(e);
                return 0;
            }
        }

        public static double CalculatePP(CustomPreviewBeatmapLevel beatmapLevel, BeatmapDifficulty difficulty, double accuracy)
        {
            var rawPP = GetPP(beatmapLevel, difficulty);
            return CalculatePP(rawPP, accuracy);
        }

        public static double CalculatePP(double rawPP, double accuracy) => rawPP * PPPercentage(accuracy);

        private static double PPPercentage(double accuracy)
        {
            if (accuracy >= 1)
                return 1.5;
            if (accuracy <= 0)
                return 0;

            var i = -1;
            foreach ((var score, var given) in ppCurve) {
                if (score > accuracy)
                    break;
                i++;
            }

            var lowerScore = ppCurve[i].Item1;
            var higherScore = ppCurve[i + 1].Item1;
            var lowerGiven = ppCurve[i].Item2;
            var higherGiven = ppCurve[i + 1].Item2;
            return Lerp(lowerScore, lowerGiven, higherScore, higherGiven, accuracy, i);
        }

        private static double Lerp(double x1, double y1, double x2, double y2, double x3, int i)
        {
            double m;
            if (slopes != null)
                m = slopes[i];
            else
                m = (y2 - y1) / (x2 - x1);
            return m * (x3 - x1) + y1;
        }
    }
}
