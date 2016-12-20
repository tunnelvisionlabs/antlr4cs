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

namespace Antlr4.Codegen
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Antlr4.Codegen.Model;
    using Antlr4.Misc;
    using Antlr4.StringTemplate;
    using Antlr4.StringTemplate.Compiler;
    using Antlr4.Tool;
    using DictionaryEntry = System.Collections.DictionaryEntry;
    using FieldAccessException = System.FieldAccessException;
    using IDictionary = System.Collections.IDictionary;
    using IEnumerable = System.Collections.IEnumerable;
    using Type = System.Type;
    using IDictionaryEnumerator = System.Collections.IDictionaryEnumerator;

    /** Convert an output model tree to template hierarchy by walking
     *  the output model. Each output model object has a corresponding template
     *  of the same name.  An output model object can have nested objects.
     *  We identify those nested objects by the list of arguments in the template
     *  definition. For example, here is the definition of the parser template:
     *
     *  Parser(parser, scopes, functions) ::= &lt;&lt;...&gt;&gt;
     *
     *  The first template argument is always the output model object from which
     *  this walker will create the template. Any other arguments identify
     *  the field names within the output model object of nested model objects.
     *  So, in this case, template Parser is saying that output model object
     *  Parser has two fields the walker should chase called a scopes and functions.
     *
     *  This simple mechanism means we don't have to include code in every
     *  output model object that says how to create the corresponding template.
     */
    public class OutputModelWalker
    {
        internal AntlrTool tool;
        internal TemplateGroup templates;

        public OutputModelWalker(AntlrTool tool, TemplateGroup templates)
        {
            this.tool = tool;
            this.templates = templates;
        }

        public virtual Template Walk(OutputModelObject omo)
        {
            // CREATE TEMPLATE FOR THIS OUTPUT OBJECT
            Type cl = omo.GetType();
            string templateName = cl.Name;
            if (templateName == null)
            {
                tool.errMgr.ToolError(ErrorType.NO_MODEL_TO_TEMPLATE_MAPPING, cl.Name);
                return new Template("[" + templateName + " invalid]");
            }
            Template st = templates.GetInstanceOf(templateName);
            if (st == null)
            {
                tool.errMgr.ToolError(ErrorType.CODE_GEN_TEMPLATES_INCOMPLETE, templateName);
                return new Template("[" + templateName + " invalid]");
            }
            if (st.impl.FormalArguments == null)
            {
                tool.errMgr.ToolError(ErrorType.CODE_TEMPLATE_ARG_ISSUE, templateName, "<none>");
                return st;
            }

            IDictionary<string, FormalArgument> formalArgs = new LinkedHashMap<string, FormalArgument>();
            foreach (var argument in st.impl.FormalArguments)
                formalArgs[argument.Name] = argument;

            // PASS IN OUTPUT MODEL OBJECT TO TEMPLATE AS FIRST ARG
            string modelArgName = st.impl.FormalArguments[0].Name;
            st.Add(modelArgName, omo);

            // COMPUTE STs FOR EACH NESTED MODEL OBJECT MARKED WITH @ModelElement AND MAKE ST ATTRIBUTE
            ISet<string> usedFieldNames = new HashSet<string>();
            IEnumerable<FieldInfo> fields = GetFields(cl);
            foreach (FieldInfo fi in fields)
            {
                ModelElementAttribute annotation = fi.GetCustomAttribute<ModelElementAttribute>();
                if (annotation == null)
                {
                    continue;
                }

                string fieldName = fi.Name;

                if (!usedFieldNames.Add(fieldName))
                {
                    tool.errMgr.ToolError(ErrorType.INTERNAL_ERROR, "Model object " + omo.GetType().Name + " has multiple fields named '" + fieldName + "'");
                    continue;
                }

                // Just don't set [ModelElement] fields w/o formal argument in target ST
                if (!formalArgs.ContainsKey(fieldName))
                    continue;

                try
                {
                    object o = fi.GetValue(omo);
                    if (o is OutputModelObject)
                    {
                        // SINGLE MODEL OBJECT?
                        OutputModelObject nestedOmo = (OutputModelObject)o;
                        Template nestedST = Walk(nestedOmo);
                        //System.Console.WriteLine("set ModelElement " + fieldName + "=" + nestedST + " in " + templateName);
                        st.Add(fieldName, nestedST);
                    }
                    else if (o is IDictionary)
                    {
                        IDictionary nestedOmoMap = (IDictionary)o;
                        IDictionary<object, Template> m = new LinkedHashMap<object, Template>();
                        for (IDictionaryEnumerator enumerator = nestedOmoMap.GetEnumerator(); enumerator.MoveNext(); )
                        {
                            DictionaryEntry entry = enumerator.Entry;
                            Template nestedST = Walk((OutputModelObject)entry.Value);
                            //System.Console.WriteLine("set ModelElement " + fieldName + "=" + nestedST + " in " + templateName);
                            m[entry.Key] = nestedST;
                        }

                        st.Add(fieldName, m);
                    }
                    else if (o is IEnumerable && !(o is string))
                    {
                        // LIST OF MODEL OBJECTS?
                        IEnumerable nestedOmos = (IEnumerable)o;
                        foreach (object nestedOmo in nestedOmos)
                        {
                            if (nestedOmo == null)
                                continue;
                            Template nestedST = Walk((OutputModelObject)nestedOmo);
                            //System.Console.WriteLine("set ModelElement " + fieldName + "=" + nestedST + " in " + templateName);
                            st.Add(fieldName, nestedST);
                        }
                    }
                    else if (o != null)
                    {
                        tool.errMgr.ToolError(ErrorType.INTERNAL_ERROR, "not recognized nested model element: " + fieldName);
                    }
                }
                catch (FieldAccessException)
                {
                    tool.errMgr.ToolError(ErrorType.CODE_TEMPLATE_ARG_ISSUE, templateName, fieldName);
                }
            }

            //st.impl.Dump();
            return st;
        }

        private static IEnumerable<FieldInfo> GetFields(Type type)
        {
            var declaredFields = type.GetTypeInfo().DeclaredFields;
            if (type.GetTypeInfo().BaseType != null)
                declaredFields = declaredFields.Concat(GetFields(type.GetTypeInfo().BaseType));

            return declaredFields;
        }
    }
}
