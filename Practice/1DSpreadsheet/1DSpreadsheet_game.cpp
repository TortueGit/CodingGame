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

int main()
{
    int N;
    cin >> N;
    cin.ignore();

    cerr << "N = " << N << endl;

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
    if (_operAndArgs[_index][0].compare("VALUE") == 0)
    {
        if (_operAndArgs[_index][1].rfind('$', 0) == 0)
        {
            int index = atoi(_operAndArgs[_index][1].substr(1).c_str());
            if (_operAndArgs[index][3].empty())
                CalculValue(_operAndArgs, index);

            _operAndArgs[_index][3] = atoi(_operAndArgs[index][3].c_str());
        }
        else
            _operAndArgs[_index][3] = _operAndArgs[_index][1];
    }
    else if (_operAndArgs[_index][0].compare("ADD") == 0)
    {
        int val1;
        int val2;
        if (_operAndArgs[_index][1].rfind('$', 0) == 0)
        {
            int index = atoi(_operAndArgs[_index][1].substr(1).c_str());
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
            int index = atoi(_operAndArgs[_index][2].substr(1).c_str());
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
    if (_operAndArgs[_index][0].compare("SUB") == 0)
    {
        int val1;
        int val2;
        if (_operAndArgs[_index][1].rfind('$', 0) == 0)
        {
            int index = atoi(_operAndArgs[_index][1].substr(1).c_str());
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
            int index = atoi(_operAndArgs[_index][2].substr(1).c_str());
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
    if (_operAndArgs[_index][0].compare("MULT") == 0)
    {
        int val1;
        int val2;
        if (_operAndArgs[_index][1].rfind('$', 0) == 0)
        {
            int index = atoi(_operAndArgs[_index][1].substr(1).c_str());
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
            int index = atoi(_operAndArgs[_index][2].substr(1).c_str());
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
    cerr << "_index[" << _index << "]" << _operAndArgs[_index][0] << "_" << _operAndArgs[_index][1] << "_" << _operAndArgs[_index][2] << endl;
    cerr << "_operAndArgs[" << _index << "][3] = " << _operAndArgs[_index][3] << endl;
}
