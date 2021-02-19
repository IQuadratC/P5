#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <errno.h>
#include <unistd.h>
#include <arpa/inet.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <sys/time.h>
#include <sstream>
#include <vector>
#include <iostream>
#include <thread>
#include <map>
#include <chrono>
#include <errno.h>
#include <unistd.h>
#include <fcntl.h>
#include <termios.h>
#include <bitset>
#include <cmath>
#include"Lidar.h"
#include"Roboter.h"
#include"TCPServer.h"

//g++ -o test LidarTCPServer.cpp Roboter.cpp TCPServer.cpp Lidar.cpp librplidar_sdk.a -pthread




bool lidarStart = false;
bool serialstart = false;

int main()
{

	startserial(serialstart);

	startLidar(lidarStart);

	TCPServer(lidarStart, serialstart);
}
















