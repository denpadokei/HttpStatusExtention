using HarmonyLib;
using HttpSiraStatus;
using HttpSiraStatus.Interfaces;
using HttpSiraStatus.Util;
using HttpStatusExtention.HarmonyPathces;
using IPA.Loader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace HttpStatusExtention
{
    public class SRMQueueStatus : IInitializable, IDisposable
    {
        private IStatusManager _statusManager;
        private bool disposedValue;

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
            _statusManager.OtherJSON["srm_queue_status"] = new JSONBool(obj);
            _statusManager.EmitStatusUpdate(ChangedProperty.Other, BeatSaberEvent.Other);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    SRMConigPatch.OnQueueStatusChanged -= this.SRMConigPatch_OnQueueStatusChanged;
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
