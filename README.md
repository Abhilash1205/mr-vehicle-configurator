# MR Vehicle Configurator

> Interactive Mixed Reality vehicle customization app for Meta Quest 3 and mobile AR

![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-black?logo=unity)
![Platform](https://img.shields.io/badge/Platform-Quest%203%20%7C%20Mobile%20AR-blue)
![Status](https://img.shields.io/badge/Status-In%20Development-yellow)

## 📱 Overview

An immersive MR/AR vehicle configurator that allows users to visualize and customize Mercedes vehicles in real-world environments. Built for Meta Quest 3 and mobile AR platforms.

## ✨ Features

### Core Functionality
- 🚗 **Multi-Vehicle Support** - Mercedes GLC SUV and C63 AMG
- 🎨 **Real-Time Color Customization** - 8 colors for GLC, 6 for C63
- 🚪 **Interactive Door Animations** - Smooth opening/closing with custom pivot rotation
- 🔄 **360° Vehicle Rotation** - Rotate cars for complete viewing
- 📏 **Scale Controls** - Adjust vehicle size (0.5x - 2.0x)
- 📸 **Screenshot Capture** - Save customizations with one tap
- 🔊 **Sound Effects** - UI clicks, door sounds, engine audio

### Technical Features
- 📱 **AR Foundation** - Cross-platform AR support
- 🎮 **XR Interaction Toolkit** - Native Quest 3 support
- 🎨 **URP Pipeline** - Optimized rendering
- 💾 **State Management** - Persistent customization settings
- 🎵 **Audio System** - Centralized sound management

## 🛠️ Tech Stack

- **Engine:** Unity 2022.3 LTS (Compatible with Unity 6.3 LTS)
- **Rendering:** Universal Render Pipeline (URP)
- **AR:** AR Foundation 5.1+
- **XR:** XR Interaction Toolkit
- **Platform:** Meta Quest 3, Android, iOS
- **Language:** C#

## 📁 Project Structure

```
Assets/
├── _Project/
│   ├── Scenes/          # Main configurator scene
│   ├── Scripts/
│   │   ├── Core/        # VehicleConfigurator, ConfiguratorUI
│   │   ├── Audio/       # AudioManager, UIButtonSound
│   │   └── MR/          # MRPlacementController
│   ├── Prefabs/         # Vehicle prefabs (GLC, C63)
│   ├── Materials/       # Car paint materials
│   └── Audio/           # Sound effects
└── XR/                  # XR Interaction Toolkit assets
```

## 🚀 Getting Started

### Prerequisites

- Unity 2022.3 LTS or Unity 6.3 LTS
- Meta Quest 3 (for MR testing)
- Android device with ARCore (for mobile AR)

### Installation

1. Clone the repository
```bash
git clone https://github.com/yourusername/mr-vehicle-configurator.git
```

2. Open in Unity
   - Open Unity Hub
   - Click "Add" → Select project folder
   - Open with Unity 2022.3 LTS or 6.3 LTS

3. Install required packages
   - AR Foundation
   - XR Interaction Toolkit
   - XR Plugin Management

### Building for Quest 3

1. File → Build Settings → Android
2. Switch Platform
3. Player Settings:
   - XR Plug-in Management → OpenXR
   - Enable Quest 3 support
4. Build and Run

## 🎮 Controls

### Quest 3 (MR Mode)
- **Hand Tracking** - Point and pinch to interact
- **Controller** - Point and trigger to select

### Editor Testing
- **WASD** - Move camera
- **Right-click + Mouse** - Look around
- **Mouse Click** - UI interaction

## 📸 Screenshots

> Add screenshots of your app here:
> - Vehicle selection screen
> - Color customization
> - Door animation demo
> - Full showroom view

## 🗺️ Roadmap

- [x] Core vehicle placement
- [x] Color customization system
- [x] Door animations (GLC)
- [x] Sound effects integration
- [x] Showroom lighting
- [ ] Wheel style switching
- [ ] Camera orbit controls
- [ ] Interior customization
- [ ] Save/load configurations
- [ ] Hand tracking gestures

## 📝 Known Issues

- C63 doesn't have separate door animations (model limitation)
- Front left door requires manual hinge offset (1.0f)

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🎥 Demo Video

[![MR Vehicle Configurator Demo](https://img.youtube.com/vi/OOYYUikdA0w/maxresdefault.jpg)](https://www.youtube.com/watch?v=OOYYUikdA0w)

*Click to watch the full demo on YouTube*
- Sound effects: Mixkit, Freesound
- UI Icons: Flaticon

---

⭐ **Star this repo if you find it helpful!**
