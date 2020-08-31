using System;
using System.IO;

namespace KronaizlAsng8
{
    class Program
    {
        static void Main(string[] args)
        {
            //LexicalAnalyzer.LexicalAnalyzer temp = new LexicalAnalyzer.LexicalAnalyzer(path);
            Parser.Parser_Kronaizl temp = new Parser.Parser_Kronaizl();


            if (args.Length < 1)
                Console.Write("ERROR! INCORRECT NUMBER OF ARGUMENTS GIVEN! PLEASE PASS IN A VALID FILE PATH");
            else
            {
                if (!File.Exists(args[0]))
                {
                    Console.Write("ERROR! FILE NOT FOUND! PLEASE VERIFY FILEPATH" + "\r\n");
                }
                else
                {
                    temp.init(args[0]);
                }
            }
        }
    }
}
