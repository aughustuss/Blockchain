using Blockchain.Entitiy;
using System.Net.Sockets;

namespace Blockchain.Interface
{
    public interface IChain
    {
        public int SearchForCorruptedBlock();
        public void FixChain(int corruptedBlockIndex);

        Block GetBlockByHash(string hash);
        List<Block> GetBlocks();
        Task<string> AddBlock(string Data);
    }
}
