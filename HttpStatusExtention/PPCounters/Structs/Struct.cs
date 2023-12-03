using System;
using System.Collections.Generic;
using System.Reflection;

namespace HttpStatusExtention.PPCounters
{
    public class SongID : IEquatable<SongID>
    {
        public string id;
        public BeatmapDifficulty difficulty;

        public SongID(string id, BeatmapDifficulty difficulty)
        {
            this.id = id;
            this.difficulty = difficulty;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as SongID);
        }

        public bool Equals(SongID other)
        {
            return !(other is null) &&
                   this.id == other.id &&
                   this.difficulty == other.difficulty;
        }

        public override int GetHashCode()
        {
            var hashCode = 2041928400;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.id);
            hashCode = hashCode * -1521134295 + this.difficulty.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(SongID left, SongID right)
        {
            return EqualityComparer<SongID>.Default.Equals(left, right);
        }

        public static bool operator !=(SongID left, SongID right)
        {
            return !(left == right);
        }
    }

    public class RawPPData
    {
        public float _Easy_SoloStandard { get; set; }
        public float _Normal_SoloStandard { get; set; }
        public float _Hard_SoloStandard { get; set; }
        public float _Expert_SoloStandard { get; set; }
        public float _ExpertPlus_SoloStandard { get; set; }
    }

    public class BeatLeaderCacheFileData
    {
        public List<BeatLeaderLeaderboardCacheEntry> Entries;
        public long LastCheckTime;
    }

    public struct BeatLeaderLeaderboardCacheEntry
    {
        public string LeaderboardId;
        public BeatLeaderSongInfo SongInfo;
        public BeatLeaderDiffInfo DifficultyInfo;
    }

    public struct BeatLeaderSongInfo
    {
        public string id;
        public string hash;
    }

    public struct ModifiersMap
    {
        public int modifierId;

        public float da;

        public float fs;

        public float ss;

        public float sf;

        public float gn;

        public float na;

        public float nb;

        public float nf;

        public float no;

        public float pm;

        public float sc;

        public float GetModifierValueByModifierServerName(string name)
        {
            return (float)(typeof(ModifiersMap).GetField(name.ToLower(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(this) ?? -1f);
        }

        public void LoadFromGameModifiersParams(IEnumerable<GameplayModifierParamsSO> modifiersParams)
        {
            foreach (var modifiersParam in modifiersParams) {
                var text = ParseModifierLocalizationKeyToServerName(modifiersParam.modifierNameLocalizationKey);
                typeof(ModifiersMap).GetField(text.ToLower(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValueDirect(__makeref(this), modifiersParam.multiplier);
            }
        }

        public static string ParseModifierLocalizationKeyToServerName(string modifierLocalizationKey)
        {
            if (string.IsNullOrEmpty(modifierLocalizationKey)) {
                return modifierLocalizationKey;
            }

            var num = modifierLocalizationKey.IndexOf('_') + 1;
            var c = modifierLocalizationKey[num];
            var index = modifierLocalizationKey.IndexOf('_', num) + 1;
            var c2 = modifierLocalizationKey[index];
            return $"{char.ToUpper(c)}{char.ToUpper(c2)}";
        }
    }

    public struct BeatLeaderDiffInfo
    {
        public int id;
        public int value;
        public int mode;
        public int status;
        public string modeName;
        public string difficultyName;
        public float stars;
        public float accRating;
        public float passRating;
        public float techRating;
        public int type;
        public ModifiersMap modifierValues;
    }

    public struct BeatLeaderRating
    {
        public float accRating;
        public float passRating;
        public float techRating;

        public BeatLeaderRating(float accRating, float passRating, float techRating)
        {
            this.accRating = accRating;
            this.passRating = passRating;
            this.techRating = techRating;
        }
    }

    public class AccSaberRankedMap
    {
        public string difficulty;
        public string songHash;
        public float complexity;
    }

    public class Leaderboards
    {
        public ScoreSaber ScoreSaber { get; set; }
        public BeatLeader BeatLeader { get; set; }
        public AccSaber AccSaber { get; set; }
    }

    public class ScoreSaber
    {
        public List<Point> modifierCurve { get; set; }
        public List<Point> standardCurve { get; set; }
        public List<string> songsAllowingPositiveModifiers { get; set; }
        public ScoreSaberModifiers modifiers { get; set; }
    }

    public class ScoreSaberModifiers
    {
        public float da;
        public float gn;
        public float fs;
    }

    public class BeatLeader
    {
        public List<Point> accCurve { get; set; }
        public float accMultiplier { get; set; }
        public float passExponential { get; set; }
        public float passMultiplier { get; set; }
        public float passShift { get; set; }

        public float techExponentialMultiplier { get; set; }
        public float techMultiplier { get; set; }

        public float inflateExponential { get; set; }
        public float inflateMultiplier { get; set; }
    }

    public class AccSaber
    {
        public List<Point> curve { get; set; }
        public float scale;
        public float shift;
    }

    public class Point
    {
        public float x { get; set; }
        public float y { get; set; }
    }
}