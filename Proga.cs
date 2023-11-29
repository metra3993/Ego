using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Timers;

[Serializable]
public class UserData
{
    public string Name { get; set; }
    public int CharactersPerMinute { get; set; }
    public int CharactersPerSecond { get; set; }
}

public static class Leaderboard
{
    private const string LeaderboardFilePath = "leaderboard.json";
    private static List<UserData> leaderboard;
    private static readonly object leaderboardLock = new object();

    static Leaderboard()
    {
        leaderboard = LoadLeaderboard();
    }

    private static List<UserData> LoadLeaderboard()
    {
        try
        {
            if (File.Exists(LeaderboardFilePath))
            {
                string json = File.ReadAllText(LeaderboardFilePath);
                return JsonSerializer.Deserialize<List<UserData>>(json) ?? new List<UserData>();
            }
            else
            {
                return new List<UserData>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            return new List<UserData>();
        }
    }

    public static void AddToLeaderboard(UserData user)
    {
        try
        {
            lock (leaderboardLock)
            {
                leaderboard.Add(user);
                SaveLeaderboard();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    public static void ShowLeaderboard()
    {
        lock (leaderboardLock)
        {
            Console.WriteLine("\nLeaderboard:");
            Console.WriteLine("Name\t\tCPM\t\tCPS");
            foreach (var user in leaderboard.OrderByDescending(u => u.CharactersPerMinute))
            {
                Console.WriteLine($"{user.Name}\t\t{user.CharactersPerMinute}\t\t{user.CharactersPerSecond}");
            }
        }
    }

    private static void SaveLeaderboard()
    {
        try
        {
            lock (leaderboardLock)
            {
                string json = JsonSerializer.Serialize(leaderboard, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(LeaderboardFilePath, json);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
}

public class TypingTest
{
    private static string textToType = "Какой-то очень скучный и глупый текст, который я придумал за 10 секунд.";
    private static Stopwatch stopwatch = new Stopwatch();
    private static bool isTestActive = false;
    private static System.Timers.Timer timer;

    public static void Main()
    {
        timer = new System.Timers.Timer(1000); // Таймер срабатывает каждую секунду
        timer.Elapsed += OnTimedEvent;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("Привет!");
            Console.Write("Введи имя: ");
            string userName = Console.ReadLine();

            isTestActive = true;
            stopwatch.Restart();

            // Start the timer
            timer.Start();

            Console.Clear();
            Console.WriteLine($"Пиши текст:\n{textToType}\n");

            GetUserInput();

            isTestActive = false;

            // Stop the timer
            timer.Stop();

            int charactersTyped = CountCharactersTyped();

            double charactersPerMinute = charactersTyped / (stopwatch.Elapsed.TotalMinutes);
            double charactersPerSecond = charactersTyped / (stopwatch.Elapsed.TotalSeconds);

            UserData user = new UserData
            {
                Name = userName,
                CharactersPerMinute = (int)charactersPerMinute,
                CharactersPerSecond = (int)charactersPerSecond
            };

            Leaderboard.AddToLeaderboard(user);
            Leaderboard.ShowLeaderboard();

            Console.WriteLine($"\nТвоя скорость: {charactersPerMinute:F2} CPM / {charactersPerSecond:F2} CPS");

            Console.WriteLine("Заново (да/нет)");
            string response = Console.ReadLine();
            if (response?.ToLower() != "да")
            {
                break;
            }
        }
    }

    private static void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        Console.SetCursorPosition(0, Console.CursorTop + 2);
        Console.Write($"Времени прошло: {60 - stopwatch.Elapsed.TotalSeconds:F1}s");
    }

    private static void GetUserInput()
    {
        int currentIndex = 0;

        while (isTestActive && currentIndex < textToType.Length)
        {
            Console.Clear();
            Console.WriteLine($"Твой текст:\n{textToType}\n");

            Console.Write(textToType.Substring(0, currentIndex));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(textToType.Substring(currentIndex, Math.Min(Console.WindowWidth - 1, textToType.Length - currentIndex)));
            Console.ResetColor();
            Console.Write(textToType.Substring(currentIndex + Math.Min(Console.WindowWidth - 1, textToType.Length - currentIndex)));

            ConsoleKeyInfo keyInfo = Console.ReadKey();
            if (keyInfo.KeyChar == textToType[currentIndex])
            {
                currentIndex++;
            }
        }
    }

    private static int CountCharactersTyped()
    {
        int count = 0;
        foreach (char c in textToType)
        {
            if (char.IsLetterOrDigit(c))
            {
                count++;
            }
        }
        return count;
    }
}
