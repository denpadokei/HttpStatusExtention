using HttpStatusExtention.PPCounters;
using Zenject;

namespace HttpStatusExtention.Installers
{
    public class HttpStatusExtentionInstaller : Zenject.Installer
    {
        public override void InstallBindings()
        {
            // PP データはアプリ全体で 1 度だけ読み込む (HttpStatusExtentionAppInstaller)。
            _ = this.Container.BindInterfacesAndSelfTo<AccSaberCalculator>().AsCached();
            _ = this.Container.BindInterfacesAndSelfTo<ScoreSaberCalculator>().AsCached();
            _ = this.Container.BindInterfacesAndSelfTo<BeatLeaderCalculator>().AsCached();
            _ = this.Container.BindInterfacesAndSelfTo<HttpStatusExtentionController>().AsCached().NonLazy();
        }
    }
}
