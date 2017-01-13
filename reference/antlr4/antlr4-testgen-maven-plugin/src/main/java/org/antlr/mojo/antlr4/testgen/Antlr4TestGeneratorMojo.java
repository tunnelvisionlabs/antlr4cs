/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
package org.antlr.mojo.antlr4.testgen;

import org.antlr.v4.testgen.TestGenerator;
import org.apache.maven.plugin.AbstractMojo;
import org.apache.maven.plugin.MojoExecutionException;
import org.apache.maven.plugin.MojoFailureException;
import org.apache.maven.plugins.annotations.LifecyclePhase;
import org.apache.maven.plugins.annotations.Mojo;
import org.apache.maven.plugins.annotations.Parameter;
import org.apache.maven.plugins.annotations.ResolutionScope;
import org.apache.maven.project.MavenProject;

import java.io.File;

@Mojo(
	name = "antlr4.testgen",
	defaultPhase = LifecyclePhase.GENERATE_TEST_SOURCES,
	requiresDependencyResolution = ResolutionScope.TEST,
	requiresProject = true)
public class Antlr4TestGeneratorMojo extends AbstractMojo {

	// This project uses UTF-8, but the plugin might be used in another project
	// which is not. Always load templates with UTF-8, but write using the
	// specified encoding.
	@Parameter(property = "project.build.sourceEncoding")
	private String encoding;

	@Parameter(property = "project", readonly = true)
	private MavenProject project;

	@Parameter(property = "runtimeTemplates", required = true)
	private File runtimeTemplates;

	@Parameter(defaultValue = "${project.build.directory}/generated-test-sources/antlr4-tests")
	private File outputDirectory;

	@Parameter
	private boolean visualize;

	@Override
	public void execute() throws MojoExecutionException, MojoFailureException {
		TestGenerator testGenerator = new MavenTestGenerator(encoding, runtimeTemplates, outputDirectory, visualize);
		testGenerator.execute();

		if (project != null) {
			project.addTestCompileSourceRoot(outputDirectory.getPath());
		}
	}

	private class MavenTestGenerator extends TestGenerator {

		public MavenTestGenerator(String encoding, File runtimeTemplates, File outputDirectory, boolean visualize) {
			super(encoding, runtimeTemplates, outputDirectory, visualize);
		}

		@Override
		protected void warn(String message) {
			getLog().warn(message);
		}

		@Override
		protected void error(String message, Throwable throwable) {
			getLog().error(message, throwable);
		}

	}
}
