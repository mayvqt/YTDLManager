# YTDLManager

[![CI](https://github.com/mayvqt/YTDLManager/actions/workflows/ci.yml/badge.svg)](https://github.com/mayvqt/YTDLManager/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/v/release/mayvqt/YTDLManager?style=flat-square)](https://github.com/mayvqt/YTDLManager/releases/latest)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

## Overview

YTDLManager is a modern, high-performance Windows desktop application that provides an intuitive graphical interface for yt-dlp (youtube-dl). Built with .NET 8 and WPF, it features automatic updates, concurrent downloads, and comprehensive media format support across 1000+ websites.

The application is designed for both casual users who want a simple download experience and power users who need advanced customization options. With its async-first architecture and Material Design interface, YTDLManager delivers professional-grade performance while maintaining an accessible user experience.

## Features

### Core Functionality

**Download Management**
- Multi-threaded concurrent download queue with progress tracking
- Support for 1000+ websites through yt-dlp integration
- Real-time download statistics (speed, ETA, file size)
- Automatic retry logic with exponential backoff
- Playlist and channel download support with selective media extraction

**Media Processing**
- Quality selection from 144p to 8K with codec preferences (H.264, VP9, AV1, etc.)
- Audio extraction with format conversion (MP3, AAC, FLAC, OGG, WAV)
- Subtitle download and embedding with multi-language support
- Thumbnail and metadata embedding with chapter preservation
- Custom filename templates and output organization

**Auto-Update System**
- Automatic yt-dlp and ffmpeg updates with version tracking
- Background update checks with configurable intervals
- Safe update installation with rollback capabilities
- Version validation and integrity checking

### User Interface & Experience

**Modern Material Design**
- Dark and light theme support with system integration
- Responsive layout with adaptive sizing
- Real-time progress indicators with detailed status information
- Contextual error messages with suggested solutions
- Accessibility features for screen readers and keyboard navigation

**Advanced Configuration**
- Per-download custom arguments and post-processing options
- Global preferences with profile-based configurations
- Download history with search and filtering capabilities
- Batch operations for multiple URL processing
- Custom output path templates with variable substitution

### Architecture & Performance

**Async-First Design**
- Full async/await implementation throughout the application
- Non-blocking UI with responsive user interactions
- Cancellation token support for graceful operation termination
- Background service coordination with proper resource management

**Dependency Injection & MVVM**
- Clean separation of concerns with testable architecture
- Service-based design with interface abstractions
- View-ViewModel binding with property change notifications
- Command pattern implementation for user actions

**Reliability & Error Handling**
- Comprehensive logging with Serilog integration
- Structured error reporting with contextual information
- Graceful degradation when external services are unavailable
- Recovery mechanisms for transient failures

## Installation

### Download Pre-built Binary

Download the appropriate installer for your platform from the [latest release](https://github.com/mayvqt/YTDLManager/releases/latest):

- `YTDLManager-Setup-x64.msi` - Windows (x64) Installer
- `YTDLManager-Portable-x64.zip` - Windows (x64) Portable
- `YTDLManager-Setup-arm64.msi` - Windows (ARM64) Installer

All releases include SHA256 checksums in `checksums.txt` for verification.

Example download and verification (Windows):

```powershell
# Download and verify
Invoke-WebRequest -Uri "https://github.com/mayvqt/YTDLManager/releases/latest/download/YTDLManager-Setup-x64.msi" -OutFile "YTDLManager-Setup-x64.msi"
Invoke-WebRequest -Uri "https://github.com/mayvqt/YTDLManager/releases/latest/download/checksums.txt" -OutFile "checksums.txt"

# Verify checksum
$expectedHash = (Get-Content checksums.txt | Select-String "YTDLManager-Setup-x64.msi").ToString().Split()[0]
$actualHash = (Get-FileHash "YTDLManager-Setup-x64.msi" -Algorithm SHA256).Hash
if ($expectedHash -eq $actualHash) { Write-Host "Checksum verified ✓" } else { Write-Host "Checksum mismatch ✗" }

# Install
Start-Process msiexec.exe -Wait -ArgumentList '/i YTDLManager-Setup-x64.msi /quiet'
```

### Build from Source

Requirements: .NET 8 SDK or later

```powershell
git clone https://github.com/mayvqt/YTDLManager.git
cd YTDLManager/src
dotnet restore
dotnet build -c Release
```

To run in development mode:

```powershell
dotnet run
```

To create a self-contained deployment:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Configuration

YTDLManager stores configuration in `%APPDATA%\YTDLManager\config.json`. The application will create default settings on first run.

### Application Settings

| Setting | Description | Default |
|---------|-------------|---------|
| `DefaultDownloadPath` | Default directory for downloaded media | `%USERPROFILE%\Downloads\YTDLManager` |
| `DefaultOptions.Quality` | Preferred video quality | `Best` |
| `DefaultOptions.AudioFormat` | Default audio format for extraction | `Best` |
| `DefaultOptions.VideoCodec` | Preferred video codec | `Best` |
| `DefaultOptions.DownloadSubtitles` | Download subtitles by default | `false` |
| `DefaultOptions.EmbedSubtitles` | Embed subtitles in video files | `false` |
| `DefaultOptions.EmbedThumbnail` | Embed thumbnail in media files | `true` |
| `DefaultOptions.EmbedMetadata` | Embed metadata in media files | `true` |
| `MaxConcurrentDownloads` | Maximum simultaneous downloads | `3` |
| `UpdateCheckInterval` | Auto-update check frequency (hours) | `24` |
| `Theme` | UI theme preference | `Dark` |

### Advanced Configuration

For power users, additional options can be configured in the settings file:

```json
{
  "DefaultDownloadPath": "C:\\Downloads\\Media",
  "DefaultOptions": {
    "Quality": "1080p",
    "AudioFormat": "MP3",
    "VideoCodec": "H264",
    "DownloadSubtitles": true,
    "EmbedSubtitles": true,
    "EmbedThumbnail": true,
    "EmbedMetadata": true,
    "CustomArguments": "--no-playlist --add-metadata"
  },
  "MaxConcurrentDownloads": 5,
  "UpdateCheckInterval": 12,
  "Theme": "Light",
  "LogLevel": "Information",
  "RetryAttempts": 3,
  "RequestTimeout": 30
}
```

## Usage

### Basic Download

1. Launch YTDLManager
2. Paste a video URL into the input field
3. Select desired quality and format options
4. Choose output directory (optional)
5. Click "Add Download" to queue the media
6. Monitor progress in the Downloads panel

### Advanced Features

#### Quality Selection

- **Video Quality**: Choose from available resolutions (144p to 8K)
- **Video Codec**: Select H.264, VP9, AV1, or let yt-dlp choose the best
- **Audio Format**: Extract audio-only in MP3, AAC, FLAC, OGG, or WAV formats
- **Custom Arguments**: Add advanced yt-dlp command-line options

#### Playlist Management

- **Full Playlist**: Download all videos in a playlist or channel
- **Selective Download**: Choose specific videos from a playlist
- **Range Selection**: Download videos within a specified range
- **Reverse Order**: Download playlist items in reverse chronological order

#### Subtitle Handling

- **Auto-Generated**: Download auto-generated subtitles where available
- **Manual Subtitles**: Download human-created subtitle tracks
- **Multiple Languages**: Select specific subtitle languages
- **Embedding**: Embed subtitles directly into video files or save separately

### Batch Operations

Process multiple URLs simultaneously:

1. Paste multiple URLs (one per line) in the input field
2. Configure shared options for all downloads
3. Click "Add All Downloads" to queue all media
4. Individual downloads can be managed independently

### Download Management

- **Queue Control**: Pause, resume, or cancel individual downloads
- **Progress Monitoring**: Real-time speed, ETA, and completion status
- **Error Handling**: Automatic retries with manual retry options
- **History**: View completed downloads with search and filtering

## Supported Sites

YTDLManager supports all sites that yt-dlp supports (1000+ sites), including but not limited to:

**Video Platforms**
- YouTube (videos, playlists, channels, live streams)
- Vimeo (videos, channels, groups)
- Twitch (videos, clips, streams)
- Facebook (videos, live streams)
- Instagram (videos, stories, IGTV)

**Social Media**
- Twitter/X (videos, live streams)
- TikTok (videos, users)
- Reddit (videos, GIFs)
- LinkedIn (videos)

**News & Media**
- BBC iPlayer, CNN, Fox News
- ESPN, NBC Sports
- Dailymotion, LiveLeak

**Educational**
- Coursera, Udemy
- Khan Academy
- MIT OpenCourseWare

For a complete list, see the [yt-dlp supported sites documentation](https://github.com/yt-dlp/yt-dlp/blob/master/supportedsites.md).

## Architecture

### Project Structure

```
YTDLManager/
├── src/
│   ├── App.xaml                    # Application entry point and theme resources
│   ├── App.xaml.cs                 # Application lifecycle and dependency injection
│   ├── YTDLManager.csproj          # Project configuration and dependencies
│   ├── Views/
│   │   ├── MainWindow.xaml         # Main application window
│   │   └── MainWindow.xaml.cs      # Main window code-behind
│   ├── ViewModels/
│   │   └── MainViewModel.cs        # Main application view model
│   ├── Models/
│   │   └── DownloadModels.cs       # Data models for downloads and configuration
│   ├── Services/
│   │   ├── ConfigService.cs        # Configuration management
│   │   ├── DownloadService.cs      # Download queue and management
│   │   ├── UpdateService.cs        # Auto-update functionality
│   │   └── YtDlpService.cs         # yt-dlp integration and process management
│   ├── Converters/
│   │   └── ValueConverters.cs      # XAML value converters for data binding
│   └── Helpers/
│       └── UtilityHelpers.cs       # Utility functions and extensions
├── .gitignore                      # Git ignore patterns
├── LICENSE                         # MIT license
├── DEVELOPMENT.md                  # Development guidelines and setup
├── SUMMARY.md                      # Project overview and features
└── README.md                       # This file
```

### Technology Stack

**Core Framework**
- .NET 8 with WPF for native Windows performance
- Material Design In XAML Toolkit for modern UI components
- CommunityToolkit.Mvvm for MVVM infrastructure

**Dependencies**
- Serilog for structured logging with file and console output
- Newtonsoft.Json for configuration serialization
- Microsoft.Extensions.Hosting for dependency injection and service management
- Microsoft.Extensions.DependencyInjection for IoC container

**External Tools**
- yt-dlp for media downloading and format extraction
- ffmpeg for media processing and conversion
- Auto-update system for keeping tools current

### Design Patterns

**MVVM Architecture**
- Clean separation between UI (Views) and business logic (ViewModels)
- Data binding for automatic UI updates
- Command pattern for user interactions
- Service layer for business logic abstraction

**Dependency Injection**
- Constructor injection for service dependencies
- Interface-based abstractions for testability
- Singleton and transient lifetimes as appropriate
- Service registration in application startup

**Async/Await**
- Non-blocking UI operations
- Concurrent download management
- Cancellation token support
- Background service coordination

## Performance & Reliability

### Concurrency

YTDLManager uses a sophisticated concurrency model:

- **Download Queue**: Thread-safe concurrent queue with configurable limits
- **Progress Tracking**: Real-time updates without UI thread blocking  
- **Resource Management**: Automatic cleanup of completed processes
- **Cancellation Support**: Graceful termination of running operations

### Error Handling

- **Process Management**: Automatic retry for transient failures
- **Network Resilience**: Timeout handling and connection recovery
- **User Feedback**: Clear error messages with actionable suggestions
- **Logging**: Comprehensive error tracking for troubleshooting

### Memory Management

- **Streaming Downloads**: Minimal memory footprint for large files
- **Process Cleanup**: Automatic disposal of completed operations
- **UI Virtualization**: Efficient rendering of large download lists
- **Background Monitoring**: Low-overhead status updates

## Troubleshooting

### yt-dlp Not Found

If YTDLManager cannot locate yt-dlp:

1. Ensure internet connectivity for automatic download
2. Check Windows Defender or antivirus quarantine
3. Verify write permissions to `%APPDATA%\YTDLManager\bin`
4. Manually download yt-dlp to the bin directory
5. Restart the application

### Download Failures

If downloads consistently fail:

1. Verify the URL is supported by yt-dlp
2. Check if the content is age-restricted or geo-blocked
3. Try downloading with different quality settings
4. Enable debug logging in settings for detailed error information
5. Update yt-dlp to the latest version

### Performance Issues

If the application is slow or unresponsive:

1. Reduce concurrent download limit in settings
2. Close other bandwidth-intensive applications
3. Check available disk space in download directory
4. Disable real-time antivirus scanning for download folder
5. Consider using SSD storage for download location

### Firewall and Security

If updates or downloads are blocked:

1. Add YTDLManager to Windows Defender exclusions
2. Configure firewall to allow YTDLManager internet access
3. Check corporate proxy or content filtering settings
4. Verify yt-dlp.exe is not quarantined by security software

## Development

### Prerequisites

- Visual Studio 2022 or VS Code with C# Dev Kit
- .NET 8 SDK
- Git for version control
- Optional: JetBrains Rider

### Building

Clone and build the project:

```powershell
git clone https://github.com/mayvqt/YTDLManager.git
cd YTDLManager/src
dotnet restore
dotnet build
```

Run in development mode:

```powershell
dotnet run
```

### Testing

Run unit tests:

```powershell
dotnet test
```

Run with coverage:

```powershell
dotnet test --collect:"XPlat Code Coverage"
```

### Debugging

Launch with attached debugger:

```powershell
dotnet run --configuration Debug
```

Enable debug logging by setting `LogLevel` to `Debug` in the configuration file.

### Contributing

Contributions are welcome! Please follow these guidelines:

1. **Fork the Repository**: Create your own fork for development
2. **Create Feature Branch**: Use descriptive branch names (`feature/download-scheduling`)
3. **Write Tests**: Include unit tests for new functionality
4. **Follow Conventions**: Use consistent coding style and naming
5. **Update Documentation**: Include relevant documentation updates
6. **Submit Pull Request**: Provide clear description of changes

#### Code Style

- Follow .NET naming conventions and coding standards
- Use async/await for all I/O operations
- Include XML documentation for public APIs
- Write descriptive commit messages
- Keep methods focused and classes cohesive

#### Pull Request Process

1. Ensure all tests pass and no new warnings are introduced
2. Update the README if adding user-facing features
3. Add/update unit tests for new functionality
4. Verify the application builds and runs correctly
5. Request review from maintainers

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Support

- **Issues**: Report bugs or request features via [GitHub Issues](https://github.com/mayvqt/YTDLManager/issues)
- **Discussions**: Ask questions or share ideas in [GitHub Discussions](https://github.com/mayvqt/YTDLManager/discussions)
- **Documentation**: Additional documentation available in [DEVELOPMENT.md](DEVELOPMENT.md)

## Acknowledgments

This project builds upon the excellent work of:

- **[yt-dlp](https://github.com/yt-dlp/yt-dlp)** - The powerful command-line media downloader
- **[Material Design In XAML](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)** - Beautiful Material Design components for WPF
- **[Serilog](https://serilog.net/)** - Flexible and structured logging framework
- **[CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)** - Modern MVVM framework for .NET

## Disclaimer

YTDLManager is intended for personal use and educational purposes. Users are responsible for complying with applicable laws, terms of service, and copyright regulations when downloading media content. The developers assume no liability for misuse of this software.

Always respect content creators' rights and platform terms of service. Consider supporting creators through official channels when possible.
