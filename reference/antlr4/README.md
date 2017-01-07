# ANTLR v4

[![Java 6+](https://img.shields.io/badge/java-6+-4c7e9f.svg)](http://java.oracle.com) [![License](https://img.shields.io/badge/license-BSD-blue.svg)](https://raw.githubusercontent.com/tunnelvisionlabs/antlr4/master/LICENSE.txt)

**ANTLR** (ANother Tool for Language Recognition) is a powerful parser generator for reading, processing, executing, or translating structured text or binary files. It's widely used to build languages, tools, and frameworks. From a grammar, ANTLR generates a parser that can build parse trees and also generates a listener interface (or visitor) that makes it easy to respond to the recognition of phrases of interest.

## This is a fork

This is the "optimized" fork of ANTLR 4, which contains many features and optimizations not present in the reference release. For information about these features, see [doc/optimized-fork.md](doc/optimized-fork.md).

## Build Status

[![Build Travis-CI Status](https://travis-ci.org/tunnelvisionlabs/antlr4.svg?branch=master)](https://travis-ci.org/tunnelvisionlabs/antlr4) [![Build AppVeyor Status](https://ci.appveyor.com/api/projects/status/ba3jofc6j63wrl89/branch/master?svg=true)](https://ci.appveyor.com/project/sharwell/antlr4/branch/master)

[![codecov](https://codecov.io/gh/tunnelvisionlabs/antlr4/branch/master/graph/badge.svg)](https://codecov.io/gh/tunnelvisionlabs/antlr4)

## Authors and major contributors

* [Terence Parr](http://www.cs.usfca.edu/~parrt/), parrt@cs.usfca.edu
ANTLR project lead and supreme dictator for life
[University of San Francisco](http://www.usfca.edu/)
* [Sam Harwell](http://tunnelvisionlabs.com/) (Tool co-author, lead developer for the optimized fork along with the optimized Java, C#, and TypeScript targets)
* Burt Harris (Co-author of the optimized TypeScript target)

## Useful information

* [Release notes](https://github.com/tunnelvisionlabs/antlr4/releases)
* [Getting started with v4](https://github.com/tunnelvisionlabs/antlr4/blob/master/doc/getting-started.md)
* [Official site](http://www.antlr.org/)
* [Reference (non-optimized) release](https://github.com/antlr/antlr4)
* [Documentation](https://github.com/tunnelvisionlabs/antlr4/blob/master/doc/index.md)
* [FAQ](https://github.com/tunnelvisionlabs/antlr4/blob/master/doc/faq/index.md)
* [ANTLR optimized code generation targets](https://github.com/tunnelvisionlabs/antlr4/blob/master/doc/targets.md)
* [Java API](http://www.antlr.org/api/Java/index.html)
* [ANTLR v3](http://www.antlr3.org/)
* [v3 to v4 Migration, differences](https://github.com/tunnelvisionlabs/antlr4/blob/master/doc/faq/general.md)

## The Definitive ANTLR 4 Reference

Programmers run into parsing problems all the time. Whether it’s a data format like JSON, a network protocol like SMTP, a server configuration file for Apache, a PostScript/PDF file, or a simple spreadsheet macro language—ANTLR v4 and this book will demystify the process. ANTLR v4 has been rewritten from scratch to make it easier than ever to build parsers and the language applications built on top. This completely rewritten new edition of the bestselling Definitive ANTLR Reference shows you how to take advantage of these new features.

You can buy the book [The Definitive ANTLR 4 Reference](http://amzn.com/1934356999) at amazon or an [electronic version at the publisher's site](https://pragprog.com/book/tpantlr2/the-definitive-antlr-4-reference).

You will find the [Book source code](http://pragprog.com/titles/tpantlr2/source_code) useful.

## Additional grammars
[This repository](https://github.com/antlr/grammars-v4) is a collection of grammars without actions where the
root directory name is the all-lowercase name of the language parsed
by the grammar. For example, java, cpp, csharp, c, etc...
