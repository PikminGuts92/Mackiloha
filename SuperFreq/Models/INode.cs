using System;
using System.Collections.Generic;
using System.Text;

namespace SuperFreq.Models
{
    public interface INode
    {
        string Name { get; set; }
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
        List<INode> Children { get; }
    }
}
