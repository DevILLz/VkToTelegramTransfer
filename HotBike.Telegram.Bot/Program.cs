using HotBike.Telegram.Bot;

Task.Run(new TelegramBot().StartBot);

Console.WriteLine("Бот запущен успешно");
Console.ReadLine();