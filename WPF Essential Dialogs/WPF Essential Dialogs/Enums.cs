using System;
using System.Collections.Generic;
using System.Text;

namespace EssentialDialogs
{
    public static class Enums
    {
        public enum EssentialDialogsOptions
        {
            Ok,
            OkCancel,
            Select,
            SelectCancel,
            YesNo,
            YesNoCancel
        }

        public enum EssentialDialogsResult
        {
            Cancel,
            No,
            Ok,
            Selected,
            Yes
        }
    }
}
