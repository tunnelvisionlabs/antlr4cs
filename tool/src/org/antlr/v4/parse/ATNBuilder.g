/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

tree grammar ATNBuilder;
options {
	language     = Java;
	tokenVocab   = ANTLRParser;
	ASTLabelType = GrammarAST;
//	filter 	     = true;
}

// Include the copyright in this source and also the generated source
@header {
/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
package org.antlr.v4.parse;
import org.antlr.v4.tool.*;
import org.antlr.v4.tool.ast.*;
import org.antlr.v4.automata.ATNFactory;
}

@members {
    ATNFactory factory;
    public ATNBuilder(TreeNodeStream input, ATNFactory factory) {
    	this(input);
    	this.factory = factory;
    }
}

dummy : block[null] ; // avoid error about no start rule

ruleBlock[GrammarAST ebnfRoot] returns [ATNFactory.Handle p]
@init {
    List<ATNFactory.Handle> alts = new ArrayList<ATNFactory.Handle>();
    int alt = 1;
    factory.setCurrentOuterAlt(alt);
}
    :	^(BLOCK
            (^(OPTIONS .*))?
            (   a=alternative
                {alts.add($a.p); factory.setCurrentOuterAlt(++alt);}
            )+
        )
    	{$p = factory.block((BlockAST)$BLOCK, ebnfRoot, alts);}
    ;

block[GrammarAST ebnfRoot] returns [ATNFactory.Handle p]
@init {List<ATNFactory.Handle> alts = new ArrayList<ATNFactory.Handle>();}
    :	^(BLOCK (^(OPTIONS .*))? (a=alternative {alts.add($a.p);})+)
    	{$p = factory.block((BlockAST)$BLOCK, ebnfRoot, alts);}
    ;

alternative returns [ATNFactory.Handle p]
@init {List<ATNFactory.Handle> els = new ArrayList<ATNFactory.Handle>();}
    :	^(LEXER_ALT_ACTION a=alternative lexerCommands)
        {$p = factory.lexerAltCommands($a.p,$lexerCommands.p);}
    |	^(ALT elementOptions? EPSILON)							{$p = factory.epsilon($EPSILON);}
    |   ^(ALT elementOptions? (e=element {els.add($e.p);})+)	{$p = factory.alt(els);}
    ;

lexerCommands returns [ATNFactory.Handle p]
@init {List<ATNFactory.Handle> cmds = new ArrayList<ATNFactory.Handle>();}
    :   (c=lexerCommand {if ($c.cmd != null) cmds.add($c.cmd);})+
        {
        $p = factory.alt(cmds);
        }
    ;

lexerCommand returns [ATNFactory.Handle cmd]
	:	^(LEXER_ACTION_CALL ID lexerCommandExpr)
        {$cmd = factory.lexerCallCommand($ID, $lexerCommandExpr.start);}
	|	ID
        {$cmd = factory.lexerCommand($ID);}
	;

lexerCommandExpr
	:	ID
	|	INT
	;

element returns [ATNFactory.Handle p]
	:	labeledElement				{$p = $labeledElement.p;}
	|	atom						{$p = $atom.p;}
	|	subrule						{$p = $subrule.p;}
	|   ACTION						{$p = factory.action((ActionAST)$ACTION);}
	|   SEMPRED						{$p = factory.sempred((PredAST)$SEMPRED);}
	|   ^(ACTION .)					{$p = factory.action((ActionAST)$ACTION);}
	|   ^(SEMPRED .)				{$p = factory.sempred((PredAST)$SEMPRED);}
    |	^(NOT b=blockSet[true])		{$p = $b.p;}
    |	LEXER_CHAR_SET					{$p = factory.charSetLiteral($start);}
	;

astOperand returns [ATNFactory.Handle p]
	:	atom						{$p = $atom.p;}
	|	^(NOT blockSet[true])		{$p = $blockSet.p;}
	;

labeledElement returns [ATNFactory.Handle p]
	:	^(ASSIGN ID element)	    {$p = factory.label($element.p);}
	|	^(PLUS_ASSIGN ID element)   {$p = factory.listLabel($element.p);}
	;

subrule returns [ATNFactory.Handle p]
	:	^(OPTIONAL block[$start])			{$p = $block.p;}
	|	^(CLOSURE block[$start])			{$p = $block.p;}
	|	^(POSITIVE_CLOSURE block[$start])	{$p = $block.p;}
	| 	block[null] 						{$p = $block.p;}
    ;

blockSet[boolean invert] returns [ATNFactory.Handle p]
@init {List<GrammarAST> alts = new ArrayList<GrammarAST>();}
	:	^(SET (setElement {alts.add($setElement.start);})+) {$p = factory.set($start, alts, $invert);}
	;

/** Don't combine with atom otherwise it will build spurious ATN nodes */
setElement
	:	^(STRING_LITERAL .)
	|	^(TOKEN_REF .)
	|	STRING_LITERAL
	|	TOKEN_REF
	|	^(RANGE a=STRING_LITERAL b=STRING_LITERAL)
    |   LEXER_CHAR_SET
	;

atom returns [ATNFactory.Handle p]
	:	range					{$p = $range.p;}
	|	^(DOT ID terminal)		{$p = $terminal.p;}
	|	^(DOT ID ruleref)		{$p = $ruleref.p;}
    |	^(WILDCARD .)			{$p = factory.wildcard($start);}
    |	WILDCARD				{$p = factory.wildcard($start);}
    |	blockSet[false]			{$p = $blockSet.p;}
    |   terminal				{$p = $terminal.p;}
    |   ruleref					{$p = $ruleref.p;}
    ;

ruleref returns [ATNFactory.Handle p]
    :	^(RULE_REF ARG_ACTION? ^(ELEMENT_OPTIONS .*))		{$p = factory.ruleRef($RULE_REF);}
    |	^(RULE_REF ARG_ACTION?)								{$p = factory.ruleRef($RULE_REF);}
    |	RULE_REF											{$p = factory.ruleRef($RULE_REF);}
    ;

range returns [ATNFactory.Handle p]
    : ^(RANGE a=STRING_LITERAL b=STRING_LITERAL) {$p = factory.range($a,$b);}
    ;

terminal returns [ATNFactory.Handle p]
    :  ^(STRING_LITERAL .)			{$p = factory.stringLiteral((TerminalAST)$start);}
    |	STRING_LITERAL				{$p = factory.stringLiteral((TerminalAST)$start);}
    |	^(TOKEN_REF ARG_ACTION .)	{$p = factory.tokenRef((TerminalAST)$start);}
    |	^(TOKEN_REF .)				{$p = factory.tokenRef((TerminalAST)$start);}
    |	TOKEN_REF					{$p = factory.tokenRef((TerminalAST)$start);}
    ;

elementOptions
	:	^(ELEMENT_OPTIONS elementOption*)
	;

elementOption
	:	ID
	|	^(ASSIGN ID ID)
	|	^(ASSIGN ID STRING_LITERAL)
	|	^(ASSIGN ID ACTION)
	|	^(ASSIGN ID INT)
	;
