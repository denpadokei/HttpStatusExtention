using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace HttpStatusExtention.Installer
{
    public class HttpStatusExtentionMenuAndGameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<SRMQueueStatus>().FromNewComponentOnNewGameObject().AsCached().NonLazy();
        }
    }
}
