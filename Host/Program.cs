using System;
using System.Net;
using System.Threading.Tasks;
using ChatApplication.Host;

namespace ChatApplication.Host
{
    class Program
    {
        private static ChatServer _chatServer;
        private static bool _isRunning = true;

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Chat Server ===");
            
            IPAddress ipAddress = IPAddress.Any;
            int port = 8080;

            if (args.Length >= 2)
            {
                if (!IPAddress.TryParse(args[0], out ipAddress))
                {
                    Console.WriteLine($"Invalid IP address: {args[0]}. Using default: {IPAddress.Any}");
                    ipAddress = IPAddress.Any;
                }

                if (!int.TryParse(args[1], out port) || port <= 0 || port > 65535)
                {
                    Console.WriteLine($"Invalid port: {args[1]}. Using default: 8080");
                    port = 8080;
                }
            }

            _chatServer = new ChatServer(ipAddress, port);
            
            _chatServer.ServerMessage += OnServerMessage;
            _chatServer.MessageReceived += OnMessageReceived;
            _chatServer.ClientConnected += OnClientConnected;
            _chatServer.ClientDisconnected += OnClientDisconnected;
            _chatServer.ServerError += OnServerError;

            Console.CancelKeyPress += OnCancelKeyPress;

            try
            {
                await _chatServer.StartAsync();
                
                Console.WriteLine("\nServer commands:");
                Console.WriteLine("  'broadcast <message>' - Send message to all clients");
                Console.WriteLine("  'send <clientId> <message>' - Send message to specific client");
                Console.WriteLine("  'list' - List connected clients");
                Console.WriteLine("  'quit' - Stop server and quit");
                Console.WriteLine("  'help' - Show this help\n");

                await HandleUserInputAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
            finally
            {
                _chatServer?.Dispose();
            }
        }

        private static async Task HandleUserInputAsync()
        {
            while (_isRunning)
            {
                Console.Write("> ");
                string input = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string command = parts[0].ToLower();

                switch (command)
                {
                    case "broadcast":
                        if (parts.Length > 1)
                        {
                            string message = string.Join(" ", parts, 1, parts.Length - 1);
                            bool sent = await _chatServer.BroadcastMessageAsync(message);
                            Console.WriteLine(sent ? "Message broadcasted" : "Failed to broadcast message");
                        }
                        else
                        {
                            Console.WriteLine("Usage: broadcast <message>");
                        }
                        break;

                    case "send":
                        if (parts.Length > 2)
                        {
                            string clientId = parts[1];
                            string message = string.Join(" ", parts, 2, parts.Length - 2);
                            bool sent = await _chatServer.SendMessageToClientAsync(clientId, message);
                            Console.WriteLine(sent ? "Message sent" : "Failed to send message");
                        }
                        else
                        {
                            Console.WriteLine("Usage: send <clientId> <message>");
                        }
                        break;

                    case "list":
                        Console.WriteLine($"Connected clients: {_chatServer.ClientCount}");
                        break;

                    case "quit":
                        _isRunning = false;
                        break;

                    case "help":
                        Console.WriteLine("Available commands:");
                        Console.WriteLine("  broadcast <message> - Send message to all clients");
                        Console.WriteLine("  send <clientId> <message> - Send message to specific client");
                        Console.WriteLine("  list - List connected clients");
                        Console.WriteLine("  quit - Stop server and quit");
                        Console.WriteLine("  help - Show this help");
                        break;

                    default:
                        Console.WriteLine($"Unknown command: {command}. Type 'help' for available commands.");
                        break;
                }
            }

            await _chatServer.StopAsync();
        }

        private static void OnServerMessage(object sender, string message)
        {
            Console.WriteLine($"[SERVER] {message}");
        }

        private static void OnMessageReceived(object sender, ClientMessageEventArgs e)
        {
            Console.WriteLine($"[{e.ClientId}] {e.Message}");
        }

        private static void OnClientConnected(object sender, ClientEventArgs e)
        {
            Console.WriteLine($"[CONNECT] Client {e.ClientId} connected from {e.EndPoint}");
        }

        private static void OnClientDisconnected(object sender, ClientEventArgs e)
        {
            Console.WriteLine($"[DISCONNECT] Client {e.ClientId} disconnected");
        }

        private static void OnServerError(object sender, Exception error)
        {
            Console.WriteLine($"[ERROR] {error.Message}");
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _isRunning = false;
            Console.WriteLine("\nShutting down server...");
        }
    }
}