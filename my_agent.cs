using System.Collections.Generic;
using static System.Console;

class MyAgent : Player {

    // Check if the position belongs to the board
    bool isValid(int newX, int newY){
        if(newX < 0 || newX > 4){
            return false;
        } else if(newY < 0 || newY > 4){
            return false;
        }
        return true;
    }
    // Check if a disk at (x, y) is under attack
    bool underAttack(Game game, int x, int y) {
        int[] rowDirect = { -1, -1, -1, 0, 0, 1, 1, 1 };
        int[] colDirect = { -1, 0, 1, -1, 1, -1, 0, 1 };

        if (game.squares[x, y] == 1) { // red disk
            for(int i = 0; i < rowDirect.Length; i++){
                int newX = x + rowDirect[i];
                int newY = y + colDirect[i];
                if(isValid(newX, newY) && game.squares[newX, newY] == 2){
                    return true;
                }
            }
        }

        if (game.squares[x, y] == 2) { // black disk
            for(int i = 0; i < rowDirect.Length; i++){
                int newX = x + rowDirect[i];
                int newY = y + colDirect[i];
                if(isValid(newX, newY) && game.squares[newX, newY] == 2){
                    return true;
                }
            }
        }
        return false;
    }

    // Return true if the current player can win on their next move
    bool nextMoveWins(Game game) {
        (int redWinLine, int blackWinLine) = game.winLine();
        for (int x = 0; x < Game.Size; x++) {
            if (game.turn == 1 && game.squares[x, redWinLine] == 1) {
                return true;
            } else if (game.turn == 2 && game.squares[x, blackWinLine] == 2) {
                return true;
            }
        }
        return false;
    }

    // Evaluate the game position and return a value between +1000 and -1000
    int eval(Game game) {
        if (nextMoveWins(game))
            return game.turn == 1 ? 1000 : -1000;

        int v = 0;
        for (int x = 0; x < Game.Size; ++x) {
            for (int y = 0; y < Game.Size; ++y) {
                if (game.squares[x, y] == 1) {
                    v += 5 * (Game.Size - y); // Red pieces closer to top have higher value
                    if (!underAttack(game, x, y)) v += 2; // Bonus for safe pieces
                } else if (game.squares[x, y] == 2) {
                    v -= 5 * y; // Black pieces closer to bottom have higher value
                    if (!underAttack(game, x, y)) v -= 2; // Bonus for safe pieces
                }
            }
        }
        return v;
    }

    // Minimax algorithm with alpha-beta pruning
    int minimax(Game game, int depth, int alpha, int beta, out Move bestMove) {
        bestMove = null!;

        if (game.winner == 1)
            return 1000;   // red player has already won
        if (game.winner == 2)
            return -1000;  // black player has already won

        if (depth == 0)  
            return eval(game);  

        bool maximizing = game.turn == 1;
        int bestVal = maximizing ? int.MinValue : int.MaxValue;

        foreach (Move move in game.possibleMoves()) {
            bool capture = game.move(move);
            int w = minimax(game, depth - 1, alpha, beta, out Move _);
            game.unmove(move, capture);

            if (maximizing ? w > bestVal : w < bestVal) {
                bestVal = w;
                bestMove = move;
                if (maximizing) {
                    alpha = Math.Max(alpha, bestVal);
                    if (bestVal >= beta) {
                        break;
                    }
                } else {
                    beta = Math.Min(beta, bestVal);
                    if (bestVal <= alpha) {
                        break;
                    }
                }
            }
        }
        return bestVal;
    }

    // Choose the best move with dynamic depth adjustment
    public Move chooseMove(Game game) {
        int pieceCount = game.pieces[1] + game.pieces[2];
        int depth = pieceCount > 7 ? 2 : 3; // Adjust depth dynamically
        minimax(game, depth, -1000, 1000, out Move best);
        return best;
    }
}
