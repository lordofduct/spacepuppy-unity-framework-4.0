using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    /// <summary>
    /// Interface/contract for a random number generator.
    /// </summary>
    public interface IRandom
    {

        /// <summary>
        /// Float from 0->1, 1 exclusive.
        /// </summary>
        /// <returns></returns>
        float Next();
        /// <summary>
        /// Double from 0->1, 1 exclusive. 
        /// Note that values of 0.99999... approaching 1, if cast to 'float' will become 1. Do not cast double to float and expect 1 to remain excluded.
        /// </summary>
        /// <returns></returns>
        double NextDouble();
        int Next(int size);
        int Next(int low, int high);

    }

    [System.Serializable]
    public class RandomRef : com.spacepuppy.Project.InterfaceRef<IRandom>
    {

        public IRandom ValueOrDefault => this.Value ?? RandomUtil.Standard;

    }

}
