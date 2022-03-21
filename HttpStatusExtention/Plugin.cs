using HarmonyLib;
using HttpStatusExtention.Installers;
using IPA;
using IPA.Loader;
using SiraUtil.Zenject;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;

namespace HttpStatusExtention
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        private static Harmony _harmony;
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
            _harmony = new Harmony(HARMONY_ID);
            zenjector.Install<HttpStatusExtentionInstaller>(Location.Player);
            zenjector.Install<HttpStatusExtentionMenuAndGameInstaller>(Location.Menu | Location.Player);
            zenjector.Install<HttpStatusExxtentionAppInstaller>(Location.App);
        }

        #region BSIPA Config
        //Uncomment to use BSIPA's config
        /*
        [Init]
        public void InitWithConfig(Config conf)
        {
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Log.Debug("Config loaded");
        }
        */
        #endregion

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
            if (PluginManager.GetPlugin("Song Request Manager V2") != null) {
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            //HMMainThreadDispatcher.instance.Enqueue(SongDataCoreUtil.Initialize());
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmony.UnpatchSelf();
        }
    }
}
