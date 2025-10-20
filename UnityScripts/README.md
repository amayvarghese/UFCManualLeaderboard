# Unity Supabase Leaderboard Integration (No External Packages!)

This folder contains Unity scripts that integrate with your Supabase database using only Unity's built-in networking - **NO EXTERNAL PACKAGES REQUIRED!**

## Files Included:

- `LeaderBoardTestManual.cs` - Main leaderboard script that connects to Supabase via REST API
- `README.md` - This documentation file

## Setup Instructions:

### 1. No External Packages Needed! ✅

This script uses only Unity's built-in `UnityWebRequest` system - no additional packages required!

### 2. Simple Setup

1. **Replace your existing LeaderBoardTest script:**
   - Remove or disable your old `LeaderBoardTest` component
   - Add the `LeaderBoardTestManual` component to your leaderboard GameObject
   - Configure all the same UI references as before

### 3. Configuration

The script is pre-configured with your Supabase credentials:
- **URL:** `https://hbxbltuqmuukcroagztj.supabase.co`
- **Anon Key:** Already set in the script
- **Refresh Interval:** Set to 2 seconds (configurable in Inspector)

You can modify these in the Inspector if needed.

### 4. Key Differences from CSV Version:

**✅ What's the Same:**
- All UI references and settings work the same way
- Highlighting system works identically
- Same scrolling and animation behavior
- Same entry pool management

**🆕 What's New:**
- **Near real-time updates:** New entries appear within 2 seconds across all connected clients
- **Automatic highlighting:** New entries are automatically highlighted when detected
- **No CSV files:** Data is stored in Supabase cloud database
- **No external packages:** Uses only Unity's built-in networking
- **Multi-platform:** Works on any platform that supports Unity networking

### 4. Usage:

**Adding a Score:**
```csharp
// Get reference to your leaderboard script
LeaderBoardTestManual leaderboard = FindObjectOfType<LeaderBoardTestManual>();

// Add a new score (this will automatically update the leaderboard and highlight the new entry)
leaderboard.SubmitScore("PlayerName", 1500);
```

**Manual Refresh:**
```csharp
// Manually refresh the leaderboard
leaderboard.RefreshLeaderboard();
```

### 5. Near Real-time Features:

- **Automatic Updates:** The leaderboard checks for updates every 2 seconds
- **Highlighting:** New entries are automatically highlighted when detected
- **Synchronization:** All clients see the same leaderboard data (with 2-second delay)

### 6. Error Handling:

The script includes comprehensive error handling and logging. Check the Unity Console for detailed information about:
- Connection status
- Data loading progress
- Update events
- Any errors that occur

### 7. Performance Notes:

- The script uses object pooling for UI entries (same as original)
- Updates are optimized to only refresh every 2 seconds
- Database queries are efficient and lightweight
- All operations run on the main thread for smooth UI updates

## Troubleshooting:

**Connection Issues:**
- Verify your Supabase credentials are correct
- Check that Row Level Security policies are set up properly
- Ensure the table name matches exactly: "Manual Leaderboard"

**No Updates:**
- Check Unity Console for connection status messages
- Verify the refresh interval is set appropriately (default: 2 seconds)
- Make sure your internet connection is working

**Performance Issues:**
- Adjust the refresh interval in the Inspector (higher = less frequent updates)
- Large leaderboards (>100 entries) may have slight delays in updates

## API Reference:

### Public Methods:

- `SubmitScore(string playerName, int score)` - Add a new score to the leaderboard
- `RefreshLeaderboard()` - Manually refresh the leaderboard data
- `PopulateLeaderboard()` - Update the UI with current data
- `ShowLeaderboardWithHighlight(string playerName, int score)` - Show leaderboard with specific entry highlighted

### Events:

The script automatically handles database changes and updates the UI accordingly. No manual event handling is required.

## Migration from CSV Version:

1. Replace your `LeaderBoardTest` component with `LeaderBoardTestManual`
2. Configure the same UI references
3. Your existing data will be available in the Supabase database
4. All new scores will be stored in Supabase instead of CSV files

The migration is seamless - all your existing UI and game logic will work without changes!
