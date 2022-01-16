using System;
using System.Collections.Generic;

namespace DocumentProcessing
{
    class Program
    {
        static void Main(string[] args)
        {
            #if DEBUG
                Console.WriteLine("Программа собрана в debug-конфигурации!");
            #endif
            if (!checkArgs(args))
            {
                return;
            }
            Handler documentHandler = new Handler(args[0]);
            if (documentHandler.SettingsIsRead)
            { 
                documentHandler.ProcessFiles();
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Не удалось применить настройки! Обработка отменена.");
                Console.ResetColor();
                return;
            }            
        }
        protected static bool checkArgs(string[] args)
        {
            if (args.Length == 0)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Укажите каталог, содержащую файлы для обработки!");
                Console.WriteLine("Например: DocumentProcessing.exe \"<полный_путь_к_каталогу>\"");
                Console.ResetColor();
                return false;
            }
            else if (args.Length > 1)
            {
                Console.WriteLine("Использование: DocumentProcessing.exe \"<полный_путь_к_каталогу>\"");
                return false;
            }

            return true;
        }
    }
}
