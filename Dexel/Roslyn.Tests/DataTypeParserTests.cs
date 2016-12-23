﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexel.Model;
using Dexel.Model.Manager;
using Microsoft.CodeAnalysis;
using Roslyn.Parser;

namespace Roslyn.Tests
{
    [TestClass()]
    public class DataTypeParserTests
    {
        private readonly MyGenerator _gen = new MyGenerator();
        [TestMethod()]
        public void ConvertNameTypeToTypeExpressionTest()
        {
            var test = DataTypeParser.ConvertNameTypeToTypeExpression(_gen.Generator, new NameType
            {
                IsList = true,
                Name = "zahlen",
                Type = "int"
            });
            var fullstring = test.ToFullString();
            Assert.AreEqual("List<int>", fullstring);


            test = DataTypeParser.ConvertNameTypeToTypeExpression(_gen.Generator, new NameType
            {
                IsArray = true,
                Name = "zahlen",
                Type = "int"
            });
            fullstring = test.ToFullString();
            Assert.AreEqual("int[]", fullstring);
        }

        [TestMethod()]
        public void ConvertToTypeExpressionTest()
        {
            Assert.AreEqual("char", DataTypeParser.ConvertToType(_gen.Generator, "char").ToFullString());
            Assert.AreEqual("string", DataTypeParser.ConvertToType(_gen.Generator, "sTring").ToFullString());
            Assert.AreEqual("double", DataTypeParser.ConvertToType(_gen.Generator, "Double").ToFullString());
            Assert.AreEqual("Point", DataTypeParser.ConvertToType(_gen.Generator, "Point").ToFullString());
            Assert.AreEqual("int", DataTypeParser.ConvertToType(_gen.Generator, "int").ToFullString());
            
           Assert.AreEqual("DateTime", DataTypeParser.ConvertToType(_gen.Generator, "datetime").ToFullString());


        }


    }
}