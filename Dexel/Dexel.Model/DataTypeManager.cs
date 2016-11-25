﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexel.Model.DataTypes;

namespace Dexel.Model
{
    public static class DataTypeManager
    {
        public static List<string> GetUndefinedTypenames(MainModel mainmodel)
        {
            var list = CollectAllTypesFromDsds(mainmodel);
            var alltypes = list.Concat(CollectAllSubtypes(mainmodel)).ToList();
            alltypes = FilterOut(alltypes, mainmodel);
            return alltypes;
        }

        public static IEnumerable<string> CollectAllTypesFromDsds(MainModel mainmodel)
        {
            var alldsds = mainmodel.SoftwareCells.SelectMany(sc => sc.InputStreams.Concat(sc.OutputStreams).ToList());
            return alldsds.SelectMany(dsd =>
                DataStreamParser.GetInputPart(dsd.DataNames)
                    .Concat(DataStreamParser.GetOutputPart(dsd.DataNames))).Select(x => x.Type);
        }

        public static List<string> FilterOut(List<string> types, MainModel mainmodel)
        {
            types = types.Distinct().ToList();
            types.RemoveAll(IsPrimitiveType);
            types.RemoveAll(t => mainmodel.DataTypes.Any(dt => dt.Name == t));
            types.RemoveAll(string.IsNullOrEmpty);
            return types;

        }


        private static readonly List<string> Primitives = new List<string>
        {
            "int",
            "string",
            "double",
            "float"
        };

        private static bool IsPrimitiveType(string str)
        {
           return Primitives.Contains(str);
        }


        public static IEnumerable<string> CollectAllSubtypes(MainModel mainmodel)
        {
            return mainmodel.DataTypes.Where(x => x.DataTypes != null).SelectMany(dt => dt.DataTypes.Select(subDt => subDt.Type.Trim()));
        }

    }
}
