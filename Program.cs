using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;

// Custom exception to be thrown when a move is not in the expected format
public class UnrecognisedMovement : Exception
{
    public UnrecognisedMovement()
    { }

    public UnrecognisedMovement(string message)
        : base("The given command: " + message +" does not fit the expected format -> (Knight char):(Cardinal direction) e.g. R:S")
    { }
}

// JSON output class
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

// Item is a child of Entity, so it can be stored in the same list as the Knights
// items are weapons and armour etc
public class Item : Entity
{
    // These values do not change after the constructor exits so are readonly
    private readonly char _name = '\0';
    private readonly int _attackModifier;
    private readonly int _defenceModifier;

    private int[] _position;
    private bool _equipped = false;

    // Accessors
    // JsonIgnore tags to prevent unnecessary data in the output file
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

    // Place holder property inherited from Entity, needed as status cannot be set
    // as ignored in Entity because it needs to be accessed in Knight
    [JsonIgnore]
    public override string Status { get; set; }

    // Constructors
    public Item() { }

    public Item(char name, int attMod, int defMod, int[] pos)
    {
        _name = name;
        _attackModifier = attMod;
        _defenceModifier = defMod;
        _position = pos;
    }
}

// Knight is a child of Entity so can be stored with Items in the Board
public class Knight : Entity
{
    // Name doesn't change after constructor
    private readonly char _name;

    private int[] _position;
    private string _status = "LIVE";
    private Item _item = (Item)null;
    private int _attack = 1;
    private int _defence = 1;

    // Accessors
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
            // if no item is held, set item to provided Item, otherwise ignore given Item
            if (_item == (Item)null) _item = value as Item;
        }
    }

    // Only used in JSON as the Item method above also shows the Item's properties
    public string HeldItem
    {
        // If no item is held, return null, otherwise return the char of the item
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

    // Constructors
    public Knight() { }

    public Knight(char name, int[] pos)
    {
        _name = name;
        _position = pos;
    }

    // Internal methods
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

    // Allows the position to be copied by value rather than passing a reference,
    // used in Move() to check if the next position has an item, water or an enemy
    public int[] CopyPos()
    {
        return new int[] { _position[0], _position[1] };
    }
}

// Base class for Knights and Items
public class Entity
{
    private int[] _position = { -1, -1 };
    private char _name;

    // Virtual methods are overwritten in children
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

// Main Program
class Program
{
    /**
        
    If a knight moves off the board then they are swept away and drown immediately.
    Further moves do not apply to DROWNED knights.
    The final position of a DROWNED knight is null.

    **/
    // Checks if the next move will move the knight into water
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
    // Calculates the fighting scores and tells which knight to die
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

    // Checks if the next move will move the knight onto the same square as another knight
    static int CheckMoveForFight(int[] Coord, List<Entity> Board)
    {
        for (int i = 0; i < Board.Count; i++)
        {
            if (Board[i].Position.SequenceEqual(Coord) && (Board[i].GetType() == typeof(Knight)))
            {
                // Only fight if the knight in the square is actually alive
                if (Board[i].Status != "LIVE")
                {
                    // Returns the index of the knight in the board list of entities
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
    // Checks if the next move will move the knight onto the same square as an item
    static int CheckMoveForItem(int[] Coord, List<Entity> Board)
    {
        // Used to pick up the best item on a square in the event that there are multiple
        char[] itemPriority = { 'A', 'M', 'D', 'H' };
        // -1 if there is no item
        int item = -1;

        for (int i = 0; i < Board.Count; i++)
        {
            if (Board[i].Position.SequenceEqual(Coord) && (Board[i].GetType() == typeof(Item)))
            {
                if (item == -1)
                {
                    item = i;
                }
                // If there is a better item, use that one
                else if(Array.IndexOf(itemPriority, Board[i].Name) < Array.IndexOf(itemPriority, Board[item].Name))
                {
                    item = i;
                }
            }
        }

        // Returns the index of the item in the Board list of entities
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
        // Do not bother moving dead knights
        if (knight.Status != "LIVE") return;
        // Copy the position of the knight currently
        int[] next = knight.CopyPos();

        // Update the position depending on the direction
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

        // Check if there is water
        bool water = CheckMoveForWater(next);
        if (water)
        {
            // Knight drops their item on their current square before moving to the water
            if (knight.Item != (Item)null) knight.Drop();
            knight.Drown();
            // Return as the knight cannot move or fight now
            return;
        }

        // Check if there is an item
        int item = CheckMoveForItem(next, Board);
        if (item > -1)
        {
            // Pickup the item that is there
            knight.Item = (Item)Board[item];
            // Tell the item it is equipped
            knight.Item.Equipped = true;

        }
        
        // Check if there is another knight on the next square
        int fight = CheckMoveForFight(next, Board);
        if (fight > -1)
        {
            // Move the knight to the square before fighting
            knight.Position = next;
            Fight(knight, (Knight)Board[fight]);
        }
        // Move the knight to the square, if there fought then they are already there
        // but this will simply move them to the same square
        knight.Position = next;
        // If an item is held, move the item with the knight
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
    // Fill the board list with all the knights and items
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

        // Return the filled board
        return board;
    }

    static void WriteOut(List<Entity> Board)
    {
        // Create the JSON object
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

        // Serialise and format then output
        var options = new JsonSerializerOptions { WriteIndented = true};
        string jsonString = JsonSerializer.Serialize(output, options);
        File.WriteAllText("final_state.json", jsonString);
    }

    static void Main()
    {
        // Create lists for entities and the board
        List<Entity> board = new List<Entity>();
        List<Knight> Knights = new List<Knight>();
        List<Item> Items = new List<Item>();

        // Load in the knights and items and construct the data into Entities
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

        // Open the moves file        
        System.IO.StreamReader movesFile = new System.IO.StreamReader(@"moves.txt");

        // While there is another line available
        while((line = movesFile.ReadLine()) != null)
        {
            switch (line)
            {
                // On game-start, populate the board
                case "GAME-START":
                    board = PopulateBoard(Knights, Items);
                    break;

                // On game end, output to a JSON file
                case "GAME-END":
                    WriteOut(board);
                    break;

                // Regex match any of the knight names and any cardinal direction
                case string _ when Regex.IsMatch(line, @"[RBGY]:[NESW]"):
                    // Move the according knight in the given direction
                    foreach (Knight k in Knights)
                    {
                        if (k.Name == line[0])
                        {
                            Move(k, line[2], board);
                        }
                    }
                    break;

                // Any other command is not recognised
                default:
                    throw new UnrecognisedMovement(line);
            }
        }

        // Close mvoes file before closing console
        movesFile.Close();

        // Uncomment to hang console before closing
        // Console.WriteLine("Press any key to close");
        // Console.ReadKey();
    }
}