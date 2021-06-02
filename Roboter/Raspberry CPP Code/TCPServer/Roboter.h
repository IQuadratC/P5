#pragma once
#include<iostream>
#include<stdio.h>
#include<cmath>
#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <errno.h>
#include <unistd.h>
#include <fcntl.h>
#include <termios.h>
#include <vector>
#include <sstream>
#include<thread>
#include"../LidarCalc/Lidar.h"
#include"../LidarCalc/LidarCalc.h"


struct DiractionBlock{

    bool front = false;
    bool back = false;
    bool left = false;
    bool right = false;

};

void moveRobot(float x, float y, int speed);

void rotateRobot(int a, int speed);

void startserial(bool &serialstart);

void roboterstop();

void SendDatatoArduino(unsigned char senddata[13]);

bool roboterReadData();

void checkdistance(int x, int y,bool &move);