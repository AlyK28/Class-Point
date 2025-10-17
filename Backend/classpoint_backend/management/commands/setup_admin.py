from django.core.management.base import BaseCommand
from django.contrib.auth.models import User
from django.contrib.auth.hashers import make_password


class Command(BaseCommand):
    help = 'Set up Django admin with a superuser account'

    def add_arguments(self, parser):
        parser.add_argument(
            '--username',
            type=str,
            default='admin',
            help='Admin username (default: admin)'
        )
        parser.add_argument(
            '--email',
            type=str,
            default='admin@classpoint.com',
            help='Admin email (default: admin@classpoint.com)'
        )
        parser.add_argument(
            '--password',
            type=str,
            default='admin123',
            help='Admin password (default: admin123)'
        )
        parser.add_argument(
            '--force',
            action='store_true',
            help='Force creation even if superuser exists'
        )

    def handle(self, *args, **options):
        username = options['username']
        email = options['email']
        password = options['password']
        force = options['force']

        # Check if superuser already exists
        if User.objects.filter(is_superuser=True).exists() and not force:
            self.stdout.write(
                self.style.WARNING('Superuser already exists! Use --force to overwrite.')
            )
            existing_superusers = User.objects.filter(is_superuser=True)
            for user in existing_superusers:
                self.stdout.write(f"  - {user.username} ({user.email})")
            return

        # Create or update superuser
        if force and User.objects.filter(username=username).exists():
            user = User.objects.get(username=username)
            user.email = email
            user.set_password(password)
            user.is_superuser = True
            user.is_staff = True
            user.is_active = True
            user.save()
            self.stdout.write(
                self.style.SUCCESS(f'✅ Updated superuser: {username}')
            )
        else:
            user = User.objects.create_superuser(
                username=username,
                email=email,
                password=password
            )
            self.stdout.write(
                self.style.SUCCESS(f'✅ Created superuser: {username}')
            )

        self.stdout.write(
            self.style.SUCCESS(f'   Username: {username}')
        )
        self.stdout.write(
            self.style.SUCCESS(f'   Email: {email}')
        )
        self.stdout.write(
            self.style.SUCCESS(f'   Password: {password}')
        )
        self.stdout.write(
            self.style.SUCCESS(f'\n🌐 Access Django Admin at: http://localhost:8000/admin/')
        )
