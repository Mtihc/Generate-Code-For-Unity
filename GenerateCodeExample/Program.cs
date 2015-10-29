using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateCodeExample
{
    class Program
    {
        static void Main(string[] args)
        {

            string ns = "Example.Namespace";
            string typeName = "ExampleEnumType";
            string[] fields = new string[] { "Option1", "Option2", "Option3" };
            bool withFlagsAttribute = true;
            string zerothField = "None";
            string allField = "All";

            // writing to string writer
            using (var writer = new System.IO.StringWriter())
            {
                // write enum code!
                GenerateCode.EnumGenerator.WriteEnum(writer, ns, typeName, fields, withFlagsAttribute, zerothField, allField);

                // output to console
                Console.WriteLine(writer.ToString());
                Console.ReadKey();
            }
        }
    }
}
