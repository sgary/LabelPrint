# PrintService

A Windows label printing service with WebSocket API support for thermal label printers. Provides real-time label generation and printing capabilities for Zebra (ZPL), Brother (BPL/ESC/P), and other label printers.

## Features

- **WebSocket API** — Real-time bidirectional communication for print jobs
- **Multi-Protocol Support** — ZPL, BPL, and ESC/P command generation
- **Template System** — JSON-based label templates with variable interpolation
- **1D Barcodes** — Code39, Code128, EAN-13, UPC-A, and more
- **2D Barcodes** — QR Code and DataMatrix support
- **Batch Printing** — Queue and batch multiple print jobs for throughput
- **Offset Calibration** — Fine-tune label positioning per template or globally
- **Windows Service** — Runs as a native Windows service, starts on boot
- **Printer Status** — Query online/offline status of configured printers

## Quick Start

### 1. Install

Download the latest release and install as a Windows service:

```powershell
sc create PrintService binPath="C:\Path\To\PrintService.Core.exe" start=auto
sc start PrintService
```

### 2. Connect from Browser

```html
<script>
  const socket = new WebSocket('ws://localhost:8080');

  socket.onopen = () => console.log('Connected');

  function printLabel(templateId, data) {
    socket.send(JSON.stringify({
      requestId: crypto.randomUUID(),
      action: 'print',
      printerName: 'Zebra ZD420',
      templateId: templateId,
      data: data,
      copies: 1
    }));
  }

  // Print a product label
  printLabel('product-label', {
    ProductName: 'Widget Pro',
    Price: '29.99',
    SKU: 'WID-001',
    ProductUrl: 'https://example.com/product/wid-001'
  });
</script>
```

### 3. Done

Your labels will be sent to the configured printer. See the [Setup Guide](docs/setup-guide.md) for detailed instructions.

## Project Structure

```
PrintService/
├── src/
│   ├── PrintService.Core/        # Service entry point & WebSocket server
│   ├── PrintService.Protocol/    # Request/response models & message handler
│   ├── PrintService.Printing/    # Template engine, printer manager, queue
│   ├── PrintService.Barcodes/    # 1D & 2D barcode generation
│   └── PrintService.Commands/    # ZPL, BPL, ESC/P command generators
├── tests/
│   └── PrintService.Tests/       # Unit tests
├── templates/                    # Sample print templates
├── docs/                         # Documentation
│   └── setup-guide.md            # Installation & configuration guide
├── PrintService.sln              # Solution file
└── README.md                     # This file
```

## Project Components

| Component | Description |
|-----------|-------------|
| **Core** | Windows service host, WebSocket server, configuration management |
| **Protocol** | WebSocket message models and request/response handling |
| **Printing** | Template engine, printer manager, print queue, offset calibration |
| **Barcodes** | 1D barcode (Code39, Code128, EAN, UPC) and 2D barcode (QR, DataMatrix) generation |
| **Commands** | Command generators for Zebra ZPL, Brother BPL, and ESC/P printers |

## Development

### Prerequisites

- .NET 8 SDK
- An IDE (VS 2022, Rider, VS Code)

### Build & Test

```powershell
dotnet build
dotnet test
```

### Run Locally

```powershell
dotnet run --project src/PrintService.Core
```

The service will start on `ws://localhost:8080` by default.

## Documentation

- [Setup Guide](docs/setup-guide.md) — Installation, configuration, and troubleshooting
- [Templates](templates/) — Example JSON templates for common label types

## License

MIT
