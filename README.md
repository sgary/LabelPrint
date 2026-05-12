# PrintService

[![Build Status](https://github.com/yourusername/PrintService/actions/workflows/build.yml/badge.svg)](https://github.com/yourusername/PrintService/actions)
[![Release](https://img.shields.io/github/v/release/yourusername/PrintService)](https://github.com/yourusername/PrintService/releases)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)

A Windows label printing service with WebSocket API support for thermal label printers. Provides real-time label generation and printing capabilities for Zebra (ZPL), Brother (BPL/ESC/P), and other label printers.

> **Note:** This project targets Windows only and cannot be built on macOS or Linux. Use the [GitHub Actions CI/CD](#building-with-github-actions) for automated Windows builds.

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

## Installation

### Option 1: Download Pre-built MSI (Recommended)

1. Go to [Releases](https://github.com/yourusername/PrintService/releases)
2. Download the latest MSI file (e.g., `PrintService-v1.0.0-x64.msi`)
3. Double-click to install
4. The service starts automatically

### Option 2: Manual Installation

Download the latest release and install as a Windows service:

```powershell
sc create PrintService binPath="C:\Path\To\PrintService.Core.exe" start=auto
sc start PrintService
```

## Quick Start

Once the service is installed and running, you can start printing labels from your web application:

### 1. Connect from Browser

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

### 2. Done

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

- Windows 10/11 or Windows Server 2019/2022
- .NET 8 SDK
- Visual Studio 2022 or JetBrains Rider (VS Code with C# extension also works)

> **Note for macOS/Linux developers:** This project uses Windows-only APIs (`System.ServiceProcess`, `System.Drawing.Printing`) and cannot be built locally. Use the [GitHub Actions workflows](#building-with-github-actions) for automated Windows builds.

### Build & Test (Windows)

```powershell
dotnet restore
dotnet build --configuration Release
dotnet test
```

### Run Locally (Windows)

```powershell
dotnet run --project src/PrintService.Core
```

The service will start on `ws://localhost:8080` by default.

## Building with GitHub Actions

This project uses **GitHub Actions** for automated CI/CD because it targets Windows-only APIs (`net8.0-windows`) and cannot be built on macOS or Linux.

### Automated Workflows

| Workflow | Trigger | Purpose |
|----------|---------|---------|
| **Build & Test** | Push to `main`, PRs | Build solution, run tests, create MSI artifact |
| **Release** | Tag push (`v*`) | Build, test, and publish MSI to GitHub Releases |

### Creating a Release

1. Update version in your code
2. Create and push a tag:
   ```bash
   git tag -a v1.0.0 -m "Release v1.0.0"
   git push origin v1.0.0
   ```
3. GitHub Actions automatically builds and publishes the MSI to Releases

### Local Development (on Windows)

```powershell
dotnet build
dotnet test
dotnet run --project src/PrintService.Core
```

## Documentation

- [Setup Guide](docs/setup-guide.md) — Installation, configuration, and troubleshooting
- [Templates](templates/) — Example JSON templates for common label types

## License

MIT
