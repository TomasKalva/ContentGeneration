using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PrintingState
{
    public int Indent { get; set; }

    StringBuilder sb;

    public PrintingState()
    {
        sb = new StringBuilder();
        Indent = 0;
    }

    public PrintingState Print(string msg)
    {
        sb.Append(msg);
        return this;
    }

    public PrintingState PrintIndent(string msg = "")
    {
        sb.Append($"{new string('\t', Indent)}{msg}");
        return this;
    }

    public PrintingState PrintLine(string msg = "")
    {
        sb.AppendLine(msg);
        return this;
    }

    public PrintingState ChangeIndent(int change)
    {
        Indent += change;
        return this;
    }

    public void Show()
    {
        Debug.Log(sb.ToString());
    }
}

interface Printable
{
    PrintingState Print(PrintingState state);
}
