# Kinect Azure Multi-Body Tracker

A comprehensive Unity package for Microsoft Azure Kinect multi-body tracking.

## Features

- Multi-person skeleton tracking
- Real-time depth image display
- Easy-to-use API functions
- Configurable tracking parameters
- Keyboard controls for quick testing

## Requirements

- Unity 2020.3 or later
- Microsoft Azure Kinect SDK
- Azure Kinect Body Tracking SDK

## Installation

1. Open Unity Package Manager
2. Click "Add package from git URL"
3. Enter: `https://github.com/yourusername/kinect-azure-tracker.git`

## Quick Start

1. Add `TrackerHandler_multi` to your scene
2. Add `DepthImageDisplay` component for depth visualization
3. Add `Easy_Functions` for convenient controls
4. Configure your tracking parameters in the Inspector

## API Reference

### Getting Detection Info
- `GetDetectedBodyCount()` - Get current number of detected people
- `GetHeadPosition(bodyId)` - Get head position coordinates
- `GetAllHeadPositions()` - Get all detected head positions

### Control Functions
- `ToggleSkeleton()` - Toggle skeleton display
- `ToggleDepthImage()` - Toggle depth image display
- `PrintCurrentInfo()` - Print detailed detection information

## Controls

- **[S]** - Toggle skeleton display
- **[D]** - Toggle depth image
- **[I]** - Print detection info
- **[F]** - Toggle image flip
- **[Shift + 1/2/3]** - Scale image

## License

[Your License Here]
