using HttpStatusExtention.PPCounters;
using HttpStatusExtention.SongDetailsCaches;
using Zenject;

namespace HttpStatusExtention.Installers
{
    public class HttpStatusExtentionAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            _ = this.Container.BindInterfacesAndSelfTo<SongDetailsCacheUtility>().AsSingle().NonLazy();
            _ = this.Container.BindInterfacesAndSelfTo<PPDownloader>().AsSingle().NonLazy();
        }
    }
}
