using HttpStatusExtention.Bases;
using Zenject;

namespace HttpStatusExtention
{
    public class HttpStatusExtentionController : HttpStatusExtentonControllerBase
    {
        private PauseController _pauseController;

        private void OnGameResume() => HMMainThreadDispatcher.instance.Enqueue(this.SongStartWait(false, false));

        [Inject]
        protected void Constractor(DiContainer diContainer)
        {
            this._pauseController = diContainer.TryResolve<PauseController>();
        }

        protected override void Setup()
        {
            if (this._pauseController != null) {
                this._pauseController.didResumeEvent += this.OnGameResume;
            }
            base.Setup();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                if (this._pauseController != null) {
                    this._pauseController.didResumeEvent -= this.OnGameResume;
                }
            }
            base.Dispose(disposing);
        }
    }
}
