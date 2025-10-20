// Initialize Supabase client
const supabase = window.supabase.createClient(SUPABASE_URL, SUPABASE_ANON_KEY);

// DOM Elements
const scoreForm = document.getElementById('scoreForm');
const messageDiv = document.getElementById('message');
const leaderboardDiv = document.getElementById('leaderboard');
const refreshBtn = document.getElementById('refreshBtn');
const resetBtn = document.getElementById('resetBtn');

// Show message to user
function showMessage(text, type = 'success') {
    messageDiv.textContent = text;
    messageDiv.className = `message ${type}`;
    messageDiv.style.display = 'block';
    
    setTimeout(() => {
        messageDiv.style.display = 'none';
    }, 5000);
}

// Submit score
async function submitScore(e) {
    e.preventDefault();
    
    const name = document.getElementById('name').value.trim();
    const score = parseInt(document.getElementById('score').value);
    
    if (!name || isNaN(score)) {
        showMessage('Please enter valid name and score', 'error');
        return;
    }
    
    try {
        const { data, error } = await supabase
            .from('Manual Leaderboard')
            .insert([
                { name: name, score: score }
            ])
            .select();
        
        if (error) throw error;
        
        showMessage(`Score submitted successfully for ${name}!`, 'success');
        scoreForm.reset();
        
        // Refresh leaderboard
        loadLeaderboard();
    } catch (error) {
        console.error('Error submitting score:', error);
        showMessage(`Error: ${error.message}`, 'error');
    }
}

// Load and display leaderboard
async function loadLeaderboard() {
    leaderboardDiv.innerHTML = '<p class="loading">Loading leaderboard...</p>';
    
    try {
        const { data, error } = await supabase
            .from('Manual Leaderboard')
            .select('*')
            .order('score', { ascending: false })
            .order('created_at', { ascending: true });
        
        if (error) throw error;
        
        if (!data || data.length === 0) {
            leaderboardDiv.innerHTML = '<p class="no-data">No scores yet. Be the first!</p>';
            return;
        }
        
        let html = '';
        data.forEach((entry, index) => {
            const rank = index + 1;
            const topClass = rank <= 3 ? 'top-3' : '';
            const medal = rank === 1 ? '🥇' : rank === 2 ? '🥈' : rank === 3 ? '🥉' : '';
            
            html += `
                <div class="leaderboard-item ${topClass}">
                    <span class="leaderboard-rank">${medal} #${rank}</span>
                    <span class="leaderboard-name">${escapeHtml(entry.name)}</span>
                    <span class="leaderboard-score">${entry.score}</span>
                </div>
            `;
        });
        
        leaderboardDiv.innerHTML = html;
    } catch (error) {
        console.error('Error loading leaderboard:', error);
        leaderboardDiv.innerHTML = `<p class="no-data">Error loading leaderboard: ${error.message}</p>`;
    }
}

// Reset all leaderboard data
async function resetLeaderboard() {
    // Show confirmation dialog
    const confirmed = confirm('⚠️ WARNING: This will permanently delete ALL leaderboard data!\n\nAre you absolutely sure you want to reset the leaderboard?\n\nThis action cannot be undone.');
    
    if (!confirmed) {
        return;
    }
    
    // Show second confirmation for extra safety
    const doubleConfirmed = confirm('🚨 FINAL WARNING: All scores and data will be permanently deleted!\n\nType "RESET" in the next dialog to confirm.');
    
    if (!doubleConfirmed) {
        return;
    }
    
    // Final confirmation with text input
    const resetText = prompt('Type "RESET" (in all caps) to confirm deletion of all leaderboard data:');
    
    if (resetText !== 'RESET') {
        showMessage('Reset cancelled. Leaderboard data is safe.', 'error');
        return;
    }
    
    // Disable button and show loading
    resetBtn.disabled = true;
    resetBtn.textContent = 'Resetting...';
    showMessage('Resetting leaderboard...', 'error');
    
    try {
        // Delete all entries from the leaderboard
        const { error } = await supabase
            .from('Manual Leaderboard')
            .delete()
            .neq('id', 0); // This will delete all rows since no ID is 0
        
        if (error) throw error;
        
        showMessage('✅ Leaderboard has been reset successfully!', 'success');
        
        // Reload the leaderboard to show empty state
        await loadLeaderboard();
        
    } catch (error) {
        console.error('Error resetting leaderboard:', error);
        showMessage(`❌ Error resetting leaderboard: ${error.message}`, 'error');
    } finally {
        // Re-enable button
        resetBtn.disabled = false;
        resetBtn.textContent = 'Reset All Data';
    }
}

// Escape HTML to prevent XSS
function escapeHtml(text) {
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.replace(/[&<>"']/g, m => map[m]);
}

// Event listeners
scoreForm.addEventListener('submit', submitScore);
refreshBtn.addEventListener('click', loadLeaderboard);
resetBtn.addEventListener('click', resetLeaderboard);

// Load leaderboard on page load
loadLeaderboard();

// Set up real-time updates
const channel = supabase
    .channel('leaderboard-changes')
    .on('postgres_changes', 
        { 
            event: '*', 
            schema: 'public', 
            table: 'Manual Leaderboard' 
        }, 
        (payload) => {
            console.log('Leaderboard updated:', payload);
            loadLeaderboard();
        }
    )
    .subscribe();

