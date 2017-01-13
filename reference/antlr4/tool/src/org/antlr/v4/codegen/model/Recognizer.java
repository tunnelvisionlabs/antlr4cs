/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
package org.antlr.v4.codegen.model;

import org.antlr.v4.codegen.CodeGenerator;
import org.antlr.v4.codegen.OutputModelFactory;
import org.antlr.v4.codegen.model.chunk.ActionChunk;
import org.antlr.v4.codegen.model.chunk.ActionText;
import org.antlr.v4.tool.Grammar;
import org.antlr.v4.tool.Rule;

import java.io.File;
import java.util.Arrays;
import java.util.Collection;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;

public abstract class Recognizer extends OutputModelObject {
	public String name;
	public String grammarName;
	public String grammarFileName;
	public Map<String,Integer> tokens;

	/**
	 * @deprecated This field is provided only for compatibility with code
	 * generation targets which have not yet been updated to use
	 * {@link #literalNames} and {@link #symbolicNames}.
	 */
	@Deprecated
	public List<String> tokenNames;

	public List<String> literalNames;
	public List<String> symbolicNames;
	public Set<String> ruleNames;
	public Collection<Rule> rules;
	@ModelElement public ActionChunk superClass;
	public boolean abstractRecognizer;

	@ModelElement public SerializedATN atn;
	@ModelElement public LinkedHashMap<Rule, RuleSempredFunction> sempredFuncs =
		new LinkedHashMap<Rule, RuleSempredFunction>();

	public Recognizer(OutputModelFactory factory) {
		super(factory);

		Grammar g = factory.getGrammar();
		grammarFileName = new File(g.fileName).getName();
		grammarName = g.name;
		name = g.getRecognizerName();
		tokens = new LinkedHashMap<String,Integer>();
		for (Map.Entry<String, Integer> entry : g.tokenNameToTypeMap.entrySet()) {
			Integer ttype = entry.getValue();
			if ( ttype>0 ) {
				tokens.put(entry.getKey(), ttype);
			}
		}

		ruleNames = g.rules.keySet();
		rules = g.rules.values();
		atn = new SerializedATN(factory, g.atn, Arrays.asList(g.getRuleNames()));
		if (g.getOptionString("superClass") != null) {
			superClass = new ActionText(null, g.getOptionString("superClass"));
		}
		else {
			superClass = null;
		}

		tokenNames = translateTokenStringsToTarget(g.getTokenDisplayNames(), factory);
		literalNames = translateTokenStringsToTarget(g.getTokenLiteralNames(), factory);
		symbolicNames = translateTokenStringsToTarget(g.getTokenSymbolicNames(), factory);
		abstractRecognizer = g.isAbstract();
	}

	protected static List<String> translateTokenStringsToTarget(String[] tokenStrings, OutputModelFactory factory) {
		String[] result = tokenStrings.clone();
		for (int i = 0; i < tokenStrings.length; i++) {
			result[i] = translateTokenStringToTarget(tokenStrings[i], factory);
		}

		int lastTrueEntry = result.length - 1;
		while (lastTrueEntry >= 0 && result[lastTrueEntry] == null) {
			lastTrueEntry --;
		}

		if (lastTrueEntry < result.length - 1) {
			result = Arrays.copyOf(result, lastTrueEntry + 1);
		}

		return Arrays.asList(result);
	}

	protected static String translateTokenStringToTarget(String tokenName, OutputModelFactory factory) {
		if (tokenName == null) {
			return null;
		}

		if (tokenName.charAt(0) == '\'') {
			boolean addQuotes = false;
			String targetString =
				factory.getTarget().getTargetStringLiteralFromANTLRStringLiteral(factory.getGenerator(), tokenName, addQuotes);
			return "\"'" + targetString + "'\"";
		}
		else {
			return factory.getTarget().getTargetStringLiteralFromString(tokenName, true);
		}
	}

}
