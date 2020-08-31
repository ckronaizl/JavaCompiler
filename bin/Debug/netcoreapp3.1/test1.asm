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
        sub sp, 12
        mov ax, 5
        mov [bp-8], ax
        mov ax, [bp-8]
        mov [bp-2], ax
        mov ax, 10
        mov [bp-10], ax
        mov ax, [bp-10]
        mov [bp-4], ax
        mov ax, [bp-2]
        mov bx, [bp-4]
        imul bx
        mov [bp-12], ax
        mov ax, [bp-12]
        mov [bp-6], ax
        mov dx, [bp-6]
        call writeint
        call writeln
        mov ax, [bp-6]
        add sp, 12
        pop bp
        ret 0
secondclass      endp
main PROC
        call secondclass
        ret
main endp
END start
