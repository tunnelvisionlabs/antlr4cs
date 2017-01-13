/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.codegen;

import org.antlr.v4.codegen.model.Action;
import org.antlr.v4.codegen.model.CodeBlockForOuterMostAlt;
import org.antlr.v4.codegen.model.OutputModelObject;
import org.antlr.v4.codegen.model.RuleFunction;
import org.antlr.v4.codegen.model.SrcOp;
import org.antlr.v4.codegen.model.decl.CodeBlock;
import org.antlr.v4.codegen.model.decl.Decl;
import org.antlr.v4.runtime.misc.NotNull;
import org.antlr.v4.runtime.misc.Nullable;
import org.antlr.v4.tool.Alternative;
import org.antlr.v4.tool.Grammar;
import org.antlr.v4.tool.Rule;
import org.stringtemplate.v4.ST;
import org.stringtemplate.v4.STGroup;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collection;
import java.util.List;

/** Create output objects for elements *within* rule functions except
 *  buildOutputModel() which builds outer/root model object and any
 *  objects such as RuleFunction that surround elements in rule
 *  functions.
 */
public abstract class DefaultOutputModelFactory extends BlankOutputModelFactory {
	// Interface to outside world
	@NotNull
	public final Grammar g;
	@NotNull
	public final CodeGenerator gen;

	public OutputModelController controller;

	protected DefaultOutputModelFactory(@NotNull CodeGenerator gen) {
		this.gen = gen;
		this.g = gen.g;

		if (gen.getTarget() == null) {
			throw new UnsupportedOperationException("Cannot build an output model without a target.");
		}
	}

	@Override
	public void setController(OutputModelController controller) {
		this.controller = controller;
	}

	@Override
	public OutputModelController getController() {
		return controller;
	}

	@Override
	public List<SrcOp> rulePostamble(RuleFunction function, Rule r) {
		if ( r.namedActions.containsKey("after") || r.namedActions.containsKey("finally") ) {
			// See OutputModelController.buildLeftRecursiveRuleFunction
			// and Parser.exitRule for other places which set stop.
			CodeGenerator gen = getGenerator();
			STGroup codegenTemplates = gen.getTemplates();
			ST setStopTokenAST = codegenTemplates.getInstanceOf("recRuleSetStopToken");
			Action setStopTokenAction = new Action(this, function.ruleCtx, setStopTokenAST);
			List<SrcOp> ops = new ArrayList<SrcOp>(1);
			ops.add(setStopTokenAction);
			return ops;
		}
		return super.rulePostamble(function, r);
	}

	// Convenience methods

	@NotNull
	@Override
	public Grammar getGrammar() { return g; }

	@Override
	public CodeGenerator getGenerator() { return gen; }

	@Override
	public Target getTarget() {
		Target target = getGenerator().getTarget();
		assert target != null;
		return target;
	}

	@Override
	public OutputModelObject getRoot() { return controller.getRoot(); }

	@Override
	public RuleFunction getCurrentRuleFunction() { return controller.getCurrentRuleFunction(); }

	@Override
	public Alternative getCurrentOuterMostAlt() { return controller.getCurrentOuterMostAlt(); }

	@Override
	public CodeBlock getCurrentBlock() { return controller.getCurrentBlock(); }

	@Override
	public CodeBlockForOuterMostAlt getCurrentOuterMostAlternativeBlock() { return controller.getCurrentOuterMostAlternativeBlock(); }

	@Override
	public int getCodeBlockLevel() { return controller.codeBlockLevel; }

	@Override
	public int getTreeLevel() { return controller.treeLevel; }

	// MISC

	@NotNull
	public static List<SrcOp> list(SrcOp... values) {
		return new ArrayList<SrcOp>(Arrays.asList(values));
	}

	@NotNull
	public static List<SrcOp> list(Collection<? extends SrcOp> values) {
		return new ArrayList<SrcOp>(values);
	}

	@Nullable
	public Decl getCurrentDeclForName(String name) {
		if ( getCurrentBlock().locals==null ) return null;
		for (Decl d : getCurrentBlock().locals.elements()) {
			if ( d.name.equals(name) ) return d;
		}
		return null;
	}

}
