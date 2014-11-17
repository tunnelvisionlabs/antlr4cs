package org.antlr.v4.test.rt.csharp;

import org.junit.Test;

public class TestSemPredEvalParser extends BaseTest {

	@Test
	public void testSimpleValidate() throws Exception {
		String grammar = "grammar T;\n" +
	                  "s : a ;\n" +
	                  "a : {false}? ID  {Console.WriteLine(\"alt 1\");}\n" +
	                  "  | {true}?  INT {Console.WriteLine(\"alt 2\");}\n" +
	                  "  ;\n" +
	                  "ID : 'a'..'z'+ ;\n" +
	                  "INT : '0'..'9'+;\n" +
	                  "WS : (' '|'\\n') -> skip ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "x", true);
		assertEquals("", found);
		assertEquals("line 1:0 no viable alternative at input 'x'\n", this.stderrDuringParse);
	}

	@Test
	public void testSimpleValidate2() throws Exception {
		String grammar = "grammar T;\n" +
	                  "s : a a a;\n" +
	                  "a : {false}? ID  {Console.WriteLine(\"alt 1\");}\n" +
	                  "  | {true}?  INT {Console.WriteLine(\"alt 2\");}\n" +
	                  "  ;\n" +
	                  "ID : 'a'..'z'+ ;\n" +
	                  "INT : '0'..'9'+;\n" +
	                  "WS : (' '|'\\n') -> skip ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "3 4 x", true);
		assertEquals("alt 2\nalt 2\n", found);
		assertEquals("line 1:4 no viable alternative at input 'x'\n", this.stderrDuringParse);
	}

	@Test
	public void testAtomWithClosureInTranslatedLRRule() throws Exception {
		String grammar = "grammar T;\n" +
	                  "start : e[0] EOF;\n" +
	                  "e[int _p]\n" +
	                  "    :   ( 'a' | 'b'+ ) ( {3 >= $_p}? '+' e[4] )*\n" +
	                  "    ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "start", "a+b+a", false);
		assertEquals("", found);
		assertNull(this.stderrDuringParse);
	}

	@Test
	public void testValidateInDFA() throws Exception {
		String grammar = "grammar T;\n" +
	                  "s : a ';' a;\n" +
	                  "// ';' helps us to resynchronize without consuming\n" +
	                  "// 2nd 'a' reference. We our testing that the DFA also\n" +
	                  "// throws an exception if the validating predicate fails\n" +
	                  "a : {false}? ID  {Console.WriteLine(\"alt 1\");}\n" +
	                  "  | {true}?  INT {Console.WriteLine(\"alt 2\");}\n" +
	                  "  ;\n" +
	                  "ID : 'a'..'z'+ ;\n" +
	                  "INT : '0'..'9'+;\n" +
	                  "WS : (' '|'\\n') -> skip ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "x ; y", true);
		assertEquals("", found);
		assertEquals("line 1:0 no viable alternative at input 'x'\nline 1:4 no viable alternative at input 'y'\n", this.stderrDuringParse);
	}

	@Test
	public void testSimple() throws Exception {
		String grammar = "grammar T;\n" +
	                  "s : a a a; // do 3x: once in ATN, next in DFA then INT in ATN\n" +
	                  "a : {false}? ID {Console.WriteLine(\"alt 1\");}\n" +
	                  "  | {true}?  ID {Console.WriteLine(\"alt 2\");}\n" +
	                  "  | INT         {Console.WriteLine(\"alt 3\");}\n" +
	                  "  ;\n" +
	                  "ID : 'a'..'z'+ ;\n" +
	                  "INT : '0'..'9'+;\n" +
	                  "WS : (' '|'\\n') -> skip ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "x y 3", true);
		assertEquals("alt 2\nalt 2\nalt 3\n", found);
		assertNull(this.stderrDuringParse);
	}

	@Test
	public void testOrder() throws Exception {
		String grammar = "grammar T;\n" +
	                  "s : a {} a; // do 2x: once in ATN, next in DFA;\n" +
	                  "// action blocks lookahead from falling off of 'a'\n" +
	                  "// and looking into 2nd 'a' ref. !ctx dependent pred\n" +
	                  "a : ID {Console.WriteLine(\"alt 1\");}\n" +
	                  "  | {true}?  ID {Console.WriteLine(\"alt 2\");}\n" +
	                  "  ;\n" +
	                  "ID : 'a'..'z'+ ;\n" +
	                  "INT : '0'..'9'+;\n" +
	                  "WS : (' '|'\\n') -> skip ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "x y", false);
		assertEquals("alt 1\nalt 1\n", found);
		assertNull(this.stderrDuringParse);
	}

	@Test
	public void test2UnpredicatedAlts() throws Exception {
		String grammar = "grammar T;\n" +
	                  "s : {Interpreter.PredictionMode = PredictionMode.LlExactAmbigDetection;} a ';' a; // do 2x: once in ATN, next in DFA\n" +
	                  "a : ID {Console.WriteLine(\"alt 1\");}\n" +
	                  "  | ID {Console.WriteLine(\"alt 2\");}\n" +
	                  "  | {false}? ID {Console.WriteLine(\"alt 3\");}\n" +
	                  "  ;\n" +
	                  "ID : 'a'..'z'+ ;\n" +
	                  "INT : '0'..'9'+;\n" +
	                  "WS : (' '|'\\n') -> skip ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "x; y", true);
		assertEquals("alt 1\nalt 1\n", found);
		assertEquals("line 1:0 reportAttemptingFullContext d=0 (a), input='x'\nline 1:0 reportAmbiguity d=0 (a): ambigAlts={1, 2}, input='x'\nline 1:3 reportAttemptingFullContext d=0 (a), input='y'\nline 1:3 reportAmbiguity d=0 (a): ambigAlts={1, 2}, input='y'\n", this.stderrDuringParse);
	}

	@Test
	public void test2UnpredicatedAltsAndOneOrthogonalAlt() throws Exception {
		String grammar = "grammar T;\n" +
	                  "s : {Interpreter.PredictionMode = PredictionMode.LlExactAmbigDetection;} a ';' a ';' a;\n" +
	                  "a : INT {Console.WriteLine(\"alt 1\");}\n" +
	                  "  | ID {Console.WriteLine(\"alt 2\");} // must pick this one for ID since pred is false\n" +
	                  "  | ID {Console.WriteLine(\"alt 3\");}\n" +
	                  "  | {false}? ID {Console.WriteLine(\"alt 4\");}\n" +
	                  "  ;\n" +
	                  "ID : 'a'..'z'+ ;\n" +
	                  "INT : '0'..'9'+;\n" +
	                  "WS : (' '|'\\n') -> skip ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "34; x; y", true);
		assertEquals("alt 1\nalt 2\nalt 2\n", found);
		assertEquals("line 1:4 reportAttemptingFullContext d=0 (a), input='x'\nline 1:4 reportAmbiguity d=0 (a): ambigAlts={2, 3}, input='x'\nline 1:7 reportAttemptingFullContext d=0 (a), input='y'\nline 1:7 reportAmbiguity d=0 (a): ambigAlts={2, 3}, input='y'\n", this.stderrDuringParse);
	}

	@Test
	public void testRewindBeforePredEval() throws Exception {
		String grammar = "grammar T;\n" +
	                  "s : a a;\n" +
	                  "a : {this.TokenStream.Lt(1).Text.Equals(\"x\")}? ID INT {Console.WriteLine(\"alt 1\");}\n" +
	                  "  | {this.TokenStream.Lt(1).Text.Equals(\"y\")}? ID INT {Console.WriteLine(\"alt 2\");}\n" +
	                  "  ;\n" +
	                  "ID : 'a'..'z'+ ;\n" +
	                  "INT : '0'..'9'+;\n" +
	                  "WS : (' '|'\\n') -> skip ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "y 3 x 4", true);
		assertEquals("alt 2\nalt 1\n", found);
		assertNull(this.stderrDuringParse);
	}

	@Test
	public void testNoTruePredsThrowsNoViableAlt() throws Exception {
		String grammar = "grammar T;\n" +
	                  "s : a a;\n" +
	                  "a : {false}? ID INT {Console.WriteLine(\"alt 1\");}\n" +
	                  "  | {false}? ID INT {Console.WriteLine(\"alt 2\");}\n" +
	                  "  ;\n" +
	                  "ID : 'a'..'z'+ ;\n" +
	                  "INT : '0'..'9'+;\n" +
	                  "WS : (' '|'\\n') -> skip ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "y 3 x 4", false);
		assertEquals("", found);
		assertEquals("line 1:0 no viable alternative at input 'y'\n", this.stderrDuringParse);
	}

	@Test
	public void testToLeft() throws Exception {
		String grammar = "grammar T;\n" +
	                  "	s : a+ ;\n" +
	                  "a : {false}? ID {Console.WriteLine(\"alt 1\");}\n" +
	                  "  | {true}?  ID {Console.WriteLine(\"alt 2\");}\n" +
	                  "  ;\n" +
	                  "ID : 'a'..'z'+ ;\n" +
	                  "INT : '0'..'9'+;\n" +
	                  "WS : (' '|'\\n') -> skip ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "x x y", true);
		assertEquals("alt 2\nalt 2\nalt 2\n", found);
		assertNull(this.stderrDuringParse);
	}

	@Test
	public void testUnpredicatedPathsInAlt() throws Exception {
		String grammar = "grammar T;\n" +
	                  "s : a {Console.WriteLine(\"alt 1\");}\n" +
	                  "  | b {Console.WriteLine(\"alt 2\");}\n" +
	                  "  ;\n" +
	                  "a : {false}? ID INT\n" +
	                  "  | ID INT\n" +
	                  "  ;\n" +
	                  "b : ID ID\n" +
	                  "  ;\n" +
	                  "ID : 'a'..'z'+ ;\n" +
	                  "INT : '0'..'9'+;\n" +
	                  "WS : (' '|'\\n') -> skip ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "x 4", true);
		assertEquals("alt 1\n", found);
		assertNull(this.stderrDuringParse);
	}

	@Test
	public void testActionHidesPreds() throws Exception {
		String grammar = "grammar T;\n" +
	                  "@members {int i = 0;}\n" +
	                  "s : a+ ;\n" +
	                  "a : {this.i = 1;} ID {this.i == 1}? {Console.WriteLine(\"alt 1\");}\n" +
	                  "  | {this.i = 2;} ID {this.i == 2}? {Console.WriteLine(\"alt 2\");}\n" +
	                  "  ;\n" +
	                  "ID : 'a'..'z'+ ;\n" +
	                  "INT : '0'..'9'+;\n" +
	                  "WS : (' '|'\\n') -> skip ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "x x y", false);
		assertEquals("alt 1\nalt 1\nalt 1\n", found);
		assertNull(this.stderrDuringParse);
	}

	@Test
	public void testToLeftWithVaryingPredicate() throws Exception {
		String grammar = "grammar T;\n" +
	                  "@members {int i = 0;}\n" +
	                  "s : ({this.i += 1;\n" +
	                  "Console.WriteLine(\"i=\" + this.i);} a)+ ;\n" +
	                  "a : {this.i % 2 == 0}? ID {Console.WriteLine(\"alt 1\");}\n" +
	                  "  | {this.i % 2 != 0}? ID {Console.WriteLine(\"alt 2\");}\n" +
	                  "  ;\n" +
	                  "ID : 'a'..'z'+ ;\n" +
	                  "INT : '0'..'9'+;\n" +
	                  "WS : (' '|'\\n') -> skip ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "x x y", true);
		assertEquals("i=1\nalt 2\ni=2\nalt 1\ni=3\nalt 2\n", found);
		assertNull(this.stderrDuringParse);
	}

	@Test
	public void testPredicateDependentOnArg() throws Exception {
		String grammar = "grammar T;\n" +
	                  "@members {int i = 0;}\n" +
	                  "s : a[2] a[1];\n" +
	                  "a[int i]\n" +
	                  "  : {$i==1}? ID {Console.WriteLine(\"alt 1\");}\n" +
	                  "  | {$i==2}? ID {Console.WriteLine(\"alt 2\");}\n" +
	                  "  ;\n" +
	                  "ID : 'a'..'z'+ ;\n" +
	                  "INT : '0'..'9'+;\n" +
	                  "WS : (' '|'\\n') -> skip ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "a b", true);
		assertEquals("alt 2\nalt 1\n", found);
		assertNull(this.stderrDuringParse);
	}

	@Test
	public void testPredicateDependentOnArg2() throws Exception {
		String grammar = "grammar T;\n" +
	                  "@members {int i = 0;}\n" +
	                  "s : a[2] a[1];\n" +
	                  "a[int i]\n" +
	                  "  : {$i==1}? ID \n" +
	                  "  | {$i==2}? ID \n" +
	                  "  ;\n" +
	                  "ID : 'a'..'z'+ ;\n" +
	                  "INT : '0'..'9'+;\n" +
	                  "WS : (' '|'\\n') -> skip ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "a b", true);
		assertEquals("", found);
		assertNull(this.stderrDuringParse);
	}

	@Test
	public void testDependentPredNotInOuterCtxShouldBeIgnored() throws Exception {
		String grammar = "grammar T;\n" +
	                  "s : b[2] ';' |  b[2] '.' ; // decision in s drills down to ctx-dependent pred in a;\n" +
	                  "b[int i] : a[i] ;\n" +
	                  "a[int i]\n" +
	                  "  : {$i==1}? ID {Console.WriteLine(\"alt 1\");}\n" +
	                  "    | {$i==2}? ID {Console.WriteLine(\"alt 2\");}\n" +
	                  "    ;\n" +
	                  "ID : 'a'..'z'+ ;\n" +
	                  "INT : '0'..'9'+;\n" +
	                  "WS : (' '|'\\n') -> skip ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "a;", true);
		assertEquals("alt 2\n", found);
		assertNull(this.stderrDuringParse);
	}

	@Test
	public void testIndependentPredNotPassedOuterCtxToAvoidCastException() throws Exception {
		String grammar = "grammar T;\n" +
	                  "s : b ';' |  b '.' ;\n" +
	                  "b : a ;\n" +
	                  "a\n" +
	                  "  : {false}? ID {Console.WriteLine(\"alt 1\");}\n" +
	                  "  | {true}? ID {Console.WriteLine(\"alt 2\");}\n" +
	                  " ;\n" +
	                  "ID : 'a'..'z'+ ;\n" +
	                  "INT : '0'..'9'+;\n" +
	                  "WS : (' '|'\\n') -> skip ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "a;", true);
		assertEquals("alt 2\n", found);
		assertNull(this.stderrDuringParse);
	}

	@Test
	public void testPredsInGlobalFOLLOW() throws Exception {
		String grammar = "grammar T;\n" +
	                  "@members {\n" +
	                  "bool pred(bool v) {\n" +
	                  "	Console.WriteLine(\"eval=\"+v.ToString().ToLower());\n" +
	                  "	return v;\n" +
	                  "}\n" +
	                  "}\n" +
	                  "s : e {this.pred(true)}? {Console.WriteLine(\"parse\");} '!' ;\n" +
	                  "t : e {this.pred(false)}? ID ;\n" +
	                  "e : ID | ; // non-LL(1) so we use ATN\n" +
	                  "ID : 'a'..'z'+ ;\n" +
	                  "INT : '0'..'9'+;\n" +
	                  "WS : (' '|'\\n') -> skip ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "a!", true);
		assertEquals("eval=true\nparse\n", found);
		assertNull(this.stderrDuringParse);
	}

	@Test
	public void testDepedentPredsInGlobalFOLLOW() throws Exception {
		String grammar = "grammar T;\n" +
	                  "@members {\n" +
	                  "bool pred(bool v) {\n" +
	                  "	Console.WriteLine(\"eval=\"+v.ToString().ToLower());\n" +
	                  "	return v;\n" +
	                  "}\n" +
	                  "}\n" +
	                  "s : a[99] ;\n" +
	                  "a[int i] : e {this.pred($i==99)}? {Console.WriteLine(\"parse\");} '!' ;\n" +
	                  "b[int i] : e {this.pred($i==99)}? ID ;\n" +
	                  "e : ID | ; // non-LL(1) so we use ATN\n" +
	                  "ID : 'a'..'z'+ ;\n" +
	                  "INT : '0'..'9'+;\n" +
	                  "WS : (' '|'\\n') -> skip ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "a!", true);
		assertEquals("eval=true\nparse\n", found);
		assertNull(this.stderrDuringParse);
	}

	@Test
	public void testActionsHidePredsInGlobalFOLLOW() throws Exception {
		String grammar = "grammar T;\n" +
	                  "@members {\n" +
	                  "bool pred(bool v) {\n" +
	                  "	Console.WriteLine(\"eval=\"+v.ToString().ToLower());\n" +
	                  "	return v;\n" +
	                  "}\n" +
	                  "}\n" +
	                  "s : e {} {this.pred(true)}? {Console.WriteLine(\"parse\");} '!' ;\n" +
	                  "t : e {} {this.pred(false)}? ID ;\n" +
	                  "e : ID | ; // non-LL(1) so we use ATN\n" +
	                  "ID : 'a'..'z'+ ;\n" +
	                  "INT : '0'..'9'+;\n" +
	                  "WS : (' '|'\\n') -> skip ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "a!", true);
		assertEquals("eval=true\nparse\n", found);
		assertNull(this.stderrDuringParse);
	}

	String testPredTestedEvenWhenUnAmbig(String input) throws Exception {
		String grammar = "grammar T;\n" +
	                  "@members {bool enumKeyword = true;}\n" +
	                  "primary\n" +
	                  "    :   ID {Console.WriteLine(\"ID \"+$ID.text);}\n" +
	                  "    |   {!this.enumKeyword}? 'enum' {Console.WriteLine(\"enum\");}\n" +
	                  "    ;\n" +
	                  "ID : [a-z]+ ;\n" +
	                  "WS : [ \\t\\n\\r]+ -> skip ;";
		return execParser("T.g4", grammar, "TParser", "TLexer", "primary", input, true);
	}

	@Test
	public void testPredTestedEvenWhenUnAmbig_1() throws Exception {
		String found = testPredTestedEvenWhenUnAmbig("abc");
		assertEquals("ID abc\n", found);
		assertNull(this.stderrDuringParse);
	}

	@Test
	public void testPredTestedEvenWhenUnAmbig_2() throws Exception {
		String found = testPredTestedEvenWhenUnAmbig("enum");
		assertEquals("", found);
		assertEquals("line 1:0 no viable alternative at input 'enum'\n", this.stderrDuringParse);
	}

	@Test
	public void testDisabledAlternative() throws Exception {
		String grammar = "grammar T;\n" +
	                  "cppCompilationUnit : content+ EOF;\n" +
	                  "content: anything | {false}? .;\n" +
	                  "anything: ANY_CHAR;\n" +
	                  "ANY_CHAR: [_a-zA-Z0-9];";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "cppCompilationUnit", "hello", true);
		assertEquals("", found);
		assertNull(this.stderrDuringParse);
	}

	String testPredFromAltTestedInLoopBack(String input) throws Exception {
		String grammar = "grammar T;\n" +
	                  "file_\n" +
	                  "@after {Console.WriteLine($ctx.ToStringTree(this));}\n" +
	                  "  : para para EOF ;\n" +
	                  "para: paraContent NL NL ;\n" +
	                  "paraContent : ('s'|'x'|{this.InputStream.La(2)!=NL}? NL)+ ;\n" +
	                  "NL : '\\n' ;\n" +
	                  "s : 's' ;\n" +
	                  "X : 'x' ;";
		return execParser("T.g4", grammar, "TParser", "TLexer", "file_", input, true);
	}

	@Test
	public void testPredFromAltTestedInLoopBack_1() throws Exception {
		String found = testPredFromAltTestedInLoopBack("s\n\n\nx\n");
		assertEquals("(file_ (para (paraContent s) \\n \\n) (para (paraContent \\n x \\n)) <EOF>)\n", found);
		assertEquals("line 5:2 mismatched input '<EOF>' expecting '\n'\n", this.stderrDuringParse);
	}

	@Test
	public void testPredFromAltTestedInLoopBack_2() throws Exception {
		String found = testPredFromAltTestedInLoopBack("s\n\n\nx\n\n");
		assertEquals("(file_ (para (paraContent s) \\n \\n) (para (paraContent \\n x) \\n \\n) <EOF>)\n", found);
		assertNull(this.stderrDuringParse);
	}


}