/*
File: LexicalAnalyzer_Kronaizl.cs
Author: Cody Kronaizl
Date: 2/5/2020
Class: CSC-446
Description:
This file details all procedures needed to build a simplified Java scanner.
It is designed to be called directly by another class or by a Java parser.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace LexicalAnalyzer
{
    //establish token enumerators
    public enum keywords
    {
        begint = 1,
        progt = 2,
        constt = 3,
        vart = 4,
        proct = 5,
        ift = 6,
        whilet = 7,
        thent = 8,
        elset = 9,
        readt = 10,
        writet = 11,
        truet = 12,
        falset = 13,
        realt = 14,
        integert = 15,
        chart = 16,
        boolt = 17,
        arrayt = 18,
        endt = 19,
        addopt = 20,
        mulopt = 21,
        assignopt = 22,
        lparent = 23,
        rparent = 24,
        commat = 25,
        semit = 26,
        periodt = 27,
        numt = 28,
        idt = 29,
        eoft = 30,
        unknownt = 31,
        relopt = 32,
        finalt = 33,
        classt = 34,
        extendst = 35,
        voidt = 36,
        publict = 37,
        statict = 38,
        returnt = 39,
        lbracet = 40,
        rbracet = 41,
        floatt = 42,
        writelnt = 43
    }
    //store most commonly used variables globally
    public static class Globals
    {
        public static keywords Token;
        public static List<keywords> keyWords = Enum.GetValues(typeof(keywords)).Cast<keywords>().ToList();
        public static string Lexeme;
        public static char ch;
        public static int LineNo;
        public static List<int> Value = new List<int>();
        public static List<double> ValueR = new List<double>();
        public static List<string> Literal = new List<string>();
        public static StreamReader inputFile;
    }

    public class LexicalAnalyzer
    {   
        //constructor for lexer
        public LexicalAnalyzer(string filePath)
        {
            Globals.inputFile = new StreamReader(filePath);
            Globals.LineNo = 1;
        }
        //main procedure called, builds next token character by character 
        public void GetNextToken()
        {
            while (Globals.ch <= 32)
                GetNextCh();
                if(!Globals.inputFile.EndOfStream)
                {
                    ProcessToken();
                    if (Globals.Lexeme == "")
                        //Console.Write("Token - " + Globals.Token + " | Plain text - " + Globals.Lexeme + " | Line # - " + Globals.LineNo + "\r\n" + "\r\n");
                    //else
                        GetNextToken();
                }
                else
                {
                    Globals.Token = keywords.eoft;
                }
            
        }
        //character grabbing function, includes logic to navigate whitespace
        static void GetNextCh()
        {
            Globals.ch = (char)Globals.inputFile.Read();
            if (Globals.ch == 10)
            {
                Globals.LineNo++;
            }
            else if(Globals.ch == 32)
            {
                //GetNextCh();
            }
        }
        //if the token is a word token, process it properly
        static void ProcessWordToken()
        {
            ReadRest();
            associateToken();
        }
        static void ProcessNumToken()
        {
            ReadRestNum();
            if (Globals.ch == '.')
            {
                Globals.Lexeme = Globals.Lexeme + Globals.ch;
                GetNextCh();
                if (isnum(Globals.ch))
                {
                    ReadRestNum();
                    Globals.Token = keywords.numt;
                    Globals.ValueR.Add(Convert.ToDouble(Globals.Lexeme));   //convert string lexeme value to double
                }
                else
                {
                    Globals.Lexeme = Globals.Lexeme.Remove(Globals.Lexeme.Length - 1);
                    Globals.Token = keywords.numt;
                    Globals.Value.Add(Convert.ToInt32(Globals.Lexeme));
                    Globals.ch = '.';
                }
            }   
            else
            {
                Globals.Token = keywords.numt;
                Globals.Value.Add(Convert.ToInt32(Globals.Lexeme));
            }
        }
        //Main logic of the scanner, this is where the correct corresponding token is determined by using a multitude of if statements
        static void ProcessToken()
        {
            Globals.Lexeme = Globals.ch.ToString();
            GetNextCh();
            if (isword(Globals.Lexeme[0]))
                ProcessWordToken();
            else if (isnum(Globals.Lexeme[0]))
                ProcessNumToken();
            else if (Globals.Lexeme[0] == '*' | (Globals.Lexeme[0] == '/' && Globals.ch != '*' && Globals.ch != '/') | (Globals.Lexeme[0] == '&' && Globals.ch == '&'))
            {
                Globals.Token = keywords.mulopt;
                if(Globals.ch == '&')
                {
                    Globals.Lexeme = Globals.Lexeme + Globals.ch;
                    GetNextCh();
                }
            }
            else if (Globals.Lexeme[0] == '/' && Globals.ch == '/')
                ProcessComment();
            else if (Globals.Lexeme[0] == '/' && Globals.ch == '*')
                ProcessMultiComment();
            else if (Globals.Lexeme[0] == '=')
            {
                if (Globals.ch == '=')
                {
                    Globals.Token = keywords.relopt;
                    Globals.Lexeme = Globals.Lexeme + Globals.ch;
                    GetNextCh();
                }
                else
                {
                    Globals.Token = keywords.assignopt;
                }

            }
            else if (Globals.Lexeme[0] == '{')
            {
                Globals.Token = keywords.begint;
            }
            else if (Globals.Lexeme[0] == '}')
            {
                Globals.Token = keywords.endt;
            }
            else if (Globals.Lexeme[0] == '(')
            {
                Globals.Token = keywords.lparent;
            }
            else if (Globals.Lexeme[0] == ')')
            {
                Globals.Token = keywords.rparent;
            }
            else if (Globals.Lexeme[0] == ',')
            {
                Globals.Token = keywords.commat;
            }
            else if (Globals.Lexeme[0] == '.')
            {
                Globals.Token = keywords.periodt;
            }
            else if (Globals.Lexeme[0] == ';')
            {
                Globals.Token = keywords.semit;
            }
            else if (Globals.Lexeme[0] == '"')
            {
                Globals.Lexeme = Globals.Lexeme + Globals.ch;
                while (Globals.ch != '"')
                {
                    GetNextCh();
                    if (Globals.inputFile.EndOfStream | Globals.ch == 10)
                    {
                        Console.Write("ERROR! IMPROPERLY TERMINATED STRING LITERAL! PRESS ENTER TO EXIT");
                        Globals.inputFile.Close();
                        Console.ReadKey();
                        System.Environment.Exit(1);
                    }
                    Globals.Lexeme = Globals.Lexeme + Globals.ch;
                }
                Globals.ch = (char)9;
                Globals.Literal.Add(Globals.Lexeme);
                Globals.Token = keywords.realt;
            }
            else if (Globals.Lexeme[0] == '[')
            {
                Globals.Token = keywords.lbracet;
            }
            else if (Globals.Lexeme[0] == ']')
            {
                Globals.Token = keywords.rbracet;
            }
            else if (Globals.Lexeme[0] == '>' | Globals.Lexeme[0] == '<' | Globals.Lexeme[0] == '!')
            {
                Globals.Token = keywords.relopt;
                if (Globals.ch == '=')
                {
                    Globals.Lexeme = Globals.Lexeme + Globals.ch;
                    GetNextCh();
                }
            }
            else if (Globals.Lexeme[0] == '+' | Globals.Lexeme[0] == '-' | (Globals.Lexeme[0] == '|' && Globals.ch == '|'))
            {
                Globals.Token = keywords.addopt;
                {
                    if(Globals.ch =='|')
                    {
                        Globals.Lexeme = Globals.Lexeme + Globals.ch;
                        GetNextCh();
                    }
                }
            }
            else if (Globals.Lexeme[0] == 32)
                GetNextCh();
        }
   
        static void ProcessComment()
        {
            int temp = Globals.LineNo;
            while (temp == Globals.LineNo)
                GetNextCh();
            Globals.Lexeme = "";
        }
        // Process legal multiline comment (/* & */)
        static void ProcessMultiComment()
        {
            ReadRestComment();
        }
        // Corresponds to finishing a word token, this will read in the rest of a word token from the cursor in the file
        static void ReadRest()
        {
            while(((Globals.ch > 47) & (Globals.ch < 58)) | ((Globals.ch > 64) & (Globals.ch < 91)) | ((Globals.ch > 96) & (Globals.ch < 123)) | (Globals.ch == 95) & (Globals.ch !=32) & (Globals.ch != 32))
            {
                if(Globals.Lexeme.Length > 32)
                {
                    Console.WriteLine("ERROR! IDENTIFIER LENGTH EXCEEDS MAXIMUM! PRESS ANY KEY TO EXIT!");
                    Globals.inputFile.Close();
                    Console.ReadKey();
                    System.Environment.Exit(1);
                    return;
                }
                Globals.Lexeme = Globals.Lexeme + Globals.ch;
                GetNextCh();
            }
        }
        static void ReadRestNum()
        {
            while (((Globals.ch > 47) & (Globals.ch < 58))) 
            {
                Globals.Lexeme = Globals.Lexeme + Globals.ch;
                GetNextCh();
            }
        }
        static void ReadRestComment()
        {
            GetNextCh();
            while (Globals.ch != 42 && !Globals.inputFile.EndOfStream)
            {
                GetNextCh();
            }
            GetNextCh();
            if (Globals.ch == 47)
            {
                Globals.Lexeme = "";
                Globals.ch = (char)0;
                return;
            }
                
            else if (Globals.inputFile.EndOfStream)
            {
                Console.Write("ERROR! BLOCK COMMENT NOT TERMINATED! PRESS ANY KEY TO EXIT!" + "\r\n");
                Globals.inputFile.Close();
                Console.ReadKey();
                System.Environment.Exit(1);
                return;
            }
            else
                ReadRestComment();
        }
        //simple function used to determine if a character is used legally in an identifier
        static bool isword(char check)
        {
            if (((check > 64) & (check < 91)) | ((check > 96) & (check < 123)))
                return true;
            else
                return false;
        }
        static bool isnum(char check)
        {
            if ((check > 47) & (check< 58))
                return true;
            else
                return false;
        }
        //logic to associate actual key words with tokens
        static void associateToken()
        {
            string lexeme_Luthor = Globals.Lexeme;

            if (lexeme_Luthor == "class")
                Globals.Token = keywords.classt;
            else if (lexeme_Luthor == "const")
                Globals.Token = keywords.constt;
            else if (lexeme_Luthor == "new")
                Globals.Token = keywords.vart;
            else if (lexeme_Luthor == "if")
                Globals.Token = keywords.ift;
            else if (lexeme_Luthor == "while")
                Globals.Token = keywords.whilet;
            else if (lexeme_Luthor == "else")
                Globals.Token = keywords.elset;
            else if (lexeme_Luthor == "System" && Globals.ch == '.')
            {
                Globals.Lexeme = Globals.ch.ToString();
                GetNextCh();
                ReadRest();
                lexeme_Luthor = lexeme_Luthor + Globals.Lexeme;
                if (lexeme_Luthor == "System.out" && Globals.ch == '.')
                {
                    Globals.Lexeme = Globals.ch.ToString();
                    GetNextCh();
                    ReadRest();
                    lexeme_Luthor = lexeme_Luthor + Globals.Lexeme;
                    if (lexeme_Luthor == "System.out.println")
                    {
                        Globals.Token = keywords.writet;
                        Globals.Lexeme = lexeme_Luthor;
                    }
                    else
                    {
                        Console.Write("Token - " + "idt" + " | Plain text - " + "System" + " | Line # - " + Globals.LineNo + "\r\n" + "\r\n");
                        Console.Write("Token - " + "periodt" + " | Plain text - " + "." + " | Line # - " + Globals.LineNo + "\r\n" + "\r\n");
                        Console.Write("Token - " + "idt" + " | Plain text - " + "out" + " | Line # - " + Globals.LineNo + "\r\n" + "\r\n");
                        Console.Write("Token - " + "periodt" + " | Plain text - " + "." + " | Line # - " + Globals.LineNo + "\r\n" + "\r\n");
                        Globals.Token = keywords.idt;
                        GetNextCh();
                    }
                }
                else
                {
                    Console.Write("Token - " + "idt" + " | Plain text - " + "System" + " | Line # - " + Globals.LineNo + "\r\n" + "\r\n");
                    Console.Write("Token - " + "periodt" + " | Plain text - " + "." + " | Line # - " + Globals.LineNo + "\r\n" + "\r\n");
                    Console.Write("Token - " + "idt" + " | Plain text - " + "out" + " | Line # - " + Globals.LineNo + "\r\n" + "\r\n");
                    Globals.Token = keywords.idt;
                    GetNextCh();
                }
            }
            else if (lexeme_Luthor == "true")
                Globals.Token = keywords.truet;
            else if (lexeme_Luthor == "false")
                Globals.Token = keywords.falset;
            else if (lexeme_Luthor == "int")
                Globals.Token = keywords.integert;
            else if (lexeme_Luthor == "float")
                Globals.Token = keywords.floatt;
            else if (lexeme_Luthor == "boolean")
                Globals.Token = keywords.boolt;
            else if (lexeme_Luthor == "String")
                Globals.Token = keywords.arrayt;
            else if (lexeme_Luthor == "extends")
                Globals.Token = keywords.extendst;
            else if (lexeme_Luthor == "void")
                Globals.Token = keywords.voidt;
            else if (lexeme_Luthor == "public")
                Globals.Token = keywords.publict;
            else if (lexeme_Luthor == "static")
                Globals.Token = keywords.statict;
            else if (lexeme_Luthor == "final")
                Globals.Token = keywords.finalt;
            else if (lexeme_Luthor == "main")
                Globals.Token = keywords.progt;
            else if (lexeme_Luthor == "return")
                Globals.Token = keywords.returnt;
            else if (lexeme_Luthor == "write")
                Globals.Token = keywords.writet;
            else if (lexeme_Luthor == "read")
                Globals.Token = keywords.readt;
            else if (lexeme_Luthor == "writeln")
                Globals.Token = keywords.writelnt;
            else
                Globals.Token = keywords.idt;
        }
    }   
}
