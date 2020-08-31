
Proc firstclass
_ax= 0
Endp firstclass
Proc secondclass
wrs S0 "Enter a number"
wri _bp-2
wri _bp-4
rdi _bp-2
rdi _bp-4
rdi _bp-6
_bp-10=10
_bp-4=_bp-10
_bp-12=20
_bp-8=_bp-12
_bp-16=_bp-2*_bp-4
_bp-14=_bp-8-_bp-16
_bp-6=_bp-14
wrs S1 "The answer is "
wri _bp-2
wri _bp-4
wri _bp-6
wri _bp-8
wrln
_ax=_bp-6
Endp secondclass
Proc main
call secondclass
Endp main
