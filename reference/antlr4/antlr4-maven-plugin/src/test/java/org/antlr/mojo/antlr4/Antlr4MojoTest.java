/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.mojo.antlr4;

import io.takari.maven.testing.TestMavenRuntime;
import io.takari.maven.testing.TestResources;
import org.antlr.v4.runtime.misc.Utils;
import org.apache.maven.execution.MavenSession;
import org.apache.maven.plugin.MojoExecution;
import org.apache.maven.plugin.MojoExecutionException;
import org.apache.maven.project.MavenProject;
import org.codehaus.plexus.util.xml.Xpp3Dom;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;

import java.io.Closeable;
import java.io.File;
import java.io.IOException;
import java.util.Arrays;

import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;
import static org.junit.Assume.assumeTrue;

public class Antlr4MojoTest {
    @Rule
    public ExpectedException thrown = ExpectedException.none();

    @Rule
    public final TestResources resources = new TestResources();

    @Rule
    public final TestMavenRuntime maven = new TestMavenRuntime();

    @Test
    public void importTokens() throws Exception {
        File baseDir = resources.getBasedir("importTokens");
        File antlrDir = new File(baseDir, "src/main/antlr4");
        File generatedSources = new File(baseDir, "target/generated-sources/antlr4");

        File genParser = new File(generatedSources, "test/SimpleParser.java");
        File tokens = new File(antlrDir, "imports/SimpleLexer.tokens");

        MavenProject project = maven.readMavenProject(baseDir);
        MavenSession session = maven.newMavenSession(project);
        MojoExecution exec = maven.newMojoExecution("antlr4");

        ////////////////////////////////////////////////////////////////////////
        // 1st - all grammars have to be processed
        ////////////////////////////////////////////////////////////////////////

        assertFalse(genParser.exists());

        maven.executeMojo(session, project, exec);

        assertTrue(genParser.isFile());

        ////////////////////////////////////////////////////////////////////////
        // 2nd - nothing has been modified, no grammars have to be processed
        ////////////////////////////////////////////////////////////////////////

        {
            byte[] sum = checksum(genParser);

            maven.executeMojo(session, project, exec);

            assertTrue(Arrays.equals(sum, checksum(genParser)));
        }

        ////////////////////////////////////////////////////////////////////////
        // 3rd - the imported grammar changed, every dependency has to be processed
        ////////////////////////////////////////////////////////////////////////

		Change change = Change.of(tokens, "DOT=4");
        try {
            byte[] sum = checksum(genParser);

            maven.executeMojo(session, project, exec);

            assertFalse(Arrays.equals(sum, checksum(genParser)));
        } finally {
			change.close();
		}
    }

    @Test
    public void importsCustomLayout() throws Exception {
        File baseDir = resources.getBasedir("importsCustom");
        File antlrDir = new File(baseDir, "src/main/antlr4");
        File generatedSources = new File(baseDir, "src/main/java");

        File genTestLexer = new File(generatedSources, "foo/TestLexer.java");
        File genTestParser = new File(generatedSources, "foo/TestParser.java");
        File genHello = new File(generatedSources, "foo/HelloParser.java");

        File baseGrammar = new File(antlrDir, "imports/TestBaseLexer.g4");
        File lexerGrammar = new File(antlrDir, "TestLexer.g4");
        File parserGrammar = new File(antlrDir, "TestParser.g4");

        Xpp3Dom outputDirectory = TestMavenRuntime.newParameter("outputDirectory",
                "src/main/java/foo");
        Xpp3Dom arguments = new Xpp3Dom("arguments");
        arguments.addChild(TestMavenRuntime.newParameter("argument", "-package"));
        arguments.addChild(TestMavenRuntime.newParameter("argument", "foo"));

        MavenProject project = maven.readMavenProject(baseDir);
        MavenSession session = maven.newMavenSession(project);
        MojoExecution exec = maven.newMojoExecution("antlr4", outputDirectory, arguments);

        ////////////////////////////////////////////////////////////////////////
        // 1st - all grammars have to be processed
        ////////////////////////////////////////////////////////////////////////

        assertFalse(genHello.exists());
        assertFalse(genTestParser.exists());
		assertFalse(genTestLexer.exists());

        maven.executeMojo(session, project, exec);

        assertTrue(genHello.isFile());
        assertTrue(genTestParser.isFile());
        assertTrue(genTestLexer.isFile());

        ////////////////////////////////////////////////////////////////////////
        // 2nd - nothing has been modified, no grammars have to be processed
        ////////////////////////////////////////////////////////////////////////

        {
            byte[] testLexerSum = checksum(genTestLexer);
            byte[] testParserSum = checksum(genTestParser);
            byte[] helloSum = checksum(genHello);

            maven.executeMojo(session, project, exec);

            assertTrue(Arrays.equals(testLexerSum, checksum(genTestLexer)));
            assertTrue(Arrays.equals(testParserSum, checksum(genTestParser)));
            assertTrue(Arrays.equals(helloSum, checksum(genHello)));
        }

        ////////////////////////////////////////////////////////////////////////
        // 3rd - the imported grammar changed, every dependency has to be processed
        ////////////////////////////////////////////////////////////////////////

        // modify the grammar to make checksum comparison detect a change
		Change change = Change.of(baseGrammar, "DOT: '.' ;");
        try {
            byte[] testLexerSum = checksum(genTestLexer);
            byte[] testParserSum = checksum(genTestParser);
            byte[] helloSum = checksum(genHello);

            maven.executeMojo(session, project, exec);

            assertFalse(Arrays.equals(testLexerSum, checksum(genTestLexer)));
            assertFalse(Arrays.equals(testParserSum, checksum(genTestParser)));
            assertTrue(Arrays.equals(helloSum, checksum(genHello)));
        } finally {
			change.close();
		}

        ////////////////////////////////////////////////////////////////////////
        // 4th - the lexer grammar changed, the parser grammar has to be processed as well
        ////////////////////////////////////////////////////////////////////////

        // modify the grammar to make checksum comparison detect a change
		change = Change.of(lexerGrammar, "fragment DOT : '.';");
        try {
            byte[] testLexerSum = checksum(genTestLexer);
            byte[] testParserSum = checksum(genTestParser);
            byte[] helloSum = checksum(genHello);

            maven.executeMojo(session, project, exec);

            assertFalse(Arrays.equals(testLexerSum, checksum(genTestLexer)));
            assertFalse(Arrays.equals(testParserSum, checksum(genTestParser)));
            assertTrue(Arrays.equals(helloSum, checksum(genHello)));
        } finally {
			change.close();
		}

        ////////////////////////////////////////////////////////////////////////
        // 5th - the parser grammar changed, no other grammars have to be processed
        ////////////////////////////////////////////////////////////////////////

        // modify the grammar to make checksum comparison detect a change
		change = Change.of(parserGrammar, " t : WS* ;");
        try {
            byte[] testLexerSum = checksum(genTestLexer);
            byte[] testParserSum = checksum(genTestParser);
            byte[] helloSum = checksum(genHello);

            maven.executeMojo(session, project, exec);

            assumeTrue(Arrays.equals(testLexerSum, checksum(genTestLexer)));
            assertFalse(Arrays.equals(testParserSum, checksum(genTestParser)));
            assertTrue(Arrays.equals(helloSum, checksum(genHello)));
        } finally {
			change.close();
		}
    }

    @Test
    public void importsStandardLayout() throws Exception {
        File baseDir = resources.getBasedir("importsStandard");
        File antlrDir = new File(baseDir, "src/main/antlr4");
        File generatedSources = new File(baseDir, "target/generated-sources/antlr4");

        File genTestLexer = new File(generatedSources, "test/TestLexer.java");
        File genTestParser = new File(generatedSources, "test/TestParser.java");
        File genHello = new File(generatedSources, "test/HelloParser.java");

        File baseGrammar = new File(antlrDir, "imports/TestBaseLexer.g4");
        File lexerGrammar = new File(antlrDir, "test/TestLexer.g4");
        File parserGrammar = new File(antlrDir, "test/TestParser.g4");

        MavenProject project = maven.readMavenProject(baseDir);
        MavenSession session = maven.newMavenSession(project);
        MojoExecution exec = maven.newMojoExecution("antlr4");

        ////////////////////////////////////////////////////////////////////////
        // 1st - all grammars have to be processed
        ////////////////////////////////////////////////////////////////////////

        assertFalse(genHello.exists());
        assertFalse(genTestParser.exists());
        assertFalse(genTestLexer.exists());

        maven.executeMojo(session, project, exec);

        assertTrue(genHello.isFile());
        assertTrue(genTestParser.isFile());
        assertTrue(genTestLexer.isFile());

        ////////////////////////////////////////////////////////////////////////
        // 2nd - nothing has been modified, no grammars have to be processed
        ////////////////////////////////////////////////////////////////////////

        {
            byte[] testLexerSum = checksum(genTestLexer);
            byte[] testParserSum = checksum(genTestParser);
            byte[] helloSum = checksum(genHello);

            maven.executeMojo(session, project, exec);

            assertTrue(Arrays.equals(testLexerSum, checksum(genTestLexer)));
            assertTrue(Arrays.equals(testParserSum, checksum(genTestParser)));
            assertTrue(Arrays.equals(helloSum, checksum(genHello)));
        }

        ////////////////////////////////////////////////////////////////////////
        // 3rd - the imported grammar changed, every dependency has to be processed
        ////////////////////////////////////////////////////////////////////////

        // modify the grammar to make checksum comparison detect a change
		Change change = Change.of(baseGrammar, "DOT: '.' ;");
        try {
            byte[] testLexerSum = checksum(genTestLexer);
            byte[] testParserSum = checksum(genTestParser);
            byte[] helloSum = checksum(genHello);

            maven.executeMojo(session, project, exec);

            assertFalse(Arrays.equals(testLexerSum, checksum(genTestLexer)));
            assertFalse(Arrays.equals(testParserSum, checksum(genTestParser)));
            assertTrue(Arrays.equals(helloSum, checksum(genHello)));
        } finally {
			change.close();
		}

        ////////////////////////////////////////////////////////////////////////
        // 4th - the lexer grammar changed, the parser grammar has to be processed as well
        ////////////////////////////////////////////////////////////////////////

        // modify the grammar to make checksum comparison detect a change
		change = Change.of(lexerGrammar);
        try {
            byte[] testLexerSum = checksum(genTestLexer);
            byte[] testParserSum = checksum(genTestParser);
            byte[] helloSum = checksum(genHello);

            maven.executeMojo(session, project, exec);

            assertFalse(Arrays.equals(testLexerSum, checksum(genTestLexer)));
            assertFalse(Arrays.equals(testParserSum, checksum(genTestParser)));
            assertTrue(Arrays.equals(helloSum, checksum(genHello)));
        } finally {
			change.close();
		}

        ////////////////////////////////////////////////////////////////////////
        // 5th - the parser grammar changed, no other grammars have to be processed
        ////////////////////////////////////////////////////////////////////////

        // modify the grammar to make checksum comparison detect a change
		change = Change.of(parserGrammar, " t : WS* ;");
        try {
            byte[] testLexerSum = checksum(genTestLexer);
            byte[] testParserSum = checksum(genTestParser);
            byte[] helloSum = checksum(genHello);

            maven.executeMojo(session, project, exec);

            assertTrue(Arrays.equals(testLexerSum, checksum(genTestLexer)));
            assertFalse(Arrays.equals(testParserSum, checksum(genTestParser)));
            assertTrue(Arrays.equals(helloSum, checksum(genHello)));
        } finally {
			change.close();
		}
    }

    @Test
    public void processWhenDependencyRemoved() throws Exception {
        File baseDir = resources.getBasedir("dependencyRemoved");
        File antlrDir = new File(baseDir, "src/main/antlr4");

        File baseGrammar = new File(antlrDir, "imports/HelloBase.g4");

        MavenProject project = maven.readMavenProject(baseDir);
        MavenSession session = maven.newMavenSession(project);
        MojoExecution exec = maven.newMojoExecution("antlr4");

        maven.executeMojo(session, project, exec);

		Change temp = Change.of(baseGrammar);
        try {
            // if the base grammar no longer exists, processing must be performed
			assertTrue(baseGrammar.delete());

            thrown.expect(MojoExecutionException.class);
            thrown.expectMessage("ANTLR 4 caught 1 build errors.");

            maven.executeMojo(session, project, exec);
        } finally {
			temp.close();
		}
    }

    private byte[] checksum(File path) throws IOException {
        return MojoUtils.checksum(path);
    }

    private static class Change implements Closeable {
        final File file;
        final char[] original;

        public Change(File file, String change) {
            this.file = file;

            try {
                original = Utils.readFile(file.getAbsolutePath(), "UTF-8");
            } catch (IOException ex) {
                throw new RuntimeException("Could not read file " + file);
            }

            String text = new String(original) + change;

			try {
				Utils.writeFile(file.getAbsolutePath(), text, "UTF-8");
			} catch (IOException ex) {
				throw new RuntimeException("Could not write file " + file);
			}
        }

        public static Change of(File file, String change) {
            return new Change(file, change);
        }

        public static Change of(File file) {
            return new Change(file, "\n");
        }

        @Override
        public void close() {
			try {
				Utils.writeFile(file.getAbsolutePath(), new String(original), "UTF-8");
			} catch (IOException ex) {
				throw new RuntimeException("Could not restore file " + file);
			}
        }
    }
}
