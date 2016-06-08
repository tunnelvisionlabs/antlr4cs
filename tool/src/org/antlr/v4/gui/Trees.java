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

package org.antlr.v4.gui;

import javax.print.PrintException;
import javax.swing.JDialog;
import org.antlr.v4.runtime.Parser;
import org.antlr.v4.runtime.misc.NotNull;
import org.antlr.v4.runtime.misc.Nullable;
import org.antlr.v4.runtime.misc.Utils;
import org.antlr.v4.runtime.tree.Tree;

import java.io.BufferedWriter;
import java.io.FileWriter;
import java.io.IOException;
import java.util.Arrays;
import java.util.List;
import java.util.concurrent.Future;

public class Trees {
	/** Call this method to view a parse tree in a dialog box visually. */
	public static Future<JDialog> inspect(@Nullable Tree t, @Nullable List<String> ruleNames) {
		TreeViewer viewer = new TreeViewer(ruleNames, t);
		return viewer.open();
	}

	/** Call this method to view a parse tree in a dialog box visually. */
	public static Future<JDialog> inspect(@Nullable Tree t, @Nullable Parser parser) {
		List<String> ruleNames = parser != null ? Arrays.asList(parser.getRuleNames()) : null;
		return inspect(t, ruleNames);
	}

	/** Save this tree in a postscript file */
	public static void save(@Nullable Tree t, @Nullable Parser parser, String fileName)
		throws IOException, PrintException
	{
		List<String> ruleNames = parser != null ? Arrays.asList(parser.getRuleNames()) : null;
		save(t, ruleNames, fileName);
	}

	/** Save this tree in a postscript file using a particular font name and size */
	public static void save(Tree t, @Nullable Parser parser, String fileName,
					 String fontName, int fontSize)
		throws IOException
	{
		List<String> ruleNames = parser != null ? Arrays.asList(parser.getRuleNames()) : null;
		save(t, ruleNames, fileName, fontName, fontSize);
	}

	/** Save this tree in a postscript file */
	public static void save(Tree t, @Nullable List<String> ruleNames, String fileName)
		throws IOException, PrintException
	{
		writePS(t, ruleNames, fileName);
	}

	/** Save this tree in a postscript file using a particular font name and size */
	public static void save(Tree t,
	                        @Nullable List<String> ruleNames, String fileName,
	                        String fontName, int fontSize)
	throws IOException
	{
		writePS(t, ruleNames, fileName, fontName, fontSize);
	}

	public static String getPS(Tree t, @Nullable List<String> ruleNames,
							   String fontName, int fontSize)
	{
		TreePostScriptGenerator psgen =
			new TreePostScriptGenerator(ruleNames, t, fontName, fontSize);
		return psgen.getPS();
	}

	public static String getPS(Tree t, @Nullable List<String> ruleNames) {
		return getPS(t, ruleNames, "Helvetica", 11);
	}

	public static void writePS(Tree t, @Nullable List<String> ruleNames,
							   String fileName,
							   String fontName, int fontSize)
		throws IOException
	{
		String ps = getPS(t, ruleNames, fontName, fontSize);
		FileWriter f = new FileWriter(fileName);
		BufferedWriter bw = new BufferedWriter(f);
		try {
			bw.write(ps);
		}
		finally {
			bw.close();
		}
	}

	public static void writePS(Tree t, @Nullable List<String> ruleNames, String fileName)
		throws IOException
	{
		writePS(t, ruleNames, fileName, "Helvetica", 11);
	}

	/** Print out a whole tree in LISP form. Arg nodeTextProvider is used on the
	 *  node payloads to get the text for the nodes.
	 *
	 *  @since 4.5.1
	 */
	public static String toStringTree(@Nullable Tree t, @NotNull TreeTextProvider nodeTextProvider) {
		if ( t==null ) return "null";
		String s = Utils.escapeWhitespace(nodeTextProvider.getText(t), false);
		if ( t.getChildCount()==0 ) return s;
		StringBuilder buf = new StringBuilder();
		buf.append("(");
		s = Utils.escapeWhitespace(nodeTextProvider.getText(t), false);
		buf.append(s);
		buf.append(' ');
		for (int i = 0; i<t.getChildCount(); i++) {
			if ( i>0 ) buf.append(' ');
			buf.append(toStringTree(t.getChild(i), nodeTextProvider));
		}
		buf.append(")");
		return buf.toString();
	}

	private Trees() {
	}
}
