# PrintService Setup Guide

## System Requirements

| Requirement | Minimum |
|-------------|---------|
| **OS** | Windows 10/11 (x64) or Windows Server 2019+ |
| **Runtime** | .NET 8.0 Runtime (desktop) |
| **Disk** | 50 MB free space |
| **Memory** | 128 MB RAM |
| **Printer** | Any Windows-installed label/printer (ZPL, BPL, or ESC/P) |

## Installation

### 1. Install .NET 8 Runtime

Download and install the .NET 8 Runtime from:
https://dotnet.microsoft.com/download/dotnet/8.0

Choose the **.NET Desktop Runtime 8.0.x** for Windows.

Verify installation:

```powershell
dotnet --list-runtimes
```

You should see output similar to:
```
Microsoft.NETCore.App 8.0.x [...]
Microsoft.WindowsDesktop.App 8.0.x [...]
```

### 2. Download the Service

Download the latest release from the releases page, or build from source:

```powershell
git clone https://github.com/your-org/PrintService.git
cd PrintService
dotnet publish src/PrintService.Core -c Release -o publish
```

### 3. Install the Service

Open PowerShell **as Administrator** and run:

```powershell
cd C:\Path\To\publish
sc create PrintService binPath="%CD%\PrintService.Core.exe" start=auto
sc start PrintService
```

Verify the service is running:

```powershell
sc query PrintService
```

You should see `STATE: 4 RUNNING`.

### 4. Configure Firewall

If connecting from another machine, allow port 8080:

```powershell
New-NetFirewallRule -DisplayName "PrintService" -Direction Inbound -Protocol TCP -LocalPort 8080 -Action Allow
```

## Printer Configuration

### Install Your Label Printer

1. Connect the printer via USB or network
2. Install the manufacturer's Windows driver
3. Set the label size in **Printers & scanners > Printing preferences**

### Find the Printer Name

```powershell
Get-CimInstance Win32_Printer | Select-Object Name
```

Use the exact printer name in the configuration file.

## Service Configuration

The configuration file `config.json` is auto-created at the service's working directory on first run.

### Configuration Reference

```json
{
  "server": {
    "port": 8080,
    "bindAddress": "127.0.0.1"
  },
  "logging": {
    "level": "info",
    "path": "C:\\ProgramData\\PrintService\\logs"
  },
  "printing": {
    "batchSize": 10,
    "batchIntervalMs": 200,
    "defaultOffsetX": 0,
    "defaultOffsetY": 0
  },
  "templates": {
    "path": "C:\\ProgramData\\PrintService\\templates"
  }
}
```

| Key | Description | Default |
|-----|-------------|---------|
| `server.port` | WebSocket server port | `8080` |
| `server.bindAddress` | Bind address (`127.0.0.1` for local only, `0.0.0.0` for all interfaces) | `127.0.0.1` |
| `logging.level` | Log level: `debug`, `info`, `warn`, `error` | `info` |
| `logging.path` | Log file directory | `C:\ProgramData\PrintService\logs` |
| `printing.batchSize` | Maximum labels per batch job | `10` |
| `printing.batchIntervalMs` | Milliseconds between batch polls | `200` |
| `printing.defaultOffsetX` | Global X offset in dots | `0` |
| `printing.defaultOffsetY` | Global Y offset in dots | `0` |
| `templates.path` | Template JSON files directory | `C:\ProgramData\PrintService\templates` |

### Edit Configuration

1. Stop the service:
   ```powershell
   sc stop PrintService
   ```
2. Edit `config.json` in a text editor
3. Start the service:
   ```powershell
   sc start PrintService
   ```

## Template Upload Instructions

Templates are JSON files placed in the templates directory (default: `C:\ProgramData\PrintService\templates`).

### Template Format

Each template defines a label layout with positioned elements:

```json
{
  "id": "my-label",
  "name": "My Label",
  "version": "1.0",
  "pageSize": { "width": 60, "height": 40, "unit": "mm" },
  "elements": [
    { "type": "text", "x": 5, "y": 5, "content": "Hello", "fontSize": 8, "bold": true },
    { "type": "barcode", "x": 5, "y": 15, "code": "12345", "format": "Code128", "height": 10 }
  ],
  "defaultOffset": { "x": 0, "y": 0 }
}
```

### Element Types

| Type | Properties | Description |
|------|-----------|-------------|
| `text` | `content`, `font`, `fontSize`, `bold`, `align` | Text label |
| `barcode` | `code`, `format`, `height`, `moduleWidth`, `showText` | 1D barcode |
| `qrcode` | `content`, `size`, `errorCorrection` | 2D barcode (QR, DataMatrix) |
| `line` | `x1`, `y1`, `x2`, `y2`, `lineWidth` | Line separator |
| `image` | `src`, `width`, `height` | Image embedding |

### Variables

Templates support `{{VariableName}}` placeholders in `content`, `code`, and `src` fields. Values are supplied at print time via the API.

### Upload Steps

1. Create your template JSON file
2. Copy it to the templates directory:
   ```powershell
   copy my-label.json "C:\ProgramData\PrintService\templates\"
   ```
3. Restart the service to load new templates:
   ```powershell
   sc stop PrintService && sc start PrintService
   ```

## Browser Integration (JavaScript)

Connect to the service from a web page using the WebSocket API.

### Basic Example

```javascript
const socket = new WebSocket('ws://localhost:8080');

socket.onopen = () => {
  console.log('Connected to PrintService');
};

socket.onmessage = (event) => {
  const response = JSON.parse(event.data);
  console.log('Response:', response);
};

socket.onerror = (error) => {
  console.error('WebSocket error:', error);
};

function printLabel(templateId, data) {
  const request = {
    requestId: crypto.randomUUID(),
    action: 'print',
    printerName: 'Zebra ZD420',
    templateId: templateId,
    data: data,
    copies: 1
  };
  socket.send(JSON.stringify(request));
}

// Usage
printLabel('product-label', {
  ProductName: 'Widget Pro',
  Price: '29.99',
  SKU: 'WID-001',
  ProductUrl: 'https://example.com/product/wid-001'
});
```

### Batch Printing

```javascript
function printBatch(templateId, items) {
  const request = {
    requestId: crypto.randomUUID(),
    action: 'printBatch',
    printerName: 'Zebra ZD420',
    templateId: templateId,
    items: items.map(data => ({ data, copies: 1 }))
  };
  socket.send(JSON.stringify(request));
}

// Usage
printBatch('shipping-label', [
  { ReceiverName: 'Alice', TrackingNumber: '1Z999AA10123456784', /* ... */ },
  { ReceiverName: 'Bob', TrackingNumber: '1Z999AA10123456785', /* ... */ }
]);
```

### Getting Printer Status

```javascript
function getPrinterStatus() {
  const request = {
    requestId: crypto.randomUUID(),
    action: 'getPrinterStatus',
    printerName: 'Zebra ZD420'
  };
  socket.send(JSON.stringify(request));
}
```

### Full API Reference

| Action | Description |
|--------|-------------|
| `print` | Print a single label |
| `printBatch` | Print multiple labels in one batch |
| `getPrinterStatus` | Query printer status |

## Offset Calibration

When labels print misaligned, use offset calibration to adjust positioning.

### Global Offset

Set `printing.defaultOffsetX` and `printing.defaultOffsetY` in `config.json` to shift all labels.

### Per-Template Offset

Each template has its own `defaultOffset`:

```json
{
  "id": "my-label",
  "defaultOffset": { "x": 2, "y": 1 },
  ...
}
```

### Calibration Procedure

1. Print a test label with known element positions
2. Measure the horizontal and vertical misalignment
3. At 203 DPI, 1 mm ≈ 8 dots. Calculate required offset:
   - Offset (dots) = Misalignment (mm) × 8
4. Apply the offset in the template or global config
5. Re-print and verify alignment
6. Repeat until alignment is satisfactory

Example: If text is 3 mm too far left, set `defaultOffsetX = 24` (3 × 8).

## Troubleshooting

### Service Won't Start

1. Check the logs at `C:\ProgramData\PrintService\logs`
2. Verify .NET 8 runtime is installed:
   ```powershell
   dotnet --list-runtimes
   ```
3. Ensure the port is not in use:
   ```powershell
   netstat -ano | findstr :8080
   ```

### Printer Not Found

1. List available printers:
   ```powershell
   Get-CimInstance Win32_Printer | Select-Object Name
   ```
2. Verify the printer name in the API request matches exactly (case-insensitive)
3. Check the printer is online and not in an error state

### Labels Printing Blank

1. Verify the template is valid JSON
2. Check that all `{{VariableName}}` placeholders have corresponding values in the request
3. Ensure the label media is loaded correctly in the printer
4. Test with a simple text-only template first

### Connection Refused

1. Verify the service is running: `sc query PrintService`
2. Check the bind address — if set to `127.0.0.1`, only local connections are allowed
3. For remote connections, set `bindAddress` to `0.0.0.0` and configure the firewall

### Alignment Issues

1. Verify the `pageSize` in the template matches the actual label media
2. Apply per-template offset to fix consistent misalignment
3. Check that the printer driver is configured for the correct label size
4. Some printers require media calibration — run the printer's auto-calibration routine

### Logs

View the latest logs:

```powershell
Get-Content "C:\ProgramData\PrintService\logs\printservice.log" -Tail 50
```

Enable debug logging by setting `logging.level` to `debug` in `config.json`.
