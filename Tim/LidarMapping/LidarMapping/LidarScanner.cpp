#include "LidarScanner.h"
#include "Datenbank.h"

RPlidarDriver* drv;
u_result result;

std::map<mapkey, std::vector<LinePoints>> mainmap;

bool connectSuccess = false;

#include<math.h>
#define _USE_MATH_DEFINES

std::map<int, LidarData> templidardata1;
std::map<int, LidarData> templidardata2;

std::vector<LinePoints> linesfount;
std::vector<LinePoints> templinesfount;


std::vector<linedata> *linesList;

std::vector<Point> line;

std::map<int, int> deltamap;

bool dountMove = true;


void lidar::start(std::vector<linedata> *listoflines)
{
	linesList = listoflines;

	const char* opt_com_path = NULL;

	opt_com_path = "com9";


	// create the driver instance
	drv = RPlidarDriver::CreateDriver(DRIVER_TYPE_SERIALPORT);
	if (!drv) {
		exit(-2);
	}

	rplidar_response_device_info_t devinfo;



	if (!drv)
		drv = RPlidarDriver::CreateDriver(DRIVER_TYPE_SERIALPORT);
	if (IS_OK(drv->connect(opt_com_path, 115200)))
	{
		result = drv->getDeviceInfo(devinfo);



		if (IS_OK(result))
		{
			connectSuccess = true;

		}
		else
		{
			delete drv;
			drv = NULL;
		}
	}
	else {
		std::cout << "Lidar start failed" << std::endl;
	}


	drv->startMotor();

	drv->startScan(0, 1);

	std::cout << "Lidar starts" << std::endl;

}

bool flag = true;

Point p1;
Point p2;
Point delta;

bool lidar::calculateLines(std::vector<Point>& data, bool& firstscan)
{

	if (flag == true && !data.empty()) {
		if (firstscan == true) {
			p1 = data[0];
			p2 = data[1];
			firstscan = false;
		}


		float delta_x = p2.x - p1.x;
		float delta_y = p2.y - p1.y;

		delta = { delta_x ,delta_y };

		float l = sqrt(delta_x * delta_x + delta_y * delta_y);

		delta_x = delta_x / l;
		delta_y = delta_y / l;

		line.clear();
		line.push_back(p1);
		line.push_back(p2);

		flag = false;
		data.clear();
	}
	else if (flag == false && !data.empty()) {

		float vec1_x = data[0].x - p1.x;
		float vec1_y = data[0].y - p1.y;

		float k = vec1_x * delta.y - vec1_y * delta.x;

		//std::cout << "k:" << k << std::endl;
		if (abs(k) <= 1600.0f) {

			line.push_back(data[0]);
			data.clear();
		}
		else {
			bool notfound = false;
			p1 = p2;
			p2 = data[0];
			flag = true;
			data.clear();
			if (!linesList->empty()) {
				if (linesList->size() > 2) {

					for (auto& i : *linesList) {
						if (!i.points.empty()) {
							float delta_x1 = abs(i.points.front().x - line.front().x);
							float delta_y1 = abs(i.points.front().x - line.front().x);
							float delta_x2 = abs(i.points.front().x - line.front().x);
							float delta_y2 = abs(i.points.front().x - line.front().x);
							// 6
							//std::cout << delta_x1 << "|" << delta_y1 << "|" << delta_x2 << "|" << delta_y2 << std::endl;
							if (delta_x1 <= 6 && delta_y1 <= 6 && delta_x2 <= 6 && delta_y2 <= 6) {
								i.numfound++;
								notfound = false;
							}
							else {
								notfound = true;
							}
						}

					}
					if (notfound) {
						linesList->push_back({ line,0 });
					}
				}
				else {
					linesList->push_back({ line,0 });
				}
			}
			else {
				linesList->push_back({ line,0 });
			}
			line.clear();
		}
	}
	return true;
}

void lidar::getLidardata(std::map<int, LidarData>& lidardata)
{

	rplidar_response_measurement_node_hq_t nodes[8192];

	size_t nodeCount = sizeof(nodes) / sizeof(rplidar_response_measurement_node_hq_t);

	result = drv->grabScanDataHq(nodes, nodeCount);

	std::vector<Point> xydata;

	bool flag = false;

	if (IS_OK(result)) {
		drv->ascendScanData(nodes, nodeCount);
		flag = true;
		for (int i = 0; i < (int)nodeCount; i++)
		{

			int angle = nodes[i].angle_z_q14 * 90.f / (1 << 14);
			if ((int)nodes[i].quality >= 50) {


				
				float distance = nodes[i].dist_mm_q2 / (1 << 2);

 
				float x = (sin(angle * M_PI / 180) * distance);
				float y = (cos(angle * M_PI / 180) * distance) * -1;
				if (x != 0 && y != 0) {
					xydata.push_back({ x, y });
				}

				lidar::calculateLines(xydata, flag);



				lidardata[angle].distance = distance;

				if (lidardata.count(angle) == 1) {

					if (abs(lidardata[angle].distance - nodes[i].dist_mm_q2 / (1 << 2)) > 10) {


						lidardata[angle].distance = nodes[i].dist_mm_q2 / (1 << 2);
						lidardata[angle].found = true;

					}
					else {
						if (lidardata[angle].distance != 0) {
							if (lidardata[angle].numfound < 6) {
								lidardata[angle].numfound++;
							}
						}
						lidardata[angle].found = false;

					}


				}
				else {
					lidardata[angle].distance = nodes[i].dist_mm_q2 / (1 << 2);
					lidardata[angle].found = false;
					lidardata[angle].numfound = 0;
					lidardata[angle].delta = 0;

				}
			}

		}
		datenbank::writeLidarData(lidardata);
		//datenbank::writeLineData(linesList);
	}
}

void lidar::disconnect()
{
	drv->stopMotor();
	drv->disconnect();
}

LinePoints lidar::rotateline(LinePoints input, double w)
{
	LinePoints data = {0,0,0,0};

	w = (w * M_PI) / 180;
	data.x1 = (input.x1  * round(cos(w))) - (input.y1 * round(sin(w)));
	data.y1 = (input.y1  * round(cos(w))) + (input.x1 * round(sin(w)));
	data.x2 = (input.x2 * round(cos(w))) - (input.y2 * round(sin(w)));
	data.y2 = (input.y2 * round(cos(w))) + (input.x2 * round(sin(w)));

	return data;
}

void lidar::scan6times(std::map<int, LidarData>& lidardata, std::map<int, LidarData>& templidardata) {


	for (int i = 0; i < 6; i++)
	{
		lidar::getLidardata(lidardata);
		
		for (auto& i : lidardata)
		{
			if (i.second.numfound == 6) {
				templidardata[i.first] = i.second;
			}
		}
	}
}

void lidar::mainCalculation(std::map<int, LidarData>& lidardata)
{
	
	int linecounter_debug = 0;
	lidar::scan6times(lidardata, templidardata2);

	bool foundsameline = false;

	for (auto& i : *linesList) {



		if (i.points.size() > 6) {

			if (i.numfound > 3) {
				linecounter_debug++;
				

				for (auto& j : linesfount) {

					if (i.points.front().x <= j.x1 && i.points.front().y <= j.y1 && i.points.back().x >= j.x2 && i.points.back().y >= j.y2) {
						std::cout << "Test" << std::endl;
						j.numfound++;
						std::cout <<"Found"<< j.numfound << std::endl;
						foundsameline = true;
						break;
					}
					else {
						foundsameline = false;
					}


				}
				if (foundsameline) {
					
				}
				else {
					linesfount.push_back({ i.points.front().x ,i.points.front().y ,i.points.back().x ,i.points.back().y });

				}





			}
		}
	}

	std::cout << linecounter_debug << std::endl;

	datenbank::writeLineData(linesfount);

	linesList->clear();

	lidardata.clear();

	bool debug = false;

	lidar::calculateRotation();

	templinesfount = linesfount;



	templidardata1 = templidardata2;

}

void lidar::calculateRotation()
{







}
