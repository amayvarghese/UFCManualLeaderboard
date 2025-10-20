# UFC Manual Leaderboard

A beautiful, modern web application for tracking UFC scores with real-time leaderboard updates powered by Supabase.

## Features

- 🎯 Submit name and score entries
- 🏆 Real-time leaderboard with rankings
- 🥇 Special styling for top 3 positions
- ⚡ Instant updates across all connected clients
- 📱 Fully responsive design
- 🎨 Modern, gradient UI with smooth animations

## Setup Instructions

### 1. Set up Supabase

1. Go to [Supabase](https://supabase.com) and create a free account
2. Create a new project
3. Wait for the project to be set up (this may take a few minutes)

### 2. Create the Database Table

1. In your Supabase project dashboard, go to the **SQL Editor**
2. Run the following SQL command to create the leaderboard table:

```sql
-- Create the leaderboard table
CREATE TABLE leaderboard (
  id BIGSERIAL PRIMARY KEY,
  name TEXT NOT NULL,
  score INTEGER NOT NULL,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL
);

-- Enable Row Level Security
ALTER TABLE leaderboard ENABLE ROW LEVEL SECURITY;

-- Create a policy that allows everyone to read
CREATE POLICY "Allow public read access" ON leaderboard
  FOR SELECT
  USING (true);

-- Create a policy that allows everyone to insert
CREATE POLICY "Allow public insert access" ON leaderboard
  FOR INSERT
  WITH CHECK (true);

-- Create an index for faster sorting
CREATE INDEX idx_leaderboard_score ON leaderboard(score DESC);
```

### 3. Enable Realtime (Optional but Recommended)

1. In Supabase dashboard, go to **Database** → **Replication**
2. Find the `leaderboard` table
3. Enable replication for real-time updates

### 4. Get Your Supabase Credentials

1. In your Supabase project dashboard, go to **Settings** → **API**
2. Copy the following:
   - **Project URL** (e.g., `https://abcdefghijk.supabase.co`)
   - **Anon/Public Key** (starts with `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`)

### 5. Configure the Application

1. Open `config.js` in this project
2. Replace the placeholder values with your actual Supabase credentials:

```javascript
const SUPABASE_URL = 'YOUR_SUPABASE_URL';
const SUPABASE_ANON_KEY = 'YOUR_SUPABASE_ANON_KEY';
```

### 6. Run the Application

Since this is a static web application, you can run it in several ways:

**Option 1: Using Python's built-in server**
```bash
python3 -m http.server 8000
```
Then open `http://localhost:8000` in your browser.

**Option 2: Using Node.js http-server**
```bash
npx http-server -p 8000
```
Then open `http://localhost:8000` in your browser.

**Option 3: Using VS Code Live Server**
- Install the "Live Server" extension
- Right-click on `index.html`
- Select "Open with Live Server"

**Option 4: Direct file access**
- Simply open `index.html` in your web browser (may have CORS limitations)

## Usage

1. Enter your name in the "Name" field
2. Enter your score in the "Score" field
3. Click "Submit Score"
4. Your score will be added to the leaderboard
5. The leaderboard automatically updates in real-time
6. Top 3 scores are highlighted with medals 🥇🥈🥉

## Security Notes

- The current setup allows anyone to read and insert data (public access)
- For production use, consider implementing proper authentication
- You can modify the Row Level Security policies in Supabase for more control

## Customization

### Changing Colors
Edit `style.css` and modify the gradient values:
```css
background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
```

### Modifying Table Name
If you want to use a different table name, update it in:
- SQL script (in README)
- `app.js` (all `.from('leaderboard')` calls)

## Troubleshooting

**Issue: "Failed to fetch" or connection errors**
- Verify your Supabase URL and Anon Key are correct
- Check that your Supabase project is active
- Ensure you created the table with the SQL script

**Issue: Data not showing up**
- Check browser console for errors
- Verify Row Level Security policies are set correctly
- Make sure the table name matches in your code and database

**Issue: Real-time updates not working**
- Enable replication for the `leaderboard` table in Supabase
- Check browser console for subscription errors

## Technologies Used

- HTML5
- CSS3 (with modern gradients and animations)
- Vanilla JavaScript (ES6+)
- Supabase (Backend & Real-time Database)

## License

MIT License - Feel free to use this project for any purpose!

