#pragma once

#define _USE_MATH_DEFINES
#include<fstream>
#include<vector>
#include<map>
#include<sstream>
#include"../utility/Vector2.h"
#include<math.h>
#include"LidarCalc.h"





void saveMapAsPoints(MapData input, std::map<Vector2, PointsFound>& output);
