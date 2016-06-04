using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Futura.ArcGIS.NetworkExtension
{
    public class Structure : InsertionTool
    {
        public Structure()
        {
            base.Associated_FeatureClassName = "Structure";
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
        }

        protected override void OnMouseMove(ESRI.ArcGIS.Desktop.AddIns.Tool.MouseEventArgs arg)
        {
            base.OnMouseMove(arg);
        }

        protected override void OnMouseUp(ESRI.ArcGIS.Desktop.AddIns.Tool.MouseEventArgs arg)
        {
            base.OnMouseUp(arg);
        }

        protected override void OnRefresh(int hDC)
        {
            base.OnRefresh(hDC);
        }

        protected override void OnMouseDown(ESRI.ArcGIS.Desktop.AddIns.Tool.MouseEventArgs arg)
        {
            base.OnMouseDown(arg);
        }

        protected override bool OnContextMenu(int x, int y)
        {
            return base.OnContextMenu(x, y);
        }

        protected override bool OnDeactivate()
        {
            return base.OnDeactivate();
        }

        protected override void OnDoubleClick()
        {
            base.OnDoubleClick();
        }
    }

}
