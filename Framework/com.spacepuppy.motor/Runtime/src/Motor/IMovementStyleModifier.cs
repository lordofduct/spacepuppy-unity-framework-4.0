using com.spacepuppy.Utils;

namespace com.spacepuppy.Motor
{

    [AutoInitMixin]
    public interface IMovementStyleModifier : IMixin, IEventfulComponent
    {
        sealed void OnInitMixin()
        {
            this.OnEnabled += (s, e) =>
            {
                this.gameObject.FindRoot().Broadcast<IMotor>(o => o.SetCollisionMessageDirty(true));
            };
            this.OnDisabled += (s, e) =>
            {
                this.gameObject.FindRoot().Broadcast<IMotor>(o => o.SetCollisionMessageDirty(true));
            };
        }

        void OnBeforeUpdateMovement(MovementStyleController controller);
        void OnUpdateMovementComplete(MovementStyleController controller);

    }

}
