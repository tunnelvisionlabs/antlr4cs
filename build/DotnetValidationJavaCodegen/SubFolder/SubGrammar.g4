grammar SubGrammar;

compilationUnit
	:	'text' EOF
	;

WS
	:	[ \t\r\n]+ -> skip
	;
