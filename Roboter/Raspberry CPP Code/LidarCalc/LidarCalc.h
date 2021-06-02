#pragma once
#define _USE_MATH_DEFINES
#include<map>
#include<vector>
#include<thread>
#include<future>
#include<math.h>
#include<arm_neon.h>
#include<iostream>


struct LidarData{
    int distance;
    float angle;
};

struct Point{
    float x,y;
};

struct MapData{
    int x = 0;
    int y = 0;
    int angle = 0;
    std::map<int, LidarData> data;
};

struct PointsFound
{
    int counter = 0;
    bool newPoint;
};

void CalculateSinAndCos();

int calculateAngle(bool&);

void rotatePoint(float & x, float & y, int w);

Point calculateXY(std::map<int, LidarData> &referenceData, std::map<int, LidarData> &newData, int angle);

