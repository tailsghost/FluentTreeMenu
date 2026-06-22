using System;
using System.Collections.Generic;
using System.Text;
using FluentTreeMenu.Commands;
using Wpf.Ui.Controls;

namespace FluentTreeMenu.Models;

public class RowHolder
{
    public List<FluentTreeMenuCommandItem> Commands { get; set; }
    public string Title { get; set; }
    public SymbolRegular Symbol { get; set; }
}

