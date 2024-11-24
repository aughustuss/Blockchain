﻿using Blockchain.Entitiy;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Globalization;
using System.IO;

namespace Server
{
    internal class Program
    {
        private static Chain? _blockChain;
        private readonly static int _difficulty = 5;
        private readonly static List<string> _authorizedClients = ["41hh:1458"];

        private static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Simulando o funcionamento de uma blockchain que armazena dados.");

                _blockChain = new(_difficulty);

                TcpListener server = new(IPAddress.Any, 9999);

                server.Start();

                Console.WriteLine("\nServidor está online e aguardando a inserção de novos blocos");

                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();

                    Console.WriteLine("\n\nCliente enviou um dado para ser adicionado à um bloco");

                    Task.Run(() => HandleClientAsync(client));
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocorreu um erro:", ex.Message);
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            if (_blockChain == null)
                throw new Exception("Blockchain não foi iniciada corretamente");


            NetworkStream stream = client.GetStream();

            try
            {
                int corruptedBlockIndex = _blockChain.VerifyIfChainIsValid();

                byte[] buffer = new byte[1024];

                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine($"\nCliente enviou: {request}");

                string[] parts = request.Split("|", 3);

                string clientId = parts[0], command = parts[1], response = string.Empty, data = parts[2], hash = parts[2];

                if (!_authorizedClients.Contains(clientId))
                    throw new Exception("Cliente não possui acesso à rede");

                if (corruptedBlockIndex != 0 && command != "FIX_BLOCKCHAIN")
                    throw new Exception("Blockchain foi corrompida");

                if (parts.Length < 2)
                    throw new Exception("Formato de solicitação inválido");

                switch (command)
                {
                    case "CREATE_BLOCK":
                        await ConfirmOptionAndPerformRequestAsync("O bloco será adicionado...", () => CreateBlockAsync(data, _blockChain, stream), stream);
                        break;
                    case "FIND_BLOCK":
                        await ConfirmOptionAndPerformRequestAsync("O bloco será retornado...", () => FindBlockAsync(hash, _blockChain, stream), stream);
                        break;
                    case "EDIT_BLOCK":
                        string newData = parts[2].Split("|")[1];
                        await ConfirmOptionAndPerformRequestAsync("Alteração de dados solicitada...", () => EditBlockAsync(hash.Split("|")[0], _blockChain, newData, stream), stream);
                        break;
                    case "SHOW_BLOCKCHAIN":
                        await ConfirmOptionAndPerformRequestAsync("A Blockchain será exibida...", () => ShowBlockchainAsync(_blockChain, stream), stream);
                        break;
                    case "FIX_BLOCKCHAIN":
                        await ConfirmOptionAndPerformRequestAsync("O bloco será reparado...", () => FixBlockchainAsync(_blockChain, corruptedBlockIndex, stream), stream);
                        break;
                    default:
                        await SendResponseAsync(stream, "Comando inválido");
                        break;
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Erro ao processar a solicitação do cliente: {ex.Message}");
            }
            catch (Exception ex)
            {
                await SendResponseAsync(stream, ex.Message.ToString());
            }
            finally
            {
                client.Close();
            }
        }

        private static async Task ConfirmOptionAndPerformRequestAsync(string message, Func<Task> method, NetworkStream stream)
        {
            await SendResponseAsync(stream, message);

            await Task.Run(async () => await method());
        }

        private static async Task CreateBlockAsync(string data, Chain blockchain, NetworkStream stream)
        {
            blockchain.VerifyIfChainIsValid();

            string blockHash = await blockchain.AddBlock(data);

            await SendResponseAsync(stream, $"Bloco adicionado. Hash: {blockHash}");
        }

        private static async Task FindBlockAsync(string hash, Chain blockchain, NetworkStream stream)
        {
            var block = await GetBlockByHashAsync(hash, blockchain, stream);

            if (block == null)
            {
                await SendResponseAsync(stream, "Bloco com o hash fornecido não existe");
                return;
            }

            string response = $@"
+-----------------------------------------------+
| Índice: {block.Index}                                    
| Dado armazenado: {block.Data}                                       
| Hash: {block.Hash.Substring(0, 10)}...                   
| Nonce: {block.Nonce}                                    
+-----------------------------------------------+"; ;

            await SendResponseAsync(stream, response);
        }

        private static async Task EditBlockAsync(string hash, Chain blockchain, string newData, NetworkStream stream)
        {
            var block = await GetBlockByHashAsync(hash, blockchain, stream);

            if (block == null) return;

            block.Data = newData;
            block.Hash = block.CalculateHash();

            await SendResponseAsync(stream, $"Bloco com o ID {block.Index} foi editado");
        }

        private static async Task ShowBlockchainAsync(Chain blockchain, NetworkStream stream)
        {
            var chain = await Task.Run(() => blockchain.GetBlocks());

            List<string> blocks = [];

            foreach (var block in chain)
            {
                string bloco = $@"
+-------+
|   {block.Index}   |
+-------+";
                blocks.Add(bloco);
            }

            string response = string.Join(" ---> ", blocks);

            await SendResponseAsync(stream, response);
        }

        private static async Task FixBlockchainAsync(Chain blockchain, int corruptedBlockIndex, NetworkStream stream)
        {
            await Task.Run(() => blockchain.FixChain(corruptedBlockIndex));

            await SendResponseAsync(stream, "Blockchain foi corrigida");
        }

        private static async Task<Block?> GetBlockByHashAsync(string hash, Chain blockchain, NetworkStream stream)
        {
            var block = blockchain.GetBlockByHash(hash);

            if (block == null)
                await SendResponseAsync(stream, "Hash do bloco não foi fornecido ou não foi encontrado");

            return block;
        }

        private static async Task SendResponseAsync(NetworkStream stream, string response, TcpClient? client = null)
        {
            response += "\nEND_OF_MESSAGE";

            byte[] responseData = Encoding.UTF8.GetBytes(response);
            await stream.WriteAsync(responseData);
            await stream.FlushAsync();
        }
    }
}