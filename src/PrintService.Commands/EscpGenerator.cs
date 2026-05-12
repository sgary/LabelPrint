using PrintService.Printing;

namespace PrintService.Commands;

public class EscpGenerator : ICommandGenerator
{
    public string GenerateCommands(PrintTemplate template, IReadOnlyDictionary<string, object> data, int offsetX, int offsetY)
    {
        return "ESC/P raw print command generation is not implemented. Please use Windows GDI printing instead.";
    }

    public string GetPrinterType() => "brother-escp";
}
