using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using PostSharp.Laos;

namespace Aspects
{
    [Serializable]
    public class WebTrace : OnMethodBoundaryAspect
    {
        public override void OnEntry(MethodExecutionEventArgs eventArgs)
        {
            base.OnEntry(eventArgs);
            HttpContext.Current.Trace.Write("Entering " + eventArgs.Method.Name);
        }

        public override void OnExit(MethodExecutionEventArgs eventArgs)
        {
            base.OnExit(eventArgs);
            HttpContext.Current.Trace.Write("Leaving " + eventArgs.Method.Name);
        }
    }
}