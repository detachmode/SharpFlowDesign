using System;
using System.Collections.Generic;
using System.Linq;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Roslyn
{
    public static class DataTypeParser
    {


        public static SyntaxNode ConvertToType(SyntaxGenerator generator, string type)
        {
            if (type.ToLower() == "datetime")   // bug in roslyn?        
                return generator.IdentifierName("DateTime");

            var res = generator.IdentifierName(type);
            
            Convert(type, specialType => res = generator.TypeExpression(specialType));
            return res;

        }

        private static void Convert(string type, Action<SpecialType> onConverted)
        {
            switch (type)
            {
                case "bool":
                    onConverted(SpecialType.System_Boolean);
                    return;
                case "int":
                    onConverted(SpecialType.System_Int32);
                    return;

            }

            SpecialType outType;
            var success = Enum.TryParse($"system_{type}", true, out outType);
            if (success)
                onConverted(outType);
        }

        public static bool IsSystemType(string type)
        {
            bool success = false;
            Convert(type, specialType => success = true);
            return success;
        }


        public static SyntaxNode ConvertNameTypeToTypeExpression(SyntaxGenerator generator, NameType nametype)
        {
            var singletype = ConvertToType(generator, nametype.Type);
            //if (nametype.IsInsideStream)
            //{
            //    if (nametype.IsList == false && nametype.IsArray == false)
            //        return generator.GenericName("IEnumerable", singletype);
            //    if (nametype.IsList == true && nametype.IsArray == false)
            //        return generator.GenericName("IEnumerable", generator.GenericName("List", singletype));
            //    if (nametype.IsList == false && nametype.IsArray == true)
            //        return generator.GenericName("IEnumerable", generator.ArrayTypeExpression(singletype));
            //}

            if (nametype.IsArray)
                return generator.ArrayTypeExpression(singletype);
            if (nametype.IsList && nametype.IsArray == false)
                return generator.GenericName("List", singletype);

            return singletype;

        }


        public static SyntaxNode ConvertToType(SyntaxGenerator generator, IEnumerable<NameType> nametypes)
        {
            var alltypes = nametypes.ToList();
            if (alltypes.Count == 0)
            {
                return null;
            }
            if (alltypes.Any(nt => nt.IsInsideStream))
            {
                if (alltypes.Count > 1)
                {
                    return
                            generator.GenericName("Tupel",
                                generator.IdentifierName(
                                    alltypes.Select(nt => ConvertNameTypeToTypeExpression(generator, nt).ToFullString())
                                        .Aggregate((f, s) => f + "," + s)));

                }
                return ConvertNameTypeToTypeExpression(generator, alltypes.First());

            }
            if (alltypes.Count > 1)
            {
                generator.GenericName("Tupel",
                    generator.IdentifierName(
                        alltypes.Select(nt => ConvertNameTypeToTypeExpression(generator, nt).ToFullString())
                            .Aggregate((f, s) => f + "," + s)));
            }
            else
            {
                return ConvertNameTypeToTypeExpression(generator, alltypes.First());
            }

            return null;
        }


        public static void AnalyseOutputs(FunctionUnit functionUnit, Action isComplexOutput = null, Action isSimpleOutput = null)
        {

            DataStreamParser.IsStream(functionUnit.OutputStreams.First().DataNames,
                isStream: () => isComplexOutput?.Invoke(),
                isNotStream: () =>
                {
                    if (functionUnit.OutputStreams.Count > 1)
                        isComplexOutput?.Invoke();
                    else
                        isSimpleOutput?.Invoke();
                }
            );
        }


        public static void OutputOrInputIsStream(FunctionUnit functionUnit, Action bothAreStreams = null, Action onInputIsStream = null, Action onOutputIsStream = null, Action noStream = null )
        {
            var outputIsStream = false;
            var inputIsStream = false;
            DataStreamParser.IsStream(functionUnit.OutputStreams.First().DataNames, () => outputIsStream = true );
            DataStreamParser.IsStream(functionUnit.InputStreams.First().DataNames, () => inputIsStream = true);

            if (outputIsStream && inputIsStream)
                bothAreStreams?.Invoke();
            else if (inputIsStream)
                onInputIsStream?.Invoke();
            else if (outputIsStream)
                onOutputIsStream?.Invoke();
            else
                noStream?.Invoke();
        }
    }
}