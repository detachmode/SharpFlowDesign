﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dexel.Editor.CustomControls;
using Dexel.Editor.DragAndDrop;
using Dexel.Editor.ViewModels.DataTypeEditor;
using Dexel.Editor.ViewModels.DrawingBoard;
using Dexel.Library;
using Dexel.Model;
using Dexel.Model.DataTypes;
using PropertyChanged;

namespace Dexel.Editor.ViewModels
{

    [ImplementPropertyChanged]
    public class MainViewModel : IDropable
    {
        private static MainViewModel _self;


        public MainViewModel()
        {
            SoftwareCells = new ObservableCollection<IOCellViewModel>();
            SelectedSoftwareCells = new ObservableCollection<IOCellViewModel>();
            Connections = new ObservableCollection<ConnectionViewModel>();
            IntegrationBorders = new ObservableCollection<IOCellViewModel>();
            DataTypes = new ObservableCollection<DataTypeViewModel>();
            VisibileDataTypes = new ObservableCollection<DataTypeViewModel>();
            FontSizeCell = 12;
            VisibilityDatanames = Visibility.Visible;
            VisibilityBlockTextBox = Visibility.Hidden;
            SelectedSoftwareCells.CollectionChanged += (sender, args) => UpdateSelectionState();

        }

        public ObservableCollection<DataTypeViewModel> DataTypes { get; set; }
        public ObservableCollection<IOCellViewModel> IntegrationBorders { get; set; }
        public ObservableCollection<ConnectionViewModel> Connections { get; set; }
        public ObservableCollection<IOCellViewModel> SoftwareCells { get; set; }
        public ObservableCollection<IOCellViewModel> SelectedSoftwareCells { get; set; }
        public ObservableCollection<DataTypeViewModel> VisibileDataTypes { get; set; }
        public ConnectionViewModel TemporaryConnection { get; set; }
        public MainModel Model { get; set; }
        public int FontSizeCell { get; set; }
        public Visibility VisibilityDatanames { get; set; }
        public Visibility VisibilityBlockTextBox { get; set; }
        public int MissingDataTypes { get; set; }

        public static MainViewModel Instance() => _self ?? (_self = new MainViewModel());




        #region Modify Selection

        private void UpdateSelectionState()
        {
            SoftwareCells.ForEach(x => x.IsSelected = false);
            SelectedSoftwareCells.ForEach(x => x.IsSelected = true);
        }

        public void SetSelection(IOCellViewModel ioCellViewModel)
        {
            if (SelectedSoftwareCells.Contains(ioCellViewModel)) return;

            SelectedSoftwareCells.Clear();
            SelectedSoftwareCells.Add(ioCellViewModel);
        }


        public void SetSelectionCTRLMod(IOCellViewModel ioCellViewModel)
        {
            if (SelectedSoftwareCells.Contains(ioCellViewModel))
                SelectedSoftwareCells.Remove(ioCellViewModel);
            else
                SelectedSoftwareCells.Add(ioCellViewModel);
        }


        public void MoveSelectedIOCells(Vector dragDelta)
        {
            SelectedSoftwareCells.ForEach(sc => Interactions.MoveSoftwareCell(sc.Model, dragDelta.X, dragDelta.Y));
        }


        public void DuplicateSelectionAndSelectNew()
        {
            var duplicted = DuplicateSelection();
            Select(duplicted);
        }


        private void Select(List<SoftwareCell> duplicted)
        {
            SelectedSoftwareCells.Clear();
            SoftwareCells.Where(sc => duplicted.Contains(sc.Model)).ForEach(vm => SelectedSoftwareCells.Add(vm));
        }

        public SoftwareCell DuplicateIncludingChildrenAndIntegrated(SoftwareCell softwareCell)
        {
            var list = MainModelManager.GetChildrenAndIntegrated(softwareCell, new List<SoftwareCell>(), Model);
            var copiedlist = MainModelManager.Duplicate(list, Model);
            var first = copiedlist.First(x => x.OriginGuid == softwareCell.ID);
            return first.NewCell;
        }


        private List<SoftwareCell> DuplicateSelection()
        {
            var copiedList = MainModelManager.Duplicate(SelectedSoftwareCells.Select(vm => vm.Model).ToList(), Model);

            Reload();
            return copiedList.Select(x => x.NewCell).ToList();
        }


        public void ClearSelection()
        {
            SelectedSoftwareCells.Clear();
        }


        public void AddToSelection(IOCellViewModel ioCellViewModel)
        {
            SelectedSoftwareCells.Add(ioCellViewModel);
        }

        #endregion

        #region Drop

        public List<Type> AllowedDropTypes => new List<Type>
        {
            typeof(ConnectionViewModel)
        };

       


        public void Drop(object data)
        {
            data.TryCast<ConnectionViewModel>(
                connectionVM => Interactions.DeConnect(connectionVM.Model, Model));
        }

        #endregion

        #region UpdateSelectionState Positions

        public void UpdateConnectionsPosition(Point inputPoint, Point outputPoint, IOCellViewModel ioCellViewModel)
        {
            var allOutputs = Connections.Where(conn => conn.Model.Sources.Any(x => x.Parent == ioCellViewModel.Model));
            var allInputs =
                Connections.Where(conn => conn.Model.Destinations.Any(x => x.Parent == ioCellViewModel.Model));

            allInputs.ToList().ForEach(x => x.End = inputPoint);
            allOutputs.ToList().ForEach(x => x.Start = outputPoint);
        }


        public void UpdateIntegrationBorderPositions()
        {
            IntegrationBorders.ForEach(iocellvm =>
            {
                if (iocellvm.Integration.Count == 0)
                {
                    return;
                }
                var tempIntegrations =
                    iocellvm.Integration.OrderBy(cellvm1 => cellvm1.Model.Position.X + cellvm1.CellWidth);
                var min = tempIntegrations.First();
                var max = tempIntegrations.Last();

                tempIntegrations = iocellvm.Integration.OrderBy(cellvm1 => cellvm1.Model.Position.Y + cellvm1.CellHeight);
                var miny = tempIntegrations.First();
                iocellvm.IntegrationStartPosition = new Point(min.Model.Position.X - 60, miny.Model.Position.Y);
                iocellvm.IntegrationEndPosition = new Point(max.Model.Position.X + max.CellWidth + 60,
                    miny.Model.Position.Y);
            });
        }

        #endregion

        #region Load Model

        public void Reload()
        {
            if (Model != null)
            {
                LoadFromModel(Model);
            }
        }


        public void LoadFromModel(MainModel mainModel)
        {
            Model = mainModel;
            LoadConnection(mainModel.Connections);
            LoadSoftwareCells(mainModel.SoftwareCells);
            LoadIntegrations();
            LoadDataTypes(mainModel.DataTypes);
        }


        private void LoadDataTypes(List<DataType> dataTypes)
        {
            DataTypes.Clear();
            dataTypes.ForEach(dataType =>
            {
                var vm = new DataTypeViewModel();
                vm.Model = dataType;

                if (dataType.DataTypes == null || !dataType.DataTypes.Any())
                    vm.Definitions = "";
                else 
                    vm.Definitions = dataType.DataTypes
                        .Select(x => string.IsNullOrEmpty(x.Name) ? x.Type : $"{x.Name}:{x.Type}")
                        .Aggregate((str, type) => str + "\n" + type);

                DataTypes.Add(vm);

            });
        }


        private void LoadIntegrations()
        {
            IntegrationBorders.Clear();
            SoftwareCells.Where(x => x.Model.Integration.Count != 0).ToList().ForEach(hasIntegration =>
            {
                var integratedVMs =
                    SoftwareCells.Where(otherVM => hasIntegration.Model.Integration.Contains(otherVM.Model));
                integratedVMs.ToList().ForEach(hasIntegration.Integration.Add);
                IntegrationBorders.Add(hasIntegration);
            });
            UpdateIntegrationBorderPositions();
        }


        private void LoadSoftwareCells(List<SoftwareCell> softwareCells)
        {
            SoftwareCells.Clear();
            softwareCells.ForEach(modelSoftwareCell =>
            {
                var vm = new IOCellViewModel();
                vm.LoadFromModel(modelSoftwareCell);
                SoftwareCells.Add(vm);
            });
        }


        private void LoadConnection(List<DataStream> dataStreams)
        {
            Connections.Clear();
            dataStreams.Where(x => x.Sources.Any() && x.Destinations.Any()).ToList().ForEach(modelConnection =>
            {
                var vm = new ConnectionViewModel();
                vm.LoadFromModel(modelConnection);
                Connections.Add(vm);
            });
        }

        #endregion

        public void SetDataTypeFilter(string datanames)
        {
            if (datanames == null)
            {
                VisibileDataTypes.Clear();
                DataTypes.ForEach(vm => VisibileDataTypes.Add(vm));
                return;
            }

            var types = Interactions.GetFocusedDataTypes(datanames, Model);

            VisibileDataTypes.Clear();
            DataTypes.Where( x => types.Any(y => x.Model.Name == y)).ForEach(vm => VisibileDataTypes.Add(vm));
        }
    }
}