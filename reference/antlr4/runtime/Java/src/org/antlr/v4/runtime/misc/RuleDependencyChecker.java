/*
 [The "BSD license"]
 Copyright (c) 2012 Terence Parr
 Copyright (c) 2012 Sam Harwell
 All rights reserved.

 Redistribution and use in source and binary forms, with or without
 modification, are permitted provided that the following conditions
 are met:

 1. Redistributions of source code must retain the above copyright
    notice, this list of conditions and the following disclaimer.
 2. Redistributions in binary form must reproduce the above copyright
    notice, this list of conditions and the following disclaimer in the
    documentation and/or other materials provided with the distribution.
 3. The name of the author may not be used to endorse or promote products
    derived from this software without specific prior written permission.

 THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
package org.antlr.v4.runtime.misc;

import org.antlr.v4.runtime.Dependents;
import org.antlr.v4.runtime.Recognizer;
import org.antlr.v4.runtime.RuleDependencies;
import org.antlr.v4.runtime.RuleDependency;
import org.antlr.v4.runtime.RuleVersion;
import org.antlr.v4.runtime.atn.ATN;
import org.antlr.v4.runtime.atn.ATNDeserializer;
import org.antlr.v4.runtime.atn.ATNState;
import org.antlr.v4.runtime.atn.RuleTransition;
import org.antlr.v4.runtime.atn.Transition;
import org.antlr.v4.runtime.atn.TransitionType;

import java.lang.annotation.ElementType;
import java.lang.annotation.Target;
import java.lang.reflect.AnnotatedElement;
import java.lang.reflect.Constructor;
import java.lang.reflect.Field;
import java.lang.reflect.Method;
import java.lang.reflect.Modifier;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.BitSet;
import java.util.EnumSet;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 *
 * @author Sam Harwell
 */
public class RuleDependencyChecker {
	private static final Logger LOGGER = Logger.getLogger(RuleDependencyChecker.class.getName());

	private static final Set<Class<?>> checkedTypes = new HashSet<Class<?>>();

	public static void checkDependencies(Class<?> dependentClass) {
		if (isChecked(dependentClass)) {
			return;
		}

		List<Class<?>> typesToCheck = getTypesToCheck(dependentClass);
		for (final Class<?> clazz : typesToCheck) {
			if (isChecked(clazz)) {
				continue;
			}

			List<Tuple2<RuleDependency, AnnotatedElement>> dependencies = getDependencies(clazz);
			if (dependencies.isEmpty()) {
				continue;
			}

			Map<Class<? extends Recognizer<?, ?>>, List<Tuple2<RuleDependency, AnnotatedElement>>> recognizerDependencies
				= new HashMap<Class<? extends Recognizer<?, ?>>, List<Tuple2<RuleDependency, AnnotatedElement>>>();
			for (Tuple2<RuleDependency, AnnotatedElement> dependency : dependencies) {
				Class<? extends Recognizer<?, ?>> recognizerType = dependency.getItem1().recognizer();
				List<Tuple2<RuleDependency, AnnotatedElement>> list = recognizerDependencies.get(recognizerType);
				if (list == null) {
					list = new ArrayList<Tuple2<RuleDependency, AnnotatedElement>>();
					recognizerDependencies.put(recognizerType, list);
				}

				list.add(dependency);
			}

			for (Map.Entry<Class<? extends Recognizer<?, ?>>, List<Tuple2<RuleDependency, AnnotatedElement>>> entry : recognizerDependencies.entrySet()) {
				//processingEnv.getMessager().printMessage(Diagnostic.Kind.NOTE, String.format("ANTLR 4: Validating %d dependencies on rules in %s.", entry.getValue().size(), entry.getKey().toString()));
				checkDependencies(entry.getValue(), entry.getKey());
			}

			checkDependencies(dependencies, dependencies.get(0).getItem1().recognizer());
		}
	}

	private static List<Class<?>> getTypesToCheck(Class<?> clazz) {
		Set<Class<?>> result = new HashSet<Class<?>>();
		getTypesToCheck(clazz, result);
		return new ArrayList<Class<?>>(result);
	}

	private static void getTypesToCheck(Class<?> clazz, Set<Class<?>> result) {
		if (!result.add(clazz)) {
			return;
		}

		for (Class<?> declared : clazz.getDeclaredClasses()) {
			getTypesToCheck(declared, result);
		}
	}

	private static boolean isChecked(Class<?> clazz) {
		synchronized (checkedTypes) {
			return checkedTypes.contains(clazz);
		}
	}

	private static void markChecked(Class<?> clazz) {
		synchronized (checkedTypes) {
			checkedTypes.add(clazz);
		}
	}

	private static void checkDependencies(List<Tuple2<RuleDependency, AnnotatedElement>> dependencies, Class<? extends Recognizer<?, ?>> recognizerType) {
		String[] ruleNames = getRuleNames(recognizerType);
		int[] ruleVersions = getRuleVersions(recognizerType, ruleNames);
		RuleRelations relations = extractRuleRelations(recognizerType);
		StringBuilder errors = new StringBuilder();

		for (Tuple2<RuleDependency, AnnotatedElement> dependency : dependencies) {
			if (!dependency.getItem1().recognizer().isAssignableFrom(recognizerType)) {
				continue;
			}

			// this is the rule in the dependency set with the highest version number
			int effectiveRule = dependency.getItem1().rule();
			if (effectiveRule < 0 || effectiveRule >= ruleVersions.length) {
				String message = String.format("Rule dependency on unknown rule %d@%d in %s%n",
											   dependency.getItem1().rule(),
											   dependency.getItem1().version(),
											   dependency.getItem1().recognizer().toString());

				errors.append(message);
				continue;
			}

			EnumSet<Dependents> dependents = EnumSet.of(Dependents.SELF, dependency.getItem1().dependents());
			reportUnimplementedDependents(errors, dependency, dependents);

			BitSet checked = new BitSet();

			int highestRequiredDependency = checkDependencyVersion(errors, dependency, ruleNames, ruleVersions, effectiveRule, null);

			if (dependents.contains(Dependents.PARENTS)) {
				BitSet parents = relations.parents[dependency.getItem1().rule()];
				for (int parent = parents.nextSetBit(0); parent >= 0; parent = parents.nextSetBit(parent + 1)) {
					if (parent < 0 || parent >= ruleVersions.length || checked.get(parent)) {
						continue;
					}

					checked.set(parent);
					int required = checkDependencyVersion(errors, dependency, ruleNames, ruleVersions, parent, "parent");
					highestRequiredDependency = Math.max(highestRequiredDependency, required);
				}
			}

			if (dependents.contains(Dependents.CHILDREN)) {
				BitSet children = relations.children[dependency.getItem1().rule()];
				for (int child = children.nextSetBit(0); child >= 0; child = children.nextSetBit(child + 1)) {
					if (child < 0 || child >= ruleVersions.length || checked.get(child)) {
						continue;
					}

					checked.set(child);
					int required = checkDependencyVersion(errors, dependency, ruleNames, ruleVersions, child, "child");
					highestRequiredDependency = Math.max(highestRequiredDependency, required);
				}
			}

			if (dependents.contains(Dependents.ANCESTORS)) {
				BitSet ancestors = relations.getAncestors(dependency.getItem1().rule());
				for (int ancestor = ancestors.nextSetBit(0); ancestor >= 0; ancestor = ancestors.nextSetBit(ancestor + 1)) {
					if (ancestor < 0 || ancestor >= ruleVersions.length || checked.get(ancestor)) {
						continue;
					}

					checked.set(ancestor);
					int required = checkDependencyVersion(errors, dependency, ruleNames, ruleVersions, ancestor, "ancestor");
					highestRequiredDependency = Math.max(highestRequiredDependency, required);
				}
			}

			if (dependents.contains(Dependents.DESCENDANTS)) {
				BitSet descendants = relations.getDescendants(dependency.getItem1().rule());
				for (int descendant = descendants.nextSetBit(0); descendant >= 0; descendant = descendants.nextSetBit(descendant + 1)) {
					if (descendant < 0 || descendant >= ruleVersions.length || checked.get(descendant)) {
						continue;
					}

					checked.set(descendant);
					int required = checkDependencyVersion(errors, dependency, ruleNames, ruleVersions, descendant, "descendant");
					highestRequiredDependency = Math.max(highestRequiredDependency, required);
				}
			}

			int declaredVersion = dependency.getItem1().version();
			if (declaredVersion > highestRequiredDependency) {
				String message = String.format("Rule dependency version mismatch: %s has maximum dependency version %d (expected %d) in %s%n",
											   ruleNames[dependency.getItem1().rule()],
											   highestRequiredDependency,
											   declaredVersion,
											   dependency.getItem1().recognizer().toString());

				errors.append(message);
			}
		}

		if (errors.length() > 0) {
			throw new IllegalStateException(errors.toString());
		}

		markChecked(recognizerType);
	}

	private static final Set<Dependents> IMPLEMENTED_DEPENDENTS = EnumSet.of(Dependents.SELF, Dependents.PARENTS, Dependents.CHILDREN, Dependents.ANCESTORS, Dependents.DESCENDANTS);

	private static void reportUnimplementedDependents(StringBuilder errors, Tuple2<RuleDependency, AnnotatedElement> dependency, EnumSet<Dependents> dependents) {
		EnumSet<Dependents> unimplemented = dependents.clone();
		unimplemented.removeAll(IMPLEMENTED_DEPENDENTS);
		if (!unimplemented.isEmpty()) {
			String message = String.format("Cannot validate the following dependents of rule %d: %s%n",
										   dependency.getItem1().rule(),
										   unimplemented);

			errors.append(message);
		}
	}

	private static int checkDependencyVersion(StringBuilder errors, Tuple2<RuleDependency, AnnotatedElement> dependency, String[] ruleNames, int[] ruleVersions, int relatedRule, String relation) {
		String ruleName = ruleNames[dependency.getItem1().rule()];
		String path;
		if (relation == null) {
			path = ruleName;
		}
		else {
			String mismatchedRuleName = ruleNames[relatedRule];
			path = String.format("rule %s (%s of %s)", mismatchedRuleName, relation, ruleName);
		}

		int declaredVersion = dependency.getItem1().version();
		int actualVersion = ruleVersions[relatedRule];
		if (actualVersion > declaredVersion) {
			String message = String.format("Rule dependency version mismatch: %s has version %d (expected <= %d) in %s%n",
										   path,
										   actualVersion,
										   declaredVersion,
										   dependency.getItem1().recognizer().toString());

			errors.append(message);
		}

		return actualVersion;
	}

	private static int[] getRuleVersions(Class<? extends Recognizer<?, ?>> recognizerClass, String[] ruleNames) {
		int[] versions = new int[ruleNames.length];

		Field[] fields = recognizerClass.getFields();
		for (Field field : fields) {
			boolean isStatic = (field.getModifiers() & Modifier.STATIC) != 0;
			boolean isInteger = field.getType() == Integer.TYPE;
			if (isStatic && isInteger && field.getName().startsWith("RULE_")) {
				try {
					String name = field.getName().substring("RULE_".length());
					if (name.isEmpty() || !Character.isLowerCase(name.charAt(0))) {
						continue;
					}

					int index = field.getInt(null);
					if (index < 0 || index >= versions.length) {
						Object[] params = { index, field.getName(), recognizerClass.getSimpleName() };
						LOGGER.log(Level.WARNING, "Rule index {0} for rule ''{1}'' out of bounds for recognizer {2}.", params);
						continue;
					}

					Method ruleMethod = getRuleMethod(recognizerClass, name);
					if (ruleMethod == null) {
						Object[] params = { name, recognizerClass.getSimpleName() };
						LOGGER.log(Level.WARNING, "Could not find rule method for rule ''{0}'' in recognizer {1}.", params);
						continue;
					}

					RuleVersion ruleVersion = ruleMethod.getAnnotation(RuleVersion.class);
					int version = ruleVersion != null ? ruleVersion.value() : 0;
					versions[index] = version;
				} catch (IllegalArgumentException ex) {
					LOGGER.log(Level.WARNING, null, ex);
				} catch (IllegalAccessException ex) {
					LOGGER.log(Level.WARNING, null, ex);
				}
			}
		}

		return versions;
	}

	private static Method getRuleMethod(Class<? extends Recognizer<?, ?>> recognizerClass, String name) {
		Method[] declaredMethods = recognizerClass.getMethods();
		for (Method method : declaredMethods) {
			if (method.getName().equals(name) && method.isAnnotationPresent(RuleVersion.class)) {
				return method;
			}
		}

		return null;
	}

	private static String[] getRuleNames(Class<? extends Recognizer<?, ?>> recognizerClass) {
		try {
			Field ruleNames = recognizerClass.getField("ruleNames");
			return (String[])ruleNames.get(null);
		} catch (NoSuchFieldException ex) {
			LOGGER.log(Level.WARNING, null, ex);
		} catch (SecurityException ex) {
			LOGGER.log(Level.WARNING, null, ex);
		} catch (IllegalArgumentException ex) {
			LOGGER.log(Level.WARNING, null, ex);
		} catch (IllegalAccessException ex) {
			LOGGER.log(Level.WARNING, null, ex);
		}

		return new String[0];
	}

	public static List<Tuple2<RuleDependency, AnnotatedElement>> getDependencies(Class<?> clazz) {
		List<Tuple2<RuleDependency, AnnotatedElement>> result = new ArrayList<Tuple2<RuleDependency, AnnotatedElement>>();
		List<ElementType> supportedTarget = Arrays.asList(RuleDependency.class.getAnnotation(Target.class).value());
		for (ElementType target : supportedTarget) {
			switch (target) {
			case TYPE:
				if (!clazz.isAnnotation()) {
					getElementDependencies(clazz, result);
				}
				break;
			case ANNOTATION_TYPE:
				if (!clazz.isAnnotation()) {
					getElementDependencies(clazz, result);
				}
				break;
			case CONSTRUCTOR:
				for (Constructor<?> ctor : clazz.getDeclaredConstructors()) {
					getElementDependencies(ctor, result);
				}
				break;
			case FIELD:
				for (Field field : clazz.getDeclaredFields()) {
					getElementDependencies(field, result);
				}
				break;
			case LOCAL_VARIABLE:
				System.err.println("Runtime rule dependency checking is not supported for local variables.");
				break;
			case METHOD:
				for (Method method : clazz.getDeclaredMethods()) {
					getElementDependencies(method, result);
				}
				break;
			case PACKAGE:
				// package is not a subset of class, so nothing to do here
				break;
			case PARAMETER:
				System.err.println("Runtime rule dependency checking is not supported for parameters.");
				break;
			}
		}

		return result;
	}

	private static void getElementDependencies(AnnotatedElement annotatedElement, List<Tuple2<RuleDependency, AnnotatedElement>> result) {
		RuleDependency dependency = annotatedElement.getAnnotation(RuleDependency.class);
		if (dependency != null) {
			result.add(Tuple.create(dependency, annotatedElement));
		}

		RuleDependencies dependencies = annotatedElement.getAnnotation(RuleDependencies.class);
		if (dependencies != null) {
			for (RuleDependency d : dependencies.value()) {
				if (d != null) {
					result.add(Tuple.create(d, annotatedElement));
				}
			}
		}
	}

	private static RuleRelations extractRuleRelations(Class<? extends Recognizer<?, ?>> recognizer) {
		String serializedATN = getSerializedATN(recognizer);
		if (serializedATN == null) {
			return null;
		}

		ATN atn = new ATNDeserializer().deserialize(serializedATN.toCharArray());
		RuleRelations relations = new RuleRelations(atn.ruleToStartState.length);
		for (ATNState state : atn.states) {
			if (!state.epsilonOnlyTransitions) {
				continue;
			}

			for (Transition transition : state.getTransitions()) {
				if (transition.getSerializationType() != TransitionType.RULE) {
					continue;
				}

				RuleTransition ruleTransition = (RuleTransition)transition;
				relations.addRuleInvocation(state.ruleIndex, ruleTransition.target.ruleIndex);
			}
		}

		return relations;
	}

	private static String getSerializedATN(Class<?> recognizerClass) {
		try {
			Field serializedAtnField = recognizerClass.getDeclaredField("_serializedATN");
			if (Modifier.isStatic(serializedAtnField.getModifiers())) {
				return (String)serializedAtnField.get(null);
			}

			return null;
		} catch (NoSuchFieldException ex) {
			if (recognizerClass.getSuperclass() != null) {
				return getSerializedATN(recognizerClass.getSuperclass());
			}

			return null;
		} catch (SecurityException ex) {
			return null;
		} catch (IllegalArgumentException ex) {
			return null;
		} catch (IllegalAccessException ex) {
			return null;
		}
	}

	private static final class RuleRelations {
		private final BitSet[] parents;
		private final BitSet[] children;

		public RuleRelations(int ruleCount) {
			parents = new BitSet[ruleCount];
			for (int i = 0; i < ruleCount; i++) {
				parents[i] = new BitSet();
			}

			children = new BitSet[ruleCount];
			for (int i = 0; i < ruleCount; i++) {
				children[i] = new BitSet();
			}
		}

		public boolean addRuleInvocation(int caller, int callee) {
			if (caller < 0) {
				// tokens rule
				return false;
			}

			if (children[caller].get(callee)) {
				// already added
				return false;
			}

			children[caller].set(callee);
			parents[callee].set(caller);
			return true;
		}

		public BitSet getAncestors(int rule) {
			BitSet ancestors = new BitSet();
			ancestors.or(parents[rule]);
			while (true) {
				int cardinality = ancestors.cardinality();
				for (int i = ancestors.nextSetBit(0); i >= 0; i = ancestors.nextSetBit(i + 1)) {
					ancestors.or(parents[i]);
				}

				if (ancestors.cardinality() == cardinality) {
					// nothing changed
					break;
				}
			}

			return ancestors;
		}

		public BitSet getDescendants(int rule) {
			BitSet descendants = new BitSet();
			descendants.or(children[rule]);
			while (true) {
				int cardinality = descendants.cardinality();
				for (int i = descendants.nextSetBit(0); i >= 0; i = descendants.nextSetBit(i + 1)) {
					descendants.or(children[i]);
				}

				if (descendants.cardinality() == cardinality) {
					// nothing changed
					break;
				}
			}

			return descendants;
		}
	}

	private RuleDependencyChecker() {
	}
}
