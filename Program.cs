using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;

public class UnrecognisedMovement : Exception
{
    public UnrecognisedMovement()
    { }

    public UnrecognisedMovement(string message)
        : base("The given command: " + message +" does not fit the expected format -> (Knight char):(Cardinal direction) e.g. R:S")
    { }
}

public class Output
{
#pragma warning disable IDE1006 // Naming Styles - to fit given formatting
    public Knight red { get; set; }
    public Knight blue { get; set; }
    public Knight green { get; set; }
    public Knight yellow { get; set; }
    public Item magic_staff { get; set; }
    public Item helmet { get; set; }
    public Item dagger { get; set; }
    public Item axe { get; set; }
#pragma warning restore IDE1006 // Naming Styles

}

public class Item : Entity
{
    private readonly char _name = '\0';
    private readonly int _attackModifier;
    private readonly int _defenceModifier;
    private int[] _position;
    private bool _equipped = false;

    [JsonIgnore]
    public override char Name
    {
        get => _name;
    }

    [JsonIgnore]
    public int AttackModifier
    {
        get => _attackModifier;
    }

    [JsonIgnore]
    public int DefenceModifier
    {
        get => _defenceModifier;
    }

    public override int[] Position
    {
        get => _position;

        set => _position = value;
    }

    public bool Equipped
    {
        get => _equipped;

        set => _equipped = value;
    }

    [JsonIgnore]
    public override string Status { get; set; }


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
    private readonly char _name;
    private int[] _position;
    private string _status = "LIVE";
    private Item _item = (Item)null;
    private int _attack = 1;
    private int _defence = 1;

    [JsonIgnore]
    public override char Name
    {
        get => _name;
    }

    public override int[] Position
    {
        get => _position;

        set => _position = value;
    }

    public override string Status
    {
        get => _status;

        set => _status = value;

    }

    [JsonIgnore]
    public Item Item
    {
        get => _item;

        set
        {
            if (_item == (Item)null) _item = value as Item;
        }
    }

    public string HeldItem
    {
        get => _item == (Item)null ? (string)null : _item.Name.ToString();
    }

    public int Attack
    {
        get => _attack;

        set => _attack = value;
    }

    public int Defence
    {
        get => _defence;

        set => _defence = value;
    }

    public Knight() { }

    public Knight(char name, int[] pos)
    {
        _name = name;
        _position = pos;
    }

    internal void Drop()
    {
        _item.Equipped = false;
        _item = (Item)null;
    }

    internal void Drown()
    {
        _position = new int[] { };
        _attack = 0;
        _defence = 0;
        _status = "DROWNED";
    }

    internal void Die()
    {
        Drop();
        _attack = 0;
        _defence = 0;
        _status = "DEAD";
    }

    public int[] CopyPos()
    {
        return new int[] { _position[0], _position[1] };
    }
}

public class Entity
{
    private int[] _position = { -1, -1 };
    private char _name;

    public virtual int[] Position
    {
        get => _position;

        set => _position = value;
    }

    public virtual char Name
    {
        get => _name;

        set => _name = value;
    }

    public virtual string Status { get; set; }
}

class Program
{
    /**
        
    If a knight moves off the board then they are swept away and drown immediately.
    Further moves do not apply to DROWNED knights.
    The final position of a DROWNED knight is null.

    **/
    static bool CheckMoveForWater(int[] Coord)
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

    static int CheckMoveForFight(int[] Coord, List<Entity> Board)
    {
        for (int i = 0; i < Board.Count; i++)
        {
            if (Board[i].Position.SequenceEqual(Coord) && (Board[i].GetType() == typeof(Knight)))
            {
                if (Board[i].Status != "LIVE")
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
    static int CheckMoveForItem(int[] Coord, List<Entity> Board)
    {
        char[] itemPriority = { 'A', 'M', 'D', 'H' };
        int item = -1;

        for (int i = 0; i < Board.Count; i++)
        {
            if (Board[i].Position.SequenceEqual(Coord) && (Board[i].GetType() == typeof(Item)))
            {
                if (item == -1)
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
        int[] next = knight.CopyPos();
        if (knight.Status != "LIVE") return;

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

        bool water = CheckMoveForWater(next);
        if (water)
        {
            if (knight.Item != (Item)null) knight.Drop();
            knight.Drown();
            return;
        }

        int item = CheckMoveForItem(next, Board);
        if (item > -1)
        {
            knight.Item = (Item)Board[item];
            knight.Item.Equipped = true;

        }
        
        int fight = CheckMoveForFight(next, Board);
        if (fight > -1)
        {
            knight.Position = next;
            Fight(knight, (Knight)Board[fight]);
        }

        knight.Position = next;
        if(knight.Item != (Item)null) knight.Item.Position = next;
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
        var output = new Output
        {
            red = (Knight)Board[0],
            blue = (Knight)Board[1],
            green = (Knight)Board[2],
            yellow = (Knight)Board[3],

            magic_staff = (Item)Board[4],
            helmet = (Item)Board[5],
            dagger = (Item)Board[6],
            axe = (Item)Board[7],
        };

        var options = new JsonSerializerOptions { WriteIndented = true};
        string jsonString = JsonSerializer.Serialize(output, options);
        File.WriteAllText("final_state.json", jsonString);
    }

    static void Main()
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
            Items.Add(new Item(line[0], (int)Char.GetNumericValue(line[1]), (int)Char.GetNumericValue(line[2]),
                               new[] { (int)Char.GetNumericValue(line[3]), (int)Char.GetNumericValue(line[4]) }));
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
        //Console.ReadKey();
    }
}