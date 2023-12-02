using HttpStatusExtention.PPCounters;

namespace HttpStatusExtention.Installers
{
    public class HttpStatusExtentionInstaller : Zenject.Installer
    {
        public override void InstallBindings()
        {
            _ = this.Container.BindInterfacesAndSelfTo<PPData>().AsCached();
            _ = this.Container.BindInterfacesAndSelfTo<SSData>().AsCached();
            _ = this.Container.BindInterfacesAndSelfTo<AccSaberData>().AsCached();
            _ = this.Container.BindInterfacesAndSelfTo<BeatLeaderData>().AsCached();
            _ = this.Container.BindInterfacesAndSelfTo<AccSaberCalculator>().AsCached();
            _ = this.Container.BindInterfacesAndSelfTo<ScoreSaberCalculator>().AsCached();
            _ = this.Container.BindInterfacesAndSelfTo<BeatLeaderCalculator>().AsCached();
            _ = this.Container.BindInterfacesAndSelfTo<HttpStatusExtentionController>().AsCached().NonLazy();
        }
    }
}
