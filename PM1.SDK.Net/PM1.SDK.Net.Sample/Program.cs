using System;
using System.Threading;

namespace Autolabor.PM1.Sample {
    internal class Program {
        private static void Main() {
            try {
                Methods.Initialize("", null, out _);
                Methods.State = StateEnum.Unlocked;
                Thread.Sleep(100);
                Methods.TurnAround(-0.2, 1, out _);
            } catch (Exception exception) {
                Console.WriteLine(exception);
            } finally {
                Methods.ShutdownSafey();
            }
        }
    }
}
