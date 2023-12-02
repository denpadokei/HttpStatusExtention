using HarmonyLib;
using HttpStatusExtention.Installers;
using IPA;
using SiraUtil.Zenject;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;

namespace HttpStatusExtention
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        private static Harmony s_harmony;
        public const string HARMONY_ID = "HttpStatusExtention.com.github.denpadokei";

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger, Zenjector zenjector)
        {
            Instance = this;
            Log = logger;
            Log.Info("HttpStatusExtention initialized.");
            s_harmony = new Harmony(HARMONY_ID);
            zenjector.Install<HttpStatusExtentionInstaller>(Location.Player);
            zenjector.Install<HttpStatusExtentionMenuAndGameInstaller>(Location.Menu | Location.Player);
            zenjector.Install<HttpStatusExtentionAppInstaller>(Location.App);
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Log.Debug("OnApplicationStart");
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");
        }

        [OnEnable]
        public void OnEnable()
        {
            try {
                s_harmony?.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (System.Exception e) {
                Log.Error(e);
            }
        }

        [OnDisable]
        public void OnDisable()
        {
            try {
                s_harmony?.UnpatchSelf();
            }
            catch (System.Exception e) {
                Log.Error(e);
            }
        }
    }
}
