using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cairo;
using static System.Console;
using static System.Math;

class Pos {
    public int x, y;
    
    public Pos(int x, int y) { this.x = x; this.y = y; }
    
}

class Move {
    public Pos from, to;
    
    public Move(Pos from, Pos to) { this.from = from; this.to = to; }
    
}

class IsolatedIslandsFinder {
    public static List<List<(int, int)>> findIsolatedIslands(int[,] matrix, int[,] active) {
        int size = matrix.GetLength(0);
        List<List<(int, int)>> islands = FindAllIslands(active);
        List<List<(int, int)>> islandsNew = new List<List<(int, int)>>();

        foreach (var island in islands) {
            bool valid = true;
            int type = matrix[island[0].Item1, island[0].Item2];

            foreach (var cell in island) {
                if (matrix[cell.Item1, cell.Item2] != type) {
                    valid = false;
                    break;
                }
            }
            if (valid) {
                islandsNew.Add(island);
            }
        }
        return islandsNew;

    }

    static int Size = 5;

    // A function to check if a given cell (row, col) can be included in DFS
    static bool isValid(int[,] matrix, int row, int col, bool[,] visited) {
        return (row >= 0) && (row < Size) && (col >= 0)
            && (col < Size) && matrix[row, col] == 0 && !visited[row, col];
    }

    // A utility function to do DFS for a 2D boolean matrix
    static void DFS(int[,] matrix, int row, int col, bool[,] visited, List<(int, int)> island) {
        // Arrays to get row and column numbers of 8 neighbors of a given cell
        int[] rowDirect = { -1, -1, -1, 0, 0, 1, 1, 1 };
        int[] colDirect = { -1, 0, 1, -1, 1, -1, 0, 1 };

        visited[row, col] = true;
        island.Add((row, col));

        // Recur for all connected neighbours
        for (int k = 0; k < 8; ++k) {
            if (isValid(matrix, row + rowDirect[k], col + colDirect[k], visited)) {
                DFS(matrix, row + rowDirect[k], col + colDirect[k], visited, island);
            }
        }
    }

    // Function to find all islands in the given boolean 2D matrix
    public static List<List<(int, int)>> FindAllIslands(int[,] M) {
        bool[,] visited = new bool[Size, Size];
        List<List<(int, int)>> islands = new List<List<(int, int)>>();

        for (int i = 0; i < Size; ++i) {
            for (int j = 0; j < Size; ++j) {
                if (M[i, j] == 0 && !visited[i, j]) {
                    // Found a new island, start a DFS to explore it
                    List<(int, int)> island = new List<(int, int)>();
                    DFS(M, i, j, visited, island);
                    islands.Add(island);
                }
            }
        }

        return islands;
    }
}

// A Passo game.
class Game {
    public const int Size = 5;
    
    // squares[x, y] holds the piece at square (x, y):
    // 0 = empty, 1 = player 1 (red), 2 = player 2 (black)
    public int[,] squares = new int[Size, Size];

    // squaresState[x, y] holds the state of a square (x, y),
    // 0 = active, 1 = inactive
    public int[,] squaresState = new int[Size, Size];

    // Dictionary to store the island cells
    public Dictionary<(int, int), int> diction =  new Dictionary<(int, int), int>();
    
    // pieces[p] is the number of pieces currently owned by player p.
    // (The dummy entry pieces[0] is unused.)
    public int[] pieces = new int[3];
    
    // The player whose turn it is to play.
    public int turn = 1;
    
    // The number of moves that have been made in the game so far.
    public int moves = 0;
    
    // The player who has won, or 0 if nobody has won yet.
    public int winner = 0;
    
    public Game() {

        for (int x = 0 ; x < Size ; ++x) {
            squares[x, 0] = 2;
            squares[x, Size - 1] = 1;
        }
        pieces[1] = pieces[2] = Size;

        for(int x = 0; x < Size; ++x){
            for(int y = 0; y < Size; ++y){
                squaresState[x, y] = 0;
            }
        }
    }
    
    // Create an independent copy of the game state.
    public Game clone() {
        Game g = (Game) MemberwiseClone();
        g.squares = (int[,]) squares.Clone();
        g.pieces = (int[]) pieces.Clone();
        g.squaresState = (int[,]) squaresState.Clone();
        return g;
    }
        
    bool valid(Pos pos) => pos.x >= 0 && pos.x < Game.Size &&
                           pos.y >= 0 && pos.y < Game.Size;
    
    // Return true if the given move is valid.
    public bool validMove(Move move) {
        Pos from = move.from, to = move.to;

        // Check if the target position is valid
        if (!valid(from) || !valid(to)) {
            return false;
        }

        // Check if the moving piece belongs to the current player
        if (squares[from.x, from.y] != turn) {
            return false;
        }

        // Check if the move is to one of the adjacent cells ahead
        if (Abs(to.x - from.x) > 1 || Abs(to.y - from.y) > 1) {
            return false;
        }

        // Check if the target position is not occupied by the current player's piece
        if (squares[to.x, to.y] == turn) {
            return false;
        }

        // Check if the target square is active
        if (squaresState[to.x, to.y] != 0) {
            return false;
        }
        return true;
    }
    
    // Return a list of all possible moves for the current player.
    public List<Move> possibleMoves() {
        int[] rowDirect = {-1, -1, -1, 0, 0, 1, 1, 1};
        int[] colDirect = {-1, 0, 1, -1, 1, -1, 0, 1};

        var ret = new List<Move>();
        for (int x = 0 ; x < Size ; ++x)
            for (int y = 0 ; y < Size ; ++y)
                if (squares[x, y] == turn) {
                    Pos from = new Pos(x, y);
                    for (int adjCell = 0; adjCell < rowDirect.Length ; ++adjCell) {
                        Move move = new Move(from, new Pos(x + rowDirect[adjCell], y + colDirect[adjCell]));
                        if (validMove(move))
                            ret.Add(move);
                    }
                }
        return ret;
    }

    public bool hasValidMoves() {
        List<Move> moves = possibleMoves();
        return moves.Count > 0;
    }

    public (int, int) winLine(){
        (int, int) res = (0, Size - 1);
        (bool, bool) resFound = (false, false);
        bool[] inactiveLines = new bool[Size];

        for(int y = 0; y < Size; ++y){
            bool inactiveLine = true;
            for(int x = 0; x < Size; ++x){
                if(squaresState[x, y] == 0){
                    inactiveLine = false;
                    break;
                }
            }
            inactiveLines[y] = inactiveLine == true ? true : false;
        }

        for(int i = 0; i < Size; ++i){
            if(inactiveLines[i] == true && resFound.Item1 == false) {
                res.Item1 += 1;
            } else {
                resFound.Item1 = true;
            }

            if(inactiveLines[Size - 1 - i] == true && resFound.Item2 == false) {
                res.Item2 -= 1;
            } else {
                resFound.Item2 = true;
            }
        }
        return res;
    }
    
    // Update the game by having the current player make the given move.
    // Returns true if a capture was made.
    public bool move(Move m) {

        if (validMove(m)) {
            Pos from = m.from, to = m.to;
            bool capture = squares[to.x, to.y] > 0;

            // Make the move
            squares[from.x, from.y] = 0;
            squares[to.x, to.y] = turn;

            // Update pieces count and check for winner
            if (capture)
                pieces[3 - turn] -= 1;
            if ((turn == 1 && to.y == winLine().Item1) || (turn == 2 && to.y == winLine().Item2) || pieces[3 - turn] == 0)
                winner = turn;

            // Update the state of the squares
            squaresState[from.x, from.y] = 1;

            // Find isolated islands after moving
            var isolatedIslands = IsolatedIslandsFinder.findIsolatedIslands(squares, squaresState);
            Dictionary<(int, int), int> cells = new Dictionary<(int, int), int>();
            foreach (var island in isolatedIslands) {
                foreach (var cell in island) {
                    cells[cell] = squares[cell.Item1, cell.Item2]; // Store the state before marking inactive
                    squares[cell.Item1, cell.Item2] = 0;
                    if(squares[cell.Item1, cell.Item2] != 0){
                        pieces[squares[cell.Item1, cell.Item2]] -= 1;
                        squares[cell.Item1, cell.Item2] = 0;
                    }
                    squaresState[cell.Item1, cell.Item2] = 1;
                }
            }
            diction = cells;

            // Switch turns and increment move count
            turn = 3 - turn;
            moves += 1;

            return capture;
        } else {
            throw new Exception($"Move {m.from} {m.to} is outside the board boundaries or violates game rules.");
        }

    }
    
    // Reverse a previous move that was made.  When calling this method,
    // wasCapture must be true if the move was a capture.
    public void unmove(Move m, bool wasCapture) {
        foreach(KeyValuePair<(int, int), int> p in diction){
            squares[p.Key.Item1, p.Key.Item2] = p.Value;
            pieces[squares[p.Key.Item1, p.Key.Item2]] += 1;
            squaresState[p.Key.Item1, p.Key.Item2] = 1;
        }
        Pos from = m.from, to = m.to;
        moves -= 1;
        turn = 3 - turn;
        winner = 0;
        if (wasCapture)
            pieces[3 - turn] += 1;
        squares[to.x, to.y] = wasCapture ? 3 - turn : 0;
        squares[from.x, from.y] = turn;
        squaresState[from.x, from.y] = 0;
        squaresState[to.x, to.y] = 0;
    }
}

// A Player is a strategy for playing the game.  Given any game state,
// the Player's chooseMove method decides what move to make.
interface Player {
    Move chooseMove(Game game);
}

class Program {
    [STAThread]
    static void Main() {
        Player?[] players = { null, null, new MyAgent()};
        View.run(new Game(), players);
    }
}