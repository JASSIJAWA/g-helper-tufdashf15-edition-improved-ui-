# G-Helper ROG Edition

A modern, Armoury Crate-style WPF frontend for [G-Helper](https://github.com/seerge/g-helper) — the lightweight alternative to ASUS Armoury Crate.

## Features

- 🎨 **ROG Dark Theme** — Borderless window, rounded corners, red accent styling
- ⚡ **Performance Modes** — Silent / Balanced / Turbo switching
- 📊 **System Monitoring** — Real-time CPU/GPU temps and fan RPM
- 🎮 **GPU Mode Control** — Eco / Standard / Ultimate (MUX) switching
- 🔋 **Battery Management** — Charge limit slider, health monitoring
- 🖥 **Display Controls** — Refresh rate, panel overdrive, FN lock toggles

## Tech Stack

- **Language:** C# (.NET 8.0)
- **UI Framework:** WPF (Windows Presentation Foundation)
- **Backend:** G-Helper's ACPI/WMI hardware interface

## Project Structure

```
g-helper/
├── app/              ← G-Helper backend (ACPI, WMI, hardware control)
├── GHelperWPF/       ← New WPF frontend (Armoury Crate theme)
└── docs/
```

## Building

```bash
dotnet build GHelperWPF/GHelperWPF.csproj
```

## Credits

- Original [G-Helper](https://github.com/seerge/g-helper) by [seerge](https://github.com/seerge)
- WPF ROG UI by Jassi-293

## License

GPL-3.0 (same as original G-Helper)
