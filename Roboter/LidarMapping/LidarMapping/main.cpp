
//#define _USE_MATH_DEFINES
//#include<math.h>

#include <iostream>
#include<map>
#include<vector>
#include<bitset>
#include<set>

#include "LidarScanner.h"
#include "Datenbank.h"

int main()
{
	std::map<int, LidarData> lidardata;

	std::vector<linedata> listoflines;

	datenbank::connect();

	lidar::start(&listoflines);

	//lidar::getLidardata(lidardata);

	//LinePoints test = lidar::rotateline();

	//std::cout << test.x1 << "|" << test.y1 << std::endl;
	
	while (true) {
		lidar::mainCalculation(lidardata);

	}



	lidar::disconnect();


      /*pstmt = con->prepareStatement("REPLACE into lidardata (keytest, distance, angale, numfound) values(?, ?, ?, ?)");
		linepstmt = con->prepareStatement("REPLACE into new_table (x1, y1, x2, y2,linekey) values(?, ?, ?, ?,?)");
		outlines = con->prepareStatement("REPLACE into outlines (x1, y1, x2, y2, angle) values(?, ?, ?, ?, ?)");
		newoutlines = con->prepareStatement("REPLACE into new_outlines (x1, y1, x2, y2, angle) values(?, ?, ?, ?, ?)");*/
}