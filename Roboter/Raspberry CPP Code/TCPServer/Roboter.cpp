#include "Roboter.h"

float stepPerCM = 500;        // Steps per cm
float stepsPerDegree = 180;    // Steps per Degree

int serial_port = 0; // Serial Port

extern int maxSpeed; // Maximal speed (kleiner gleich schneller(0-255))

using namespace std::chrono_literals;



void moveRobot(float y, float x, int Speedprozent)
{
	if (Speedprozent != 0) {

		int Motor12_Steps = 0;
		unsigned short Motor12_Delay = 0;

		int Motor34_Steps = 0;
		unsigned short Motor34_Delay = 0;

		if (x >= 0 && y >= 0 && abs(y) >= abs(x))
		{
			Motor34_Steps = stepPerCM * y;
			Motor34_Delay = Speedprozent;

			Motor12_Steps = stepPerCM * (y - x);
			Motor12_Delay = Speedprozent * ((y - x) / y);
		}
		else if (x >= 0 && y >= 0 && abs(y) <= abs(x))
		{

			Motor34_Steps = stepPerCM * x;
			Motor34_Delay = Speedprozent;

			Motor12_Steps = -stepPerCM * (x - y);
			Motor12_Delay = Speedprozent * ((x - y) / x);
		}
		else if (x >= 0 && y <= 0 && abs(y) <= abs(x))
		{

			Motor12_Steps = -stepPerCM * x;
			Motor12_Delay = Speedprozent;

			Motor34_Steps = stepPerCM * (x + y);
			Motor34_Delay = Speedprozent * ((x + y) / x);
		}
		else if (x >= 0 && y <= 0 && abs(y) >= abs(x))
		{

			Motor12_Steps = stepPerCM * y;
			Motor12_Delay = Speedprozent;

			Motor34_Steps = stepPerCM * (y + x);
			Motor34_Delay = Speedprozent * ((y + x) / y);
		}
		else if (x <= 0 && y <= 0 && abs(y) >= abs(x))
		{

			Motor34_Steps = stepPerCM * y;
			Motor34_Delay = Speedprozent;

			Motor12_Steps = stepPerCM * (y - x);
			Motor12_Delay = Speedprozent * ((y - x) / y);
		}
		else if (x <= 0 && y <= 0 && abs(y) <= abs(x))
		{
			Motor34_Steps = stepPerCM * x;
			Motor34_Delay = Speedprozent;

			Motor12_Steps = -stepPerCM * (x - y);
			Motor12_Delay = Speedprozent * ((x - y) / x);
		}
		else if (x <= 0 && y >= 0 && abs(y) <= abs(x))
		{
			Motor12_Steps = -stepPerCM * x;
			Motor12_Delay = Speedprozent;

			Motor34_Steps = stepPerCM * (x + y);
			Motor34_Delay = Speedprozent * ((x + y) / x);
		}
		else if (x <= 0 && y >= 0 && abs(y) >= abs(x))
		{
			Motor12_Steps = stepPerCM * y;
			Motor12_Delay = Speedprozent;

			Motor34_Steps = stepPerCM * (y + x);
			Motor34_Delay = Speedprozent * ((y + x) / y);
		}

		int m1speed;
		int m2speed;
		int m3speed;
		int m4speed;

		int index_m12 = 1;
		int index_m34 = 1;
		int m12_a = 100;
		int m34_a = 100;

		for (index_m12 = 1; index_m12 < 255; index_m12++)
		{
			m12_a /= 2;
			if (m12_a < Motor12_Delay)
				break;
		}
		for (index_m34 = 1; index_m34 < 255; index_m34++)
		{
			m34_a /= 2;
			if (m34_a < Motor34_Delay)
				break;
		}

		if (index_m12 < maxSpeed) {
			index_m12 = maxSpeed;
		}
		if (index_m34 < maxSpeed) {
			index_m34 = maxSpeed;
		}

		char diractions = 0;

		if (Motor12_Steps < 0)
		{
			diractions |= 1 << 0;
			diractions |= 1 << 1;
		}
		if (Motor34_Steps < 0)
		{
			diractions |= 1 << 2;
			diractions |= 1 << 3;
		}

		Motor12_Steps = abs(Motor12_Steps);
		Motor34_Steps = abs(Motor34_Steps);

		unsigned short m1_Steps = 0;
		unsigned short m2_Steps = 0;
		unsigned short m3_Steps = 0;
		unsigned short m4_Steps = 0;

		if (Motor12_Steps > std::numeric_limits<unsigned short>::max())
		{
			m1_Steps = std::numeric_limits<unsigned short>::max();
			m2_Steps = std::numeric_limits<unsigned short>::max();
		}
		else
		{
			m1_Steps = Motor12_Steps;
			m2_Steps = Motor12_Steps;
		}

		if (Motor34_Steps > std::numeric_limits<unsigned short>::max())
		{
			m3_Steps = std::numeric_limits<unsigned short>::max();
			m4_Steps = std::numeric_limits<unsigned short>::max();
		}
		else
		{
			m3_Steps = Motor34_Steps;
			m4_Steps = Motor34_Steps;
		}

		unsigned char senddata[15];

		memset(senddata, 0, 15);

		senddata[0] = 0x3C; //Start Byte
		senddata[1] = m1_Steps >> 8 & 0xFF;
		senddata[2] = m1_Steps & 0xFF;
		senddata[3] = m2_Steps >> 8 & 0xFF;
		senddata[4] = m2_Steps & 0xFF;
		senddata[5] = m3_Steps >> 8 & 0xFF;
		senddata[6] = m3_Steps & 0xFF;
		senddata[7] = m4_Steps >> 8 & 0xFF;
		senddata[8] = m4_Steps & 0xFF;
		senddata[9] = index_m12;
		senddata[10] = index_m12;
		senddata[11] = index_m34;
		senddata[12] = index_m34;
		senddata[13] = diractions;
		senddata[14] = 0x3E; //End Byte

		SendDatatoArduino(senddata);

		std::this_thread::sleep_for(50ms);

		m1_Steps = 0;
		m2_Steps = 0;
		m3_Steps = 0;
		m4_Steps = 0;
	}
	else {

		unsigned char senddata[15];
		memset(senddata, 0, 15);
		senddata[0] = 0x3C;
		senddata[14] = 0x3E;
		SendDatatoArduino(senddata);
	}
}

void rotateRobot(int a, int Speedprozent)
{
	if (Speedprozent != 0)
	{
		int diractions = 0;
		if (a > 0)
		{
			diractions |= 1 << 1;
			diractions |= 1 << 3;
		}
		else
		{
			diractions |= 1 << 0;
			diractions |= 1 << 2;
		}

		unsigned char senddata[15];
		memset(senddata, 0, 15);

		int m1 = abs(stepsPerDegree * a);
		int m2 = abs(stepsPerDegree * a);
		int m3 = abs(stepsPerDegree * a);
		int m4 = abs(stepsPerDegree * a);

		int speed = (255 - (Speedprozent * 2.55));

		if (speed < maxSpeed) speed = maxSpeed;

		senddata[0] = 0x3C; //Start Byte
		senddata[1] = m1 >> 8 & 0xFF;
		senddata[2] = m1 & 0xFF;
		senddata[3] = m2 >> 8 & 0xFF;
		senddata[4] = m2 & 0xFF;
		senddata[5] = m3 >> 8 & 0xFF;
		senddata[6] = m3 & 0xFF;
		senddata[7] = m4 >> 8 & 0xFF;
		senddata[8] = m4 & 0xFF;
		senddata[9] = speed;
		senddata[10] = speed;
		senddata[11] = speed;
		senddata[12] = speed;
		senddata[13] = diractions;
		senddata[14] = 0x3E; //End Byte
		SendDatatoArduino(senddata);
		std::this_thread::sleep_for(50ms);

		
	}
	else {
		unsigned char senddata[15];
		memset(senddata, 0, 15);
		senddata[0] = 0x3C;
		senddata[14] = 0x3E;
		SendDatatoArduino(senddata);
		std::this_thread::sleep_for(50ms);
	}
	
}

void startserial(bool& serialstart)
{
	//Quelle: https://blog.mbedded.ninja/programming/operating-systems/linux/linux-serial-ports-using-c-cpp/
	serialstart = true;

	serial_port = open("/dev/ttyACM0", O_RDWR);
	if (serial_port == -1)
	{
		printf("[ERROR] UART open()\n");
		serialstart = false;
	}
	if (serialstart)
	{
		struct termios options;
		if (tcgetattr(serial_port, &options) != 0)
		{
			printf("Error %i from tcgetattr: %s\n", errno, strerror);
			serialstart = false;
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
		options.c_cc[VTIME] = 10;
		options.c_cc[VMIN] = 1;
		cfsetspeed(&options, B115200);

		if (tcsetattr(serial_port, TCSANOW, &options) != 0)
		{
			printf("Error %i from tcsetattr: %s\n", errno, strerror);
			serialstart = false;
		}
		else
		{
			serialstart = true;
		}
	}
}

void SendDatatoArduino(unsigned char senddata[15])
{

	if (serial_port != -1)
	{
		int out = write(serial_port, senddata, 15);
		if (out < 0)
		{
			printf("[ERROR] UART TX\n");
		}
		for (int i = 0; i < 15; i++) {
			std::cout << std::hex << (short)senddata[i] << std::dec << std::endl;
		}
	}
}

bool roboterReadData()
{
	char buffer;
	read(serial_port, &buffer, 1);
	if (buffer == 0xff)
	{
		return true;
	}
	else
	{
		return false;
	}
	return false;
}
