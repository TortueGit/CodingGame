#include <iostream>
#include <string>
#include <vector>
#include <algorithm>

using namespace std;

struct Cell {
	static vector<Cell> cells;
	string op, arg[2];
	int valArg[2];

	void resolveRefs() {
		if (arg[0][0] == '$') {
			string refCell = arg[0].substr(1);
			valArg[0] = cells[stoi(refCell)].evaluate();
		}
		else {
			valArg[0] = stoi(arg[0]);
		}
		if (arg[1][0] == '$') {
			string refCell = arg[1].substr(1);
			valArg[1] = cells[stoi(refCell)].evaluate();
		}
		else {
			if(arg[1][0] != '_')
				valArg[1] = stoi(arg[1]);
		}
	}

	int evaluate() {
		resolveRefs();
		int result;
		if (op == "VALUE") {
			result = valArg[0];
		}
		else if (op == "ADD") {
			result = valArg[0] + valArg[1];
		}
		else if (op == "SUB") {
			result = valArg[0] - valArg[1];
		}
		else if (op == "MULT") {
			result = valArg[0] * valArg[1];
		}

		if (op != "VALUE") {
			arg[0] = to_string(valArg[0]);
			arg[1] = to_string(valArg[1]);
		}

		return result;
	}
};

vector<Cell> Cell::cells;

int main()
{
	int N;
	cin >> N; cin.ignore();

	for (int i = 0; i < N; i++) {
		Cell next;
		cin >> next.op >> next.arg[0] >> next.arg[1]; cin.ignore();
		Cell::cells.push_back(next);
	}

	for (int i = 0; i < N; i++) {
		cout << Cell::cells[i].evaluate() << endl;
	}
}
