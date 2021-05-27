#include"Lidar.h"


RPlidarDriver* drv = nullptr;
u_result result;


void startLidar(bool& lidarstart) {

	const char* opt_com_path = "/dev/ttyUSB0";

	// create the driver instance
	drv = RPlidarDriver::CreateDriver(DRIVER_TYPE_SERIALPORT);
	std::cout << "Test" << std::endl;
	if (!drv) {
		return;
	}
	
	rplidar_response_device_info_t devinfo;

	if (IS_OK(drv->connect(opt_com_path, 115200)))
	{
		result = drv->getDeviceInfo(devinfo);


		if (IS_OK(result))
		{
			lidarstart = true;
			for (int pos = 0; pos < 16; ++pos) {
				printf("%02X", devinfo.serialnum[pos]);
			}
			printf("\n");
			
			printf("Firmware Version: %d.%02d\n"
					"Hardware Rev: %d\n"
				, devinfo.firmware_version >> 8
				, devinfo.firmware_version & 0xFF
				, (int)devinfo.hardware_version);

		}
		else
		{
			delete drv;
			drv = nullptr;
		}
	}
	else {
		std::cout << "Lidar start failed" << std::endl;
		lidarstart = false;
		return;
	}

	drv->startMotor();
	drv->startScan(0, 1);

	std::cout << "Lidar Starts" << std::endl;
}

void getLidarData(std::map<int, LidarData>& data) {

	rplidar_response_measurement_node_hq_t nodes[8192];

	size_t nodeCount = sizeof(nodes) / sizeof(rplidar_response_measurement_node_hq_t);

	result = drv->grabScanDataHq(nodes, nodeCount);

	if (IS_OK(result)) {
		drv->ascendScanData(nodes, nodeCount);

		for (int i = 0; i < (int)nodeCount; i++)
		{

			int angle = nodes[i].angle_z_q14 * 90.f / (1 << 14);

			if ((int)nodes[i].quality >= 60)
			{

				data[angle].distance = nodes[i].dist_mm_q2 / (1 << 2);
                data[angle].angle = nodes[i].angle_z_q14 * 90.f / (1 << 14);
			}

		}

	}
}

void savescans(std::map<int, std::vector<LidarData>> &distancesData, int howmany)
{
	for (int i = 0; i < howmany; i++)
	{
		std::map<int, LidarData> tempdata;

		getLidarData(tempdata);

		for (auto &i : tempdata)
		{
			distancesData[i.first].push_back(i.second);
		}
	}
}

void calculateaverage(std::map<int, LidarData> &lidardatasum, int howmany)
{

	std::map<int, std::vector<LidarData>> distancesData;

	savescans(distancesData, howmany);

	for (auto &i : distancesData)
	{

		float sum = 0;
		for (auto &j : i.second)
		{

			sum += j.distance;
		}
		if (sum > 0)
		{
			sum /= i.second.size();
		}
		else
		{
			continue;
		}
		lidardatasum[i.first].distance = sum;
	}
}

void StopLidar() {

	if (drv != nullptr) {

		drv->stop();
		drv->stopMotor();

		RPlidarDriver::DisposeDriver(drv);
	}
}