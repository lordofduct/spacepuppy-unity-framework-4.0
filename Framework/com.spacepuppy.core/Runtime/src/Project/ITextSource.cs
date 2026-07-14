using System;
using System.Collections.Generic;

namespace com.spacepuppy.Project
{

    public interface ITextSource : IEnumerable<string>
    {

        string text { get; }
        string this[int index] { get; }
        int Count { get; }

    }

    public interface ITextField
    {
        string text { get; set; }
        UnityEngine.Color color { get; set; }
        
    }

}
