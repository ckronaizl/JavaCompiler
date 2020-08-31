        .model small
        .586
        .stack 100h
        .data
S0      DB      "The answer is ","$"
S1      DB      " ","$"
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
        sub sp, 6
        mov ax, [bp+4]
        mov bx, [bp+6]
        imul bx
        mov [bp-4], ax
        mov ax, [bp-4]
        add ax, [bp+8]
        mov [bp-6], ax
        mov ax, [bp-6]
        mov [bp-2], ax
        mov dx, offset S0
        call writestr
        mov dx, [bp-2]
        call writeint
        mov dx, offset S1
        call writestr
        call writeln
        mov ax, [bp-2]
        add sp, 6
        pop bp
        ret 6
secondclass      endp
thirdclass       PROC
        push bp
        mov bp, sp
        sub sp, 12
        mov ax, 5
        mov [bp-8], ax
        mov ax, [bp-8]
        mov [bp-2], ax
        mov ax, 10
        mov [bp-10], ax
        mov ax, [bp-10]
        mov [bp-4], ax
        mov ax, 20
        mov [bp-12], ax
        mov ax, [bp-12]
        mov [bp-6], ax
        mov ax, [bp-2]
        push ax
        mov ax, [bp-6]
        push ax
        mov ax, [bp-4]
        push ax
        call secondclass
        mov ax, [bp-2]
        add sp, 12
        pop bp
        ret 0
thirdclass       endp
main PROC
        call thirdclass
        ret
main endp
END start
