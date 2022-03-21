using HttpStatusExtention.SongDetailsCaches;
using Zenject;

namespace HttpStatusExtention.Installers
{
    public class HttpStatusExxtentionAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<SongDetailsCacheUtility>().AsSingle().NonLazy();
        }
    }
}
