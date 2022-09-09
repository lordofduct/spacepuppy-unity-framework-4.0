namespace com.spacepuppy.Motor
{

    public interface IMovementStyleModifier : MSignalEnabled.IAutoDecorator
    {

        void OnBeforeUpdateMovement(MovementStyleController controller);
        void OnUpdateMovementComplete(MovementStyleController controller);

    }

}
