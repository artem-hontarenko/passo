using System.Collections.Generic;
using static System.Console;

class MyAgent : Player {
    // Check if a pawn at (x, y) is under attack
    bool underAttack(Game game, int x, int y) {
        if (game.squares[x, y] == 1) { // red disk
            return ((x - 1 >= 0 && y - 1 >= 0 && game.squares[x - 1, y - 1] == 2) ||
                    (y - 1 >= 0 && game.squares[x, y - 1] == 2) ||
                    (x + 1 <= 4 && y - 1 >= 0 && game.squares[x + 1, y - 1] == 2));
        }

        if (game.squares[x, y] == 2) { // black disk
            return ((x - 1 >= 0 && y + 1 <= 4 && game.squares[x - 1, y + 1] == 1) ||
                    (y + 1 <= 4 && game.squares[x, y + 1] == 1) ||
                    (x + 1 <= 4 && y + 1 <= 4 && game.squares[x + 1, y + 1] == 1));
        }
        return false;
    }

    // Return true if the current player can win on their next move
    bool nextMoveWins(Game game) {
        for (int x = 0; x < 5; x++) {
            if (game.turn == 1 && game.squares[x, game.winLine().Item1] == 1) {
                return true;
            } else if (game.turn == 2 && game.squares[x, game.winLine().Item2] == 2) {
                return true;
            }
        }
        return false;
    }

    // Evaluate the game position and return a value between +1000 and -1000
    int eval(Game game) {
        if (nextMoveWins(game))
            return game.turn == 1 ? 1000 : -1000;

        for (int x = 0; x < 5; ++x) {
            if (game.turn == 1 && game.squares[x, game.winLine().Item2] == 2 && !underAttack(game, x, 5)) {
                return -1000;
            } else if (game.turn == 2 && game.squares[x, game.winLine().Item1] == 1 && !underAttack(game, x, 1)) {
                return 1000;
            }
        }

        int v = 0;
        for (int x = 0; x < 5; ++x) {
            for (int y = 0; y < 5; ++y) {
                if (game.squares[x, y] == 1) {
                    v += 25 - (game.winLine().Item2 + y);
                } else if (game.squares[x, y] == 2) {
                    v -= 25 + (game.winLine().Item2 - y);
                }
            }
        }
        return v;
    }

    // Minimax algorithm with alpha-beta pruning
    int minimax(Game game, int depth, int alpha, int beta, out Move bestMove) {
        bestMove = null!;

        if (game.winner == 1)
            return 1000;   // player 1 has already won
        if (game.winner == 2)
            return -1000;  // player 2 has already won

        if (depth == 0)  
            return eval(game);  

        bool maximizing = game.turn == 1;
        int bestVal = maximizing ? int.MinValue : int.MaxValue;

        foreach (Move move in game.possibleMoves()) {
            Console.WriteLine($"Minimax evaluating move: {move.from} -> {move.to} at depth {depth}");
            
            bool capture = game.move(move);
            int w = minimax(game, depth - 1, alpha, beta, out Move _);
            game.unmove(move, capture);

            Console.WriteLine($"Undone move");

            if (maximizing ? w > bestVal : w < bestVal) {
                bestVal = w;
                bestMove = move;
                if (maximizing) {
                    if (bestVal >= beta) {
                        return bestVal;
                    }
                    alpha = alpha > bestVal ? alpha : bestVal;
                } else {
                    if (bestVal <= alpha) {
                        return bestVal;
                    }
                    beta = beta < bestVal ? beta : bestVal;
                }
            }
        }
        return bestVal;
    }

    // Choose the best move
    public Move chooseMove(Game game) {
        // Search to 2 plies.
        minimax(game, 3, -1000, +1000, out Move best);
        return best;
    }
}
