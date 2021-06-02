#include "LidarCalc.h"

extern std::map<int, LidarData> referenceData;

extern std::map<int, LidarData> newData;

extern std::vector<MapData> MainMap;

std::map<int, int> threadFehler;

static double sinus[360000];
static double cosinus[360000];

void CalculateSinAndCos() {

    for (int i = 0; i < 360000; i++) {
        double x = i / 1000.f;
        sinus[i] = sin(x * M_PI / 180.f);
    }
    for (int i = 0; i < 360000; i++) {
        double x = i / 1000.f;
        cosinus[i] = cos(x * M_PI / 180.f);
    }

}


int calculateFehler(int w)
{

    int fehler = 0;
    int counter = 0;

    for (auto i : referenceData)
    {
        int angle = i.first + w;

        if (angle > 359)
            angle -= 359;
        if (angle < 0)
            angle += 359;

        if (newData.find(angle) != newData.end())
        {
            fehler += abs(i.second.distance - newData[angle].distance);
        }
    }

    return fehler;
}

int calculateAngleThread()
{

    threadFehler.clear();

    for (int w = -180; w < 0; w++)
    {

        int fehler = calculateFehler(w);

        threadFehler[fehler] = w;
    }
    return 1;
}

int old_sum = 0;

int calculateAngle(bool& AngleError)
{

    std::map<int, int> fehlermap;

    std::future<int> w_thread;

    w_thread = std::async(calculateAngleThread);

    for (int w = 0; w < 180; w++)
    {
        int fehler = calculateFehler(w);
        fehlermap[fehler] = w;
    }

    int test = w_thread.get();

    for (auto i : threadFehler)
        fehlermap[i.first] = i.second;

    int index = 0;
    int sum = 0;
    if (fehlermap.begin()->first < 50000) {
        return old_sum;
        AngleError = true;
    }
    else {
        AngleError = false;
    }

    for (auto i : fehlermap)
    {
        if (index >= 2)
        {
            sum /= index;
            break;
        }

        sum += i.second;
        index++;
    }
    old_sum = sum;
    return sum;
}

static std::vector<Point> mapQ1_XY(90), mapQ2_XY(90), mapQ3_XY(90), mapQ4_XY(90);

static std::vector<Point> refmapQ1_XY(90), refmapQ2_XY(90), refmapQ3_XY(90), refmapQ4_XY(90);

static std::map<int, int> delta_X_Q1, delta_Y_Q1;
static std::map<int, int> delta_X_Q2, delta_Y_Q2;
static std::map<int, int> delta_X_Q3, delta_Y_Q3;
static std::map<int, int> delta_X_Q4, delta_Y_Q4;

void rotatePoint(float& x, float& y, int w) {
    if (w > 359)
        w -= 359;
    if (w < 0)
        w += 359;
    float s = sinus[int(w * 1000)];
    float c = cosinus[int(w * 1000)];


	float xnew = x * c - y * s;
	float ynew = x * s + y * c;

	x = xnew;
	y = ynew;
}

void calculateXYThreads(std::map<int, int> &data_x, std::map<int, int> &data_y, std::vector<Point> &input_ref, std::vector<Point> input_newData)
{

    data_x.clear();
    data_y.clear();

    for (auto &i : input_ref)
        for (auto &j : input_newData)
        {

            data_x[round(i.x - j.x)]++;

            data_y[round(i.y - j.y)]++;
        }

    int max_x = 0;
    int max_y = 0;
    int dx = 0;
    int dy = 0;

    for (auto &i : data_x)
        if (max_x < i.second)
        {
            max_x = i.second;
            dx = i.first;
        }
    for (auto &i : data_y)
        if (max_y < i.second)
        {
            max_y = i.second;
            dy = i.first;
        }
}

Point calculateXY(std::map<int, LidarData> &referenceData, std::map<int, LidarData> &newData, int angle)
{

    refmapQ1_XY.clear();
    refmapQ2_XY.clear();
    refmapQ3_XY.clear();
    refmapQ4_XY.clear();

    for (auto i : referenceData)
    {
        float w = abs(i.second.angle);
        float x = sinus[int(w * 1000)] * i.second.distance / 10;
        float y = cosinus[int(w * 1000)] * i.second.distance / 10;

        rotatePoint(x, y, MainMap.rbegin()->angle);

        if (x > 0 && y > 0)
            refmapQ1_XY.push_back({x, y});
        if (x > 0 && y < 0)
            refmapQ2_XY.push_back({x, y});
        if (x < 0 && y > 0)
            refmapQ3_XY.push_back({x, y});
        if (x < 0 && y < 0)
            refmapQ4_XY.push_back({x, y});
    }

    mapQ1_XY.clear();
    mapQ2_XY.clear();
    mapQ3_XY.clear();
    mapQ4_XY.clear();

    for (auto &i : newData)
    {
        float w = abs(i.second.angle);
        float x = sinus[int(w * 1000)] * i.second.distance / 10;
        float y = cosinus[int(w * 1000)] * i.second.distance / 10;

        rotatePoint(x, y, angle + MainMap.rbegin()->angle);

        if (x > 0 && y > 0) mapQ1_XY.push_back({x, y});
        if (x > 0 && y < 0) mapQ2_XY.push_back({x, y});
        if (x < 0 && y > 0) mapQ3_XY.push_back({x, y});
        if (x < 0 && y < 0) mapQ4_XY.push_back({x, y});
    }


    std::future<void> xyThreads[4];


    xyThreads[0] = std::async(calculateXYThreads,std::ref(delta_X_Q1),std::ref(delta_Y_Q1),std::ref(refmapQ1_XY),std::ref(mapQ1_XY));
    xyThreads[1] = std::async(calculateXYThreads,std::ref(delta_X_Q2),std::ref(delta_Y_Q2),std::ref(refmapQ2_XY),std::ref(mapQ2_XY));
    xyThreads[2] = std::async(calculateXYThreads,std::ref(delta_X_Q3),std::ref(delta_Y_Q3),std::ref(refmapQ3_XY),std::ref(mapQ3_XY));
    xyThreads[3] = std::async(calculateXYThreads,std::ref(delta_X_Q4),std::ref(delta_Y_Q4),std::ref(refmapQ4_XY),std::ref(mapQ4_XY));

    for (size_t i = 0; i < 4; i++) xyThreads[i].get();

    std::map<int,int> gesX,gesY;

    for(auto& i : delta_X_Q1) gesX[i.first] += i.second;
    for(auto& i : delta_X_Q2) gesX[i.first] += i.second;
    for(auto& i : delta_X_Q3) gesX[i.first] += i.second;
    for(auto& i : delta_X_Q4) gesX[i.first] += i.second;


    for(auto& i : delta_Y_Q1) gesY[i.first] += i.second;
    for(auto& i : delta_Y_Q2) gesY[i.first] += i.second;
    for(auto& i : delta_Y_Q3) gesY[i.first] += i.second;
    for(auto& i : delta_Y_Q4) gesY[i.first] += i.second;


    int gesdx = 0;
    int gesdy = 0;

    int max_x_ges = 0;
    int max_y_ges = 0;

    for(auto& i : gesX) if(max_x_ges < i.second){
        max_x_ges = i.second;
        gesdx = i.first;
    }
    for(auto& i : gesY) if(max_y_ges < i.second){
        max_y_ges = i.second;
        gesdy = i.first;
    }

    return {(float) gesdx,(float) gesdy};

}