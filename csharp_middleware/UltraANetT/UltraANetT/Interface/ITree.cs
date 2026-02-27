using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraTreeList.Nodes;

namespace UltraANetT.Interface
{
    public interface ITree
    {
        bool SaveTreeList(string xmlPath);
        bool LoadTreeList(string vehXmlPath);
        void DrawTreeColor();
        void ShowCmsByNode(TreeListNode node);
        void StartEdit();
        void BingEvent();

        void DrawTreeImage();

        List<string> GetCurrentNode(TreeListNode node);
       
        void Create_Click(object sender, EventArgs e);
        void ReName_Click(object sender, EventArgs e);
        void Del_Click(object sender, EventArgs e);
    }
}
