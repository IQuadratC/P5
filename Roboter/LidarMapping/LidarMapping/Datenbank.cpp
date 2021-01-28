#include "Datenbank.h"


sql::Statement* stmt;

sql::Connection* con;

sql::Driver* driver;
sql::ResultSet* res;
sql::PreparedStatement* pstmt;
sql::PreparedStatement* linepstmt;


void datenbank::connect()
{

	try {

		driver = get_driver_instance();
		con = driver->connect("tcp://127.0.0.1:3306", "root", "root");
		con->setSchema("testdata");

		stmt = con->createStatement();
		stmt->execute("SET autocommit = 0");
		stmt->execute("truncate new_table");
		delete stmt;
		std::cout << "Connected to database" << std::endl;

	}
	catch (sql::SQLException& e) {
		std::cout << "# ERR: SQLException in " << __FILE__;
		std::cout << "(" << __FUNCTION__ << ") on line " << __LINE__ << std::endl;
		std::cout << "# ERR: " << e.what();
		std::cout << " (MySQL error code: " << e.getErrorCode();
		std::cout << ", SQLState: " << e.getSQLState() << " )" << std::endl;
	}

}

void datenbank::writeLidarData(std::map<int, LidarData>& lidardata)
{
	if (!lidardata.empty()) {

		try {

			pstmt = con->prepareStatement("REPLACE into lidardata (keytest, distance, angale, numfound) values(?, ?, ?, ?)");

			for (auto& i : lidardata) {
				if (i.second.distance > 0) {
					pstmt->setInt(1, i.first);
					pstmt->setDouble(2, i.second.distance);
					pstmt->setDouble(3, i.first);
					pstmt->setInt(4, i.second.numfound);
					pstmt->executeUpdate();
				}
			}

			stmt = con->createStatement();
			stmt->execute("COMMIT");
			delete pstmt;
			delete stmt;

		}
		catch (sql::SQLException& e) {
			std::cout << "# ERR: SQLException in " << __FILE__;
			std::cout << "(" << __FUNCTION__ << ") on line " << __LINE__ << std::endl;
			std::cout << "# ERR: " << e.what();
			std::cout << " (MySQL error code: " << e.getErrorCode();
			std::cout << ", SQLState: " << e.getSQLState() << " )" << std::endl;
		}
	}
}

void datenbank::writeLineData(std::vector<LinePoints> &listoflines)
{

	try {

		stmt = con->createStatement();
		stmt->execute("truncate new_table");


		linepstmt = con->prepareStatement("REPLACE into new_table (x1, y1, x2, y2) values(?, ?, ?, ?)");

		for (auto &i : listoflines) {

			linepstmt->setDouble(1, i.x1);
			linepstmt->setDouble(2, i.y1);
			linepstmt->setDouble(3, i.x2);
			linepstmt->setDouble(4, i.y2);
			linepstmt->executeUpdate();
	
		}

		stmt->execute("COMMIT");
		delete stmt;
		delete linepstmt;

	}
	catch (sql::SQLException& e) {
		std::cout << "# ERR: SQLException in " << __FILE__;
		std::cout << "(" << __FUNCTION__ << ") on line " << __LINE__ << std::endl;
		std::cout << "# ERR: " << e.what();
		std::cout << " (MySQL error code: " << e.getErrorCode();
		std::cout << ", SQLState: " << e.getSQLState() << " )" << std::endl;
	}
}