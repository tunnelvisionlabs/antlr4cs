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

namespace Antlr4.Tool
{
    /**
     * A complex enumeration of all the error messages that the tool can issue.
     * <p>
     * When adding error messages, also add a description of the message to the
     * Wiki with a location under the Wiki page
     * <a href="http://www.antlr.org/wiki/display/ANTLR4/Errors+Reported+by+the+ANTLR+Tool">Errors Reported by the ANTLR Tool</a>.</p>
     *
     * @author Jim Idle &lt;jimi@temporal-wave.com&gt;, Terence Parr
     * @since 4.0
     */
    public sealed class ErrorType {
        /*
         * Tool errors
         */

        /**
         * Compiler Error 1.
         *
         * <p>cannot write file '<em>filename</em>': <em>reason</em></p>
         */
        public static readonly ErrorType CANNOT_WRITE_FILE = new ErrorType(nameof(CANNOT_WRITE_FILE), 1, "cannot write file '<arg>': <arg2>", ErrorSeverity.ERROR);
        /**
         * Compiler Error 2.
         *
         * <p>unknown command-line option '<em>option</em>'</p>
         */
        public static readonly ErrorType INVALID_CMDLINE_ARG = new ErrorType(nameof(INVALID_CMDLINE_ARG), 2, "unknown command-line option '<arg>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 3.
         *
         * <p>cannot find tokens file '<em>filename</em>' given for '<em>arg2</em>'</p>
         */
        public static readonly ErrorType CANNOT_FIND_TOKENS_FILE_GIVEN_ON_CMDLINE = new ErrorType(nameof(CANNOT_FIND_TOKENS_FILE_GIVEN_ON_CMDLINE), 3, "cannot find tokens file '<arg>' given for '<arg2>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 4.
         *
         * <p>error reading tokens file '<em>filename</em>': <em>reason</em></p>
         */
        public static readonly ErrorType ERROR_READING_TOKENS_FILE = new ErrorType(nameof(ERROR_READING_TOKENS_FILE), 4, "error reading tokens file '<arg>': <arg2>", ErrorSeverity.ERROR);
        /**
         * Compiler Error 5.
         *
         * <p>directory not found: <em>directory</em></p>
         */
        public static readonly ErrorType DIR_NOT_FOUND = new ErrorType(nameof(DIR_NOT_FOUND), 5, "directory not found: <arg>", ErrorSeverity.ERROR);
        /**
         * Compiler Error 6.
         *
         * <p>output directory is a file: <em>filename</em></p>
         */
        public static readonly ErrorType OUTPUT_DIR_IS_FILE = new ErrorType(nameof(OUTPUT_DIR_IS_FILE), 6, "output directory is a file: <arg>", ErrorSeverity.ERROR);
        /**
         * Compiler Error 7.
         *
         * <p>cannot find or open file: <em>filename</em></p>
         */
        public static readonly ErrorType CANNOT_OPEN_FILE = new ErrorType(nameof(CANNOT_OPEN_FILE), 7, "cannot find or open file: <arg><if(exception&&verbose)>; reason: <exception><endif>", ErrorSeverity.ERROR);
        /**
         * Compiler Error 8.
         *
         * <p>
         * grammar name '<em>name</em>' and file name '<em>filename</em>' differ</p>
         */
        public static readonly ErrorType FILE_AND_GRAMMAR_NAME_DIFFER = new ErrorType(nameof(FILE_AND_GRAMMAR_NAME_DIFFER), 8, "grammar name '<arg>' and file name '<arg2>' differ", ErrorSeverity.ERROR);
        /**
         * Compiler Error 9.
         *
         * <p>invalid {@code -Dname=value} syntax: '<em>syntax</em>'</p>
         */
        public static readonly ErrorType BAD_OPTION_SET_SYNTAX = new ErrorType(nameof(BAD_OPTION_SET_SYNTAX), 9, "invalid -Dname=value syntax: '<arg>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 10.
         *
         * <p>warning treated as error</p>
         */
        public static readonly ErrorType WARNING_TREATED_AS_ERROR = new ErrorType(nameof(WARNING_TREATED_AS_ERROR), 10, "warning treated as error", ErrorSeverity.ERROR_ONE_OFF);
        /**
         * Compiler Error 11.
         *
         * <p>error reading imported grammar '<em>arg</em>' referenced in '<em>arg2</em>'</p>
         */
        public static readonly ErrorType ERROR_READING_IMPORTED_GRAMMAR = new ErrorType(nameof(ERROR_READING_IMPORTED_GRAMMAR), 11, "error reading imported grammar '<arg>' referenced in '<arg2>'", ErrorSeverity.ERROR);

        /**
         * Compiler Error 20.
         *
         * <p>internal error: <em>message</em></p>
         */
        public static readonly ErrorType INTERNAL_ERROR = new ErrorType(nameof(INTERNAL_ERROR), 20, "internal error: <arg> <arg2><if(exception&&verbose)>: <exception>" +
                       "<stackTrace; separator=\"\\n\"><endif>", ErrorSeverity.ERROR);
        /**
         * Compiler Error 21.
         *
         * <p>.tokens file syntax error <em>filename</em>: <em>message</em></p>
         */
        public static readonly ErrorType TOKENS_FILE_SYNTAX_ERROR = new ErrorType(nameof(TOKENS_FILE_SYNTAX_ERROR), 21, ".tokens file syntax error <arg>:<arg2>", ErrorSeverity.ERROR);
        /**
         * Compiler Warning 22.
         *
         * <p>template error: <em>message</em></p>
         */
        public static readonly ErrorType STRING_TEMPLATE_WARNING = new ErrorType(nameof(STRING_TEMPLATE_WARNING), 22, "template error: <arg> <arg2><if(exception&&verbose)>: <exception>" +
                       "<stackTrace; separator=\"\\n\"><endif>", ErrorSeverity.WARNING);

        /*
         * Code generation errors
         */

        /**
         * Compiler Error 30.
         *
         * <p>can't find code generation templates: <em>group</em></p>
         */
        public static readonly ErrorType MISSING_CODE_GEN_TEMPLATES = new ErrorType(nameof(MISSING_CODE_GEN_TEMPLATES), 30, "can't find code generation templates: <arg>", ErrorSeverity.ERROR);
        /**
         * Compiler Error 31.
         *
         * <p>
         * ANTLR cannot generate '<em>language</em>' code as of version
         * <em>version</em></p>
         */
        public static readonly ErrorType CANNOT_CREATE_TARGET_GENERATOR = new ErrorType(nameof(CANNOT_CREATE_TARGET_GENERATOR), 31, "ANTLR cannot generate '<arg>' code as of version " + AntlrTool.VERSION, ErrorSeverity.ERROR);
        /**
         * Compiler Error 32.
         *
         * <p>
         * code generation template '<em>template</em>' has missing, misnamed, or
         * incomplete arg list; missing '<em>field</em>'</p>
         */
        public static readonly ErrorType CODE_TEMPLATE_ARG_ISSUE = new ErrorType(nameof(CODE_TEMPLATE_ARG_ISSUE), 32, "code generation template '<arg>' has missing, misnamed, or incomplete arg list; missing '<arg2>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 33.
         *
         * <p>missing code generation template '<em>template</em>'</p>
         */
        public static readonly ErrorType CODE_GEN_TEMPLATES_INCOMPLETE = new ErrorType(nameof(CODE_GEN_TEMPLATES_INCOMPLETE), 33, "missing code generation template '<arg>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 34.
         *
         * <p>
         * no mapping to template name for output model class '<em>class</em>'</p>
         */
        public static readonly ErrorType NO_MODEL_TO_TEMPLATE_MAPPING = new ErrorType(nameof(NO_MODEL_TO_TEMPLATE_MAPPING), 34, "no mapping to template name for output model class '<arg>'", ErrorSeverity.ERROR);

        /*
         * Grammar errors
         */

        /**
         * Compiler Error 50.
         *
         * <p>syntax error: <em>message</em></p>
         */
        public static readonly ErrorType SYNTAX_ERROR = new ErrorType(nameof(SYNTAX_ERROR), 50, "syntax error: <arg>", ErrorSeverity.ERROR);
        /**
         * Compiler Error 51.
         *
         * <p>rule '<em>rule</em>' redefinition; previous at line <em>line</em></p>
         */
        public static readonly ErrorType RULE_REDEFINITION = new ErrorType(nameof(RULE_REDEFINITION), 51, "rule '<arg>' redefinition; previous at line <arg2>", ErrorSeverity.ERROR);
        /**
         * Compiler Error 52.
         *
         * <p>lexer rule '<em>rule</em>' not allowed in parser</p>
         */
        public static readonly ErrorType LEXER_RULES_NOT_ALLOWED = new ErrorType(nameof(LEXER_RULES_NOT_ALLOWED), 52, "lexer rule '<arg>' not allowed in parser", ErrorSeverity.ERROR);
        /**
         * Compiler Error 53.
         *
         * <p>parser rule '<em>rule</em>' not allowed in lexer</p>
         */
        public static readonly ErrorType PARSER_RULES_NOT_ALLOWED = new ErrorType(nameof(PARSER_RULES_NOT_ALLOWED), 53, "parser rule '<arg>' not allowed in lexer", ErrorSeverity.ERROR);
        /**
         * Compiler Error 54.
         *
         * <p>
         * repeated grammar prequel spec ({@code options}, {@code tokens}, or
         * {@code import}); please merge</p>
         */
        public static readonly ErrorType REPEATED_PREQUEL = new ErrorType(nameof(REPEATED_PREQUEL), 54, "repeated grammar prequel spec (options, tokens, or import); please merge", ErrorSeverity.ERROR);
        /**
         * Compiler Error 56.
         *
         * <p>reference to undefined rule: <em>rule</em></p>
         *
         * @see #PARSER_RULE_REF_IN_LEXER_RULE
         */
        public static readonly ErrorType UNDEFINED_RULE_REF = new ErrorType(nameof(UNDEFINED_RULE_REF), 56, "reference to undefined rule: <arg>", ErrorSeverity.ERROR);
        /**
         * Compiler Error 57.
         *
         * <p>
         * reference to undefined rule '<em>rule</em>' in non-local ref
         * '<em>reference</em>'</p>
         */
        public static readonly ErrorType UNDEFINED_RULE_IN_NONLOCAL_REF = new ErrorType(nameof(UNDEFINED_RULE_IN_NONLOCAL_REF), 57, "reference to undefined rule '<arg>' in non-local ref '<arg3>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 60.
         *
         * <p>token names must start with an uppercase letter: <em>name</em></p>
         */
        public static readonly ErrorType TOKEN_NAMES_MUST_START_UPPER = new ErrorType(nameof(TOKEN_NAMES_MUST_START_UPPER), 60, "token names must start with an uppercase letter: <arg>", ErrorSeverity.ERROR);
        /**
         * Compiler Error 63.
         *
         * <p>
         * unknown attribute reference '<em>attribute</em>' in
         * '<em>expression</em>'</p>
         */
        public static readonly ErrorType UNKNOWN_SIMPLE_ATTRIBUTE = new ErrorType(nameof(UNKNOWN_SIMPLE_ATTRIBUTE), 63, "unknown attribute reference '<arg>' in '<arg2>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 64.
         *
         * <p>
         * parameter '<em>parameter</em>' of rule '<em>rule</em>' is not accessible
         * in this scope: <em>expression</em></p>
         */
        public static readonly ErrorType INVALID_RULE_PARAMETER_REF = new ErrorType(nameof(INVALID_RULE_PARAMETER_REF), 64, "parameter '<arg>' of rule '<arg2>' is not accessible in this scope: <arg3>", ErrorSeverity.ERROR);
        /**
         * Compiler Error 65.
         *
         * <p>
         * unknown attribute '<em>attribute</em>' for rule '<em>rule</em>' in
         * '<em>expression</em>'</p>
         */
        public static readonly ErrorType UNKNOWN_RULE_ATTRIBUTE = new ErrorType(nameof(UNKNOWN_RULE_ATTRIBUTE), 65, "unknown attribute '<arg>' for rule '<arg2>' in '<arg3>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 66.
         *
         * <p>
         * attribute '<em>attribute</em>' isn't a valid property in
         * '<em>expression</em>'</p>
         */
        public static readonly ErrorType UNKNOWN_ATTRIBUTE_IN_SCOPE = new ErrorType(nameof(UNKNOWN_ATTRIBUTE_IN_SCOPE), 66, "attribute '<arg>' isn't a valid property in '<arg2>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 67.
         *
         * <p>
         * missing attribute access on rule reference '<em>rule</em>' in
         * '<em>expression</em>'</p>
         */
        public static readonly ErrorType ISOLATED_RULE_REF = new ErrorType(nameof(ISOLATED_RULE_REF), 67, "missing attribute access on rule reference '<arg>' in '<arg2>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 69.
         *
         * <p>label '<em>label</em>' conflicts with rule with same name</p>
         */
        public static readonly ErrorType LABEL_CONFLICTS_WITH_RULE = new ErrorType(nameof(LABEL_CONFLICTS_WITH_RULE), 69, "label '<arg>' conflicts with rule with same name", ErrorSeverity.ERROR);
        /**
         * Compiler Error 70.
         *
         * <p>label '<em>label</em>' conflicts with token with same name</p>
         */
        public static readonly ErrorType LABEL_CONFLICTS_WITH_TOKEN = new ErrorType(nameof(LABEL_CONFLICTS_WITH_TOKEN), 70, "label '<arg>' conflicts with token with same name", ErrorSeverity.ERROR);
        /**
         * Compiler Error 72.
         *
         * <p>label '<em>label</em>' conflicts with parameter with same name</p>
         */
        public static readonly ErrorType LABEL_CONFLICTS_WITH_ARG = new ErrorType(nameof(LABEL_CONFLICTS_WITH_ARG), 72, "label '<arg>' conflicts with parameter with same name", ErrorSeverity.ERROR);
        /**
         * Compiler Error 73.
         *
         * <p>label '<em>label</em>' conflicts with return value with same name</p>
         */
        public static readonly ErrorType LABEL_CONFLICTS_WITH_RETVAL = new ErrorType(nameof(LABEL_CONFLICTS_WITH_RETVAL), 73, "label '<arg>' conflicts with return value with same name", ErrorSeverity.ERROR);
        /**
         * Compiler Error 74.
         *
         * <p>label '<em>label</em>' conflicts with local with same name</p>
         */
        public static readonly ErrorType LABEL_CONFLICTS_WITH_LOCAL = new ErrorType(nameof(LABEL_CONFLICTS_WITH_LOCAL), 74, "label '<arg>' conflicts with local with same name", ErrorSeverity.ERROR);
        /**
         * Compiler Error 75.
         *
         * <p>
         * label '<em>label</em>' type mismatch with previous definition:
         * <em>message</em></p>
         */
        public static readonly ErrorType LABEL_TYPE_CONFLICT = new ErrorType(nameof(LABEL_TYPE_CONFLICT), 75, "label '<arg>' type mismatch with previous definition: <arg2>", ErrorSeverity.ERROR);
        /**
         * Compiler Error 76.
         *
         * <p>
         * return value '<em>name</em>' conflicts with parameter with same name</p>
         */
        public static readonly ErrorType RETVAL_CONFLICTS_WITH_ARG = new ErrorType(nameof(RETVAL_CONFLICTS_WITH_ARG), 76, "return value '<arg>' conflicts with parameter with same name", ErrorSeverity.ERROR);
        /**
         * Compiler Error 79.
         *
         * <p>missing arguments(s) on rule reference: <em>rule</em></p>
         */
        public static readonly ErrorType MISSING_RULE_ARGS = new ErrorType(nameof(MISSING_RULE_ARGS), 79, "missing arguments(s) on rule reference: <arg>", ErrorSeverity.ERROR);
        /**
         * Compiler Error 80.
         *
         * <p>rule '<em>rule</em>' has no defined parameters</p>
         */
        public static readonly ErrorType RULE_HAS_NO_ARGS = new ErrorType(nameof(RULE_HAS_NO_ARGS), 80, "rule '<arg>' has no defined parameters", ErrorSeverity.ERROR);
        /**
         * Compiler Warning 83.
         *
         * <p>unsupported option '<em>option</em>'</p>
         */
        public static readonly ErrorType ILLEGAL_OPTION = new ErrorType(nameof(ILLEGAL_OPTION), 83, "unsupported option '<arg>'", ErrorSeverity.WARNING);
        /**
         * Compiler Warning 84.
         *
         * <p>unsupported option value '<em>name</em>=<em>value</em>'</p>
         */
        public static readonly ErrorType ILLEGAL_OPTION_VALUE = new ErrorType(nameof(ILLEGAL_OPTION_VALUE), 84, "unsupported option value '<arg>=<arg2>'", ErrorSeverity.WARNING);
        /**
         * Compiler Error 94.
         *
         * <p>redefinition of '<em>action</em>' action</p>
         */
        public static readonly ErrorType ACTION_REDEFINITION = new ErrorType(nameof(ACTION_REDEFINITION), 94, "redefinition of '<arg>' action", ErrorSeverity.ERROR);
        /**
         * Compiler Error 99.
         *
         * <p>This error may take any of the following forms.</p>
         *
         * <ul>
         * <li>grammar '<em>grammar</em>' has no rules</li>
         * <li>implicitly generated grammar '<em>grammar</em>' has no rules</li>
         * </ul>
         */
        public static readonly ErrorType NO_RULES = new ErrorType(nameof(NO_RULES), 99, "<if(arg2.implicitLexerOwner)>implicitly generated <endif>grammar '<arg>' has no rules", ErrorSeverity.ERROR);
        /**
         * Compiler Error 105.
         *
         * <p>
         * reference to undefined grammar in rule reference:
         * <em>grammar</em>.<em>rule</em></p>
         */
        public static readonly ErrorType NO_SUCH_GRAMMAR_SCOPE = new ErrorType(nameof(NO_SUCH_GRAMMAR_SCOPE), 105, "reference to undefined grammar in rule reference: <arg>.<arg2>", ErrorSeverity.ERROR);
        /**
         * Compiler Error 106.
         *
         * <p>rule '<em>rule</em>' is not defined in grammar '<em>grammar</em>'</p>
         */
        public static readonly ErrorType NO_SUCH_RULE_IN_SCOPE = new ErrorType(nameof(NO_SUCH_RULE_IN_SCOPE), 106, "rule '<arg2>' is not defined in grammar '<arg>'", ErrorSeverity.ERROR);
        /**
         * Compiler Warning 108.
         *
         * <p>token name '<em>Token</em>' is already defined</p>
         */
        public static readonly ErrorType TOKEN_NAME_REASSIGNMENT = new ErrorType(nameof(TOKEN_NAME_REASSIGNMENT), 108, "token name '<arg>' is already defined", ErrorSeverity.WARNING);
        /**
         * Compiler Warning 109.
         *
         * <p>options ignored in imported grammar '<em>grammar</em>'</p>
         */
        public static readonly ErrorType OPTIONS_IN_DELEGATE = new ErrorType(nameof(OPTIONS_IN_DELEGATE), 109, "options ignored in imported grammar '<arg>'", ErrorSeverity.WARNING);
        /**
         * Compiler Error 110.
         *
         * <p>
         * can't find or load grammar <em>grammar</em></p>
         */
        public static readonly ErrorType CANNOT_FIND_IMPORTED_GRAMMAR = new ErrorType(nameof(CANNOT_FIND_IMPORTED_GRAMMAR), 110, "can't find or load grammar '<arg>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 111.
         *
         * <p>
         * <em>grammartype</em> grammar '<em>grammar1</em>' cannot import
         * <em>grammartype</em> grammar '<em>grammar2</em>'</p>
         */
        public static readonly ErrorType INVALID_IMPORT = new ErrorType(nameof(INVALID_IMPORT), 111, "<arg.typeString> grammar '<arg.name>' cannot import <arg2.typeString> grammar '<arg2.name>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 113.
         *
         * <p>
         * <em>grammartype</em> grammar '<em>grammar1</em>' and imported
         * <em>grammartype</em> grammar '<em>grammar2</em>' both generate
         * '<em>recognizer</em>'</p>
         */
        public static readonly ErrorType IMPORT_NAME_CLASH = new ErrorType(nameof(IMPORT_NAME_CLASH), 113, "<arg.typeString> grammar '<arg.name>' and imported <arg2.typeString> grammar '<arg2.name>' both generate '<arg2.recognizerName>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 160.
         *
         * <p>cannot find tokens file '<em>filename</em>'</p>
         */
        public static readonly ErrorType CANNOT_FIND_TOKENS_FILE_REFD_IN_GRAMMAR = new ErrorType(nameof(CANNOT_FIND_TOKENS_FILE_REFD_IN_GRAMMAR), 160, "cannot find tokens file '<arg>'", ErrorSeverity.ERROR);
        /**
         * Compiler Warning 118.
         *
         * <p>
         * all operators of alt '<em>alt</em>' of left-recursive rule must have same
         * associativity</p>
         *
         * @deprecated This warning is no longer applicable with the current syntax for specifying associativity.
         */
        [System.Obsolete]
        public static readonly ErrorType ALL_OPS_NEED_SAME_ASSOC = new ErrorType(nameof(ALL_OPS_NEED_SAME_ASSOC), 118, "all operators of alt '<arg>' of left-recursive rule must have same associativity", ErrorSeverity.WARNING);
        /**
         * Compiler Error 119.
         *
         * <p>
         * The following sets of rules are mutually left-recursive
         * <em>[rules]</em></p>
         */
        public static readonly ErrorType LEFT_RECURSION_CYCLES = new ErrorType(nameof(LEFT_RECURSION_CYCLES), 119, "The following sets of rules are mutually left-recursive <arg:{c| [<c:{r|<r.name>}; separator=\", \">]}; separator=\" and \">", ErrorSeverity.ERROR);
        /**
         * Compiler Error 120.
         *
         * <p>lexical modes are only allowed in lexer grammars</p>
         */
        public static readonly ErrorType MODE_NOT_IN_LEXER = new ErrorType(nameof(MODE_NOT_IN_LEXER), 120, "lexical modes are only allowed in lexer grammars", ErrorSeverity.ERROR);
        /**
         * Compiler Error 121.
         *
         * <p>cannot find an attribute name in attribute declaration</p>
         */
        public static readonly ErrorType CANNOT_FIND_ATTRIBUTE_NAME_IN_DECL = new ErrorType(nameof(CANNOT_FIND_ATTRIBUTE_NAME_IN_DECL), 121, "cannot find an attribute name in attribute declaration", ErrorSeverity.ERROR);
        /**
         * Compiler Error 122.
         *
         * <p>rule '<em>rule</em>': must label all alternatives or none</p>
         */
        public static readonly ErrorType RULE_WITH_TOO_FEW_ALT_LABELS = new ErrorType(nameof(RULE_WITH_TOO_FEW_ALT_LABELS), 122, "rule '<arg>': must label all alternatives or none", ErrorSeverity.ERROR);
        /**
         * Compiler Error 123.
         *
         * <p>
         * rule alt label '<em>label</em>' redefined in rule '<em>rule1</em>',
         * originally in rule '<em>rule2</em>'</p>
         */
        public static readonly ErrorType ALT_LABEL_REDEF = new ErrorType(nameof(ALT_LABEL_REDEF), 123, "rule alt label '<arg>' redefined in rule '<arg2>', originally in rule '<arg3>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 124.
         *
         * <p>
         * rule alt label '<em>label</em>' conflicts with rule '<em>rule</em>'</p>
         */
        public static readonly ErrorType ALT_LABEL_CONFLICTS_WITH_RULE = new ErrorType(nameof(ALT_LABEL_CONFLICTS_WITH_RULE), 124, "rule alt label '<arg>' conflicts with rule '<arg2>'", ErrorSeverity.ERROR);
        /**
         * Compiler Warning 125.
         *
         * <p>implicit definition of token '<em>Token</em>' in parser</p>
         */
        public static readonly ErrorType IMPLICIT_TOKEN_DEFINITION = new ErrorType(nameof(IMPLICIT_TOKEN_DEFINITION), 125, "implicit definition of token '<arg>' in parser", ErrorSeverity.WARNING);
        /**
         * Compiler Error 126.
         *
         * <p>
         * cannot create implicit token for string literal in non-combined grammar:
         * <em>literal</em></p>
         */
        public static readonly ErrorType IMPLICIT_STRING_DEFINITION = new ErrorType(nameof(IMPLICIT_STRING_DEFINITION), 126, "cannot create implicit token for string literal in non-combined grammar: <arg>", ErrorSeverity.ERROR);
        /**
         * Compiler Error 128.
         *
         * <p>
         * attribute references not allowed in lexer actions:
         * <em>expression</em></p>
         */
        public static readonly ErrorType ATTRIBUTE_IN_LEXER_ACTION = new ErrorType(nameof(ATTRIBUTE_IN_LEXER_ACTION), 128, "attribute references not allowed in lexer actions: $<arg>", ErrorSeverity.ERROR);
        /**
         * Compiler Error 130.
         *
         * <p>label '<em>label</em>' assigned to a block which is not a set</p>
         */
        public static readonly ErrorType LABEL_BLOCK_NOT_A_SET = new ErrorType(nameof(LABEL_BLOCK_NOT_A_SET), 130, "label '<arg>' assigned to a block which is not a set", ErrorSeverity.ERROR);
        /**
         * Compiler Warning 131.
         *
         * <p>This warning may take any of the following forms.</p>
         *
         * <ul>
         * <li>greedy block {@code ()*} contains wildcard; the non-greedy syntax {@code ()*?} may be preferred</li>
         * <li>greedy block {@code ()+} contains wildcard; the non-greedy syntax {@code ()+?} may be preferred</li>
         * </ul>
         */
        public static readonly ErrorType EXPECTED_NON_GREEDY_WILDCARD_BLOCK = new ErrorType(nameof(EXPECTED_NON_GREEDY_WILDCARD_BLOCK), 131, "greedy block ()<arg> contains wildcard; the non-greedy syntax ()<arg>? may be preferred", ErrorSeverity.WARNING);
        /**
         * Compiler Error 132.
         *
         * <p>
         * action in lexer rule '<em>rule</em>' must be last element of single
         * outermost alt</p>
         *
         * @deprecated This error is no longer issued by ANTLR 4.2.
         */
        [System.Obsolete]
        public static readonly ErrorType LEXER_ACTION_PLACEMENT_ISSUE = new ErrorType(nameof(LEXER_ACTION_PLACEMENT_ISSUE), 132, "action in lexer rule '<arg>' must be last element of single outermost alt", ErrorSeverity.ERROR);
        /**
         * Compiler Error 133.
         *
         * <p>
         * {@code ->command} in lexer rule '<em>rule</em>' must be last element of
         * single outermost alt</p>
         */
        public static readonly ErrorType LEXER_COMMAND_PLACEMENT_ISSUE = new ErrorType(nameof(LEXER_COMMAND_PLACEMENT_ISSUE), 133, "->command in lexer rule '<arg>' must be last element of single outermost alt", ErrorSeverity.ERROR);
        /**
         * Compiler Error 134.
         *
         * <p>
         * symbol '<em>symbol</em>' conflicts with generated code in target language
         * or runtime</p>
         *
         * <p>
         * Note: This error has the same number as the unrelated error
         * {@link #UNSUPPORTED_REFERENCE_IN_LEXER_SET}.</p>
         */
        public static readonly ErrorType USE_OF_BAD_WORD = new ErrorType(nameof(USE_OF_BAD_WORD), 134, "symbol '<arg>' conflicts with generated code in target language or runtime", ErrorSeverity.ERROR);
        /**
         * Compiler Error 134.
         *
         * <p>rule reference '<em>rule</em>' is not currently supported in a set</p>
         *
         * <p>
         * Note: This error has the same number as the unrelated error
         * {@link #USE_OF_BAD_WORD}.</p>
         */
        public static readonly ErrorType UNSUPPORTED_REFERENCE_IN_LEXER_SET = new ErrorType(nameof(UNSUPPORTED_REFERENCE_IN_LEXER_SET), 134, "rule reference '<arg>' is not currently supported in a set", ErrorSeverity.ERROR);
        /**
         * Compiler Error 135.
         *
         * <p>cannot assign a value to list label '<em>label</em>'</p>
         */
        public static readonly ErrorType ASSIGNMENT_TO_LIST_LABEL = new ErrorType(nameof(ASSIGNMENT_TO_LIST_LABEL), 135, "cannot assign a value to list label '<arg>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 136.
         *
         * <p>return value '<em>name</em>' conflicts with rule with same name</p>
         */
        public static readonly ErrorType RETVAL_CONFLICTS_WITH_RULE = new ErrorType(nameof(RETVAL_CONFLICTS_WITH_RULE), 136, "return value '<arg>' conflicts with rule with same name", ErrorSeverity.ERROR);
        /**
         * Compiler Error 137.
         *
         * <p>return value '<em>name</em>' conflicts with token with same name</p>
         */
        public static readonly ErrorType RETVAL_CONFLICTS_WITH_TOKEN = new ErrorType(nameof(RETVAL_CONFLICTS_WITH_TOKEN), 137, "return value '<arg>' conflicts with token with same name", ErrorSeverity.ERROR);
        /**
         * Compiler Error 138.
         *
         * <p>parameter '<em>parameter</em>' conflicts with rule with same name</p>
         */
        public static readonly ErrorType ARG_CONFLICTS_WITH_RULE = new ErrorType(nameof(ARG_CONFLICTS_WITH_RULE), 138, "parameter '<arg>' conflicts with rule with same name", ErrorSeverity.ERROR);
        /**
         * Compiler Error 139.
         *
         * <p>parameter '<em>parameter</em>' conflicts with token with same name</p>
         */
        public static readonly ErrorType ARG_CONFLICTS_WITH_TOKEN = new ErrorType(nameof(ARG_CONFLICTS_WITH_TOKEN), 139, "parameter '<arg>' conflicts with token with same name", ErrorSeverity.ERROR);
        /**
         * Compiler Error 140.
         *
         * <p>local '<em>local</em>' conflicts with rule with same name</p>
         */
        public static readonly ErrorType LOCAL_CONFLICTS_WITH_RULE = new ErrorType(nameof(LOCAL_CONFLICTS_WITH_RULE), 140, "local '<arg>' conflicts with rule with same name", ErrorSeverity.ERROR);
        /**
         * Compiler Error 141.
         *
         * <p>local '<em>local</em>' conflicts with rule token same name</p>
         */
        public static readonly ErrorType LOCAL_CONFLICTS_WITH_TOKEN = new ErrorType(nameof(LOCAL_CONFLICTS_WITH_TOKEN), 141, "local '<arg>' conflicts with rule token same name", ErrorSeverity.ERROR);
        /**
         * Compiler Error 142.
         *
         * <p>local '<em>local</em>' conflicts with parameter with same name</p>
         */
        public static readonly ErrorType LOCAL_CONFLICTS_WITH_ARG = new ErrorType(nameof(LOCAL_CONFLICTS_WITH_ARG), 142, "local '<arg>' conflicts with parameter with same name", ErrorSeverity.ERROR);
        /**
         * Compiler Error 143.
         *
         * <p>local '<em>local</em>' conflicts with return value with same name</p>
         */
        public static readonly ErrorType LOCAL_CONFLICTS_WITH_RETVAL = new ErrorType(nameof(LOCAL_CONFLICTS_WITH_RETVAL), 143, "local '<arg>' conflicts with return value with same name", ErrorSeverity.ERROR);
        /**
         * Compiler Error 144.
         *
         * <p>
         * multi-character literals are not allowed in lexer sets:
         * <em>literal</em></p>
         */
        public static readonly ErrorType INVALID_LITERAL_IN_LEXER_SET = new ErrorType(nameof(INVALID_LITERAL_IN_LEXER_SET), 144, "multi-character literals are not allowed in lexer sets: <arg>", ErrorSeverity.ERROR);
        /**
         * Compiler Error 145.
         *
         * <p>
         * lexer mode '<em>mode</em>' must contain at least one non-fragment
         * rule</p>
         *
         * <p>
         * Every lexer mode must contain at least one rule which is not declared
         * with the {@code fragment} modifier.</p>
         */
        public static readonly ErrorType MODE_WITHOUT_RULES = new ErrorType(nameof(MODE_WITHOUT_RULES), 145, "lexer mode '<arg>' must contain at least one non-fragment rule", ErrorSeverity.ERROR);
        /**
         * Compiler Warning 146.
         *
         * <p>non-fragment lexer rule '<em>rule</em>' can match the empty string</p>
         *
         * <p>All non-fragment lexer rules must match at least one character.</p>
         *
         * <p>The following example shows this error.</p>
         *
         * <pre>
         * Whitespace : [ \t]+;  // ok
         * Whitespace : [ \t];   // ok
         *
         * fragment WS : [ \t]*; // ok
         *
         * Whitespace : [ \t]*;  // error 146
         * </pre>
         */
        public static readonly ErrorType EPSILON_TOKEN = new ErrorType(nameof(EPSILON_TOKEN), 146, "non-fragment lexer rule '<arg>' can match the empty string", ErrorSeverity.WARNING);
        /**
         * Compiler Error 147.
         *
         * <p>
         * left recursive rule '<em>rule</em>' must contain an alternative which is
         * not left recursive</p>
         *
         * <p>Left-recursive rules must contain at least one alternative which is not
         * left recursive.</p>
         *
         * <p>The following rule produces this error.</p>
         *
         * <pre>
         * // error 147:
         * a : a ID
         *   | a INT
         *   ;
         * </pre>
         */
        public static readonly ErrorType NO_NON_LR_ALTS = new ErrorType(nameof(NO_NON_LR_ALTS), 147, "left recursive rule '<arg>' must contain an alternative which is not left recursive", ErrorSeverity.ERROR);
        /**
         * Compiler Error 148.
         *
         * <p>
         * left recursive rule '<em>rule</em>' contains a left recursive alternative
         * which can be followed by the empty string</p>
         *
         * <p>In left-recursive rules, all left-recursive alternatives must match at
         * least one symbol following the recursive rule invocation.</p>
         *
         * <p>The following rule produces this error.</p>
         *
         * <pre>
         * a : ID    // ok        (alternative is not left recursive)
         *   | a INT // ok        (a must be follow by INT)
         *   | a ID? // error 148 (the ID following a is optional)
         *   ;
         * </pre>
         */
        public static readonly ErrorType EPSILON_LR_FOLLOW = new ErrorType(nameof(EPSILON_LR_FOLLOW), 148, "left recursive rule '<arg>' contains a left recursive alternative which can be followed by the empty string", ErrorSeverity.ERROR);
        /**
         * Compiler Error 149.
         *
         * <p>
         * lexer command '<em>command</em>' does not exist or is not supported by
         * the current target</p>
         *
         * <p>Each lexer command requires an explicit implementation in the target
         * templates. This error indicates that the command was incorrectly written
         * or is not supported by the current target.</p>
         *
         * <p>The following rule produces this error.</p>
         *
         * <pre>
         * X : 'foo' -&gt; type(Foo);  // ok
         * Y : 'foo' -&gt; token(Foo); // error 149 (token is not a supported lexer command)
         * </pre>
         *
         * @since 4.1
         */
        public static readonly ErrorType INVALID_LEXER_COMMAND = new ErrorType(nameof(INVALID_LEXER_COMMAND), 149, "lexer command '<arg>' does not exist or is not supported by the current target", ErrorSeverity.ERROR);
        /**
         * Compiler Error 150.
         *
         * <p>missing argument for lexer command '<em>command</em>'</p>
         *
         * <p>Some lexer commands require an argument.</p>
         *
         * <p>The following rule produces this error.</p>
         * 
         * <pre>
         * X : 'foo' -&gt; type(Foo); // ok
         * Y : 'foo' -&gt; type;      // error 150 (the type command requires an argument)
         * </pre>
         *
         * @since 4.1
         */
        public static readonly ErrorType MISSING_LEXER_COMMAND_ARGUMENT = new ErrorType(nameof(MISSING_LEXER_COMMAND_ARGUMENT), 150, "missing argument for lexer command '<arg>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 151.
         *
         * <p>lexer command '<em>command</em>' does not take any arguments</p>
         *
         * <p>A lexer command which does not take parameters was invoked with an
         * argument.</p>
         *
         * <p>The following rule produces this error.</p>
         *
         * <pre>
         * X : 'foo' -&gt; popMode;    // ok
         * Y : 'foo' -&gt; popMode(A); // error 151 (the popMode command does not take an argument)
         * </pre>
         *
         * @since 4.1
         */
        public static readonly ErrorType UNWANTED_LEXER_COMMAND_ARGUMENT = new ErrorType(nameof(UNWANTED_LEXER_COMMAND_ARGUMENT), 151, "lexer command '<arg>' does not take any arguments", ErrorSeverity.ERROR);
        /**
         * Compiler Error 152.
         *
         * <p>unterminated string literal</p>
         *
         * <p>The grammar contains an unterminated string literal.</p>
         *
         * <p>The following rule produces this error.</p>
         *
         * <pre>
         * x : 'x'; // ok
         * y : 'y;  // error 152
         * </pre>
         *
         * @since 4.1
         */
        public static readonly ErrorType UNTERMINATED_STRING_LITERAL = new ErrorType(nameof(UNTERMINATED_STRING_LITERAL), 152, "unterminated string literal", ErrorSeverity.ERROR);
        /**
         * Compiler Error 153.
         *
         * <p>
         * rule '<em>rule</em>' contains a closure with at least one alternative
         * that can match an empty string</p>
         *
         * <p>A rule contains a closure ({@code (...)*}) or positive closure
         * ({@code (...)+}) around an empty alternative.</p>
         *
         * <p>The following rule produces this error.</p>
         *
         * <pre>
         * x  : ;
         * y  : x+;                                // error 153
         * z1 : ('foo' | 'bar'? 'bar2'?)*;         // error 153
         * z2 : ('foo' | 'bar' 'bar2'? | 'bar2')*; // ok
         * </pre>
         *
         * @since 4.1
         */
        public static readonly ErrorType EPSILON_CLOSURE = new ErrorType(nameof(EPSILON_CLOSURE), 153, "rule '<arg>' contains a closure with at least one alternative that can match an empty string", ErrorSeverity.ERROR);
        /**
         * Compiler Warning 154.
         *
         * <p>
         * rule '<em>rule</em>' contains an optional block with at least one
         * alternative that can match an empty string</p>
         *
         * <p>A rule contains an optional block ({@code (...)?}) around an empty
         * alternative.</p>
         *
         * <p>The following rule produces this warning.</p>
         *
         * <pre>
         * x  : ;
         * y  : x?;                                // warning 154
         * z1 : ('foo' | 'bar'? 'bar2'?)?;         // warning 154
         * z2 : ('foo' | 'bar' 'bar2'? | 'bar2')?; // ok
         * </pre>
         *
         * @since 4.1
         */
        public static readonly ErrorType EPSILON_OPTIONAL = new ErrorType(nameof(EPSILON_OPTIONAL), 154, "rule '<arg>' contains an optional block with at least one alternative that can match an empty string", ErrorSeverity.WARNING);
        /**
         * Compiler Warning 155.
         *
         * <p>
         * rule '<em>rule</em>' contains a lexer command with an unrecognized
         * constant value; lexer interpreters may produce incorrect output</p>
         *
         * <p>A lexer rule contains a standard lexer command, but the constant value
         * argument for the command is an unrecognized string. As a result, the
         * lexer command will be translated as a custom lexer action, preventing the
         * command from executing in some interpreted modes. The output of the lexer
         * interpreter may not match the output of the generated lexer.</p>
         *
         * <p>The following rule produces this warning.</p>
         *
         * <pre>
         * &#064;members {
         * public static final int CUSTOM = HIDDEN + 1;
         * }
         *
         * X : 'foo' -&gt; channel(HIDDEN);           // ok
         * Y : 'bar' -&gt; channel(CUSTOM);           // warning 155
         * </pre>
         *
         * @since 4.2
         */
        public static readonly ErrorType UNKNOWN_LEXER_CONSTANT = new ErrorType(nameof(UNKNOWN_LEXER_CONSTANT), 155, "rule '<arg>' contains a lexer command with an unrecognized constant value; lexer interpreters may produce incorrect output", ErrorSeverity.WARNING);
        /**
         * Compiler Error 156.
         *
         * <p>invalid escape sequence</p>
         *
         * <p>The grammar contains a string literal with an invalid escape sequence.</p>
         *
         * <p>The following rule produces this error.</p>
         *
         * <pre>
         * x : 'x';  // ok
         * y : '\u005Cu'; // error 156
         * </pre>
         *
         * @since 4.2.1
         */
        public static readonly ErrorType INVALID_ESCAPE_SEQUENCE = new ErrorType(nameof(INVALID_ESCAPE_SEQUENCE), 156, "invalid escape sequence", ErrorSeverity.ERROR);
        /**
         * Compiler Warning 157.
         *
         * <p>rule '<em>rule</em>' contains an 'assoc' element option in an
         * unrecognized location</p>
         *
         * <p>
         * In ANTLR 4.2, the position of the {@code assoc} element option was moved
         * from the operator terminal(s) to the alternative itself. This warning is
         * reported when an {@code assoc} element option is specified on a grammar
         * element that is not recognized by the current version of ANTLR, and as a
         * result will simply be ignored.
         * </p>
         *
         * <p>The following rule produces this warning.</p>
         *
         * <pre>
         * x : 'x'
         *   | x '+'&lt;assoc=right&gt; x   // warning 157
         *   |&lt;assoc=right&gt; x '*' x   // ok
         *   ;
         * </pre>
         *
         * @since 4.2.1
         */
        public static readonly ErrorType UNRECOGNIZED_ASSOC_OPTION = new ErrorType(nameof(UNRECOGNIZED_ASSOC_OPTION), 157, "rule '<arg>' contains an 'assoc' terminal option in an unrecognized location", ErrorSeverity.WARNING);
        /**
         * Compiler Warning 158.
         *
         * <p>fragment rule '<em>rule</em>' contains an action or command which can
         * never be executed</p>
         *
         * <p>A lexer rule which is marked with the {@code fragment} modifier
         * contains an embedded action or lexer command. ANTLR lexers only execute
         * commands and embedded actions located in the top-level matched rule.
         * Since fragment rules can never be the top-level rule matched by a lexer,
         * actions or commands placed in these rules can never be executed during
         * the lexing process.</p>
         *
         * <p>The following rule produces this warning.</p>
         *
         * <pre>
         * X1 : 'x' -&gt; more    // ok
         *    ;
         * Y1 : 'x' {more();}  // ok
         *    ;
         * fragment
         * X2 : 'x' -&gt; more    // warning 158
         *    ;
         * fragment
         * Y2 : 'x' {more();}  // warning 158
         *    ;
         * </pre>
         *
         * @since 4.2.1
         */
        public static readonly ErrorType FRAGMENT_ACTION_IGNORED = new ErrorType(nameof(FRAGMENT_ACTION_IGNORED), 158, "fragment rule '<arg>' contains an action or command which can never be executed", ErrorSeverity.WARNING);
        /**
         * Compiler Error 159.
         *
         * <p>cannot declare a rule with reserved name '<em>rule</em>'</p>
         *
         * <p>A rule was declared with a reserved name.</p>
         *
         * <p>The following rule produces this error.</p>
         *
         * <pre>
         * EOF : ' '   // error 159 (EOF is a reserved name)
         *     ;
         * </pre>
         *
         * @since 4.2.1
         */
        public static readonly ErrorType RESERVED_RULE_NAME = new ErrorType(nameof(RESERVED_RULE_NAME), 159, "cannot declare a rule with reserved name '<arg>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 160.
         *
         * <p>reference to parser rule '<em>rule</em>' in lexer rule '<em>name</em>'</p>
         *
         * @see #UNDEFINED_RULE_REF
         */
        public static readonly ErrorType PARSER_RULE_REF_IN_LEXER_RULE = new ErrorType(nameof(PARSER_RULE_REF_IN_LEXER_RULE), 160, "reference to parser rule '<arg>' in lexer rule '<arg2>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 161.
         *
         * <p>channel '<em>name</em>' conflicts with token with same name</p>
         */
        public static readonly ErrorType CHANNEL_CONFLICTS_WITH_TOKEN = new ErrorType(nameof(CHANNEL_CONFLICTS_WITH_TOKEN), 161, "channel '<arg>' conflicts with token with same name", ErrorSeverity.ERROR);
        /**
         * Compiler Error 162.
         *
         * <p>channel '<em>name</em>' conflicts with mode with same name</p>
         */
        public static readonly ErrorType CHANNEL_CONFLICTS_WITH_MODE = new ErrorType(nameof(CHANNEL_CONFLICTS_WITH_MODE), 162, "channel '<arg>' conflicts with mode with same name", ErrorSeverity.ERROR);
        /**
         * Compiler Error 163.
         *
         * <p>custom channels are not supported in parser grammars</p>
         */
        public static readonly ErrorType CHANNELS_BLOCK_IN_PARSER_GRAMMAR = new ErrorType(nameof(CHANNELS_BLOCK_IN_PARSER_GRAMMAR), 163, "custom channels are not supported in parser grammars", ErrorSeverity.ERROR);
        /**
         * Compiler Error 164.
         *
         * <p>custom channels are not supported in combined grammars</p>
         */
        public static readonly ErrorType CHANNELS_BLOCK_IN_COMBINED_GRAMMAR = new ErrorType(nameof(CHANNELS_BLOCK_IN_COMBINED_GRAMMAR), 164, "custom channels are not supported in combined grammars", ErrorSeverity.ERROR);
        /**
         * Compiler Error 165.
         *
         * <p>rule '<em>rule</em>': must label all alternatives in rules with the same base context, or none</p>
         */
        public static readonly ErrorType RULE_WITH_TOO_FEW_ALT_LABELS_GROUP = new ErrorType(nameof(RULE_WITH_TOO_FEW_ALT_LABELS_GROUP), 165, "rule '<arg>': must label all alternatives in rules with the same base context, or none", ErrorSeverity.ERROR);
        /**
         * Compiler Error 166.
         *
         * <p>rule '<em>rule</em>': baseContext option value must reference a rule</p>
         */
        public static readonly ErrorType BASE_CONTEXT_MUST_BE_RULE_NAME = new ErrorType(nameof(BASE_CONTEXT_MUST_BE_RULE_NAME), 166, "rule '<arg>': baseContext option value must reference a rule", ErrorSeverity.ERROR);
        /**
         * Compiler Error 167.
         *
         * <p>rule '<em>rule</em>': base context must reference a rule that does not specify a base context</p>
         */
        public static readonly ErrorType BASE_CONTEXT_CANNOT_BE_TRANSITIVE = new ErrorType(nameof(BASE_CONTEXT_CANNOT_BE_TRANSITIVE), 167, "rule '<arg>': base context must reference a rule that does not specify a base context", ErrorSeverity.ERROR);
        /**
         * Compiler Error 168.
         *
         * <p>rule '<em>rule</em>': lexer rules cannot specify a base context</p>
         */
        public static readonly ErrorType LEXER_RULE_CANNOT_HAVE_BASE_CONTEXT = new ErrorType(nameof(LEXER_RULE_CANNOT_HAVE_BASE_CONTEXT), 168, "rule '<arg>': lexer rules cannot specify a base context", ErrorSeverity.ERROR);

        /**
         * Compiler Error 169.
         *
         * <p>rule '<em>rule</em>' is left recursive but doesn't conform to a pattern ANTLR can handle</p>
         *
         * @since 4.5
         */
        public static readonly ErrorType NONCONFORMING_LR_RULE = new ErrorType(nameof(NONCONFORMING_LR_RULE), 169, "rule '<arg>' is left recursive but doesn't conform to a pattern ANTLR can handle", ErrorSeverity.ERROR);
        /**
         * Compiler Error 170.
         *
         * <pre>
         * mode M1;
         * A1: 'a'; // ok
         * mode M2;
         * A2: 'a'; // ok
         * M1: 'b'; // error 170
         * </pre>
         *
         * <p>mode <em>name</em> conflicts with token with same name</p>
         */
        public static readonly ErrorType MODE_CONFLICTS_WITH_TOKEN = new ErrorType(nameof(MODE_CONFLICTS_WITH_TOKEN), 170, "mode '<arg>' conflicts with token with same name", ErrorSeverity.ERROR);
        /**
         * Compiler Error 171.
         *
         * <p>can not use or declare token with reserved name</p>
         *
         * <p>Reserved names: HIDDEN, DEFAULT_TOKEN_CHANNEL, SKIP, MORE, MAX_CHAR_VALUE, MIN_CHAR_VALUE.</p>
         *
         * <p>Can be used but cannot be declared: EOF</p>
         */
        public static readonly ErrorType TOKEN_CONFLICTS_WITH_COMMON_CONSTANTS = new ErrorType(nameof(TOKEN_CONFLICTS_WITH_COMMON_CONSTANTS), 171, "cannot use or declare token with reserved name '<arg>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 172.
         *
         * <p>can not use or declare channel with reserved name</p>
         *
         * <p>Reserved names: DEFAULT_MODE, SKIP, MORE, EOF, MAX_CHAR_VALUE, MIN_CHAR_VALUE.</p>
         *
         * <p>Can be used but cannot be declared: HIDDEN, DEFAULT_TOKEN_CHANNEL</p>
         */
        public static readonly ErrorType CHANNEL_CONFLICTS_WITH_COMMON_CONSTANTS = new ErrorType(nameof(CHANNEL_CONFLICTS_WITH_COMMON_CONSTANTS), 172, "cannot use or declare channel with reserved name '<arg>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 173.
         *
         * <p>can not use or declare mode with reserved name</p>
         *
         * <p>Reserved names: HIDDEN, DEFAULT_TOKEN_CHANNEL, SKIP, MORE, MAX_CHAR_VALUE, MIN_CHAR_VALUE.</p>
         *
         * <p>Can be used and cannot declared: DEFAULT_MODE</p>
         */
        public static readonly ErrorType MODE_CONFLICTS_WITH_COMMON_CONSTANTS = new ErrorType(nameof(MODE_CONFLICTS_WITH_COMMON_CONSTANTS), 173, "cannot use or declare mode with reserved name '<arg>'", ErrorSeverity.ERROR);
        /**
         * Compiler Error 174.
         *
         * <p>empty strings not allowed</p>
         *
         * <pre>A: '''test''';</pre>
         * <pre>B: '';</pre>
         * <pre>C: 'test' '';</pre>
         */
        public static readonly ErrorType EMPTY_STRINGS_NOT_ALLOWED = new ErrorType(nameof(EMPTY_STRINGS_NOT_ALLOWED), 174, "string literals cannot be empty", ErrorSeverity.ERROR);

        /*
         * Backward incompatibility errors
         */

        /**
         * Compiler Error 200.
         *
         * <p>tree grammars are not supported in ANTLR 4</p>
         *
         * <p>
         * This error message is provided as a compatibility notice for users
         * migrating from ANTLR 3. ANTLR 4 does not support tree grammars, but
         * instead offers automatically generated parse tree listeners and visitors
         * as a more maintainable alternative.</p>
         */
        public static readonly ErrorType V3_TREE_GRAMMAR = new ErrorType(nameof(V3_TREE_GRAMMAR), 200, "tree grammars are not supported in ANTLR 4", ErrorSeverity.ERROR);
        /**
         * Compiler Warning 201.
         *
         * <p>
         * labels in lexer rules are not supported in ANTLR 4; actions cannot
         * reference elements of lexical rules but you can use
         * {@link Lexer#getText()} to get the entire text matched for the rule</p>
         *
         * <p>
         * ANTLR 4 uses a DFA for recognition of entire tokens, resulting in faster
         * and smaller lexers than ANTLR 3 produced. As a result, sub-rules
         * referenced within lexer rules are not tracked independently, and cannot
         * be assigned to labels.</p>
         */
        public static readonly ErrorType V3_LEXER_LABEL = new ErrorType(nameof(V3_LEXER_LABEL), 201, "labels in lexer rules are not supported in ANTLR 4; " +
            "actions cannot reference elements of lexical rules but you can use " +
            "getText() to get the entire text matched for the rule", ErrorSeverity.WARNING);
        /**
         * Compiler Warning 202.
         *
         * <p>
         * '{@code tokens {A; B;}}' syntax is now '{@code tokens {A, B}}' in ANTLR
         * 4</p>
         *
         * <p>
         * ANTLR 4 uses comma-separated token declarations in the {@code tokens{}}
         * block. This warning appears when the tokens block is written using the
         * ANTLR 3 syntax of semicolon-terminated token declarations.</p>
         *
         * <p>
         * <strong>NOTE:</strong> ANTLR 4 does not allow a trailing comma to appear following the
         * last token declared in the {@code tokens{}} block.</p>
         */
        public static readonly ErrorType V3_TOKENS_SYNTAX = new ErrorType(nameof(V3_TOKENS_SYNTAX), 202, "'tokens {A; B;}' syntax is now 'tokens {A, B}' in ANTLR 4", ErrorSeverity.WARNING);
        /**
         * Compiler Error 203.
         *
         * <p>
         * assignments in {@code tokens{}} are not supported in ANTLR 4; use lexical
         * rule '<em>TokenName</em> : <em>LiteralValue</em>;' instead</p>
         *
         * <p>
         * ANTLR 3 allowed literal tokens to be declared and assigned a value within
         * the {@code tokens{}} block. ANTLR 4 no longer offers this syntax. When
         * migrating a grammar from ANTLR 3 to ANTLR 4, any tokens with a literal
         * value declared in the {@code tokens{}} block should be converted to
         * standard lexer rules.</p>
         */
        public static readonly ErrorType V3_ASSIGN_IN_TOKENS = new ErrorType(nameof(V3_ASSIGN_IN_TOKENS), 203, "assignments in tokens{} are not supported in ANTLR 4; use lexical rule '<arg> : <arg2>;' instead", ErrorSeverity.ERROR);
        /**
         * Compiler Warning 204.
         *
         * <p>
         * {@code {...}?=>} explicitly gated semantic predicates are deprecated in
         * ANTLR 4; use {@code {...}?} instead</p>
         *
         * <p>
         * ANTLR 4 treats semantic predicates consistently in a manner similar to
         * gated semantic predicates in ANTLR 3. When migrating a grammar from ANTLR
         * 3 to ANTLR 4, all uses of the gated semantic predicate syntax can be
         * safely converted to the standard semantic predicated syntax, which is the
         * only form used by ANTLR 4.</p>
         */
        public static readonly ErrorType V3_GATED_SEMPRED = new ErrorType(nameof(V3_GATED_SEMPRED), 204, "{...}?=> explicitly gated semantic predicates are deprecated in ANTLR 4; use {...}? instead", ErrorSeverity.WARNING);
        /**
         * Compiler Error 205.
         *
         * <p>{@code (...)=>} syntactic predicates are not supported in ANTLR 4</p>
         *
         * <p>
         * ANTLR 4's improved lookahead algorithms do not require the use of
         * syntactic predicates to disambiguate long lookahead sequences. The
         * syntactic predicates should be removed when migrating a grammar from
         * ANTLR 3 to ANTLR 4.</p>
         */
        public static readonly ErrorType V3_SYNPRED = new ErrorType(nameof(V3_SYNPRED), 205, "(...)=> syntactic predicates are not supported in ANTLR 4", ErrorSeverity.ERROR);

        // Dependency sorting errors

        /* t1.g4 -> t2.g4 -> t3.g4 ->t1.g4 */
        //CIRCULAR_DEPENDENCY(200, "your grammars contain a circular dependency and cannot be sorted into a valid build order", ErrorSeverity.ERROR),

        private readonly string name;

        /**
         * The error or warning message, in StringTemplate 4 format using {@code &lt;}
         * and {@code >} as the delimiters. Arguments for the message may be
         * referenced using the following names:
         *
         * <ul>
         * <li>{@code arg}: The first template argument</li>
         * <li>{@code arg2}: The second template argument</li>
         * <li>{@code arg3}: The third template argument</li>
         * <li>{@code verbose}: {@code true} if verbose messages were requested; otherwise, {@code false}</li>
         * <li>{@code exception}: The exception which resulted in the error, if any.</li>
         * <li>{@code stackTrace}: The stack trace for the exception, when available.</li>
         * </ul>
         */
        public readonly string msg;
        /**
         * The error or warning number.
         *
         * <p>The code should be unique, and following its
         * use in a release should not be altered or reassigned.</p>
         */
        public readonly int code;
        /**
         * The error severity.
         */
        public readonly ErrorSeverity severity;

        /**
         * Constructs a new {@link ErrorType} with the specified code, message, and
         * severity.
         *
         * @param code The unique error number.
         * @param msg The error message template.
         * @param severity The error severity.
         */
        private ErrorType(string name, int code, string msg, ErrorSeverity severity) {
            this.name = name;
            this.code = code;
            this.msg = msg;
            this.severity = severity;
        }

        public string Name =>
            name;
    }
}
