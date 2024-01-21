#include <iostream>
#include <string>
#include <vector>
#include <algorithm>
#include <regex>

using namespace std;

/*
Rock (R)
Paper (P)
sCissors (C)
Lizard (L)
Spock (S)

Scissors cuts Paper
Paper covers Rock
Rock crushes Lizard
Lizard poisons Spock
Spock smashes Scissors
Scissors decapitates Lizard
Lizard eats Paper
Paper disproves Spock
Spock vaporizes Rock
Rock crushes Scissors
and in case of a tie, the player with the lowest number wins (it's scandalous but it's the rule).
*/
struct Player
{
	int numPlayer;
	string signPlayer;
	string playersFighted;
};

int fight(string sign1, string sign2);
std::string rtrim(const std::string& s);

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
int main()
{
	vector<Player> players;
    int N;
    int round = 0;
    cin >> N; cin.ignore();
    for (int i = 0; i < N; i++) {
        int NUMPLAYER;
        string SIGNPLAYER;
        cin >> NUMPLAYER >> SIGNPLAYER; cin.ignore();
        struct Player pl = {NUMPLAYER, SIGNPLAYER, ""};
        players.push_back(pl);
    }

    while (players.size() > 1)
    {
    	int result = fight(players[round].signPlayer, players[round+1].signPlayer);
    	if (result == 0)
		{
    		if (players[round].numPlayer > players[round+1].numPlayer)
    		{
    			players[round+1].playersFighted += to_string(players[round].numPlayer) + " ";
    			players.erase(players.begin()+round);
    		}
    		else
    		{
    			players[round].playersFighted += to_string(players[round+1].numPlayer) + " ";
    			players.erase(players.begin()+round+1);
    		}
		}
    	else if (result == 1)
    	{
    		players[round].playersFighted += to_string(players[round+1].numPlayer) + " ";
    		players.erase(players.begin()+round+1);
		}
    	else
    	{
    		players[round+1].playersFighted += to_string(players[round].numPlayer) + " ";
    		players.erase(players.begin()+round);
    	}

    	round++;
    	N--;
    	if (round >= N)
    	{
    		round = 0;
    	}
    }

    // Write an answer using cout. DON'T FORGET THE "<< endl"
    // To debug: cerr << "Debug messages..." << endl;

    cerr << "WHO IS THE WINNER?" << endl;
    cout << players[0].numPlayer << endl;
    cout << rtrim(players[0].playersFighted) << endl;
}

int fight(string sign1, string sign2)
{
	if (sign1 == sign2)
		return 0;

	if (sign1 == "R")
	{
		if (sign2 == "L" || sign2 == "C")
			return 1;
		else
			return 2;
	}

	if (sign1 == "P")
	{
		if (sign2 == "R" || sign2 == "S")
			return 1;
		else
			return 2;
	}

	if (sign1 == "C")
	{
		if (sign2 == "P" || sign2 == "L")
			return 1;
		else
			return 2;
	}

	if (sign1 == "L")
	{
		if (sign2 == "S" || sign2 == "P")
			return 1;
		else
			return 2;
	}

	if (sign1 == "S")
	{
		if (sign2 == "C" || sign2 == "R")
			return 1;
		else
			return 2;
	}
}

std::string rtrim(const std::string& s) {
	return std::regex_replace(s, std::regex("\\s+$"), std::string(""));
}
