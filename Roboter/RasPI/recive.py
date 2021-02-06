import socket
import serial
from math import sqrt
import threading
import time

sidewaysMultiplier = 10
steepPerMeter = 1000
millisecondsPerMeter = 20000
stepsPerDegree = 1
rotationMillisecondPerSteep = 20
stopMultiThread = 0

ser = serial.Serial(
    port='\\\\.\\COM7',
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
    print(' '.join(format(x, '02x') for x in msg))


def read(msg):
    global stopMultiThread
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
    elif msg.split(" ", 1)[0] == "move":
        move(float(msg.split(" ", 2)[1]), float(msg.split(" ", 2)[2]))
    elif msg.split(" ", 1)[0] == "rotate":
        rotate(float(msg.split(" ", 1)[1]))
    elif msg.split(",", 1)[0] == "multi":
        stopMultiThread = 0
        thread = threading.Thread(target=multi, args=(msg.split(",", 1)[1],))
        thread.start()


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
    if m1 != 0:
        m1speed = int(time/abs(m1))
    else:
        m1speed = 0

    if m2 != 0:
        m2speed = int(time/abs(m2))
    else:
        m2speed = 0

    if m3 != 0:
        m3speed = int(time/abs(m3))
    else:
        m3speed = 0

    if m4 != 0:
        m4speed = int(time/abs(m4))
    else:
        m4speed = 0

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
        m1speed.to_bytes(1, "big", signed=False) +  # 1 Byte Motor 1 Speed
        m2speed.to_bytes(1, "big", signed=False) +  # 1 Byte Motor 2 Speed
        m3speed.to_bytes(1, "big", signed=False) +  # 1 Byte Motor 3 Speed
        m4speed.to_bytes(1, "big", signed=False) +  # 1 Byte Motor 4 Speed
        (directions).to_bytes(1, "big", signed=False)  # 1.-4. Bit Motor 1-4 direction
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
        (directions).to_bytes(1, "big", signed=False)  # 1.-4. Bit Motor 1-4 direction
    )


def multi(msg):
    for i in msg.split(","):
        read(i)
        ser.read(size=1)  # wait until movement is done
        if stopMultiThread == 1:
            return


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
        stopMultiThread = 1
        read(msg.decode("utf-8"))
        print(msg.decode("utf-8"))
        msg = c.recv(1024)

    # disconnect the server
    c.close()
