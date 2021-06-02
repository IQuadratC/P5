#include"/home/pi/Header/rplidar.h"
#include<iostream>
#include<map>
#include"LidarCalc.h"

using namespace rp::standalone::rplidar;

void startLidar(bool &lidarStart);

void getLidarData(std::map<int, LidarData> &lidardata);

void calculateaverage(std::map<int, LidarData> &lidardatasum, int howmany);

void StopLidar();