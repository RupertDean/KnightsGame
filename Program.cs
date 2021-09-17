using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


public class UnrecognisedMovement : Exception
{
    public UnrecognisedMovement()
    {
    }

    public UnrecognisedMovement(string message)
        : base("The given command: " + message +" does not fit the expected format -> (Knight char):(Cardinal direction) e.g. R:S")
    { 
    }
}

public class Item : Entity
{
    private char _name;
    private int _attackModifier;
    private int _defenceModifier;
    private int[] _position;

    public new char Name
    {
        get => _name;
    }

    public int AttackModifier
    {
        get => _attackModifier;
    }

    public int DefenceModifier
    {
        get => _defenceModifier;
    }

    public new int[] Position
    {
        get => _position;

        set => _position = value;
    }

    public Item() { }

    public Item(char name, int attMod, int defMod, int[] pos)
    {
        _name = name;
        _attackModifier = attMod;
        _defenceModifier = defMod;
        _position = pos;
    }
}

public class Knight : Entity
{
    private char _name;
    private int _attack = 1;
    private int _defence = 1;
    private Item _item = (Item)null;
    private int[] _position;
    private bool _dead = false;

    public new char Name
    {
        get => _name;
    }

    public int Attack
    {
        get => _attack;
    }

    public int Defence
    {
        get => _defence;
    }

    public Item Item
    {
        get => _item;

        set
        {
            if (_item == (Item)null) _item = value as Item;
        }
    }

    public new int[] Position
    {
        get => _position;

        set => _position = value;
    }

    public new bool Dead
    {
        get => _dead;
    }

    public Knight() { }

    public Knight(char name, int[] pos)
    {
        _name = name;
        _position = pos;
    }

    internal void Drop()
    {
        _item = (Item)null;
    }

    internal void Drown()
    {
        _position = new int[] { -1, -1 };
        _attack = 0;
        _defence = 0;
        _dead = true;
    }

    internal void Die()
    {
        Drop();
        _attack = 0;
        _defence = 0;
        _dead = true;
    }
}

public class Entity
{
    private int[] _position = { -1, -1};
    internal readonly bool Dead;

    public int[] Position
    {
        get => _position;
    }
    public Array Name { get; internal set; }

    public Entity()
    {

    }
}

class Program
{
    /**
        
    If a knight moves off the board then they are swept away and drown immediately.
    Further moves do not apply to DROWNED knights.
    The final position of a DROWNED knight is null.

    **/
    static bool checkMoveForWater(int[] Coord)
    {
        if ((Coord[0] < 0) || (Coord[0] > 7) || (Coord[1] < 0) || Coord[1] > 7) return true;
        return false;
    }

    /**
        ### Fighting

    Each Knight has a base attack and defence score of 1:

    `Attack  (1)`  
    `Defence (1)`  

    If one knight moves onto the tile of another knight then they will attack.
    The knight already on the tile will defend.

    The outcome of a fight is determined as follows:
    * The attacker takes their base attack score and adds any item modifiers.
    * The attacker adds 0.5 to their attack score (for the element of surprise).
    * The defender takes their base defence score and adds any item modifiers.
    * The attackers final attack score is compared to the defenders final defence score.
    * The higher score wins, the losing knight dies.

    DEAD knights drop any equipped items immediately.
    Further moves do not apply to DEAD knights.
    The final position of a DEAD knight is the tile that they die on.

    A DEAD or DROWNED knight has attack 0 and defence 0.

        **/
    static void Fight(Knight Attacker, Knight Defender)
    {
        float attackerScore = ((float)(Attacker.Attack + 0.5 + ((Attacker.Item == (Item)null) ? 0 : Attacker.Item.AttackModifier)));
        float defenderScore = ((float) Defender.Defence + ((Defender.Item ==(Item)null) ? 0 : Defender.Item.DefenceModifier));

        if(attackerScore - defenderScore > 0)
        {
            Defender.Die();
            return;
        }

        Attacker.Die();
        return;
    }

    static int checkMoveForFight(int[] Coord, List<Entity> Board)
    {
        for (int i = 0; i < Board.Count; i++)
        {
            if ((Board[i].Position == Coord) && (Board[i].GetType() == typeof(Knight)))
            {
                if (!Board[i].Dead)
                {
                return i;
                }
            }
        }

        return -1;
    }

    /**
        ### Items

    Around the board are the following four items.

    `Axe         (A):  +2 Attack`  
    `Dagger      (D):  +1 Attack`  
    `Helmet      (H):  +1 Defence`  
    `MagicStaff  (M):  +1 Attack, +1 Defence`  

    They start at the following locations:

    `Axe         (A) (2,2)`  
    `Dagger      (D) (2,5)`  
    `MagicStaff  (M) (5,2)`  
    `Helmet      (H) (5,5)`  

    If a Knight moves onto a tile with an item they are immediately equipped with that item, gaining the bonus.
    A Knight may only hold one item.
    If a knight with an item moves over another item then they ignore it.
    If a knight moves onto a tile which has two items on it then they pick up the best item in this order: (A, M, D, H).
    Knights will pick up an item on a tile before fighting any enemies on that tile.
    Knights that die in battle drop their item (if they have one).
    Knights that drown throw their item to the bank before sinking down to Davy Jones' Locker - the item is left on the last valid tile that the knight was on.

    **/     
    static int checkMoveForItem(int[] Coord, List<Entity> Board)
    {
        char[] itemPriority = { 'A', 'M', 'D', 'H' };
        int item = -1;

        for (int i = 0; i < Board.Count; i++)
        {
            if (Board[i].Position == Coord && (Board[i].GetType() == typeof(Item)))
            {
                if(item == -1)
                {
                    item = i;
                }
                else if(Array.IndexOf(itemPriority, Board[i].Name) < Array.IndexOf(itemPriority, Board[item].Name))
                {
                    item = i;
                }
            }
        }

        return item;
    }

    /**
    ### Movement

    Each Knight moves one tile at a time in one of four directions.

    `North (N)  (UP)`  
    `East  (E)  (RIGHT)`  
    `South (S)  (DOWN)`  
    `West  (W)  (LEFT)`  

    If a knight moves off the board then they are swept away and drown immediately.

        **/
    static void Move(Knight knight, char Direction, List<Entity> Board)
    {
        int[] next = knight.Position;
        if (knight.Dead) return;

        switch (Direction)
        {
            case 'N':
                next[0]--;
                break;
            case 'E':
                next[1]++;
                break;
            case 'S':
                next[0]++;
                break;
            case 'W':
                next[0]--;
                break;

            default:
                return;                      
        }

        bool water = checkMoveForWater(next);
        if (water)
        {
            knight.Drop();
            knight.Drown();
            return;
        }

        int item = checkMoveForItem(next, Board);
        if (item > -1)
        {
            knight.Item = (Item)Board[item];
        }
        
        int fight = checkMoveForFight(next, Board);
        if (fight > -1)
        {
            knight.Position = next;
            Fight(knight, (Knight)Board[fight]);
        }

        knight.Position = next;
        return;
    }

    /**
        ### Game

    The initial state of the board looks like this:
    ```
    (0,0) _ _ _ _ _ _ _ _ (0,7)
            |R|_|_|_|_|_|_|Y|
            |_|_|_|_|_|_|_|_|
            |_|_|A|_|_|D|_|_|
            |_|_|_|_|_|_|_|_|
            |_|_|_|_|_|_|_|_|
            |_|_|M|_|_|H|_|_|
            |_|_|_|_|_|_|_|_|
            |B|_|_|_|_|_|_|G|
    (7,0)                 (7,7)
    ```

        **/
    static List<Entity> PopulateBoard(List<Knight> Knights, List<Item> Items)
    {
        List<Entity> board = new List<Entity>();

        for(int i = 0; i < Knights.Count; i++)
        {
            board.Add(Knights[i]);
        }

        for(int i = 0; i < Items.Count; i++)
        {
            board.Add(Items[i]);
        }

        return board;
    }

    static void WriteOut(List<Entity> Board)
    {

    }

    static void Main(string[] args)
    {
        List<Entity> board = new List<Entity>();
        List<Knight> Knights = new List<Knight>();
        List<Item> Items = new List<Item>();

        string line;
        System.IO.StreamReader knightsFile = new System.IO.StreamReader(@"knights.txt");
        while ((line = knightsFile.ReadLine()) != null)
        {
            Knights.Add(new Knight(line[0], new[] { (int)Char.GetNumericValue(line[1]), (int)Char.GetNumericValue(line[2]) }));
        }
        knightsFile.Close();
        
        System.IO.StreamReader itemsFile = new System.IO.StreamReader(@"items.txt");
        while ((line = itemsFile.ReadLine()) != null)
        {
            Items.Add(new Item(line[0], line[1], line[2], new[] { (int)Char.GetNumericValue(line[3]), (int)Char.GetNumericValue(line[4]) } ));
        }
        itemsFile.Close();
        
        System.IO.StreamReader movesFile = new System.IO.StreamReader(@"moves.txt");

        while((line = movesFile.ReadLine()) != null)
        {
            switch (line)
            {
                case "GAME-START":
                    board = PopulateBoard(Knights, Items);
                    System.Console.WriteLine("Game Started");
                    break;

                case "GAME-END":
                    WriteOut(board);
                    System.Console.WriteLine("Game Finished");
                    break;

                case string _ when Regex.IsMatch(line, @"[RBGY]:[NESW]"):
                    foreach (Knight k in Knights)
                    {
                        if (k.Name == line[0])
                        {
                            Move(k, line[2], board);
                        }
                    }
                    break;

                default:
                    throw new UnrecognisedMovement(line);
            }
        }

        Console.WriteLine("Press any key to close");
        movesFile.Close();
        Console.ReadKey();
    }
}