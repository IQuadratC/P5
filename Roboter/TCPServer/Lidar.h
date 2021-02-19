#pragma once
#include<iostream>
#include<map>
//#include "/home/pi/Desktop/Header/rplidar.h"
#include "/home/tim/Schreibtisch/Header/rplidar.h"

using namespace rp::standalone::rplidar;


void startLidar(bool &lidarStart);

void getlidardata(std::map<int, int> &lidardata);

void savescans(std::map<int, std::vector<int>> &distancesData, int howmany);

void calculateaverage(std::map<int, int> &lidardatasum, int howmany);