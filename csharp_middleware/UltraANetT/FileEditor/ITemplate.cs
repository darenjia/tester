using System.Collections.Generic;
using DevExpress.Spreadsheet;

namespace FileEditor
{
    public interface ITemplate
    {
        void DrawNav();
        void InitDict();
        void DrawTem(string temName);
        void SetAttribute(List<string> nodeList );
    }
}
