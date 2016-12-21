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

namespace Antlr4.Parse
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Antlr4.Codegen;
    using Antlr4.Misc;
    using Antlr4.Tool;
    using Antlr4.Tool.Ast;
    using Exception = System.Exception;
    using File = System.IO.File;
    using FileNotFoundException = System.IO.FileNotFoundException;
    using Math = System.Math;
    using Path = System.IO.Path;
    using TokenTypes = Antlr.Runtime.TokenTypes;
    using Encoding = System.Text.Encoding;

    /** */
    public class TokenVocabParser {
        protected readonly Grammar g;

        public TokenVocabParser(Grammar g) {
            this.g = g;
        }

        /** Load a vocab file {@code &lt;vocabName&gt;.tokens} and return mapping. */
        public virtual IDictionary<string, int> Load() {
            IDictionary<string, int> tokens = new LinkedHashMap<string, int>();
            int maxTokenType = -1;
            string fullFile = GetImportedVocabFile();
            AntlrTool tool = g.tool;
            string vocabName = g.GetOptionString("tokenVocab");
            try {
                Regex tokenDefPattern = new Regex("([^\n]+?)[ \\t]*?=[ \\t]*?([0-9]+)");
                string[] lines;
                if (tool.grammarEncoding != null) {
                    lines = File.ReadAllLines(fullFile, Encoding.GetEncoding(tool.grammarEncoding));
                }
                else {
                    lines = File.ReadAllLines(fullFile);
                }

                for (int i = 0; i < lines.Length; i++)
                {
                    string tokenDef = lines[i];
                    int lineNum = i + 1;
                    Match matcher = tokenDefPattern.Match(tokenDef);
                    if (matcher.Success) {
                        string tokenID = matcher.Groups[1].Value;
                        string tokenTypeS = matcher.Groups[2].Value;
                        int tokenType;
                        if (!int.TryParse(tokenTypeS, out tokenType))
                        {
                            tool.errMgr.ToolError(ErrorType.TOKENS_FILE_SYNTAX_ERROR,
                                                  vocabName + CodeGenerator.VOCAB_FILE_EXTENSION,
                                                  " bad token type: " + tokenTypeS,
                                                  lineNum);
                            tokenType = TokenTypes.Invalid;
                        }

                        tool.Log("grammar", "import " + tokenID + "=" + tokenType);
                        tokens[tokenID] = tokenType;
                        maxTokenType = Math.Max(maxTokenType, tokenType);
                        lineNum++;
                    }
                    else {
                        if (tokenDef.Length > 0) { // ignore blank lines
                            tool.errMgr.ToolError(ErrorType.TOKENS_FILE_SYNTAX_ERROR,
                                                  vocabName + CodeGenerator.VOCAB_FILE_EXTENSION,
                                                  " bad token def: " + tokenDef,
                                                  lineNum);
                        }
                    }
                }
            }
            catch (FileNotFoundException) {
                GrammarAST inTree = g.ast.GetOptionAST("tokenVocab");
                string inTreeValue = inTree.Token.Text;
                if (vocabName.Equals(inTreeValue)) {
                    tool.errMgr.GrammarError(ErrorType.CANNOT_FIND_TOKENS_FILE_REFD_IN_GRAMMAR,
                                             g.fileName,
                                             inTree.Token,
                                             fullFile);
                }
                else { // must be from -D option on cmd-line not token in tree
                    tool.errMgr.ToolError(ErrorType.CANNOT_FIND_TOKENS_FILE_GIVEN_ON_CMDLINE,
                                          fullFile,
                                          g.name);
                }
            }
            catch (Exception e) {
                tool.errMgr.ToolError(ErrorType.ERROR_READING_TOKENS_FILE,
                                      e,
                                      fullFile,
                                      e.Message);
            }

            return tokens;
        }

        /** Return a File descriptor for vocab file.  Look in library or
         *  in -o output path.  antlr -o foo T.g4 U.g4 where U needs T.tokens
         *  won't work unless we look in foo too. If we do not find the
         *  file in the lib directory then must assume that the .tokens file
         *  is going to be generated as part of this build and we have defined
         *  .tokens files so that they ALWAYS are generated in the base output
         *  directory, which means the current directory for the command line tool if there
         *  was no output directory specified.
         */
        public virtual string GetImportedVocabFile() {
            string vocabName = g.GetOptionString("tokenVocab");
            string f = Path.Combine(g.tool.libDirectory,
                              vocabName +
                              CodeGenerator.VOCAB_FILE_EXTENSION);
            if (File.Exists(f)) {
                return f;
            }

            // We did not find the vocab file in the lib directory, so we need
            // to look for it in the output directory which is where .tokens
            // files are generated (in the base, not relative to the input
            // location.)
            f = Path.Combine(g.tool.outputDirectory, vocabName + CodeGenerator.VOCAB_FILE_EXTENSION);
            return f;
        }
    }
}
