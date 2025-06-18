#!/usr/bin/env bash

MONGO_BACKUP_SCRIPT="mongo_backup.sh"
LOGS_BACKUP_SCRIPT="logs_backup.sh"
ENV_FILE="/root/build/.env"

if [ ! -f "$MONGO_BACKUP_SCRIPT" ]; then
    echo "Ошибка: Скрипт $MONGO_BACKUP_SCRIPT не найден"
    exit 1
fi

if [ ! -f "$LOGS_BACKUP_SCRIPT" ]; then
    echo "Ошибка: Скрипт $LOGS_BACKUP_SCRIPT не найден"
    exit 1
fi

if [ ! -f "$ENV_FILE" ]; then
    echo "Ошибка: Файл $ENV_FILE не найден"
    exit 1
fi

echo "Запуск бэкапа MongoDB..."
bash "$MONGO_BACKUP_SCRIPT" "$ENV_FILE" 2>/dev/null
if [ $? -eq 0 ]; then
    echo "Бэкап MongoDB успешно завершен"
else
    echo "Ошибка при выполнении бэкапа MongoDB"
    exit 1
fi

echo "Запуск бэкапа логов..."
bash "$LOGS_BACKUP_SCRIPT" 2>/dev/null
if [ $? -eq 0 ]; then
    echo "Бэкап логов успешно завершен"
else
    echo "Ошибка при выполнении бэкапа логов"
    exit 1
fi

echo "Все бэкапы успешно завершены"
