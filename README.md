# YTDLManager

[![CI](https://github.com/mayvqt/YTDLManager/actions/workflows/ci.yml/badge.svg)](https://github.com/mayvqt/YTDLManager/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/v/release/mayvqt/YTDLManager?style=flat-square)](https://github.com/mayvqt/YTDLManager/releases/latest)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A modern Windows desktop GUI for yt-dlp. Built with .NET 8 and WPF, featuring a dark teal theme, concurrent downloads, and automatic updates for yt-dlp and ffmpeg.

Works with 1000+ sites including YouTube, Twitch, Twitter, and more.

## Features

**Download Management**
- Multi-threaded download queue with real-time progress tracking
- Supports 1000+ websites via yt-dlp
- Concurrent downloads with configurable limits
- Automatic retry on failures
- Playlist and channel support

**Media Processing**
- Video quality selection (144p to 8K)
- Audio-only extraction (MP3, AAC, FLAC, OGG, WAV)
- Codec preferences (H.264, VP9, AV1)
- Subtitle download and embedding
- Metadata and thumbnail embedding
- Custom filename templates

**Interface**
- Professional dark theme with teal accents
- Material Design components
- Real-time download statistics
- Error messages with suggestions
- Keyboard navigation support

**Updates**
- Auto-updates for yt-dlp and ffmpeg
- Version tracking
- Background update checks

## Installation

Grab the latest release from the [releases page](https://github.com/mayvqt/YTDLManager/releases/latest). Releases are built automatically via GitHub Actions.

### Build from Source

Requires .NET 8 SDK:

```powershell
git clone https://github.com/mayvqt/YTDLManager.git
cd YTDLManager/src
dotnet build -c Release
dotnet run
```

For a standalone executable:

```powershell
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

## Configuration

Settings are stored in `%APPDATA%\YTDLManager\config.json` and are created automatically on first run.

| Setting | Description | Default |
|---------|-------------|---------|
| `DefaultDownloadPath` | Where downloads are saved | `%USERPROFILE%\Downloads\YTDLManager` |
| `DefaultOptions.Quality` | Video quality preference | `Best` |
| `DefaultOptions.AudioFormat` | Audio format for extraction | `Best` |
| `MaxConcurrentDownloads` | Max simultaneous downloads | `3` |
| `UpdateCheckInterval` | Hours between update checks | `24` |
| `Theme` | UI theme | `Dark` |

Example config:

```json
{
  "DefaultDownloadPath": "C:\\Downloads",
  "DefaultOptions": {
    "Quality": "1080p",
    "AudioFormat": "MP3",
    "DownloadSubtitles": true,
    "EmbedThumbnail": true
  },
  "MaxConcurrentDownloads": 5
}
```

## Usage

1. Paste a URL
2. Pick quality and format options
3. Choose where to save (optional)
4. Click "Add Download"
5. Watch the progress bar

### Quality Options

- Video quality from 144p to 8K
- Audio-only mode for music downloads
- Codec selection (H.264, VP9, AV1)
- Custom yt-dlp arguments for advanced users

### Playlists

Downloads entire playlists or channels. You can also select specific ranges or download in reverse order.

### Subtitles

Toggle subtitle downloads and choose whether to embed them or save separately. Supports multiple languages.
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

Works with any site supported by yt-dlp (1000+ sites). Some examples:

- **Video**: YouTube, Vimeo, Twitch, Facebook, Instagram
- **Social**: Twitter/X, TikTok, Reddit, LinkedIn  
- **News**: BBC iPlayer, CNN, ESPN
- **Education**: Coursera, Udemy, Khan Academy

See the [full list](https://github.com/yt-dlp/yt-dlp/blob/master/supportedsites.md).

## Architecture

```
YTDLManager/
├── src/
│   ├── Views/              # WPF windows and controls
│   ├── ViewModels/         # MVVM view models
│   ├── Models/             # Data models
│   ├── Services/           # Business logic
│   │   ├── YtDlpService.cs
│   │   ├── DownloadService.cs
│   │   ├── UpdateService.cs
│   │   └── ConfigService.cs
│   ├── Converters/         # XAML value converters
│   └── Helpers/            # Utilities
```

**Stack**
- .NET 8 / WPF
- Material Design In XAML Toolkit
- CommunityToolkit.Mvvm
- Serilog
- Microsoft.Extensions.Hosting

**Patterns**
- MVVM with dependency injection
- Async/await throughout
- Service-based architecture
- Command pattern for UI actions

## Troubleshooting

**yt-dlp not found**
- Check internet connection (auto-downloads on first run)
- Check antivirus quarantine
- Verify write permissions to `%APPDATA%\YTDLManager\bin`
- Restart the app

**Downloads failing**
- Verify URL is supported
- Check for geo-restrictions or age-gates
- Try different quality settings
- Update yt-dlp via the UI button

**Slow performance**
- Lower concurrent download limit
- Check disk space
- Add download folder to antivirus exclusions

**Firewall issues**
- Add YTDLManager to Windows Defender exclusions
- Allow internet access in firewall settings

## Development

**Prerequisites**
- Visual Studio 2022 or VS Code with C# Dev Kit
- .NET 8 SDK

**Build**
```powershell
git clone https://github.com/mayvqt/YTDLManager.git
cd YTDLManager/src
dotnet build
dotnet run
```

**Testing**
```powershell
dotnet test
dotnet test --collect:"XPlat Code Coverage"
```

**Contributing**

PRs welcome! Please:
- Follow existing code style
- Use async/await for I/O operations
- Include tests for new features
- Update docs as needed

## License

MIT License - see [LICENSE](LICENSE) for details.

## Support

- **Issues**: [GitHub Issues](https://github.com/mayvqt/YTDLManager/issues)
- **Discussions**: [GitHub Discussions](https://github.com/mayvqt/YTDLManager/discussions)

## Acknowledgments

Built with:
- [yt-dlp](https://github.com/yt-dlp/yt-dlp) - The core download engine
- [Material Design In XAML](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) - UI components
- [Serilog](https://serilog.net/) - Logging framework

## Disclaimer

For personal use. Respect copyright laws and site ToS. The developers aren't responsible for how you use this software.
