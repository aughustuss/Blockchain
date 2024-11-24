using System.Net.Sockets;
using System.Text;

namespace Client
{
    internal class Program
    {

        private readonly static string _clientId = "41hh:1458";

        private static async Task Main(string[] args)
        {
            Console.WriteLine("Conectado ao servidor");

            while (true)
            {
                Console.WriteLine("\nEscolha uma opção:");
                Console.WriteLine("1 - Criar um novo bloco com dados");
                Console.WriteLine("2 - Encontrar um bloco pelo hash");
                Console.WriteLine("3 - Mostrar blockchain");
                Console.WriteLine("4 - Editar bloco existente (simular um bloqueio pela Blockchain)");
                Console.WriteLine("5 - Consertar blockchain");
                Console.WriteLine("6 - Interromper conexão");
                Console.Write("\nOpção: ");

                string? input = Console.ReadLine();

                if (!int.TryParse(input, out int option) || option < 1 || option > 6)
                {
                    Console.WriteLine("\nOpção inválida. Tente novamente.");
                    continue;
                }

                if (String.IsNullOrEmpty(input))
                {
                    Console.WriteLine("\nDigite algo.");
                    continue;
                }

                switch (option)
                {
                    case 1:
                        await CreateBlockAsync();
                        break;
                    case 2:
                        await FindBlockByHashAsync();
                        break;
                    case 3:
                        await ShowBlockchainAsync();
                        break;
                    case 4:
                        await EditBlockAsync();
                        break;
                    case 5:
                        await FixBlockchainAsync();
                        break;
                    case 6:
                        Console.WriteLine("\nEncerrando o cliente...");
                        return;
                }
            }
        }

        private static async Task CreateBlockAsync()
        {
            Console.Write("\nDigite os dados para o novo bloco: ");

            string data = Console.ReadLine() ?? string.Empty;

            await CreateAndSendRequestAsync("CREATE_BLOCK", data);
        }

        private static async Task FindBlockByHashAsync()
        {
            Console.Write("\nDigite o hash do bloco a ser encontrado: ");

            string hash = Console.ReadLine() ?? string.Empty;

            await CreateAndSendRequestAsync("FIND_BLOCK", hash);
        }

        private static async Task ShowBlockchainAsync()
        {
            await CreateAndSendRequestAsync("SHOW_BLOCKCHAIN", "");
        }

        private static async Task EditBlockAsync()
        {
            Console.Write("\nDigite o hash do bloco a ser editado: ");
            string hash = Console.ReadLine() ?? string.Empty;

            Console.Write("Digite os novos dados para o bloco: ");
            string newData = Console.ReadLine() ?? string.Empty;

            string request = $"{hash}|{newData}";

            await CreateAndSendRequestAsync("EDIT_BLOCK", request);
        }

        private static async Task FixBlockchainAsync()
        {
            await CreateAndSendRequestAsync("FIX_BLOCKCHAIN", "");
        }

        private static async Task SendRequestAsync(TcpClient client, string request)
        {
            NetworkStream stream = client.GetStream();

            string requestWithId = $"{_clientId}|{request}";

            byte[] dataBytes = Encoding.UTF8.GetBytes(requestWithId);

            await stream.WriteAsync(dataBytes);

            await stream.FlushAsync();
        }

        private static async Task ReceiveResponseAsync(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            StringBuilder responseBuilder = new();

            while (true)
            {
                try
                {
                    int bytesRead = await stream.ReadAsync(buffer);

                    if (bytesRead == 0) break;

                    responseBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                    if (responseBuilder.ToString().Contains("END_OF_MESSAGE"))
                    {
                        string completeResponse = responseBuilder.ToString()
                            .Replace("END_OF_MESSAGE", "")
                            .Trim();

                        completeResponse = completeResponse.StartsWith('+') ? $"\n{completeResponse}" : completeResponse;

                        if (!string.IsNullOrEmpty(completeResponse))
                            Console.WriteLine($"\nResposta do servidor: {completeResponse}");

                        responseBuilder.Clear();
                    }
                }
                catch
                {
                }

            }
        }

        private static async Task CreateAndSendRequestAsync(string option, string data)
        {
            string request = $"{option}|{data}";

            using TcpClient client = new("127.0.0.1", 9999);

            await SendRequestAsync(client, request);

            await ReceiveResponseAsync(client.GetStream());
        }
    }
}
