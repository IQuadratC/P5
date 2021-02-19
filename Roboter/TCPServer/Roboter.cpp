#include "Roboter.h"




float steepPerCM = 1000;
float sidewaysMultiplier = 10;
float moveSpeed = 20000; // milliseconds per step
float stepsPerDegree = 1;
float rotationSpeed = 20; //milliseconds per step
float stopMultiThread = 0;

int serial_port = 0;



void moveRobot(float x, float y, float speed)
{

    int m1 = steepPerCM * x;
    int m2 = steepPerCM * x;
    int m3 = steepPerCM * x;
    int m4 = steepPerCM * x;
    m1 += y * steepPerCM * sidewaysMultiplier;
    m2 += y * steepPerCM * sidewaysMultiplier;
    m3 -= y * steepPerCM * sidewaysMultiplier;
    m4 -= y * steepPerCM * sidewaysMultiplier;

    int m1speed = 0;
    int m2speed = 0;
    int m3speed = 0;
    int m4speed = 0;

    int time = speed * sqrt(x * x + y * y);
    if (m1 != 0)
    {
        m1speed = int(time / abs(m1));
    }
    else
    {
        m1speed = 0;
    }
    if (m1 != 0)
    {
        m2speed = int(time / abs(m2));
    }
    else
    {
        m2speed = 0;
    }
    if (m1 != 0)
    {
        m3speed = int(time / abs(m3));
    }
    else
    {
        m3speed = 0;
    }
    if (m1 != 0)
    {
        m4speed = int(time / abs(m4));
    }
    else
    {
        m4speed = 0;
    }

    char diractions = 0;

    if (m1 < 0)
    {
        diractions |= 1 << 0;
    }
    if (m2 < 0)
    {
        diractions |= 1 << 1;
    }
    if (m3 < 0)
    {
        diractions |= 1 << 2;
    }
    if (m4 < 0)
    {
        diractions |= 1 << 3;
    }
    unsigned char senddata[13];
    memset(senddata, 0, 13);
    senddata[0] = m1 >> 8 & 0xFF;
    senddata[1] = m1 & 0xFF;
    senddata[2] = m2 >> 8 & 0xFF;
    senddata[3] = m2 & 0xFF;
    senddata[4] = m3 >> 8 & 0xFF;
    senddata[5] = m3 & 0xFF;
    senddata[6] = m4 >> 8 & 0xFF;
    senddata[7] = m4 & 0xFF;
    senddata[8] = m1speed;
    senddata[9] = m2speed;
    senddata[10] = m3speed;
    senddata[11] = m4speed;
    senddata[12] = diractions;
    std::cout << m1speed << std::endl;
    std::cout << m1 << std::endl;
    for (auto i : senddata)
    {
        std::cout << std::hex << (short)i << '\n';
    }
    SendDatatoArduino(senddata);
}

void rotateRobot(int a, int speed)
{

    int diractions = 0;
    if (a < 0)
    {
        diractions |= 1 << 0;
        diractions |= 1 << 2;
    }
    else
    {
        diractions |= 1 << 1;
        diractions |= 1 << 3;
    }

    unsigned char senddata[13];
    memset(senddata, 0, 13);

    int m1 = abs(stepsPerDegree * a);
    int m2 = abs(stepsPerDegree * a);
    int m3 = abs(stepsPerDegree * a);
    int m4 = abs(stepsPerDegree * a);

    senddata[0] = m1 >> 8 & 0xFF;
    senddata[1] = m1 & 0xFF;
    senddata[2] = m2 >> 8 & 0xFF;
    senddata[3] = m2 & 0xFF;
    senddata[4] = m3 >> 8 & 0xFF;
    senddata[5] = m3 & 0xFF;
    senddata[6] = m4 >> 8 & 0xFF;
    senddata[7] = m4 & 0xFF;
    senddata[8] = rotationSpeed;
    senddata[9] = rotationSpeed;
    senddata[10] = rotationSpeed;
    senddata[11] = rotationSpeed;
    senddata[12] = diractions;

    SendDatatoArduino(senddata);
}

void startserial(bool &serialstart)
{
    //Quelle: https://blog.mbedded.ninja/programming/operating-systems/linux/linux-serial-ports-using-c-cpp/
    serial_port = open("/dev/ttyACM0", O_RDWR);
    if (serial_port == -1)
    {
        printf("[ERROR] UART open()\n");
    }

    struct termios options;
    if (tcgetattr(serial_port, &options) != 0)
    {
        printf("Error %i from tcgetattr: %s\n", errno, strerror);
    }

    options.c_cflag &= ~PARENB;
    options.c_cflag &= ~CSTOPB;
    options.c_cflag &= ~CSIZE;
    options.c_cflag |= CS8;
    options.c_cflag &= ~CRTSCTS;
    options.c_cflag |= CREAD | CLOCAL;
    options.c_lflag &= ICANON;
    options.c_lflag &= ECHO;
    options.c_lflag &= ECHOE;
    options.c_lflag &= ECHONL;
    options.c_lflag &= ~ISIG;
    options.c_iflag &= ~(IXON | IXOFF | IXANY);
    options.c_iflag &= ~(IGNBRK | BRKINT | PARMRK | ISTRIP | INLCR | IGNCR | ICRNL);
    options.c_oflag &= ~OPOST;
    options.c_oflag &= ~ONLCR;
    options.c_cc[VTIME] = 1;
    options.c_cc[VMIN] = 0;
    cfsetspeed(&options, B230400);

    if (tcsetattr(serial_port, TCSANOW, &options) != 0)
    {
        printf("Error %i from tcsetattr: %s\n", errno, strerror);
    }
    else
    {
        serialstart = true;
    }
}

void SendDatatoArduino(unsigned char senddata[13])
{

    if (serial_port != -1)
    {
        int out = write(serial_port, senddata, 13);
        if (out < 0)
        {
            printf("[ERROR] UART TX\n");
        }
        else
        {
            printf("[STATUS: TX %i Bytes] %s\n", out, senddata);
        }
    }
}


bool roboterreadData(){
    char buffer;
    read(serial_port,&buffer,1);
    if(buffer == 0xff){
        return true;
    }else {
        return false;
    }
}

