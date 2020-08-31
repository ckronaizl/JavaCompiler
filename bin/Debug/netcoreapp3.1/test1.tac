
Proc firstclass
_ax= 0
Endp firstclass
Proc secondclass
_bp-8=5
_bp-2=_bp-8
_bp-10=10
_bp-4=_bp-10
_bp-12=_bp-2*_bp-4
_bp-6=_bp-12
wri _bp-6
wrln
_ax=_bp-6
Endp secondclass
Proc main
call secondclass
Endp main
