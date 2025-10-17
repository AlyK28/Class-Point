from django.db import migrations


def noop_forward(apps, schema_editor):
    pass


def noop_reverse(apps, schema_editor):
    pass


class Migration(migrations.Migration):
    dependencies = [
        ('quizzes', '0002_drawingoptions_question_text_and_more'),
    ]

    operations = [
        migrations.RunPython(noop_forward, noop_reverse),
    ]


