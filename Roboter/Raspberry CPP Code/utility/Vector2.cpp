#include "Vector2.h"


Vector2::Vector2(void) : x(0), y(0) { }
Vector2::Vector2(float xValue, float yValue) : x(xValue), y(yValue) { }
Vector2::Vector2(const Vector2& v) : x(v.x), y(v.y) { }

Vector2::~Vector2() {};

void Vector2::Set(float xValue, float yValue) { x = xValue; y = yValue; }

float Vector2::Length() const { return (float)sqrt(x * x + y * y); }
float Vector2::LengthSquared() const { return x * x + y * y; }
float Vector2::Distance(const Vector2& v) const { return sqrt(((x - v.x) * (x - v.x)) + ((y - v.y) * (y - v.y))); }
float Vector2::DistanceSquared(const Vector2& v) const { return ((x - v.x) * (x - v.x)) + ((y - v.y) * (y - v.y)); }
float Vector2::Scalar(const Vector2& v) const { return x * v.x + y * v.y; }
float Vector2::Cross(const Vector2& v) const { return { (x * v.y)-(y * v.x)}; }

Vector2& Vector2::Ortho() { Set(-y, x); return *this; }

Vector2& Vector2::Normalize()
{
    if (Length() != 0)
    {
        float len = Length();
        x /= len; y /= len;
        return *this;
    }

    x = y = 0;
    return *this;
}

std::ostream& operator <<(std::ostream& os, const Vector2& dt) { return os << dt.x << "|" << dt.y; };


