using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DevExpress.XtraTreeList.Nodes;

namespace UltraANetT.Interface
{
    public interface IDraw
    {
        void InitGrid();

        void Submit();
        // ReSharper disable once InconsistentNaming
        Dictionary<string, object> GetDataFromUI();

        // ReSharper disable once InconsistentNaming
        void SetDataToUI(DataRow selectedRow);

        void InitDict();

        void SwitchCtl(bool isSwitch);






    }
}
