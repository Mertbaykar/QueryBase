

namespace QueryBase
{
    public interface IQueryStatus
    {
        /// <summary>
        /// has no setter for allowing use of nonpublic setter
        /// </summary>
        bool IsActive { get; }
        void Activate();
        void DeActivate();
    }

}
