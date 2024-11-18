using SiraUtil.Zenject;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zenject;

namespace HttpStatusExtention.PPCounters
{
    public class ScoreSaberCalculator : IAsyncInitializable
    {
        [Inject] private readonly SSData ssData;
        private readonly RelativeScoreAndImmediateRankCounter relativeScoreAndImmediateRank;
        //private readonly IDifficultyBeatmap difficultyBeatmap;
        private BeatmapLevel _level;
        private BeatmapKey _key;
        private readonly GameplayModifiers gameplayModifiers;
        private readonly PPData ppData;

        private HashSet<string> songsAllowingPositiveModifiers = new HashSet<string>();

        private List<Point> _curve;
        private float[] _slopes;
        private float _multiplier;

        [Inject]
        public ScoreSaberCalculator(GameplayCoreSceneSetupData gameplayCoreSceneSetupData, RelativeScoreAndImmediateRankCounter relativeScoreAndImmediateRankCounter, GameplayModifiers gameplayModifiers, PPData pPData)
        {
            _level = gameplayCoreSceneSetupData.beatmapLevel;
            _key = gameplayCoreSceneSetupData.beatmapKey;
            this.gameplayModifiers = gameplayModifiers;
            this.relativeScoreAndImmediateRank = relativeScoreAndImmediateRankCounter;
            this.ppData = pPData;
        }

        public async Task InitializeAsync(CancellationToken token)
        {
            if (this.ppData?.CurveInit != true) {
                await Task.Delay(1);
            }
            var id = SongDataUtils.GetHash(this._level.levelID);
            this.SetCurve(this.ppData.Curves.ScoreSaber, new SongID(id, this._key.difficulty), this.relativeScoreAndImmediateRank._gameplayModifiersModel, this.gameplayModifiers);
        }

        public void SetCurve(ScoreSaber scoreSaber, SongID songID, GameplayModifiersModelSO gameplayModifiersModelSO, GameplayModifiers gameplayModifiers)
        {
            var allowedPositiveModifiers = this.AllowedPositiveModifiers(songID);
            var updatedModifiers = allowedPositiveModifiers ? GameplayModifierUtils.RemoveSuperFast(gameplayModifiers) : GameplayModifierUtils.RemovePositiveModifiers(gameplayModifiers);

            this._multiplier = CalculateMultiplier(gameplayModifiersModelSO, updatedModifiers, scoreSaber.modifiers);

            this.songsAllowingPositiveModifiers = scoreSaber.songsAllowingPositiveModifiers.ToHashSet();

            this._curve = allowedPositiveModifiers ? scoreSaber.modifierCurve : scoreSaber.standardCurve;
            this._slopes = CurveUtils.GetSlopes(this._curve);
        }

        public bool IsRanked(SongID songID)
        {
            return this.ssData.IsRanked(songID);
        }

        public bool AllowedPositiveModifiers(SongID songID)
        {
            return this.AllowedPositiveModifiers(songID.id);
        }

        public bool AllowedPositiveModifiers(string songID)
        {
            return this.songsAllowingPositiveModifiers.Contains(songID);
        }

        public float CalculatePP(SongID songID, float accuracy, bool failed = false)
        {
            var rawPP = this.ssData.GetPP(songID);
            return this.CalculatePP(rawPP, accuracy, failed);
        }

        public float CalculatePP(float rawPP, float accuracy, bool failed = false)
        {
            var multiplier = this._multiplier;
            if (failed) {
                multiplier -= 0.5f;
            }

            return rawPP * CurveUtils.GetCurveMultiplier(this._curve, this._slopes, accuracy * multiplier);
        }

        public static float CalculateMultiplier(GameplayModifiersModelSO gameplayModifiersModelSO, GameplayModifiers gameplayModifiers, ScoreSaberModifiers modifierMultipliers)
        {
            var modifierParams = gameplayModifiersModelSO.CreateModifierParamsList(gameplayModifiers);
            var multiplier = gameplayModifiersModelSO.GetTotalMultiplier(modifierParams, 1f);

            // ScoreSaber weights these multipliers differently
            if (gameplayModifiers.disappearingArrows) {
                multiplier += modifierMultipliers.da - GameplayModifierUtils.DA_ORIGINAL;
            }

            if (gameplayModifiers.ghostNotes) {
                multiplier += modifierMultipliers.gn - GameplayModifierUtils.GN_ORIGINAL;
            }

            if (gameplayModifiers.songSpeed.Equals(GameplayModifiers.SongSpeed.Faster)) {
                multiplier += modifierMultipliers.fs - GameplayModifierUtils.FS_ORIGINAL;
            }

            return multiplier;
        }
    }
}
