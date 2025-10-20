using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;

[System.Serializable]
public class LeaderboardEntry
{
    public long id;
    public string name;
    public int score;
    public string created_at;

    public LeaderboardEntry(string name, int score)
    {
        this.name = name;
        this.score = score;
    }
}

[System.Serializable]
public class SupabaseResponse
{
    public LeaderboardEntry[] data;
}

[System.Serializable]
public class LeaderboardEntryWrapper
{
    public LeaderboardEntry[] data;
}

public class LeaderBoardTestManual : MonoBehaviour
{
    [Header("Supabase Configuration")]
    [SerializeField] private string supabaseUrl = "https://hbxbltuqmuukcroagztj.supabase.co";
    [SerializeField] private string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhieGJsdHVxbXV1a2Nyb2FnenRqIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjA4OTI0MTIsImV4cCI6MjA3NjQ2ODQxMn0.FVBw6I53IXIN2CrMmzLTsVtJgZOdr6man_lmfyVmuzk";
    [SerializeField] private float refreshInterval = 2f; // How often to check for updates (seconds)

    [Header("UI References")]
    [SerializeField] private PlayerEntryObjectTest entryObject; // inactive template
    [SerializeField] private Transform contentParent;
    [SerializeField] private Vector2 whenReloadPosContent;

    [Header("Settings")]
    [SerializeField] private int maxEntries = 20;
    [SerializeField] private float verticalSpacing = 50f;
    [SerializeField] private string placeholderText = "-------";

    [Header("Highlight Settings")]
    [SerializeField] private Color highlightColor = Color.yellow; // color for last player
    [SerializeField] private float highlightDuration = 3f; // seconds to show temporary highlight

    [Header("UI Controls")]
    [SerializeField] Button Registerbutton;
    [SerializeField] string levelToLoadName;

    private List<PlayerEntryObjectTest> entriesPool = new List<PlayerEntryObjectTest>();
    private Coroutine highlightCoroutine;
    private Coroutine refreshCoroutine;
    private int lastVisibleCount = 0;
    private List<LeaderboardEntry> currentLeaderboardData = new List<LeaderboardEntry>();
    private bool isInitialized = false;
    private int lastDataCount = 0;

    // Track the last added entry for highlighting
    private LeaderboardEntry lastAddedEntry = null;
    private bool shouldHighlightNewEntry = false;

    private void Start()
    {
        Debug.Log("LeaderBoardTestManual.Start() - Beginning initialization");
        InitializeEntriesPool();
        StartCoroutine(InitializeAndStartRefresh());
        Debug.Log("LeaderBoardTestManual.Start() - Initialization complete");
    }

    private IEnumerator InitializeAndStartRefresh()
    {
        Debug.Log("LeaderBoardTestManual.InitializeAndStartRefresh() - Starting initialization");
        
        // Load initial data
        yield return StartCoroutine(LoadLeaderboardData());
        
        isInitialized = true;
        Debug.Log("LeaderBoardTestManual.InitializeAndStartRefresh() - Initialization complete");
        
        // Start automatic refresh
        refreshCoroutine = StartCoroutine(AutoRefreshCoroutine());
    }

    private IEnumerator AutoRefreshCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(refreshInterval);
            
            if (isInitialized)
            {
                yield return StartCoroutine(LoadLeaderboardData());
            }
        }
    }

    private IEnumerator LoadLeaderboardData()
    {
        Debug.Log("LeaderBoardTestManual.LoadLeaderboardData() - Loading data from Supabase");
        
        // Build the REST API URL
        string url = $"{supabaseUrl}/rest/v1/Manual%20Leaderboard?select=*&order=score.desc,created_at.asc";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Add headers for Supabase authentication
            request.SetRequestHeader("apikey", supabaseAnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {supabaseAnonKey}");
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Prefer", "return=minimal");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"LeaderBoardTestManual.LoadLeaderboardData() - Response: {request.downloadHandler.text}");
                
                try
                {
                    // Parse the JSON response
                    string jsonData = request.downloadHandler.text;
                    LeaderboardEntryWrapper wrapper = JsonUtility.FromJson<LeaderboardEntryWrapper>("{\"data\":" + jsonData + "}");
                    
                    if (wrapper != null && wrapper.data != null)
                    {
                        currentLeaderboardData = wrapper.data.ToList();
                        Debug.Log($"LeaderBoardTestManual.LoadLeaderboardData() - Loaded {currentLeaderboardData.Count} entries");
                        
                        // Check if we have new entries (for highlighting)
                        if (currentLeaderboardData.Count > lastDataCount && lastDataCount > 0)
                        {
                            // Find the newest entry (last in the list since it's sorted by created_at)
                            var newestEntry = currentLeaderboardData.OrderByDescending(e => e.created_at).FirstOrDefault();
                            if (newestEntry != null)
                            {
                                shouldHighlightNewEntry = true;
                                lastAddedEntry = newestEntry;
                                Debug.Log($"LeaderBoardTestManual.LoadLeaderboardData() - New entry detected: {newestEntry.name} - {newestEntry.score}");
                            }
                        }
                        
                        lastDataCount = currentLeaderboardData.Count;
                        
                        // Update UI
                        if (shouldHighlightNewEntry && lastAddedEntry != null)
                        {
                            ShowLeaderboardWithHighlight(lastAddedEntry.name, lastAddedEntry.score);
                            shouldHighlightNewEntry = false;
                            lastAddedEntry = null;
                        }
                        else
                        {
                            PopulateLeaderboard();
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"LeaderBoardTestManual.LoadLeaderboardData() - JSON parsing error: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"LeaderBoardTestManual.LoadLeaderboardData() - Request failed: {request.error}");
            }
        }
    }

    public IEnumerator AddScore(string playerName, int score)
    {
        Debug.Log($"LeaderBoardTestManual.AddScore() - Adding score for {playerName}: {score}");
        
        // Build the REST API URL for POST request
        string url = $"{supabaseUrl}/rest/v1/Manual%20Leaderboard";
        
        // Create the data to send
        LeaderboardEntry newEntry = new LeaderboardEntry(playerName, score);
        string jsonData = JsonUtility.ToJson(newEntry);
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            // Add headers for Supabase authentication
            request.SetRequestHeader("apikey", supabaseAnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {supabaseAnonKey}");
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Prefer", "return=minimal");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"LeaderBoardTestManual.AddScore() - Score added successfully for {playerName}");
                
                // Immediately refresh to show the new entry
                yield return StartCoroutine(LoadLeaderboardData());
            }
            else
            {
                Debug.LogError($"LeaderBoardTestManual.AddScore() - Request failed: {request.error}");
            }
        }
    }

    private void InitializeEntriesPool()
    {
        Debug.Log("LeaderBoardTestManual.InitializeEntriesPool() - Initializing pool");
        int poolSize = maxEntries + 1;
        Debug.Log($"LeaderBoardTestManual.InitializeEntriesPool() - Pool size: {poolSize}");
        
        for (int i = 0; i < poolSize; i++)
        {
            Debug.Log($"LeaderBoardTestManual.InitializeEntriesPool() - Instantiating entry {i + 1}/{poolSize}");
            PlayerEntryObjectTest newEntry = Instantiate(entryObject, contentParent);
            newEntry.gameObject.SetActive(false);
            entriesPool.Add(newEntry);
        }
        Debug.Log("LeaderBoardTestManual.InitializeEntriesPool() - All entries instantiated");

        entryObject.gameObject.SetActive(false);
        Debug.Log("LeaderBoardTestManual.InitializeEntriesPool() - Template deactivated");
    }

    public void PopulateLeaderboard()
    {
        Debug.Log("LeaderBoardTestManual.PopulateLeaderboard() - Starting population");
        
        if (currentLeaderboardData == null || currentLeaderboardData.Count == 0)
        {
            Debug.Log("LeaderBoardTestManual.PopulateLeaderboard() - No data available");
            return;
        }

        var displayEntries = new List<(string Name, int Score, string RankText)>();
        Debug.Log($"LeaderBoardTestManual.PopulateLeaderboard() - Building display entries for top {maxEntries}");

        int placeholderCount = 0;
        for (int i = 0; i < maxEntries; i++)
        {
            if (i < currentLeaderboardData.Count)
            {
                string playerName = currentLeaderboardData[i].name;
                int playerScore = currentLeaderboardData[i].score;
                string rankText = $"#{i + 1}";

                displayEntries.Add((playerName, playerScore, rankText));
                Debug.Log($"LeaderBoardTestManual.PopulateLeaderboard() - Added entry {i + 1}: {playerName} - {playerScore}");
            }
            else
            {
                displayEntries.Add((placeholderText, 0, "-"));
                placeholderCount++;
            }
        }
        Debug.Log($"LeaderBoardTestManual.PopulateLeaderboard() - Added {placeholderCount} placeholders");

        UpdateEntries(displayEntries, maxEntries);
        Debug.Log("LeaderBoardTestManual.PopulateLeaderboard() - Entries updated");
    }

    private void UpdateEntries(List<(string Name, int Score, string RankText)> displayEntries, int visibleCount)
    {
        Debug.Log($"LeaderBoardTestManual.UpdateEntries() - Updating {visibleCount} entries (last was {lastVisibleCount})");

        // Optimize: Only deactivate extras if count changed
        if (visibleCount < lastVisibleCount)
        {
            Debug.Log("LeaderBoardTestManual.UpdateEntries() - Deactivating extras");
            for (int i = visibleCount; i < lastVisibleCount; i++)
            {
                if (i < entriesPool.Count)
                {
                    entriesPool[i].gameObject.SetActive(false);
                    entriesPool[i].ResetColorToDefault();
                }
            }
        }
        else if (visibleCount > lastVisibleCount)
        {
            Debug.Log("LeaderBoardTestManual.UpdateEntries() - Reactivating extras");
        }

        // Always reset and set the visible ones (avoids full deactivate flash)
        for (int i = 0; i < visibleCount && i < displayEntries.Count; i++)
        {
            var entryData = displayEntries[i];
            if (i >= entriesPool.Count) break; // Safety

            // Reset color first
            entriesPool[i].ResetColorToDefault();

            // Set values
            Debug.Log($"LeaderBoardTestManual.UpdateEntries() - Setting entry {i}: {entryData.Name} - {entryData.Score} - {entryData.RankText}");
            entriesPool[i].SetValues(entryData.Name, entryData.Score.ToString(), entryData.RankText);

            // Ensure active
            entriesPool[i].gameObject.SetActive(true);
        }

        lastVisibleCount = visibleCount;
        Debug.Log($"LeaderBoardTestManual.UpdateEntries() - Activated/updated {visibleCount} entries");

        if (contentParent.TryGetComponent<RectTransform>(out var rt))
        {
            Debug.Log("LeaderBoardTestManual.UpdateEntries() - Forcing layout rebuild");
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
        else
        {
            Debug.LogWarning("LeaderBoardTestManual.UpdateEntries() - No RectTransform found on contentParent");
        }
        Debug.Log("LeaderBoardTestManual.UpdateEntries() - Update complete");
    }

    public void ShowLeaderboardWithHighlight(string playerName, int playerScore)
    {
        Debug.Log($"LeaderBoardTestManual.ShowLeaderboardWithHighlight() - Showing highlight for {playerName} with score {playerScore}");
        
        if (highlightCoroutine != null)
        {
            Debug.Log("LeaderBoardTestManual.ShowLeaderboardWithHighlight() - Stopping previous coroutine");
            StopCoroutine(highlightCoroutine);
        }

        if (currentLeaderboardData == null || currentLeaderboardData.Count == 0)
        {
            Debug.LogError("LeaderBoardTestManual.ShowLeaderboardWithHighlight() - No leaderboard data available!");
            PopulateLeaderboard();
            return;
        }

        Debug.Log($"LeaderBoardTestManual.ShowLeaderboardWithHighlight() - Found {currentLeaderboardData.Count} entries");

        int playerIndex = currentLeaderboardData.FindIndex(e => e.name == playerName && e.score == playerScore);
        Debug.Log($"LeaderBoardTestManual.ShowLeaderboardWithHighlight() - Player index: {playerIndex}");
        
        if (playerIndex == -1)
        {
            Debug.LogWarning($"LeaderBoardTestManual.ShowLeaderboardWithHighlight() - Player '{playerName}' with score {playerScore} not found in leaderboard.");
            PopulateLeaderboard();
            return;
        }

        int actualRank = playerIndex + 1;
        bool inTopMax = playerIndex < maxEntries;
        Debug.Log($"LeaderBoardTestManual.ShowLeaderboardWithHighlight() - Actual rank: {actualRank}, in top {maxEntries}: {inTopMax}");

        var topDisplayEntries = new List<(string Name, int Score, string RankText)>();
        Debug.Log("LeaderBoardTestManual.ShowLeaderboardWithHighlight() - Building top display entries");

        int placeholderCount = 0;
        for (int i = 0; i < maxEntries; i++)
        {
            if (i < currentLeaderboardData.Count)
            {
                string pName = currentLeaderboardData[i].name;
                int pScore = currentLeaderboardData[i].score;
                string rankText = $"#{i + 1}";

                topDisplayEntries.Add((pName, pScore, rankText));
                Debug.Log($"LeaderBoardTestManual.ShowLeaderboardWithHighlight() - Added top entry {i + 1}: {pName} - {pScore}");
            }
            else
            {
                topDisplayEntries.Add((placeholderText, 0, "-"));
                placeholderCount++;
            }
        }
        Debug.Log($"LeaderBoardTestManual.ShowLeaderboardWithHighlight() - Added {placeholderCount} placeholders to top");

        int visibleCount = maxEntries;
        bool showExtra = !inTopMax;
        int highlightedEntryIndex = playerIndex; // Track which entry to highlight

        if (showExtra)
        {
            string playerRankText = $"#{actualRank}";
            topDisplayEntries.Add((playerName, playerScore, playerRankText));
            visibleCount = maxEntries + 1;
            highlightedEntryIndex = maxEntries; // It's the last entry
            Debug.Log($"LeaderBoardTestManual.ShowLeaderboardWithHighlight() - Added extra entry for player at rank {actualRank}");
        }

        UpdateEntries(topDisplayEntries, visibleCount);

        Debug.Log("LeaderBoardTestManual.ShowLeaderboardWithHighlight() - Applying highlight color");
        bool highlighted = false;
        foreach (var entry in entriesPool)
        {
            if (entry.nameText != null && entry.nameText.text == playerName)
            {
                entry.SetColor(highlightColor);
                highlighted = true;
                Debug.Log($"LeaderBoardTestManual.ShowLeaderboardWithHighlight() - Highlighted entry for {playerName}");
                break;
            }
        }
        if (!highlighted)
        {
            Debug.LogWarning("LeaderBoardTestManual.ShowLeaderboardWithHighlight() - Could not find entry to highlight");
        }

        // Scroll to the highlighted entry
        ScrollToEntry(highlightedEntryIndex);

        Debug.Log("LeaderBoardTestManual.ShowLeaderboardWithHighlight() - Hiding register button");
        if (Registerbutton != null)
            Registerbutton.gameObject.SetActive(false);

        // Ensure GameObject is active before starting coroutine
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("LeaderBoardTestManual.ShowLeaderboardWithHighlight() - GameObject was inactive, activating now");
            gameObject.SetActive(true);
        }

        highlightCoroutine = StartCoroutine(ResetLeaderboardAfterDelay(10f));
        Debug.Log("LeaderBoardTestManual.ShowLeaderboardWithHighlight() - Started reset coroutine");
    }

    private void ScrollToEntry(int entryIndex)
    {
        Debug.Log($"LeaderBoardTestManual.ScrollToEntry() - Scrolling to entry at index {entryIndex}");

        // Get the ScrollRect component (should be on the parent of contentParent)
        ScrollRect scrollRect = contentParent.GetComponentInParent<ScrollRect>();
        if (scrollRect == null)
        {
            Debug.LogWarning("LeaderBoardTestManual.ScrollToEntry() - No ScrollRect found!");
            return;
        }

        // Force layout update to ensure positions are calculated
        Canvas.ForceUpdateCanvases();

        if (entryIndex >= entriesPool.Count || entryIndex < 0)
        {
            Debug.LogWarning($"LeaderBoardTestManual.ScrollToEntry() - Invalid entry index: {entryIndex}");
            return;
        }

        RectTransform targetEntry = entriesPool[entryIndex].GetComponent<RectTransform>();
        RectTransform contentRect = contentParent.GetComponent<RectTransform>();
        RectTransform viewportRect = scrollRect.viewport;

        if (targetEntry == null || contentRect == null || viewportRect == null)
        {
            Debug.LogWarning("LeaderBoardTestManual.ScrollToEntry() - Missing required RectTransforms");
            return;
        }

        // Calculate the position of the target entry relative to content
        float entryPosition = -targetEntry.anchoredPosition.y;

        // Calculate the viewport height and content height
        float viewportHeight = viewportRect.rect.height;
        float contentHeight = contentRect.rect.height;

        // Calculate normalized position (0 = top, 1 = bottom)
        // We want to center the entry in the viewport
        float targetNormalizedPosition = 1f - ((entryPosition - (viewportHeight * 0.5f)) / (contentHeight - viewportHeight));

        // Clamp between 0 and 1
        targetNormalizedPosition = Mathf.Clamp01(targetNormalizedPosition);

        // Smooth scroll to position
        scrollRect.verticalNormalizedPosition = targetNormalizedPosition;

        Debug.Log($"LeaderBoardTestManual.ScrollToEntry() - Scrolled to normalized position: {targetNormalizedPosition}");
    }

    private IEnumerator ResetLeaderboardAfterDelay(float delay)
    {
        Debug.Log($"LeaderBoardTestManual.ResetLeaderboardAfterDelay() - Waiting {delay} seconds before reset");
        yield return new WaitForSeconds(delay);
        Debug.Log("LeaderBoardTestManual.ResetLeaderboardAfterDelay() - Delay complete, resetting leaderboard");
        PopulateLeaderboard();
        Debug.Log($"LeaderBoardTestManual.ResetLeaderboardAfterDelay() - Loading scene: {levelToLoadName}");
        SceneManager.LoadScene(levelToLoadName);
    }

    private void OnDestroy()
    {
        if (refreshCoroutine != null)
        {
            StopCoroutine(refreshCoroutine);
        }
        
        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
        }
    }

    // Public method to manually refresh leaderboard (can be called from UI button)
    public void RefreshLeaderboard()
    {
        if (isInitialized)
        {
            StartCoroutine(LoadLeaderboardData());
        }
    }

    // Public method to add a new score (can be called from game logic)
    public void SubmitScore(string playerName, int score)
    {
        if (isInitialized)
        {
            StartCoroutine(AddScore(playerName, score));
        }
    }
}
