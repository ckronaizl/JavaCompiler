        .model small
        .586
        .stack 100h
        .data
S0      DB      "Enter a number","$"
S1      DB      "The answer is ","$"
        .code
        include io.asm
start   PROC
        mov ax, @data
        mov ds, ax
        call main
        mov ah, 4ch
        mov al,0
        int 21h
start ENDP

firstclass       PROC
        push bp
        mov bp, sp
        sub sp, 0
        mov ax,  0
        add sp, 0
        pop bp
        ret 0
firstclass       endp
secondclass      PROC
        push bp
        mov bp, sp
        sub sp, 16
        mov dx, offset S0
        call writestr
        call readint
        mov [bp-2], bx
        mov ax, 10
        mov [bp-10], ax
        mov ax, [bp-10]
        mov [bp-4], ax
        mov ax, 20
        mov [bp-12], ax
        mov ax, [bp-12]
        mov [bp-8], ax
        mov ax, [bp-2]
        mov bx, [bp-4]
        imul bx
        mov [bp-16], ax
        mov ax, [bp-8]
        add ax, [bp-16]
        mov [bp-14], ax
        mov ax, [bp-14]
        mov [bp-6], ax
        mov dx, offset S1
        call writestr
        mov dx, [bp-6]
        call writeint
        call writeln
        mov ax, [bp-6]
        add sp, 16
        pop bp
        ret 0
secondclass      endp
main PROC
        call secondclass
        ret
main endp
END start
