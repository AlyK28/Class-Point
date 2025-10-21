#!/bin/bash

# ClassPoint Image Upload System - Quick Start Script
# This script starts both the Django backend and web frontend

echo "🚀 Starting ClassPoint Image Upload System"
echo "=========================================="

# Function to check if a port is in use
check_port() {
    if lsof -Pi :$1 -sTCP:LISTEN -t >/dev/null ; then
        return 0  # Port is in use
    else
        return 1  # Port is free
    fi
}

# Check if Django backend is already running
if check_port 8000; then
    echo "✅ Django backend is already running on port 8000"
else
    echo "🔧 Starting Django backend..."
    cd Class-Point/Backend
    
    # Check if virtual environment exists
    if [ ! -d "venv" ]; then
        echo "📦 Creating virtual environment..."
        python3 -m venv venv
    fi
    
    # Activate virtual environment
    echo "🔌 Activating virtual environment..."
    source venv/bin/activate
    
    # Install dependencies if needed
    if [ ! -f "venv/pyvenv.cfg" ] || [ ! -d "venv/lib" ]; then
        echo "📥 Installing dependencies..."
        pip install -r requirements.txt
    fi
    
    # Run migrations
    echo "🗄️ Running database migrations..."
    python manage.py migrate
    
    # Start Django server in background
    echo "🚀 Starting Django server on port 8000..."
    python manage.py runserver 8000 &
    DJANGO_PID=$!
    echo "Django PID: $DJANGO_PID"
    
    # Wait a moment for Django to start
    sleep 3
    cd ../..
fi

# Check if web frontend is already running
if check_port 3001; then
    echo "✅ Web frontend is already running on port 3001"
else
    echo "🌐 Starting web frontend..."
    cd Class-Point/Frontend/web-frontend
    
    # Start web server in background
    echo "🚀 Starting web server on port 3001..."
    python3 server.py &
    WEB_PID=$!
    echo "Web server PID: $WEB_PID"
    
    cd ../../..
fi

# Wait a moment for both servers to start
sleep 2

echo ""
echo "🎉 ClassPoint Image Upload System is starting up!"
echo "================================================"
echo ""
echo "📊 System Status:"
echo "   Django Backend:  http://localhost:8000"
echo "   Web Frontend:    http://localhost:3001"
echo ""
echo "🔐 Login Credentials:"
echo "   Username: admin"
echo "   Password: admin123"
echo ""
echo "🎯 Quick Test:"
echo "   python3 test_system.py"
echo ""
echo "🛑 To stop the system:"
echo "   Press Ctrl+C or run: pkill -f 'python.*manage.py runserver'"
echo "   pkill -f 'python.*server.py'"
echo ""

# Keep script running and show status
while true; do
    echo "⏰ $(date '+%H:%M:%S') - System running... (Press Ctrl+C to stop)"
    sleep 30
done
