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



void moveRobot(float x, float y, float speed);

void rotateRobot(int a, int speed);

void startserial(bool &serialstart);

void SendDatatoArduino(unsigned char senddata[13]);

bool roboterreadData();