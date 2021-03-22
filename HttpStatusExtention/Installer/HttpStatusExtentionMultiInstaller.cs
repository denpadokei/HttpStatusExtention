using Zenject;

namespace HttpStatusExtention.Installer
{
    public class HttpStatusExtentionMultiInstaller : MonoInstaller
    {
        public override void InstallBindings() => this.Container.BindInterfacesAndSelfTo<HttpStatusExtentionMultiController>().AsCached().NonLazy();
    }
}
