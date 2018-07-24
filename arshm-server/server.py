from flask import Flask, render_template
from flask_socketio import SocketIO, send, emit

app = Flask(__name__)
app.config['SECRET_KEY'] = 'secret!'
socketio = SocketIO(app)

db = {}

@socketio.on('connect')
def on_connect():
    print('connect')
    for data in db.values():
        if data['type'] == 'annotation':
            emit('model', data)

@socketio.on('disconnect')
def on_disconnect():
    print('disconnect')

@socketio.on('model')
def on_model(data):
    print(data)
    db[data['id']] = data
    if data['type'] == 'annotation':
        emit('model', broadcast=True, include_self=False)

@socketio.on('fetch')
def on_fetch(item_id):
    if item_id in db:
        emit('model', db[item_id])
    else:
        send(None)

if __name__ == '__main__':
    socketio.run(app, host='0.0.0.0', port=5000)
