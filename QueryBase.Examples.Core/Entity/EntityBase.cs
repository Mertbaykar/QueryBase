using System.ComponentModel.DataAnnotations.Schema;


namespace QueryBase.Examples.Core
{
    public abstract class EntityBase : IQueryEntity<int>
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
