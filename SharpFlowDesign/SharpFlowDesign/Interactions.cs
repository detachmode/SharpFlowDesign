using System;
using System.Linq;
using System.Threading;
using System.Windows;
using FlowDesignModel;
using Roslyn;
using SharpFlowDesign.DebuggingHelper;
using SharpFlowDesign.ViewModels;

namespace SharpFlowDesign
{
    public static class Interactions
    {
        public enum DragMode
        {
            Single,
            Multiple
        }

        private static Timer _aTimer;


        public static void AddNewIOCell(Point pos)
        {
            var softwareCell = SoftwareCellsManager.CreateNew();
            pos.X -= 100;
            pos.Y -= 20;
            softwareCell.Position = new Point(pos.X, pos.Y);
            softwareCell.InputStreams.Add(DataStreamManager.CreateNewDefinition(softwareCell, "input"));
            softwareCell.OutputStreams.Add(DataStreamManager.CreateNewDefinition(softwareCell,"output"));

            MainModel.Get().SoftwareCells.Add(softwareCell);
            ViewRedraw();
        }


        public static void ViewRedraw()
        {
            MainViewModel.Instance().LoadFromModel(MainModel.Get());
        }


        public static void ChangeConnectionDestination(DataStream dataStream, SoftwareCell newdestination,
            MainModel mainModel)
        {
            //MainModelManager.RemoveConnection(dataStream, mainModel);
            DeConnect(dataStream, mainModel);
            MainModelManager.Connect(dataStream.Sources.First().Parent, newdestination, dataStream.DataNames, mainModel,
                dataStream.ActionName);

            ViewRedraw();
        }


        public static void AddNewInput(Guid softwareCellID, string datanames, MainModel mainModel,
            string actionName = "")
        {
            MainModelManager.AddNewInput(softwareCellID, datanames, mainModel, actionName);
        }


        public static void AddNewOutput(SoftwareCell softwareCell, string datanames)
        {
            MainModelManager.AddNewOutput(softwareCell, datanames);
            ViewRedraw();
        }

        internal static void AddNewInput(SoftwareCell softwareCell, string datanames)
        {
            MainModelManager.AddNewInput(softwareCell, datanames);
            ViewRedraw();
        }

        public static void MoveSoftwareCell(SoftwareCell model, double horizontalChange, double verticalChange)
        {
            var pos = model.Position;
            pos.X += horizontalChange;
            pos.Y += verticalChange;
            model.Position = pos;
        }


        public static void DeConnect(DataStream dataStream, MainModel mainModel)
        {
            dataStream.Sources.ForEach(streamDefinition =>
                DataStreamManager.DeConnect(streamDefinition.Parent.OutputStreams, dataStream));
            dataStream.Destinations.ForEach(streamDefinition =>
                DataStreamManager.DeConnect(streamDefinition.Parent.InputStreams, dataStream));

            MainModelManager.RemoveConnection(dataStream, mainModel);
            ViewRedraw();
        }


        public static void ConnectTwoDangelingConnections(DataStreamDefinition defintion, SoftwareCell source,
            SoftwareCell destination, MainModel mainModel)
        {

            MainModelManager.AddNewConnection(defintion, source, destination, mainModel);

            ViewRedraw();
        }


        public static void ConnectDangelingConnectionAndSoftwareCell(DataStreamDefinition defintion, SoftwareCell source,
            SoftwareCell destination, MainModel mainModel)
        {

            MainModelManager.AddNewConnection(defintion, source, destination, mainModel);
            ViewRedraw();
        }


        public static void DeleteDatastreamDefiniton(DataStreamDefinition dataStreamDefinition,
            SoftwareCell softwareCell)
        {
            softwareCell.InputStreams.RemoveAll(x => x == dataStreamDefinition);
            softwareCell.OutputStreams.RemoveAll(x => x == dataStreamDefinition);
            ViewRedraw();
        }

        public static void ConsolePrintGeneratedCode(MainModel mainModel)
        {
            var gen = new MyGenerator();
            Console.Clear();
            gen.GenerateCodeAndPrint(mainModel);
        }


        public static void DebugPrint(MainModel mainModel)
        {
            Console.Clear();
            DebugPrinter.PrintConnections(mainModel);
            DebugPrinter.PrintSoftwareCells(mainModel);
        }

        public static void AutoPrintOFF()
        {
            _aTimer.Dispose();
        }


        public static void AutoPrint(MainModel mainModel, Action<MainModel> printAction)
        {
            _aTimer?.Dispose();
            _aTimer = new Timer(state => { printAction(mainModel); }, null, 0, 200);
        }
    }
}