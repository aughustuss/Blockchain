using Blockchain.Entitiy;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Blockchain
{
    internal class Program
    {

        private static Chain? _blockChain;

        private static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Simulando o funcionamento de uma blockchain que armazena dados.");

                int difficulty = 2;
                _blockChain = new(difficulty);

                TcpListener server = new TcpListener(IPAddress.IPv6Any, 9999);

                server.Start();

                Console.WriteLine("Servidor está online e aguardando a inserção de novos blocos");

                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();

                    Console.WriteLine("Cliente foi conectado");

                    HandleClient(client);
                }


            } catch (Exception ex)
            {
                Console.WriteLine("Ocorreu o erro:", ex.Message);
            }
        }

        private static void HandleClient(TcpClient client)
        {

            if (_blockChain == null)
                throw new Exception("Blockchain não foi iniciada corretamente");

            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Console.WriteLine($"Recebido o dado: {data}, do cliente");

            _blockChain.AddBlock(data);

            string response = "Bloco adicionado com sucesso";

            byte[] responseData = Encoding.UTF8.GetBytes(response);

            stream.Write(responseData, 0, responseData.Length);

            Console.WriteLine("\n Estado atual da Blockchain:");

            _blockChain.DisplayChain();

            client.Close();

        }
    }
}
