import java.util.*;
import java.io.*;
import java.math.*;
import java.text.MessageFormat;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Solution {
	static final int STEP_COUNT_MAX = 64;
    static final int CHESSBOARD_WIDTH = 17;
    static final int CHESSBOARD_HEIGHT = 9;
	static final int START_POS_X = 8;
	static final int START_POS_Y = 4;
    static final char START_VALUE = 'S';
    static final char END_VALUE = 'E';
    static final String CODINGAME_STRING = "[CODINGAME]";
    static final String TOP_STRING = MessageFormat.format("+---{0}---+", CODINGAME_STRING);
	static final String BOTTOM_STRING = "+" + "-".repeat(CHESSBOARD_WIDTH) + "+";
    static final String BORDER_TOP_BOTTOM = "-";
    static final String BORDER_LEFT = "|";
    static final String BORDER_RIGHT = "|\n";
    static final Map<Integer, Character> VALUE_CHAR_MAP = Map.ofEntries(
            Map.entry(0, ' '),
            Map.entry(1, '.'),
            Map.entry(2, 'o'),
            Map.entry(3, '+'),
			Map.entry(4, '='),
            Map.entry(5, '*'),
            Map.entry(6, 'B'),
            Map.entry(7, 'O'),
            Map.entry(8, 'X'),
            Map.entry(9, '@'),
            Map.entry(10, '%'),
            Map.entry(11, '&'),
            Map.entry(12, '#'),
            Map.entry(13, '/'),
            Map.entry(14, '^'),
            Map.entry(15, END_VALUE)
        );

    public static void main(String args[]) {
        int[][] board = new int[CHESSBOARD_HEIGHT][CHESSBOARD_WIDTH];

        Scanner in = new Scanner(System.in);
        String fingerprint = in.nextLine();

        if (!fingerprint.equals(" ")) { // for Test purpose!

            String[] steps = fingerprint.split(":");

            int posX = START_POS_X;
            int posY = START_POS_Y;
            int stepCount = 0;
            for (String step : steps) {
                String binString = hexToBin(step);
                for (int i = binString.length() - 1; i >= 0; i--) {
                    boolean moveRight = binString.charAt(i) == '1';
                    if (canMoveOnX(posX, moveRight)) {
                        posX += moveRight ? +1 : -1;
                    }
                    i--;
                    boolean moveTop = binString.charAt(i) == '0';
                    if (canMoveOnY(posY, moveTop)) {
                        posY += moveTop ? -1 : +1;
                    }

                    if (board[posY][posX] == 14) {
                        board[posY][posX] = 0;
                    } else {
                        board[posY][posX]++;
                    }
                }

                stepCount += 4;
                if (stepCount == STEP_COUNT_MAX) {
                    board[posY][posX] = 15;
                    break;
                }
            }
            
        }

        // Write an answer using System.out.println()
        // To debug: System.err.println("Debug messages...");

		System.out.println(TOP_STRING);
		for (int h = 0; h < CHESSBOARD_HEIGHT; h++) {
			System.out.print(BORDER_LEFT);
			for (int w = 0; w < CHESSBOARD_WIDTH; w++) {
				if (w == START_POS_X && h == START_POS_Y){
					System.out.print(START_VALUE);
				} else {
					System.out.print(VALUE_CHAR_MAP.get(board[h][w]));
				}
			}

			System.out.print(BORDER_RIGHT);
		}
		System.out.println(BOTTOM_STRING);

		in.close();
    }

	static String hexToBin(String s) {
        String result = new BigInteger(s, 16).toString(2);
        if (result.length() < 8) result = "0".repeat(8 - result.length()) + result;
		return result;
	}

    static boolean canMoveOnX(int posX, boolean moveRight) {
        if (posX == CHESSBOARD_WIDTH-1 && moveRight) {
            return false;
        } else if (posX == 0 && !moveRight) {
            return false;
        }

        return true;
    }

    static boolean canMoveOnY(int posY, boolean moveTop) {
        // Top is 0 Bot is 9!
        if (posY == CHESSBOARD_HEIGHT-1 && !moveTop) {
            return false;
        } else if (posY == 0 && moveTop) {
            return false;
        }

        return true;
        
    }
}