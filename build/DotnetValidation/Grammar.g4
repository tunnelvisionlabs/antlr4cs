grammar Grammar;

compilationUnit
	:	'text' EOF
	;

keys : 'keys';
values : 'values';

WS
	:	[ \t\r\n]+ -> skip
	;
