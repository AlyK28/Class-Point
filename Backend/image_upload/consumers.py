import json
from channels.generic.websocket import AsyncWebsocketConsumer
from channels.db import database_sync_to_async
from .models import ImageUploadSession, ImageSubmission


class SessionConsumer(AsyncWebsocketConsumer):
    """WebSocket consumer for real-time session updates."""
    
    async def connect(self):
        """Connect to WebSocket and join session room."""
        self.session_code = self.scope['url_route']['kwargs']['session_code']
        self.room_group_name = f'session_{self.session_code}'
        
        # Check if session exists and is active
        session = await self.get_session()
        if not session or not session.is_active():
            await self.close()
            return
        
        # Join room group
        await self.channel_layer.group_add(
            self.room_group_name,
            self.channel_name
        )
        
        await self.accept()
        
        # Send current session info
        await self.send_session_info(session)
    
    async def disconnect(self, close_code):
        """Leave room group when disconnecting."""
        await self.channel_layer.group_discard(
            self.room_group_name,
            self.channel_name
        )
    
    async def receive(self, text_data):
        """Receive message from WebSocket."""
        try:
            text_data_json = json.loads(text_data)
            message_type = text_data_json.get('type')
            
            if message_type == 'join_session':
                await self.handle_join_session()
            elif message_type == 'leave_session':
                await self.handle_leave_session()
            else:
                await self.send(text_data=json.dumps({
                    'type': 'error',
                    'message': 'Unknown message type'
                }))
        except json.JSONDecodeError:
            await self.send(text_data=json.dumps({
                'type': 'error',
                'message': 'Invalid JSON'
            }))
    
    async def handle_join_session(self):
        """Handle join session message."""
        session = await self.get_session()
        if session:
            await self.send_session_info(session)
    
    async def handle_leave_session(self):
        """Handle leave session message."""
        await self.send(text_data=json.dumps({
            'type': 'session_left',
            'message': 'Left session'
        }))
    
    async def send_session_info(self, session):
        """Send current session information."""
        await self.send(text_data=json.dumps({
            'type': 'session_info',
            'session': {
                'code': session.session_code,
                'name': session.name,
                'question': session.question,
                'submission_count': session.submission_count,
                'max_submissions': session.max_submissions,
                'status': session.status
            }
        }))
    
    # WebSocket event handlers
    async def submission_created(self, event):
        """Handle new submission event."""
        await self.send(text_data=json.dumps({
            'type': 'submission_created',
            'submission': event['submission']
        }))
    
    async def submission_liked(self, event):
        """Handle submission liked event."""
        await self.send(text_data=json.dumps({
            'type': 'submission_liked',
            'submission_id': event['submission_id'],
            'likes': event['likes'],
            'is_liked': event['is_liked']
        }))
    
    async def submission_deleted(self, event):
        """Handle submission deleted event."""
        await self.send(text_data=json.dumps({
            'type': 'submission_deleted',
            'submission_id': event['submission_id']
        }))
    
    async def session_closed(self, event):
        """Handle session closed event."""
        await self.send(text_data=json.dumps({
            'type': 'session_closed',
            'session_id': event['session_id'],
            'closed_at': event['closed_at']
        }))
    
    @database_sync_to_async
    def get_session(self):
        """Get session from database."""
        try:
            return ImageUploadSession.objects.get(
                session_code=self.session_code,
                status='active'
            )
        except ImageUploadSession.DoesNotExist:
            return None
