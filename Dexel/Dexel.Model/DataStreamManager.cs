using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dexel.Contracts.Model;

namespace Dexel.Model
{
    public class DataStreamManager : IDataStreamManager
    {
        public IDataStream CreateNew(string datanames, string actionsName = "")
        {
            var dataStream = new DataStream();
            dataStream.ID = Guid.NewGuid();
            dataStream.DataNames = datanames;
            dataStream.ActionName = actionsName;
            return dataStream;
        }


        public IDataStreamDefinition CreateNewDefinition(ISoftwareCell parent, string datanames, string actionsName = "",
            bool connected = false)
        {
            var dataStream = new DataStreamDefinition();
            dataStream.ID = Guid.NewGuid();
            dataStream.DataNames = datanames;
            dataStream.Parent = parent;
            dataStream.ActionName = actionsName;
            dataStream.Connected = connected;
            return dataStream;
        }


        public IDataStream GetFirst(Guid id, IMainModel mainModel)
        {
            return mainModel.Connections.First(x => x.ID.Equals(id));
        }


        public IDataStream CreateNew(IDataStreamDefinition datastreamDefintion)
        {
            return CreateNew(datastreamDefintion.DataNames, datastreamDefintion.ActionName);
        }


        public IDataStreamDefinition CreateNewDefinition(ISoftwareCell parent, IDataStreamDefinition defintion,
            bool connected = false)
        {
            return CreateNewDefinition(parent, defintion.DataNames, defintion.ActionName, connected: connected);
        }


        public IDataStreamDefinition FindExistingDefinition(IDataStreamDefinition defintion,
            IEnumerable<IDataStreamDefinition> definitions, Action<IDataStreamDefinition> onFound,
            Action onNotFound = null)
        {
            var found = definitions.Where(x => x.IsEquals(defintion));
            if (found.Any())
            {
                onFound(found.First());
                return found.First();
            }
            onNotFound?.Invoke();
            return null;
        }


        public void DeConnect(IEnumerable<IDataStreamDefinition> definitions, IDataStream dataStream)
        {
            definitions.Where(def => def.IsEquals(dataStream)).ToList()
                .ForEach(def => { def.Connected = false; });
        }


        public Guid CheckForStreamWithSameName(ISoftwareCell source, ISoftwareCell destination,
            IDataStream tempStream, IMainModel mainModel,
            Action<IDataStreamDefinition> onFound, Action onNotFound)
        {
            var found = source.OutputStreams.Where(x => x.DataNames.Equals(tempStream.DataNames)).ToList();
            if (found.Any())
            {
                onFound(found.First());
                return found.First().ID;
            }

            onNotFound();
            return tempStream.ID;
        }


        public static string MergeDataNames(IDataStreamDefinition sourceDSD, IDataStreamDefinition destinationDSD)
        {
            if (destinationDSD == null)
                return sourceDSD.DataNames + " | ";
            return sourceDSD.DataNames + " | " + destinationDSD.DataNames;
        }


        public void ChangeDatanames(IDataStream datastream, string newDatanames)
        {
            // update datanames of connection itself
            datastream.DataNames = newDatanames;
            
            // update datanames of DSDs
            var splitted = SolvePipeLogic(datastream);
            datastream.Sources.ForEach(dsd => dsd.DataNames = splitted[0].Trim());
            datastream.Destinations.ForEach(dsd => dsd.DataNames = splitted[1].Trim());
        }




        public string[] SolvePipeLogic(IDataStream datastream)
        {
            var strings = SolveWithParenthesis(datastream.DataNames);
            strings = SolveNoParenthesis(datastream.DataNames, strings);
            strings = SolveNoPipe(datastream.DataNames, strings);

            return strings;
        }


        private string[] SolveNoPipe(string datanames, string[] strings)
        {
            return strings ?? new[] {datanames, datanames};
        }


        private static string[] SolveNoParenthesis(string datanames, string[] strings)
        {
            if (strings != null)
                return strings;

            var withoutparenthesis =
                new Regex(
                    @"^\s*(?'output'.*)(?'pipe'\|)\s*(?'dots'\.{3})?(?'input'.*)");
            var matches = withoutparenthesis.Matches(datanames);
            if (matches.Count == 1)
            {
                string input = "", output = "";
                var grps = matches[0].Groups;

                output = datanames.Split('|')[0];
                input = datanames.Split('|')[1];


                if (!grps["dots"].Success)
                    return new[] { output, input };

                input = grps["output"].Value.Trim() + ", " + grps["input"].Value.Trim();
                return new[] { output, input };

            }
            return null;
        }


        private string[] SolveWithParenthesis(string datanames)
        {
            var withparenthesis =
                new Regex(
                    @"^\s*(?'open1'\()(?'output'.*)(?'close1'\)\*?)\s*(?'pipe'\|)\s*(?'open2'\()\s*(?'dots'\.{3})?(?'input'.*)(?'close2'\)\*?)\s*");
            var matches = withparenthesis.Matches(datanames);
            if (matches.Count == 1)
            {
                string input = "", output = "";
                var grps = matches[0].Groups;

                output = datanames.Split('|')[0];
                input = datanames.Split('|')[1];


                if (!grps["dots"].Success)
                    return new[] {output, input};

                input = grps["open2"].Value + grps["output"].Value.Trim() + ", " + grps["input"].Value.Trim() +
                        grps["close2"].Value;

                return new[] {output, input};
            }
            return null;
        }
    }
}