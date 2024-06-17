using Cairo;
using Gdk;
using Gtk;
using System;
using System.Collections.Generic;
using CairoHelper = Gdk.CairoHelper;
using Window = Gtk.Window;



class View : Window {
    Game game;
    Player?[] players;
    Move? lastMove = null;
    Pos? moveFrom = null;
    bool wasCapture;
    // Dictionary<(int, int), int> islands;
    Stack<(Move, bool)> undoStack = new Stack<(Move, bool)>();
    bool undone = false;
    const int Square = 100;  // square size in pixels
    Pixbuf blackDisk = new Pixbuf("black_disk.png"),
           redDisk = new Pixbuf("red_disk.png");

    public View(Game game, Player?[] players) : base("") {
        this.game = game;
        this.players = players;
        Resize(Square * Game.Size, Square * Game.Size);
        AddEvents((int) (EventMask.KeyPressMask | EventMask.ButtonPressMask));
        setTitle();
    }
    
    void setTitle() {
        Player? p1 = players[1];
        string name1 = p1 == null ? "Human" : p1.GetType().Name;
        string name2 = players[2]!.GetType().Name;
        Title = $"Passo: red = {name1} | black = {name2}";
    }
    
    void move() {
        undone = false;
        lastMove = players[game.turn]!.chooseMove(game);
        wasCapture = game.move(lastMove);
        undoStack.Push((lastMove, wasCapture));
        QueueDraw();

        if (!game.hasValidMoves() || game.pieces[game.turn] == 0) {
            game.winner = 3 - game.turn; // The current player loses
            Console.WriteLine("the game is ended");
        }
    }

    void unmove() {
        if (undoStack.Count == 0) Console.WriteLine("Application");
        else {
            undone = true;
            (Move undoMove, bool undoCapture) = undoStack.Pop();

            game.unmove(undoMove, undoCapture);
            QueueDraw();
        }
    }
    
    static RGBA color(string name) {
        RGBA c = new RGBA();
        if (!c.Parse(name))
            throw new Exception("unknown color");
        return c;
    }
    
    static void drawLine(Context c, RGBA color, int lineWidth, int x1, int y1, int x2, int y2) {
        CairoHelper.SetSourceRgba(c, color);
        c.LineWidth = lineWidth;
        c.MoveTo(x1, y1);
        c.LineTo(x2, y2);
        c.Stroke();
    }
    
    static void drawRectangle(Context c, RGBA color, int lineWidth, int x, int y, int width, int height) {
        CairoHelper.SetSourceRgba(c, color);
        c.LineWidth = lineWidth;
        c.Rectangle(x, y, width, height);
        c.Stroke();
    }
    
    static void fillRectangle(Context c, RGBA color, int x, int y, int width, int height) {
        CairoHelper.SetSourceRgba(c, color);
        c.Rectangle(x, y, width, height);
        c.Fill();
    }
    
    static void drawImage(Context c, Pixbuf pixbuf, int x, int y) {
        CairoHelper.SetSourcePixbuf(c, pixbuf, x, y);
        c.Paint();
    }
    
    void highlight(Context c, RGBA color, int x, int y) {
        drawRectangle(c, color, 3, Square * x, Square * y, Square, Square);
    }
    
    protected override bool OnDrawn(Context c) {
        RGBA peru = color("peru"), paleGoldenrod = color("paleGoldenrod"),
              lightGreen = color("lightGreen"), darkGray = color("darkGray"),
              green = color("green");
        
        for (int x = 0 ; x < Game.Size ; ++x)
            for (int y = 0 ; y < Game.Size ; ++y) {
                if(game.squaresState[x, y] == 0){
                    fillRectangle(c, paleGoldenrod,
                                Square * x, Square * y, Square, Square);
                }
                else{
                    fillRectangle(c, peru,
                                Square * x, Square * y, Square, Square);
                }
                drawLine(c, peru, 1,  Square * x, Square * y, Square * x + 100, Square * y);
                drawLine(c, peru, 1,  Square * x, Square * y, Square * x, Square * y + 100);
                if (game.winner > 0 && game.squares[x, y] == game.winner)
                    fillRectangle(c, lightGreen,
                                    Square * x + 4, Square * y + 4, Square - 8, Square - 8);
                if (lastMove != null && !undone && wasCapture &&
                    x == lastMove.to.x && y == lastMove.to.y) {
                        drawLine(c, darkGray, 4, Square * x + 4, Square * y + 4,
                                        Square * (x + 1) - 4, Square * (y + 1) - 4);
                        drawLine(c, darkGray, 4, Square * x + 4, Square * (y + 1) - 4,
                                        Square * (x + 1) - 4, Square * y + 4);
                }
                if (game.squares[x, y] > 0 && game.squaresState[x, y] == 0)
                    drawImage(c, game.squares[x, y] == 1 ? redDisk : blackDisk,
                                Square * x + 5, Square * y + 5);
            }
        
        if (!undone && moveFrom != null)
            highlight(c, green, moveFrom.x, moveFrom.y);
        else if (!undone && lastMove != null) {
            highlight(c, green, lastMove.from.x, lastMove.from.y);
            highlight(c, green, lastMove.to.x, lastMove.to.y);
        }
        
        return true;
    }

    bool gameOver() {
        if (game.winner > 0) {
            Application.Quit();
            return true;
        }
        return false;
    }
    
protected override bool OnButtonPressEvent(EventButton e) {
    if (gameOver() || game.turn == 2) // If the game is over or it's black's turn, return true
        return true;
    
    int x = (int)e.X / Square, y = (int)e.Y / Square;

    if (moveFrom != null) {
        Move move = new Move(moveFrom, new Pos(x, y));
        if (game.validMove(move)) {
            moveFrom = null;
            game.move(move);
            QueueDraw();
            if (game.winner == 0)
                this.move();  // Make MyAgent move
            return true;
        }
    }
    if (game.squares[x, y] == 1) {
        moveFrom = new Pos(x, y);
        QueueDraw();
    }

    return true;
}
    
    protected override bool OnDeleteEvent(Event ev) {
        Application.Quit();
        return true;
    }
    
    public static void run(Game game, Player?[] players) {
        Application.Init();
        View v = new View(game, players);
        v.ShowAll();
        Application.Run();
    }
}
