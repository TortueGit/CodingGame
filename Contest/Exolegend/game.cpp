#include <iostream>
#include <string>
#include <vector>
#include <algorithm>
#include <sstream>
#include <random>

using namespace std;

enum ErrorLevel
{
	DEBUG = 0,
	DEBUG_INFOS = 1,
	INFOS = 2,
	ERROR = 5
};

#pragma region Constants
const char WALL = '#';
const char FLOOR = ' ';
const string ME_NAME = "me";
const string OPPONENT_NAME = "opponent";
const pair<int, int> NO_GOAL(-1, -1);
const int ERROR_LVL = ERROR; // 0 -> show every logs. // 10 -> We submit, so we don't want to see logs, because we don't have errors in our code ;). We select a big value in case of we have lvl of error logs, but we don't have yet, so it just for fun :D
#pragma endregion

#pragma region GlobalProperties
vector<string> errorStrings;
#pragma endregion

struct PacInput
{
	int pacId;
	bool mine; // true if this pac is yours
	int x, y;
	string type;
};

struct PelletInput
{
	int x, y, value;
};

class Tools
{
public:
	static const string GetPairString(pair<int, int> value) {
		stringstream ss_result;

		ss_result << "[" << value.first << "," << value.second << "]";
		return ss_result.str();
	};

	static void PrintErrors(int level) {	
		if (level >= ERROR_LVL)
		{
			for (auto str : errorStrings)
			{
				cerr << str << endl;
			}
		}
		errorStrings.clear();
	};

	template <typename T>
	static const bool IsInVector(vector<T> list, T value) {
		return find(list.begin(), list.end(), value) != list.end();
	};

	template <typename T>
	static const T GetRandomInVector(vector<T> list) {
		vector<T> result;

		sample(list.begin(), list.end(), back_inserter(result), 1, mt19937{random_device{}()});

		return result.at(0);
	};

	static const pair<int, int> GetRandomLocationFromList(vector<pair<int, int>> locationList) {
		vector<pair<int, int>> result;

		sample(locationList.begin(), locationList.end(), back_inserter(result), 1, mt19937{random_device{}()});

		return result.at(0);
	};

	static const pair<int, int> GetClosestPosInVector(vector<pair<int, int>> locationList, pair<int, int> pos) {
		vector<pair<int, pair<int, int>>> distPosList;

		stringstream infos;
		infos << "looking for pos: [" << Tools::GetPairString(pos) << "]\n";
		infos << "locationList: \n";
		for (auto loc : locationList)
		{
			infos << Tools::GetPairString(loc) << "\n";
		}

		for (auto location : locationList)
		{
			distPosList.push_back(make_pair(GetDist(pos, location), location));
		}

		sort(distPosList.begin(), distPosList.end(), [](pair<int, pair<int, int>> el1, pair<int, pair<int, int>> el2) { return el1.first < el2.first; });

		infos << "locationList sorted: \n";
		for (auto distPos : distPosList)
		{
			infos << Tools::GetPairString(distPos.second) << " dist: [" << distPos.first << "]" << "\n";
		}
		errorStrings.push_back(infos.str());
		Tools::PrintErrors(ErrorLevel::DEBUG_INFOS);

		return distPosList.at(0).second;
	};

	static const int GetDist(pair<int, int> initPos, pair<int, int> destPos)
	{	
		int distX;
		if (destPos.first > initPos.first)
		{
			distX = destPos.first - initPos.first;
		}
		else
		{
			distX = initPos.first - destPos.first;
		}
		
		int distY;
		if (destPos.second > initPos.second)
		{
			distY = destPos.second - initPos.second;
		}
		else
		{
			distY = initPos.second - destPos.second;
		}

		return distX + distY;
	}

	Tools() = delete;
};

class ApiEngine
{
public: 
	static pair<int, int> GetMapSizeInfos() {
		errorStrings.push_back("GetMapSizeInfos()!");
		Tools::PrintErrors(ErrorLevel::DEBUG);

		int width;	// size of the grid
		int height; // top left corner is (x=0, y=0)
		cin >> width >> height;
		cin.ignore();
		
		return make_pair(width, height);
	};

	static char** GetMapInfos(int height) {
		errorStrings.push_back("GetMapInfos(" + to_string(height) + ")!");
		Tools::PrintErrors(ErrorLevel::DEBUG);

		char** gameMap = new char*[height];
		for (int h = 0; h < height; h++)
		{
			string row;
			getline(cin, row); // one line of the grid: space " " is floor, pound "#" is wall			
			gameMap[h] = new char[row.length()];
			for (int w = 0; w < row.length(); w++)
			{
				gameMap[h][w] = row.data()[w];
			}
		}

		return gameMap;
	};

	static pair<int, int> GetScoreInfos() {
		errorStrings.push_back("GetScoreInfos()!");
		Tools::PrintErrors(ErrorLevel::DEBUG);

		int my_score;
		int opponent_score;
		cin >> my_score >> opponent_score;
		cin.ignore();

		return make_pair(my_score, opponent_score);
	};

	static vector<PacInput> GetVisiblePacs() {
		errorStrings.push_back("GetVisiblePacs()!");
		Tools::PrintErrors(ErrorLevel::DEBUG);

		vector<PacInput> resultList;

		int visible_pac_count; // all your pacs and enemy pacs in sight
		cin >> visible_pac_count;
		cin.ignore();

		for (int i = 0; i < visible_pac_count; i++)
		{
			PacInput pacInput;
			cin >> pacInput.pacId >> pacInput.mine >> pacInput.x >> pacInput.y >> pacInput.type;
			cin.ignore();

			resultList.push_back(pacInput);
		}

		return resultList;
	};

	static vector<PelletInput> GetVisiblePellets() {
		errorStrings.push_back("GetVisiblePellets()!");
		Tools::PrintErrors(ErrorLevel::DEBUG);

		vector<PelletInput> resultList;

		int visible_pellet_count; // all pellets in sight
		cin >> visible_pellet_count;
		cin.ignore();

		for (int i = 0; i < visible_pellet_count; i++)
		{
			PelletInput input;
			cin >> input.x >> input.y >> input.value;
			cin.ignore();

			resultList.push_back(input);
		}

		return resultList;
	};

	ApiEngine() = delete;
};

class Pac
{
private:
	int pacId;
	pair<int, int> location;
	string type;
	pair<int, int> destGoal;
	pair<int, int> previousLocation;
	vector<pair<int, int>> blockingDest;

public:
	Pac(int id, int x, int y, string type) {
		this->pacId = id;
		this->location = make_pair(x, y);
		this->type = type;
		this->destGoal = NO_GOAL;
	};

	const int GetId() {
		return this->pacId;
	};

	const pair<int, int> GetLocation() {
		return this->location;
	};

	void SetLocation(pair<int, int> pos) {
		this->location = pos;
		if (pos == this->destGoal)
		{
			errorStrings.push_back("RESET GOAL!!!");
			errorStrings.push_back(ToString());
			Tools::PrintErrors(ERROR);
			this->destGoal = NO_GOAL;
		}
	};

	const string GetType(){
		return this->type;
	};

	void SetType(string type) {
		this->type = type;
	};

	const pair<int, int> GetDestGoal() {
		return this->destGoal;
	};

	void SetDestGoal(pair<int, int> dest) {
		this->destGoal = dest;
	};

	const pair<int, int> GetPreviousLocation() {
		return this->previousLocation;
	};

	void SetPreviousLocation(pair<int, int> pos) {
		this->previousLocation = pos;
	};

	bool IsBlock() {
		return this->location == this->previousLocation;
	};

	bool CanGo(pair<int, int> dest)
	{
		errorStrings.push_back(ToString());
		Tools::PrintErrors(DEBUG_INFOS);

		return !Tools::IsInVector(this->blockingDest, dest);
	}

	void AddBlockingDest(pair<int, int> dest)
	{
		this->blockingDest.push_back(dest);
	}

	void ClearBlockingDest()
	{
		this->blockingDest.clear();
	}

	bool IsMovingForward()
	{
		return this->location.first == this->previousLocation.first + 1;
	}

	bool IsMovingBackward()
	{
		return this->location.first == this->previousLocation.first - 1;
	}

	bool IsMovingUp()
	{
		return this->location.second == this->previousLocation.second + 1;
	}

	bool IsMovingDown()
	{
		return this->location.second == this->previousLocation.second - 1;
	}

	const string ToString() {
		stringstream pacInfos;
		pacInfos << "PacID: [" << this->pacId << "]" << "\n";
		pacInfos << "Location: " << Tools::GetPairString(this->location) << "\n";
		pacInfos << "DestGoal: " << Tools::GetPairString(this->destGoal) << "\n";
		pacInfos << "PreviousLocation: " << Tools::GetPairString(this->previousLocation) << "\n";
		pacInfos << "IsBlock: " << IsBlock() << "\n";
		pacInfos << "BlockingDest: " << this->blockingDest.size() << "\n";
		pacInfos << "Type: " << this->type << "\n";

		return pacInfos.str();
	};
	
	const bool operator ==(const Pac& pac)
	{
		return this->pacId == pac.pacId;
	};
};

class WorldMap
{
private:
	int mapWidth;
	int mapHeight;
	char** mapInfos;
	vector<pair<int, int>> bigPelletList;
	vector<pair<int, int>> smallPelletList;
	vector<pair<int, int>> validLocations;

public:
	WorldMap(int width, int height)
	{
		this->mapWidth = width;
		this->mapHeight = height;
		this->mapInfos = new char*[height];
		for (int i = 0; i < height; i++) {
			this->mapInfos[i] = new char[width];
		}
	};

	~WorldMap()
	{
	};

	void SetMapInfos(char* infos[])
	{ 
		errorStrings.push_back("SetMapInfos(" + to_string(sizeof(infos)) + ")! \n");
		Tools::PrintErrors(ErrorLevel::DEBUG);

		this->mapInfos = infos;
		PrintMapInfos();
		SetValidLocations();
	};

	void PrintMapInfos() {
		stringstream infos;
		infos << "Map: \n";
		infos << "mapHeight: " << this->mapHeight << "\n";
		infos << "mapInfos size: " << sizeof(this->mapInfos) << "\n";
		for (int h = 0; h < this->mapHeight; h++)
		{			
			infos << this->mapInfos[h] << "\n";
		}

		errorStrings.push_back(infos.str());
		Tools::PrintErrors(ErrorLevel::DEBUG_INFOS);
	};

	const vector<pair<int, int>> GetBigPelletList() { return this->bigPelletList; };
	
	const vector<pair<int, int>> GetSmallPelletList() { return this->smallPelletList; };
	
	void AddBigPellet(pair<int, int> pos) { this->bigPelletList.push_back(pos); };
	
	void ClearBigPelletList() { this->bigPelletList.clear(); };
	
	void AddSmallPellet(pair<int, int> pos) { this->smallPelletList.push_back(pos); };
	
	void ClearSmallPelletList() { this->smallPelletList.clear(); };
	
	bool IsPosInBigPellets(pair<int, int> pos) { return Tools::IsInVector(this->bigPelletList, pos); };
	
	bool IsPosInSmallPellets(pair<int, int> pos) { return Tools::IsInVector(this->smallPelletList, pos); };
	
	pair<int, int> GetRandomValidLocation() {
		errorStrings.push_back("GetRandomValidLocation()");
		errorStrings.push_back("validLocations size [" + to_string(this->validLocations.size()) + "]");
		Tools::PrintErrors(ErrorLevel::DEBUG_INFOS);
		return Tools::GetRandomInVector(this->validLocations);
	};

	void PrintBigPelletList(ErrorLevel lvl) {
		errorStrings.push_back("WorldMap BigPellet list:");
		for (auto pos : this->bigPelletList)
		{
			errorStrings.push_back(Tools::GetPairString(pos));
		}

		Tools::PrintErrors(lvl);
	}

private:
	void SetValidLocations() {
		errorStrings.push_back("SetValidLocations");
		errorStrings.push_back("mapHeight: [" + to_string(this->mapHeight) + "]");
		errorStrings.push_back("mapWidth: [" + to_string(this->mapWidth) + "]");
		Tools::PrintErrors(ErrorLevel::DEBUG_INFOS);
		for (int h = 0; h < this->mapHeight; h++)
		{
			for (int w = 0; w < this->mapWidth; w++)
			{
				if (!IsObstacle(this->mapInfos[h][w]))
				{
					this->validLocations.push_back(make_pair(w, h));
				}
			}
		}

		errorStrings.push_back("validLocations size [" + to_string(this->validLocations.size()) + "]");
		Tools::PrintErrors(ErrorLevel::DEBUG_INFOS);
	};

	bool IsObstacle(char mapValue) {
		if (mapValue == WALL)
		{
			return true;
		}

		return false;
	};
};

struct Player
{
	string name;
	int score;
	vector<Pac*> pacs;
};

class Game
{
private:
	bool isFirstTurn;
	WorldMap* worldMap;
	Player me;
	Player opponent;
	vector<pair<int, int>> destGoals;
	vector<pair<int, int>> knownPellets;
	vector<pair<int, int>> knownLocationList;

public:
	Game() {
		errorStrings.push_back("Game Init!");
		Tools::PrintErrors(ErrorLevel::DEBUG);
		this->me = Player({ME_NAME});
		this->opponent = Player({OPPONENT_NAME});

		pair<int, int> mapSizeInfos = ApiEngine::GetMapSizeInfos();
		this->worldMap = new WorldMap(mapSizeInfos.first, mapSizeInfos.second);
		this->worldMap->SetMapInfos(ApiEngine::GetMapInfos(mapSizeInfos.second));

		this->isFirstTurn = true;
	};

	~Game() {
		delete this->worldMap;
	};

	void GetTurnInfos() {
		SetScoreInfos();
		PrintScoreInfos();
		PrintPacInfos(DEBUG_INFOS);
		GetPacInfos();
		PrintPacInfos(INFOS);
		GetPelletInfos();
		PrintPelletsInfos();
		ResolvePelletAting(me.pacs);
		ResolvePelletAting(opponent.pacs);
	}

	void EndTurn() {
		errorStrings.push_back("START END TURN!");
		Tools::PrintErrors(ERROR);

		SetPacsGoal();

		if (this->isFirstTurn)
		{
			this->isFirstTurn = false;
		}
		SetPacsPreviousLocation(me.pacs);
		SetPacsPreviousLocation(opponent.pacs);
		WriteActions();
	};

private:
	void SetPacsPreviousLocation(vector<Pac*> pacList) {
		for (Pac* pac : pacList) {

			errorStrings.push_back(pac->ToString());
			Tools::PrintErrors(INFOS);

			if (!pac->IsBlock())
			{
				errorStrings.push_back("PAC IS NOT BLOCKED CLEAR BLOCKING DEST!!!");
				Tools::PrintErrors(INFOS);
				pac->ClearBlockingDest();
			}
			else
			{
				errorStrings.push_back("PAC IS BLOCK!!!");
				Tools::PrintErrors(INFOS);
				pac->AddBlockingDest(pac->GetDestGoal());
			}

			pac->SetPreviousLocation(pac->GetLocation());
			this->knownLocationList.push_back(pac->GetLocation());
		}
	};

	void ResolvePelletAting(vector<Pac*> pacList) {
		for (Pac* pac : pacList)
		{
			if (Tools::IsInVector(this->knownPellets, pac->GetLocation()))
			{
				RemovePelletKnownLocation(pac->GetLocation());
			}
		}		
	};

	void ClearProperties() {
		this->worldMap->ClearBigPelletList();
		this->worldMap->ClearSmallPelletList();
		for (auto pac : this->opponent.pacs)
		{
			delete pac;
		}
		this->opponent.pacs.clear();
	};

	void SetScoreInfos() {
		pair<int, int> scoreInfos = ApiEngine::GetScoreInfos();
		ClearProperties();
		me.score = scoreInfos.first;
		opponent.score = scoreInfos.second;
		
		errorStrings.push_back("START TURN!");
		Tools::PrintErrors(ERROR);
	}

	void PrintScoreInfos() {
		stringstream infos;
		infos << "My score: [" << me.score << "]\n";
		infos << "Opponent score: [" << opponent.score << "]\n";

		errorStrings.push_back(infos.str());
		Tools::PrintErrors(ErrorLevel::DEBUG_INFOS);
	};

	void GetPacInfos() {
		vector<PacInput> visiblePacInfoList = ApiEngine::GetVisiblePacs();
		for (auto pac : visiblePacInfoList)
		{
			if (pac.mine)
			{
				AddOrUpdatePac(this->me, pac);
			}
			else
			{
				AddOrUpdatePac(this->opponent, pac);
			}
		}
	}

	void PrintPacInfos(ErrorLevel lvl) {
		stringstream infos;
		infos << "MyPacs: \n";
		for (auto pac : me.pacs)
		{
			infos << pac->ToString() << "\n";
		}
		infos << "OpponentPacs: \n";
		for (auto pac : opponent.pacs)
		{
			infos << pac->ToString() << "\n";
		}

		errorStrings.push_back(infos.str());
		Tools::PrintErrors(lvl);
	}

	void GetPelletInfos() {
		vector<PelletInput> visiblePelletInfoList = ApiEngine::GetVisiblePellets();
		for (auto pellet : visiblePelletInfoList)
		{
			pair<int, int> pelletPos = make_pair(pellet.x, pellet.y);
			if (pellet.value > 1)
			{
				this->worldMap->AddBigPellet(pelletPos);
			}
			else
			{
				this->worldMap->AddSmallPellet(pelletPos);
			}

			AddPelletKnownLocation(pelletPos);
		}
	}

	void AddPelletKnownLocation(pair<int, int> pos)
	{
		if (!Tools::IsInVector(this->knownPellets, pos))
		{
			this->knownPellets.push_back(pos);
		}
	}

	void RemovePelletKnownLocation(pair<int, int> pos)
	{		
		errorStrings.push_back("REMOVE KNOWN PELLET " + Tools::GetPairString(pos));
		errorStrings.push_back("KNOWN PELLET SIZE BEFORE" + to_string(this->knownPellets.size()));
		Tools::PrintErrors(ERROR);
		this->knownPellets.erase(remove(this->knownPellets.begin(), this->knownPellets.end(), pos));
		errorStrings.push_back("KNOWN PELLET SIZE AFTER" + to_string(this->knownPellets.size()));
	}

	void PrintPelletsInfos() {
		stringstream infos;
		infos << "BigPellets: \n";
		for (auto pos : this->worldMap->GetBigPelletList())
		{
			infos << Tools::GetPairString(pos) << "\n";
		}
		
		infos << "SmallPellets: \n";
		for (auto pos : this->worldMap->GetSmallPelletList())
		{
			infos << Tools::GetPairString(pos) << "\n";
		}

		errorStrings.push_back(infos.str());
		Tools::PrintErrors(ErrorLevel::DEBUG);
	}

	void AddOrUpdatePac(Player& player, const PacInput& pacInfos) {
		if (this->isFirstTurn || player.name == OPPONENT_NAME)
		{
			AddPac(player, pacInfos);
		}
		else
		{
			UpdatePac(player, pacInfos);
		}
	};

	void AddPac(Player& player, const PacInput& pacInfos)
	{
		player.pacs.push_back(new Pac(pacInfos.pacId, pacInfos.x, pacInfos.y, pacInfos.type));
	}

	void UpdatePac(Player& player, const PacInput& pacInfos)
	{
		auto it = find_if(player.pacs.begin(), player.pacs.end(), [pacInfos](Pac* pac) { return pac->GetId() == pacInfos.pacId; });

		if ((*it) != nullptr)
		{
			Pac& pac = *(*it);
			pac.SetLocation(make_pair(pacInfos.x, pacInfos.y));
			pac.SetType(pacInfos.type);
			Tools::PrintErrors(INFOS);
			return;
		}
	}

	void SetPacsGoal() {
		if (this->isFirstTurn || this->worldMap->GetBigPelletList().size() > 0)
		{
			SetClosestBigPelletToPacs();
		}

		for (Pac* pac : this->me.pacs) 
		{
			if (pac->GetType() == "DEAD")
				continue;

			if (ShouldRecalculateGoal(*pac)) {
				pair<int, int> goal = FindGoal(*pac);
				if (pac->GetDestGoal() != NO_GOAL)
				{
					this->destGoals.erase(find(this->destGoals.begin(), this->destGoals.end(), pac->GetDestGoal()));
				}
				pac->SetDestGoal(goal);
				this->destGoals.push_back(goal);
			}
		}
	};

	void SetClosestBigPelletToPacs() {
		errorStrings.push_back("SetPacsGoal FIRST TURN!");
			Tools::PrintErrors(ERROR);
			vector<pair<int, int>> myPosList;
			struct ClosestPacPos
			{
				pair<int, int> pacPos;
				pair<int, int> bigPellet;
				int dist;
			};
			vector<ClosestPacPos> pacPelletDistList;

			for (auto pac : this->me.pacs)
			{
				myPosList.push_back(pac->GetLocation());
			}
			for (auto bigPellet : this->worldMap->GetBigPelletList())
			{
				ClosestPacPos cpp = ClosestPacPos();			
				cpp.pacPos = Tools::GetClosestPosInVector(myPosList, bigPellet);
				cpp.bigPellet = bigPellet;
				cpp.dist = Tools::GetDist(cpp.pacPos, cpp.bigPellet);
				pacPelletDistList.push_back(cpp);
			}
			for (auto cpp : pacPelletDistList)
			{
				auto it = find_if(this->me.pacs.begin(), this->me.pacs.end(), [&](Pac* pac) { return cpp.pacPos == pac->GetLocation(); });
				errorStrings.push_back("PAC ID [" + to_string((*it)->GetId()) + "]");
				errorStrings.push_back("POS PAC [" + Tools::GetPairString((*it)->GetLocation()) + "]");
				errorStrings.push_back("PAC GOAL [" + Tools::GetPairString((*it)->GetDestGoal()) + "]");
				errorStrings.push_back("POS PELLET [" + Tools::GetPairString(cpp.bigPellet) + "]");
				errorStrings.push_back("DIST [" + to_string(cpp.dist) + "]");
				Tools::PrintErrors(ERROR);
				if ((*it)->GetDestGoal() == NO_GOAL)
				{
					(*it)->SetDestGoal(cpp.bigPellet);
					this->destGoals.push_back(cpp.bigPellet);
					errorStrings.push_back("NO GOAL SET [" + Tools::GetPairString((*it)->GetDestGoal()) + "]");
					Tools::PrintErrors(ERROR);
				}
				else if (Tools::GetDist((*it)->GetLocation(), (*it)->GetDestGoal()) > cpp.dist)
				{
					this->destGoals.erase(remove(this->destGoals.begin(), this->destGoals.end(), cpp.bigPellet));
					(*it)->SetDestGoal(cpp.bigPellet);
					this->destGoals.push_back(cpp.bigPellet);
					errorStrings.push_back("CLOSEST GOAL SET [" + Tools::GetPairString((*it)->GetDestGoal()) + "]");
					Tools::PrintErrors(ERROR);
				}
				Tools::PrintErrors(ERROR);
			}
			Tools::PrintErrors(ERROR);
	};

	void WriteActions() {
		// Write an action using cout. DON'T FORGET THE "<< endl"
		stringstream actions;
		for (Pac* pac : this->me.pacs)
		{
			errorStrings.push_back("WRITE ACTIONS FOR PACS:");
			errorStrings.push_back(pac->ToString());
			Tools::PrintErrors(DEBUG_INFOS);
			if (pac->GetType() == "DEAD")
				continue;

			if (actions.str().length() > 0)
			{
				actions << "|";
			}
			actions << "MOVE " << pac->GetId() << " " << pac->GetDestGoal().first << " " << pac->GetDestGoal().second;
		}

		errorStrings.push_back("END TURN!");
		Tools::PrintErrors(ERROR);
		cout << actions.str() << endl; // MOVE <pacId> <x> <y>
	};

	bool ShouldRecalculateGoal(Pac& pac) {
		errorStrings.push_back("SHOULD RECALCULATE FOR PAC(" + to_string(pac.GetId()) + ")");
		Tools::PrintErrors(ERROR);
		if (pac.GetDestGoal() == NO_GOAL)
		{
			errorStrings.push_back("NO GOAL!");
			Tools::PrintErrors(ERROR);
			return true;
		}

		if (pac.IsBlock() && !this->worldMap->IsPosInBigPellets(pac.GetDestGoal()))
		{
			errorStrings.push_back("IS BLOCK!");
			errorStrings.push_back(pac.ToString());
			errorStrings.push_back(to_string(this->GetValidPelletLocationList(pac).size()));
			Tools::PrintErrors(ERROR);
			return true;
		}
		
		if (!this->worldMap->IsPosInBigPellets(pac.GetDestGoal()) && !Tools::IsInVector(this->knownLocationList, pac.GetDestGoal()))
		{
			errorStrings.push_back("GOAL NO LONGER EXIST!");
			Tools::PrintErrors(ERROR);
			return true;
		}

		if (this->knownPellets.size() < 5) {
			errorStrings.push_back("SHOULD EXPLORE MAP!");
			Tools::PrintErrors(ERROR);
			return true;
		}

		errorStrings.push_back("KEEP GOAL!");
		Tools::PrintErrors(ERROR);
		return false;
	}

	pair<int, int> FindGoal(Pac& pac) {
		vector<pair<int, int>> remainingBigPelletList = GetRemainingBigPelletList(pac);

		if (remainingBigPelletList.size() > 0) {
			pair<int, int> goal = Tools::GetClosestPosInVector(remainingBigPelletList, pac.GetLocation());
			errorStrings.push_back("FindGoal(" + to_string(pac.GetId()) + "): ");
			errorStrings.push_back("BigPellet GOAL: " + Tools::GetPairString(goal));
			Tools::PrintErrors(ERROR);
			return goal;
		}

		vector<pair<int, int>> validPelletLocation = GetValidPelletLocationList(pac);
		if (validPelletLocation.size() > 0
			|| pac.IsBlock() && !validPelletLocation.size() == 1)
		{
			pair<int, int> goal = Tools::GetClosestPosInVector(validPelletLocation, pac.GetLocation());
			errorStrings.push_back("FindGoal(" + to_string(pac.GetId()) + "): ");
			errorStrings.push_back("SmallPellet GOAL: " + Tools::GetPairString(goal));
			Tools::PrintErrors(ERROR);
			return goal;
		}

		pair<int, int> defaultGoal = this->worldMap->GetRandomValidLocation();
		errorStrings.push_back("FindGoal(" + to_string(pac.GetId()) + "): ");
		errorStrings.push_back("GetRandomValidLocation [" + Tools::GetPairString(defaultGoal) + "]");
		Tools::PrintErrors(ERROR);
		return this->worldMap->GetRandomValidLocation();
	};

	vector<pair<int, int>> GetRemainingBigPelletList(Pac& pac) {
		vector<pair<int, int>> remainingBigPelletList;
		this->worldMap->PrintBigPelletList(ErrorLevel::DEBUG_INFOS);
		
		for (auto bigPellet : this->worldMap->GetBigPelletList())
		{
			if (!Tools::IsInVector(this->destGoals, bigPellet) && pac.CanGo(bigPellet))
			{
				remainingBigPelletList.push_back(bigPellet);
			}
		}

		errorStrings.push_back("RemainingBigPelletList size [" + to_string(remainingBigPelletList.size()) + "]");
		Tools::PrintErrors(ErrorLevel::DEBUG_INFOS);

		return remainingBigPelletList;
	};

	vector<pair<int, int>> GetValidPelletLocationList(Pac& pac) {		
		vector<pair<int, int>> validPelletLocationList;
		errorStrings.push_back("Visible Small Pellets size [" + to_string(this->worldMap->GetSmallPelletList().size()) + "]");
		Tools::PrintErrors(ErrorLevel::ERROR);

		if (this->worldMap->GetSmallPelletList().size() > 0) 
		{
			for (auto pellet : this->worldMap->GetSmallPelletList())
			{
				if (IsNextTo(pac.GetLocation(), pellet))
				{
					errorStrings.push_back("PACID: " + to_string(pac.GetId()));
					errorStrings.push_back("LOCATION: " + Tools::GetPairString(pac.GetLocation()));
					errorStrings.push_back("PELLET: " + Tools::GetPairString(pellet));
					Tools::PrintErrors(ErrorLevel::ERROR);
					validPelletLocationList.push_back(pellet);
				}
			}

			if (validPelletLocationList.size() > 0)
			{
				if (validPelletLocationList.size() == 1 && pac.IsBlock())
				{
					errorStrings.push_back("ISNEXT TO BLOCKING DEST!!!! size [" + to_string(validPelletLocationList.size()) + "]");
					Tools::PrintErrors(ErrorLevel::ERROR);
					pac.AddBlockingDest(validPelletLocationList.at(0));
					validPelletLocationList.pop_back();
				}
				else
				{
					errorStrings.push_back("ISNEXT TO validPelletLocation size [" + to_string(validPelletLocationList.size()) + "]");
					Tools::PrintErrors(ErrorLevel::ERROR);
					return validPelletLocationList;
				}
			}
		}


		for (auto pellet : this->knownPellets)
		{
			if (pac.CanGo(pellet))
			{
				validPelletLocationList.push_back(pellet);
			}
		}

		errorStrings.push_back("validPelletLocation size [" + to_string(validPelletLocationList.size()) + "]");
		Tools::PrintErrors(ErrorLevel::ERROR);

		return validPelletLocationList;
	};

	bool IsNextTo(pair<int, int> initPos, pair<int, int> destPos)
	{
		return Tools::GetDist(initPos, destPos) == 1;
	}
};

/**
 * Grab the pellets as fast as you can!
 **/
int main()
{
	Game* game = new Game();

	// game loop
	while (1)
	{
		game->GetTurnInfos();

		// Write an action using cout. DON'T FORGET THE "<< endl"
		// To debug: cerr << "Debug messages..." << endl;
		
		game->EndTurn();
	}
}
