# 🔫 Gun Mayhem - Multiplayer 2D Battle Arena

A fast-paced multiplayer 2D platform shooter built with Unity and Photon PUN2 networking, featuring server-authoritative gameplay and competitive combat mechanics.

![Gun Mayhem Screenshot](Assets/Screenshot%202024-06-29%20205015.png)

## 🎮 Game Features

### Core Gameplay
- **Multiplayer Combat**: Up to 4 players in real-time battles
- **Platform Shooter**: 2D side-scrolling arena combat
- **Weapon System**: Multiple weapon types with unique behaviors
- **Explosive Combat**: Boom/grenade system for tactical gameplay
- **Physics-Based**: Realistic knockback and movement mechanics

### Network Architecture
- **Server-Authoritative**: Master Client handles all game logic
- **Anti-Cheat Protection**: Server-side validation of player actions
- **Smooth Synchronization**: Lag compensation and state interpolation
- **Object Pooling**: Optimized bullet and object management
- **Master Client Switching**: Seamless authority transfer when host leaves

### Weapon Arsenal
- **Assault Rifles**: High rate of fire, moderate damage
- **Shotguns**: Spread shot, high close-range damage  
- **Sniper Rifles**: High damage, long-range precision
- **Pistols**: Balanced damage and fire rate
- **Explosives**: Area damage boom system

## 🛠️ Technical Features

### Networking (Photon PUN2)
```csharp
// Master Client Authority System
- Server-side movement validation
- Centralized bullet physics
- Synchronized game state
- Network object pooling
- Authority transfer handling
```

### Performance Optimizations
- **Bullet Pooling**: Reduces garbage collection
- **Master Client Physics**: Single source of truth
- **Efficient Networking**: Minimal bandwidth usage
- **Smooth Interpolation**: 60 FPS gameplay experience

### Input System
- **New Input System**: Modern Unity input handling
- **Customizable Controls**: Rebindable key mappings
- **Controller Support**: Gamepad compatibility
- **Input Validation**: Server-side input verification

## 🎯 Controls

### Default Keyboard Controls
| Action | Key | Description |
|--------|-----|-------------|
| Move | A/D or Arrow Keys | Horizontal movement |
| Jump | Space | Jump/Double jump |
| Shoot | J | Fire current weapon |
| Boom | K | Throw explosive |
| Pause | Escape | Pause menu |

### Gameplay Mechanics
- **Double Jump**: Available when airborne
- **Weapon Pickup**: Walk over weapons to equip
- **Knockback System**: Physics-based impact reactions
- **Respawn System**: Automatic respawn after death
- **Random Weapon Boxes**: Appear periodically on map

## 🏗️ Project Structure

```
Assets/
├── Scripts/
│   ├── PlayerMovement.cs       # Player controller with networking
│   ├── WeaponHandler.cs        # Weapon management system
│   ├── Bullet.cs              # Bullet physics and networking
│   ├── Boom.cs                # Explosive system
│   ├── NetworkManager.cs      # Master Client authority
│   ├── BulletPool.cs          # Object pooling system
│   ├── RandomBox.cs           # Weapon spawn system
│   └── SoundManager.cs        # Audio management
├── Scenes/
│   ├── MainMenu.unity         # Main menu scene
│   ├── Lobby.unity           # Multiplayer lobby
│   └── GameLevel.unity       # Battle arena
├── Prefabs/
│   ├── Player.prefab         # Networked player prefab
│   ├── Weapons/              # Weapon prefabs
│   └── Bullets/              # Bullet prefabs
└── Resources/               # Network instantiation resources
```

## 🚀 Getting Started

### Prerequisites
- Unity 2022.3.9f1 or later
- Photon PUN2 (included)
- New Input System package

### Installation
1. **Clone Repository**
   ```bash
   git clone https://github.com/yourusername/gun-mayhem.git
   cd gun-mayhem
   ```

2. **Open in Unity**
   - Open Unity Hub
   - Click "Open" and select the project folder
   - Wait for Unity to import assets

3. **Photon Setup**
   - Get free Photon App ID from [Photon Dashboard](https://dashboard.photonengine.com)
   - Open `Window > Photon Unity Networking > PUN Wizard`
   - Enter your App ID

4. **Build Settings**
   - Add all scenes to Build Settings in order:
     1. MainMenu
     2. Lobby  
     3. GameLevel

### Running the Game
1. **Play in Editor**: Press Play in Unity Editor
2. **Build Executable**: 
   - File > Build Settings > Build
   - Run multiple instances for local testing

## 🌐 Network Architecture

### Master Client Authority
The game uses a server-authoritative model where the Master Client handles:

- **Movement Processing**: All player input validation
- **Bullet Physics**: Collision detection and damage
- **Game State**: Match timers, scoring, win conditions
- **Weapon Spawning**: Random box generation
- **Death/Respawn**: Player lifecycle management

### Anti-Cheat Measures
- Server-side input validation
- Position verification and correction
- Fire rate limiting
- Physics-based movement constraints

### Performance Features
```csharp
// Bullet pooling reduces GC pressure
BulletPool.Instance.GetBullet();  // Reuse bullets
BulletPool.Instance.ReturnBullet(bullet);  // Return to pool

// Master Client processes all physics
if (!PhotonNetwork.IsMasterClient) return;
ProcessPlayerMovement();  // Authoritative processing
```

## 🎨 Game Assets

### Graphics
- **Pixel Art Style**: Retro-inspired 2D sprites
- **Animated Characters**: Smooth player animations
- **Weapon Sprites**: Detailed weapon graphics
- **Environmental Art**: Platform and background assets

### Audio
- **Sound Effects**: Weapon firing, explosions, impacts
- **Background Music**: Dynamic gameplay soundtrack
- **Audio Manager**: Centralized sound system

### UI/UX
- **Main Menu**: Clean, modern interface
- **Lobby System**: Room browsing and creation
- **In-Game HUD**: Health, ammo, score display
- **Pause Menu**: Settings and game controls

## 🔧 Configuration

### Game Settings
```csharp
// Adjust in NetworkManager.cs
public int maxPlayers = 8;        // Room capacity
public float matchDuration = 300f; // 5 minute matches
public int respawnTime = 3;        // Respawn delay

// Weapon balance in Weapon.cs
public float fireRate = 5f;        // Shots per second
public int damage = 25;            // Damage per shot
public float range = 100f;         // Bullet travel distance
```

### Network Settings
- **Region**: Auto-select best Photon region
- **Send Rate**: 30 Hz for smooth gameplay
- **Tick Rate**: 60 FPS physics simulation

## 🐛 Troubleshooting

### Common Issues

1. **Connection Problems**
   - Check Photon App ID configuration
   - Verify internet connection
   - Try different Photon regions

2. **Synchronization Issues**
   - Ensure Master Client authority is working
   - Check PhotonView components on prefabs
   - Verify network object setup

3. **Performance Issues**
   - Enable object pooling for bullets
   - Reduce physics simulation complexity
   - Optimize network send rates

### Debug Tools
```csharp
// Enable debug logging
PhotonNetwork.LogLevel = PunLogLevel.Full;

// Monitor network statistics
Debug.Log($"Ping: {PhotonNetwork.GetPing()}ms");
Debug.Log($"Players: {PhotonNetwork.CurrentRoom.PlayerCount}");
```

## 🤝 Contributing

### Development Setup
1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Follow Unity coding standards
4. Test multiplayer functionality
5. Submit pull request

### Code Style
- Use C# naming conventions
- Comment complex networking logic
- Maintain Master Client authority pattern
- Test with multiple clients

## 📋 Roadmap

### Planned Features
- [ ] **More Game Modes**: Capture the Flag, King of the Hill
- [ ] **Weapon Customization**: Attachments and modifications
- [ ] **Player Progression**: Unlockable weapons and skins
- [ ] **Map Editor**: Community map creation tools
- [ ] **Tournament Mode**: Competitive ranking system
- [ ] **Mobile Support**: Touch controls and optimization

### Technical Improvements
- [ ] **Dedicated Servers**: Move beyond P2P networking
- [ ] **Spectator Mode**: Watch ongoing matches
- [ ] **Replay System**: Record and playback matches
- [ ] **Advanced Anti-Cheat**: Enhanced validation systems


## 🙏 Acknowledgments

- **Unity Technologies**: Game engine and networking
- **Photon**: Networking infrastructure
- **Asset Store Artists**: Graphics and audio assets
- **Community**: Testing and feedback


---

## 🎯 Quick Start Guide

### For Players
1. Download and launch Gun Mayhem
2. Enter your player name
3. Join or create a room
4. Wait for other players
5. Battle for supremacy!

### For Developers
1. Clone repository
2. Open in Unity 2022.3+
3. Set up Photon App ID
4. Build and test locally
5. Deploy to your platform

**Made with ❤️ in Unity**