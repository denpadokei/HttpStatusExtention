using HttpSiraStatus;
using HttpSiraStatus.Interfaces;
using HttpSiraStatus.Util;
using HttpStatusExtention.HarmonyPathces;
using IPA.Loader;
using System;
using System.Reflection;
using Zenject;

namespace HttpStatusExtention
{
    public class SRMQueueStatus : IInitializable, IDisposable
    {
        private IStatusManager _statusManager;
        private bool _disposedValue;

        [Inject]
        public void Constractor(IStatusManager statusManager)
        {
            this._statusManager = statusManager;
        }

        public void Initialize()
        {
            if (PluginManager.GetPlugin("Song Request Manager V2") == null) {
                return;
            }
            try {
                SRMConigPatch.OnQueueStatusChanged += this.SRMConigPatch_OnQueueStatusChanged;
                var configType = Type.GetType("SongRequestManagerV2.Configuration.RequestBotConfig, SongRequestManagerV2");
                var instanceProperty = configType?.GetProperty("Instance", (BindingFlags.Static | BindingFlags.Public));
                var instance = instanceProperty?.GetValue(configType);
                var queueStatusProperty = configType?.GetProperty("RequestQueueOpen", (BindingFlags.Instance | BindingFlags.Public));
                var queueStatus = (bool)queueStatusProperty?.GetValue(instance);
                this.SRMConigPatch_OnQueueStatusChanged(queueStatus);
            }
            catch (Exception e) {
                Plugin.Log.Error(e);
            }
        }

        private void SRMConigPatch_OnQueueStatusChanged(bool obj)
        {
            this._statusManager.OtherJSON["srm_queue_status"] = new JSONBool(obj);
            this._statusManager.EmitStatusUpdate(ChangedProperty.Other, BeatSaberEvent.Other);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue) {
                if (disposing) {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    SRMConigPatch.OnQueueStatusChanged -= this.SRMConigPatch_OnQueueStatusChanged;
                }
                this._disposedValue = true;
            }
        }
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
