grammar Grammar;

compilationUnit
	:	'text' EOF
	;

WS
	:	[ \t\r\n]+ -> skip
	;
