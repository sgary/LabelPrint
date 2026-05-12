using PrintService.Printing;

namespace PrintService.Commands;

public interface ICommandGenerator
{
    string GenerateCommands(PrintTemplate template, IReadOnlyDictionary<string, object> data, int offsetX, int offsetY);

    string GetPrinterType();
}
