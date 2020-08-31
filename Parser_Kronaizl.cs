/*
File: Parser_Kronaizl.cs
Author: Cody Kronaizl
Date: 2/2/2020
Class: CSC-446
Description:
This file details all procedures needed to build a simplified Java Parser.
It is designed to be called directly by another class to allow for integration
into a simple Java compiler.


Updated: 4/1/2020
Description:
A multitude of changes were made to the compiler's parser in order to implement 
the addition of a subset java program's variables to a symbol table.


Updated: 4/3/2020
Description:
New Grammar rules were added, as the existing grammar rules were unable to execute
any statements. The grammar rules are still used for a subset of Java, and allows
for some small imperfections. These added rules will be 

Updated: 4/21/2020
Description:
Logic was implemented inside the grammar rules that generates three address code,
that of which will be processed into Intel x86 assembly in the future. Generated 
three address code is output to both the console window and a .TAC file in the
same parent directory as the passed in .java file.

 Updated 4/30/2020
 Description:
 More grammar rules were added to support I/O with the system console. Additional
 three address code generation was added to support I/O packages.
 */

using System;
using System.Collections.Generic;
using System.IO;
using LexicalAnalyzer;
using SymbolTable_Kronaizl;

namespace Parser
{
    //setup global variables, includes lexeme
    public static class Globals
    {
        public static LexicalAnalyzer.LexicalAnalyzer lexer_luthor;
        public static SymbolTable_Kronaizl.SymbolTable_Kronaizl symTable;
        public static List<string> incorrectTokens = new List<string>();
        public static List<Method> methodsInProg = new List<Method>();
        public static keywords currentType;
        public static int depth, location, yloc;
        public static bool isMethod = false;
        public static bool isConstant = false;
        public static bool isClass = false;
        public static bool assignMethod = false;
        public static List<int> offset = new List<int>();
        public static Variable tempVar = new Variable();
        public static Method tempMethod = new Method();
        public static Class tempClass = new Class();
        public static Constant tempConstant = new Constant();
        public static string tacCode;
        public static string outputFile;
        public static string ASMfile;
        public static string tempLex;
        public static string tmpptr;
        public static string lookptr;
        public static string idStr;
        public static string idptr;
        public static string Rsyn;
        public static string Tsyn;
        public static int tempOffset;
        public static int stringCount;
        public static List<stackEntry> stack = new List<stackEntry>();
    }
    class Parser_Kronaizl
    {
        //kick off the parser, give it path of file passed in
        public void init(string path)
        {
            Globals.outputFile = path.Remove(path.IndexOf('.')) + ".tac";
            Globals.depth = 1;

            using (StreamWriter file = new StreamWriter(@Globals.outputFile, false))
            {
                file.WriteLine("");
            }

            Globals.symTable = new SymbolTable_Kronaizl.SymbolTable_Kronaizl();
            Globals.lexer_luthor = new LexicalAnalyzer.LexicalAnalyzer(path);
            Globals.lexer_luthor.GetNextToken();
            Globals.offset.Add(0);
            Prog();
            if (LexicalAnalyzer.Globals.Token == keywords.eoft)
            {
                LexicalAnalyzer.Globals.inputFile.Close();
                Console.WriteLine("File successfully Parsed!");
            }
            else
                Console.WriteLine("ERROR! UNUSED TOKENS!");

            //translate the created TAC into assembly
            AssemblyTranslator_Kronaizl assemblybuild = new AssemblyTranslator_Kronaizl();
            assemblybuild.init();
        }
        //make sure token found is desired token, if not throw error and exit program
        public void match(LexicalAnalyzer.keywords desired)
        {

            if (LexicalAnalyzer.Globals.Token == desired)
            {
                //Console.WriteLine("Expected token " + desired.ToString() + " found");         //uncomment for verbose parsing

                //begin semantic actions
                switch (LexicalAnalyzer.Globals.Token)
                {
                    case keywords.idt:
                        {
                            //set fields of entry
                            if (Globals.isMethod)
                            {
                                checkForDuplicate(LexicalAnalyzer.Globals.Lexeme);
                                Globals.symTable.insert(LexicalAnalyzer.Globals.Lexeme, LexicalAnalyzer.Globals.Token, Globals.depth);
                                updateMethodEntry();
                            }
                            else if (Globals.isConstant)
                            {
                                checkForDuplicate(LexicalAnalyzer.Globals.Lexeme);
                                Globals.symTable.insert(LexicalAnalyzer.Globals.Lexeme, LexicalAnalyzer.Globals.Token, Globals.depth);
                                updateConstEntry();
                            }
                            else if (Globals.currentType == keywords.integert | Globals.currentType == keywords.boolt | Globals.currentType == keywords.arrayt)
                            {
                                checkForDuplicate(LexicalAnalyzer.Globals.Lexeme);
                                Globals.symTable.insert(LexicalAnalyzer.Globals.Lexeme, LexicalAnalyzer.Globals.Token, Globals.depth);
                                updateVarEntry();
                            }
                            else if (Globals.currentType == keywords.classt)
                            {
                                checkForDuplicate(LexicalAnalyzer.Globals.Lexeme);
                                Globals.symTable.insert(LexicalAnalyzer.Globals.Lexeme, LexicalAnalyzer.Globals.Token, Globals.depth);
                                updateClassEntry();
                            }
                            break;
                        }
                    //setup method
                    case keywords.progt:
                        {
                            checkForDuplicate(LexicalAnalyzer.Globals.Lexeme);
                            Globals.symTable.insert(LexicalAnalyzer.Globals.Lexeme, keywords.idt, Globals.depth);
                            //set fields of entry
                            updateMethodEntry();
                            break;
                        }
                    //begin setup for read tokens, grammar specific
                    case keywords.integert:
                        {
                            if (Globals.currentType == keywords.publict)
                                Globals.isMethod = true;
                            else if (Globals.currentType == keywords.finalt)
                                Globals.isConstant = true;
                            Globals.currentType = desired;
                            break;
                        }
                    case keywords.boolt:
                        {
                            if (Globals.currentType == keywords.publict)
                                Globals.isMethod = true;
                            else if (Globals.currentType == keywords.finalt)
                                Globals.isConstant = true;
                            Globals.currentType = desired;
                            break;
                        }
                    case keywords.voidt:
                        {
                            if (Globals.currentType == keywords.publict)
                                Globals.isMethod = true;
                            else if (Globals.currentType == keywords.finalt)
                                Globals.isConstant = true;
                            Globals.currentType = desired;
                            break;
                        }
                    case keywords.floatt:
                        {
                            if (Globals.currentType == keywords.publict)
                                Globals.isMethod = true;
                            else if (Globals.currentType == keywords.finalt)
                                Globals.isConstant = true;
                            Globals.currentType = desired;
                            break;
                        }
                    case keywords.numt:
                        {
                            if (Globals.isConstant)
                                finalizeConstEntry();
                            break;
                        }
                    case keywords.extendst:
                    case keywords.classt:
                    case keywords.publict:
                    case keywords.finalt:
                    case keywords.semit:
                        Globals.currentType = desired;
                        break;
                }
                //else for setting previous token type here
                Globals.lexer_luthor.GetNextToken();
            }
            else
            {
                Globals.incorrectTokens.Add(desired.ToString());
                getOut();
            }
        }
        //Fill in variable characteristics
        public void updateVarEntry()
        {
            int yloc = Globals.symTable.lookup(LexicalAnalyzer.Globals.Lexeme);
            int location = Globals.symTable.hash(LexicalAnalyzer.Globals.Lexeme);
            Variable tempVar = new Variable();
            tempVar.token = Globals.currentType;
            tempVar.lexeme = LexicalAnalyzer.Globals.Lexeme;



            tempVar.depth = Globals.depth;
            tempVar.type = RecordType.variable_record;
            //setup allocation size & offsets
            switch (Globals.currentType)
            {
                case keywords.boolt:
                    tempVar.size = 1;
                    tempVar.offset = Globals.offset[Globals.depth - 1];
                    Globals.offset[Globals.depth - 1] += 1;
                    tempVar.typeOfVariable = VarType.type_bool;
                    break;
                case keywords.integert:
                    tempVar.size = 2;
                    tempVar.offset = Globals.offset[Globals.depth - 1];
                    Globals.offset[Globals.depth - 1] += 2;
                    tempVar.typeOfVariable = VarType.type_int;
                    break;
                case keywords.arrayt:
                    tempVar.size = 2;
                    tempVar.offset = Globals.offset[Globals.depth - 1];
                    Globals.offset[Globals.depth - 1] += 2;
                    tempVar.typeOfVariable = VarType.type_string;
                    break;
                case keywords.voidt:
                    tempVar.size = 0;
                    tempVar.offset = Globals.offset[Globals.depth - 1];
                    Globals.offset[Globals.depth - 1] += 0;
                    tempVar.typeOfVariable = VarType.type_void;
                    break;
            }
            if (Globals.isClass)
            {
                Globals.tempClass.sizeOfLocals = Globals.offset[Globals.depth - 1];
                Globals.tempClass.variableNames.Add(tempVar.lexeme);
            }
            SymbolTable_Kronaizl.Globals.table[location][yloc] = tempVar;
        }

        //make sure lexeme does not exist at current depth
        public void checkForDuplicate(string symEntry)
        {
            int location = Globals.symTable.hash(symEntry);
            foreach (TableEntryInterface entry in SymbolTable_Kronaizl.Globals.table[location])
            {
                if (entry.depth == Globals.depth)
                {
                    Console.WriteLine("INVALID DECLARATION! TWO VARIABLES OF THE LEXEME " + symEntry + " EXIST AT THE SAME DEPTH ON LINE " + LexicalAnalyzer.Globals.LineNo);
                    Console.WriteLine("Press any key to exit");
                    Console.ReadKey();
                    System.Environment.Exit(1);
                }
            }
        }

        //fill in class characteristics
        public void updateClassEntry()
        {
            Globals.tempClass = new Class();
            Globals.isClass = true;
            int location = Globals.symTable.hash(LexicalAnalyzer.Globals.Lexeme);
            int yloc = Globals.symTable.lookup(LexicalAnalyzer.Globals.Lexeme);
            Globals.tempClass.token = LexicalAnalyzer.Globals.Token;
            Globals.tempClass.lexeme = LexicalAnalyzer.Globals.Lexeme;
            Globals.tempClass.depth = Globals.depth;
            Globals.tempClass.type = RecordType.class_record;

            SymbolTable_Kronaizl.Globals.table[location][yloc] = Globals.tempClass;
        }

        //enter details from class members
        public void finalizeClassEntry()
        {
            int location = Globals.symTable.hash(Globals.tempClass.lexeme);
            int yloc = Globals.symTable.lookup(Globals.tempClass.lexeme);

            SymbolTable_Kronaizl.Globals.table[location][yloc] = Globals.tempClass;
        }

        //create method entry with known details
        public void updateMethodEntry()
        {
            Globals.isMethod = false;
            Globals.tempMethod = new Method();
            int location = Globals.symTable.hash(LexicalAnalyzer.Globals.Lexeme);
            int yloc = Globals.symTable.lookup(LexicalAnalyzer.Globals.Lexeme);
            Globals.tempMethod.token = LexicalAnalyzer.Globals.Token;
            Globals.tempMethod.lexeme = LexicalAnalyzer.Globals.Lexeme;
            Globals.tempClass.methodNames.Add(LexicalAnalyzer.Globals.Lexeme);
            Globals.tempMethod.depth = Globals.depth;
            Globals.tempMethod.type = RecordType.method_record;
            Globals.tempMethod.numberOfParams = 0;
            Globals.tempMethod.sizeOfLocals = 0;
            Globals.tempMethod.paramTypes.Clear();
            Globals.tempMethod.passingMode.Clear();

            SymbolTable_Kronaizl.Globals.table[location][yloc] = Globals.tempMethod;

        }
        //populate dynamic details
        public void finalizeMethodEntry()
        {
            int location = Globals.symTable.hash(Globals.tempMethod.lexeme);
            int yloc = Globals.symTable.lookup(Globals.tempMethod.lexeme);

            Globals.tempMethod.sizeOfLocals = Globals.offset[Globals.depth - 1] + Globals.tempOffset;
            Globals.tempMethod.numberOfParams = Globals.tempMethod.paramTypes.Count;

            SymbolTable_Kronaizl.Globals.table[location][yloc] = Globals.tempMethod;
            Globals.methodsInProg.Add(Globals.tempMethod);
        }
        //create constant details
        public void updateConstEntry()
        {
            switch (Globals.currentType)
            {
                case keywords.boolt:
                    Globals.offset[Globals.depth - 1] += 1;
                    break;
                case keywords.integert:
                    Globals.offset[Globals.depth - 1] += 2;
                    break;
            }

            int temp = Globals.symTable.hash(LexicalAnalyzer.Globals.Lexeme);
            int yloc = Globals.symTable.lookup(LexicalAnalyzer.Globals.Lexeme);
            Globals.tempConstant = new Constant();
            Globals.tempConstant.token = Globals.currentType;
            Globals.tempConstant.lexeme = LexicalAnalyzer.Globals.Lexeme;
            Globals.tempConstant.depth = Globals.depth;
            Globals.tempConstant.type = RecordType.constant_record;
            SymbolTable_Kronaizl.Globals.table[temp][yloc] = Globals.tempConstant;
        }
        public void finalizeConstEntry()
        {
            Globals.isConstant = false;
            if (Globals.tempConstant.token == keywords.floatt)
                Globals.tempConstant.value = LexicalAnalyzer.Globals.ValueR[LexicalAnalyzer.Globals.ValueR.Count - 1];
            else
                Globals.tempConstant.value = LexicalAnalyzer.Globals.Value[LexicalAnalyzer.Globals.Value.Count - 1];
        }

        //increase current depth & offset
        public void increaseDepth()
        {
            Globals.depth++;
            Globals.offset.Add(0);
        }

        //decrease depth, output table if desired
        public void decreaseDepth()
        {
            //Globals.symTable.writeTable(Globals.depth);   //uncomment for verbose semantics
            Globals.symTable.deleteDepth(Globals.depth);
            Globals.offset.RemoveAt(Globals.depth - 1);
            Globals.depth--;
        }

        //beginning of context free grammar definitions

        //Prog			->	MoreClasses MainClass
        public void Prog()
        {
            MoreClasses();
            MainClass();
            decreaseDepth();
        }

        //MoreClasses		->	ClassDecl MoreClasses | (empty)
        public void MoreClasses()
        {
            if (LexicalAnalyzer.Globals.Token == keywords.classt)
            {
                ClassDecl();
                MoreClasses();
            }
        }

        //MainClass		->	finalt classt idt { publict statict voidt main (String [] idt) 
        //{
        //                                                      SeqofStats
        //        }
        //                                                }
        public void MainClass()
        {
            match(keywords.finalt);
            match(keywords.classt);
            match(keywords.idt);
            match(keywords.begint);
            increaseDepth();
            match(keywords.publict);
            match(keywords.statict);
            match(keywords.voidt);
            match(keywords.progt);
            match(keywords.lparent);
            increaseDepth();
            Globals.tacCode = "Proc main";
            emit(Globals.tacCode);
            Globals.tacCode = "";
            match(keywords.arrayt);
            match(keywords.lbracet);
            match(keywords.rbracet);
            match(keywords.idt);
            match(keywords.rparent);
            match(keywords.begint);
            SeqOfStats();
            Globals.tacCode = "Endp main";
            emit(Globals.tacCode);
            Globals.tacCode = "";
            match(keywords.endt);
            match(keywords.endt);
            decreaseDepth();
            decreaseDepth();
        }

        //ClassDecl		->	class idt { VarDecl MethodDecl } | class idt extendst idt { VarDecl MethodDecl }
        public void ClassDecl()
        {
            match(keywords.classt);
            match(keywords.idt);

            if (LexicalAnalyzer.Globals.Token == keywords.extendst)
            {
                match(keywords.extendst);
                match(keywords.idt);
            }
            match(keywords.begint);
            increaseDepth();
            VarDecl();
            Globals.isClass = false;
            MethodDecl();
            match(keywords.endt);
            decreaseDepth();
            finalizeClassEntry();
        }

        //VarDecl		-> 	Type IdentifierList; VarDecl | finalt Type idt = numt; VarDecl | (empty)
        public void VarDecl()
        {
            string tempLex = LexicalAnalyzer.Globals.Lexeme;
            //Type case
            if (LexicalAnalyzer.Globals.Token == keywords.integert | LexicalAnalyzer.Globals.Token == keywords.boolt | LexicalAnalyzer.Globals.Token == keywords.voidt)
            {
                Type();
                IdentifierList();
                match(keywords.semit);
                VarDecl();
            }
            else if (LexicalAnalyzer.Globals.Token == keywords.finalt)
            {
                match(keywords.finalt);
                Type();
                match(keywords.idt);
                Globals.stack.Add(new stackEntry(0 - Globals.offset[Globals.offset.Count - 1], tempLex));
                match(keywords.assignopt);
                match(keywords.numt);
                match(keywords.semit);
                VarDecl();
            }
        }

        //IdentifierList		->	idt | IdentifierList , idt
        public void IdentifierList()
        {
            string tempLex = LexicalAnalyzer.Globals.Lexeme;
            match(keywords.idt);
            Globals.stack.Add(new stackEntry(0 - Globals.offset[Globals.offset.Count - 1], tempLex)); //verify offset change
            if (LexicalAnalyzer.Globals.Token == keywords.commat)
            {
                match(keywords.commat);
                IdentifierList();
            }
        }

        //MethodDecl		->	publict Type idt(FormalList) { VarDecl SeqOfStats returnt Expr; } MethodDecl | (empty)
        public void MethodDecl()
        {
            string retVal = "";
            if (LexicalAnalyzer.Globals.Token == keywords.publict)
            {

                match(keywords.publict);
                Type();
                match(keywords.idt);
                match(keywords.lparent);
                increaseDepth();
                FormalList();
                match(keywords.rparent);
                Globals.tempMethod.sizeOfParams = Globals.offset[Globals.depth - 1];
                Globals.offset[Globals.depth - 1] = 0;
                match(keywords.begint);
                VarDecl();
                Globals.tacCode = "Proc " + Globals.tempMethod.lexeme;
                emit(Globals.tacCode);
                Globals.tacCode = "";
                SeqOfStats();
                match(keywords.returnt);
                Expr(ref retVal);
                if (retVal != "")
                    emit("_ax=" + retVal);
                else
                    emit("_ax= 0");
                match(keywords.semit);
                match(keywords.endt);
                Globals.tacCode = "Endp " + Globals.tempMethod.lexeme;
                emit(Globals.tacCode);
                Globals.tacCode = "";
                finalizeMethodEntry();
                decreaseDepth();
                Globals.stack.Clear();
                Globals.tempOffset = 0;
                Globals.stack.Add(new stackEntry(0, "Return Address"));
                MethodDecl();
                Globals.stack.Clear();
                Globals.stack.Add(new stackEntry(0, "Return Address"));
            }
        }

        //SeqOfStatments	->	Statement  ; StatTail | (empty)
        public void SeqOfStats()
        {
            if (LexicalAnalyzer.Globals.Token == keywords.idt || LexicalAnalyzer.Globals.Token == keywords.writet || LexicalAnalyzer.Globals.Token == keywords.readt || LexicalAnalyzer.Globals.Token == keywords.writelnt)
            {
                Statement();
                match(keywords.semit);
                StatTail();
            }
        }
        //StatTail ->	Statement  ; StatTail | (empty)
        public void StatTail()
        {
            if (LexicalAnalyzer.Globals.Token == keywords.idt || LexicalAnalyzer.Globals.Token == keywords.writet || LexicalAnalyzer.Globals.Token == keywords.readt || LexicalAnalyzer.Globals.Token == keywords.writelnt)
            {
                Statement();
                match(keywords.semit);
                StatTail();
            }
        }
        //Statement		-> 	AssignStat	| IOStat
        public void Statement()
        {
            if (LexicalAnalyzer.Globals.Token == keywords.idt)
            {
                AssignStat();
            }
            else
            {
                IOStat();
            }

        }
        //AssignStat		->	idt  =  Expr | idt = MethodCall | MethodCall
        public void AssignStat()
        {
            string locSyn = "";
            searchTable();
            if (Globals.location == -1)
            {
                Console.WriteLine("ERROR ON LINE " + LexicalAnalyzer.Globals.LineNo + "! UNDECLARED VARIABLE USED!");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                Environment.Exit(1);
            }

            Globals.tempLex = LexicalAnalyzer.Globals.Lexeme;

            getTempVar();
            Globals.idptr = Globals.lookptr;
            //if identifer is a variable, use expression rule, if method use the method call assignment, otherwise use method call
            match(keywords.idt);

            switch (SymbolTable_Kronaizl.Globals.table[Globals.yloc][Globals.location].type)
            {
                case RecordType.variable_record:
                    match(keywords.assignopt);
                    Expr(ref locSyn);
                    if(Globals.tmpptr!=null)
                    {
                        Globals.tacCode = Globals.idptr + "=" + Globals.tmpptr;
                        emit(Globals.tacCode);
                        Globals.tacCode = "";
                    }
                    else
                    {
                        Globals.tacCode = Globals.idptr + "=" + Globals.lookptr;
                        emit(Globals.tacCode);
                        Globals.tacCode = "";
                    }
                    break;
                case RecordType.method_record:
                    match(keywords.assignopt);
                    match(keywords.idt);
                    Globals.tacCode = Globals.tempLex + "= _AX\n";
                    MethodCall();
                    break;
                case RecordType.constant_record:
                    break;
                default:
                    MethodCall();
                    break;
            }

        }

        //MethodCall -> ClassName.idt (Params)
        public void MethodCall()
        {

            match(keywords.periodt);
            Globals.tacCode = "call " + LexicalAnalyzer.Globals.Lexeme;// + "\n" + Globals.tacCode;
            match(keywords.idt);
            match(keywords.lparent);
            Params();
            match(keywords.rparent);
            emit(Globals.tacCode);
            Globals.tacCode = "";
            Globals.tmpptr = "_ax";
        }

        //Params -> idt ParamsTail | numt ParamsTail | empty
        public void Params()
        {
            if (LexicalAnalyzer.Globals.Token == keywords.idt)
            {
                getTempVar();
                Globals.tacCode = "push " + Globals.lookptr + "\n" + Globals.tacCode;
                match(keywords.idt);
                ParamsTail();
            }
            else if (LexicalAnalyzer.Globals.Token == keywords.numt)
            {
                getTempVar();
                Globals.tacCode = "push " + Globals.lookptr + "\n" + Globals.tacCode;
                match(keywords.numt);
                ParamsTail();
            }

        }
        // ParamsTail -> , idt ParamsTail | , numt ParamsTail | empty
        public void ParamsTail()
        {
            if (LexicalAnalyzer.Globals.Token == keywords.commat)
            {
                match(keywords.commat);
                getTempVar();
                Globals.tacCode = "push " + Globals.lookptr + "\n" + Globals.tacCode;
                if (LexicalAnalyzer.Globals.Token == keywords.idt)
                {
                    match(keywords.idt);
                }
                else
                {
                    match(keywords.numt);
                }
                ParamsTail();
            }
        }

        //IOStat ->	In_Stat | Out_Stat
        public void IOStat()
        {
            if (LexicalAnalyzer.Globals.Token == keywords.readt)
            {
                In_Stat();
            }
            else
                Out_Stat();
        }
        public void In_Stat()
        {
            match(keywords.readt);
            match(keywords.lparent);
            Id_List();
            match(keywords.rparent);
        }
        public void Out_Stat()
        {
            if (LexicalAnalyzer.Globals.Token == keywords.writet)
            {
                match(keywords.writet);
                match(keywords.lparent);
                Write_List();
                match(keywords.rparent);
            }
            else
            {
                match(keywords.writelnt);
                match(keywords.lparent);
                Write_List();
                emit("wrln");
                match(keywords.rparent);
            }
        }
        public void Write_List()
        {
            Write_Token();
            Write_List_Tail();
        }
        public void Write_List_Tail()
        {
            if (LexicalAnalyzer.Globals.Token == keywords.commat)
            {
                match(keywords.commat);
                Write_Token();
                Write_List_Tail();
            }
        }
        public void Write_Token()
        {
            Variable checkVar;
            switch(LexicalAnalyzer.Globals.Token)
            {
                case keywords.idt:
                    searchTable();
                    getTempVar();
                    if(SymbolTable_Kronaizl.Globals.table[Globals.yloc][Globals.location].type == RecordType.variable_record)
                    {
                        checkVar = (Variable)SymbolTable_Kronaizl.Globals.table[Globals.yloc][Globals.location];
                        switch (checkVar.typeOfVariable)
                        {
                            case VarType.type_int:
                                emit("wri " + Globals.lookptr.ToString());
                                break;
                            case VarType.type_string:
                                emit("wrs " + Globals.lookptr.ToString());
                                break;
                        }
                    }
                    match(keywords.idt);
                    break;
                case keywords.numt:
                    match(keywords.numt);
                    break;
                case keywords.realt: //replace with literal code
                    emit("wrs S" + Globals.stringCount + " " + LexicalAnalyzer.Globals.Lexeme);
                    Globals.stringCount++;
                    match(keywords.realt);
                    break;

            }
        }
        public void Id_List()
        {
            Variable checkVar;
            searchTable();
            getTempVar();
            if (SymbolTable_Kronaizl.Globals.table[Globals.yloc][Globals.location].type == RecordType.variable_record)
            {
                checkVar = (Variable)SymbolTable_Kronaizl.Globals.table[Globals.yloc][Globals.location];
                switch (checkVar.typeOfVariable)
                {
                    case VarType.type_int:
                        emit("rdi " + Globals.lookptr.ToString());
                        break;
                    case VarType.type_string:
                        emit("rds " + Globals.lookptr.ToString());
                        break;
                }
            }
            match(keywords.idt);
            Id_List_Tail();
        }
        public void Id_List_Tail()
        {
            Variable checkVar;
            if (LexicalAnalyzer.Globals.Token==keywords.commat)
            {
                match(keywords.commat);
                searchTable();
                getTempVar();
                if (SymbolTable_Kronaizl.Globals.table[Globals.yloc][Globals.location].type == RecordType.variable_record)
                {
                    checkVar = (Variable)SymbolTable_Kronaizl.Globals.table[Globals.yloc][Globals.location];
                    switch (checkVar.typeOfVariable)
                    {
                        case VarType.type_int:
                            emit("rdi " + Globals.lookptr.ToString());
                            break;
                        case VarType.type_string:
                            emit("rds " + Globals.lookptr.ToString());
                            break;
                    }
                }
                match(keywords.idt);
                Id_List_Tail();
            }
        }
        //Expr -> Relation | (empty)
        public void Expr(ref string exprSyn)
        {
            if(LexicalAnalyzer.Globals.Token==keywords.idt || LexicalAnalyzer.Globals.Token == keywords.numt|| LexicalAnalyzer.Globals.Token == keywords.lparent || LexicalAnalyzer.Globals.Token == keywords.truet||LexicalAnalyzer.Globals.Token == keywords.falset || (LexicalAnalyzer.Globals.Token == keywords.relopt && LexicalAnalyzer.Globals.Lexeme=="!") || (LexicalAnalyzer.Globals.Token == keywords.addopt && LexicalAnalyzer.Globals.Lexeme == "-"))
                Relation(ref exprSyn);
        }
        //Relation	->	SimpleExpr
        public void Relation(ref string relSyn)
        {
            SimpleExpr(ref relSyn);
        }
        //SimpleExpr ->	Term MoreTerm
        public void SimpleExpr(ref string Esyn)
        {
            Term(ref Esyn);
            MoreTerm(ref Esyn);
        }
        //Term	->	Factor  MoreFactor
        public void Term(ref string Esyn)
        {
            Factor(ref Esyn);
            MoreFactor(ref Esyn);
        }
        //MoreTerm	->	Addop Term MoreTerm | (empty)
        public void MoreTerm(ref string termSyn)
        {
            string code = "";
            string locTmpptr = "";
            if(LexicalAnalyzer.Globals.Token==keywords.addopt)
            {
                generateTemp(); //grab a new temp variable
                locTmpptr = Globals.tmpptr;
                code = locTmpptr;
                code += "=" + termSyn;
                code += LexicalAnalyzer.Globals.Lexeme; //addop
                Addop();
                Term(ref termSyn);
                code += termSyn;
                //emit();
                termSyn = locTmpptr;
                MoreTerm(ref termSyn);
                emit(code);
                Globals.tmpptr = locTmpptr;
            }
        }
        //MoreFactor -> Mulop Factor MoreFactor | (empty)
        public void MoreFactor(ref string Rsyn)
        {
            string code = "";
            string locTmpptr = "";
            if(LexicalAnalyzer.Globals.Token==keywords.mulopt)
            {
                generateTemp(); //grab a new temp variable
                locTmpptr = Globals.tmpptr;
                code = locTmpptr;
                code += "=" + Rsyn;
                code += LexicalAnalyzer.Globals.Lexeme;
                Mulop();
                Factor(ref Rsyn);
                code += Rsyn;

                MoreFactor(ref locTmpptr);
                emit(code);
                Rsyn = locTmpptr;
                Globals.tmpptr = locTmpptr;
            }
            //Rsyn = locTmpptr;
        }
        //Factor ->	idt | numt | (Expr ) | ! Factor | SignOp Factor | true | false
        public void Factor(ref string facSyn)
        {
            searchTable();
            switch (LexicalAnalyzer.Globals.Token)
            {

                case keywords.idt:
                    if (Globals.symTable.lookup(LexicalAnalyzer.Globals.Lexeme) == -1)
                    {
                        Console.WriteLine("ERROR ON LINE " + LexicalAnalyzer.Globals.LineNo + "! UNDECLARED VARIABLE USED!");
                        Console.WriteLine("Press any key to exit");
                        Console.ReadKey();
                        Environment.Exit(1);
                    }

                    getTempVar();
                    match(keywords.idt);
                    if (LexicalAnalyzer.Globals.Token != keywords.periodt)
                    {
                        //Console.WriteLine( " Here lies #1");
                        
                        facSyn = Globals.lookptr;
                        Globals.assignMethod = false;
                    }
                    else
                    {
                        MethodCall();
                        Globals.assignMethod = true;
                    }
                    

                    break;
                case keywords.numt:
                    generateTemp();
                    Globals.tacCode = Globals.tmpptr + "=" + LexicalAnalyzer.Globals.Lexeme;
                    facSyn = Globals.tmpptr;
                    emit(Globals.tacCode);
                    match(keywords.numt);
                    break;
                case keywords.lparent:
                    {
                        match(keywords.lparent);
                        Expr(ref facSyn);
                        match(keywords.rparent);
                    }
                    break;
                case keywords.relopt:
                    if (LexicalAnalyzer.Globals.Lexeme == "!")
                    {
                        match(keywords.relopt);
                        Factor(ref facSyn);
                    }
                    else
                    {
                        Globals.incorrectTokens.Add("Relational operator ! ");
                        getOut();
                    }
                    break;
                case keywords.addopt:
                    if (LexicalAnalyzer.Globals.Lexeme == "-")
                    {
                        SignOp();
                        generateTemp();
                        
                        Factor(ref facSyn);
                        emit(Globals.tmpptr + "=0-" + facSyn);
                        facSyn = Globals.tmpptr;
                    }
                    else
                    {
                        Globals.incorrectTokens.Add("sign operator - ");
                        getOut();
                    }
                    break;
                case keywords.truet:
                    match(keywords.truet);
                    break;
                case keywords.falset:
                    match(keywords.falset);
                    break;
                default:
                    Globals.incorrectTokens.Add("Proper Factor Grammar conditions not met!");
                    getOut();
                    break;
            }
        }
        //Addop	->	+ | - | ||
        public void Addop()
        {
            match(keywords.addopt);
        }
        //Mulop	-> 	* | / | &&
        public void Mulop()
        {
            match(keywords.mulopt);
        }
        //SignOp ->	-
        public void SignOp()
        {
            match(keywords.addopt);
        }
        //Type			->	intt | booleant |voidt 
        public void Type()
        {
            if (LexicalAnalyzer.Globals.Token == keywords.integert)
            {
                match(keywords.integert);
            }
            else if (LexicalAnalyzer.Globals.Token == keywords.boolt)
            {
                match(keywords.boolt);
            }
            else if (LexicalAnalyzer.Globals.Token == keywords.floatt)
            {
                match(keywords.floatt);
            }
            else if (LexicalAnalyzer.Globals.Token == keywords.voidt)
            {
                match(keywords.voidt);
            }
            else
            {
                Console.WriteLine("Error -  expecting int, float, or char");
                Globals.incorrectTokens.Add("int, float, bool, or char");
                getOut();
            }
        }

        //FormalList		->	Type idt FormalRest | (empty)
        public void FormalList()
        {
            if(LexicalAnalyzer.Globals.Token == keywords.integert | LexicalAnalyzer.Globals.Token == keywords.boolt | LexicalAnalyzer.Globals.Token == keywords.voidt)
            {
                Type();
                switch (Globals.currentType)
                {
                    case keywords.integert:
                        Globals.tempMethod.paramTypes.Add(VarType.type_int);
                        Globals.tempMethod.passingMode.Add(PassMode.value);
                        Globals.stack.Add(new stackEntry(Globals.offset[Globals.offset.Count-1] + 2, LexicalAnalyzer.Globals.Lexeme));
                        break;
                    case keywords.boolt:
                        Globals.tempMethod.paramTypes.Add(VarType.type_bool);
                        Globals.tempMethod.passingMode.Add(PassMode.value);
                        Globals.stack.Add(new stackEntry(Globals.offset[Globals.offset.Count - 1] + 2, LexicalAnalyzer.Globals.Lexeme));
                        break;
                }
                match(keywords.idt);
                Globals.tempMethod.numberOfParams++;
                FormalRest();
            }
        }

        //FormalRest		->	, Type idt FormalRest | (empty)
        public void FormalRest()
        {
            if(LexicalAnalyzer.Globals.Token == keywords.commat) 
            {
                match(keywords.commat);
                Type();
                switch(Globals.currentType)
                {
                    case keywords.integert:
                        Globals.tempMethod.paramTypes.Add(VarType.type_int);
                        Globals.tempMethod.passingMode.Add(PassMode.value);
                        Globals.stack.Add(new stackEntry(Globals.offset[Globals.offset.Count - 1] + 2, LexicalAnalyzer.Globals.Lexeme));
                        break;
                    case keywords.boolt:
                        Globals.tempMethod.paramTypes.Add(VarType.type_bool);
                        Globals.tempMethod.passingMode.Add(PassMode.value);
                        Globals.stack.Add(new stackEntry(Globals.offset[Globals.offset.Count - 1] + 2, LexicalAnalyzer.Globals.Lexeme));
                        break;
                }
                
                match(keywords.idt);
                Globals.tempMethod.numberOfParams++;
                FormalRest();
            }
            
        }

        public void emit(string codeToEmit)
        {
            using (StreamWriter file = new StreamWriter(@Globals.outputFile,true))
            {
                file.WriteLine(codeToEmit);
            }
            //Console.WriteLine(codeToEmit);   //uncomment for verbose TAC generation
        }

        //function used to exit program, reports expected token, and what was found
        public void getOut()
        {
            int x = 0;
            Console.Write("Error on line " + LexicalAnalyzer.Globals.LineNo + "! Incorrect Token found! Expecting ");
            for(x=0;x<Globals.incorrectTokens.Count;x++)
            {
                Console.Write(Globals.incorrectTokens[x] + " | \n");
            }
            Console.WriteLine(" But " + LexicalAnalyzer.Globals.Token.ToString() + " was found!");
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            System.Environment.Exit(1);
        }
        public void searchTable()
        {
            Globals.yloc = Globals.symTable.hash(LexicalAnalyzer.Globals.Lexeme);
            Globals.location = Globals.symTable.lookup(LexicalAnalyzer.Globals.Lexeme);
        }
        public void getTempVar()
        {
            bool found = false;
            foreach (stackEntry stk in Globals.stack)
            {
                if (stk.lexeme == LexicalAnalyzer.Globals.Lexeme) //checks for method parameters
                {
                    found = true;
                    if(stk.offset >= 0)
                        Globals.lookptr = "_bp+" + (stk.offset + 2).ToString();
                    else
                        Globals.lookptr = "_bp" + (stk.offset).ToString();

                    if(SymbolTable_Kronaizl.Globals.table[Globals.yloc][Globals.location].type == RecordType.constant_record)
                    {
                        Globals.tempConstant = (Constant)SymbolTable_Kronaizl.Globals.table[Globals.yloc][Globals.location];
                        Globals.lookptr = Globals.tempConstant.value.ToString();
                    }
                }
            }
            if (!found && SymbolTable_Kronaizl.Globals.table[Globals.yloc][Globals.location].type == RecordType.variable_record)
            {
                Globals.tempVar = (Variable)SymbolTable_Kronaizl.Globals.table[Globals.yloc][Globals.location];
                Globals.lookptr = "_bp" + (0 - Globals.tempVar.offset - 2);
                Globals.stack.Add(new stackEntry(0 - Globals.tempVar.offset - 2, Globals.tempVar.lexeme));
            }
            else if(!found && SymbolTable_Kronaizl.Globals.table[Globals.yloc][Globals.location].type == RecordType.constant_record)
            {
                Globals.tempConstant = (Constant)SymbolTable_Kronaizl.Globals.table[Globals.yloc][Globals.location];
                Globals.stack.Add(new stackEntry(0-Globals.offset[Globals.offset.Count - 1], Globals.tempConstant.lexeme));
                Globals.lookptr = Globals.tempConstant.value.ToString();
            }
        }
        public void generateTemp()
        {
            Globals.tmpptr = "_bp" + (0 - Globals.offset[Globals.offset.Count-1] - Globals.tempOffset - 2);
            Globals.tempOffset += 2;
        }
    }

    public class stackEntry
    {
        public string lexeme;
        public int offset;
        public stackEntry(int off, string lex)
        {
            offset = off;
            lexeme = lex;
        }
    }
}
