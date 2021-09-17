using System;

namespace KnightsGame_RichPharm
{
    class Program
    {
        static void Move()
        {

        }

        static void PickUp()
        {

        }
        
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

        static void Main(string[] args)
        {
            char[,] board = PopulateBoard();


            Console.WriteLine("Press any key to close");
            Console.ReadKey();
        }
    }
}
