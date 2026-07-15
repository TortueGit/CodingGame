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
	int goldRemain;	   // -1 = unknown
	int maxMineSize;   // -1 = unknown
	int structureType; // -1 = No structure, 0 = Goldmine, 1 = Tower, 2 = Barracks
	int owner;		   // -1 = No structure, 0 = Friendly, 1 = Enemy
	int param1;
	int param2;
};

struct Unit
{
	int x;
	int y;
	int owner;
	int unitType; // -1 = QUEEN, 0 = KNIGHT, 1 = ARCHER, 2 = GIANT
	int health;
};

#pragma region MainProperties
Unit queen;
std::pair<int, int> homePosition;
bool areWeInDanger;

int gold;
int touchedSite; // -1 if none
int goldEarnByTurn;
int goldEarnByTurnEnemy;

int towerIdToUpgrade = -1;
int towerIdHealth = -1;

// Score de menace : chaque knight ennemi contribue d'autant plus qu'il est proche de la reine.
const double DANGER_RADIUS = 600.0;	   // au-delà de ~6 tours de déplacement d'un knight, il ne compte plus.
const double DANGER_THRESHOLD = 1.0;   // ~1 knight au contact, ou plusieurs en approche.
const double FLEE_RADIUS = 300.0;	   // knight à moins de 3 tours de déplacement : on fuit au lieu de construire.
const double MINE_SAFETY_RADIUS = 400; // pas de mine si un knight ennemi est plus proche que ça : elle serait détruite aussitôt.

const double TOWER_CROWD_RADIUS = 400.0; // les knights dans ce rayon d'une tour comptent comme "autour" d'elle.
const int TOWER_CROWD_LIMIT = 3;		 // trop de knights autour de la tour de refuge : on change de tour.

std::vector<Site> allSites;
std::vector<Unit> allUnits;

std::vector<Site> allyKnightSites;
std::vector<Site> allyGiantSites;
std::vector<Site> allyTowerSites;
std::vector<Site> allyMineSites;

std::vector<Unit> allyGiantUnits;

std::vector<Site> enemyKnightSites;
std::vector<Site> enemyTowerSites;
std::vector<Site> enemyMinesSites;
std::vector<Site> enemySites;

std::vector<Unit> enemyKnightUnits;

std::vector<Site> emptySites;

bool debugLogs = true; // logs de décision sur cerr, visibles tour par tour dans le replay. false = soumission silencieuse.
int turnCount = 0;
#pragma endregion

#pragma region ToolsMethods
void initProperties();

Unit getQueen(int _owner);
std::vector<Unit> getUnitsByType(int _owner, int _type);

int getSiteIndexById(int _siteId, std::vector<Site> &_sites);
std::vector<Site> getSitesByTypeAndOwner(int _structureType, int _owner, int _param2 = -1);
std::vector<Site> getSiteToBuild();
Unit getClosestUnit(std::vector<Unit> &_units);
Site getClosestSite(std::vector<Site> &_sites, int _checkSafety = -1);
bool isUnderTowerRange(int _x, int _y, std::vector<Site> &_towerSites);
int knightsAroundPoint(int _x, int _y, double _radius);
double getDistance(std::pair<int, int> _pos1, std::pair<int, int> _pos2);

std::vector<Site> getEmptySites(int _type); // 0 - no filter; 1 - mine filter;

void logLine(std::string _msg);
#pragma endregion

#pragma region BrainMethods
bool isQueenInDanger();
std::pair<int, int> getFleePoint();
int getDefensiveTowerSpot(bool _allowBarracks);
void performAction();
void performTraining();
void trainKnights();
void trainGiants();
bool performBuild();
bool buildEconomy();
Site getMineToUpgrade();
Site getTowerToUpgrade();
#pragma endregion

#pragma region ActionsStringConstructionMethods
void moveToPoint(int x, int y, bool _buildOnTheWay = true);
void buildTower(int _siteId);
void buildBarrack(int _siteId, std::string _unitType);
void buildMine(int _siteId);

void train();
void train(std::vector<int> _siteIds);
#pragma endregion

int main()
{
	bool firstTurn = true;

	int numSites;
	cin >> numSites;
	cin.ignore();
	for (int i = 0; i < numSites; i++)
	{
		int siteId;
		int x;
		int y;
		int radius;
		cin >> siteId >> x >> y >> radius;
		cin.ignore();
		allSites.push_back(Site());
		allSites.back().siteId = siteId;
		allSites.back().x = x;
		allSites.back().y = y;
		allSites.back().radius = radius;
	}

	// game loop
	while (1)
	{
		goldEarnByTurn = 0;

		cin >> gold >> touchedSite;
		cin.ignore();
		for (int i = 0; i < numSites; i++)
		{
			int siteId;
			int goldRemain;
			int maxMineSize;
			int structureType;
			int owner;
			int param1;
			int param2;
			cin >> siteId >> goldRemain >> maxMineSize >> structureType >> owner >> param1 >> param2;
			cin.ignore();

			int index = getSiteIndexById(siteId, allSites);
			if (index == -1)
				continue;

			allSites.at(index).goldRemain = goldRemain;
			allSites.at(index).maxMineSize = maxMineSize;
			allSites.at(index).structureType = structureType;
			allSites.at(index).owner = owner;
			allSites.at(index).param1 = param1;
			allSites.at(index).param2 = param2;

			if (owner == 0 && structureType == 0)
				goldEarnByTurn += param1;
		}

		allUnits.clear();
		int numUnits;
		cin >> numUnits;
		cin.ignore();
		for (int i = 0; i < numUnits; i++)
		{
			int x;
			int y;
			int owner;
			int unitType;
			int health;
			cin >> x >> y >> owner >> unitType >> health;
			cin.ignore();
			allUnits.push_back(Unit());
			allUnits.back().x = x;
			allUnits.back().y = y;
			allUnits.back().owner = owner;
			allUnits.back().unitType = unitType;
			allUnits.back().health = health;
		}

		queen = getQueen(0);
		if (firstTurn)
		{
			homePosition.first = queen.x;
			homePosition.second = queen.y;
			firstTurn = false;
		}

		initProperties();

		// Le param1 des mines ennemies est masqué (-1) : on estime leur revenu à 3 or/mine.
		goldEarnByTurnEnemy = (int)enemyMinesSites.size() * 3;

		// Détection de danger anticipative : on évalue la menace avant de prendre des dégâts.
		areWeInDanger = isQueenInDanger();

		// Ligne d'état du tour, pour analyser les matchs a posteriori.
		turnCount++;
		int closestKnightDist = -1;
		if (enemyKnightUnits.size() > 0)
		{
			Unit closestKnight = getClosestUnit(enemyKnightUnits);
			closestKnightDist = (int)getDistance(std::make_pair(queen.x, queen.y), std::make_pair(closestKnight.x, closestKnight.y));
		}
		logLine("T" + std::to_string(turnCount) +
				" hp=" + std::to_string(queen.health) +
				" pos=" + std::to_string(queen.x) + "," + std::to_string(queen.y) +
				" gold=" + std::to_string(gold) +
				" inc=" + std::to_string(goldEarnByTurn) + "vs" + std::to_string(goldEarnByTurnEnemy) +
				" mines=" + std::to_string(allyMineSites.size()) +
				" tours=" + std::to_string(allyTowerSites.size()) +
				" casernes=" + std::to_string(allyKnightSites.size()) +
				" eKnights=" + std::to_string(enemyKnightUnits.size()) +
				" dKnight=" + std::to_string(closestKnightDist) +
				" danger=" + std::to_string(areWeInDanger));

		// First line: A valid queen action
		// Second line: A set of training instructions
		performAction();
		performTraining();
	}
}

#pragma region Tools
void initProperties()
{
	enemySites.clear();

	// Nos constructions.
	allyKnightSites = getSitesByTypeAndOwner(2, 0, 0);
	allyGiantSites = getSitesByTypeAndOwner(2, 0, 2);
	allyTowerSites = getSitesByTypeAndOwner(1, 0);
	allyMineSites = getSitesByTypeAndOwner(0, 0);

	// Nos unités.
	allyGiantUnits = getUnitsByType(0, 2);

	// Constructions ennemies.
	enemyKnightSites = getSitesByTypeAndOwner(2, 1, 0);
	enemyTowerSites = getSitesByTypeAndOwner(1, 1);
	enemyMinesSites = getSitesByTypeAndOwner(0, 1);

	for (auto site : allSites)
	{
		if (site.owner == 1)
			enemySites.push_back(site);
	}

	// Unités ennemies.
	enemyKnightUnits = getUnitsByType(1, 0);

	// Sites libres.
	emptySites = getSiteToBuild();
}

Unit getQueen(int _owner)
{
	for (auto unit : allUnits)
	{
		if (unit.unitType == -1 && unit.owner == _owner)
			return unit;
	}

	logLine("ERROR getQueen(" + std::to_string(_owner) + ") : reine introuvable");

	return Unit();
}

std::vector<Unit> getUnitsByType(int _owner, int _type)
{
	std::vector<Unit> units;

	for (auto unit : allUnits)
	{
		if (unit.owner == _owner &&
			unit.unitType == _type)
		{
			units.push_back(unit);
		}
	}

	return units;
}

int getSiteIndexById(int _siteId, std::vector<Site> &_sites)
{
	for (size_t i = 0; i < _sites.size(); i++)
	{
		if (_sites.at(i).siteId == _siteId)
			return (int)i;
	}

	logLine("ERROR getSiteIndexById(" + std::to_string(_siteId) + ") : site inconnu");

	return -1;
}

std::vector<Site> getSitesByTypeAndOwner(int _structureType, int _owner, int _param2 /* = -1*/)
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

	return sites;
}

Unit getClosestUnit(std::vector<Unit> &_units)
{
	Unit selectedUnit = {-1, -1, -1, -1, 0};
	double bestDist = -1.0;

	for (auto unit : _units)
	{
		double dist = getDistance(std::make_pair(queen.x, queen.y), std::make_pair(unit.x, unit.y));
		if (bestDist < 0 || dist < bestDist)
		{
			bestDist = dist;
			selectedUnit = unit;
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
	selectedSite.siteId = -1;
	double bestDist = -1.0;

	for (auto site : _sites)
	{
		if (_checkSafety == 0 && !isUnderTowerRange(site.x, site.y, allyTowerSites))
			continue;
		if (_checkSafety == 1 && isUnderTowerRange(site.x, site.y, enemyTowerSites))
			continue;

		double dist = getDistance(std::make_pair(queen.x, queen.y), std::make_pair(site.x, site.y));
		if (bestDist < 0 || dist < bestDist)
		{
			bestDist = dist;
			selectedSite = site;
		}
	}

	return selectedSite;
}

/*
Return true : (_x, _y) est dans le rayon d'action d'une des tours.
*/
bool isUnderTowerRange(int _x, int _y, std::vector<Site> &_towerSites)
{
	for (auto tower : _towerSites)
	{
		if (getDistance(std::make_pair(_x, _y), std::make_pair(tower.x, tower.y)) <= tower.param2)
			return true;
	}

	return false;
}

/*
Nombre de knights ennemis à moins de _radius du point (_x, _y).
*/
int knightsAroundPoint(int _x, int _y, double _radius)
{
	int count = 0;
	for (auto knight : enemyKnightUnits)
	{
		if (getDistance(std::make_pair(_x, _y), std::make_pair(knight.x, knight.y)) <= _radius)
			count++;
	}

	return count;
}

double getDistance(std::pair<int, int> _pos1, std::pair<int, int> _pos2)
{
	return hypot(_pos1.first - _pos2.first, _pos1.second - _pos2.second);
}

std::vector<Site> getEmptySites(int _type)
{
	if (_type == 0)
		return emptySites;

	// Emplacement pour une mine : il doit rester de l'or (goldRemain == -1 -> inconnu, on tente),
	// et aucun knight ennemi à proximité (il détruirait la mine aussitôt construite).
	std::vector<Site> sites;
	for (auto site : emptySites)
	{
		if (site.goldRemain == 0)
			continue;

		bool knightTooClose = false;
		for (auto knight : enemyKnightUnits)
		{
			if (getDistance(std::make_pair(site.x, site.y), std::make_pair(knight.x, knight.y)) <= MINE_SAFETY_RADIUS)
			{
				knightTooClose = true;
				break;
			}
		}

		if (!knightTooClose)
			sites.push_back(site);
	}

	return sites;
}

void logLine(std::string _msg)
{
	if (debugLogs)
		cerr << _msg << endl;
}
#pragma endregion

#pragma region Brain

/*
Détection de danger anticipative : au lieu d'attendre que la reine perde des PV,
on évalue la menace des knights ennemis en approche.
Chaque knight contribue entre 0 (à DANGER_RADIUS ou plus) et 1 (au contact).
La reine est aussi en danger si elle est sous le feu d'une tour ennemie.
*/
bool isQueenInDanger()
{
	if (isUnderTowerRange(queen.x, queen.y, enemyTowerSites))
		return true;

	double threat = 0.0;
	for (auto knight : enemyKnightUnits)
	{
		double dist = getDistance(std::make_pair(queen.x, queen.y), std::make_pair(knight.x, knight.y));
		if (dist < DANGER_RADIUS)
			threat += (DANGER_RADIUS - dist) / DANGER_RADIUS;
	}

	return threat >= DANGER_THRESHOLD;
}

/*
Point de fuite.
Avec des tours : on orbite autour de la tour de refuge en la gardant entre la reine
et la meute (recalculé chaque tour, donc la reine tourne autour pendant qu'elle est
protégée par les tirs de la tour) ; si trop de knights entourent la tour courante,
on déménage vers la tour la moins encerclée.
Sans tour : à l'opposé de la meute, ou vers la maison si elle nous en éloigne.
*/
std::pair<int, int> getFleePoint()
{
	// Centre de la meute de knights proches.
	double centerX = 0.0;
	double centerY = 0.0;
	int count = 0;

	for (auto knight : enemyKnightUnits)
	{
		if (getDistance(std::make_pair(queen.x, queen.y), std::make_pair(knight.x, knight.y)) <= 600)
		{
			centerX += knight.x;
			centerY += knight.y;
			count++;
		}
	}

	if (count == 0)
	{
		logLine("FLEE maison (pas de meute proche)");
		return homePosition;
	}

	centerX /= count;
	centerY /= count;

	// Tourner autour d'une tour alliée : elle fait obstacle et tire sur les poursuivants.
	if (allyTowerSites.size() > 0)
	{
		Site refugeTower = getClosestSite(allyTowerSites);
		int refugeCrowd = knightsAroundPoint(refugeTower.x, refugeTower.y, TOWER_CROWD_RADIUS);

		// Trop de knights autour de la tour courante : on déménage vers la moins encerclée.
		if (refugeCrowd >= TOWER_CROWD_LIMIT)
		{
			for (auto tower : allyTowerSites)
			{
				int crowd = knightsAroundPoint(tower.x, tower.y, TOWER_CROWD_RADIUS);
				if (crowd < refugeCrowd)
				{
					refugeTower = tower;
					refugeCrowd = crowd;
				}
			}
		}

		logLine("FLEE orbite tour=" + std::to_string(refugeTower.siteId) + " crowd=" + std::to_string(refugeCrowd));

		// Se placer derrière la tour par rapport à la meute.
		double dirX = refugeTower.x - centerX;
		double dirY = refugeTower.y - centerY;
		double norm = hypot(dirX, dirY);
		if (norm < 1.0)
		{
			// La meute est exactement sur la tour : on s'écarte côté reine.
			dirX = queen.x - refugeTower.x;
			dirY = queen.y - refugeTower.y;
			norm = hypot(dirX, dirY);
			if (norm < 1.0)
				return homePosition;
		}

		double orbit = refugeTower.radius + 60.0; // collée à la tour, côté opposé aux knights.
		return std::make_pair(refugeTower.x + (int)(dirX / norm * orbit),
							  refugeTower.y + (int)(dirY / norm * orbit));
	}

	// Pas de tour : la maison si elle nous éloigne de la meute...
	std::pair<int, int> packCenter = std::make_pair((int)centerX, (int)centerY);
	if (getDistance(homePosition, packCenter) > getDistance(std::make_pair(queen.x, queen.y), packCenter) + 100)
	{
		logLine("FLEE maison (opposee a la meute)");
		return homePosition;
	}

	// ...sinon plein gaz à l'opposé.
	double dirX = queen.x - centerX;
	double dirY = queen.y - centerY;
	double norm = hypot(dirX, dirY);
	if (norm < 1.0)
	{
		logLine("FLEE maison (meute sur nous)");
		return homePosition;
	}

	logLine("FLEE oppose-meute (sans tour)");
	return std::make_pair(queen.x + (int)(dirX / norm * 400.0), queen.y + (int)(dirY / norm * 400.0));
}

/*
Pendant la défense, un site tout proche de la reine peut être converti en tour :
- un site vide : une tour de plus qui tire, autant ne pas piétiner à côté pour rien;
- (_allowBarracks) notre caserne inactive alors que l'or manque pour entraîner (< 80) :
  autant la remplacer par une tour, elle sera reconstruite quand l'économie le permettra.
  À réserver au contact : en préventif on détruirait notre caserne pour rien.

Return l'id du site à fortifier, ou -1 si rien à portée immédiate.
*/
int getDefensiveTowerSpot(bool _allowBarracks)
{
	int bestSiteId = -1;
	double bestDist = -1.0;

	for (auto site : allSites)
	{
		bool convertible = false;

		if (site.structureType == -1)
			convertible = true;
		else if (_allowBarracks && site.structureType == 2 && site.owner == 0 && site.param1 == 0 && gold < 80)
			convertible = true;

		if (!convertible)
			continue;

		// Trop loin : on ne dévie pas de la trajectoire de défense.
		double dist = getDistance(std::make_pair(queen.x, queen.y), std::make_pair(site.x, site.y));
		if (dist > site.radius + 60.0)
			continue;

		if (bestDist < 0 || dist < bestDist)
		{
			bestDist = dist;
			bestSiteId = site.siteId;
		}
	}

	return bestSiteId;
}

void performAction()
{
	// Sous le feu d'une tour ennemie : on s'échappe sans s'arrêter en route.
	if (isUnderTowerRange(queen.x, queen.y, enemyTowerSites))
	{
		logLine("ACT fuite tour-ennemie");
		std::pair<int, int> fleePoint = getFleePoint();
		moveToPoint(fleePoint.first, fleePoint.second, false);
		return;
	}

	// Des knights au contact : on fuit, mais si un site convertible est à portée
	// immédiate on y dresse une tour au passage - elle tirera sur la meute.
	if (enemyKnightUnits.size() > 0)
	{
		Unit closestKnight = getClosestUnit(enemyKnightUnits);
		if (getDistance(std::make_pair(queen.x, queen.y), std::make_pair(closestKnight.x, closestKnight.y)) <= FLEE_RADIUS)
		{
			int towerSpot = getDefensiveTowerSpot(true);
			if (towerSpot != -1)
			{
				logLine("ACT fortif site=" + std::to_string(towerSpot) + " (knights au contact)");
				buildTower(towerSpot);
				return;
			}

			logLine("ACT fuite knights");
			std::pair<int, int> fleePoint = getFleePoint();
			moveToPoint(fleePoint.first, fleePoint.second, false);
			return;
		}
	}

	// Menace en approche mais pas encore au contact : on a le temps de se préparer.
	if (areWeInDanger)
	{
		// Économie morte : sans or on ne peut plus rien entraîner ni défendre, la reconstruire
		// passe avant tout (les emplacements proposés évitent déjà les knights ennemis).
		if (goldEarnByTurn < 2)
		{
			logLine("ACT eco-urgence (danger, inc=" + std::to_string(goldEarnByTurn) + ")");
			if (buildEconomy())
				return;
		}

		// Un site vide à portée immédiate : on densifie le cluster défensif
		// (pas de conversion de caserne en préventif, seulement au contact).
		int towerSpot = getDefensiveTowerSpot(false);
		if (towerSpot != -1)
		{
			logLine("ACT fortif site=" + std::to_string(towerSpot) + " (danger en approche)");
			buildTower(towerSpot);
			return;
		}

		// Se réfugier sous la tour alliée la plus proche et la renforcer.
		if (allyTowerSites.size() > 0)
		{
			Site closestTower = getClosestSite(allyTowerSites, 1);
			if (closestTower.siteId == -1)
				closestTower = getClosestSite(allyTowerSites);
			logLine("ACT refuge tour=" + std::to_string(closestTower.siteId));
			buildTower(closestTower.siteId);
			return;
		}

		// Aucune tour : en construire une au plus vite.
		std::vector<Site> empS = getEmptySites(0);
		if (empS.size() > 0)
		{
			logLine("ACT tour-urgence (danger sans tour)");
			buildTower(getClosestSite(empS).siteId);
			return;
		}

		// Plus aucun site libre : tenter de prendre un site ennemi.
		logLine("ACT tour-urgence sur site ennemi (plus de site libre)");
		buildTower(getClosestSite(enemySites).siteId);
		return;
	}

	// Harcèlement opportuniste : voler une mine ou une caserne ennemie toute proche.
	if (enemyMinesSites.size() > 0)
	{
		Site enMineSite = getClosestSite(enemyMinesSites, 1);
		if (enMineSite.siteId != -1 &&
			getDistance(std::make_pair(queen.x, queen.y), std::make_pair(enMineSite.x, enMineSite.y)) <= 200)
		{
			logLine("ACT harcelement mine-ennemie=" + std::to_string(enMineSite.siteId));
			buildTower(enMineSite.siteId);
			return;
		}
	}

	if (enemyKnightSites.size() > 0)
	{
		Site enKnightSite = getClosestSite(enemyKnightSites, 1);
		if (enKnightSite.siteId != -1 &&
			getDistance(std::make_pair(queen.x, queen.y), std::make_pair(enKnightSite.x, enKnightSite.y)) <= 200)
		{
			logLine("ACT harcelement caserne-ennemie=" + std::to_string(enKnightSite.siteId));
			buildTower(enKnightSite.siteId);
			return;
		}
	}

	// Construction selon la liste de priorités.
	if (performBuild())
		return;

	// Rien à faire : on se replie sur la tour alliée la plus proche.
	if (allyTowerSites.size() > 0)
	{
		logLine("ACT repli (rien a construire)");
		buildTower(getClosestSite(allyTowerSites, 1).siteId);
	}
	else
	{
		logLine("ACT WAIT (rien a faire)");
		cout << "WAIT" << endl;
	}
}

/*
Liste de priorités explicite, réévaluée chaque tour :
1. Économie : ne jamais se laisser distancer par le revenu de l'ennemi.
2. Finir l'upgrade de la tour en cours (jusqu'à ~400 de portée).
3. Une caserne de knights (une deuxième si l'économie le permet).
4. Défense : 3 tours.
5. Une caserne de giants si l'ennemi accumule les tours.
6. Upgrades : mines puis tours.
7. Une mine de plus s'il reste un site sûr.

Return true : une action a été envoyée.
Return false : rien à construire, aucune action envoyée.
*/
bool performBuild()
{
	// 1. Économie d'abord : c'est elle qui paie les vagues de knights.
	//    Cible de revenu : celui de l'ennemi + 1, borné entre 4 et 10 or/tour.
	int incomeTarget = goldEarnByTurnEnemy + 1;
	if (incomeTarget < 4)
		incomeTarget = 4;
	if (incomeTarget > 10)
		incomeTarget = 10;

	if (allyMineSites.size() < 2 || goldEarnByTurn < incomeTarget)
	{
		logLine("BUILD p1-eco (inc=" + std::to_string(goldEarnByTurn) + " cible=" + std::to_string(incomeTarget) + ")");
		if (buildEconomy())
			return true;
	}

	// 2. Finir l'upgrade de la tour en cours (on s'arrête à 400 de portée :
	//    au-delà, le temps de la reine rapporte plus dans les mines).
	if (towerIdToUpgrade != -1)
	{
		int index = getSiteIndexById(towerIdToUpgrade, allSites);
		if (index != -1 &&
			allSites.at(index).owner == 0 &&
			allSites.at(index).structureType == 1 &&
			allSites.at(index).param2 < 400 &&
			allSites.at(index).param2 != towerIdHealth)
		{
			towerIdHealth = allSites.at(index).param2;
			logLine("BUILD p2-finir-tour id=" + std::to_string(towerIdToUpgrade) + " portee=" + std::to_string(towerIdHealth));
			buildTower(towerIdToUpgrade);
			return true;
		}

		towerIdToUpgrade = -1;
		towerIdHealth = -1;
	}

	// 3. Une caserne de knights (une deuxième si l'économie le permet).
	if (allyKnightSites.size() < 1 || (goldEarnByTurn >= 8 && allyKnightSites.size() < 2))
	{
		Site spot = getClosestSite(emptySites, 1);
		if (spot.siteId != -1)
		{
			logLine("BUILD p3-caserne site=" + std::to_string(spot.siteId));
			buildBarrack(spot.siteId, "KNIGHT");
			return true;
		}
	}

	// 4. Défense : 3 tours.
	if (allyTowerSites.size() < 3)
	{
		Site spot = getClosestSite(emptySites, 1);
		if (spot.siteId != -1)
		{
			logLine("BUILD p4-tour site=" + std::to_string(spot.siteId) + " (tours=" + std::to_string(allyTowerSites.size()) + ")");
			buildTower(spot.siteId);
			return true;
		}
	}

	// 5. Une caserne de giants si l'ennemi accumule les tours - seulement quand on a
	//    déjà l'or d'entraîner un giant, sinon le trajet est du temps de reine gaspillé.
	if (enemyTowerSites.size() > 1 && allyGiantSites.size() == 0 && gold >= 140)
	{
		Site spot = getClosestSite(emptySites, 1);
		if (spot.siteId != -1)
		{
			logLine("BUILD p5-caserne-giant site=" + std::to_string(spot.siteId));
			buildBarrack(spot.siteId, "GIANT");
			return true;
		}
	}

	// 6. Upgrades : mines puis tours.
	Site mineToUpgrade = getMineToUpgrade();
	if (mineToUpgrade.siteId != -1)
	{
		logLine("BUILD p6-upgrade-mine id=" + std::to_string(mineToUpgrade.siteId));
		buildMine(mineToUpgrade.siteId);
		return true;
	}

	Site towerToUpgrade = getTowerToUpgrade();
	if (towerToUpgrade.siteId != -1)
	{
		logLine("BUILD p6-upgrade-tour id=" + std::to_string(towerToUpgrade.siteId) + " portee=" + std::to_string(towerToUpgrade.param2));
		buildTower(towerToUpgrade.siteId);
		return true;
	}

	// 7. Une mine de plus s'il reste un site sûr.
	logLine("BUILD p7-mine-extra");
	if (buildEconomy())
		return true;

	logLine("BUILD rien-a-construire");
	return false;
}

/*
Upgrade une mine existante, sinon en construit une nouvelle sur le site sûr
le plus proche (hors portée des tours ennemies, loin des knights ennemis).

Return true : une action a été envoyée.
Return false : aucune mine à upgrader ni site sûr disponible.
*/
bool buildEconomy()
{
	Site mineToUpgrade = getMineToUpgrade();
	if (mineToUpgrade.siteId != -1)
	{
		logLine("ECO upgrade-mine id=" + std::to_string(mineToUpgrade.siteId) +
				" (" + std::to_string(mineToUpgrade.param1) + "/" + std::to_string(mineToUpgrade.maxMineSize) + ")");
		buildMine(mineToUpgrade.siteId);
		return true;
	}

	std::vector<Site> mineSpots = getEmptySites(1);
	Site mineSpot = getClosestSite(mineSpots, 1);
	if (mineSpot.siteId != -1)
	{
		logLine("ECO nouvelle-mine id=" + std::to_string(mineSpot.siteId) +
				" (candidats=" + std::to_string(mineSpots.size()) + ")");
		buildMine(mineSpot.siteId);
		return true;
	}

	logLine("ECO aucun site sur pour miner");
	return false;
}

void performTraining()
{
	// On ne thésaurise (vague double à 160) que si l'économie est forte ET qu'on a
	// 2 casernes à lâcher d'un coup. Sinon, la pression continue paie plus que l'attente.
	if (!areWeInDanger && goldEarnByTurn >= 8 && allyKnightSites.size() >= 2 && gold >= 80 && gold < 160)
	{
		logLine("TRAIN attente-or (gold=" + std::to_string(gold) + "<160, eco forte)");
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

		if (allyKnightSites.size() > 0)
		{
			trainKnights();
			return;
		}
	}

	logLine("TRAIN rien (gold=" + std::to_string(gold) + ")");
	train();
}

void trainKnights()
{
	if (allyKnightSites.size() > 0)
	{
		std::vector<int> siteIds;
		for (auto barrack : allyKnightSites)
		{
			if (barrack.param1 == 0 && gold >= 80)
			{
				siteIds.push_back(barrack.siteId);
				gold -= 80;
			}
		}
		logLine("TRAIN knights vagues=" + std::to_string(siteIds.size()));
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
		for (auto barrack : allyGiantSites)
		{
			if (barrack.param1 == 0 && gold >= 140)
			{
				siteIds.push_back(barrack.siteId);
				gold -= 140;
			}
		}
		logLine("TRAIN giants vagues=" + std::to_string(siteIds.size()));
		train(siteIds);
	}
	else
	{
		train();
	}
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
Return -1 : No tower to upgrade.
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
void moveToPoint(int x, int y, bool _buildOnTheWay /*= true*/)
{
	if (x < 0)
		x = 0;
	if (x > 1920)
		x = 1920;
	if (y < 0)
		y = 0;
	if (y > 1000)
		y = 1000;

	// En fuite (_buildOnTheWay == false), on ne s'arrête jamais : construire = rester immobile un tour.
	if (_buildOnTheWay)
	{
		// If we have a very close empty site on our way, build a tower can be great.
		Site closestEmptySite = getClosestSite(emptySites, 1);
		if (closestEmptySite.siteId != -1 &&
			getDistance(std::make_pair(queen.x, queen.y), std::make_pair(closestEmptySite.x, closestEmptySite.y)) <= 100)
		{
			logLine("MOVE fortif-chemin site=" + std::to_string(closestEmptySite.siteId));
			buildTower(closestEmptySite.siteId);
			return;
		}

		if (touchedSite != -1)
		{
			// We run, maybe we're close to an empty site. And maybe we can build a Tower to defend ourself.
			int index = getSiteIndexById(touchedSite, allSites);
			if (index != -1 && allSites.at(index).structureType == -1)
			{
				logLine("MOVE fortif-contact site=" + std::to_string(touchedSite));
				buildTower(touchedSite);
				return;
			}
		}
	}

	cout << "MOVE " + std::to_string(x) + " " + std::to_string(y) << endl;
}

void buildTower(int _siteId)
{
	if (_siteId == -1)
	{
		logLine("ERROR buildTower(-1) -> WAIT");
		cout << "WAIT" << endl;
		return;
	}

	if (_siteId != towerIdToUpgrade)
	{
		towerIdToUpgrade = _siteId;
		towerIdHealth = -1;
	}

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
