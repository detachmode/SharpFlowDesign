﻿using System;
using System.Collections.Generic;
using Dexel.Editor.CustomControls;
using Dexel.Editor.DragAndDrop;
using Dexel.Model.DataTypes;
using PropertyChanged;

namespace Dexel.Editor.ViewModels.DrawingBoard
{
    [ImplementPropertyChanged]
    public class DangelingConnectionViewModel : IDragable, IDropable
    {
        public DangelingConnectionViewModel()
        {
            DataNames = "param";
        }

        public Guid ID;
        public DataStreamDefinition Model { get; set; }
        public SoftwareCell Parent { get; set; }
        public string DataNames { get; set; }
        public string Actionname { get; set; }
        public double Width { get; set; }

        Type IDragable.DataType => typeof (DangelingConnectionViewModel);


        public void LoadFromModel(SoftwareCell parent, DataStreamDefinition dataStream)
        {
            ID = dataStream.ID;
            Model = dataStream;
            Parent = parent;
            DataNames = dataStream.DataNames;
            Actionname = dataStream.ActionName;
        }
        

        public List<Type> AllowedDropTypes => new List<Type> { typeof(DangelingConnectionViewModel)};
       


        public void Drop(object data)
        {
            data.TryCast<DangelingConnectionViewModel>(
                dangConnVm => Interactions.DragDroppedTwoDangelingConnections(dangConnVm.Model, this.Model, MainViewModel.Instance().Model));
        }
    }
}