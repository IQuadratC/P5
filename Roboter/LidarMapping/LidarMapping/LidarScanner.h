#pragma once

#include"rplidar.h"
#include<iostream>
#include<map>
#include<math.h>
#define _USE_MATH_DEFINES


using namespace rp::standalone::rplidar;

struct Point
{
	float x;
	float y;
};
struct LinePoints
{
	float x1;
	float y1;
	float x2;
	float y2;
	int numfound;

};
struct linedata {
	std::vector<Point> points;
	int numfound = 0;
};

struct LidarData
{
	int angle;
	float distance;
	int numfound;
	bool found;
	int delta;
};
struct mapkey {

	int x;
	int y;

};

namespace lidar {

	void start(std::vector<linedata>*listoflines);

	void getLidardata(std::map<int, LidarData>& lidardata);

	void disconnect();

	LinePoints rotateline(LinePoints input, double w);

	void mainCalculation(std::map<int, LidarData>& lidardata);

	void scan6times(std::map<int, LidarData>& lidardata, std::map<int, LidarData>& templidardata2);

	bool calculateLines(std::vector<Point>& data,bool& firstscan);
 
	void calculateRotation();

}

