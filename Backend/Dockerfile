# Use a lightweight Python image
FROM python:3.11-slim

# Prevent Python from writing .pyc files and buffering stdout
ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1

# Set the working directory inside the container
WORKDIR /app

# Install system dependencies (PostgreSQL client & build tools)
RUN apt-get update && apt-get install -y libpq-dev && rm -rf /var/lib/apt/lists/*


# Copy requirements and install Python dependencies
COPY requirements.txt .
RUN pip install --upgrade pip && pip install --no-cache-dir -r requirements.txt

# Copy the rest of your Django project
COPY . .

# Collect static files (optional — only if your Django app uses STATIC_ROOT)
# RUN python manage.py collectstatic --noinput

# Expose the port Django/Gunicorn will run on
EXPOSE 8000

# Start Gunicorn
CMD ["gunicorn", "classpoint.wsgi:application", "--bind", "0.0.0.0:8000"]
