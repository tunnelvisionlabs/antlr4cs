# Optimized Fork

The optimized fork of ANTLR 4 is maintained by Sam Harwell at Tunnel Vision Laboratories, LLC. This "bleeding edge" implementation of ANTLR 4 contains numerous features and performance optimizations which are not included in the reference release. In general, these features are not included in the reference release for one or more of the following reasons.

* **Implementation complexity:** Many features of the optimized fork were developed to meet specific concerns found during the use of ANTLR 4 for large-scale commercial applications. In some cases, the implementation of the feature is *extremely* complex, while the target audience likely to benefit from the change is small. The reference release of ANTLR is widely used in both educational scenarios and small- to medium-sized parsing applications where these optimizations are not needed.
* **Incomplete specification and/or implementation:** Some features of the optimized fork were developed to address a specific use case without specific concern to other use cases. These features may or may not work in other use cases.
* **Configuration required:** Some optimizations present in the optimized fork, including some of the most powerful, have the ability to dramatically improve performance for some grammars while reducing performance for others. Optimal use of these features requires manual configuration with respect to a specific grammar, and in some cases knowledge about characteristics of the machine where the application is being executed. While these features are essential for certain applications, including them all in a reference release of ANTLR would be quite overwhelming for new users.

## Features

### Base Context

**Reason for exclusion:** Incomplete implementation (not known to work in all grammars)

This release has the ability to instruct multiple rules in a grammar to use the same context object. This feature was originally intended for use in cases like the following, which is extracted from a [Java 8 grammar](https://github.com/sharwell/antlr4/blob/java8-grammar/tool/test/org/antlr/v4/test/Java.g4) derived from the language specification (many uses of the feature are seen in this grammar).

```antlr
primitiveType
    :   annotation* numericType
    |   annotation* 'boolean'
    ;

unannPrimitiveType
options { baseContext=primitiveType; }
    :   numericType
    |   'boolean'
    ;
```

In the above example, the rule `unannPrimitiveType` does not allow annotations, but the parse tree will still contain `PrimitiveTypeContext`

### Automated left factoring

**Reason for exclusion:** Incomplete implementation (not known to work in all grammars)

This release has the ability to automatically left-factor a named rule reference from alternatives of a rule. This feature was created a few releases into GoWorks in an effort to reduce memory consumption by reducing the lookahead requirements for a specific rule in the grammar. Many features of GoWorks were already written with a specific parse tree shape in mind, and changing the shape of the parse tree in order to manually left factor the offending rule would be both time-consuming and risky for introducing bugs. To address this, I created a parse-tree-preserving grammar transformation which left-factors a rule without changing the shape of the final parse tree. GoWorks [uses the feature](https://github.com/tunnelvisionlabs/goworks/blob/5543a633dfc0d9b0e2ab407445a82bc70b24f100/goworks.editor/src/org/tvl/goworks/editor/go/parser/GoParser.g4#L435-L444) to left factor its *expression* rule out of the *simpleStmt* rule.

```antlr
simpleStmt
@version{1}
@leftfactor{expression}
    :   shortVarDecl
    |   sendStmt
    |   incDecStmt
    |   assignment
    |   expressionStmt
    |   emptyStmt
    ;
```

> :warning: While the parse tree shape will not be changed for *correct* input, it is possible that use of the automatic left factoring functionality could change the behavior in error handling scenarios. Specifically, it may be possible for erroneous input to result in parse trees where nodes are placed in locations that appear impossible according to the grammar.

### Indirect left recursion elimination

**Reason for exclusion:** Incomplete implementation (not known to work in all grammars)

This release has the ability to eliminate indirect left recursion (reported by the reference release as "mutual" left recursion). This feature is an early expansion of the ability to automatically left factor a grammar.

### Backwards compatibility

**Reason for exclusion:** Implementation complexity (quite constraining to the benefit of only a few users)

The optimized fork releases have a stronger emphasis on preserving compatibility. Unlike the reference release, grammars typically do not need to be regenerated when upgrading the runtime. However, we still recommend that grammars be generated using the new version as it may improve performance or the available features at runtime.

### `@Nullable` and `@NotNull` annotations

**Reason for exclusion:** Implementation complexity

### Rule versioning

**Reason for exclusion:** Implementation complexity

The optimized fork includes the [rule versioning](https://github.com/sharwell/antlr4/wiki/Rule-Versioning) feature.

## Optimizations

### Improved SLL termination condition

**Reason for exclusion:** Implementation complexity

This release of ANTLR 4 uses a stronger termination condition for SLL prediction. In some cases it is possible for this implementation to detect an SLL conflict with fewer symbols of lookahead than the reference release. In general, this change would not be observable. However, aside from the performance benefit it is possible for the shorter lookahead to allow for better error handling in some edge cases.

### Full context DFA

**Reason for exclusion:** Implementation complexity, unproven theoretical impact on algorithmic complexity of ALL(*)

The reference release of ANTLR 4 only uses a DFA for local-context prediction (SLL). The optimized fork expands on that by allowing the use of a DFA for full-context prediction (LL) as well.

> :bulb: This optimization is disabled by default. It can be enabled by setting `ParserATNSimulator.enable_global_context_dfa` to true:
>
> ```java
> Parser myParser = new MyParser(input);
> myParser.getInterpreter().enable_global_context_dfa = true;
> ```
>
> :warning: This feature can substantially increase the memory consumed by the DFA. For grammars and applications that rarely need to use full context prediction, especially in combination with two-stage parsing, the overhead of this feature may not provide gains that justify its use. I recommend leaving this feature disabled initially, and only experiment with it if you find that other options do not provide an acceptable level of performance. With that said, there are multiple known grammars which are practically unusable without this feature.

### Tail call elimination

**Reason for exclusion:** Incomplete specification (work in progress)

The optimized fork of ANTLR compacts `PredictionContext` instances associated with states in the DFA by eliminating certain unnecessary return states. As an added bonus, this feature reduced overall DFA storage requirements by allowing `DFAState` instances to be shared in scenarios where the reference release of ANTLR believes the states to be distinct.

> :bulb: This feature includes a configuration option `ParserATNSimulator.tail_call_preserves_sll`, which has a default value of `true`. Before enabling this feature, be aware of the following advantages and disadvantages.
>
> * Advantages of `tail_call_preserves_sll=true` (the default)
>   * Preserves maximum accuracy of `PredictionMode.SLL`. When the setting is false, it is possible (varies by grammar and input) for `PredictionMode.SLL` to report a parse error even though the input successfully parses when the setting is true.
>   * Minimizes lookahead of SLL decisions. When the setting is false, it is possible (varies by grammar and input) for SLL prediction - which is enabled even for `PredictionMode.LL` - to increase the lookahead. In one case it was observed that overall performance was degraded due to the impact of this.
> * Advantages of `tail_call_preserves_sll=false`
>   * Reduces DFA size. For cases where two-stage parsing is used (so reduced accuracy of SLL mode is acceptable) and lookahead doesn't hit a pathological case, explicitly setting `tail_call_preserves_sll` to false can substantially reduce the size of the DFA.

### DFA edge optimization

**Reason for exclusion:** Implementation complexity (the reference release uses a subset of this optimization)

The optimized fork uses several different map implementations based on the number of outgoing edges held in a DFA state. This feature minimizes the size of DFA states, especially in infrequently used areas of the DFA.

### ATN configuration optimization

**Reason for exclusion:** Implementation complexity (most small- and medium-sized applications don't run into DFA-related memory problems)

The optimized fork uses several `ATNConfig` classes to reduce the size of the DFA. For configurations returning default values from most properties, a small `ATNConfig` instance is used. The larger types are only used for configurations that need to represent non-default values from one or more methods. As a simple example, configurations appearing in the lexer DFA need to store a few more fields than configurations appearing in the parser DFA. By removing these fields from `ATNConfig` instances used in the parser DFA, some applications observe marked reductions in the memory overhead for the parser DFA (we've seen 20MiB or more for large applications).

### Prediction context optimization

**Reason for exclusion:** Implementation complexity

The optimized release uses an exact implementation for merging `PredictionContext` instances. In some cases, the reference release produces prediction context graphs which are not fully reduced (maximum sharing of nodes in the graph). The algorithm used by the optimized fork implements an exact merge for these contexts, so the `PredictionContext` instances appearing in the DFA cache are fully reduced.
