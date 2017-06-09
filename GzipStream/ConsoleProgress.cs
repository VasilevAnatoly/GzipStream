using System;

namespace GzipStream
{
    public static class ConsoleProgress
    {
        public static void ProgressBar(long current, long overall)
        {
            Console.CursorVisible = false;
            Console.CursorLeft = 0;
            float onechunk = 30.0f / overall;
            long percentage = overall / 100;

            int position = 1;
            for (int i = 0; i < onechunk * current; i++)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            for (int i = position; i <= 31; i++)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            Console.CursorLeft = 35;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(" " + current / percentage + "%" + " " + current + " of " + overall);

            if (current / percentage == 100)
            {
                Console.CursorLeft = 35;
                Console.Write(" " + "Completed" + " " + current + " of " + overall);
            }
        }
    }
}
