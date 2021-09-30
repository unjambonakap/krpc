using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using iCSharp.Kernel;
using UnityEngine;
using static KRPC.Utils.Logger;

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
        public Queue<Action> Queue { get; set; } = new Queue<Action>();
        public int UpdateCount { get; set; } = 0;
        public int AwakeCount { get; set; } = 0;
        public KernelObj Current { get; set; }

        public void Push(Action action) => Current?.Push(action);
        public void Register(KernelObj obj)
        {
            Current = obj;
        }


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
                Name = "_krpc",
            };

            _launcher = new KernelLauncher(config);
            var args = KernelLauncher.GetExtraParams();
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
        public Queue<Action> Queue { get; set; } = new Queue<Action>();
        public int UpdateCount { get; set; } = 0;
        public int AwakeCount { get; set; } = 0;

        public void Push(Action action)
        {
            lock (Queue) Queue.Enqueue(action);

        }

        public void Update()
        {
            UpdateCount += 1;
            try
            {
                lock (Queue)
                {
                    while (Queue.Count > 0)
                    {
                        var x = Queue.Dequeue();
                        x();
                    }
                }
            }
            catch (Exception e)
            {
                Utils.Logger.WriteLine($"Update exception {e.Message} {e.StackTrace}");
            }
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
