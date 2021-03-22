using HttpStatusExtention.Bases;
using Zenject;

namespace HttpStatusExtention
{
    public class HttpStatusExtentionController : HttpStatusExtentonControllerBase
    {
        private PauseController _pauseController;

        private void OnGameResume() => HMMainThreadDispatcher.instance.Enqueue(this.SongStartWait(false, false));

        [Inject]
        protected void Constractor(PauseController pauseController) => this._pauseController = pauseController;

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
                this._pauseController.didResumeEvent -= this.OnGameResume;
            }
            base.Dispose(disposing);
        }
    }
}
