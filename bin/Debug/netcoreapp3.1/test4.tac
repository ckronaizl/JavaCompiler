
Proc firstclass
_ax= 0
Endp firstclass
Proc secondclass
_bp-2=_bp+4
_bp-6=2
_bp-8=_bp-6*_bp-2
_bp-2=_bp-8
_ax=_bp-2
Endp secondclass
Proc thirdclass
_bp-6=5
_bp-4=_bp-6
push _bp-4
call secondclass
_bp-2=_ax
wri _bp-2
wrln
_ax=_bp-4
Endp thirdclass
Proc main
call thirdclass
Endp main
