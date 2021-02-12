import serial
from math import sqrt
import threading

sidewaysMultiplier = 10
steepPerCM = 1000
moveSpeed = 20000  # milliseconds per step
stepsPerDegree = 1
rotationSpeed = 20  # milliseconds per step
stopMultiThread = 0

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
    ser.write(bytearray(msg))
    print(' '.join(format(x, '02x') for x in bytearray(msg)))


def read(msg):
    global stopMultiThread
    stopMultiThread = stopMultiThread + 1
    if msg == "stop":
        send((0).to_bytes(13, "big"))
    elif msg.split(" ", 1)[0] == "move":
        move(float(msg.split(" ", 2)[1]), float(msg.split(" ", 3)[2]), float(msg.split(" ", 3)[2]))
    elif msg.split(" ", 1)[0] == "rotate":
        rotate(float(msg.split(" ", 1)[1]))
    elif msg.split(",", 1)[0] == "multi":
        thread = threading.Thread(target=multi, args=(msg.split(",", 1)[1],))
        thread.start()


def move(x, y, speed): # x, y in meters
    m1 = steepPerCM * x
    m2 = steepPerCM * x
    m3 = steepPerCM * x
    m4 = steepPerCM * x
    m1 -= y * steepPerCM * sidewaysMultiplier
    m2 += y * steepPerCM * sidewaysMultiplier
    m3 += y * steepPerCM * sidewaysMultiplier
    m4 -= y * steepPerCM * sidewaysMultiplier
    time = speed * sqrt(x * x + y * y)
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
        int(rotationSpeed).to_bytes(1, "big", signed=False) +  # 1 Byte Motor 1 Speed
        int(rotationSpeed).to_bytes(1, "big", signed=False) +  # 1 Byte Motor 2 Speed
        int(rotationSpeed).to_bytes(1, "big", signed=False) +  # 1 Byte Motor 3 Speed
        int(rotationSpeed).to_bytes(1, "big", signed=False) +  # 1 Byte Motor 4 Speed
        (directions).to_bytes(1, "big", signed=False)  # 1.-4. Bit Motor 1-4 direction
    )


def multi(msg):
    stop = stopMultiThread
    for i in msg.split(","):
        read(i)
        ser.read(size=1)  # wait until movement is done
        if stopMultiThread != stop:
            return
