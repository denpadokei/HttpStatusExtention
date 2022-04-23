using HarmonyLib;
using IPA.Loader;
using System;

namespace HttpStatusExtention.HarmonyPathces
{
    [HarmonyPatch("SongRequestManagerV2.Configuration.RequestBotConfig, SongRequestManagerV2", "RequestQueueOpen", MethodType.Setter)]
    public class SRMConigPatch
    {
        [HarmonyPrepare]
        public static bool Prepare()
        {
            return PluginManager.GetPlugin("Song Request Manager V2") != null;
        }

        public static void Postfix(ref object __instance)
        {
            var isOpen = (bool)__instance.GetType().GetProperty("RequestQueueOpen").GetValue(__instance);
            OnQueueStatusChanged?.Invoke(isOpen);
        }

        public static event Action<bool> OnQueueStatusChanged;
    }
}
