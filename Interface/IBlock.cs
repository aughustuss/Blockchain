namespace Blockchain.Interface
{
    public interface IBlock
    {
        public string CalculateHash();
        public void MineBlock(int difficulty);
    }
}
