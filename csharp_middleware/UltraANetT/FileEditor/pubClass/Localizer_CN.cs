using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraNavBar;
using DevExpress.XtraSpreadsheet.Localization;

namespace FileEditor
{
    // ReSharper disable once InconsistentNaming
    public class Localizer_CN
    {
        public Localizer_CN()
        {
             XtraSpreadsheetLocalizer.Active = new XtraSpreadSheet_CN();
        }
    }

    // ReSharper disable once InconsistentNaming
    public class XtraSpreadSheet_CN : XtraSpreadsheetLocalizer
    {
        public XtraSpreadSheet_CN()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }
        public override string Language => "简体中文";

        public override string GetLocalizedString(XtraSpreadsheetStringId id)
        {
            switch (id)
            {
                case XtraSpreadsheetStringId.Caption_DataValidationList: return "序列";
            }
            return base.GetLocalizedString(id);
        }
    }
}
