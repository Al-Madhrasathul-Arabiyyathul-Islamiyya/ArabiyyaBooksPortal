#!/bin/bash
# Start Books Portal - Backend + Frontend Development Servers
# Usage: ./start-dev.sh

echo -e "\033[0;36mStarting Books Portal Development Environment...\033[0m"
echo ""

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Start Backend API in new terminal
echo -e "\033[0;32m[Backend] Starting ASP.NET Core API...\033[0m"
if command -v gnome-terminal &> /dev/null; then
    gnome-terminal -- bash -c "cd '$SCRIPT_DIR/BooksPortal/src/BooksPortal.API'; echo 'Backend API Server'; dotnet run; exec bash"
elif command -v xterm &> /dev/null; then
    xterm -e "cd '$SCRIPT_DIR/BooksPortal/src/BooksPortal.API'; echo 'Backend API Server'; dotnet run; bash" &
else
    # Fallback: run in background
    cd "$SCRIPT_DIR/BooksPortal/src/BooksPortal.API"
    dotnet run > backend.log 2>&1 &
    BACKEND_PID=$!
    echo "Backend started in background (PID: $BACKEND_PID, log: backend.log)"
    cd "$SCRIPT_DIR"
fi

# Wait for backend to initialize
sleep 2

# Start Frontend in new terminal
echo -e "\033[0;34m[Frontend] Starting Nuxt Dev Server...\033[0m"
if command -v gnome-terminal &> /dev/null; then
    gnome-terminal -- bash -c "cd '$SCRIPT_DIR/BooksPortalFrontEnd'; echo 'Frontend Dev Server'; bun run dev; exec bash"
elif command -v xterm &> /dev/null; then
    xterm -e "cd '$SCRIPT_DIR/BooksPortalFrontEnd'; echo 'Frontend Dev Server'; bun run dev; bash" &
else
    # Fallback: run in background
    cd "$SCRIPT_DIR/BooksPortalFrontEnd"
    bun run dev > ../frontend.log 2>&1 &
    FRONTEND_PID=$!
    echo "Frontend started in background (PID: $FRONTEND_PID, log: frontend.log)"
    cd "$SCRIPT_DIR"
fi

echo ""
echo -e "\033[0;36mDevelopment servers started!\033[0m"
echo -e "  \033[0;32mBackend:  https://localhost:5001/api\033[0m"
echo -e "  \033[0;34mFrontend: http://localhost:3000\033[0m"
echo ""
echo -e "\033[0;33mPress Ctrl+C in each terminal window to stop the servers.\033[0m"
echo ""
