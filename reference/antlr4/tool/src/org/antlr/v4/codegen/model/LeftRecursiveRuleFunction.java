/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.codegen.model;

import org.antlr.v4.codegen.CodeGenerator;
import org.antlr.v4.codegen.OutputModelFactory;
import org.antlr.v4.codegen.model.decl.RuleContextDecl;
import org.antlr.v4.codegen.model.decl.RuleContextListDecl;
import org.antlr.v4.codegen.model.decl.StructDecl;
import org.antlr.v4.parse.ANTLRParser;
import org.antlr.v4.runtime.misc.Tuple2;
import org.antlr.v4.tool.LeftRecursiveRule;
import org.antlr.v4.tool.Rule;
import org.antlr.v4.tool.ast.GrammarAST;

public class LeftRecursiveRuleFunction extends RuleFunction {
	public LeftRecursiveRuleFunction(OutputModelFactory factory, LeftRecursiveRule r) {
		super(factory, r);

		// Since we delete x=lr, we have to manually add decls for all labels
		// on left-recur refs to proper structs
		for (Tuple2<GrammarAST,String> pair : r.leftRecursiveRuleRefLabels) {
			GrammarAST idAST = pair.getItem1();
			String altLabel = pair.getItem2();
			String label = idAST.getText();
			GrammarAST rrefAST = (GrammarAST)idAST.getParent().getChild(1);
			if ( rrefAST.getType() == ANTLRParser.RULE_REF ) {
				Rule targetRule = factory.getGrammar().getRule(rrefAST.getText());
				String ctxName = factory.getTarget().getRuleFunctionContextStructName(targetRule);
				RuleContextDecl d;
				if (idAST.getParent().getType() == ANTLRParser.ASSIGN) {
					d = new RuleContextDecl(factory, label, ctxName);
				}
				else {
					d = new RuleContextListDecl(factory, label, ctxName);
				}

				StructDecl struct = ruleCtx;
				if ( altLabelCtxs!=null ) {
					StructDecl s = altLabelCtxs.get(altLabel);
					if ( s!=null ) struct = s; // if alt label, use subctx
				}
				struct.addDecl(d); // stick in overall rule's ctx
			}
		}
	}
}
