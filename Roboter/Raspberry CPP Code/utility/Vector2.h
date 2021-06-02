#pragma once

#include <math.h>
#include <iostream>

class Vector2
{
public:
	float x,y;
	friend std::ostream& operator<<(std::ostream& os, const Vector2& dt);

	Vector2();
	Vector2(float xValue, float yValue);
	Vector2(const Vector2& v);

	~Vector2(void);

	void Set(float xValue, float yValue);

	float Length() const;
	float LengthSquared() const;
	float Distance(const Vector2& v) const;
	float DistanceSquared(const Vector2& v) const;
	float Scalar(const Vector2& v) const;
	float Cross(const Vector2& v) const;
	Vector2& Ortho();

	Vector2& Normalize();

	inline Vector2& operator = (const Vector2& v) { x = v.x; y = v.y; return *this; }
	inline Vector2& operator = (const float& f) { x = f; y = f; return *this; }
	inline Vector2& operator - (void) { x = -x; y = -y; return *this; }
	inline bool operator == (const Vector2& v) const { return (x == v.x) && (y == v.y); }
	inline bool operator != (const Vector2& v) const { return (x != v.x) || (y != v.y); }
	inline bool operator < (const Vector2& rhs) const { return (x < rhs.x) || ((x == rhs.x) && (y < rhs.y)); }
	inline bool operator > (const Vector2& rhs) const { return (x > rhs.x) || ((x == rhs.x) && (y > rhs.y)); }


	inline const Vector2 operator + (const Vector2& v) const { return Vector2(x + v.x, y + v.y); }
	inline const Vector2 operator - (const Vector2& v) const { return Vector2(x - v.x, y - v.y); }
	inline const Vector2 operator * (const Vector2& v) const { return Vector2(x * v.x, y * v.y); }
	inline const Vector2 operator / (const Vector2& v) const { return Vector2(x / v.x, y / v.y); }

	inline Vector2& operator += (const Vector2& v) { x += v.x; y += v.y; return *this; }
	inline Vector2& operator -= (const Vector2& v) { x -= v.x; y -= v.y; return *this; }
	inline Vector2& operator *= (const Vector2& v) { x *= v.x; y *= v.y; return *this; }
	inline Vector2& operator /= (const Vector2& v) { x /= v.x; y /= v.y; return *this; }

	inline const Vector2 operator + (float v) const { return Vector2(x + v, y + v); }
	inline const Vector2 operator - (float v) const { return Vector2(x - v, y - v); }
	inline const Vector2 operator * (float v) const { return Vector2(x * v, y * v); }
	inline const Vector2 operator / (float v) const { return Vector2(x / v, y / v); }

	inline Vector2& operator += (float v) { x += v; y += v; return *this; }
	inline Vector2& operator -= (float v) { x -= v; y -= v; return *this; }
	inline Vector2& operator *= (float v) { x *= v; y *= v; return *this; }
	inline Vector2& operator /= (float v) { x /= v; y /= v; return *this; }

};
