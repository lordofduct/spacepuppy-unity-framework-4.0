using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Project;

namespace com.spacepuppy.DataBinding
{

    public interface IDataProvider : System.Collections.IEnumerable
    {

        /// <summary>
        /// A binders might only care about binding the first element of the list, this is a fast accessor for that element.
        /// </summary>
        object FirstElement { get; }

    }

    [System.Serializable]
    public class DataProviderRef : SerializableInterfaceRef<IDataProvider> { }

}
