import java.util.*;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Solution {

    static final boolean PRINT_ERROR = false;

    static final String NEW_LINE = "nl";
    static final Map<String, Character> ABREVIATION_MAP = Map.of("sp", ' ', "bS", '\\', "sQ", '\'');

    public static void main(String args[]) {
        Scanner in = new Scanner(System.in);
        String T = in.nextLine();

        String[] recipeStep = T.split(" ");

        String result = "";
        for (String step : recipeStep) {            
            if (step.equals(NEW_LINE)) {
                System.out.println(result);
                result = "";
                continue;
            }
            
            Pair<Integer, Character> oc = GetOccurenceAndChar(step);
            int o = oc.first;
            String c = oc.second.toString();

            result += c.repeat(o);
        }

        // Write an answer using System.out.println()
        // To debug: System.err.println("Debug messages...");
        System.out.println(result);
    }

    static Pair<Integer, Character> GetOccurenceAndChar(String step) {
        char toPrint = ' ';
        int occurences = 0;
        if (IsNumeric(step)) {
            toPrint = step.charAt(step.length() - 1);
            step = step.substring(0, step.length() - 1);
            occurences = Integer.parseInt(step);
        } else {
            String[] occursToPrint = step.split("(?<=\\D)(?=\\d)|(?<=\\d)(?=\\D)");
            for (String string : occursToPrint) {
                PrintError(string);
            }
            occurences = Integer.parseInt(occursToPrint[0]);

            if (occursToPrint.length > 1) {
                if (occursToPrint[1].length() == 1)
                {
                    toPrint = occursToPrint[1].charAt(0);
                } else {
                    if (occursToPrint[1] == NEW_LINE) {
                        toPrint = '\n';
                    } else {
                        toPrint = ABREVIATION_MAP.get(occursToPrint[1]);
                    }
                }
            }
        }

        return new Pair<Integer, Character>(occurences, toPrint);
    }

    static class Pair<T1, T2> {
        T1 first;
        T2 second;

        Pair(T1 first, T2 second) { this.first = first; this.second = second; }
    }

    static boolean IsNumeric(String string) {
        try {
            Integer.parseInt(string);
        } catch (NumberFormatException ex) {
            return false;
        }

        return true;
    }

    static void PrintError(String errorString) {
        if (PRINT_ERROR)
            System.err.println(errorString);
    }    
}