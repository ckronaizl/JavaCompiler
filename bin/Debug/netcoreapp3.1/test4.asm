        .model small
        .586
        .stack 100h
        .data
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
        sub sp, 8
        mov ax, [bp+4]
        mov [bp-2], ax
        mov ax, 2
        mov [bp-6], ax
        mov ax, [bp-6]
        mov bx, [bp-2]
        imul bx
        mov [bp-8], ax
        mov ax, [bp-8]
        mov [bp-2], ax
        mov ax, [bp-2]
        add sp, 8
        pop bp
        ret 2
secondclass      endp
thirdclass       PROC
        push bp
        mov bp, sp
        sub sp, 6
        mov ax, 5
        mov [bp-6], ax
        mov ax, [bp-6]
        mov [bp-4], ax
        mov ax, [bp-4]
        push ax
        call secondclass
        mov [bp-2], ax
        mov dx, [bp-2]
        call writeint
        call writeln
        mov ax, [bp-4]
        add sp, 6
        pop bp
        ret 0
thirdclass       endp
main PROC
        call thirdclass
        ret
main endp
END start
