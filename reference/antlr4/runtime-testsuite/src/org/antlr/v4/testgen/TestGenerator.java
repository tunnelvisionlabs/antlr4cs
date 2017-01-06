/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
package org.antlr.v4.testgen;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.OutputStreamWriter;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Collections;
import java.util.List;
import java.util.Map;
import org.stringtemplate.v4.ST;
import org.stringtemplate.v4.STGroup;
import org.stringtemplate.v4.STGroupFile;
import org.stringtemplate.v4.StringRenderer;
import org.stringtemplate.v4.gui.STViz;
import org.stringtemplate.v4.misc.ErrorBuffer;

public class TestGenerator {
	// This project uses UTF-8, but the plugin might be used in another project
	// which is not. Always load templates with UTF-8, but write using the
	// specified encoding.
	private final String encoding;

	private final File runtimeTemplates;

	private final File outputDirectory;

	private final boolean visualize;

	public TestGenerator(String encoding, File runtimeTemplates, File outputDirectory, boolean visualize) {
		this.encoding = encoding;
		this.runtimeTemplates = runtimeTemplates;
		this.outputDirectory = outputDirectory;
		this.visualize = visualize;
	}

	public void execute() {
		STGroup targetGroup = new STGroupFile(runtimeTemplates.getPath());
		targetGroup.registerModelAdaptor(STGroup.class, new STGroupModelAdaptor());
		targetGroup.registerRenderer(String.class, new StringRenderer(), true);
		targetGroup.defineDictionary("escape", new JavaEscapeStringMap());
		targetGroup.defineDictionary("lines", new LinesStringMap());
		targetGroup.defineDictionary("strlen", new StrlenStringMap());

		String rootFolder = "org/antlr/v4/test/runtime/templates";
		generateCodeForFoldersInIndex(targetGroup, rootFolder);
	}

	protected void generateCodeForFoldersInIndex(STGroup targetGroup, String rootFolder) {
		STGroup index = new STGroupFile(rootFolder+"/Index.stg");
		index.load(); // make sure the index group is loaded since we call rawGetDictionary

		Map<String, Object> folders = index.rawGetDictionary("TestFolders");
		if (folders != null) {
			for (String key : folders.keySet()) {
				final String testdir = rootFolder + "/" + key;
				STGroup testIndex = new STGroupFile(testdir + "/Index.stg");
				testIndex.load();
				Map<String, Object> templateNames = testIndex.rawGetDictionary("TestTemplates");
				if ( templateNames != null && !templateNames.isEmpty() ) {
					final ArrayList<String> sortedTemplateNames = new ArrayList<String>(templateNames.keySet());
					Collections.sort(sortedTemplateNames);
					generateTestFile(testIndex, targetGroup,
									 testdir,
									 sortedTemplateNames);
				}
			}
		}
	}

	protected void generateTestFile(STGroup index,
									STGroup targetGroup,
									String testdir,
									Collection<String> testTemplates)
	{
		ErrorBuffer errors = new ErrorBuffer();
		targetGroup.setListener(errors);

		File targetFolder = getOutputDir(testdir);
		String testName = testdir.substring(testdir.lastIndexOf('/') + 1);
		File targetFile = new File(targetFolder, "Test" + testName + ".java");
//		System.out.println("Generating file "+targetFile.getAbsolutePath());
		List<ST> templates = new ArrayList<ST>();
		for (String template : testTemplates) {
			STGroup testGroup = new STGroupFile(testdir + "/" + template + STGroup.GROUP_FILE_EXTENSION);
			importLanguageTemplates(testGroup, targetGroup);
			ST testType = testGroup.getInstanceOf("TestType");
			if (testType == null) {
				warn(String.format("Unable to generate tests for %s: no TestType specified.", template));
				continue;
			}

			ST testMethodTemplate = targetGroup.getInstanceOf(testType.render() + "TestMethod");
			if (testMethodTemplate == null) {
				warn(String.format("Unable to generate tests for %s: TestType '%s' is not supported by the current runtime.", template, testType.render()));
				continue;
			}

			testMethodTemplate.add(testMethodTemplate.impl.formalArguments.keySet().iterator().next(), testGroup);
			templates.add(testMethodTemplate);
		}

		ST testFileTemplate = targetGroup.getInstanceOf("TestFile");
		testFileTemplate.addAggr("file.{Options,name,tests}",
		                         index.rawGetDictionary("Options"),
		                         testName,
		                         templates);

		if (visualize) {
			STViz viz = testFileTemplate.inspect();
			try {
				viz.waitForClose();
			}
			catch (InterruptedException ex) { }
		}

		try {
			String output = testFileTemplate.render();
			if ( errors.errors.size()>0 ) {
				System.err.println("errors in "+targetGroup.getName()+": "+errors);
			}
			writeFile(targetFile, output);
		}
		catch (IOException ex) {
			error(String.format("Failed to write output file: %s", targetFile), ex);
		}
	}

	private void importLanguageTemplates(STGroup testGroup, STGroup languageGroup) {
		// make sure the test group is loaded
		testGroup.load();

		if (testGroup == languageGroup) {
			assert false : "Attempted to import the language group into itself.";
			return;
		}

		if (testGroup.getImportedGroups().isEmpty()) {
			testGroup.importTemplates(languageGroup);
			return;
		}

		if (testGroup.getImportedGroups().contains(languageGroup)) {
			return;
		}

		for (STGroup importedGroup : testGroup.getImportedGroups()) {
			importLanguageTemplates(importedGroup, languageGroup);
		}
	}

	public void writeFile(File file, String content) throws IOException {
		file.getParentFile().mkdirs();

		FileOutputStream fos = new FileOutputStream(file);
		OutputStreamWriter osw = new OutputStreamWriter(fos, encoding != null ? encoding : "UTF-8");
		try {
			osw.write(content);
		}
		finally {
			osw.close();
		}
	}

	public File getOutputDir(String templateFolder) {
		return new File(outputDirectory, templateFolder.substring(0, templateFolder.indexOf("/templates")));
	}

	protected void info(String message) {
		System.out.println("INFO: " + message);
	}

	protected void warn(String message) {
		System.err.println("WARNING: " + message);
	}

	protected void error(String message, Throwable throwable) {
		System.err.println("ERROR: " + message);
	}
}
