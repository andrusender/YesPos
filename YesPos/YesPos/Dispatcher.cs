using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
namespace YesPos
{
    public delegate void AsyncAction();

    public delegate void DispatcherInvoker(Form form, AsyncAction a);

    public class Dispatcher
    {
        public static void Invoke(Form form, AsyncAction action)
        {
            if (!form.InvokeRequired)
            {
                action();
            }
            else
            {
                form.Invoke((DispatcherInvoker)Invoke, form, action);
            }
        }
    }
}
