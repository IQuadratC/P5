import socket
import serial
from math import sqrt


sidewaysMultiplier = 1
steepPerMeter = 1000
millisecondsPerMeter = 20000
stepsPerDegree = 1
rotationMillisecondPerSteep = 20

# ser = serial.Serial(
#     port='\\\\.\\COM4',
#     baudrate=115200,
#     parity=serial.PARITY_ODD,
#     stopbits=serial.STOPBITS_ONE,
#     bytesize=serial.EIGHTBITS
# )
# if ser.isOpen():
#     ser.close()
# ser.open()
# ser.isOpen()


def send(msg):
    # ser.write(msg)
    print(' '.join(format(x, '02x') for x in msg))


def read(msg):
    if msg == "forward":
        move(0xffff / steepPerMeter, 0)
    elif msg == "backward":
        move(-0xffff / steepPerMeter, 0)
    elif msg == "right":
        move(0, 0xffff/(steepPerMeter*sidewaysMultiplier))
    elif msg == "left":
        move(0, -0xffff/(steepPerMeter*sidewaysMultiplier))
    elif msg == "rotateLeft":
        rotate(0xffff / stepsPerDegree)
    elif msg == "rotateRight":
        rotate(-0xffff / stepsPerDegree)
    elif msg == "stop":
        send((0).to_bytes(13, "big"))


def move(x, y): # x, y in meters
    m1 = steepPerMeter * x
    m2 = steepPerMeter * x
    m3 = steepPerMeter * x
    m4 = steepPerMeter * x
    m1 -= y * steepPerMeter * sidewaysMultiplier
    m2 += y * steepPerMeter * sidewaysMultiplier
    m3 += y * steepPerMeter * sidewaysMultiplier
    m4 -= y * steepPerMeter * sidewaysMultiplier
    time = millisecondsPerMeter * sqrt(x*x+y*y)
    directions = 0b0
    if m1 < 0:
        directions = 0b1000
    if m2 < 0:
        directions += 0b0100
    if m3 < 0:
        directions += 0b0010
    if m4 < 0:
        directions += 0b0001

    send(
        int(abs(m1)).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 1 Steps
        int(abs(m2)).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 2 Steps
        int(abs(m3)).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 3 Steps
        int(abs(m4)).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 4 Steps
        int(time/abs(m1)).to_bytes(1, "big", signed=False) +  # 1 Byte Motor 1 Speed
        int(time/abs(m2)).to_bytes(1, "big", signed=False) +  # 1 Byte Motor 2 Speed
        int(time/abs(m3)).to_bytes(1, "big", signed=False) +  # 1 Byte Motor 3 Speed
        int(time/abs(m4)).to_bytes(1, "big", signed=False) +  # 1 Byte Motor 4 Speed
        (directions * 16).to_bytes(1, "big", signed=False)  # 1.-4. Bit Motor 1-4 direction
    )


def rotate(a):  # a in degrees
    if a < 0:
        directions = 0b0101
    else:
        directions = 0b1010
        
    send(
        int(abs(stepsPerDegree * a)).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 1 Steps
        int(abs(stepsPerDegree * a)).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 2 Steps
        int(abs(stepsPerDegree * a)).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 3 Steps
        int(abs(stepsPerDegree * a)).to_bytes(2, "big", signed=False) +  # 2 Bytes Motor 4 Steps
        int(rotationMillisecondPerSteep).to_bytes(1, "big", signed=False) +  # 1 Byte Motor 1 Speed
        int(rotationMillisecondPerSteep).to_bytes(1, "big", signed=False) +  # 1 Byte Motor 2 Speed
        int(rotationMillisecondPerSteep).to_bytes(1, "big", signed=False) +  # 1 Byte Motor 3 Speed
        int(rotationMillisecondPerSteep).to_bytes(1, "big", signed=False) +  # 1 Byte Motor 4 Speed
        (directions * 16).to_bytes(1, "big", signed=False)  # 1.-4. Bit Motor 1-4 direction
    )

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
