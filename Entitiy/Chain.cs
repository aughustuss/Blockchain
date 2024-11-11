using Blockchain.Interface;

namespace Blockchain.Entitiy
{
    public class Chain : IChain
    {

        private readonly List<Block> _chain;
        private readonly int _difficulty;

        public Chain(int difficulty)
        {
            _chain = [];
            _difficulty = difficulty;
            AddGenesisBlock();
        }

        private void AddGenesisBlock()
        {
            _chain.Add(new Block('0', DateTime.Now, "Primeiro bloco da rede", "0"));
        }

        public void AddBlock(string data)
        {
            Block previousBlock = _chain[_chain.Count - 1];
            Block newBlock = new(previousBlock.Index + 1, DateTime.Now, data, previousBlock.Hash);
            newBlock.MineBlock(_difficulty);
            _chain.Add(newBlock);
        }

        public Block GetBlockByHash(string hash)
        {
            return _chain.Find(block => block.Hash.Equals(hash, StringComparison.OrdinalIgnoreCase));
        }

        public void DisplayChain()
        {
            foreach (var block in _chain)
            {
                Console.WriteLine($"Index: {block.Index}");
                Console.WriteLine($"Hash: {block.Hash}");
                Console.WriteLine();
            }
        }

    }
}
