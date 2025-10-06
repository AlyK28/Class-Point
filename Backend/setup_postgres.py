#!/usr/bin/env python
"""
Setup script for Class-Point backend with PostgreSQL
"""
import os
import subprocess
import sys
import time


def run_command(command, description):
    """Run a command and handle errors"""
    print(f"\n🔄 {description}...")
    try:
        result = subprocess.run(command, shell=True, check=True, capture_output=True, text=True)
        print(f"✅ {description} completed successfully")
        return True
    except subprocess.CalledProcessError as e:
        print(f"❌ {description} failed: {e}")
        if e.stdout:
            print(f"stdout: {e.stdout}")
        if e.stderr:
            print(f"stderr: {e.stderr}")
        return False


def check_docker():
    """Check if Docker is running"""
    return run_command("docker version", "Checking Docker")


def start_postgres_docker():
    """Start PostgreSQL using Docker"""
    if not check_docker():
        print("❌ Docker is not running. Please start Docker Desktop first.")
        return False
    
    print("\n🚀 Starting PostgreSQL with Docker...")
    success = run_command("docker-compose up -d db", "Starting PostgreSQL container")
    if success:
        print("⏳ Waiting for PostgreSQL to be ready...")
        time.sleep(5)  # Give PostgreSQL time to start
    return success


def run_migrations():
    """Run Django migrations"""
    return run_command("python manage.py migrate", "Running database migrations")


def create_superuser():
    """Optionally create a superuser"""
    response = input("\n🤔 Would you like to create a superuser? (y/n): ")
    if response.lower() in ['y', 'yes']:
        print("📝 Creating superuser...")
        subprocess.run("python manage.py createsuperuser", shell=True)


def main():
    """Main setup function"""
    print("🏗️  Class-Point Backend Setup with PostgreSQL")
    print("=" * 50)
    
    # Check if we're in the right directory
    if not os.path.exists("manage.py"):
        print("❌ Please run this script from the Backend directory")
        sys.exit(1)
    
    # Check if .env exists
    if not os.path.exists(".env"):
        print("❌ .env file not found. Please create one with database configuration.")
        sys.exit(1)
    
    print("✅ Found manage.py and .env file")
    
    # Option to use Docker or local PostgreSQL
    print("\n📋 Setup Options:")
    print("1. Use Docker PostgreSQL (recommended)")
    print("2. Use local PostgreSQL installation")
    
    choice = input("\nChoose option (1 or 2): ")
    
    if choice == "1":
        if not start_postgres_docker():
            print("\n❌ Failed to start PostgreSQL with Docker")
            print("💡 Make sure Docker Desktop is running and try again")
            sys.exit(1)
    elif choice == "2":
        print("\n📝 Using local PostgreSQL. Make sure it's running and configured correctly.")
    else:
        print("❌ Invalid choice")
        sys.exit(1)
    
    # Run migrations
    if not run_migrations():
        print("\n❌ Failed to run migrations")
        print("💡 Make sure PostgreSQL is running and credentials are correct")
        sys.exit(1)
    
    # Create superuser
    create_superuser()
    
    print("\n🎉 Setup completed successfully!")
    print("\n📝 Next steps:")
    print("   1. Run: python manage.py runserver")
    print("   2. Visit: http://localhost:8000")
    print("   3. Admin panel: http://localhost:8000/admin")


if __name__ == "__main__":
    main()