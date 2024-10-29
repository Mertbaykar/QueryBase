
namespace QueryBase.Examples.Core
{
    public abstract class EntityBase : IEntity<int>
    {
        public int Id { get; private set; }
        public bool IsActive { get; private set; } = true;

        public void Activate()
        {
            IsActive = true;
        }

        public void DeActivate()
        {
            IsActive = false;
        }
    }
}
