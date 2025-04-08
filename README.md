# VK to Telegram Bot 🚀

Простой бот, который автоматически берёт посты из группы [ВКонтакте](https://vk.com/goryachievelomany) и публикует их в [Телеграм-канале]([#](https://t.me/HotBikeYar)).

## 🧠 Что делает бот

- Получает новые посты из заданного VK-сообщества
- Проверяет, были ли эти посты уже опубликованы в TG, и надо ли их обновить
- Публикует их в Telegram-канале с редактированием под особенности TG
- Проверяет наличие новых постов с заданной периодичностью
- Можно написать боту с просьбой проверить обновления вне очереди
- Можно запустить как сервис винды

## 🛠️ Используемые технологии

- С# 
- VK API
- Telegram Bot API

## ⚙️ Как запустить

   git clone https://github.com/DevILLz/HotBikeTelegramBot.git
   1. собрать консольный проект и запустить
   2. запуск как сервис:
   dotnet publish -c Release -r win-x64 --self-contained true
   New-Service -Name "VkToTelegramBot" -BinaryPathName (Resolve-Path ".\WorkerService\bin\Release\net9.0\win-x64\publish\WorkerService.exe") -DisplayName "VkToTelegram Service" -StartupType Automatic
   Start-Service -Name "VkToTelegramBot"
   
   для обновления -
   Stop-Service -Name "VkToTelegramBot"
   dotnet publish -c Release -r win-x64 --self-contained true
   Start-Service -Name "VkToTelegramBot"

   *На текущем этапе необходимо выдать права пользователя сервису. Решу эту проблему позже

   Windows-сервисы по умолчанию работают от LocalSystem.
   Если ты пытаешься получить доступ к:
   
   сети (например, обращаться к API),
   
   файловой системе вне разрешённых папок,
   
   пользовательским настройкам,
   
   это может вызывать сбои.
   
   ✅ Что попробовать:
   В services.msc → найти свой сервис → Правый клик → Свойства → Вкладка "Вход в систему"
   
   Поставь: "Вход от имени: Локальная служба" или укажи свою учётку

TODO: 
1) Отладка работоспособности и добавление логгирования на всех этапах работы бота
2) Запуск как сервиса windows (сделано)
3) Выделение универсальной части для использования в других группах
