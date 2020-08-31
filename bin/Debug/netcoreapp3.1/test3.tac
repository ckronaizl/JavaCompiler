
Proc firstclass
_ax= 0
Endp firstclass
Proc secondclass
_bp-4=_bp+4*_bp+6
_bp-6=_bp-4+_bp+8
_bp-2=_bp-6
wrs S0 "The answer is "
wri _bp-2
wrs S1 " "
wrln
_ax=_bp-2
Endp secondclass
Proc thirdclass
_bp-8=5
_bp-2=_bp-8
_bp-10=10
_bp-4=_bp-10
_bp-12=20
_bp-6=_bp-12
push _bp-2
push _bp-6
push _bp-4
call secondclass
_ax=_bp-2
Endp thirdclass
Proc main
call thirdclass
Endp main
