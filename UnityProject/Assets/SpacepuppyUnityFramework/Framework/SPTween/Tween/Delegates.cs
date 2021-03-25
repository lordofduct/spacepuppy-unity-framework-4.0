using com.spacepuppy.Dynamic.Accessors;

namespace com.spacepuppy.Tween
{

    /// <summary>
    /// The shape a CustomMemberCurveAttributed static function should be to be recognized by the tween factory.
    /// </summary>
    /// <param name="accessor"></param>
    /// <param name="option"></param>
    /// <returns></returns>
    public delegate TweenCurve CreateCurveFactoryCallback(IMemberAccessor accessor, int option);
    public delegate void TweenConfigCallback(TweenHash hash, Ease ease, float dur);

}
