#include <cstdint>
#include <iostream>
#include <functional>
#include <optional>

using FutureInt = std::function<int32_t()>;

std::array<FutureInt, 101> spreadsheet;

FutureInt read_arg(std::string s) {
    if (s[0] == '$')
        return [n = std::stoi(s.substr(1))](){ return spreadsheet[n](); };
    else if (s[0] == '_')
        return nullptr;
    else
        return [v = std::stoi(s)](){ return v; };

}

FutureInt memoize(FutureInt f)
{
    return [res = std::optional<int32_t>{}, f = std::move(f)] () mutable {
        if (res)
            return *res;
        int32_t r = f();
        res = {r};
        return r;
    };
}

int main() {
    size_t n; std::cin >> n;
    
    for (int i = 0; i < n; i++) {
        std::string op, arg1, arg2; std::cin >> op >> arg1 >> arg2;
        FutureInt a1 = read_arg(arg1);
        FutureInt a2 = read_arg(arg2);
        if (op == "VALUE")
               spreadsheet[i] = memoize([=](){return a1();});
        if (op == "ADD")
               spreadsheet[i] = memoize([=](){return a1() + a2();});
        if (op == "SUB")
               spreadsheet[i] = memoize([=](){return a1() - a2();});
        if (op == "MULT")
               spreadsheet[i] = memoize([=](){return a1() * a2();});
    }
    
    for (int i = 0; i < n; i++) {
        std::cout << spreadsheet[i]() << std::endl;
    }
}