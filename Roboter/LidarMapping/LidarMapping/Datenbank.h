#pragma once

#include "mysql_connection.h"

#include <cppconn/driver.h>
#include <cppconn/driver.h>
#include <cppconn/exception.h>
#include <cppconn/resultset.h>
#include <cppconn/statement.h>
#include <cppconn/prepared_statement.h>

#include"LidarScanner.h"



namespace datenbank {


	void connect();

	void writeLidarData(std::map<int, LidarData>& lidardata);

	void writeLineData(std::vector<LinePoints>& listoflines);

}
