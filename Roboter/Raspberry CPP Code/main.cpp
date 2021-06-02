#include<thread>
#include"LidarCalc/LidarTestV4.h"
#include"TCPServer/TCPServer.h"
#include"LidarCalc/Lidar.h"
#include<stdio.h>
#include<signal.h>
#include<stdlib.h>

void signalhandler(int);

int maxSpeed = 8;

int main() {

	// Abfangen von ctrl+c damit der Serial Port geschlossen wird
	signal(SIGINT, signalhandler);

	bool lidarstarts = false;
	//Start Lidar Serial connection
	startLidar(lidarstarts);

	// Create new thread for Lidar Calculation
	std::thread LidarCalc(mainLidarCalc);

	//Start TCP Server on this thread
	TCPServer(lidarstarts);

}

void maxSpeedIn() {

	std::cout << "Bitte eine Zahl zwischen 5 und 8 eingeben. Für die Maxmiale Geschwindigkeit vom Roboter!" << std::endl;
	maxSpeed = 0;
	std::cin >> maxSpeed;
	if (maxSpeed < 5 || maxSpeed > 8) maxSpeedIn();

}

void  signalhandler(int sig)
{
	signal(sig, SIG_IGN);

	StopLidar();
	std::cout << "Lidar Stop! program closes!" << std::endl;
	exit(-2);
}