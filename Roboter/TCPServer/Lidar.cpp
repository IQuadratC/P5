#include "Lidar.h"




RPlidarDriver *drv;
u_result result;

void startLidar(bool &lidarStart)
{

	const char *opt_com_path = "/dev/ttyUSB0";

	delete drv;
	// create the driver instance
	drv = RPlidarDriver::CreateDriver(DRIVER_TYPE_SERIALPORT);
	if (!drv)
	{
		exit(-2);
	}

	rplidar_response_device_info_t devinfo;

	if (!drv)
		drv = RPlidarDriver::CreateDriver(DRIVER_TYPE_SERIALPORT);
	if (IS_OK(drv->connect(opt_com_path, 115200)))
	{
		result = drv->getDeviceInfo(devinfo);

		printf("Serial Number:\t\t");
		for (auto &i : devinfo.serialnum)
		{
			printf("%02X", i);
		}

		printf("\n"
			   "Firmware Verison:\t%d.%02d\n"
			   "Hardware Verison:\t%d\n",
			   devinfo.firmware_version >> 8, devinfo.firmware_version & 0xFF, (int)devinfo.hardware_version);

		if (IS_OK(result))
		{
			lidarStart = true;
		}
		else
		{
			delete drv;
			drv = NULL;
			return;
		}
	}
	else
	{
		std::cout << "Lidar start failed" << std::endl;
		return;
	}

	u_result op_result;
	rplidar_response_device_health_t healthinfo;
	op_result = drv->getHealth(healthinfo);
	if (IS_OK(op_result))
	{
		printf("RPLidar health status : %d\n", healthinfo.status);
		if (healthinfo.status == RPLIDAR_STATUS_ERROR)
		{
			printf("Error, rplidar internal error detected. Please reboot the device to retry.\n");
			drv->reset();
		}
	}
	else
	{
		printf("Error, cannot retrieve the lidar health code: %x\n", op_result);
	}

	drv->startMotor();

	drv->startScan(0, 1);

	std::cout << "Lidar starts" << std::endl;
}

void getlidardata(std::map<int, int> &lidardata)
{

	rplidar_response_measurement_node_hq_t nodes[8192];

	size_t nodeCount = sizeof(nodes) / sizeof(rplidar_response_measurement_node_hq_t);

	result = drv->grabScanDataHq(nodes, nodeCount);

	if (IS_OK(result))
	{
		drv->ascendScanData(nodes, nodeCount);

		for (int i = 0; i < (int)nodeCount; i++)
		{

			int angle = nodes[i].angle_z_q14 * 90.f / (1 << 14);

			if ((int)nodes[i].quality >= 50)
			{

				lidardata[angle] = nodes[i].dist_mm_q2 / (1 << 2);
			}
		}
	}
}

void savescans(std::map<int, std::vector<int>> &distancesData, int howmany)
{
	for (int i = 0; i < howmany; i++)
	{
		std::map<int, int> tempdata;

		getlidardata(tempdata);

		for (auto &i : tempdata)
		{
			distancesData[i.first].push_back(i.second);
		}
	}
}

void calculateaverage(std::map<int, int> &lidardatasum, int howmany)
{

	std::map<int, std::vector<int>> distancesData;

	savescans(distancesData, howmany);

	for (auto &i : distancesData)
	{

		float sum = 0;
		for (auto &j : i.second)
		{

			sum += j;
		}
		if (sum > 0)
		{
			sum /= i.second.size();
		}
		else
		{
			continue;
		}
		lidardatasum[i.first] = sum;
	}
}


