# YTDLManager - Modern yt-dlp GUI for .NET

A high-performance, modern Windows desktop application built with .NET 8 and WPF that provides a beautiful graphical interface for yt-dlp with automatic updates and comprehensive feature support.

## Features

### Core Features
- 🚀 **Auto-Update System** - Automatically keeps yt-dlp and ffmpeg up to date
- 🎨 **Modern Material Design UI** - Clean, intuitive interface with dark/light themes
- ⚡ **High Performance** - Multi-threaded download management with queue system
- 📊 **Real-time Progress Tracking** - Live download statistics and progress bars
- 🎥 **Format Selection** - Choose video quality, audio formats, and codecs
- 📝 **Subtitle Support** - Download and embed subtitles in multiple languages
- 📑 **Playlist Support** - Download entire playlists or channels
- 🔄 **Batch Downloads** - Queue multiple downloads simultaneously
- 💾 **Download History** - Track all completed downloads
- ⚙️ **Advanced Options** - Access to all yt-dlp command-line features

### Technical Features
- Built on **.NET 8** with WPF
- **MVVM Architecture** for clean separation of concerns
- **Async/Await** throughout for responsive UI
- **Dependency Injection** for testability and maintainability
- **Thread-Safe** concurrent download management
- **Extensible Plugin Architecture** for future enhancements
- **Comprehensive Logging** with Serilog

## Requirements

- Windows 10/11 (64-bit)
- .NET 8 Runtime (bundled with self-contained deployment)
- Internet connection for downloading content and updates

## Installation

### Option 1: Download Release (Coming Soon)
1. Download the latest release from the [Releases](../../releases) page
2. Extract the ZIP file
3. Run `YTDLManager.exe`

### Option 2: Build from Source
```powershell
# Clone the repository
git clone https://github.com/mayvqt/ytdlgui.git
cd ytdlgui

# Build the project
dotnet build src/YTDLManager/YTDLManager.csproj -c Release

# Run the application
dotnet run --project src/YTDLManager/YTDLManager.csproj
```

## Usage

### Basic Download
1. Paste a video URL into the input field
2. Select your desired quality and format
3. Choose output directory
4. Click "Download"

### Advanced Options
- **Quality Settings**: Select resolution (144p to 8K), codec (H.264, VP9, AV1)
- **Audio Options**: Extract audio only, choose format (MP3, AAC, FLAC, etc.)
- **Subtitles**: Download auto-generated or manual subtitles, embed or save separately
- **Playlist Options**: Download entire playlist, specific range, or individual videos
- **Post-Processing**: Embed metadata, thumbnails, chapters, and subtitles

## Architecture

```
src/
├── YTDLManager/                # Main WPF Application
│   ├── App.xaml                # Application entry point
│   ├── Views/                  # WPF Views (Windows, UserControls)
│   ├── ViewModels/             # MVVM ViewModels
│   ├── Models/                 # Domain models
│   ├── Services/               # Business logic services
│   │   ├── YtDlpService.cs     # yt-dlp wrapper
│   │   ├── UpdateService.cs    # Auto-update functionality
│   │   ├── DownloadService.cs  # Download queue management
│   │   └── ConfigService.cs    # Configuration management
│   ├── Converters/             # XAML value converters
│   ├── Helpers/                # Utility classes
│   └── Resources/              # Images, styles, themes
```

## Configuration

Settings are stored in `%APPDATA%/YTDLManager/config.json`:
- Default download directory
- Preferred quality and format settings
- Theme preferences
- Auto-update settings
- Advanced yt-dlp options

## Supported Sites

Supports all sites that yt-dlp supports (1000+ sites), including:
- YouTube
- Twitch
- Twitter/X
- Reddit
- TikTok
- Vimeo
- And many more...

## Roadmap

- [ ] Scheduler for automatic downloads
- [ ] Browser integration extension
- [ ] Custom format templates
- [ ] Download speed limiting
- [ ] Proxy support
- [ ] Video preview before download
- [ ] Conversion tools
- [ ] Cloud storage integration

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [yt-dlp](https://github.com/yt-dlp/yt-dlp) - The amazing command-line downloader
- [MaterialDesignInXamlToolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) - Beautiful Material Design components
- [Serilog](https://serilog.net/) - Flexible logging framework

## Disclaimer

This tool is for personal use only. Please respect copyright laws and the terms of service of the websites you download from. The developers are not responsible for any misuse of this software.
