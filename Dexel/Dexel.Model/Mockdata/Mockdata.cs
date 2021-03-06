﻿using System.Collections.Generic;
using System.Windows;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;

namespace Dexel.Model.Mockdata
{
    public static class Mockdata
    {

        public static MainModel MakeRandomPerson2()
        {

            var testModel = new MainModel();
            var first = MainModelManager.AddNewFunctionUnit("Random Name", testModel);
            first.Position = new Point(20, 50);
            MainModelManager.AddNewInput(first, "");

            var firstOp = MainModelManager.AddNewFunctionUnit("Operation", testModel);
            firstOp.Position = new Point(-60, 180);
            MainModelManager.AddNewOutput(firstOp, "");
            MainModelManager.AddNewInput(firstOp, "");
            first.IsIntegrating.Add(firstOp);
            var firstOp2 = MainModelManager.AddNewFunctionUnit("Operation", testModel);
            
            firstOp2.Position = new Point(160, 180);
            MainModelManager.AddNewOutput(firstOp2, "");
            MainModelManager.AddNewInput(firstOp2, "");
            first.IsIntegrating.Add(firstOp2);

            var alter = MainModelManager.AddNewFunctionUnit("Random Age", testModel);
            alter.Position = new Point(280, 50);
            MainModelManager.ConnectTwoFunctionUnits(first, alter, "name:string", "", testModel);

            var person = MainModelManager.AddNewFunctionUnit("Create Person", testModel);
            person.Position = new Point(540, 50);
            MainModelManager.ConnectTwoFunctionUnits(alter, person, "age:int ","age:int, name:string", testModel);
            var definition = DataStreamManager.NewDefinition(person, "rndPerson:Person");
            person.OutputStreams.Add(definition);


            return testModel;

        }

        public static MainModel StartMainModel()
        {

            var testModel = new MainModel();
            var first = MainModelManager.AddNewFunctionUnit("foo", testModel);
            first.Position = new Point(80, 50);
            MainModelManager.AddNewInput(first, "()");

            var firstOp = MainModelManager.AddNewFunctionUnit("subbar", testModel);
            firstOp.Position = new Point(80, 180);
            MainModelManager.AddNewInput(firstOp, "()");
            MainModelManager.AddNewOutput(firstOp, "(age:int)");
            first.IsIntegrating.Add(firstOp);

            var inttype = new SubDataType { Name = "age", Type = "int" };
            var nametype = new SubDataType { Name = "name", Type = "string" };
            var sublist = new List<SubDataType> { inttype, nametype };
            testModel.DataTypes.Add(new CustomDataType() { Name = "Person", SubDataTypes = sublist });

            var person = MainModelManager.AddNewFunctionUnit("create person", testModel);
            person.Position = new Point(450, 50);
            MainModelManager.ConnectTwoFunctionUnits(first, person, "(age:int)", "(age:int, name:string)", testModel);
            MainModelManager.AddNewOutput(person, "(rndPerson:Person)");



            return testModel;

        }
    }
}
