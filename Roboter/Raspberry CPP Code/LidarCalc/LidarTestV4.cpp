#include"LidarTestV4.h"


// g++ Compile Code(VM) : g++ -I/usr/include -L /usr/lib64 *.cpp -o test -lmysqlcppconn librplidar_sdk.a -pthread

Vector2 roboterPos;
int roboterAngle;


struct MainMapPointData {
	int counter;
	float x;
	float y;
};



std::map<int, LidarData> referenceData;

std::map<int, LidarData> newData;


std::map<Vector2, MapData> MainMapV2;
std::vector<MapData> MainMap;
std::map<Vector2, PointsFound> MainMapPoints;



bool lidarStart = false;

bool AngleError = false;

void mainLidarCalc()
{

	CalculateSinAndCos();

	getLidarData(referenceData);

	MainMap.push_back({ 0,0,0,referenceData });
	saveMapAsPoints({0,0,0,referenceData}, MainMapPoints);

	int angle = 0;

	int counter = 0;

	int old_sum = 0;

	bool old_data = false;

	float old_x = 0;
	float old_y = 0;

	while (true)
	{
		newData.clear();

		getLidarData(newData);

		angle = calculateAngle(AngleError);

		Point transfrom = calculateXY(referenceData, newData, angle);

		int x = transfrom.x;
		int y = transfrom.y;

		roboterPos = { (float)(x + MainMap.rbegin()->x),(float)(y + MainMap.rbegin()->y) };
		roboterAngle = angle + MainMap.rbegin()->angle;

		if (abs(old_sum - angle) > 2) {
			old_sum = angle;
		}
		else if (abs(x) > 2 || abs(y) > 2 && !AngleError) {

			referenceData.clear();
			referenceData = newData;
			MapData mapdata;
			if (old_data) {
				mapdata.x = old_x + x;
				mapdata.y = old_y + y;

				mapdata.angle = MainMap.rbegin()->angle + angle;
				mapdata.data = referenceData;
			}
			else {

				mapdata.x = MainMap.rbegin()->x + x;
				mapdata.y = MainMap.rbegin()->y + y;

				mapdata.angle = MainMap.rbegin()->angle + angle;

				mapdata.data = referenceData;
			}

			MainMap.push_back(mapdata);
			saveMapAsPoints(mapdata, MainMapPoints);
			printf("Map:%I64u\n", MainMap.size());

			if (MainMapV2.find({ (float)mapdata.x, (float)mapdata.y }) == MainMapV2.end()) {
				MainMapV2[{(float)mapdata.x, (float)mapdata.y}] = mapdata;
				old_data = false;
			}
			else {
				referenceData = MainMapV2[{(float)mapdata.x, (float)mapdata.y}].data;
				old_x = mapdata.x;
				old_y = mapdata.y;
				old_data = true;
				std::cout << "Old_Data" << std::endl;
			}
		}
	}
}
