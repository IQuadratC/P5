#include "MapCalc.h"

void rotatePoint(float& x, float& y, int w);

void xyToPolar(float x, float y, float& r, float& q) {
	r = sqrt(x * x + y * y); 
	q = std::atan2(y, x);
}

int cellsize = 5;

void saveMapAsPoints(MapData input, std::map<Vector2, PointsFound>& output) {

	for (auto& i : input.data) {

		if (i.second.distance != 0) {
			float x = 0;
			float y = 0;

			x = (sin(i.first * M_PI / 180) * i.second.distance / 10);
			y = (cos(i.first * M_PI / 180) * i.second.distance / 10);

			rotatePoint(x, y, input.angle);

			x += input.x;
			y += input.y;

			x /= cellsize;
			y /= cellsize;

			if (output.find({ round(x) * cellsize, round(y) * cellsize}) == output.end()) {
				output[{round(x) * cellsize, round(y) * cellsize}].counter++;
				output[{round(x)* cellsize, round(y)* cellsize}].newPoint = true;
			}
			else {
				output[{round(x) * cellsize, round(y) * cellsize}].counter++;
				output[{round(x) * cellsize, round(y) * cellsize}].newPoint = false;
			}
			
		}

	}

}

