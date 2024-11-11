using Blockchain.Entitiy;

namespace Blockchain.Interface
{
    public interface IChain
    {
        public void AddBlock(string Data);
        public void DisplayChain();
        Block GetBlockByHash(string hash);
    }
}
