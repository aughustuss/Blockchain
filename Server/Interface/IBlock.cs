using Blockchain.Entitiy;

namespace Blockchain.Interface
{
    public interface IBlock
    {
        public string CalculateHash();
        Task MineBlockAsync(int difficulty);
    }
}
