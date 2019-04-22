using System;
using System.Threading;

namespace Autolabor.PM1.Sample {
    internal class ProgressHandler : IProgress<double> {
        public void Report(double value) => Console.WriteLine(value.ToString("0.%"));
    }

    internal class Program {
        private static void Main() {
            try {
                Methods.Initialize("", out _);
                Methods.State = StateEnum.Unlocked;
                Thread.Sleep(100);
                AsyncMethods.DriveAsync(
                    0.1, 0, Methods.SpatiumCalculate(0.5, 0),
                    new ProgressHandler(),
                    (e) => Console.WriteLine(e.Message)
                ).Wait();
            } finally {
                Methods.ShutdownSafety();
            }
        }
    }
}
