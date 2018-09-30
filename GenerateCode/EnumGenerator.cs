using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CSharp;

namespace GenerateCode
{
    public static class EnumGenerator
    {

        public static void WriteEnum(TextWriter writer, string ns, string typeName, string[] fields, bool withFlagsAttribute = false, string zerothField = null, string allField = null)
        {

            var compileUnit = new CodeCompileUnit();


            // create namespace.
            var codeNamespace = new CodeNamespace(ns);
            compileUnit.Namespaces.Add(codeNamespace);
            
            // create enum type declaration
            var codeType = new CodeTypeDeclaration(typeName);
            codeNamespace.Types.Add(codeType);
            // is enum
            codeType.IsEnum = true;
            // optionally add [Flags] attribute
            if (withFlagsAttribute)
            {
                // using System;
                //codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
                // [Flags] attribute
                codeType.CustomAttributes.Add(new CodeAttributeDeclaration("System.Flags"));
            }

            var index = 0;
            // create the None field
            if (withFlagsAttribute && !string.IsNullOrEmpty(zerothField))
            {
                var codeField = new CodeMemberField(codeType.Name, zerothField);
                codeField.InitExpression = new CodePrimitiveExpression(0);
                codeType.Members.Add(codeField);
            }

            // creating enum fields
            foreach (var valueName in fields)
            {
                // create enum fields
                var codeField = new CodeMemberField(codeType.Name, valueName);

                if (withFlagsAttribute)
                {
                    // flags have values that are a power of 2 (each its own bit 1, 2, 4, 8, 16), 
                    codeField.InitExpression = new CodePrimitiveExpression(1 << index++);// index is the index of the bit
                }
                else
                {
                    // just the default zero-based index (0, 1, 2, 4, 5)
                    codeField.InitExpression = new CodePrimitiveExpression(index++);
                }
                // add field to type
                codeType.Members.Add(codeField);
            }
            // create the All field. This is the total of all of the above combined
            if (withFlagsAttribute && !string.IsNullOrEmpty(allField))
            {
                var codeField = new CodeMemberField(codeType.Name, allField);
                codeField.InitExpression = new CodePrimitiveExpression((1 << index)-1);
                codeType.Members.Add(codeField);
            }

            // write the code
            var provider = new CSharpCodeProvider();
            var options = new CodeGeneratorOptions();
            options.BlankLinesBetweenMembers = false;

            provider.GenerateCodeFromCompileUnit(compileUnit, writer, options);

        }


    }
}
