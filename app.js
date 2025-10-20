// Initialize Supabase client
const supabase = window.supabase.createClient(SUPABASE_URL, SUPABASE_ANON_KEY);

// DOM Elements
const scoreForm = document.getElementById('scoreForm');
const messageDiv = document.getElementById('message');
const leaderboardDiv = document.getElementById('leaderboard');
const refreshBtn = document.getElementById('refreshBtn');

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

