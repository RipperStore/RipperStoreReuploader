using System;
using System.Collections.Generic;
using System.Text;
using static System.Console;

namespace RipperStoreReuploader
{
    internal class Menu
    {
        private int SelectedIndex;
        private string[] Options;

        public Menu(string[] options)
        {
            Options = options;
            SelectedIndex = 0;
        }

        private void DisplayOptions()
        {
            Console.WriteLine("");
            Console.WriteLine("██████╗ ██╗██████╗ ██████╗ ███████╗██████╗ ███████╗████████╗ ██████╗ ██████╗ ███████╗");
            Console.WriteLine("██╔══██╗██║██╔══██╗██╔══██╗██╔════╝██╔══██╗██╔════╝╚══██╔══╝██╔═══██╗██╔══██╗██╔════╝");
            Console.WriteLine("██████╔╝██║██████╔╝██████╔╝█████╗  ██████╔╝███████╗   ██║   ██║   ██║██████╔╝█████╗  ");
            Console.WriteLine("██╔══██╗██║██╔═══╝ ██╔═══╝ ██╔══╝  ██╔══██╗╚════██║   ██║   ██║   ██║██╔══██╗██╔══╝  ");
            Console.WriteLine("██║  ██║██║██║     ██║     ███████╗██║  ██║███████║   ██║   ╚██████╔╝██║  ██║███████╗");
            Console.WriteLine("╚═╝  ╚═╝╚═╝╚═╝     ╚═╝     ╚══════╝╚═╝  ╚═╝╚══════╝   ╚═╝    ╚═════╝ ╚═╝  ╚═╝╚══════╝");
            Console.WriteLine("");
            Console.WriteLine("            Welcome to RipperStore-Reuploader, what would you like to do?            ");
            Console.WriteLine("              (Use your arrow keys to navigate, press enter to confirm)              ");
            Console.WriteLine("");
            Console.WriteLine("                                 Available Actions:                                  ");
            Console.WriteLine("");

            for (int i = 0; i < Options.Length; i++)
            {


                string currentOption = Options[i];
                string prefix;

                if (i == SelectedIndex)
                {
                    prefix = "■";
                    ForegroundColor = ConsoleColor.Black;
                    BackgroundColor = ConsoleColor.White;
                }
                else
                {
                    prefix = " ";
                    ForegroundColor = ConsoleColor.White;
                    BackgroundColor = ConsoleColor.Black;
                }
                WriteLine($"> {prefix} {currentOption} ");
            }
            ResetColor();
        }

        public int Run()
        {
            ConsoleKey keyPressed;
            do
            {
                Clear();
                DisplayOptions();

                ConsoleKeyInfo keyInfo = ReadKey(true);
                keyPressed = keyInfo.Key;

                if (keyPressed == ConsoleKey.UpArrow)
                {
                    SelectedIndex--;
                    if (SelectedIndex == -1) SelectedIndex = Options.Length - 1;
                }
                else if (keyPressed == ConsoleKey.DownArrow)
                {
                    SelectedIndex++;
                    if (SelectedIndex == Options.Length) SelectedIndex = 0;
                }


            } while (keyPressed != ConsoleKey.Enter);

            return SelectedIndex;
        }
    }
}
