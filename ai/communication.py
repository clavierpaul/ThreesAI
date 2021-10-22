import threading
from typing import Dict
import zmq

class GameConnector:
    context: zmq.Context
    socket: zmq.Socket

    def __init__(self, address: str, port: int) -> None:
        self.context = zmq.Context()
        self.socket = self.context.socket(zmq.REQ)
        self.socket.connect(f"tcp://{address}:{port}")

    def shift(self, direction: int) -> Dict:
        self.socket.send(direction.to_bytes(1, byteorder='little'))
        print(threading.get_ident())
        return self.socket.recv_json()

    def restart(self) -> Dict:
        self.socket.send(b'\x05')
        print(threading.current_thread().name)
        return self.socket.recv_json()