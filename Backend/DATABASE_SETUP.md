# Class-Point Backend Database Setup

## PostgreSQL Configuration

This project has been configured to use PostgreSQL instead of SQLite. Here are the setup instructions:

### Option 1: Using Docker (Recommended)

1. Make sure you have Docker and Docker Compose installed
2. Navigate to the Backend directory:
   ```bash
   cd Backend
   ```
3. Start the PostgreSQL database:
   ```bash
   docker-compose up -d db
   ```
4. Install Python dependencies:
   ```bash
   pip install -r requirements.txt
   ```
5. Run database migrations:
   ```bash
   python manage.py migrate
   ```
6. Create a superuser (optional):
   ```bash
   python manage.py createsuperuser
   ```
7. Start the Django development server:
   ```bash
   python manage.py runserver
   ```

### Option 2: Using Local PostgreSQL Installation

1. Install PostgreSQL on your system
2. Create a database and user:
   ```sql
   CREATE DATABASE classpoint;
   CREATE USER classpoint WITH PASSWORD 'secret';
   GRANT ALL PRIVILEGES ON DATABASE classpoint TO classpoint;
   ```
3. Update the `.env` file with your database credentials if different from defaults
4. Install Python dependencies:
   ```bash
   pip install -r requirements.txt
   ```
5. Run database migrations:
   ```bash
   python manage.py migrate
   ```
6. Start the Django development server:
   ```bash
   python manage.py runserver
   ```

### Environment Variables

The following environment variables can be configured in the `.env` file:

- `DATABASE_NAME`: Database name (default: classpoint)
- `DATABASE_USER`: Database user (default: classpoint)
- `DATABASE_PASSWORD`: Database password (default: secret)
- `DATABASE_HOST`: Database host (default: localhost)
- `DATABASE_PORT`: Database port (default: 5432)
- `DATABASE_URL`: Complete database URL (optional, overrides individual settings)
- `DEBUG`: Debug mode (default: True)
- `SECRET_KEY`: Django secret key

### Using the Full Stack with Docker

To run both the database and web application:

```bash
docker-compose up
```

This will start both PostgreSQL and the Django application.

### Migration from SQLite

If you had existing data in SQLite and want to migrate it:

1. Create a database dump from SQLite:
   ```bash
   python manage.py dumpdata > data.json
   ```
2. Set up PostgreSQL as described above
3. Load the data into PostgreSQL:
   ```bash
   python manage.py loaddata data.json
   ```

## Troubleshooting

- Make sure PostgreSQL is running before starting Django
- Check that the database credentials in `.env` match your PostgreSQL setup
- Ensure `psycopg2-binary` is installed (`pip install psycopg2-binary`)