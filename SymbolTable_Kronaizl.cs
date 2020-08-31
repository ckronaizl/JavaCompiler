/*
File: SymbolTable_Kronaizl.cs
Author: Cody Kronaizl
Date: 3/6/2020
Class: CSC-446
Description:
This file details all procedures needed to build a simple symbol table.
This symbol table will be implemented in a simplified Java compiler,
but can theoretically be used to store necessary information for any language.
The hash function used to build the table, commonly referred to as "hashpjw",
is a hash function created by Peter Weinberger, and all credit goes to him
for its design, as well as its theoretical collision proofs.
*/

using System;
using System.Collections.Generic;
using LexicalAnalyzer;

namespace SymbolTable_Kronaizl
{
    //possible different entry types
    public enum RecordType
    { 
        variable_record = 1,
        constant_record = 2,
        method_record = 3,
        class_record = 4
    }
    //possible variable types for variable entries
    public enum VarType
    {
        type_int = 1,
        type_bool = 2,
        type_string = 3,
        type_void = 4
    }
    //possible forms of parameter passing
    public enum PassMode
    {
        reference = 1,
        value = 2
    }
    //globals needed for class
    public static class Globals
    {
        public const int TABLESIZE = 211;
        public static List<TableEntryInterface>[] table = new List<TableEntryInterface>[211];
    }
    //actual symbol table class
    public class SymbolTable_Kronaizl
    {
        //constructor must initialize the table, as it is comprised of an array of  
        //lists, each entry must be initialized
        public SymbolTable_Kronaizl()
        {
            for (int i = 0; i < Globals.table.Length; i++)
                Globals.table[i] = new List<TableEntryInterface>();
        }

        //"hashpjw" implemented in C#
        public int hash(string lexeme)
        {
            uint h = 0, g = 0;
            int temp;
            for (temp = 0; temp != lexeme.Length; temp++)
            {
                h = (h << 24) + ((byte)lexeme[temp]);
                g = h & 0xf0000000;
                if (g != 0)
                {
                    h ^= (g >> 24);
                    h ^= g;
                }
            }
            return (int)h % Globals.TABLESIZE;
        }

        //this funciton is used to insert entries into the symbol table
        public void insert(string lex, keywords token, int depth)
        {
            int location = hash(lex);
            TableEntryInterface newEntry = new SymbolTableEntry();
            newEntry.lexeme = lex;
            newEntry.token = token;
            newEntry.depth = depth;
            if (token == keywords.idt)
            {
                if (Globals.table[location].Count != 0)
                {
                    Globals.table[location].Insert(0, newEntry);
                }
                else
                {
                    Globals.table[location].Add(newEntry);
                }
            }
            else
            {
                Console.WriteLine("ERROR! SYMBOL TABLE ONLY ACCEPTS IDENTIFIER TOKENS!");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                System.Environment.Exit(1);
            }
        }

        //lookup the first entry with the lexeme "lex", this will be used to modify the values within the symbol table later.
        public int lookup(string lex)
        {
            int location, index;
            location = hash(lex);
            for (index = 0; index < Globals.table[location].Count; index++)
            {
                if (Globals.table[location][index].lexeme == lex)
                    return index;      //will make necessary changes to entry in wrapper then reinsert
            }
            return -1;    //Entry wasn't found, will return a null value which will be handled in wrapper class
        }

        //delete all entires at specified depth
        public void deleteDepth(int depth)
        {
            int index, parse;
            for (index = 0; index < 211; index++)
            {
                for (parse = 0; parse <= Globals.table[index].Count - 1; parse++)
                {
                    if (Globals.table[index][parse].depth == depth)
                    {
                        Globals.table[index].RemoveAt(parse);
                    }
                }
            }
        }

        //used for debugging, this method will print out all entries at a specified depth
        public void writeTable(int depth)
        {
            int index, parse;
            for (index = 0; index < 211; index++)
            {
                for (parse = 0; parse <= Globals.table[index].Count - 1; parse++)
                {
                    if (Globals.table[index][parse].depth == depth)
                    {
                        Console.WriteLine(Globals.table[index][parse].lexeme + " - lex | " + Globals.table[index][parse].type.ToString() + " - type | " + Globals.table[index][parse].depth + " - depth"); //print out all entries at defined depth
                    }
                }

            }
        }
    }
            
        

    //base table entry, this is what all items are inserted as, characteristics are populated after insertion
    public class SymbolTableEntry : TableEntryInterface
    {
        public string lexeme { get; set; }
        public int depth { get; set; }
        public keywords token { get; set; }
        public RecordType type { get; set; }
    }
    //interface for the base entry, each type of entry shares these characteristics, as each of these 
    //entry types need to know basic location values. Using an interface for this will make
    //adding the symbol table to the rest of the compiler much simpler.
    public interface TableEntryInterface
    {
        public string lexeme { get; set; }
        public int depth { get; set; }
        public keywords token { get; set; }
        public RecordType type { get; set; }
    }

    //Below are the different types of entries that can be entered into the symbol table.
    public class Variable: TableEntryInterface
    {
        public int size;
        public int offset;
        public VarType typeOfVariable;
        public string lexeme { get; set; }
        public int depth { get; set; }
        public keywords token { get; set; }
        public RecordType type { get; set; }
    }
    public class Constant : TableEntryInterface
    {
        public double value;
        public string lexeme { get; set; }
        public int depth { get; set; }
        public keywords token { get; set; }
        public RecordType type { get; set; }
    }
    public class Method : TableEntryInterface
    {
        public int sizeOfLocals;
        public int sizeOfParams;
        public int numberOfParams;
        public List<VarType> paramTypes = new List<VarType>();
        public List<PassMode> passingMode = new List<PassMode>();
        public string lexeme { get; set; }
        public int depth { get; set; }
        public keywords token { get; set; }
        public RecordType type { get; set; }
    }
    public class Class : TableEntryInterface
    {
        public int sizeOfLocals;
        public List<string> methodNames = new List<string>();
        public List<string> variableNames = new List<string>();
        public string lexeme { get; set; }
        public int depth { get; set; }
        public keywords token { get; set; }
        public RecordType type { get; set; }
    }
}