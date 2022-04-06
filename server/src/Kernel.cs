using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using iCSharp.Kernel;
using UnityEngine;
using static KRPC.Utils.Logger;
using System.Threading.Tasks;

namespace KRPC
{
    /// <summary>
    /// Main KRPC addon. Contains the kRPC core, config and UI.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.AllGameScenes, true)]
    [SuppressMessage("Gendarme.Rules.Correctness", "DeclareEventsExplicitlyRule")]
    public sealed class Kernel : MonoBehaviour
    {
        // TODO: clean this up

        /// <summary>
        /// The instance of the addon
        /// </summary>
        public static Kernel Instance { get; private set; }
        KernelLauncher _launcher;
        public KernelObj Current { get; set; }
        public bool ExecuteOnMainThread {get;set;} = true;
        public int AwakeCount { get; set; } = 0;

        public void Push(Action action) => Current?.Push(action);
        public void Register(KernelObj obj)
        {
            Current = obj;
        }

        public void PushAction(Action action){
          if (ExecuteOnMainThread) Current?.Push(action).Wait();
          else action();

        }
        public void UpdateInternal() => Current?.Update();

        /// <summary>
        /// Called whenever a scene change occurs. Ensures the server has been initialized,
        /// (re)creates the UI, and shuts down the server in the main menu.
        /// </summary>
        public void Awake()
        {
            AwakeCount += 1;
            Instance = this;
            Utils.Logger.WriteLine($"Startup of kernel mod", Severity.Info);
            Instance = this;
            this.enabled = true;

            KernelLauncher.Init();
            var config = new KernelLauncherConfig
            {
                KernelConfigJsonWriteDirectory = "/tmp/kernels",
                Name = "krpc",
            };

            _launcher = new KernelLauncher(config);
            var args = KernelLauncher.GetExtraParams();
            args.PushAction = PushAction;
            args.Data = this;
            var confFile = _launcher.Create(args);


            Utils.Logger.WriteLine($"Wrote icsharp kernel conf to {confFile}", Severity.Info);

        }
        public void OnApplicationQuit()
        {
            _launcher?.Dispose();
            _launcher = null;
        }

    }

    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    [SuppressMessage("Gendarme.Rules.Correctness", "DeclareEventsExplicitlyRule")]
    public sealed class KernelObj : MonoBehaviour
    {
        // TODO: clean this up

        /// <summary>
        /// The instance of the addon
        /// </summary>
        public static KernelObj Instance { get; private set; }
        public Queue<Task> Queue { get; set; } = new Queue<Task>();
        public int UpdateCount { get; set; } = 0;
        public int AwakeCount { get; set; } = 0;
        public bool LockUpdateThread {get;set;} = false;

        public Task Push(Action action)
        {
          var task = new Task(() => action());
            lock (Queue) Queue.Enqueue(task);
            return task;

        }

        public void Update()
        {
          UpdateCount += 1;
          do {
            try
            {
              lock (Queue)
              {
                while (Queue.Count > 0)
                {
                  var x = Queue.Dequeue();
                  x.RunSynchronously();
                }
              }
            }
            catch (Exception e)
            {
              Utils.Logger.WriteLine($"Update exception {e.Message} {e.StackTrace}");
            }
          } while(LockUpdateThread);
        }


        /// <summary>
        /// Called whenever a scene change occurs. Ensures the server has been initialized,
        /// (re)creates the UI, and shuts down the server in the main menu.
        /// </summary>
        public void Awake()
        {
          Kernel.Instance?.Register(this);
        }
        public void OnDestroy()
        {
          Kernel.Instance?.Register(null);
        }

    }
}
