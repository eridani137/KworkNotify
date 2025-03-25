#!/usr/bin/env bash

if [ $# -ne 1 ]; then
    echo "Использование: $0 /путь/к/.env"
    exit 1
fi

ENV_FILE=$1

if [ ! -f "$ENV_FILE" ]; then
    echo "Ошибка: Файл $ENV_FILE не найден"
    exit 1
fi

CONNECTION_STRING=$(grep "CONNECTION_STRING" "$ENV_FILE" | sed "s/.*CONNECTION_STRING=['\"]\([^'\"]*\)['\"].*/\1/")

if [ -z "$CONNECTION_STRING" ]; then
    echo "Ошибка: Переменная CONNECTION_STRING не найдена или пуста в $ENV_FILE"
    exit 1
fi

TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_NAME="mongodb_backup_$TIMESTAMP"

DB_NAME="KworkNotify"

echo "Создание бэкапа MongoDB для базы $DB_NAME..."
mongodump --uri="$CONNECTION_STRING" --db="$DB_NAME" --out="$BACKUP_NAME" 2>&1

if [ $? -eq 0 ]; then
    echo "Бэкап успешно создан в директории: $BACKUP_NAME"
    
    echo "Сжатие бэкапа..."
    tar -czf "$BACKUP_NAME.tar.gz" "$BACKUP_NAME"
    
    rm -rf "$BACKUP_NAME"
    
    echo "Бэкап завершен: $BACKUP_NAME.tar.gz"
else
    echo "Ошибка при создании бэкапа"
    exit 1
fi
