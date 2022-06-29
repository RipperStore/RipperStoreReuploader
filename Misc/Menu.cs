using System;
using System.Collections.Generic;
using System.Text;
using static System.Console;

namespace RipperStoreReuploader.Misc
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
            WriteLine("");
            WriteLine("██████╗ ██╗██████╗ ██████╗ ███████╗██████╗ ███████╗████████╗ ██████╗ ██████╗ ███████╗");
            WriteLine("██╔══██╗██║██╔══██╗██╔══██╗██╔════╝██╔══██╗██╔════╝╚══██╔══╝██╔═══██╗██╔══██╗██╔════╝");
            WriteLine("██████╔╝██║██████╔╝██████╔╝█████╗  ██████╔╝███████╗   ██║   ██║   ██║██████╔╝█████╗  ");
            WriteLine("██╔══██╗██║██╔═══╝ ██╔═══╝ ██╔══╝  ██╔══██╗╚════██║   ██║   ██║   ██║██╔══██╗██╔══╝  ");
            WriteLine("██║  ██║██║██║     ██║     ███████╗██║  ██║███████║   ██║   ╚██████╔╝██║  ██║███████╗");
            WriteLine("╚═╝  ╚═╝╚═╝╚═╝     ╚═╝     ╚══════╝╚═╝  ╚═╝╚══════╝   ╚═╝    ╚═════╝ ╚═╝  ╚═╝╚══════╝");
            WriteLine("");
            WriteLine("             Welcome to RipperStore-Reuploader, what would you like to do?            ");
            WriteLine("               (Use your arrow keys to navigate, press enter to confirm)              ");
            WriteLine("");
            WriteLine($"             - logged in as: {ReuploadHelper.customApiUser.DisplayName}");
            WriteLine("");
            WriteLine("                                 Available Actions:                                  ");
            WriteLine("");

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
