using System;
using System.Text.RegularExpressions;

namespace KnightsGame_RichPharm
{

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

    class Program
    {
        /**
        
        If a knight moves off the board then they are swept away and drown immediately.
        Further moves do not apply to DROWNED knights.
        The final position of a DROWNED knight is null.

        **/
        static void Drown()
        {

        }

        static void checkMoveForWater()
        {
            Drown();
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
        static void Fight()
        {
            if (true) LoseFight();
        }

        static void checkMoveForFight()
        {
            Fight();
        }

        static void LoseFight()
        {
            Drop();
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
        static void Drop()
        {

        }
        
        static void PickUp()
        {

        }

        static void checkMoveForItem()
        {
            PickUp();
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
        static void Move(char Knight, char Direction)
        {
            checkMoveForWater();
            checkMoveForItem();
            checkMoveForFight();
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
        static char[,] PopulateBoard()
        {
            char[,] board = {
                { 'R', '0', '0', '0', '0', '0', '0', 'Y' },
                { '0', '0', '0', '0', '0', '0', '0', '0' },
                { '0', '0', 'A', '0', '0', 'D', '0', '0' },
                { '0', '0', '0', '0', '0', '0', '0', '0' },
                { '0', '0', '0', '0', '0', '0', '0', '0' },
                { '0', '0', 'M', '0', '0', 'H', '0', '0' },
                { '0', '0', '0', '0', '0', '0', '0', '0' },
                { 'B', '0', '0', '0', '0', '0', '0', 'G' }
            };

            return board;
        }

        static void WriteOut(Char[,] Board)
        {

        }

        static void Main(string[] args)
        {
            char[,] board = { };
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(@"moves.txt");

            while((line = file.ReadLine()) != null)
            {
                switch (line)
                {
                    case "GAME-START":
                        board = PopulateBoard();
                        System.Console.WriteLine("Game Started");
                        break;
                    case "GAME-END":
                        WriteOut(board);
                        System.Console.WriteLine("Game Finished");
                        break;
                    case string _ when Regex.IsMatch(line, @"[RBGY]:[NESW]"):
                        Move(line[0], line[2]);

                        break;
                    default:
                        throw new UnrecognisedMovement(line);
                }
            }

            Console.WriteLine("Press any key to close");
            file.Close();
            Console.ReadKey();
        }
    }
}
