#include <iostream>
#include <string>
#include <vector>
#include <cmath>
#include <algorithm>
#include <sstream>

using namespace std;

/**
* Auto-generated code below aims at helping you parse
* the standard input according to the problem statement.
**/

class LocationPoint
{
public:
	double latitude;
	double longitude;

	LocationPoint()
	{
		latitude = longitude = 0;
	}

	LocationPoint(string _latitude, string _longitude)
	{
		// Replace "," by "." if "," founded.
		size_t pos = _latitude.find(',');		// size_t => type of unsigned int, can store max size of all array or object type.
		if (pos != string::npos)		// string::npos => static member constant value with the greatest possible value for an element of type size_t.
			_latitude[pos] = '.';

		pos = _longitude.find(',');
		if (pos != string::npos)
			_longitude[pos] = '.';

		// Convert string to double.
		latitude = stod(_latitude);
		longitude = stod(_longitude);
	}

	static double getDistance(const LocationPoint& _a, const LocationPoint& _b);
};

class Defibrilator
{
public:
	string id;
	string name;
	string address;
	string tel;
	LocationPoint location;

	Defibrilator(string _initString)
	{
		istringstream iss(_initString);
		string token;
		std::vector<string> values;
		while (getline(iss, token, ';'))
		{
			values.push_back(token);
		}

		id = values[0];
		name = values[1];
		address = values[2];
		tel = values[3];
		location = LocationPoint(values[5], values[4]);
	};
};

double LocationPoint::getDistance(const LocationPoint& _a, const LocationPoint& _b)
{
	double x;
	double y;
	double d;
	
	x = (_b.longitude - _a.longitude) * std::cos((_a.latitude + _b.latitude) / 2);
	y = (_b.latitude - _a.latitude);
	d = sqrt(pow(x, 2) + pow(y, 2)) * 6371;

	return (d);	//std::sqrt => racine carrée.
}

int main()
{
	string defibrilatorName;
	double minDist = -1;
	string LON;
	cin >> LON; cin.ignore();
	string LAT;
	cin >> LAT; cin.ignore();

	LocationPoint userPoint(LAT, LON);

	int N;
	cin >> N; cin.ignore();

	for (int i = 0; i < N; i++) {
		string DEFIB;
		getline(cin, DEFIB);
		Defibrilator d(DEFIB);

		double dist = LocationPoint::getDistance(userPoint, d.location);
		if ((minDist == -1) or (minDist > dist))
		{
			minDist = dist;
			defibrilatorName = d.name;
		}
	}

	// Write an action using cout. DON'T FORGET THE "<< endl"
	// To debug: cerr << "Debug messages..." << endl;

	cout << defibrilatorName << endl;

	return 0;
}