#pragma once

#include<iostream>
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
#include<vector>
#include<sstream>
#include<map>
#include"Lidar.h"
#include"Roboter.h"
#include<thread>
#include<chrono>
#include<ostream>

struct StringData
{
	std::string command;
	std::string args;
};

void TCPServer(bool &lidarStart, bool &serialstart);