using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Utilities {
   public static class Assert {
      public static bool ThrowExceptions {
         get { return _throwExceptions; }
         set { _throwExceptions = value; }
      }
      [ContractAnnotation("halt <= b: true")] public static void False(bool b, string message = null) {
         if (b) {
            if (message != null) Trace.WriteLine("Assertion failed. Message: " + message);
            else Trace.WriteLine("Assertion failed.");
            if (_throwExceptions) {
               StackFrame frame = new StackFrame(1, true);
               throw new AssertException(frame.GetMethod().Name, frame.GetFileName(), frame.GetFileLineNumber(), message);
            }
         }
      }
      [ContractAnnotation("halt <= obj: null")] public static T NotNull<T>(T obj, string message = null) {
         if (ReferenceEquals(obj, null)) {
            if (message != null) Trace.WriteLine("Assertion failed. Message: " + message);
            else Trace.WriteLine("Assertion failed.");
            if (_throwExceptions) {
               StackFrame frame = new StackFrame(1, true);
               throw new AssertException(frame.GetMethod().Name, frame.GetFileName(), frame.GetFileLineNumber(), message);
            }
         }
         return obj;
      }
      [ContractAnnotation("halt <= true")] public static void Now(string message = null) {
         if (message != null) Trace.WriteLine("Assertion failed (Assert.Now). Message: " + message);
         else Trace.WriteLine("Assertion failed (Assert.Now).");
         if (_throwExceptions) {
            StackFrame frame = new StackFrame(1, true);
            throw new AssertException(frame.GetMethod().Name, frame.GetFileName(), frame.GetFileLineNumber(), message);
         }
      }
      [ContractAnnotation("halt <= b: false")] public static void True(bool b, string message = null) {
         if (!b) {
            if (message != null) Trace.WriteLine("Assertion failed. Message: " + message);
            else Trace.WriteLine("Assertion failed.");
            if (_throwExceptions) {
               StackFrame frame = new StackFrame(1, true);
               throw new AssertException(frame.GetMethod().Name, frame.GetFileName(), frame.GetFileLineNumber(), message);
            }
         }
      }
      private static bool _throwExceptions = true;
      private class AssertException : Exception {
         public AssertException(string method, string filename, int linenumber, string message = null)
            : base("This exception should never occur. " + Environment.NewLine + (message ?? " Here is some helpful debugging info:") + Environment.NewLine +
                   "Method: " + method + (string.IsNullOrEmpty(filename) ? "" : Environment.NewLine + "File(line): " + filename + "(" + linenumber + ")")) {
         }
      }
   }
}