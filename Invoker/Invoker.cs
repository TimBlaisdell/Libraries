using System.ComponentModel;
using System.Windows.Forms;

namespace InvokerLib {
    /// <summary>
    /// This is a convenient and useful class for calling methods on Controls that may need to be invoked for thread safety.
    /// </summary>
    public static class Invoker {
        /// <summary>
        /// Invokes <c>method</c> with <c>control.BeginInvoke</c>, if <c>control.InvokeRequired</c> is set, otherwise just calls <c>method</c> directly.
        /// </summary>
        /// <param name="control">The control on which the code is to be invoked.</param>
        /// <param name="method">The method to invoke.</param>
        public static void AsyncInvoke ( ISynchronizeInvoke control, MethodInvoker method ) {
            if (control != null && control.InvokeRequired) control.BeginInvoke(method, null);
            else method();
        }
        /// <summary>
        /// Invokes <c>method</c> with <c>control.BeginInvoke</c>, if <c>control.InvokeRequired</c> is set, otherwise just calls <c>method</c> directly.
        /// </summary>
        /// <param name="control">The control on which the code is to be invoked.</param>
        /// <param name="method">The method to invoke.</param>
        public static void AsyncInvoke ( Control control, MethodInvoker method ) {
            if (control != null) {
                if (control.IsDisposed || control.Disposing) return;
                if (control.InvokeRequired) control.BeginInvoke(method, null);
                else method();
            }
            else method();
        }
        /// <summary>
        /// Invokes <c>method</c> with <c>control.Invoke</c>, if <c>control.InvokeRequired</c> is set, otherwise just calls <c>method</c> directly.
        /// </summary>
        /// <param name="control">The control on which the code is to be invoked.</param>
        /// <param name="method">The method to invoke.</param>
        public static void SyncInvoke ( ISynchronizeInvoke control, MethodInvoker method ) {
            if (control != null && control.InvokeRequired) control.Invoke(method, null);
            else method();
        }
        /// <summary>
        /// Invokes <c>method</c> with <c>control.Invoke</c>, if <c>control.InvokeRequired</c> is set, otherwise just calls <c>method</c> directly.
        /// </summary>
        /// <param name="control">The control on which the code is to be invoked.</param>
        /// <param name="method">The method to invoke.</param>
        public static void SyncInvoke ( Control control, MethodInvoker method ) {
            if (control != null) {
                if (control.IsDisposed || control.Disposing) return;
                if (control.InvokeRequired) control.Invoke(method, null);
                else method();
            }
            else method();
        }
    }
}