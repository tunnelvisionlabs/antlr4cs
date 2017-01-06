/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.atn;

import java.util.HashMap;
import java.util.Map;

/** Used to cache {@link PredictionContext} objects. Its used for the shared
 *  context cash associated with contexts in DFA states. This cache
 *  can be used for both lexers and parsers.
 *
 * @author Sam Harwell
 */
public class PredictionContextCache {
    public static final PredictionContextCache UNCACHED = new PredictionContextCache(false);

    private final Map<PredictionContext, PredictionContext> contexts =
        new HashMap<PredictionContext, PredictionContext>();
    private final Map<PredictionContextAndInt, PredictionContext> childContexts =
        new HashMap<PredictionContextAndInt, PredictionContext>();
    private final Map<IdentityCommutativePredictionContextOperands, PredictionContext> joinContexts =
        new HashMap<IdentityCommutativePredictionContextOperands, PredictionContext>();

    private final boolean enableCache;

    public PredictionContextCache() {
        this(true);
    }

    private PredictionContextCache(boolean enableCache) {
        this.enableCache = enableCache;
    }

    public PredictionContext getAsCached(PredictionContext context) {
        if (!enableCache) {
            return context;
        }

        PredictionContext result = contexts.get(context);
        if (result == null) {
            result = context;
            contexts.put(context, context);
        }

        return result;
    }

    public PredictionContext getChild(PredictionContext context, int invokingState) {
        if (!enableCache) {
            return context.getChild(invokingState);
        }

        PredictionContextAndInt operands = new PredictionContextAndInt(context, invokingState);
        PredictionContext result = childContexts.get(operands);
        if (result == null) {
            result = context.getChild(invokingState);
            result = getAsCached(result);
            childContexts.put(operands, result);
        }

        return result;
    }

    public PredictionContext join(PredictionContext x, PredictionContext y) {
        if (!enableCache) {
            return PredictionContext.join(x, y, this);
        }

        IdentityCommutativePredictionContextOperands operands = new IdentityCommutativePredictionContextOperands(x, y);
        PredictionContext result = joinContexts.get(operands);
        if (result != null) {
            return result;
        }

        result = PredictionContext.join(x, y, this);
        result = getAsCached(result);
        joinContexts.put(operands, result);
        return result;
    }

    protected static final class PredictionContextAndInt {
        private final PredictionContext obj;
        private final int value;

        public PredictionContextAndInt(PredictionContext obj, int value) {
            this.obj = obj;
            this.value = value;
        }

        @Override
        public boolean equals(Object obj) {
            if (!(obj instanceof PredictionContextAndInt)) {
                return false;
            } else if (obj == this) {
                return true;
            }

            PredictionContextAndInt other = (PredictionContextAndInt)obj;
            return this.value == other.value
                && (this.obj == other.obj || (this.obj != null && this.obj.equals(other.obj)));
        }

        @Override
        public int hashCode() {
            int hashCode = 5;
            hashCode = 7 * hashCode + (obj != null ? obj.hashCode() : 0);
            hashCode = 7 * hashCode + value;
            return hashCode;
        }
    }

    protected static final class IdentityCommutativePredictionContextOperands {

        private final PredictionContext x;
        private final PredictionContext y;

        public IdentityCommutativePredictionContextOperands(PredictionContext x, PredictionContext y) {
            this.x = x;
            this.y = y;
        }

        public PredictionContext getX() {
            return x;
        }

        public PredictionContext getY() {
            return y;
        }

        @Override
        public boolean equals(Object obj) {
            if (!(obj instanceof IdentityCommutativePredictionContextOperands)) {
                return false;
            }
            else if (this == obj) {
                return true;
            }

            IdentityCommutativePredictionContextOperands other = (IdentityCommutativePredictionContextOperands)obj;
            return (this.x == other.x && this.y == other.y) || (this.x == other.y && this.y == other.x);
        }

        @Override
        public int hashCode() {
            return x.hashCode() ^ y.hashCode();
        }
    }

}
