using Blockchain.Interface;
using System.Security.Cryptography;
using System.Text;

namespace Blockchain.Entitiy
{
    public class Block : IBlock
    {
        public int Index { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Data { get; set; } = "";
        public string PreviousHash { get; set; } = "";
        public string Hash { get; set; } = "";
        public int Nonce { get; set; }

        public Block(int index, DateTime timeStamp, string data, string previousHash)
        {
            Index = index;
            TimeStamp = timeStamp;
            Data = data;
            PreviousHash = previousHash;
            Hash = CalculateHash();
            Nonce = new Random().Next(0, int.MaxValue);
        }

        public string CalculateHash()
        {
            string data = $"{Index}{TimeStamp}{Data}{PreviousHash}{Nonce}";
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        public void MineBlock(int difficulty)
        {
            string hashPrefix = new('0', difficulty);
            while (true)
            {
                Hash = CalculateHash();
                if (Hash.Substring(0, difficulty) == hashPrefix)
                    break;
                Nonce++;
            }
            Console.WriteLine($"Bloco mineirado: {Hash} com o nonce: {Nonce}");
        }
    }
}
