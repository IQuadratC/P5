import socket
import serial

speed = 20
sidewaysMultiplier = 1

ser = serial.Serial(
    port='\\\\.\\COM4',
    baudrate=115200,
    parity=serial.PARITY_ODD,
    stopbits=serial.STOPBITS_ONE,
    bytesize=serial.EIGHTBITS
)
if ser.isOpen():
    ser.close()
ser.open()
ser.isOpen()


def send(msg):
    ser.write(msg)
    print(msg)


def read(msg):
    if msg == "forward":
        send(
            (0b1111111111111111).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 1 Steps
            (0b1111111111111111).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 2 Steps
            (0b1111111111111111).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 3 Steps
            (0b1111111111111111).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 4 Steps
            speed.to_bytes(1, "big", signed=False) +  # 1 Byte Motor 1 Speed
            speed.to_bytes(1, "big", signed=False) +  # 1 Byte Motor 2 Speed
            speed.to_bytes(1, "big", signed=False) +  # 1 Byte Motor 3 Speed
            speed.to_bytes(1, "big", signed=False) +  # 1 Byte Motor 4 Speed
            (0b00000000).to_bytes(1, "big", signed=False)  # 1.-4. Bit Motor 1-4 direction
        )
    elif msg == "backward":
        send(
            (0xffff).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 1 Steps
            (0xffff).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 2 Steps
            (0xffff).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 3 Steps
            (0xffff).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 4 Steps
            speed.to_bytes(1, "big", signed=False) +  # 1 Byte Motor 1 Speed
            speed.to_bytes(1, "big", signed=False) +  # 1 Byte Motor 2 Speed
            speed.to_bytes(1, "big", signed=False) +  # 1 Byte Motor 3 Speed
            speed.to_bytes(1, "big", signed=False) +  # 1 Byte Motor 4 Speed
            (0b1111*16).to_bytes(1, "big", signed=False)  # 1.-4. Bit Motor 1-4 direction
        )
    elif msg == "right":
        send(
            (0xffff).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 1 Steps
            (0xffff).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 2 Steps
            (0xffff).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 3 Steps
            (0xffff).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 4 Steps
            (speed*sidewaysMultiplier).to_bytes(1, "big", signed=False) +  # 1 Byte Motor 1 Speed
            (speed*sidewaysMultiplier).to_bytes(1, "big", signed=False) +  # 1 Byte Motor 2 Speed
            (speed*sidewaysMultiplier).to_bytes(1, "big", signed=False) +  # 1 Byte Motor 3 Speed
            (speed*sidewaysMultiplier).to_bytes(1, "big", signed=False) +  # 1 Byte Motor 4 Speed
            (0b1001*16).to_bytes(1, "big", signed=False)  # 1.-4. Bit Motor 1-4 direction
        )
    elif msg == "left":
        send(
            (0xffff).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 1 Steps
            (0xffff).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 2 Steps
            (0xffff).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 3 Steps
            (0xffff).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 4 Steps
            (speed*sidewaysMultiplier).to_bytes(1, "big", signed=False) +  # 1 Byte Motor 1 Speed
            (speed*sidewaysMultiplier).to_bytes(1, "big", signed=False) +  # 1 Byte Motor 2 Speed
            (speed*sidewaysMultiplier).to_bytes(1, "big", signed=False) +  # 1 Byte Motor 3 Speed
            (speed*sidewaysMultiplier).to_bytes(1, "big", signed=False) +  # 1 Byte Motor 4 Speed
            (0b0110*16).to_bytes(1, "big", signed=False)  # 1.-4. Bit Motor 1-4 direction
        )
    elif msg == "rotateLeft":
        send(
            (0xffff).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 1 Steps
            (0xffff).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 2 Steps
            (0xffff).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 3 Steps
            (0xffff).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 4 Steps
            speed.to_bytes(1, "big", signed=False) +  # 1 Byte Motor 1 Speed
            speed.to_bytes(1, "big", signed=False) +  # 1 Byte Motor 2 Speed
            speed.to_bytes(1, "big", signed=False) +  # 1 Byte Motor 3 Speed
            speed.to_bytes(1, "big", signed=False) +  # 1 Byte Motor 4 Speed
            (0b0101*16).to_bytes(1, "big", signed=False)  # 1.-4. Bit Motor 1-4 direction
        )
    elif msg == "rotateRight":
        send(
            (0xffff).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 1 Steps
            (0xffff).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 2 Steps
            (0xffff).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 3 Steps
            (0xffff).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 4 Steps
            speed.to_bytes(1, "big", signed=False) +  # 1 Byte Motor 1 Speed
            speed.to_bytes(1, "big", signed=False) +  # 1 Byte Motor 2 Speed
            speed.to_bytes(1, "big", signed=False) +  # 1 Byte Motor 3 Speed
            speed.to_bytes(1, "big", signed=False) +  # 1 Byte Motor 4 Speed
            (0b1010*16).to_bytes(1, "big", signed=False)  # 1.-4. Bit Motor 1-4 direction
        )
    elif msg == "stop":
        send((0).to_bytes(13, "big"))


# take the server name and port name
host = 'local host'
port = 5000

# create a socket at server side using TCP / IP protocol
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# bind the socket with server and port number
s.bind(('', port))

# allow maximum 1 connection to the socket
s.listen(1)

while True:
    # wait till a client accept connection
    c, addr = s.accept()

    # display client address
    print("CONNECTION FROM:", str(addr))

    # receive message string from server, at a time 1024 B
    msg = c.recv(1024)
    while msg:
        read(msg.decode("utf-8"))
        print(msg.decode("utf-8"))
        msg = c.recv(1024)

    # disconnect the server
    c.close()
