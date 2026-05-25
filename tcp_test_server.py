#!/usr/bin/env python3
import socket
import struct
import random
import sys

def recvall(sock, n):
    """Read exactly n bytes from the socket or return None on EOF."""
    data = b''
    while len(data) < n:
        packet = sock.recv(n - len(data))
        if not packet:
            return None
        data += packet
    return data

def main(host='127.0.0.1', port=12345):
    # 1) Create, bind, and listen
    srv = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    srv.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    srv.bind((host, port))
    srv.listen(1)
    print(f"[SERVER] Listening on {host}:{port}…")

    conn, addr = srv.accept()
    print(f"[SERVER] Connection from {addr}")

    try:
        while True:
            # 2) Read 4 bytes (Unity’s reward)
            data = recvall(conn, 4)
            if data is None:
                print("[SERVER] Client disconnected")
                break
            reward = struct.unpack('<f', data)[0]
            print(f"[SERVER] Got reward = {reward:.3f}")

            # 3) Decide your action (here, random in [-1,1] for demo)
            action = [random.uniform(-1,1) for _ in range(3)]
            payload = b''.join(struct.pack('<f', a) for a in action)

            # 4) Send back exactly 12 bytes
            conn.sendall(payload)
            print(f"[SERVER] Sent action = {[round(a,3) for a in action]}")
    except KeyboardInterrupt:
        print("\n[SERVER] Shutting down (keyboard interrupt).")
    finally:
        conn.close()
        srv.close()

if __name__ == '__main__':
    main()
