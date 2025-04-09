# VK to Telegram Bot 🚀

Простой бот, который автоматически берёт посты из группы [ВКонтакте](https://vk.com/goryachievelomany) и публикует их в [Телеграм-канале]([#](https://t.me/HotBikeYar)).
![image](https://github.com/user-attachments/assets/63c45e9f-8860-49cf-bab9-23e64d046004)

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
- Serilog
- LiteDB

## ⚙️ Как запустить
   Для работы бота нужны:
   - сервискный ключ api VK
   - токен бота в телеграмм (обращайтесь к BotFater)

для начала стоит запустить бота через консоль, он создаст конфигурационный файл в appdata - roaming - VkToTelegram
там надо заполнить:
- TelegramToken, 
- VkServiceKey, 
- VkRequestAttributes > Domain > Value (id вашей группы вк)
- TelegramChatId (Id канала в телеге. Можно узнать разными путями, я просто приглашаю бота и смотрю ID в updates пока - метод HandleUpdateAsync)
- StartCheckDate - дата начала проверки постов. Раньше этой даты посты рассматриваться не будут

.. при необходимости, есть еще функционал замена ссылок на что-то другое. Я использую что бы заменить ссылку на пост в вк на ссылку на такой же пост в телеге (типа правил)
UrlPartDificultiesVK - часть ссылки поста в вк (часть, потому что можно встретить разные варианты ссылки на пост на стене)
UrlDificultiesTG - то, на что будет заменена вся ссылка (не только найденная часть)

... прочие параметры говорят сами за себя, и\или не особо важны

   .. запуск ..
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
1) ~~Отладка работоспособности и добавление логгирования на всех этапах работы бота~~
2) ~~Запуск как сервиса windows~~
3) ~~Выделение универсальной части для использования в других группах~~
4) Отказоустойчивость и автоперезапуск "watchdog"
5) Убрать неоходимость давать сервису права пользователя, вероятно, преместив все документы в папку с приложением... подумаю еще над этим
6) Вариативность таймера проверки. Выключать\включать таймер по расписанию, добавить часы пик для опроса раз в Х минут, а в остальное время - 1 раз в час
7) Корректное обновление постов при добавлении\удалении фотографии (т.к. это считается в телеге постами разного типа)
8) Обработка длинных постов - больше 4к символов (ПРИОРИТЕТ)
9) Подумать над трансляцией историй. Пока не ясно, как лучше реализовать, т.к. просто брать истории из вк и делать истории в телеге нельзя, из за ограничений по бустам канала (жадины)
