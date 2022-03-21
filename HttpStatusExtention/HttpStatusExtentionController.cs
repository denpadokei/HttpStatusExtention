using HttpStatusExtention.Bases;
using System;
using Zenject;

namespace HttpStatusExtention
{
    public class HttpStatusExtentionController : HttpStatusExtentonControllerBase, IInitializable, IDisposable
    {
        private IGamePause _gamePause;

        private void OnGameResume()
        {
            HMMainThreadDispatcher.instance.Enqueue(this.SongStartWait(false));
        }

        [Inject]
        protected void Constractor(IGamePause gamePause)
        {
            this._gamePause = gamePause;

        }

        protected override void Setup()
        {
            this._gamePause.didResumeEvent += this.OnGameResume;
            base.Setup();
        }

        protected override void Dispose(bool disposing)
        {

            if (disposing) {
                this._gamePause.didResumeEvent -= this.OnGameResume;
            }
            base.Dispose(disposing);
        }
    }
}
