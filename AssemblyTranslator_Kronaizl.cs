using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Parser
{
    class AssemblyTranslator_Kronaizl
    {
        public void init()
        {
            bool inMethod = false;
            string methodName = "";
            string spaces = "        ";
            string tempStr;    //temporary bp reference
            int equalMarker;    //where is the equal sign
            int opMarker;       //where is the operator
            string lvar;   //used for operator statements
            string rvar;
            Console.WriteLine("Beginning Three Address Code translation to assembly of file " + Globals.outputFile);
            List<string> tacCode = new List<string>(File.ReadAllLines(Globals.outputFile));
            List<string> asmCode = new List<string>();
            List<string> strLiterals = new List<string>();
            FileStream tacFile = File.OpenRead(Globals.outputFile);

            Globals.ASMfile = Globals.outputFile.Remove(Globals.outputFile.IndexOf('.')) + ".asm";
            using (StreamWriter file = new StreamWriter(@Globals.ASMfile, false))
            {
                //file.WriteLine("");
            }

            asmCode.Add("start   PROC");
            asmCode.Add(spaces + "mov ax, @data");
            asmCode.Add(spaces + "mov ds, ax");
            asmCode.Add(spaces + "call main");
            asmCode.Add(spaces + "mov ah, 4ch");
            asmCode.Add(spaces + "mov al,0");
            asmCode.Add(spaces + "int 21h");
            asmCode.Add("start ENDP");
            asmCode.Add("");

            foreach(string line in tacCode)
            {
                if(line.Contains("wrs"))
                {
                    strLiterals.Add(line.Substring(4));
                    //Console.WriteLine(line); //uncomment for literal discovery
                    asmCode.Add(spaces + "mov dx, offset " + line.Substring(4,3).Trim());
                    asmCode.Add(spaces + "call writestr");
                }
                else if (line.Contains("wri"))
                {
                    tempStr = line.Substring(5);
                    asmCode.Add(spaces + "mov dx, [" + tempStr + "]");
                    asmCode.Add(spaces + "call writeint");
                }
                else if (line.Contains("rdi"))
                {
                    tempStr = line.Substring(5);
                    asmCode.Add(spaces + "call readint");
                    asmCode.Add(spaces + "mov [" + tempStr + "], bx");
                }

                else if (line.Contains("push"))
                {
                    tempStr = line.Substring(6);
                    asmCode.Add(spaces + "mov ax, [" + tempStr + "]");
                    asmCode.Add(spaces + "push ax");
                }
                else if(line.Contains("Proc") && !line.Contains("main"))
                {
                    inMethod = true;
                    methodName = line.Substring(5);
                    foreach(SymbolTable_Kronaizl.Method mtd in Globals.methodsInProg)
                    {
                        if (mtd.lexeme == methodName)
                            Globals.tempMethod = mtd;
                    }
                    asmCode.Add(methodName.PadRight(17) + "PROC");
                    asmCode.Add(spaces + "push bp");
                    asmCode.Add(spaces + "mov bp, sp");
                    asmCode.Add(spaces + "sub sp, " + Globals.tempMethod.sizeOfLocals);
                }
                else if(line.Contains("Proc") && line.Contains("main"))
                {
                    methodName = line.Substring(5);
                    asmCode.Add(methodName + " PROC");
                }
                else if(line.Contains("Endp") && !line.Contains("main"))
                {
                    inMethod = false;
                    asmCode.Add(spaces + "add sp, " + Globals.tempMethod.sizeOfLocals);
                    asmCode.Add(spaces + "pop bp");
                    asmCode.Add(spaces + "ret " + Globals.tempMethod.sizeOfParams);
                    asmCode.Add(methodName.PadRight(17) + "endp");
                }
                else if (line.Contains("Endp") && line.Contains("main"))
                {
                    inMethod = false;
                    asmCode.Add(spaces + "ret");
                    asmCode.Add(methodName + " endp");
                }
                else if(line.Contains("call"))
                {
                    asmCode.Add(spaces + line);
                }
                else if (line.Contains("wrln"))
                {
                    asmCode.Add(spaces + "call writeln");
                }
                else if (line.Contains("=_ax"))
                {
                    equalMarker = line.IndexOf("=");
                    tempStr = line.Substring(1,equalMarker-1);
                    asmCode.Add(spaces + "mov [" + tempStr + "], ax");
                }
                else if (line.Contains("_ax"))
                {
                    equalMarker = line.IndexOf("=");
                    if (line.Substring(equalMarker + 1).Contains("_"))
                        asmCode.Add(spaces + "mov ax, [" + line.Substring(equalMarker + 2) + "]");
                    else
                        asmCode.Add(spaces + "mov ax, " + line.Substring(equalMarker + 1));
                }
                else if (line.Contains("="))
                {
                    equalMarker = line.IndexOf("=");
                    tempStr = line.Substring(1, equalMarker - 1);
                    if(line.Contains("+_"))
                    {
                        opMarker = line.IndexOf("+");
                        lvar = line.Substring(equalMarker+2,(opMarker-equalMarker-2));
                        rvar = line.Substring(line.IndexOf("+")+2);
                        asmCode.Add(spaces + "mov ax, [" + lvar + "]");
                        asmCode.Add(spaces + "add ax, [" + rvar + "]");
                        asmCode.Add(spaces + "mov [" + tempStr + "], ax");
                    }
                    else if(line.Contains("*"))
                    {
                        opMarker = line.IndexOf("*");
                        lvar = line.Substring(equalMarker + 2, (opMarker - equalMarker-2));
                        rvar = line.Substring(line.IndexOf("*") + 2);
                        asmCode.Add(spaces + "mov ax, [" + lvar + "]");
                        asmCode.Add(spaces + "mov bx, [" + rvar + "]");
                        asmCode.Add(spaces + "imul bx");
                        asmCode.Add(spaces + "mov [" + tempStr + "], ax");
                    }
                    else if (line.Contains("-_"))
                    {
                        opMarker = line.IndexOf("-_");
                        lvar = line.Substring(equalMarker + 2, (opMarker - equalMarker - 1));
                        rvar = line.Substring(line.IndexOf("-_") + 1);
                        asmCode.Add(spaces + "mov ax, [" + lvar + "]");
                        asmCode.Add(spaces + "sub ax, [" + rvar + "]");
                        asmCode.Add(spaces + "mov [" + tempStr + "], ax");
                    }
                    else
                    {
                        if(line.Substring(equalMarker+1).Contains("_"))
                            asmCode.Add(spaces + "mov ax, [" + line.Substring(equalMarker + 2) + "]");
                        else
                            asmCode.Add(spaces + "mov ax, " + line.Substring(equalMarker + 1));

                        asmCode.Add(spaces + "mov [" + tempStr + "], ax");
                    }
                }
            }
            strLiterals.Reverse();
            asmCode.Insert(0, spaces + "include io.asm");
            asmCode.Insert(0, spaces + ".code");
            foreach (string literal in strLiterals)
            {
                equalMarker = literal.IndexOf("\"");
                asmCode.Insert(0,literal.Substring(0,equalMarker).PadRight(8) + "DB".PadRight(8) + literal.Substring(equalMarker) + ",\"$\"");
            }   
            asmCode.Insert(0, spaces + ".data");
            asmCode.Insert(0, spaces + ".stack 100h");
            asmCode.Insert(0, spaces + ".586");
            asmCode.Insert(0, spaces + ".model small");
            asmCode.Add("END start");

            foreach (string asmLine in asmCode)
                ASMemit(asmLine);
        }

        public void ASMemit(string ASMtoEmit)
        {
            using (StreamWriter file = new StreamWriter(@Globals.ASMfile, true))
            {
                file.WriteLine(ASMtoEmit);
            }
            //Console.WriteLine(ASMtoEmit);   //uncomment for verbose TAC generation
        }
    }
}
