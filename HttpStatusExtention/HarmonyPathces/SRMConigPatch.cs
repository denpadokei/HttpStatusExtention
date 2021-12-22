using HarmonyLib;
using System;

namespace HttpStatusExtention.HarmonyPathces
{
    [HarmonyPatch("SongRequestManagerV2.Configuration.RequestBotConfig, SongRequestManagerV2", "RequestQueueOpen", MethodType.Setter)]
    public class SRMConigPatch
    {
        public static void Postfix(ref object __instance)
        {
            var isOpen = (bool)__instance.GetType().GetProperty("RequestQueueOpen").GetValue(__instance);
            OnQueueStatusChanged?.Invoke(isOpen);
        }

        public static event Action<bool> OnQueueStatusChanged;
    }
}
