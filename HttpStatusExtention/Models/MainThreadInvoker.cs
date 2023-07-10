using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.Util;

namespace HttpStatusExtention.Models
{
    internal class MainThreadInvoker : ComponentSingleton<MainThreadInvoker>
    {
        private readonly ConcurrentQueue<Action> actionQueue = new ConcurrentQueue<Action>();

        internal void Enqueue(IEnumerator enumerator)
        {
            this.actionQueue.Enqueue(() => this.StartCoroutine(enumerator));
        }

        internal void Enqueue(Action enumerator)
        {
            this.actionQueue.Enqueue(enumerator);
        }

        private void Update()
        {
            if (this.actionQueue.TryDequeue(out var action)) {
                action?.Invoke();
            }
        }
    }
}
