import java.util.*;
import java.util.function.BiPredicate;
import java.io.*;
import java.math.*;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Solution {
    static final boolean PRINT_ERROR = false;

    public static void main(String args[]) {
        Scanner in = new Scanner(System.in);
        int n = in.nextInt();
        int m = in.nextInt();
        Map<String, String> inputMap = new HashMap<>();
        PrintError("inputs:");
        for (int i = 0; i < n; i++) {
            String inputName = in.next();
            String inputSignal = in.next();
            inputMap.put(inputName, inputSignal);
            
            PrintError(inputName + " " + inputSignal);
        }

        record OutputRecord(String ouputName, String type, String inputName1, String inputName2) {};
        Map<Integer, OutputRecord> outputMap = new HashMap<>();
        PrintError("outputs:");
        for (int i = 0; i < m; i++) {
            String outputName = in.next();
            String type = in.next();
            String inputName1 = in.next();
            String inputName2 = in.next();
            outputMap.put(i, new OutputRecord(outputName, type, inputName1, inputName2));

            PrintError(outputName + " " + type + " " + inputName1 + " " + inputName2);
        }

        for (int i = 0; i < m; i++) {
            OutputRecord output = outputMap.get(i);
            String result = GetResult(output.type, inputMap.get(output.inputName1), inputMap.get(output.inputName2));
            PrintError("result: " + result);

            // Write an answer using System.out.println()
            // To debug: System.err.println("Debug messages...");

            System.out.println(String.format("%s %s", output.ouputName, result));
        }

        in.close();
    }

    static String GetResult(String type, String inputSignal1, String inputSignal2) {
        String result = "";

        for (int i = 0; i < inputSignal1.length(); i++) {
            char a = inputSignal1.charAt(i);
            char b = inputSignal2.charAt(i);
            result += PerformOperation(type, a, b) ? '-' : '_';
        }

        return result;
    }

    static boolean PerformOperation(String type, char a, char b) {
        PrintError(type + " " + a + " " + b);

        return GetOperation(type).test(a == '-', b == '-');
    }

    static BiPredicate<Boolean, Boolean> GetOperation(String type) {
        switch (type) {
            case "AND":
                return (a, b) -> a && b;
            case "OR":
                return (a, b) -> a || b;
            case "XOR":
                return (a, b) -> a ^ b;
            case "NAND":
                return (a, b) -> !(a && b);
            case "NOR":
                return (a, b) -> !(a || b);
            case "NXOR":
                return (a, b) -> !(a ^ b);
        
            default:
                throw new Error("Type operator error!");
        }
    }

    static void PrintError(String errorString) {
        if (PRINT_ERROR)
            System.err.println(errorString);
    }
}