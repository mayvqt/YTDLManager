# Build and Development Guide

## Prerequisites

- **Visual Studio 2022** (Community, Professional, or Enterprise) with:
  - .NET desktop development workload
  - .NET 8 SDK
- **OR** .NET 8 SDK (standalone) + VS Code or other editor
- Windows 10/11 (64-bit)

## Quick Start

### Using Visual Studio 2022

1. Open `YTDLManager.sln` in Visual Studio 2022
2. Wait for NuGet package restore to complete
3. Press `F5` to build and run in Debug mode
4. Or use `Ctrl+Shift+B` to build without running

### Using .NET CLI

```powershell
# Restore dependencies
dotnet restore

# Build the project
dotnet build src/YTDLManager/YTDLManager.csproj

# Run the application
dotnet run --project src/YTDLManager/YTDLManager.csproj

# Build for Release
dotnet build src/YTDLManager/YTDLManager.csproj -c Release

# Publish self-contained executable
dotnet publish src/YTDLManager/YTDLManager.csproj -c Release -r win-x64 --self-contained -o publish/
```

## Project Structure

```
YTDLManager/
├── src/
│   └── YTDLManager/
│       ├── App.xaml                    # Application entry and resources
│       ├── App.xaml.cs                 # Application startup logic
│       ├── YTDLManager.csproj          # Project file
│       ├── Models/
│       │   └── DownloadModels.cs       # Domain models and enums
│       ├── Services/
│       │   ├── ConfigService.cs        # Configuration management
│       │   ├── UpdateService.cs        # Auto-update for yt-dlp/ffmpeg
│       │   ├── YtDlpService.cs         # yt-dlp wrapper
│       │   └── DownloadService.cs      # Download queue manager
│       ├── ViewModels/
│       │   └── MainViewModel.cs        # Main window view model
│       ├── Views/
│       │   ├── MainWindow.xaml         # Main window UI
│       │   └── MainWindow.xaml.cs      # Main window code-behind
│       ├── Converters/
│       │   └── ValueConverters.cs      # XAML value converters
│       └── Helpers/
│           └── UtilityHelpers.cs       # Utility functions
├── YTDLManager.sln                     # Solution file
├── README.md                           # Project documentation
├── LICENSE                             # MIT License
└── .gitignore                          # Git ignore rules
```

## Key Features Implemented

### 1. **Auto-Update System**
- Automatically checks for yt-dlp updates on startup
- Downloads latest yt-dlp from GitHub releases
- Downloads ffmpeg if not present
- Version tracking and display

### 2. **Download Management**
- Multi-threaded download queue (configurable concurrent downloads)
- Real-time progress tracking
- Download status management (Pending, Downloading, Completed, Failed, Cancelled)
- Cancel and remove downloads
- Clear completed downloads

### 3. **Quality & Format Options**
- Video quality selection (144p to 8K, or audio-only)
- Audio format selection (MP3, AAC, FLAC, WAV, etc.)
- Video codec preference (H.264, H.265, VP9, AV1)
- Subtitle download and embedding
- Metadata and thumbnail embedding

### 4. **Playlist Support**
- Detect and download entire playlists
- Playlist range selection (start/end)
- Reverse playlist download order

### 5. **Modern UI**
- Material Design theme (dark mode)
- Responsive layout
- Real-time download progress bars
- Status messages and notifications

### 6. **Configuration**
- Persistent settings storage in AppData
- Default download directory
- Default quality/format preferences
- Auto-update settings

## Architecture Highlights

### MVVM Pattern
- Clean separation of concerns
- ViewModels handle business logic
- Views are purely presentational
- Data binding for reactive UI updates

### Dependency Injection
- Microsoft.Extensions.DependencyInjection
- Service registration in App.xaml.cs
- Easy testing and maintainability

### Async/Await Throughout
- Non-blocking UI operations
- Responsive user experience
- Proper cancellation token support

### Thread-Safe Operations
- ConcurrentDictionary for download tracking
- SemaphoreSlim for concurrency control
- Process management for parallel downloads

### Logging
- Serilog for structured logging
- File and console outputs
- Log rotation (daily, 7-day retention)

## NuGet Packages Used

- **MaterialDesignThemes** (5.0.0) - Material Design UI components
- **MaterialDesignColors** (3.0.0) - Color themes
- **Microsoft.Extensions.DependencyInjection** (8.0.0) - DI container
- **Microsoft.Extensions.Hosting** (8.0.0) - Application host
- **CommunityToolkit.Mvvm** (8.2.2) - MVVM helpers and attributes
- **Serilog** (4.0.0) - Logging framework
- **Serilog.Sinks.File** (6.0.0) - File logging
- **Serilog.Sinks.Console** (6.0.0) - Console logging
- **Newtonsoft.Json** (13.0.3) - JSON serialization

## Debugging

### Common Issues

1. **yt-dlp not found**
   - The app automatically downloads yt-dlp on first run
   - Check `%APPDATA%/YTDLManager/bin/` for yt-dlp.exe

2. **ffmpeg not found**
   - The app automatically downloads ffmpeg if missing
   - Required for merging video/audio streams and conversions

3. **Download fails**
   - Check logs in `%APPDATA%/YTDLManager/Logs/`
   - Verify URL is valid and supported by yt-dlp
   - Check network connectivity

### Development Tips

- Use Debug mode for detailed logging
- Check Output window in Visual Studio for real-time logs
- Review log files for historical information
- Use breakpoints in ViewModels to debug business logic

## Extending the Application

### Adding New Download Options

1. Add property to `DownloadOptions` in `Models/DownloadModels.cs`
2. Add UI control in `Views/MainWindow.xaml`
3. Bind to ViewModel property in `ViewModels/MainViewModel.cs`
4. Update `BuildDownloadArguments()` in `Services/YtDlpService.cs`

### Adding New Services

1. Create interface in `Services/` folder
2. Implement service class
3. Register in `App.xaml.cs` ConfigureServices method
4. Inject into ViewModels or other services

### Customizing UI Theme

Edit `App.xaml`:
```xml
<materialDesign:BundledTheme 
    BaseTheme="Dark"           <!-- Light or Dark -->
    PrimaryColor="DeepPurple"  <!-- Primary color -->
    SecondaryColor="Lime" />   <!-- Accent color -->
```

## Performance Optimization

- **MaxConcurrentDownloads**: Default is 3, adjust based on system resources
- **MaxConcurrentFragments**: Controls parallel fragment downloads per video
- **Speed Limiting**: Optional bandwidth throttling
- **Proxy Support**: Built-in for restricted networks

## Security Considerations

- Downloads are executed in isolated processes
- No shell command injection vulnerabilities
- User input is sanitized for file paths
- URLs are validated before processing

## Future Enhancements

See README.md Roadmap section for planned features.

## Support

For issues or questions:
1. Check the logs in `%APPDATA%/YTDLManager/Logs/`
2. Review yt-dlp documentation: https://github.com/yt-dlp/yt-dlp
3. Open an issue on GitHub with:
   - Steps to reproduce
   - Log excerpts
   - System information (Windows version, .NET version)

## Contributing

See README.md for contribution guidelines.
