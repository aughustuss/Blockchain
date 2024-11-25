using Blockchain.Interface;
using System;
using System.Net.Sockets;

namespace Blockchain.Entitiy
{
    public class Chain : IChain
    {

        private readonly List<Block> _chain;
        private readonly int _difficulty;
        private Timer? _timer;

        public Chain(int difficulty)
        {
            _chain = [];
            _difficulty = difficulty;
            StartPeriodicValidation();
            AddGenesisBlock();  
        }

        private void AddGenesisBlock()
        {
            _chain.Add(new Block(0, DateTime.Now, "Primeiro bloco da rede", "0"));
        }

        public async Task<string> AddBlock(string data)
        {

            int corruptedBlockIndex = SearchForCorruptedBlock();

            if (corruptedBlockIndex == 0)
            {
                Block previousBlock = _chain[^1];
                Block newBlock = new(previousBlock.Index + 1, DateTime.Now, data, previousBlock.Hash);

                await newBlock.MineBlockAsync(_difficulty);

                _chain.Add(newBlock);
                return newBlock.Hash;
            }

            return string.Empty;
        }

        public void StartPeriodicValidation()
        {
            _timer = new Timer(async =>
            {
                SearchForCorruptedBlock();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
        }

        public Block GetBlockByHash(string hash) => _chain?.Find(block => block.Hash.Equals(hash, StringComparison.OrdinalIgnoreCase)) ?? null;

        public List<Block> GetBlocks() => _chain.FindAll(b => !String.IsNullOrEmpty(b.Data)).ToList();

        public int SearchForCorruptedBlock()
        {
            for (int i = 1; i < _chain.Count; i++)
            {
                Block currentBlock = _chain[i];
                Block previousBlock = _chain[i - 1];

                if (currentBlock.PreviousHash != previousBlock.Hash)
                {
                    Console.WriteLine($"\nErro: Bloco #{i} não está encadeado corretamente com o bloco anterior");
                    return i - 1;
                }
            }
            return 0;
        }

        public void FixChain(int corruptedBlockIndex)
        {
            if (corruptedBlockIndex <= 0 || corruptedBlockIndex >= _chain.Count)
            {
                Console.WriteLine("Índice inválido para remoção.");
                return;
            }

            _chain.RemoveAt(corruptedBlockIndex);

            RecalculateHashes();
        }

        private async void RecalculateHashes()
        {
            for(int i = 1; i < _chain.Count; i++)
            {
                Block currentBlock = _chain[i];
                Block previousBlock = _chain[i - 1];

                currentBlock.PreviousHash = previousBlock.Hash;
                currentBlock.Nonce = 0;

                await currentBlock.MineBlockAsync(_difficulty);
            }
        }
    }
}
