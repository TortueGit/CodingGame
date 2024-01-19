#include <iostream>
#include <string>
#include <vector>
#include <algorithm>
#include <sstream>
#include <random>

using namespace std;

struct Pac {
    int pacId;
    int x;
    int y;
    string type;
    pair<int, int> destGoal;
};

#pragma region Constants
const char WALL = '#';
const char FLOOR = ' ';
const pair<int, int> NO_GOAL(-1, -1);
#pragma endregion

#pragma region MainProperties
int mapWidth;
int mapHeight;
char worldMap[35 * 17];

vector<Pac> myPacs;

vector<pair<int, int>> bigPellets;
vector<pair<int, int>> smallPellets;
vector<pair<int, int>> destGoals;
vector<pair<int, int>> validLocations;

vector<string> errorStrings;
int errorLvl = 10;	// 0 -> show every logs. // 10 -> We submit, so we don't want to see logs, because we don't have errors in our code ;). We select a big value in case of we have lvl of error logs, but we don't have yet, so it just for fun :D
#pragma endregion

#pragma region ToolsMethods
void ClearProperties();
void InitProperties();
char GetMap(int x, int y, char* mapWorld[]);
bool IsObstacle(char mapValue);
void ErrorOutput(int level = 0);
void PrintPacInfos(const Pac &pac);
string GetPairString(pair<int, int> value);
int GetRandom(int start, int end);
pair<int, int> GetValidLocation();
pair<int, int> GetValidPelletLocation();
void SetValidLocations(int width, int height, char *map[]);
#pragma endregion

#pragma region BrainMethods
void SetGoal(Pac *pac, int width, int height, char* map[]);
#pragma endregion

/**
 * Grab the pellets as fast as you can!
 **/
int main() {
    bool isFirstTurn = true;
    int width;  // size of the grid
    int height; // top left corner is (x=0, y=0)
    cin >> width >> height;
    char* gameMap[height*width];
    cin.ignore();
    
    errorStrings.push_back("DEGUG : map " + to_string(width) + "x" + to_string(height) + " !");
    for (int i = 0; i < height; i++) {
        string row;
        getline(cin, row); // one line of the grid: space " " is floor, pound "#" is wall
        gameMap[i] = row.data();
        errorStrings.push_back(gameMap[i]);
    }
    ErrorOutput(0);

    SetValidLocations(width, height, gameMap);

    // game loop
    while (1) {
        ClearProperties();
        int my_score;
        int opponent_score;
        cin >> my_score >> opponent_score; cin.ignore();

        int visible_pac_count; // all your pacs and enemy pacs in sight
        cin >> visible_pac_count; cin.ignore();

        for (int i = 0; i < visible_pac_count; i++) {
            Pac inputPac;
            bool mine; // true if this pac is yours
            cin >> inputPac.pacId >> mine >> inputPac.x >> inputPac.y >> inputPac.type; cin.ignore();

            PrintPacInfos(inputPac);

            if (mine) {
                if (isFirstTurn) {
                    inputPac.destGoal = NO_GOAL;
                    myPacs.push_back(inputPac);
                } else {
                    vector<Pac>::iterator myPac = find_if(myPacs.begin(), myPacs.end(), [&inputPac](const Pac pac)
                                             { return pac.pacId == inputPac.pacId; });
                    myPac->x = inputPac.x;
                    myPac->y = inputPac.y;

	                errorStrings.push_back("I know this pac!");
                    PrintPacInfos(*myPac);
                }
            }
        }

        int visible_pellet_count; // all pellets in sight
        cin >> visible_pellet_count; cin.ignore();

        for (int i = 0; i < visible_pellet_count; i++) {
            int x;
            int y;
            int value; // amount of points this pellet is worth
            cin >> x >> y >> value; cin.ignore();

            if (value > 1) {
                bigPellets.push_back(make_pair(x, y));
            } else {
                smallPellets.push_back(make_pair(x, y));
            }
        }

        // Write an action using cout. DON'T FORGET THE "<< endl"
        // To debug: cerr << "Debug messages..." << endl;
        stringstream actions;
        for (Pac& pac : myPacs)
        {
            stringstream pacString;
            pacString << "pacId: " << pac.pacId;
            errorStrings.push_back(pacString.str());
            ErrorOutput(0);
            SetGoal(&pac, width, height, gameMap);
	        errorStrings.push_back("After SetGoal!");
            PrintPacInfos(pac);
            if (actions.str().length() > 0) {
                actions << "|";
            }
            actions << "MOVE " << pac.pacId << " " << pac.destGoal.first << " " << pac.destGoal.second;
        }

        cout << actions.str() << endl; // MOVE <pacId> <x> <y>

        if (isFirstTurn) {
            isFirstTurn = false;
        }

        for (Pac pac : myPacs) {
	        errorStrings.push_back("Just before the end !");
            PrintPacInfos(pac);
        }
    }
}

#pragma region Tools
void ClearProperties() {
	errorStrings.push_back("ClearProperties() -- BEGIN !");
	ErrorOutput(0);

    bigPellets.clear();
    smallPellets.clear();
    destGoals.clear();

    errorStrings.push_back("ClearProperties() -- END !");
	ErrorOutput(0);
}

void InitProperties() {
	errorStrings.push_back("InitProperties() -- BEGIN !");
	ErrorOutput(0);

	ClearProperties();

	errorStrings.push_back("InitProperties() -- END !");
	ErrorOutput(0);
}

char GetMap(int x, int y, char* mapWorld[]) {
    if (x < 0 
            || x >= mapWidth 
            || y < 0 
            || y >= mapHeight) {
        return '#';
    }

    return mapWorld[y][x];
}

bool IsObstacle(char mapValue) {
    if (mapValue == WALL) {
        return true;
    }

    return false;
}

void ErrorOutput(int level) {
	if (level >= errorLvl) {
		for (auto str : errorStrings) {
			cerr << str << endl;
		}
	}
	errorStrings.clear();
}

void PrintPacInfos(const Pac &pac) {
    stringstream pacInfos;
    pacInfos << "PacID: [" << pac.pacId << "]";
    pacInfos << "X: [" << pac.x<< "]";
    pacInfos << "Y: [" << pac.y<< "]";
    pacInfos << "DestGoal: " << GetPairString(pac.destGoal);
    
    errorStrings.push_back(pacInfos.str());
    ErrorOutput(0);
}

string GetPairString(pair<int, int> value) {
    stringstream ss_result;
    
    ss_result << "[" << value.first << "," << value.second << "]";
    return ss_result.str();
}

int GetRandom(int start, int end) {    
    random_device rd; // obtain a random number from hardware
    mt19937 gen(rd()); // seed the generator
    uniform_int_distribution<> distr(start, end); // define the range

    return distr(gen);
}

pair<int, int> GetValidLocation() {
    vector<pair<int, int>> result;

    sample(validLocations.begin(), validLocations.end(), back_inserter(result), 1, mt19937{random_device{}()});

    return result.at(0);
}

pair<int, int> GetValidPelletLocation() {
    vector<pair<int, int>> result;

    sample(smallPellets.begin(), smallPellets.end(), back_inserter(result), 1, mt19937{random_device{}()});

    return result.at(0);
}

void SetValidLocations(int width, int height, char *map[]) {
    for (int h = 0; h < height; h++) {
        for (int w = 0; w < width; w++) {
            if (!IsObstacle(map[h][w])) {
                validLocations.push_back(make_pair(w, h));
            }
        }
    }
}
#pragma endregion

#pragma region Brain
void SetGoal(Pac *pac, int width, int height, char* map[]) {
	errorStrings.push_back("SetGoal(Pac pac) -- BEGIN !");
	ErrorOutput(0);
    PrintPacInfos(*pac);

    if (pac->destGoal != NO_GOAL) {
        if (find(bigPellets.begin(), bigPellets.end(), pac->destGoal) != bigPellets.end() 
                || find(smallPellets.begin(), smallPellets.end(), pac->destGoal) != smallPellets.end()) {
            errorStrings.push_back("SetGoal(Pac pac) -- NO_GOAL END !");
            ErrorOutput(0);
            return;
        }
    }

    for (pair<int, int> bigPellet : bigPellets) {
        errorStrings.push_back(GetPairString(bigPellet));
        ErrorOutput(0);
        if (find(destGoals.begin(), destGoals.end(), bigPellet) == destGoals.end())
        {
            pac->destGoal = bigPellet;
            destGoals.push_back(bigPellet);
            errorStrings.push_back("SetGoal(Pac pac) -- DEFINED GOAL END !");
            stringstream destGoal;
            destGoal << pac->destGoal.first << " " << pac->destGoal.second;
            errorStrings.push_back(destGoal.str());
            ErrorOutput(0);
            return;
        }
    }

    if (smallPellets.size() > 0) {
        pac->destGoal = GetValidPelletLocation();
    } else {
        pac->destGoal = GetValidLocation();
    }
    errorStrings.push_back("SetGoal(Pac pac) -- END !");
    ErrorOutput(0);
}
#pragma endregion
