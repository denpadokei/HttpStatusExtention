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
            // 曲ごとにファイルを読み直したり、初期化待ちのポーリングを増やしたりしないよう
            // これらはアプリスコープで 1 度だけ初期化する。
            _ = this.Container.BindInterfacesAndSelfTo<PPData>().AsSingle().NonLazy();
            _ = this.Container.BindInterfacesAndSelfTo<SSData>().AsSingle().NonLazy();
            _ = this.Container.BindInterfacesAndSelfTo<AccSaberData>().AsSingle().NonLazy();
        }
    }
}
