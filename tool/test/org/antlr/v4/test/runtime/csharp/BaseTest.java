/*
 * [The "BSD license"]
 *  Copyright (c) 2012 Terence Parr
 *  Copyright (c) 2012 Sam Harwell
 *  All rights reserved.
 *
 *  Redistribution and use in source and binary forms, with or without
 *  modification, are permitted provided that the following conditions
 *  are met:
 *
 *  1. Redistributions of source code must retain the above copyright
 *     notice, this list of conditions and the following disclaimer.
 *  2. Redistributions in binary form must reproduce the above copyright
 *     notice, this list of conditions and the following disclaimer in the
 *     documentation and/or other materials provided with the distribution.
 *  3. The name of the author may not be used to endorse or promote products
 *     derived from this software without specific prior written permission.
 *
 *  THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 *  IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 *  OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 *  IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 *  INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 *  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 *  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 *  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 *  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 *  THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
package org.antlr.v4.test.runtime.csharp;

import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import org.antlr.v4.CSharpTool;
import org.antlr.v4.Tool;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.misc.Utils;
import org.antlr.v4.tool.ANTLRMessage;
import org.antlr.v4.tool.DefaultToolListener;
import org.junit.Before;
import org.junit.Rule;
import org.junit.rules.TestName;
import org.stringtemplate.v4.ST;

import static org.junit.Assert.assertTrue;

public abstract class BaseTest {
	public String tmpdir;
	@Rule public TestName name = new TestName();
	protected String input;
	protected String expectedOutput;
	protected String expectedErrors;

	/**
	 * Generates C# verbatim style Template String body (WITHOUT surrounding quotes)
	 */
	private static String asTemplateString(String text) {
		return text.replace("\"","\"\"");
	}

	public static void writeFile(String dir, String fileName, String content) {
		try {
			Utils.writeFile(dir+"/"+fileName, content, "UTF-8");
		}
		catch (IOException ioe) {
			System.err.println("can't write file");
			ioe.printStackTrace(System.err);
		}
	}

	@Before
	public void setUp() throws Exception {
		File cd = new File(".").getAbsoluteFile();
		File baseDir = cd.getParentFile().getParentFile();
		File classDir = new File(baseDir, "runtime/CSharp/Antlr4.Runtime.Test/Generated/" + getClass().getSimpleName());
		File testDir = new File(classDir, name.getMethodName());
		testDir.mkdirs();
		for (File file : testDir.listFiles()) {
			file.delete();
		}
		this.tmpdir = testDir.getAbsolutePath();
	}

    private org.antlr.v4.Tool newTool(String[] args) {
		return new CSharpTool(args);
	}

	private ErrorQueue antlr(String grammarFileName, boolean defaultListener, String... extraOptions) {
		final List<String> options = new ArrayList<String>();
		Collections.addAll(options, extraOptions);
		options.add("-Dlanguage=CSharp_v4_5");
		options.add("-package");
		options.add("Antlr4.Runtime.Test.Generated." + getClass().getSimpleName() + "._" + this.name.getMethodName().substring(4));
		if ( !options.contains("-o") ) {
			options.add("-o");
			options.add(tmpdir);
		}
		if ( !options.contains("-lib") ) {
			options.add("-lib");
			options.add(tmpdir);
		}
		if ( !options.contains("-encoding") ) {
			options.add("-encoding");
			options.add("UTF-8");
		}
		options.add(new File(tmpdir,grammarFileName).toString());

		final String[] optionsA = new String[options.size()];
		options.toArray(optionsA);
		Tool antlr = newTool(optionsA);
		ErrorQueue equeue = new ErrorQueue(antlr);
		antlr.addListener(equeue);
		if (defaultListener) {
			antlr.addListener(new DefaultToolListener(antlr));
		}
		antlr.processGrammarsOnCommandLine();

		if ( !defaultListener && !equeue.errors.isEmpty() ) {
			System.err.println("antlr reports errors from "+options);
			for (int i = 0; i < equeue.errors.size(); i++) {
				ANTLRMessage msg = equeue.errors.get(i);
				System.err.println(msg);
			}
			System.out.println("!!!\ngrammar:");
			try {
				System.out.println(new String(Utils.readFile(tmpdir+"/"+grammarFileName)));
			}
			catch (IOException ioe) {
				System.err.println(ioe.toString());
			}
			System.out.println("###");
		}
		if ( !defaultListener && !equeue.warnings.isEmpty() ) {
			System.err.println("antlr reports warnings from "+options);
			for (int i = 0; i < equeue.warnings.size(); i++) {
				ANTLRMessage msg = equeue.warnings.get(i);
				System.err.println(msg);
			}
		}

		return equeue;
	}

	private ErrorQueue antlr(String grammarFileName, String grammarStr, boolean defaultListener, String... extraOptions) {
		mkdir(tmpdir);
		writeFile(tmpdir, grammarFileName, grammarStr);
		return antlr(grammarFileName, defaultListener, extraOptions);
	}

	protected void generateLexerTest(String grammarFileName,
							   String grammarStr,
							   String lexerName,
							   boolean showDFA)
	{
		boolean success = rawGenerateRecognizer(grammarFileName,
									  grammarStr,
									  null,
									  lexerName);
		assertTrue(success);
		writeLexerTestFile(lexerName, showDFA);
	}

	protected void generateParserTest(String grammarFileName,
								String grammarStr,
								String parserName,
								String lexerName,
								String startRuleName,
								boolean debug)
	{
		boolean success = rawGenerateRecognizer(grammarFileName,
														grammarStr,
														parserName,
														lexerName,
														"-visitor");
		assertTrue(success);
		if (parserName == null) {
			writeLexerTestFile(lexerName, false);
		}
		else {
			writeParserTestFile(parserName, lexerName, startRuleName, debug);
		}
	}

	/** Return true if all is well */
	private boolean rawGenerateRecognizer(String grammarFileName,
													String grammarStr,
													String parserName,
													String lexerName,
													String... extraOptions)
	{
		return rawGenerateRecognizer(grammarFileName, grammarStr, parserName, lexerName, false, extraOptions);
	}

	/** Return true if all is well */
	private boolean rawGenerateRecognizer(String grammarFileName,
													String grammarStr,
													String parserName,
													String lexerName,
													boolean defaultListener,
													String... extraOptions)
	{
		ErrorQueue equeue = antlr(grammarFileName, grammarStr, defaultListener, extraOptions);
		return equeue.errors.isEmpty();
	}

	protected void mkdir(String dir) {
		File f = new File(dir);
		f.mkdirs();
	}

	private void writeParserTestFile(String parserName,
								 String lexerName,
								 String parserStartRuleName,
								 boolean debug)
	{
//		ST outputFileST = new ST(
//			"using System;\n" +
//			"using Antlr4.Runtime;\n" +
//			"using Antlr4.Runtime.Tree;\n" +
//			"\n" +
//			"public class Test {\n" +
//			"    public static void Main(string[] args) {\n" +
//			"        ICharStream input = new AntlrFileStream(args[0]);\n" +
//			"        <lexerName> lex = new <lexerName>(input);\n" +
//			"        CommonTokenStream tokens = new CommonTokenStream(lex);\n" +
//			"        <createParser>\n"+
//			"		 parser.BuildParseTree = true;\n" +
//			"        ParserRuleContext tree = parser.<parserStartRuleName>();\n" +
//			"        ParseTreeWalker.Default.Walk(new TreeShapeListener(), tree);\n" +
//			"    }\n" +
//			"}\n" +
//			"\n" +
//			"class TreeShapeListener : IParseTreeListener {\n" +
//			"	public void VisitTerminal(ITerminalNode node) { }\n" +
//			"	public void VisitErrorNode(IErrorNode node) { }\n" +
//			"	public void ExitEveryRule(ParserRuleContext ctx) { }\n" +
//			"\n" +
//			"	public void EnterEveryRule(ParserRuleContext ctx) {\n" +
//			"		for (int i = 0; i \\< ctx.ChildCount; i++) {\n" +
//			"			IParseTree parent = ctx.GetChild(i).Parent;\n" +
//			"			if (!(parent is IRuleNode) || ((IRuleNode)parent).RuleContext != ctx) {\n" +
//			"				throw new Exception(\"Invalid parse tree shape detected.\");\n" +
//			"			}\n" +
//			"		}\n" +
//			"	}\n" +
//			"}"
//			);
//        ST createParserST = new ST("        <parserName> parser = new <parserName>(tokens);\n");
//		if ( debug ) {
//			createParserST =
//				new ST(
//				"        <parserName> parser = new <parserName>(tokens);\n" +
//				"        parser.Interpreter.reportAmbiguities = true;\n" +
//                "        parser.AddErrorListener(new DiagnosticErrorListener());\n");
//		}
//		outputFileST.add("createParser", createParserST);
//		outputFileST.add("parserName", parserName);
//		outputFileST.add("lexerName", lexerName);
//		outputFileST.add("parserStartRuleName", parserStartRuleName);
		ST outputFileST = new ST(
				"using Microsoft.VisualStudio.TestTools.UnitTesting;\n" +
				"\n" +
				"namespace Antlr4.Runtime.Test.Generated.<className>._<testName> {\n" +
				"[TestClass] public class Test : BaseTest {\n" +
				"[TestMethod] [TestCategory(\"runtime-suite\")] public void Test<testName>() {\n" +
				"	base.ParserTest(new ParserTestOptions\\< <parserName> > {\n" +
				"		TestName = \"<testName>\",\n" +
				"		Lexer = input => new <lexerName>(input),\n" +
				"		Parser = tokens => new <parserName>(tokens),\n" +
				"		ParserStartRule = parser => parser.<parserStartRuleName>(),\n" +
				"		Debug = <debug>,\n" +
				"		Input = @\"<input>\",\n" +
				"		ExpectedOutput = @\"<expectedOutput>\",\n" +
				"		ExpectedErrors = @\"<expectedErrors>\",\n" +
				"		ShowDFA = <showDFA>\n" +
				"		});\n" +
				"	}\n" +
				"}\n" +
				"}\n");

		outputFileST.add("className", getClass().getSimpleName());
		outputFileST.add("testName", this.name.getMethodName().substring(4));
		outputFileST.add("lexerName", lexerName);
		outputFileST.add("parserName", parserName);
		outputFileST.add("parserStartRuleName", asTemplateString(parserStartRuleName));
		outputFileST.add("debug", debug ? "true" : "false");
		outputFileST.add("input", asTemplateString(this.input));
		outputFileST.add("expectedOutput", asTemplateString(this.expectedOutput));
		outputFileST.add("expectedErrors", asTemplateString(this.expectedErrors));
		outputFileST.add("showDFA", "false");
		writeFile(tmpdir, "Test.cs", outputFileST.render());
	}

	private void writeLexerTestFile(String lexerName, boolean showDFA) {
		ST outputFileST = new ST(
				"using Microsoft.VisualStudio.TestTools.UnitTesting;\n" +
				"\n" +
				"namespace Antlr4.Runtime.Test.Generated.<className>._<testName> {\n" +
				"[TestClass] public class Test : BaseTest {\n" +
				"[TestMethod] [TestCategory(\"runtime-suite\")] public void Test<testName>() {\n" +
				"	base.LexerTest(new LexerTestOptions {\n" +
				"		TestName = \"<testName>\",\n" +
				"		Lexer = input => new <lexerName>(input),\n" +
				"		Input = @\"<input>\",\n" +
				"		ExpectedOutput = @\"<expectedOutput>\",\n" +
				"		ExpectedErrors = @\"<expectedErrors>\",\n" +
				"		ShowDFA = <showDFA>\n" +
				"		});\n" +
				"	}\n" +
				"}\n" +
				"}\n");

		outputFileST.add("className", getClass().getSimpleName());
		outputFileST.add("testName", this.name.getMethodName().substring(4));
		outputFileST.add("lexerName", lexerName);
		outputFileST.add("input", asTemplateString(this.input));
		outputFileST.add("expectedOutput", asTemplateString(this.expectedOutput));
		outputFileST.add("expectedErrors", asTemplateString(this.expectedErrors));
		outputFileST.add("showDFA", showDFA ? "true" : "false");
		writeFile(tmpdir, "Test.cs", outputFileST.render());
	}

	public List<String> realElements(List<String> elements) {
		return elements.subList(Token.MIN_USER_TOKEN_TYPE, elements.size());
	}

	/** Return map sorted by key */
	public <K extends Comparable<? super K>,V> LinkedHashMap<K,V> sort(Map<K,V> data) {
		LinkedHashMap<K,V> dup = new LinkedHashMap<K, V>();
		List<K> keys = new ArrayList<K>();
		keys.addAll(data.keySet());
		Collections.sort(keys);
		for (K k : keys) {
			dup.put(k, data.get(k));
		}
		return dup;
	}
}
