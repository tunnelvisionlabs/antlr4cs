/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.test.tool;

import org.antlr.v4.runtime.atn.ATNSerializer;
import org.junit.Test;

import static org.junit.Assert.*;

/** test runtime parse errors */
@SuppressWarnings("unused")
public class TestParseErrors extends BaseTest {
	@Test public void testTokenMismatch() throws Exception {
		String grammar =
			"grammar T;\n" +
			"a : 'a' 'b' ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "a", "aa", false);
		String expecting = "line 1:1 mismatched input 'a' expecting 'b'\n";
		String result = stderrDuringParse;
		assertEquals(expecting, result);
	}

	@Test public void testSingleTokenDeletion() throws Exception {
		String grammar =
			"grammar T;\n" +
			"a : 'a' 'b' ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "a", "aab", false);
		String expecting = "line 1:1 extraneous input 'a' expecting 'b'\n";
		String result = stderrDuringParse;
		assertEquals(expecting, result);
	}

	@Test public void testSingleTokenDeletionExpectingSet() throws Exception {
		String grammar =
			"grammar T;\n" +
			"a : 'a' ('b'|'c') ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "a", "aab", false);
		String expecting = "line 1:1 extraneous input 'a' expecting {'b', 'c'}\n";
		String result = stderrDuringParse;
		assertEquals(expecting, result);
	}

	@Test public void testSingleTokenInsertion() throws Exception {
		String grammar =
			"grammar T;\n" +
			"a : 'a' 'b' 'c' ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "a", "ac", false);
		String expecting = "line 1:1 missing 'b' at 'c'\n";
		String result = stderrDuringParse;
		assertEquals(expecting, result);
	}

	@Test public void testConjuringUpToken() throws Exception {
		String grammar =
			"grammar T;\n" +
			"a : 'a' x='b' {System.out.println(\"conjured=\"+$x);} 'c' ;";
		String result = execParser("T.g4", grammar, "TParser", "TLexer", "a", "ac", false);
		String expecting = "conjured=[@-1,-1:-1='<missing 'b'>',<2>,1:1]\n";
		assertEquals(expecting, result);
	}

	@Test public void testSingleSetInsertion() throws Exception {
		String grammar =
			"grammar T;\n" +
			"a : 'a' ('b'|'c') 'd' ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "a", "ad", false);
		String expecting = "line 1:1 missing {'b', 'c'} at 'd'\n";
		String result = stderrDuringParse;
		assertEquals(expecting, result);
	}

	@Test public void testConjuringUpTokenFromSet() throws Exception {
		String grammar =
			"grammar T;\n" +
			"a : 'a' x=('b'|'c') {System.out.println(\"conjured=\"+$x);} 'd' ;";
		String result = execParser("T.g4", grammar, "TParser", "TLexer", "a", "ad", false);
		String expecting = "conjured=[@-1,-1:-1='<missing 'b'>',<2>,1:1]\n";
		assertEquals(expecting, result);
	}

	@Test public void testLL2() throws Exception {
		String grammar =
			"grammar T;\n" +
			"a : 'a' 'b'" +
			"  | 'a' 'c'" +
			";\n" +
			"q : 'e' ;\n";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "a", "ae", false);
		String expecting = "line 1:1 no viable alternative at input 'ae'\n";
		String result = stderrDuringParse;
		assertEquals(expecting, result);
	}

	@Test public void testLL3() throws Exception {
		String grammar =
			"grammar T;\n" +
			"a : 'a' 'b'* 'c'" +
			"  | 'a' 'b' 'd'" +
			"  ;\n" +
			"q : 'e' ;\n";
		System.out.println(grammar);
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "a", "abe", false);
		String expecting = "line 1:2 no viable alternative at input 'abe'\n";
		String result = stderrDuringParse;
		assertEquals(expecting, result);
	}

	@Test public void testLLStar() throws Exception {
		String grammar =
			"grammar T;\n" +
			"a : 'a'+ 'b'" +
			"  | 'a'+ 'c'" +
			";\n" +
			"q : 'e' ;\n";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "a", "aaae", false);
		String expecting = "line 1:3 no viable alternative at input 'aaae'\n";
		String result = stderrDuringParse;
		assertEquals(expecting, result);
	}

	@Test public void testSingleTokenDeletionBeforeLoop() throws Exception {
		String grammar =
			"grammar T;\n" +
			"a : 'a' 'b'* EOF ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "a", "aabc", false);
		String expecting = "line 1:1 extraneous input 'a' expecting {<EOF>, 'b'}\n" +
			"line 1:3 token recognition error at: 'c'\n";
		String result = stderrDuringParse;
		assertEquals(expecting, result);
	}

	@Test public void testMultiTokenDeletionBeforeLoop() throws Exception {
		// can only delete 1 before loop
		String grammar =
			"grammar T;\n" +
			"a : 'a' 'b'* 'c';";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "a", "aacabc", false);
		String expecting =
			"line 1:1 extraneous input 'a' expecting {'b', 'c'}\n";
		String result = stderrDuringParse;
		assertEquals(expecting, result);
	}

	@Test public void testSingleTokenDeletionDuringLoop() throws Exception {
		String grammar =
			"grammar T;\n" +
			"a : 'a' 'b'* 'c' ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "a", "ababbc", false);
		String expecting = "line 1:2 extraneous input 'a' expecting {'b', 'c'}\n";
		String result = stderrDuringParse;
		assertEquals(expecting, result);
	}

	@Test public void testMultiTokenDeletionDuringLoop() throws Exception {
		String grammar =
			"grammar T;\n" +
			"a : 'a' 'b'* 'c' ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "a", "abaaababc", false);
		String expecting =
				"line 1:2 extraneous input 'a' expecting {'b', 'c'}\n" +
				"line 1:6 extraneous input 'a' expecting {'b', 'c'}\n";
		String result = stderrDuringParse;
		assertEquals(expecting, result);
	}

	// ------

	@Test public void testSingleTokenDeletionBeforeLoop2() throws Exception {
		String grammar =
			"grammar T;\n" +
			"a : 'a' ('b'|'z'{;})* EOF ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "a", "aabc", false);
		String expecting = "line 1:1 extraneous input 'a' expecting {<EOF>, 'b', 'z'}\n" +
			"line 1:3 token recognition error at: 'c'\n";
		String result = stderrDuringParse;
		assertEquals(expecting, result);
	}

	@Test public void testMultiTokenDeletionBeforeLoop2() throws Exception {
		// can only delete 1 before loop
		String grammar =
			"grammar T;\n" +
			"a : 'a' ('b'|'z'{;})* 'c';";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "a", "aacabc", false);
		String expecting =
			"line 1:1 extraneous input 'a' expecting {'b', 'z', 'c'}\n";
		String result = stderrDuringParse;
		assertEquals(expecting, result);
	}

	@Test public void testSingleTokenDeletionDuringLoop2() throws Exception {
		String grammar =
			"grammar T;\n" +
			"a : 'a' ('b'|'z'{;})* 'c' ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "a", "ababbc", false);
		String expecting = "line 1:2 extraneous input 'a' expecting {'b', 'z', 'c'}\n";
		String result = stderrDuringParse;
		assertEquals(expecting, result);
	}

	@Test public void testMultiTokenDeletionDuringLoop2() throws Exception {
		String grammar =
			"grammar T;\n" +
			"a : 'a' ('b'|'z'{;})* 'c' ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "a", "abaaababc", false);
		String expecting =
				"line 1:2 extraneous input 'a' expecting {'b', 'z', 'c'}\n" +
				"line 1:6 extraneous input 'a' expecting {'b', 'z', 'c'}\n";
		String result = stderrDuringParse;
		assertEquals(expecting, result);
	}

	@Test public void testLL1ErrorInfo() throws Exception {
		String grammar =
			"grammar T;\n" +
			"start : animal (AND acClass)? service EOF;\n" +
			"animal : (DOG | CAT );\n" +
			"service : (HARDWARE | SOFTWARE) ;\n" +
			"AND : 'and';\n" +
			"DOG : 'dog';\n" +
			"CAT : 'cat';\n" +
			"HARDWARE: 'hardware';\n" +
			"SOFTWARE: 'software';\n" +
			"WS : ' ' -> skip ;" +
			"acClass\n" +
			"@init\n" +
			"{ System.out.println(getExpectedTokens().toString(tokenNames)); }\n" +
			"  : ;\n";
		String result = execParser("T.g4", grammar, "TParser", "TLexer", "start", "dog and software", false);
		String expecting = "{'hardware', 'software'}\n";
		assertEquals(expecting, result);
	}

	/**
	 * This is a regression test for #6 "NullPointerException in getMissingSymbol".
	 * https://github.com/antlr/antlr4/issues/6
	 */
	@Test
	public void testInvalidEmptyInput() throws Exception {
		String grammar =
			"grammar T;\n" +
			"start : ID+;\n" +
			"ID : [a-z]+;\n" +
			"\n";
		String result = execParser("T.g4", grammar, "TParser", "TLexer", "start", "", true);
		String expecting = "";
		assertEquals(expecting, result);
		assertEquals("line 1:0 mismatched input '<EOF>' expecting ID\n", this.stderrDuringParse);
	}

	/**
	 * Regression test for "Getter for context is not a list when it should be".
	 * https://github.com/antlr/antlr4/issues/19
	 */
	@Test
	public void testContextListGetters() throws Exception {
		String grammar =
			"grammar T;\n" +
			"@parser::members{\n" +
			"  void foo() {\n" +
			"    SContext s = null;\n" +
			"    List<? extends AContext> a = s.a();\n" +
			"    List<? extends BContext> b = s.b();\n" +
			"  }\n" +
			"}\n" +
			"s : (a | b)+;\n" +
			"a : 'a' {System.out.print('a');};\n" +
			"b : 'b' {System.out.print('b');};\n" +
			"";
		String result = execParser("T.g", grammar, "TParser", "TLexer", "s", "abab", true);
		String expecting = "abab\n";
		assertEquals(expecting, result);
		assertNull(this.stderrDuringParse);
	}

	/**
	 * This is a regression test for #26 "an exception upon simple rule with double recursion in an alternative".
	 * https://github.com/antlr/antlr4/issues/26
	 */
	void testDuplicatedLeftRecursiveCall(String input) throws Exception {
		String grammar =
			"grammar T;\n" +
			"start : expr EOF;\n" +
			"expr : 'x'\n" +
			"     | expr expr\n" +
			"     ;\n" +
			"\n";
		String result = execParser("T.g4", grammar, "TParser", "TLexer", "start", input, true);
		assertEquals("", result);
		assertNull(this.stderrDuringParse);
	}


	@Test
	public void testDuplicatedLeftRecursiveCall1() throws Exception {
		testDuplicatedLeftRecursiveCall("x");
	}

	@Test
	public void testDuplicatedLeftRecursiveCall2() throws Exception {
		testDuplicatedLeftRecursiveCall("xx");
	}

	@Test
	public void testDuplicatedLeftRecursiveCall3() throws Exception {
		testDuplicatedLeftRecursiveCall("xxx");
	}

	@Test
	public void testDuplicatedLeftRecursiveCall4() throws Exception {
		testDuplicatedLeftRecursiveCall("xxxx");
	}

	/**
	 * Regression test for "Ambiguity at k=1 prevents full context parsing".
	 * https://github.com/antlr/antlr4/issues/44
	 */
	@Test
	public void testConflictingAltAnalysis() throws Exception {
		String grammar =
			"grammar T;\n" +
			"ss : s s EOF;\n" +
			"s : | x;\n" +
			"x : 'a' 'b';\n" +
			"";
		String result = execParser("T.g", grammar, "TParser", "TLexer", "ss", "abab", true);
		String expecting = "";
		assertEquals(expecting, result);
		assertEquals(
			"line 1:4 reportAttemptingFullContext d=0 (s), input='ab'\n" +
			"line 1:2 reportContextSensitivity d=0 (s), input='a'\n",
			this.stderrDuringParse);
	}

	/**
	 * This is a regression test for #45 "NullPointerException in ATNConfig.hashCode".
	 * https://github.com/antlr/antlr4/issues/45
	 * <p/>
	 * The original cause of this issue was an error in the tool's ATN state optimization,
	 * which is now detected early in {@link ATNSerializer} by ensuring that all
	 * serialized transitions point to states which were not removed.
	 */
	@Test
	public void testInvalidATNStateRemoval() throws Exception {
		String grammar =
			"grammar T;\n" +
			"start : ID ':' expr;\n" +
			"expr : primary expr? {} | expr '->' ID;\n" +
			"primary : ID;\n" +
			"ID : [a-z]+;\n" +
			"\n";
		String result = execParser("T.g4", grammar, "TParser", "TLexer", "start", "x:x", true);
		String expecting = "";
		assertEquals(expecting, result);
		assertNull(this.stderrDuringParse);
	}

	@Test public void testNoViableAltAvoidance() throws Exception {
		// "a." matches 'a' to rule e but then realizes '.' won't match.
		// previously would cause noviablealt. now prediction pretends to
		// have "a' predict 2nd alt of e. Will get syntax error later so
		// let it get farther.
		String grammar =
			"grammar T;\n" +
			"s : e '!' ;\n" +
			"e : 'a' 'b'\n" +
			"  | 'a'\n" +
			"  ;\n" +
			"DOT : '.' ;\n" +
			"WS : [ \\t\\r\\n]+ -> skip;\n";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "s", "a.", false);
		String expecting =
				"line 1:1 mismatched input '.' expecting '!'\n";
		String result = stderrDuringParse;
		assertEquals(expecting, result);
	}

	@Test
	public void testSingleTokenDeletionConsumption() throws Exception {
		String grammar =
			"grammar T;\n" +
			"set: ('b'|'c') ;\n" +
			"a: 'a' set 'd' {System.out.println($set.stop);} ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "a", "aabd", false);
		assertEquals("[@2,2:2='b',<1>,1:2]\n", found);
		assertEquals("line 1:1 extraneous input 'a' expecting {'b', 'c'}\n", this.stderrDuringParse);
	}

	@Test
	public void testSingleSetInsertionConsumption() throws Exception {
		String grammar =
			"grammar T;\n" +
			"set: ('b'|'c') ;\n" +
			"a: 'a' set 'd' {System.out.println($set.stop);} ;";
		String found = execParser("T.g4", grammar, "TParser", "TLexer", "a", "ad", false);
		assertEquals("[@0,0:0='a',<3>,1:0]\n", found);
		assertEquals("line 1:1 missing {'b', 'c'} at 'd'\n", this.stderrDuringParse);
	}
}
