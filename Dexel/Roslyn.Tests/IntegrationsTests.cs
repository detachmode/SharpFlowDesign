﻿using System.Linq;
using System.Text.RegularExpressions;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Roslyn.Tests
{

    [TestClass]
    public class IntegrationsTests
    {
        private readonly MyGenerator _mygen = new MyGenerator();


        [TestMethod]
        public void FindParametersTest()
        {
            // unnamed syntax test
            var testModel = new MainModel();
            var newName = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            MainModelManager.AddNewInput(newName, "");

            var alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            MainModelManager.ConnectTwoCells(newName, alter, "string", "", testModel);

            var person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            MainModelManager.ConnectTwoCells(alter, person, "int", "int, string", testModel);
            MainModelManager.AddNewOutput(person, "Person");

            var dependecies = Integrations.FindParameters(person, testModel.Connections, newName);
            Assert.IsTrue(dependecies.Any(x => x.Source == alter));
            Assert.IsTrue(dependecies.Any(x => x.Source == newName));

            // ...  syntax test
            testModel = new MainModel();
            newName = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            MainModelManager.AddNewInput(newName, "");

            alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            MainModelManager.ConnectTwoCells(newName, alter, "string", "", testModel);

            person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            MainModelManager.ConnectTwoCells(alter, person, "int", "int, string", testModel);
            MainModelManager.AddNewOutput(person, "Person");

            dependecies = Integrations.FindParameters(person, testModel.Connections, newName);
            Assert.IsTrue(dependecies.Any(x => x.Source == alter));
            Assert.IsTrue(dependecies.Any(x => x.Source == newName));
        }


        [TestMethod]
        public void CreateIntegrationBodyTest()
        {
            InnerStreamOnly();
            StreamOutput();
        }


        [TestMethod]
        public void InnerStreamOnly()
        {
            var testModel = new MainModel();
            var x = MainModelManager.AddNewSoftwareCell("X", testModel);
            MainModelManager.AddNewInput(x, "()");
            MainModelManager.AddNewOutput(x, "(int)");

            var createPersons = MainModelManager.AddNewSoftwareCell("Create Persons", testModel);
            MainModelManager.AddNewInput(createPersons, "()");
            MainModelManager.AddNewOutput(createPersons, "(Person)*");

            var addAge = MainModelManager.AddNewSoftwareCell("Add Age", testModel);
            MainModelManager.AddNewInput(addAge, "(Person)*");
            MainModelManager.AddNewOutput(addAge, "(int)");


            MainModelManager.ConnectTwoDefintions(createPersons.OutputStreams.First(),
                addAge.InputStreams.First(), testModel);

            x.Integration.Add(createPersons);
            x.Integration.Add(addAge);


            var res = Integrations.CreateIntegrationBody(_mygen.Generator, testModel.Connections, x);
            var formatted = _mygen.CompileToString(res.ToList());

            Assert.IsTrue(
                Regex.IsMatch(
                    formatted,
                    @".*CreatePersons.*AddAge.*", RegexOptions.Singleline));

            Assert.IsTrue(
                Regex.IsMatch(
                    formatted,
                    @".*CreatePersons\(x =>.*\S* aPerson = AddAge\(x\);.*onPerson\(aPerson\);.*",
                    RegexOptions.Singleline));
        }

        [TestMethod]
        public void StreamOutput()
        {
            var testModel = new MainModel();
            var x = MainModelManager.AddNewSoftwareCell("X", testModel);
            MainModelManager.AddNewInput(x, "()");
            MainModelManager.AddNewOutput(x, "(Person)*");

            var createPersons = MainModelManager.AddNewSoftwareCell("Create Persons", testModel);
            MainModelManager.AddNewInput(createPersons, "()");
            MainModelManager.AddNewOutput(createPersons, "(Person)*");

            var addAge = MainModelManager.AddNewSoftwareCell("Add Age", testModel);
            MainModelManager.AddNewInput(addAge, "(Person)*");
            MainModelManager.AddNewOutput(addAge, "(Person)*");


            MainModelManager.ConnectTwoDefintions(createPersons.OutputStreams.First(),
                addAge.InputStreams.First(), testModel);

            x.Integration.Add(createPersons);
            x.Integration.Add(addAge);


            var res = Integrations.CreateIntegrationBody(_mygen.Generator, testModel.Connections, x);
            var formatted = _mygen.CompileToString(res.ToList());

            Assert.IsTrue(
                Regex.IsMatch(
                    formatted,
                    @".*CreatePersons.*AddAge.*", RegexOptions.Singleline));

            Assert.IsTrue(
                Regex.IsMatch(
                    formatted,
                    @".*CreatePersons\(x =>.*\S* aPerson = AddAge\(x\);.*onPerson\(aPerson\);.*",
                    RegexOptions.Singleline));
        }
    }

}