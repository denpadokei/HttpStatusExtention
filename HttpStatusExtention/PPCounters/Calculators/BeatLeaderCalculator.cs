using SiraUtil.Zenject;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace HttpStatusExtention.PPCounters
{
    public class BeatLeaderCalculator : IAsyncInitializable
    {
        [Inject] private readonly BeatLeaderData beatLeaderData;

        private List<Point> _accCurve;
        private float[] _accSlopes;
        private float _accMultiplier;

        private float _passExponential;
        private float _passMultiplier;
        private float _passShift;

        private float _techExponentialMultiplier;
        private float _techMultiplier;

        private float _inflateExponential;
        private float _inflateMultiplier;

        private float _modifierMultiplier;
        private ModifiersMap _modifiersMap;
        private float _powerBottom;
        private BeatLeaderRating _rating;
        private float _passPP;

        private readonly IDifficultyBeatmap difficultyBeatmap;
        private readonly GameplayModifiers gameplayModifiers;
        private PPData _pPData;
        [Inject]
        public BeatLeaderCalculator(PPData pPData, IDifficultyBeatmap difficultyBeatmap, RelativeScoreAndImmediateRankCounter relativeScoreAndImmediateRankCounter, GameplayModifiers gameplayModifiers)
        {
            this.difficultyBeatmap = difficultyBeatmap;
            this.gameplayModifiers = gameplayModifiers;
            this._pPData = pPData;
        }

        public void SetCurve(BeatLeader beatLeader, SongID songID, GameplayModifiers modifiers)
        {
            this._accCurve = beatLeader.accCurve;
            this._accMultiplier = beatLeader.accMultiplier;

            this._passExponential = beatLeader.passExponential;
            this._passMultiplier = beatLeader.passMultiplier;
            this._passShift = beatLeader.passShift;

            this._techExponentialMultiplier = beatLeader.techExponentialMultiplier;
            this._techMultiplier = beatLeader.techMultiplier;

            this._inflateExponential = beatLeader.inflateExponential;
            this._inflateMultiplier = beatLeader.inflateMultiplier;

            this._modifiersMap = this.beatLeaderData.GetModifiersMap(songID);

            this.CalculateModifiersMultiplier(songID, modifiers);

            this._powerBottom = 0;

            this._rating = this.beatLeaderData.GetStars(songID);
            this._passPP = this.GetPassPP(this._rating.passRating * this._modifierMultiplier);

            this._accSlopes = CurveUtils.GetSlopes(this._accCurve);
        }

        public bool IsRanked(SongID songID)
        {
            return this.beatLeaderData.IsRanked(songID);
        }

        // hopefully this doesn't take too long to run...
        public float CalculatePP(SongID songID, float accuracy, bool failed = false)
        {
            var multiplier = this._modifierMultiplier + (failed ? this._modifiersMap.nf : 0);

            var passPP = this._passPP;

            // TODO: don't calculate this every time
            if (failed) {
                passPP = this.GetPassPP(this._rating.passRating * multiplier);
            }

            var accPP = this.GetAccPP(this._rating.accRating * multiplier, accuracy);
            var techPP = this.GetTechPP(this._rating.techRating * multiplier, accuracy);

            var rawPP = this.Inflate(passPP + accPP + techPP);

            if (float.IsInfinity(rawPP) || float.IsNaN(rawPP) || float.IsNegativeInfinity(rawPP)) {
                rawPP = 0;
            }

            return rawPP;
        }

        private float Inflate(float pp)
        {
            if (Mathf.Approximately(this._powerBottom, 0)) {
                this._powerBottom = Mathf.Pow(this._inflateMultiplier, this._inflateExponential);
            }

            return this._inflateMultiplier * Mathf.Pow(pp, this._inflateExponential) / this._powerBottom;
        }

        private float GetPassPP(float passRating)
        {
            var passPP = (this._passMultiplier * Mathf.Exp(Mathf.Pow(passRating, this._passExponential))) + this._passShift;
            if (float.IsInfinity(passPP) || float.IsNaN(passPP) || float.IsNegativeInfinity(passPP) || passPP < 0) {
                passPP = 0;
            }

            return passPP;
        }

        private float GetAccPP(float accRating, float accuracy)
        {
            return CurveUtils.GetCurveMultiplier(this._accCurve, this._accSlopes, accuracy) * accRating * this._accMultiplier;
        }

        private float GetTechPP(float techRating, float accuracy)
        {
            return (float)Math.Exp(this._techExponentialMultiplier * accuracy) * this._techMultiplier * techRating;
        }

        private void CalculateModifiersMultiplier(SongID songID, GameplayModifiers modifiers)
        {
            this._modifierMultiplier = 1;

            if (modifiers.disappearingArrows) {
                this._modifierMultiplier += this._modifiersMap.da;
            }
            if (modifiers.songSpeed.Equals(GameplayModifiers.SongSpeed.Faster)) {
                this._modifierMultiplier += this._modifiersMap.fs;
            }
            else if (modifiers.songSpeed.Equals(GameplayModifiers.SongSpeed.Slower)) {
                this._modifierMultiplier += this._modifiersMap.ss;
            }
            else if (modifiers.songSpeed.Equals(GameplayModifiers.SongSpeed.SuperFast)) {
                this._modifierMultiplier += this._modifiersMap.sf;
            }
            if (modifiers.ghostNotes) {
                this._modifierMultiplier += this._modifiersMap.gn;
            }
            if (modifiers.noArrows) {
                this._modifierMultiplier += this._modifiersMap.na;
            }
            if (modifiers.noBombs) {
                this._modifierMultiplier += this._modifiersMap.nb;
            }
            if (modifiers.enabledObstacleType.Equals(GameplayModifiers.EnabledObstacleType.NoObstacles)) {
                this._modifierMultiplier += this._modifiersMap.no;
            }
            if (modifiers.proMode) {
                this._modifierMultiplier += this._modifiersMap.pm;
            }
            if (modifiers.smallCubes) {
                this._modifierMultiplier += this._modifiersMap.sc;
            }
        }

        public async Task InitializeAsync(CancellationToken token)
        {
            while (this._pPData?.CurveInit != true) {
                await Task.Delay(1);
            }
            var id = SongDataUtils.GetHash(this.difficultyBeatmap.level.levelID);
            this.SetCurve(this._pPData.Curves.BeatLeader, new SongID(id, this.difficultyBeatmap.difficulty), this.gameplayModifiers);
        }
    }
}
