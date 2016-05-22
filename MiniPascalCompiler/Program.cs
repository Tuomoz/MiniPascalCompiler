using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MiniPascalCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: compiler filename [-r]\n\n-r: JIT and run the program immediately");
                return;
            }
            string filename = args[0];
            StreamReader sourceFile;
            try
            {
                sourceFile = new StreamReader(filename);
            }
            catch (Exception)
            {
                Console.WriteLine("File " + filename + " not found.");
                return;
            }

            SourceReader reader = new SourceReader(sourceFile);
            ErrorHandler errors = new ErrorHandler();
            Scanner scanner = new Scanner(reader, errors);
            Parser parser = new Parser(scanner, errors);
            ProgramNode program = parser.Parse();
            SemanticAnalyzer analyzer = new SemanticAnalyzer(program, errors);
            var symbols = analyzer.Analyze();
            //LambdaLifter lifter = new LambdaLifter(program, symbols);
            //lifter.LiftLambdas();
            if (errors.HasErrors)
            {
                Console.WriteLine("Given program contains following errors:");
                foreach (var error in errors.GetErrors())
                {
                    Console.WriteLine(error);

                }
                return;
            }
            else
            {
                AssemblyBuilder assembly;
                CodeGenerator generator = new CodeGenerator(program, symbols);
                try
                {
                    assembly = generator.Generate();
                }
                catch (Exception e)
                {
                    Console.WriteLine("The compiler encountered an unrecoverable error during compilation:\n{0}", e);
                    return;
                }                
                Console.WriteLine(string.Format("Compiler program saved to {0}.exe", program.Identifier));
                assembly.Save(program.Identifier + ".exe");
                if (args.Length > 1 && args[1] == "-r")
                {
                    Console.WriteLine("Program output:");
                    Type progType = assembly.GetType(program.Identifier);
                    MethodInfo info = progType.GetMethod("Main");
                    object o1 = Activator.CreateInstance(progType);
                    try
                    {
                        info.Invoke(null, null);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("The program encountered an unrecoverable runtime error:\n{0}", e);
                    }
                }
            }
        }
    }
}
