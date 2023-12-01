using Zenject;

namespace HttpStatusExtention.Installers
{
    public class HttpStatusExtentionMenuAndGameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            _ = this.Container.BindInterfacesAndSelfTo<SRMQueueStatus>().AsCached().NonLazy();
        }
    }
}
