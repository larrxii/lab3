using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
// Сервер
class EchoServer
{
    private static int solvedCount = 0;
    private static int clientId = 1;
    public static void Main()
    {
        StartServer();
    }
    public static void StartServer()
    {
        // Устанавливаем IP-адрес и порт
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        int port = 8000;
        // Создаем TCP listener
        TcpListener server = new TcpListener(ipAddress, port);
        try
        {
            // Запускаем сервер
            server.Start();
            Console.WriteLine($"Сервер запущен на {ipAddress}, {port}");
            while (true)
            {
                Console.WriteLine("Ожидание подключения...");
                // Принимаем клиента
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Клиент подключен!");

                Task.Run(() => HandleClient(client));
                // Получаем поток для чтения и записи             
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка сервера: {0}", ex.Message);
        }
        finally
        {
            server.Stop();
        }
    }

    private static void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        int currentClientId = clientId++;
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead;
            // Читаем данные от клиента
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Клиент {currentClientId}: {receivedMessage}");

                string[] coefficients = receivedMessage.Split(' ');
                if (coefficients.Length != 3 || !double.TryParse(coefficients[0], out double a) || !double.TryParse(coefficients[1], out double b) || !double.TryParse(coefficients[2], out double c))
                {
                    string errorResponse = "Ошибка!";
                    byte[] response = Encoding.UTF8.GetBytes(errorResponse);
                    stream.Write(response, 0, response.Length);
                    Console.WriteLine($"Отправлено обратно: {errorResponse}");
                    continue;
                }
                string result = QuadraticEquation(a, b, c);
                byte[] responseBytes = Encoding.UTF8.GetBytes(result);
                stream.Write(responseBytes, 0, responseBytes.Length);
                Console.WriteLine($"Клиенту {currentClientId} отправлено обратно: {result}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при обработке клиента: {0}", ex.Message);
        }
        finally
        {
            // Закрываем соединение с клиентом
            stream.Close();
            client.Close();
            Console.WriteLine("Клиент отключен");
        }
    }

    private static string QuadraticEquation(double a, double b, double c)
    {
        if (a == 0)
            return "a не должно быть равно 0!";
        double D = b * b - 4 * a * c;
        solvedCount++;
        if (D > 0)
        {
            double x1 = (-b + Math.Sqrt(D)) / (2 * a);
            double x2 = (-b - Math.Sqrt(D)) / (2 * a);
            return $"x1 = {x1}, x2 = {x2}\nВсего решенных уравнений: {solvedCount}";
        }
        else if (D == 0)
        {
            double x = -b / (2 * a);
            return $"x = {x}\nВсего решенных уравнений: {solvedCount}";
            ;
        }
        else
            return $"Нет корней\nВсего решенных уравнений: {solvedCount}";
    }
}
