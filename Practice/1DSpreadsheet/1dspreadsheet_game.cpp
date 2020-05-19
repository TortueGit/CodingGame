#include <iostream>
#include <string>
#include <vector>
#include <algorithm>

using namespace std;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

void CalculValue(vector<vector<string>> &_operAndArgs, int _index);
void ReadValue(vector<vector<string>> &_operAndArgs, int _index);
void AddValue(vector<vector<string>> &_operAndArgs, int _index);
void SubValue(vector<vector<string>> &_operAndArgs, int _index);
void MultValue(vector<vector<string>> &_operAndArgs, int _index);
int GetIndex(string cellValue);

int main()
{
    int N;
    cin >> N;
    cin.ignore();

    //cerr << "N = " << N << endl;

    // Create a vector to stack a vector of string which will represents the cell values.
    vector<vector<string>> operAndArgs;

    // Reading entries...
    for (int i = 0; i < N; i++)
    {
    	// Stack a new vector of string with 4 cells ([0] operation | [1] arg1 | [2] arg2 | [3] calculated value)
        operAndArgs.push_back(vector<string>(4));
        // Read the entries, and stack them in the vector of string.
        cin >> operAndArgs[i][0] >> operAndArgs[i][1] >> operAndArgs[i][2];
        cin.ignore();

        //cerr << operAndArgs[i][0] << operAndArgs[i][1] << operAndArgs[i][2] << endl;

        // Calculated value is initialized to "" and will be calculated later.
        operAndArgs[i][3] = "";
    }

    // Let's calculate the values.
    for (int i = 0; i < N; i++)
    {
    	/* If the value of the cell [3] is empty,
    	 * then we have to calculate the value
    	 * otherwise we already made it, so we keep moving.*/
        if (operAndArgs[i][3].empty())
            CalculValue(operAndArgs, i);
        else
            continue;
    }

    // Write the results.
    for (int i = 0; i < N; i++)
        cout << operAndArgs[i][3] << endl;
}

void CalculValue(vector<vector<string>> &_operAndArgs, int _index)
{
	// Reading cell operation to know what to do.
    if (_operAndArgs[_index][0].compare("VALUE") == 0)
    {
    	ReadValue(_operAndArgs, _index);
    }
    else if (_operAndArgs[_index][0].compare("ADD") == 0)
    {
    	AddValue(_operAndArgs, _index);
    }
    else if (_operAndArgs[_index][0].compare("SUB") == 0)
    {
    	SubValue(_operAndArgs, _index);
    }
    else if (_operAndArgs[_index][0].compare("MULT") == 0)
    {
    	MultValue(_operAndArgs, _index);
    }
    //cerr << "_index[" << _index << "]" << _operAndArgs[_index][0] << "_" << _operAndArgs[_index][1] << "_" << _operAndArgs[_index][2] << endl;
    //cerr << "_operAndArgs[" << _index << "][3] = " << _operAndArgs[_index][3] << endl;
}

void ReadValue(vector<vector<string>> &_operAndArgs, int _index)
{
	// Easy one, we have the direct value.
	// But maybe it is the value of an other cell so we have to check it.
	if (_operAndArgs[_index][1].rfind('$', 0) == 0)
	{
		// It is a referenced value, let's go find the real value.
		int index = GetIndex(_operAndArgs[_index][1]);
		// The value of the referenced cell have to be calculated, so we go for it.
		if (_operAndArgs[index][3].empty())
			CalculValue(_operAndArgs, index);

		// We have calculate the referenced cell so we can write the value in the current cell.
		_operAndArgs[_index][3] = _operAndArgs[index][3];
	}
	else
	{
		// No referenced cell, we write the value.
		_operAndArgs[_index][3] = _operAndArgs[_index][1];
	}
}

void AddValue(vector<vector<string>> &_operAndArgs, int _index)
{
	// Operation ADD, so we have to calculate arg1+arg2.
	int val1;
	int val2;
	if (_operAndArgs[_index][1].rfind('$', 0) == 0)
	{
		// arg1 is a referenced cell value, so go get this value.
		int index = GetIndex(_operAndArgs[_index][1]);
		if (_operAndArgs[index][3].empty())
			CalculValue(_operAndArgs, index);

		val1 = atoi(_operAndArgs[index][3].c_str());
	}
	else
	{
		val1 = atoi(_operAndArgs[_index][1].c_str());
	}

	if (_operAndArgs[_index][2].rfind('$', 0) == 0)
	{
		// arg2 is a referenced cell value, so go get this value.
		int index = GetIndex(_operAndArgs[_index][2]);
		if (_operAndArgs[index][3].empty())
			CalculValue(_operAndArgs, index);

		val2 = atoi(_operAndArgs[index][3].c_str());
	}
	else
	{
		val2 = atoi(_operAndArgs[_index][2].c_str());
	}
	_operAndArgs[_index][3] = to_string(val1+val2);
}

void SubValue(vector<vector<string>> &_operAndArgs, int _index)
{
	// Operation SUB, so we have to calculate arg1-arg2.
	int val1;
	int val2;
	if (_operAndArgs[_index][1].rfind('$', 0) == 0)
	{
		int index = GetIndex(_operAndArgs[_index][1]);
		if (_operAndArgs[index][3].empty())
			CalculValue(_operAndArgs, index);

		val1 = atoi(_operAndArgs[index][3].c_str());
	}
	else
	{
		val1 = atoi(_operAndArgs[_index][1].c_str());
	}

	if (_operAndArgs[_index][2].rfind('$', 0) == 0)
	{
		int index = GetIndex(_operAndArgs[_index][2]);
		if (_operAndArgs[index][3].empty())
			CalculValue(_operAndArgs, index);

		val2 = atoi(_operAndArgs[index][3].c_str());
	}
	else
	{
		val2 = atoi(_operAndArgs[_index][2].c_str());
	}
	_operAndArgs[_index][3] = to_string(val1-val2);
}

void MultValue(vector<vector<string>> &_operAndArgs, int _index)
{
	// Operation MULT, so we have to calculate arg1*arg2.
	int val1;
	int val2;
	if (_operAndArgs[_index][1].rfind('$', 0) == 0)
	{
		int index = GetIndex(_operAndArgs[_index][1]);
		if (_operAndArgs[index][3].empty())
			CalculValue(_operAndArgs, index);

		val1 = atoi(_operAndArgs[index][3].c_str());
	}
	else
	{
		val1 = atoi(_operAndArgs[_index][1].c_str());
	}

	if (_operAndArgs[_index][2].rfind('$', 0) == 0)
	{
		int index = GetIndex(_operAndArgs[_index][2]);
		if (_operAndArgs[index][3].empty())
			CalculValue(_operAndArgs, index);

		val2 = atoi(_operAndArgs[index][3].c_str());
	}
	else
	{
		val2 = atoi(_operAndArgs[_index][2].c_str());
	}
	_operAndArgs[_index][3] = to_string(val1*val2);
}

int GetIndex(string cellValue)
{
	return atoi(cellValue.substr(1).c_str());
}
