/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
package org.antlr.v4.runtime;

import org.antlr.v4.runtime.misc.NotNull;
import org.antlr.v4.runtime.misc.Nullable;
import org.antlr.v4.runtime.misc.Utils;

import java.io.IOException;

/**
 * This is an {@link ANTLRInputStream} that is loaded from a file all at once
 * when you construct the object.
 */
public class ANTLRFileStream extends ANTLRInputStream {
	protected String fileName;

	public ANTLRFileStream(@NotNull String fileName) throws IOException {
		this(fileName, null);
	}

	public ANTLRFileStream(@NotNull String fileName, String encoding) throws IOException {
		this.fileName = fileName;
		load(fileName, encoding);
	}

	public void load(@NotNull String fileName, @Nullable String encoding)
		throws IOException
	{
		data = Utils.readFile(fileName, encoding);
		this.n = data.length;
	}

	@Override
	public String getSourceName() {
		return fileName;
	}
}
