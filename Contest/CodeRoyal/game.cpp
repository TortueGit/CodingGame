#include <iostream>
#include <string>
#include <vector>
#include <algorithm>
#include <cmath>

using namespace std;

struct Site
{
	int siteId;
	int x;
	int y;
	int radius;
	int goldRemain; // used in future leagues
	int maxMineSize; // used in future leagues
	int structureType; // -1 = No structure, 0 = Goldmine, 1 = Tower, 2 = Barracks
	int owner; // -1 = No structure, 0 = Friendly, 1 = Enemy
	int param1;
	int param2;
};

struct Unit
{
	int x;
	int y;
	int owner;
	int unitType; // -1 = QUEEN, 0 = KNIGHT, 1 = ARCHER
	int health;
};

#pragma region MainProperties
Unit queen;
Unit enemyQueen;
std::pair<int, int> homePosition;
bool isGoBackHome;
bool areWeInDanger;

int gold;
int touchedSite; // -1 if none
int goldEarnByTurn;
int goldEarnByTurnEnemy;

int towerIdToUpgrade = -1;
int towerIdHealth = -1;

std::vector<Site> allSites;
std::vector<Unit> allUnits;

std::vector<Site> allyArcherySites;
std::vector<Site> allyKnightSites;
std::vector<Site> allyGiantSites;
std::vector<Site> allyTowerSites;
std::vector<Site> allyMineSites;

std::vector<Unit> allyKnightUnits;
std::vector<Unit> allyArcherUnits;
std::vector<Unit> allyGiantUnits;

std::vector<Site> enemyArcherySites;
std::vector<Site> enemyKnightSites;
std::vector<Site> enemyGiantSites;
std::vector<Site> enemyTowerSites;
std::vector<Site> enemyMinesSites;
std::vector<Site> enemySites;

std::vector<Unit> enemyKnightUnits;
std::vector<Unit> enemyArcherUnits;
std::vector<Unit> enemyGiantUnits;
std::vector<Unit> enemyUnits;

std::vector<Site> emptySites;

std::vector<int> emptyMinesId;

std::vector<string> errorStrings;

int errorLvl = 10;	// 0 -> show every logs.	// 10 -> We sumbmit, so we don't want to see logs, becouse we don't have errors in our code ;). We select a big value in case of we have lvl of error logs, but we don't have yet, so it just for fun :D
#pragma endregion


#pragma region ToolsMethods
void initProperties();

Unit getQueen(int owner);
std::vector<Unit> getUnitsByType(int _owner, int _type);

int getSiteIndexById(int _siteId, std::vector<Site> _sites);
std::vector<Site> getSitesByTypeAndOwner(int _structureType, int _owner, int _param1 = -1, int _param2 = -1);
std::vector<Site> getSiteToBuild();
Unit getClosestUnit(std::vector<Unit> &_units, int _unitType);
Site getClosestSite(std::vector<Site> &_sites, int _checkSafety = -1);
bool isSiteUnderTowerRange(Site _site, std::vector<Site> _towerSites);
bool isUnitUnderTowerRange(Unit _unit, std::vector<Site> _towerSites);
double getDistance(std::pair<int, int> _pos1, std::pair<int, int> _pos2);

std::vector<Site> getEmptySites(int _type);	// 0 - no filter; 1 - mine filter; 

void errorOutput(int _level = 0);
#pragma endregion

#pragma region BrainMethods
void performAction();
void performTraining();
void trainKnights();
void trainGiants();
bool buildBarracks();
Site getMineToUpgrade();
Site getTowerToUpgrade();
#pragma endregion

#pragma region ActionsStringConstructionMethods
bool moveToPoint(int x, int y);
void buildTower(int _siteId);
void buildBarrack(int _siteId, std::string _unitType);
void buildMine(int _siteId);

void train();
void train(std::vector<int> _siteIds);
#pragma endregion


/**
* Auto-generated code below aims at helping you parse
* the standard input according to the problem statement.
**/
int main()
{
	bool areWeMoving = false;
	int previousSiteId = -1;
	int siteToReached = -1;
	int castleId = -1;
	int previousHealth = -1;

	int numSites;
	cin >> numSites; cin.ignore();
	for (int i = 0; i < numSites; i++) {
		int siteId;
		int x;
		int y;
		int radius;
		cin >> siteId >> x >> y >> radius; cin.ignore();
		allSites.push_back(Site());
		allSites.back().siteId = siteId;
		allSites.back().x = x;
		allSites.back().y = y;
		allSites.back().radius = radius;
	}

	// game loop
	while (1) {
		goldEarnByTurn = 0;
		goldEarnByTurnEnemy = 0;

		cin >> gold >> touchedSite; cin.ignore();
		for (int i = 0; i < numSites; i++) {
			int index;

			int siteId;
			int goldRemain; // used in future leagues
			int maxMineSize; // used in future leagues
			int structureType; // -1 = No structure, 0 = Goldmine, 1 = Tower, 2 = Barracks
			int owner; // -1 = No structure, 0 = Friendly, 1 = Enemy
			int param1;
			int param2;
			cin >> siteId >> goldRemain >> maxMineSize >> structureType >> owner >> param1 >> param2; cin.ignore();

			index = getSiteIndexById(siteId, allSites);
			allSites.at(index).goldRemain = goldRemain;
			allSites.at(index).maxMineSize = maxMineSize;
			allSites.at(index).structureType = structureType;
			allSites.at(index).owner = owner;
			allSites.at(index).param1 = param1;
			allSites.at(index).param2 = param2;

			if (goldRemain == 0)
			{
				emptyMinesId.push_back(siteId);
			}

			if (owner == 0 && structureType == 0)
				goldEarnByTurn += param1;

			if (owner == 1 && structureType == 0)
				goldEarnByTurnEnemy += param1;

			errorStrings.push_back("allSites.at(" + std::to_string(index) + "); siteId(" + std::to_string(allSites.at(index).siteId) + "); goldRemain(" + std::to_string(allSites.at(index).goldRemain) + ") (" + std::to_string(allSites.at(index).goldRemain) + ");");
			errorOutput(0);
		}

		errorStrings.push_back("DEGUG : goldEarByTurn = " + std::to_string(goldEarnByTurn) + " !");
		errorOutput(0);

		allUnits.clear();
		int numUnits;
		cin >> numUnits; cin.ignore();
		for (int i = 0; i < numUnits; i++) {
			int x;
			int y;
			int owner;
			int unitType; // -1 = QUEEN, 0 = KNIGHT, 1 = ARCHER
			int health;
			cin >> x >> y >> owner >> unitType >> health; cin.ignore();
			allUnits.push_back(Unit());
			allUnits.back().x = x;
			allUnits.back().y = y;
			allUnits.back().owner = owner;
			allUnits.back().unitType = unitType;
			allUnits.back().health = health;
		}

		// Write an action using cout. DON'T FORGET THE "<< endl"
		// To debug: cerr << "Debug messages..." << endl;        

		// Get our Queen.
		queen = getQueen(0);
		enemyQueen = getQueen(1);
		if (previousHealth == -1)
		{
			areWeInDanger = false;
			previousHealth = queen.health;
			homePosition.first = queen.x;
			homePosition.second = queen.y;
			isGoBackHome = false;
		}
		else if (previousHealth > queen.health)
		{
			areWeInDanger = true;
			previousHealth = queen.health;
		}
		else
			areWeInDanger = false;

		if (queen.x == homePosition.first && queen.y == homePosition.second || !areWeInDanger)
			isGoBackHome = false;

		errorStrings.push_back("initProperties() !");
		errorOutput(0);

		initProperties();

		errorStrings.push_back("performAction(); !");
		errorOutput(0);

		performAction();

		errorStrings.push_back("performTraining() !");
		errorOutput(0);

		performTraining();

		// First line: A valid queen action
		// Second line: A set of training instructions
		//cout << "WAIT" << endl;
		//cout << "TRAIN" << endl;
	}
}

#pragma region Tools
void clearProperties()
{
	errorStrings.push_back("clearProperties() -- BEGIN !");
	errorOutput(0);

	allyArcherySites.clear();
	allyKnightSites.clear();
	allyGiantSites.clear();
	allyTowerSites.clear();
	allyMineSites.clear();

	// Get our units.
	allyKnightUnits.clear();
	allyArcherUnits.clear();
	allyGiantUnits.clear();

	// Get enemy constructions.
	enemyArcherySites.clear();
	enemyKnightSites.clear();
	enemyGiantSites.clear();
	enemyTowerSites.clear();
	enemyMinesSites.clear();
	enemySites.clear();

	// Get enemy units.
	enemyKnightUnits.clear();
	enemyArcherUnits.clear();
	enemyGiantUnits.clear();
	enemyUnits.clear();

	// Get empty sites.
	emptySites.clear();

	errorStrings.push_back("clearProperties() -- END !");
	errorOutput(0);
}

void initProperties()
{
	errorStrings.push_back("initProperties() -- BEGIN !");
	errorOutput(0);

	clearProperties();

	//Get our constructions.
	allyArcherySites = getSitesByTypeAndOwner(2, 0, -1, 1);
	allyKnightSites = getSitesByTypeAndOwner(2, 0, -1, 0);
	allyGiantSites = getSitesByTypeAndOwner(2, 0, -1, 2);
	allyTowerSites = getSitesByTypeAndOwner(1, 0);
	allyMineSites = getSitesByTypeAndOwner(0, 0);

	// Get our units.
	allyKnightUnits = getUnitsByType(0, 0);
	allyArcherUnits = getUnitsByType(0, 1);
	allyGiantUnits = getUnitsByType(0, 2);

	// Get enemy constructions.
	enemyArcherySites = getSitesByTypeAndOwner(2, 1, -1, 1);
	enemyKnightSites = getSitesByTypeAndOwner(2, 1, -1, 0);
	enemyGiantSites = getSitesByTypeAndOwner(2, 1, -1, 2);
	enemyTowerSites = getSitesByTypeAndOwner(1, 1);
	enemyMinesSites = getSitesByTypeAndOwner(0, 1);

	enemySites.insert(enemySites.end(), enemyArcherySites.begin(), enemyArcherySites.end());
	enemySites.insert(enemySites.end(), enemyKnightSites.begin(), enemyKnightSites.end());
	enemySites.insert(enemySites.end(), enemyGiantSites.begin(), enemyGiantSites.end());
	enemySites.insert(enemySites.end(), enemyTowerSites.begin(), enemyTowerSites.end());
	enemySites.insert(enemySites.end(), enemyMinesSites.begin(), enemyMinesSites.end());

	// Get enemy units.
	enemyKnightUnits = getUnitsByType(1, 0);
	enemyArcherUnits = getUnitsByType(1, 1);
	enemyGiantUnits = getUnitsByType(1, 2);
	enemyUnits.insert(enemyUnits.end(), enemyKnightUnits.begin(), enemyKnightUnits.end());
	enemyUnits.insert(enemyUnits.end(), enemyArcherUnits.begin(), enemyArcherUnits.end());
	enemyUnits.insert(enemyUnits.end(), enemyGiantUnits.begin(), enemyGiantUnits.end());

	// Get empty sites.
	emptySites = getSiteToBuild();

	errorStrings.push_back("initProperties() -- END !");
	errorOutput(0);
}

Unit getQueen(int _owner)
{
	errorStrings.push_back("getQueen(int " + std::to_string(_owner) + ") -- BEGIN !");
	errorOutput(1);

	for (auto unit : allUnits)
	{
		if (unit.unitType == -1 && unit.owner == _owner)
		{
			errorStrings.push_back("getQueen() return " + std::to_string(unit.unitType) + " -- END !");
			errorOutput(1);

			return unit;
		}
	}

	errorStrings.push_back("ERROR : getQueen() return defaultUnit -- END !");
	errorOutput(5);

	return Unit();
}

std::vector<Unit> getUnitsByType(int _owner, int _type)
{
	errorStrings.push_back("getUnitsByType(int " + std::to_string(_owner) + ", int " + std::to_string(_type) + ") -- BEGIN !");
	errorOutput(1);

	std::vector<Unit> units;

	for (auto unit : allUnits)
	{
		if (unit.owner == _owner &&
			unit.unitType == _type)
		{
			units.push_back(unit);
		}
	}

	errorStrings.push_back("getUnitsByType() return " + std::to_string(units.size()) + " -- END !");
	errorOutput(1);

	return units;
}

std::vector<Unit> getUnitsByType(int _owner, int _type, std::vector<Unit> _units)
{
	errorStrings.push_back("getUnitsByType(int " + std::to_string(_owner) + ", int " + std::to_string(_type) + ", std::vector<Unit> " + std::to_string(_units.size()) + ") -- BEGIN !");
	errorOutput(1);

	std::vector<Unit> units;

	for (auto unit : _units)
	{
		if (unit.owner == _owner &&
			unit.unitType == _type)
		{
			units.push_back(unit);
		}
	}

	errorStrings.push_back("getUnitsByType() return " + std::to_string(units.size()) + " -- END !");
	errorOutput(1);
	return units;
}

int getSiteIndexById(int _siteId, std::vector<Site> _sites)
{
	for (int i = 0; i < _sites.size(); i++)
	{
		if (_sites.at(i).siteId == _siteId)
			return i;
	}

	// siteId non existant.
	errorStrings.push_back("ERROR : site inconnu !");
	errorStrings.push_back("DETAILS : ");
	errorStrings.push_back("   getSiteIndexById(int _siteId=" + std::to_string(_siteId) + ", std::vector<Site> _sites=" + std::to_string(_sites.size()) + ")");
	errorOutput(5);

	return -1;
}

std::vector<Site> getSitesByTypeAndOwner(int _structureType, int _owner, int _param1/* = -1*/, int _param2/* = -1*/)
{
	std::vector<Site> sites;
	for (auto site : allSites)
	{
		if (site.structureType == _structureType &&
			site.owner == _owner)
		{
			if (_structureType == 2) // Special treatment when barracks => look for unitType.
			{
				if (site.param2 == _param2)
					sites.push_back(site);
			}
			else
			{
				sites.push_back(site);
			}
		}
	}

	return sites;
}

std::vector<Site> getSiteToBuild()
{
	std::vector<Site> sites;
	for (auto site : allSites)
	{
		if (site.structureType == -1)
			sites.push_back(site);
	}

	if (sites.size() < 0)
	{
		errorStrings.push_back("No more empty site.");
		errorOutput(2);
	}

	return sites;
}

Unit getClosestUnit(std::vector<Unit> &_units, int _unitType)
{
	Unit selectedUnit = { -1,-1,-1,-1 };
	bool first = true;
	int selUnitDistX = 0;
	int selUnitDistY = 0;
	int curUnitDistX = 0;
	int curUnitDistY = 0;

	for (auto unit : _units)
	{
		if (first)
		{
			// first treatment, we select a Site.
			selectedUnit = unit;
			first = false;
		}
		else
		{
			// Avant ou apr�s sur x ?
			if (selectedUnit.x > queen.x)
			{
				// apr�s.
				selUnitDistX = (selectedUnit.x - queen.x);
			}
			else
			{
				selUnitDistX = (queen.x - selectedUnit.x);
			}

			// La m�me sur l'axe y.
			if (selectedUnit.y > queen.y)
			{
				selUnitDistY = (selectedUnit.y - queen.y);
			}
			else
			{
				selUnitDistY = (queen.y - selectedUnit.y);
			}

			// Current site x.
			if (unit.x > queen.x)
			{
				curUnitDistX = (unit.x - queen.x);
			}
			else
			{
				curUnitDistX = (queen.x - unit.x);
			}

			// same on y
			if (unit.y > queen.y)
			{
				curUnitDistY = (unit.y - queen.y);
			}
			else
			{
				curUnitDistY = (queen.y - unit.y);
			}

			if (hypot(selUnitDistX, selUnitDistY) >= hypot(curUnitDistX, curUnitDistY))
			{
				selectedUnit = unit;
			}
		}
	}

	return selectedUnit;
}

/*
_checkSafety : -1 = no check; 0 = ally tower check; 1 = not under enemyTowerCheck;
*/
Site getClosestSite(std::vector<Site> &_sites, int _checkSafety /*=-1*/)
{
	Site selectedSite = Site();
	bool first = true;
	int selSiteDistX = 0;
	int selSiteDistY = 0;
	int curSiteDistX = 0;
	int curSiteDistY = 0;

	selectedSite.siteId = -1;

	for (auto site : _sites)
	{
		if (first)
		{
			// first treatment, we select a Site.
			if (_checkSafety == -1)
			{
				selectedSite = site;
				first = false;
			}
			else if (_checkSafety == 0)
			{
				if (isSiteUnderTowerRange(site, allyTowerSites))
				{
					selectedSite = site;
					first = false;
				}
			}
			else if (_checkSafety == 1)
			{
				if (!isSiteUnderTowerRange(site, enemyTowerSites))
				{
					selectedSite = site;
					first = false;
				}
			}
		}
		else
		{
			// Avant ou apr�s sur x ?
			if (selectedSite.x > queen.x)
			{
				// apr�s.
				selSiteDistX = (selectedSite.x - queen.x);
			}
			else
			{
				selSiteDistX = (queen.x - selectedSite.x);
			}

			// La m�me sur l'axe y.
			if (selectedSite.y > queen.y)
			{
				selSiteDistY = (selectedSite.y - queen.y);
			}
			else
			{
				selSiteDistY = (queen.y - selectedSite.y);
			}

			// Current site x.
			if (site.x > queen.x)
			{
				curSiteDistX = (site.x - queen.x);
			}
			else
			{
				curSiteDistX = (queen.x - site.x);
			}

			// same on y
			if (site.y > queen.y)
			{
				curSiteDistY = (site.y - queen.y);
			}
			else
			{
				curSiteDistY = (queen.y - site.y);
			}

			if (hypot(selSiteDistX, selSiteDistY) >= hypot(curSiteDistX, curSiteDistY))
			{
				if (_checkSafety == -1)
				{
					selectedSite = site;
				}
				else if (_checkSafety == 0)
				{
					if (isSiteUnderTowerRange(site, allyTowerSites))
					{
						selectedSite = site;
					}
				}
				else if (_checkSafety == 1)
				{
					if (!isSiteUnderTowerRange(site, enemyTowerSites))
					{
						selectedSite = site;
					}
				}
			}
		}
	}

	if (selectedSite.siteId < 0)
	{
		errorStrings.push_back("ERROR ! selectedSite.siteId = " + std::to_string(selectedSite.siteId) + " !");
		errorOutput(5);
	}

	return selectedSite;
}

/*
Return false : if not under tower range.
Return true : if site is under tower range.
*/
bool isSiteUnderTowerRange(Site _site, std::vector<Site> _towerSites)
{
	double distSiteTower = 0.0;
	int distX = 0;
	int distY = 0;

	for (auto tower : _towerSites)
	{
		// Get distance between tower and site.
		if (_site.x > tower.x) distX = tower.x - _site.x;
		else distX = _site.x - tower.x;

		if (_site.y > tower.y) distY = tower.y - _site.y;
		else distY = _site.y - tower.y;

		distSiteTower = hypot(distX, distY);
		if (distSiteTower <= tower.param2)
			return true;
	}

	return false;
}

/*
Return false : if not under tower range.
Return true : if site is under tower range.
*/
bool isUnitUnderTowerRange(Unit _unit, std::vector<Site> _towerSites)
{
	double distUnitTower = 0.0;
	int distX = 0;
	int distY = 0;

	for (auto tower : _towerSites)
	{
		// Get distance between tower and site.
		if (_unit.x > tower.x) distX = tower.x - _unit.x;
		else distX = _unit.x - tower.x;

		if (_unit.y > tower.y) distY = tower.y - _unit.y;
		else distY = _unit.y - tower.y;

		distUnitTower = hypot(distX, distY);
		if (distUnitTower <= tower.param2)
			return true;
	}

	return false;
}

std::vector<Site> getEmptySites(int _type)
{
	errorStrings.push_back("getEmptySites(int " + std::to_string(_type) + ") emptySites.size() = " + std::to_string(emptySites.size()) + " -- BEGIN !");
	errorOutput(1);

	std::vector<Site> sites;
	if (_type == 0)
		return emptySites;

	// We want a place for a mine, so the place must have goldRemain.
	if (_type == 1)
	{
		for (auto mineSite : emptySites)
		{
			bool isEmptyMine = false;
			for (auto siteId : emptyMinesId)
			{
				if (mineSite.siteId == siteId)
					isEmptyMine = true;
			}
			errorStrings.push_back("getEmptySites(int " + std::to_string(_type) + ") mineSite.siteId = " + std::to_string(mineSite.siteId) + " mineSite.goldRemain = " + std::to_string(mineSite.goldRemain) + " !");
			errorOutput(1);

			if (!isEmptyMine && (mineSite.goldRemain > 0 || mineSite.goldRemain == -1))
			{
				sites.push_back(mineSite);
			}
		}
	}

	if (sites.size() == 0)
	{
		errorStrings.push_back("ERROR : getEmptySites() -- No more empty site --");
		errorOutput(5);
	}

	errorStrings.push_back("getEmptySites() -- END !");
	errorOutput(0);
	return sites;
}

void errorOutput(int _level /*=0*/)
{
	if (_level >= errorLvl)
	{
		for (auto str : errorStrings)
		{
			cerr << str << endl;
		}
	}
	errorStrings.clear();
}
#pragma endregion

#pragma region Brain

void performAction()
{
	errorStrings.push_back("areWeInDanger ? " + std::to_string(areWeInDanger) + " !");
	errorOutput(1);

	/** NOT A GOOD STRAT ! **/
	/*Site eks = getClosestSite(enemyKnightSites, 1);
	if (eks.siteId != -1 && queen.health > 70)
	{
		buildTower(eks.siteId);
		return;
	}*/

	/*Site ems = getClosestSite(enemyMinesSites, 1);
	if (ems.siteId != -1)
	{
		buildTower(ems.siteId);
		return;
	}*/

	if (!areWeInDanger &&
		goldEarnByTurn >= 3 &&
		allyTowerSites.size() >= 4 &&
		allyKnightSites.size() >= 1 &&
		queen.health > enemyQueen.health &&
		towerIdToUpgrade == -1)
	{
		buildTower(getTowerToUpgrade().siteId);
		return;
	}

	if (enemyMinesSites.size() > 0)
	{
		Site enMineSite = getClosestSite(enemyMinesSites, 1);
		if (getDistance(std::make_pair(queen.x, queen.y), std::make_pair(enMineSite.x, enMineSite.y)) <= 200)
		{
			buildTower(enMineSite.siteId);
			return;
		}
	}

	if (enemyKnightSites.size() > 0)
	{
		Site enKnightSite = getClosestSite(enemyKnightSites, 1);
		if (getDistance(std::make_pair(queen.x, queen.y), std::make_pair(enKnightSite.x, enKnightSite.y)) <= 200)
		{
			buildTower(enKnightSite.siteId);
			return;
		}
	}

	/*if (goldEarnByTurnEnemy >= 5)
	{
		Site enemyMine = getClosestSite(enemyMinesSites, 1);
		if (enemyMine.siteId != -1)
		{
			buildTower(enemyMine.siteId);
			return;
		}
	}*/

	if (isGoBackHome)
	{
		// Still going back home.
		moveToPoint(homePosition.first, homePosition.second);
		return;
	}

	// Are we in alert (under attack) ?
	if (areWeInDanger)
	{
		//TODO: Are we under tower ? If yes, first move to a safe place.
		if (isUnitUnderTowerRange(queen, enemyTowerSites))
		{
			errorStrings.push_back("DEBUG : Go back home -- moveToPoint(" + std::to_string(homePosition.first) + "," + std::to_string(homePosition.second) + "); --");
			errorOutput(1);
			// Just go back home for now, maybe it's warm and safe.
			isGoBackHome = true;
			moveToPoint(homePosition.first, homePosition.second);
			return;
		}

		Site towerToUpgrade = getTowerToUpgrade();
		if (towerToUpgrade.siteId != -1 && towerIdToUpgrade == -1)
		{
			buildTower(towerToUpgrade.siteId);
			return;
		}
		// IF YES : Perform a defensive action.
		// Do we have tower ?
		if (allyTowerSites.size() > 3 && towerIdToUpgrade == -1)
		{
			// We do have tower, do we have to go inside ?
			// We should go inside the closest one and see what next.
			Site closestTower = getClosestSite(allyTowerSites, 1);
			if (closestTower.siteId == -1)
			{
				closestTower = getClosestSite(allyTowerSites);
			}

			errorStrings.push_back("DEBUG : Safe mode on ! -- buildTower(" + std::to_string(closestTower.siteId) + "; --");
			errorOutput(1);

			buildTower(closestTower.siteId);
			return;
		}
		else
		{
			// We don't have tower, it's the shit. We must construct a tower to defend ourself.
			// Do we have empty sites to construct ? (we should first look for empty safe sites - which are not under enemy tower range -, then we should look everywhere, and finally, we should try to get an enemy place).
			std::vector<Site> empS = getEmptySites(0);
			if (empS.size() > 0)
			{
				// We have empty site available.
				// Are we touching an empty site ?
				if (getClosestSite(empS).siteId == touchedSite)
				{
					errorStrings.push_back("DEBUG : Safe mode on ! -- We touched the empty site. -- buildTower(" + std::to_string(touchedSite) + "; --");
					errorOutput(1);

					buildTower(touchedSite);
					return;
				}
				else
				{
					errorStrings.push_back("DEBUG : Safe mode on ! -- We're not touching the empty site. -- buildTower(" + std::to_string(getClosestSite(empS).siteId) + "; --");
					errorOutput(1);

					buildTower(getClosestSite(empS).siteId);
					return;
				}
			}
			else
			{
				// No more empty sites.
				// Suicide mission : Go to the closest enemy site and build Tower to defend ourself.
				Site closestEnemySite = getClosestSite(enemySites);
				buildTower(closestEnemySite.siteId);
				return;
			}
		}
	}
	else
	{
		if (enemyUnits.size() > 1)
		{
			// Enemy have units. Let's see where is the closest one and adapt our position from this one.
			Unit closestEnemyUnit = getClosestUnit(enemyUnits, 0);
			if (closestEnemyUnit.x != -1 && closestEnemyUnit.unitType != -1)
			{
				// We have an enemy that is not the queen.
				if (getDistance(std::make_pair(queen.x, queen.y), std::make_pair(closestEnemyUnit.x, closestEnemyUnit.y)) <= 300)
				{
					errorStrings.push_back("ALERT -- Enemy close of the queen ! God save the Queen !");
					errorStrings.push_back("Enemy Unit : type(" + std::to_string(closestEnemyUnit.unitType) + "); health(" + std::to_string(closestEnemyUnit.health) +
						"); owner(" + std::to_string(closestEnemyUnit.owner) + "); x(" + std::to_string(closestEnemyUnit.x) + "); y(" + std::to_string(closestEnemyUnit.y) + ")");
					errorStrings.push_back("Queen Unit : type(" + std::to_string(queen.unitType) + "); health(" + std::to_string(queen.health) +
						"); owner(" + std::to_string(queen.owner) + "); x(" + std::to_string(queen.x) + "); y(" + std::to_string(queen.y) + ")");
					errorStrings.push_back("distance : " + std::to_string(getDistance(std::make_pair(queen.x, queen.y), std::make_pair(closestEnemyUnit.x, closestEnemyUnit.y))) + "!");
					errorOutput(1);

					// An enemy is a little to closer, what we do ?
					// TODO: I think, we should run away, but I also think that we have to stay close of a tower...
					// Let's just move to the opposite first.
					int x, y;
					x = (queen.x - closestEnemyUnit.x) + queen.x;
					y = (queen.y - closestEnemyUnit.y) + queen.y;
					moveToPoint(x, y);
					return;
				}
			}
		}
		// IF NO : Perform an attack action.
		if (buildBarracks())
			return;
	}

	errorStrings.push_back("Unmanaged case - we wait or we go to the closest allyTower !");
	errorOutput(1);
	if (allyTowerSites.size() > 0)
		buildTower(getClosestSite(allyTowerSites, 1).siteId);
	else
		cout << "WAIT" << endl;
}

double getDistance(std::pair<int, int> _pos1, std::pair<int, int> _pos2)
{
	double distance;
	bool first = true;
	int distX = 0;
	int distY = 0;

	if (_pos1.first > _pos2.first)
		distX = _pos1.first - _pos2.first;
	else
		distX = _pos2.first - _pos1.first;

	if (_pos1.second > _pos2.second)
		distY = _pos1.second - _pos2.second;
	else
		distY = _pos2.second - _pos1.second;

	distance = hypot(distX, distY);

	errorStrings.push_back("getDistance((" + std::to_string(_pos1.first) + ", " + std::to_string(_pos1.second) + "), (" + std::to_string(_pos2.first) + ", " + std::to_string(_pos2.second) + "))");
	errorStrings.push_back("disX = " + std::to_string(distX) + "; distY = " + std::to_string(distY) + ";");
	errorStrings.push_back("distance = " + std::to_string(distance) + "!");
	errorOutput(1);

	return distance;
}

void performTraining()
{
	if (!areWeInDanger && goldEarnByTurn >= 4 && queen.health >= 10 && allyTowerSites.size() > 1 && gold < 160)
	{
		errorStrings.push_back("Safe mode on !");
		errorOutput(5);
		// We're safe and under defense. Just wait for money.
		train();
		return;
	}

	// Do we have enough gold for train ?
	if (gold >= 80)
	{
		if (gold >= 140 && allyGiantSites.size() > 0 && allyGiantUnits.size() < 1)
		{
			trainGiants();
			if (gold < 80)
				return;
		}
		// IF YES : Choose what to train.
		// Do we have Knights ? We want Knights, to attack the evil queen.
		//if (allyKnightUnits.size() > 0)
		//{
		//	// OK ! We have at least one Knight.
		//}
		//else 
		if (allyKnightSites.size() > 0)
		{
			// We don't have Knights, but we have barrack to train Knights.
			trainKnights();
			return;
		}
	}

	errorStrings.push_back("Unmanaged case or just no more gold - we train !");
	errorOutput(0);
	cout << "TRAIN" << endl;
}

void trainKnights()
{
	if (allyKnightSites.size() > 0)
	{
		std::vector<int> siteIds;
		// We have Knight barracks
		for (auto barrack : allyKnightSites)
		{
			if (barrack.param1 == 0 && gold >= 80)
			{
				siteIds.push_back(barrack.siteId);
				gold -= 80;
			}
		}
		train(siteIds);
	}
	else
	{
		train();
	}
}

void trainGiants()
{
	if (allyGiantSites.size() > 0)
	{
		std::vector<int> siteIds;
		// We have Knight barracks
		for (auto barrack : allyGiantSites)
		{
			if (barrack.param1 == 0 && gold >= 80)
			{
				siteIds.push_back(barrack.siteId);
				gold -= 140;
			}
		}
		train(siteIds);
	}
	else
	{
		train();
	}
}

/*
Return True : we construct, so action was perform.
Return False : we don't construct, action was not send.
*/
bool buildBarracks()
{
	if (towerIdToUpgrade != -1)
	{
		if (allSites.at(getSiteIndexById(towerIdToUpgrade, allSites)).param2 != towerIdHealth)
		{
			towerIdHealth = allSites.at(getSiteIndexById(towerIdToUpgrade, allSites)).param2;
			errorStrings.push_back("Upgrade tower : tower.siteId = " + std::to_string(allSites.at(getSiteIndexById(towerIdToUpgrade, allSites)).siteId));
			errorStrings.push_back("Upgrade tower : tower.param2 = " + std::to_string(allSites.at(getSiteIndexById(towerIdToUpgrade, allSites)).param2));
			errorOutput(5);

			buildTower(towerIdToUpgrade);
			return true;
		}
		else
		{
			towerIdToUpgrade = -1;
			towerIdHealth = -1;
		}
	}

	if (goldEarnByTurn >= 8)
	{
		if (allyKnightSites.size() <= 1)
		{
			buildBarrack(getClosestSite(emptySites, 1).siteId, "KNIGHT");
			return true;
		}
	}

	if (enemyTowerSites.size() > 1 && allyGiantSites.size() <= 0)
	{
		// Enemy start to have a lot of defenses. Maybe we have to try to get Giants.
		buildBarrack(getClosestSite(emptySites, 1).siteId, "GIANT");
		return true;
	}

	if (allyMineSites.size() <= 1 || (goldEarnByTurn < 5 && allyTowerSites.size() >= 4))
	{
		// Yes ! So great. I think we start to be in the good way to win.
		// Do we have a Mine to upgrade ?
		Site mineToUpgrade = getMineToUpgrade();
		if (mineToUpgrade.siteId != -1)
		{
			// We have a mine to upgrade !
			buildMine(mineToUpgrade.siteId);
			return true;
		}

		// We don't have Mines. Mines are really importants to get money to train units.
		std::vector<Site> emptys = getEmptySites(1);
		buildMine(getClosestSite(emptys, 1).siteId);
		return true;
	}
	else
	{
		// Yes ! So great. I think we start to be in the good way to win.
		// Do we have a Mine to upgrade ?
		Site mineToUpgrade = getMineToUpgrade();
		if (mineToUpgrade.siteId != -1)
		{
			// We have a mine to upgrade !
			buildMine(mineToUpgrade.siteId);
			return true;
		}

		// We already have a Mine.
		// Do we have Knight barracks to spend the money earn by our Mine ?
		if (allyKnightSites.size() > 0)
		{
			// Yes, we have ! That's really great !
			// Do we have Tower, to defend ourself ?
			if (allyTowerSites.size() > 2)
			{
				// Yes ! So great. I think we start to be in the good way to win.
				// Do we have a Mine to upgrade ?
				Site mineToUpgrade = getMineToUpgrade();
				if (mineToUpgrade.siteId != -1)
				{
					// We have a mine to upgrade !
					buildMine(mineToUpgrade.siteId);
					return true;
				}
				else
				{
					// No mine to upgrade. We can look if we have a safety place to build one.
					std::vector<Site> emptys = getEmptySites(0);
					Site safetyPlace = getClosestSite(emptys, 0);
					if (safetyPlace.siteId != -1)
					{
						if (goldEarnByTurn < 5)
						{
							emptys = getEmptySites(1);
							safetyPlace = getClosestSite(emptys, 0);
							if (safetyPlace.siteId == -1)
								return false;
							// We've got a safety place. Go build a mine on it.
							buildMine(safetyPlace.siteId);
							return true;
						}


						// Try with only one Barracks of Knights.
						/*if (goldEarByTurn >= 5 &&
						allyKnightSites.size() <= 1)
						{
						buildBarrack(safetyPlace.siteId, "KNIGHT");
						return true;
						}*/

						if (goldEarnByTurn >= 2 && allyTowerSites.size() < 4)
						{
							buildTower(safetyPlace.siteId);
							return true;
						}
						else
						{
							// TODO: We earn "a lot" of money (5) and have "a lot" of defenses (4 towers). We should upgrade tower and maybe wait for train "a lot" of knights (2 draft).
							Site tower = getTowerToUpgrade();
							if (tower.siteId != -1)
							{
								buildTower(tower.siteId);
								return true;
							}
							else
							{
								emptys = getEmptySites(1);
								safetyPlace = getClosestSite(emptys, 0);
								if (safetyPlace.siteId == -1)
									return false;
								// We've got a safety place. Go build a mine on it.
								buildMine(safetyPlace.siteId);
								return true;
							}
						}

						return false;
					}
				}
			}
			else
			{
				// No ! Let's build one !
				std::vector<Site> emptys = getEmptySites(0);
				buildTower(getClosestSite(emptys).siteId);
				return true;
			}
		}
		else
		{
			// No, we havn't ! That's not good, we should build one.
			std::vector<Site> emptys = getEmptySites(0);
			buildBarrack(getClosestSite(emptys, 1).siteId, "KNIGHT");
			return true;
		}
	}

	return false;
}

/*
Return -1 : No mine to upgrade.
Return site : first mine to upgrade.
*/
Site getMineToUpgrade()
{
	Site site;
	site.siteId = -1;

	for (auto mine : allyMineSites)
	{
		if (mine.maxMineSize > mine.param1)
			return mine;
	}

	return site;
}

/*
Return -1 : No mine to upgrade.
Return site : first tower to upgrade.
*/
Site getTowerToUpgrade()
{
	Site site;
	site.siteId = -1;

	for (auto tower : allyTowerSites)
	{
		if (tower.param2 <= 400 && tower.siteId != towerIdToUpgrade)
			return tower;
	}

	return site;
}
#pragma endregion

#pragma region ActionsStringConstruction
bool moveToPoint(int x, int y)
{
	if (x < 0)
		x = 0;
	if (x > 1920)
		x = 1920;
	if (y < 0)
		y = 0;
	if (y > 1000)
		y = 1000;


	// If we have a very close empty site on our way, build a tower can be great.
	Site closestEmptySite = getClosestSite(emptySites, 1);
	if (closestEmptySite.siteId != -1 && getDistance(std::make_pair(queen.x, queen.y), std::make_pair(closestEmptySite.x, closestEmptySite.y)) <= 100)
	{
		buildTower(closestEmptySite.siteId);
		return true;
	}

	if (touchedSite != -1)
	{
		// We run, maybe we're close to an empty site. And maybe we can build a Tower to defend ourself.
		Site site = allSites.at(getSiteIndexById(touchedSite, allSites));
		if (site.structureType == -1)
		{
			buildTower(touchedSite);
			return true;
		}
	}

	cout << "MOVE " + std::to_string(x) + " " + std::to_string(y) << endl;
}

void buildTower(int _siteId)
{
	if (_siteId == -1)
	{
		errorStrings.push_back("WOW ! BUILD on siteId -1 => just WAIT !");
		errorOutput(5);
		cout << "WAIT" << endl;
		return;
	}

	if (_siteId != towerIdToUpgrade)
	{
		// We want to construct a new tower.
		// Do we have Tower ?
		if (allyTowerSites.size() > 0)
		{
			// Yes ! we have.
			// Is there a tower that needed to be upgrade before construct another one ?
			Site tower = getTowerToUpgrade();
			if (tower.siteId != -1)
			{
				// Yes, a tower need to be upgrade. So why the f*** do u want build another one ?
				// Maybe you are in danger, and u don't give a shit to upgrade tower ?
				// Are you in danger ?
				if (!areWeInDanger)
					_siteId == tower.siteId;
			}
		}
	}

	towerIdToUpgrade = _siteId;
	cout << "BUILD " + std::to_string(_siteId) + " TOWER" << endl;
}

void buildBarrack(int _siteId, std::string _unitType)
{
	cout << "BUILD " + std::to_string(_siteId) + " BARRACKS-" + _unitType << endl;
}

void buildMine(int _siteId)
{
	cout << "BUILD " + std::to_string(_siteId) + " MINE" << endl;
}

void train()
{
	cout << "TRAIN" << endl;
}

void train(std::vector<int> _siteIds)
{
	string string_siteId = "";
	for (auto siteId : _siteIds)
	{
		string_siteId += " " + std::to_string(siteId);
	}

	cout << "TRAIN" + string_siteId << endl;
}

#pragma endregion
