using System.ServiceProcess;

namespace PrintService.Core;

public static class Program
{
    public static void Main(string[] args)
    {
        var isConsoleMode = args.Any(arg => string.Equals(arg, "--console", StringComparison.OrdinalIgnoreCase));

        if (isConsoleMode)
        {
            var service = new PrintService();
            service.StartConsole(args);

            Console.WriteLine("PrintService running in console mode. Press any key to stop...");
            Console.ReadKey(intercept: true);

            service.StopConsole();
            return;
        }

        ServiceBase.Run(new PrintService());
    }
}
