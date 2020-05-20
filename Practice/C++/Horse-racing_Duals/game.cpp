#include <iostream>
#include <string>
#include <vector>
#include <algorithm>

using namespace std;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
int main()
{
	int D = -1;
	std::vector<int> Pis;
	int LastPi = -1;
    int N;
    cin >> N; cin.ignore();
    for (int i = 0; i < N; i++) {
        int Pi;
        cin >> Pi; cin.ignore();
		Pis.push_back(Pi);
    }

	std::sort(Pis.rbegin(), Pis.rend());

	for (auto pi : Pis)
	{
		if (D == -1)
		{
			if (LastPi == -1)
				LastPi = pi;
			else
				D = (LastPi >= pi) ? LastPi - pi : pi - LastPi;
		}
		else
		{
			if (D > ((LastPi >= pi) ? LastPi - pi : pi - LastPi))
				D = (LastPi >= pi) ? LastPi - pi : pi - LastPi;
		}
		LastPi = pi;
	}

    // Write an action using cout. DON'T FORGET THE "<< endl"
    // To debug: cerr << "Debug messages..." << endl;

    cout << D << endl;
}