#!/bin/bash

# UFC Manual Leaderboard - GitHub Setup Script
echo "🚀 Setting up GitHub repository for UFC Manual Leaderboard"
echo ""

# Check if GitHub CLI is installed
if command -v gh &> /dev/null; then
    echo "✅ GitHub CLI found"
    
    # Create repository on GitHub
    echo "📦 Creating repository on GitHub..."
    gh repo create UFCManualLeaderboard --public --description "UFC Manual Leaderboard with Supabase integration, mobile-responsive design, and Unity support" --source=. --remote=origin --push
    
    if [ $? -eq 0 ]; then
        echo "✅ Repository created successfully!"
        echo "🌐 Your repository is available at: https://github.com/$(gh api user --jq .login)/UFCManualLeaderboard"
    else
        echo "❌ Failed to create repository with GitHub CLI"
        echo "📝 Please create the repository manually on GitHub and then run:"
        echo "   git remote add origin https://github.com/YOUR_USERNAME/UFCManualLeaderboard.git"
        echo "   git push -u origin main"
    fi
else
    echo "❌ GitHub CLI not found"
    echo "📝 Please follow these steps manually:"
    echo ""
    echo "1. Go to https://github.com/new"
    echo "2. Create a new repository named 'UFCManualLeaderboard'"
    echo "3. Make it public"
    echo "4. Don't initialize with README (we already have one)"
    echo "5. Copy the repository URL and run:"
    echo "   git remote add origin https://github.com/YOUR_USERNAME/UFCManualLeaderboard.git"
    echo "   git push -u origin main"
    echo ""
    echo "🔗 Repository description: 'UFC Manual Leaderboard with Supabase integration, mobile-responsive design, and Unity support'"
fi

echo ""
echo "🎯 Next steps for Vercel deployment:"
echo "1. Go to https://vercel.com"
echo "2. Sign in with GitHub"
echo "3. Click 'New Project'"
echo "4. Import your 'UFCManualLeaderboard' repository"
echo "5. Deploy with default settings"
echo ""
echo "🎉 Your UFC Leaderboard will be live on Vercel!"
