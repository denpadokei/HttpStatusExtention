namespace HttpStatusExtention.Installer
{
    public class HttpStatusExtentionInstaller : Zenject.Installer
    {
        public override void InstallBindings() => this.Container.BindInterfacesAndSelfTo<HttpStatusExtentionController>().AsCached().NonLazy();
    }
}
